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
        if (!TurnOrderController.Instance.IsCurrentTurn(creature))
            return;

        var mover = creature.Mover;
        var oldCell = mover.CurrentCell;
        int speed = creature.GetStat(CreatureStatusType.Speed);
        var moveType = creature.MovementType;

        // 1) Считаем reachable
        List<HexCell> reachable = pathfindingManager
            .GetReachableCells(oldCell, speed, moveType);

        if (!targetCell.IsWalkable || !reachable.Contains(targetCell))
            return;

        // 2) Показываем новую зону и выделяем цель
        highlightController.ClearHighlights();
        highlightController.HighlightReachable(reachable, oldCell);
        targetCell.ShowHighlight(true);

        // 3) СРАЗУ ЖЕ прячем все outline (без анимации), 
        //    чтобы в момент старта перемещения на поле 
        //    ничего не горело
        highlightController.ClearHighlightsImmediate();

        // 4) Двигаем/телепортируем
        bool moved = false;
        if (moveType == MovementType.Teleport)
            moved = await mover.TeleportToCell(targetCell);
        else
        {
            var path = pathfindingManager.FindPath(oldCell, targetCell, moveType);
            if (path != null && path.Count > 0)
                moved = await mover.MoveAlongPath(path);
        }

        if (!moved)
            return;

        // 5) Обновляем occupants
        oldCell.RemoveOccupant(creature.gameObject);
        targetCell.AddOccupant(creature.gameObject, CellObjectType.Creature);

        // 6) Перерисовываем подсветку после перемещения
        var newReachable = pathfindingManager
            .GetReachableCells(targetCell, speed, moveType);
        highlightController.ClearHighlightsImmediate();
        highlightController.HighlightReachable(newReachable, targetCell);

        OnMovementComplete?.Invoke(creature);
    }
}
