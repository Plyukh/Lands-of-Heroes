using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PathfindingManager pathfindingManager;
    [SerializeField] private HighlightController highlightController;

    /// <summary>
    /// ����������, ����� �������� ������� ������������� ��� �����������������.
    /// </summary>
    public event Action<Creature> OnMovementComplete;

    /// <summary>
    /// ������ ����� �� ������ ��� �����������.
    /// </summary>
    public async void OnCellClicked(Creature creature, HexCell targetCell)
    {
        if (creature == null || targetCell == null)
            return;

        var mover = creature.Mover;
        var startCell = mover.CurrentCell;
        int speed = creature.GetStat(CreatureStatusType.Speed);
        var moveType = creature.MovementType;

        // 1) ��������� ���� ��������� �����
        List<HexCell> reachable = pathfindingManager
            .GetReachableCells(startCell, speed, moveType);

        // 2) ���������, �������� �� ���� � � ���� �� ���
        if (!targetCell.isWalkable || !reachable.Contains(targetCell))
            return;

        // 3) ������������ ��� reachable � ��������� ������
        highlightController.ClearHighlights();
        highlightController.HighlightReachable(reachable, startCell);
        targetCell.ShowHighlight(true);

        // 4) ��������� ��� �������������
        bool moved = false;
        if (moveType == MovementType.Teleport)
        {
            moved = await mover.TeleportToCell(targetCell);
        }
        else
        {
            List<HexCell> path = pathfindingManager
                .FindPath(startCell, targetCell, moveType);
            if (path != null && path.Count > 0)
                moved = await mover.MoveAlongPath(path);
        }

        // 5) ���� ������������� � ��������� ��������� � �������� �������
        if (moved)
        {
            var newReachable = pathfindingManager
                .GetReachableCells(mover.CurrentCell, speed, moveType);

            highlightController.ClearHighlights();
            highlightController.HighlightReachable(newReachable, mover.CurrentCell);

            OnMovementComplete?.Invoke(creature);
        }
    }
}
