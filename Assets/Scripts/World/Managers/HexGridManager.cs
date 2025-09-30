using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class HexGridManager : MonoBehaviour
{
    [Header("Grid Cells")]
    [Tooltip("Все клетки поля боя")]
    [SerializeField] private List<HexCell> cells = new List<HexCell>();
    public IReadOnlyList<HexCell> Cells => cells;

    public Dictionary<string, HexCell> CellMap { get; private set; }

    private static readonly (int dr, int dc)[] oddRowOffsets = {
        (-1,  0), (-1, +1),
        ( 0, -1), ( 0, +1),
        (+1,  0), (+1, +1)
    };
    private static readonly (int dr, int dc)[] evenRowOffsets = {
        (-1, -1), (-1,  0),
        ( 0, -1), ( 0, +1),
        (+1, -1), (+1,  0)
    };

    private void Awake()
    {
        CellMap = cells.ToDictionary(cell => cell.CellId);
    }

    public IEnumerable<HexCell> GetNeighbors(HexCell cell)
    {
        bool odd = ((cell.row - 1) % 2) != 0;
        var offsets = odd ? oddRowOffsets : evenRowOffsets;

        foreach (var (dr, dc) in offsets)
        {
            string id = $"r{cell.row + dr}_c{cell.column + dc}";
            if (CellMap.TryGetValue(id, out var neighbor))
                yield return neighbor;
        }
    }

    public void UpdateAllWalkability()
    {
        foreach (var cell in cells)
        {
            cell.RefreshWalkable();
        }
    }
}
