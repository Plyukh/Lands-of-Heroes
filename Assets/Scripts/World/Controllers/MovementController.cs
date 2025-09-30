using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[DisallowMultipleComponent]
public class MovementController : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("�������� ������ ���� � ���� ������������")]
    [SerializeField] private PathfindingManager pathfindingManager;
    [Tooltip("���������� ��������� ��������� ������")]
    [SerializeField] private HighlightController highlightController;

    public event Action<Creature> OnMovementComplete;

    public async void OnCellClicked(Creature creature, HexCell targetCell)
    {
        if (creature == null || targetCell == null)
            return;

        // ��� �� ��� ����� ��������?
        if (!TurnOrderController.Instance.IsCurrentTurn(creature))
            return;

        var mover = creature.Mover;
        var oldCell = mover.CurrentCell;
        int speed = creature.GetStat(CreatureStatusType.Speed);
        var moveType = creature.MovementType;

        // 1) ��������� ��� reachable ������
        List<HexCell> reachable = pathfindingManager
            .GetReachableCells(oldCell, speed, moveType);

        // 2) ������� ������ ������ ���� �������� �������� � ��� � ����
        if (!targetCell.IsWalkable || !reachable.Contains(targetCell))
            return;

        // 3) ������������ ���� � ��������� ������
        highlightController.ClearHighlights();
        highlightController.HighlightReachable(reachable, oldCell);
        targetCell.ShowHighlight(true);

        // 4) ���������� ��� �������������
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

        // 5) ��������� occupants � �������
        oldCell.RemoveOccupant(creature.gameObject);
        targetCell.AddOccupant(creature.gameObject, CellObjectType.Creature);

        // 6) ������������� � ������������ ����� ����
        var newReachable = pathfindingManager
            .GetReachableCells(targetCell, speed, moveType);

        highlightController.ClearHighlights();
        highlightController.HighlightReachable(newReachable, targetCell);

        // 7) ����������, ��� ��� �������� �������� 
        OnMovementComplete?.Invoke(creature);
    }
}
