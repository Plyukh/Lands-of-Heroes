using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[DisallowMultipleComponent]
public class CombatController : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Менеджер поиска пути и зоны досягаемости")]
    [SerializeField] private PathfindingManager pathfindingManager;
    [Tooltip("Контроллер подсветки клеток")]
    [SerializeField] private HighlightController highlightController;

    public event Action<Creature> OnCombatComplete;

    public async void OnCreatureClicked(Creature attacker, Creature target)
    {
        if (attacker == null || target == null || attacker == target)
            return;
        if (!TurnOrderController.Instance.IsCurrentTurn(attacker))
            return;

        var startCell = attacker.Mover.CurrentCell;
        var targetCell = target.Mover.CurrentCell;
        int speed = attacker.GetStat(CreatureStatusType.Speed);
        var moveType = attacker.MovementType;

        // 1) Дальний бой — сразу запускаем анимацию скрытия через Disable
        if (attacker.AttackType == AttackType.Ranged)
        {
            highlightController.ClearHighlights();   // ← анимированно гасим
            await PlayAttackSequence(attacker, target);
            return;
        }

        // 2) Ближний бой: ищем свободные соседи цели
        var neighborCells = pathfindingManager
            .GetReachableCells(targetCell, 1, MovementType.Teleport)
            .Where(c => c.IsWalkable)
            .ToList();

        // если уже рядом — всё то же самое
        if (neighborCells.Contains(startCell))
        {
            highlightController.ClearHighlights();   // ← анимированно гасим
            await PlayAttackSequence(attacker, target);
            return;
        }

        // 3) Куда подойти
        var reachable = pathfindingManager
            .GetReachableCells(startCell, speed, moveType);

        var candidates = neighborCells.Intersect(reachable).ToList();
        if (candidates.Count == 0)
            return;

        var attackPos = candidates
            .OrderBy(c => Vector3.Distance(startCell.transform.position, c.transform.position))
            .First();

        // 4) Прежде чем двигаться — гасим все outline через анимацию Disable
        highlightController.ClearHighlights();       // ← именно это

        // 5) Двигаемся
        bool moved = false;
        if (moveType == MovementType.Teleport)
            moved = await attacker.Mover.TeleportToCell(attackPos);
        else
        {
            var path = pathfindingManager.FindPath(startCell, attackPos, moveType);
            if (path != null && path.Count > 0)
                moved = await attacker.Mover.MoveAlongPath(path);
        }

        if (!moved)
            return;

        // 6) Обновляем occupants
        startCell.RemoveOccupant(attacker.gameObject);
        attackPos.AddOccupant(attacker.gameObject, CellObjectType.Creature);

        // ни одной новой подсветки — сразу в атаку
        await PlayAttackSequence(attacker, target);
    }


    private async Task PlayAttackSequence(Creature attacker, Creature target)
    {
        var mover = attacker.Mover;
        var anim = mover.AnimatorController;
        var hitTcs = new TaskCompletionSource<bool>();
        bool isRanged = attacker.AttackType == AttackType.Ranged;

        await mover.RotateTowardsAsync(target.transform.position);
        anim.SetAttackTarget(target, attacker);

        Action onHit = null;
        if (isRanged)
        {
            onHit = () =>
            {
                anim.OnAttackHit -= onHit;
                hitTcs.TrySetResult(true);
            };
            anim.OnAttackHit += onHit;
            anim.PlayAttack();
        }
        else
        {
            onHit = () =>
            {
                anim.OnMeleeAttackHit -= onHit;
                hitTcs.TrySetResult(true);
            };
            anim.OnMeleeAttackHit += onHit;
            anim.PlayMeleeAttack();
        }

        await hitTcs.Task;
        OnCombatComplete?.Invoke(attacker);
    }
}