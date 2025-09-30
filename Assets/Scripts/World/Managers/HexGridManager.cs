using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexGridManager : MonoBehaviour
{
    [Header("Grid Cells")]
    [Tooltip("��� ������ ���� ���")]
    public List<HexCell> cells = new List<HexCell>();

    public Dictionary<string, HexCell> CellMap { get; private set; }

    // �������� �������� ������ ��� ������ � �������� ����� (odd-r layout)
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
        // ������ ������� ��� �������� ������ �� ������/�������
        CellMap = cells.ToDictionary(cell => cell.CellId);
    }

    /// <summary>
    /// ���������� ��� �������� ������ ��� ������� � ������������� �����.
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
    /// ��������� ���� isWalkable ��� ������ ������
    /// �� ������occupants, ����������� ������������.
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
            // ������ �����������, ���� � ��� ���� ����� occupant �� blockingTypes
            bool walkable = !cell.occupants.Any(o => blockingTypes.Contains(o.type));
            cell.isWalkable = walkable;
        }
    }
}
