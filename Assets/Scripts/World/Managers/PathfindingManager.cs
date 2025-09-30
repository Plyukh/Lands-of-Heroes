using System.Collections.Generic;
using UnityEngine;

public class PathfindingManager : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Ссылки на HexGridManager для доступа к соседям и проходимости")]
    [SerializeField] private HexGridManager gridManager;

    /// <summary>
    /// Ищет кратчайший путь от start до target с учётом типа передвижения.
    /// Возвращает список клеток (не включая start), или null, если путь не найден.
    /// </summary>
    public List<HexCell> FindPath(
        HexCell start,
        HexCell target,
        MovementType moveType)
    {
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

                if (!CanTraverse(neighbour, moveType))
                    continue;

                visited.Add(neighbour);
                parent[neighbour] = cell;
                queue.Enqueue(neighbour);
            }
        }

        if (!parent.ContainsKey(target))
            return null;

        // Восстанавливаем путь от target до start
        var path = new List<HexCell>();
        for (var cur = target; cur != start; cur = parent[cur])
            path.Add(cur);
        path.Reverse();
        return path;
    }

    /// <summary>
    /// Возвращает все клетки, достижимые из start за maxSteps ходов.
    /// Включает сам start.
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
    /// Проверяет, можно ли пройти сквозь клетку в зависимости от MovementType.
    /// </summary>
    private bool CanTraverse(HexCell cell, MovementType moveType)
    {
        if (moveType == MovementType.Flying ||
            moveType == MovementType.Teleport)
        {
            return true;
        }

        return cell.isWalkable;
    }
}
