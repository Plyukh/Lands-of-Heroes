using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[DisallowMultipleComponent]
public class CombatController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PathfindingManager pathfindingManager;
    [SerializeField] private HighlightController highlightController;

    public event Action<Creature> OnCombatComplete;

    public async void OnCreatureClicked(Creature attacker, Creature target)
    {
        if (attacker == null || target == null || attacker == target)
            return;
        if (!TurnOrderController.Instance.IsCurrentTurn(attacker))
            return;

        var mover = attacker.Mover;
        var startCell = mover.CurrentCell;
        var targetCell = target.Mover.CurrentCell;
        int speed = attacker.GetStat(CreatureStatusType.Speed);
        var moveType = attacker.MovementType;

        // 1) Дальний бой — сразу атакуем
        if (attacker.AttackType == AttackType.Ranged)
        {
            highlightController.ClearHighlights();
            await PlayAttackSequence(attacker, target);
            return;
        }

        // 2) Ближний бой — ищем свободные соседние клетки у цели
        var neighborCells = pathfindingManager
            .GetReachableCells(targetCell, 1, MovementType.Teleport)
            .Where(c => c.IsWalkable)
            .ToList();

        // 2a) Если уже в соседней клетке — сразу атакуем
        if (neighborCells.Contains(startCell))
        {
            highlightController.ClearHighlights();
            await PlayAttackSequence(attacker, target);
            return;
        }

        // 3) Находим пересечение reachable и свободных соседних клеток
        var reachable = pathfindingManager.GetReachableCells(startCell, speed, moveType);
        var candidates = neighborCells.Intersect(reachable).ToList();
        if (candidates.Count == 0)
            return;

        // 4) Выбираем ближайшую к стартовой клетку
        var attackPos = candidates
            .OrderBy(c => Vector3.Distance(startCell.transform.position, c.transform.position))
            .First();

        // 5) Запрещаем телепорт/полёт на занятую клетку
        if ((moveType == MovementType.Teleport || moveType == MovementType.Flying)
            && !attackPos.IsWalkable)
        {
            return;
        }

        bool moved = false;

        // 6) Логика перемещения
        if (moveType == MovementType.Teleport)
        {
            highlightController.HighlightTeleportTarget(attackPos);
            moved = await mover.TeleportToCell(attackPos);
            if (moved)
                attackPos.ShowHighlight(false);
        }
        else
        {
            var path = pathfindingManager.FindPath(startCell, attackPos, moveType);
            if (path == null || path.Count == 0)
                return;

            highlightController.HighlightPath(path);
            void OnStep(HexCell cell) => cell.ShowHighlight(false);
            mover.OnCellEntered += OnStep;

            moved = await mover.MoveAlongPath(path);
            mover.OnCellEntered -= OnStep;
        }

        if (!moved)
            return;

        // 7) Переносим существо и очищаем подсветку перед атакой
        startCell.RemoveOccupant(attacker.gameObject);
        attackPos.AddOccupant(attacker.gameObject, CellObjectType.Creature);

        highlightController.ClearHighlights();
        await PlayAttackSequence(attacker, target);
    }

    private async Task PlayAttackSequence(Creature attacker, Creature target)
    {
        var anim = attacker.Mover.AnimatorController;
        var hitTcs = new TaskCompletionSource<bool>();
        bool isRanged = attacker.AttackType == AttackType.Ranged;

        await attacker.Mover.RotateTowardsAsync(target.transform.position);
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
