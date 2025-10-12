using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[DisallowMultipleComponent]
public class CombatController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PathfindingManager pathfindingManager;
    [SerializeField] private HighlightController highlightController;

    public event Action<Creature> OnCombatComplete;

    /// <summary>
    /// Запускает полный цикл: (move → attack) для melee, или сразу (attack) для ranged.
    /// Путь и зона остаются подсвеченными активным материалом, а маршрут
    /// превью подкрашивается previewMaterial.
    /// </summary>
    public async Task ExecuteCombat(Creature attacker, Creature target, HexCell attackCell)
    {
        if (attacker == null || target == null || attacker == target)
            return;
        if (!TurnOrderController.Instance.IsCurrentTurn(attacker))
            return;

        var mover = attacker.Mover;
        var startCell = mover.CurrentCell;

<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
        // дальний бой — без перемещения
=======
        // 1) Дальний бой — гасим всё и атакуем сразу
>>>>>>> parent of 5c41c94 (Fixed movement bug)
=======
        // 1) Дальний бой — гасим всё и атакуем сразу
>>>>>>> parent of 5c41c94 (Fixed movement bug)
=======
        // 1) Дальний бой — гасим всё и атакуем сразу
>>>>>>> parent of 5c41c94 (Fixed movement bug)
        if (attacker.AttackType == AttackType.Ranged)
        {
            await PlayAttackSequence(attacker, target);
            return;
        }

<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
        // проверяем, что выбранная melee-клетка допустима
        int speed = attacker.GetStat(CreatureStatusType.Speed);
        var moveType = attacker.MovementType;
=======
=======
>>>>>>> parent of 5c41c94 (Fixed movement bug)
=======
>>>>>>> parent of 5c41c94 (Fixed movement bug)
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
<<<<<<< HEAD
<<<<<<< HEAD
>>>>>>> parent of 5c41c94 (Fixed movement bug)
=======
>>>>>>> parent of 5c41c94 (Fixed movement bug)
=======
>>>>>>> parent of 5c41c94 (Fixed movement bug)
        var reachable = pathfindingManager.GetReachableCells(startCell, speed, moveType);
        if (!reachable.Contains(attackCell) || !attackCell.IsWalkable)
            return;

<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
        // находим маршрут
        var path = pathfindingManager.FindPath(startCell, attackCell, moveType);
        if (path == null || path.Count == 0)
            return;

        // показываем превью маршрута поверх активной зоны
        highlightController.PreviewPath(path);

        // выполняем движение
        bool moved = await mover.MoveAlongPath(path);

        // возвращаем к активному материалу
        highlightController.ResetPreview();
=======
=======
>>>>>>> parent of 5c41c94 (Fixed movement bug)
=======
>>>>>>> parent of 5c41c94 (Fixed movement bug)
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
>>>>>>> parent of 5c41c94 (Fixed movement bug)

        if (!moved)
            return;

<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
        // обновляем Occupant
=======
        // 5) Переносим Occupant и собираемся в атаку
>>>>>>> parent of 5c41c94 (Fixed movement bug)
=======
        // 5) Переносим Occupant и собираемся в атаку
>>>>>>> parent of 5c41c94 (Fixed movement bug)
=======
        // 5) Переносим Occupant и собираемся в атаку
>>>>>>> parent of 5c41c94 (Fixed movement bug)
        startCell.RemoveOccupant(attacker.gameObject);
        attackCell.AddOccupant(attacker.gameObject, CellObjectType.Creature);

<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
        // атака
        await PlayAttackSequence(attacker, target);
    }

    public async Task PlayAttackSequence(Creature attacker, Creature target)
=======
        // 6) Убираем остатки подсветки перед атакой
        highlightController.ClearHighlights();

        // 7) Запускаем анимацию атаки
        await PlayAttackSequence(attacker, target);
    }

=======
        // 6) Убираем остатки подсветки перед атакой
        highlightController.ClearHighlights();

        // 7) Запускаем анимацию атаки
        await PlayAttackSequence(attacker, target);
    }

>>>>>>> parent of 5c41c94 (Fixed movement bug)
=======
        // 6) Убираем остатки подсветки перед атакой
        highlightController.ClearHighlights();

        // 7) Запускаем анимацию атаки
        await PlayAttackSequence(attacker, target);
    }

>>>>>>> parent of 5c41c94 (Fixed movement bug)

    private async Task PlayAttackSequence(Creature attacker, Creature target)
>>>>>>> parent of 5c41c94 (Fixed movement bug)
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
