using System.Collections.Generic;
using UnityEngine;

public class PathfindingManager : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("������ �� HexGridManager ��� ������� � ������� � ������������")]
    [SerializeField] private HexGridManager gridManager;

    public List<HexCell> FindPath(
        HexCell start,
        HexCell target,
        MovementType moveType)
    {
        // ���� ����� � ���� ���������, ���������� ������ ����
        if (start == target)
            return new List<HexCell>();

        var queue = new Queue<HexCell>();
        var parent = new Dictionary<HexCell, HexCell>();
        var visited = new HashSet<HexCell> { start };

        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var cell = queue.Dequeue();
            if (cell == target)
                break;

            foreach (var neighbour in gridManager.GetNeighbors(cell))
            {
                if (visited.Contains(neighbour))
                    continue;

                // ��������� �������� �� target ���� ���� CanTraverse == false
                bool isTarget = neighbour == target;
                if (!isTarget && !CanTraverse(neighbour, moveType))
                    continue;

                visited.Add(neighbour);
                parent[neighbour] = cell;
                queue.Enqueue(neighbour);
            }
        }

        // ���� target ��� � �� ���������� � parent, ���� ���
        if (!parent.ContainsKey(target))
            return null;

        // ��������������� ���� �� target ������� � start
        var path = new List<HexCell>();
        var cur = target;
        while (cur != start)
        {
            path.Add(cur);
            cur = parent[cur];
        }
        path.Reverse();
        return path;
    }

    /// <summary>
    /// ���������� ��� ������, ���������� �� start �� maxSteps �����.
    /// �������� ��� start.
    /// </summary>
    public List<HexCell> GetReachableCells(
        HexCell start,
        int maxSteps,
        MovementType moveType)
    {
        var result = new List<HexCell>();
        var visited = new HashSet<HexCell> { start };
        var queue = new Queue<(HexCell cell, int step)>();
        queue.Enqueue((start, 0));

        while (queue.Count > 0)
        {
            var (cell, step) = queue.Dequeue();
            result.Add(cell);

            if (step >= maxSteps)
                continue;

            foreach (var neighbour in gridManager.GetNeighbors(cell))
            {
                if (visited.Contains(neighbour))
                    continue;

                if (!CanTraverse(neighbour, moveType))
                    continue;

                visited.Add(neighbour);
                queue.Enqueue((neighbour, step + 1));
            }
        }

        return result;
    }

    /// <summary>
    /// ���������, ����� �� ������ ������ ������ � ����������� �� MovementType.
    /// </summary>
    private bool CanTraverse(HexCell cell, MovementType moveType)
    {
        if (moveType == MovementType.Flying ||
            moveType == MovementType.Teleport)
        {
            return true;
        }

        return cell.IsWalkable;
    }
}
