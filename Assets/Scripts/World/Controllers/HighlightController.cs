using System.Collections.Generic;
using UnityEngine;

public class HighlightController : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("������ �� �������� ����� ��� ������� �� ���� HexCell")]
    [SerializeField] private HexGridManager gridManager;

    /// <summary>
    /// ������� ��������� �� ���� ������� ����.
    /// </summary>
    public void ClearHighlights()
    {
        foreach (var cell in gridManager.cells)
        {
            cell.ShowHighlight(false);
        }
    }

    /// <summary>
    /// ������������ ��� ���������� ������, ����� ���������.
    /// </summary>
    /// <param name="reachable">������, �� ������� ����� �����</param>
    /// <param name="startCell">������, �� ������� ����� �������� (�� ��������������)</param>
    public void HighlightReachable(IEnumerable<HexCell> reachable, HexCell startCell)
    {
        foreach (var cell in reachable)
        {
            if (cell != startCell && cell.isWalkable)
            {
                cell.ShowHighlight(true);
            }
        }
    }
}
