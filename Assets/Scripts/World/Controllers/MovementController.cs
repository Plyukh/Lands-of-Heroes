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
    [Tooltip("Контроллер подсветки клеток")]
    [SerializeField] private HighlightController highlightController;

    public event Action<Creature> OnMovementComplete;

    public async void OnCellClicked(Creature creature, HexCell targetCell)
    {
        if (creature == null || targetCell == null)
            return;
        if (!TurnOrderController.Instance.IsCurrentTurn(creature))
            return;

        var mover = creature.Mover;
        var startCell = mover.CurrentCell;
        var moveType = creature.MovementType;
        int speed = creature.GetStat(CreatureStatusType.Speed);

        // 1) Получаем зону досягаемости с учётом типа движения
        var reachable = pathfindingManager.GetReachableCells(startCell, speed, moveType);

        // 2) Если цель вне досягаемости — игнорируем
        if (!reachable.Contains(targetCell))
            return;

        // 3) Для телепорта/полёта запрещаем приземляться на занятую клетку
        if ((moveType == MovementType.Teleport || moveType == MovementType.Flying)
            && !targetCell.IsWalkable)
        {
            return;
        }

        if (moveType == MovementType.Teleport)
        {
            highlightController.HighlightTeleportTarget(targetCell);

            bool teleported = await mover.TeleportToCell(targetCell);
            if (!teleported)
                return;

            // выключаем подсветку и обновляем Occupant
            targetCell.ShowHighlight(false);
            startCell.RemoveOccupant(creature.gameObject);
            targetCell.AddOccupant(creature.gameObject, CellObjectType.Creature);

            OnMovementComplete?.Invoke(creature);
            return;
        }

        // —— Обычное перемещение по пути ——
        var path = pathfindingManager.FindPath(startCell, targetCell, moveType);
        if (path == null || path.Count == 0)
            return;

        highlightController.HighlightPath(path);

        // снимаем подсветку по мере движения
        void OnStep(HexCell cell) => cell.ShowHighlight(false);
        mover.OnCellEntered += OnStep;

        bool moved = await mover.MoveAlongPath(path);
        mover.OnCellEntered -= OnStep;
        if (!moved)
            return;

        startCell.RemoveOccupant(creature.gameObject);
        targetCell.AddOccupant(creature.gameObject, CellObjectType.Creature);
        OnMovementComplete?.Invoke(creature);
    }

    public async void MoveAlongPath(Creature creature, List<HexCell> path)
    {
        if (creature == null || path == null || path.Count == 0)
            return;

        var mover = creature.Mover;
        var startCell = mover.CurrentCell;

        // 1) Показываем весь путь
        highlightController.HighlightPath(path);

        // 2) Подписываемся на событие входа в клетку, чтобы гасить её highlight
        void OnStep(HexCell cell) => cell.ShowHighlight(false);
        mover.OnCellEntered += OnStep;

        // 3) Ждём завершения анимации движения
        bool moved = await mover.MoveAlongPath(path);

        // 4) Убираем подписку
        mover.OnCellEntered -= OnStep;

        if (!moved)
            return;

        // 5) Обновляем Occupant на клетках
        startCell.RemoveOccupant(creature.gameObject);
        var targetCell = path[path.Count - 1];
        targetCell.AddOccupant(creature.gameObject, CellObjectType.Creature);

        OnMovementComplete?.Invoke(creature);
    }
}