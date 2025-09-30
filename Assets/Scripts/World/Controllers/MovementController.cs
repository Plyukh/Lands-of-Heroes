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
    [Tooltip("Контроллер подсветки доступных клеток")]
    [SerializeField] private HighlightController highlightController;

    public event Action<Creature> OnMovementComplete;

    public async void OnCellClicked(Creature creature, HexCell targetCell)
    {
        if (creature == null || targetCell == null)
            return;

        // Это не ход этого существа?
        if (!TurnOrderController.Instance.IsCurrentTurn(creature))
            return;

        var mover = creature.Mover;
        var oldCell = mover.CurrentCell;
        int speed = creature.GetStat(CreatureStatusType.Speed);
        var moveType = creature.MovementType;

        // 1) Вычисляем все reachable клетки
        List<HexCell> reachable = pathfindingManager
            .GetReachableCells(oldCell, speed, moveType);

        // 2) Целевой клетке должно быть доступно движение и она в зоне
        if (!targetCell.IsWalkable || !reachable.Contains(targetCell))
            return;

        // 3) Подсвечиваем зону и выбранную клетку
        highlightController.ClearHighlights();
        highlightController.HighlightReachable(reachable, oldCell);
        targetCell.ShowHighlight(true);

        // 4) Перемещаем или телепортируем
        bool moved = false;
        if (moveType == MovementType.Teleport)
        {
            moved = await mover.TeleportToCell(targetCell);
        }
        else
        {
            var path = pathfindingManager.FindPath(oldCell, targetCell, moveType);
            if (path != null && path.Count > 0)
                moved = await mover.MoveAlongPath(path);
        }

        if (!moved)
            return;

        // 5) Обновляем occupants в клетках
        oldCell.RemoveOccupant(creature.gameObject);
        targetCell.AddOccupant(creature.gameObject, CellObjectType.Creature);

        // 6) Пересчитываем и подсвечиваем новую зону
        var newReachable = pathfindingManager
            .GetReachableCells(targetCell, speed, moveType);

        highlightController.ClearHighlights();
        highlightController.HighlightReachable(newReachable, targetCell);

        // 7) Уведомляем, что ход существо завершён 
        OnMovementComplete?.Invoke(creature);
    }
}
