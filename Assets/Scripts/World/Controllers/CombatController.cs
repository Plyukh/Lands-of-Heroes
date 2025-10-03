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

        // 1) Дальний бой — гасим всё и атакуем сразу
        if (attacker.AttackType == AttackType.Ranged)
        {
            highlightController.ClearHighlights();
            await PlayAttackSequence(attacker, target);
            return;
        }

        // 2) Ближний — собираем соседние свободные
        var neighborCells = pathfindingManager
            .GetReachableCells(targetCell, 1, MovementType.Teleport)
            .Where(c => c.IsWalkable)
            .ToList();

        // 2a) Если уже в соседней клетке — сразу в атаку
        if (neighborCells.Contains(startCell))
        {
            highlightController.ClearHighlights();
            await PlayAttackSequence(attacker, target);
            return;
        }

        // 3) Ищем, куда подойти
        var reachable = pathfindingManager.GetReachableCells(startCell, speed, moveType);
        var candidates = neighborCells.Intersect(reachable).ToList();
        if (candidates.Count == 0)
            return;

        // Выбираем ближайшую к startCell
        var attackPos = candidates
            .OrderBy(c => Vector3.Distance(startCell.transform.position, c.transform.position))
            .First();

        bool moved = false;

        // 4) Логика перемещения с подсветкой
        if (moveType == MovementType.Teleport)
        {
            // для телепорта: сразу гасим всё, подсвечиваем только цель
            highlightController.HighlightTeleportTarget(attackPos);

            moved = await mover.TeleportToCell(attackPos);

            if (moved)
            {
                // гасим её контур
                attackPos.ShowHighlight(false);
            }
        }
        else
        {
            // строим путь
            var path = pathfindingManager.FindPath(startCell, attackPos, moveType);
            if (path == null || path.Count == 0)
                return;

            // подсвечиваем весь маршрут
            highlightController.HighlightPath(path);

            // поэтапно гася контур через событие
            void OnStep(HexCell cell) => cell.ShowHighlight(false);
            mover.OnCellEntered += OnStep;

            moved = await mover.MoveAlongPath(path);

            mover.OnCellEntered -= OnStep;
        }

        if (!moved)
            return;

        // 5) Переносим Occupant и собираемся в атаку
        startCell.RemoveOccupant(attacker.gameObject);
        attackPos.AddOccupant(attacker.gameObject, CellObjectType.Creature);

        // 6) Убираем остатки подсветки перед атакой
        highlightController.ClearHighlights();

        // 7) Запускаем анимацию атаки
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
