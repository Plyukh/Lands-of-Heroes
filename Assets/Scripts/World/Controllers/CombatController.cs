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

        // Дальний бой
        if (attacker.AttackType == AttackType.Ranged)
        {
            highlightController.ClearHighlightsImmediate();
            await PlayAttackSequence(attacker, target);
            return;
        }

        // Ближний бой: ищем соседние
        var neighborCells = pathfindingManager
            .GetReachableCells(targetCell, 1, MovementType.Teleport)
            .Where(c => c.IsWalkable)
            .ToList();

        // Если уже рядом
        if (neighborCells.Contains(startCell))
        {
            highlightController.ClearHighlightsImmediate();
            await PlayAttackSequence(attacker, target);
            return;
        }

        // Вычисляем зону подхода
        var reachable = pathfindingManager
            .GetReachableCells(startCell, speed, moveType);

        var candidates = neighborCells.Intersect(reachable).ToList();
        if (candidates.Count == 0)
        {
            Debug.Log($"[CombatController] Нельзя подойти к {target.name}");
            return;
        }

        var attackPos = candidates
            .OrderBy(c => Vector3.Distance(startCell.transform.position, c.transform.position))
            .First();

        // Подсвечиваем зону подхода
        highlightController.ClearHighlights();
        highlightController.HighlightReachable(reachable, startCell);
        attackPos.ShowHighlight(true);

        // Убираем сразу
        highlightController.ClearHighlightsImmediate();

        // Двигаемся
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

        // Обновляем occupants
        startCell.RemoveOccupant(attacker.gameObject);
        attackPos.AddOccupant(attacker.gameObject, CellObjectType.Creature);

        // Перерисовываем новую зону (опционально)
        var newReachable = pathfindingManager
            .GetReachableCells(attackPos, speed, moveType);
        highlightController.ClearHighlightsImmediate();
        highlightController.HighlightReachable(newReachable, attackPos);

        // Атакуем
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