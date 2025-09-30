using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexGridManager : MonoBehaviour
{
    [Header("Grid Cells")]
    [Tooltip("Все клетки поля боя")]
    public List<HexCell> cells = new List<HexCell>();

    public Dictionary<string, HexCell> CellMap { get; private set; }

    // Смещения соседних клеток для чётных и нечётных рядов (odd-r layout)
    private readonly (int dr, int dc)[] oddRowOffsets = {
        (-1, 0), (-1, +1),
        ( 0, -1), ( 0, +1),
        (+1, 0), (+1, +1)
    };
    private readonly (int dr, int dc)[] evenRowOffsets = {
        (-1, -1), (-1,  0),
        ( 0, -1), ( 0, +1),
        (+1, -1), (+1,  0)
    };

    private void Awake()
    {
        // Строим словарь для быстрого поиска по строке/столбцу
        CellMap = cells.ToDictionary(cell => cell.CellId);
    }

    /// <summary>
    /// Возвращает все соседние клетки для данного в шестиугольной сетке.
    /// </summary>
    public IEnumerable<HexCell> GetNeighbors(HexCell cell)
    {
        bool oddIndexed = ((cell.row - 1) % 2) != 0;
        var offsets = oddIndexed ? oddRowOffsets : evenRowOffsets;

        foreach (var (dr, dc) in offsets)
        {
            string id = $"r{cell.row + dr}_c{cell.column + dc}";
            if (CellMap.TryGetValue(id, out var neighbor))
                yield return neighbor;
        }
    }

    /// <summary>
    /// Обновляет флаг isWalkable для каждой клетки
    /// на основеoccupants, блокирующих проходимость.
    /// </summary>
    public void UpdateAllWalkability()
    {
        var blockingTypes = new HashSet<CellObjectType>
        {
            CellObjectType.Obstacle,
            CellObjectType.ForceField,
            CellObjectType.Creature
        };

        foreach (var cell in cells)
        {
            // Клетка непроходима, если в ней есть любой occupant из blockingTypes
            bool walkable = !cell.occupants.Any(o => blockingTypes.Contains(o.type));
            cell.isWalkable = walkable;
        }
    }
}
