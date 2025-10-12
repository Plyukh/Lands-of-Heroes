using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[DisallowMultipleComponent]
public class MovementController : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Менеджер поиска пути и зоны досягаемости")]
    [SerializeField] private PathfindingManager pathfindingManager;
    [Tooltip("Контроллер подсветки клеток с active/preview-материалами")]
    [SerializeField] private HighlightController highlightController;

    public event Action<Creature> OnMovementComplete;

    /// <summary>
    /// Вызывается при клике по пустой клетке: показывает превью пути, выполняет движение,
    /// сбрасывает превью и уведомляет об окончании хода.
    /// </summary>
    public async void OnCellClicked(Creature creature, HexCell targetCell)
    {
        if (creature == null || targetCell == null) return;
        if (!TurnOrderController.Instance.IsCurrentTurn(creature)) return;

        var mover = creature.Mover;
        var startCell = mover.CurrentCell;
        var moveType = creature.MovementType;
        int speed = creature.GetStat(CreatureStatusType.Speed);

<<<<<<< HEAD
        // 1) Получаем зону досягаемости
        var reachable = pathfindingManager.GetReachableCells(startCell, speed, moveType);
        if (!reachable.Contains(targetCell))
            return;

        // 2) Для Teleport/Flying запрещаем приземляться на непроходимую
        if ((moveType == MovementType.Teleport || moveType == MovementType.Flying)
            && !targetCell.IsWalkable)
        {
            return;
        }
=======
        // Проверяем, находится ли targetCell в зоне досягаемости
        var reachable = pathfindingManager.GetReachableCells(startCell, speed, moveType);
        if (!reachable.Contains(targetCell))
            return; // цель вне досягаемости — игнорируем клик
>>>>>>> parent of 5c41c94 (Fixed movement bug)

        // 3) Готовим список клеток для превью и движения
        List<HexCell> pathCells;
        bool moved = false;

        if (moveType == MovementType.Teleport)
        {
<<<<<<< HEAD
            // Для телепорта превью — одна клетка
            pathCells = new List<HexCell> { targetCell };
            highlightController.PreviewPath(pathCells);
            moved = await mover.TeleportToCell(targetCell);
        }
        else
        {
            // Обычный путь через A*/BFS
            pathCells = pathfindingManager.FindPath(startCell, targetCell, moveType);
            if (pathCells == null || pathCells.Count == 0)
                return;

            highlightController.PreviewPath(pathCells);
            moved = await mover.MoveAlongPath(pathCells);
        }

        // 4) Сброс превью — возвращаем активную подсветку зоны
        highlightController.ResetPreview();

        if (!moved)
            return;
=======
            // 1) Сразу гасим все подсветки и включаем только цель
            highlightController.HighlightTeleportTarget(targetCell);

            // 2) Телепорт
            bool teleported = await mover.TeleportToCell(targetCell);

            if (teleported)
            {
                // 3) Сразу гасим и её
                targetCell.ShowHighlight(false);

                // 4) Обновляем Occupant и завершаем ход
                startCell.RemoveOccupant(creature.gameObject);
                targetCell.AddOccupant(creature.gameObject, CellObjectType.Creature);
                OnMovementComplete?.Invoke(creature);
            }
            return;
        }

        // —— Обычное движение по пути ——
        var fullPath = pathfindingManager.FindPath(startCell, targetCell, moveType);
        if (fullPath == null || fullPath.Count == 0) return;

        highlightController.HighlightPath(fullPath);

        void OnStep(HexCell cell) => cell.ShowHighlight(false);
        mover.OnCellEntered += OnStep;

        bool moved = await mover.MoveAlongPath(fullPath);

        mover.OnCellEntered -= OnStep;
        if (!moved) return;
>>>>>>> parent of 5c41c94 (Fixed movement bug)

        // 5) Перенос Occupant и завершение хода
        startCell.RemoveOccupant(creature.gameObject);
        targetCell.AddOccupant(creature.gameObject, CellObjectType.Creature);
        OnMovementComplete?.Invoke(creature);
    }
}
