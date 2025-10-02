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
        if (creature == null || targetCell == null) return;
        if (!TurnOrderController.Instance.IsCurrentTurn(creature)) return;

        var mover = creature.Mover;
        var startCell = mover.CurrentCell;
        var moveType = creature.MovementType;

        // —— ТЕЛЕПОРТ ——
        if (moveType == MovementType.Teleport)
        {
            // 1) анимированно гасим всё, подсвечиваем только цель
            highlightController.HighlightTeleportTarget(targetCell);

            // 2) ждём конца анимации телепорта (PlayStartTeleport → OnTeleportMove → OnTeleportEnd)
            bool teleported = await mover.TeleportToCell(targetCell);

            if (teleported)
            {
                // 3) гасим её контур
                targetCell.ShowHighlight(false);

                // 4) переносим Occupant и завершаем ход
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

        startCell.RemoveOccupant(creature.gameObject);
        targetCell.AddOccupant(creature.gameObject, CellObjectType.Creature);
        OnMovementComplete?.Invoke(creature);
    }
}