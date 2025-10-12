using System.Collections.Generic;
using System.Linq;
using UnityEditor.VersionControl;
using UnityEngine;

[DisallowMultipleComponent]
public class HighlightController : MonoBehaviour
{
    [Header("Grid Reference")]
    [SerializeField] private HexGridManager gridManager;

    // ������, ���������� ��� active
    private HashSet<HexCell> activeCells = new HashSet<HexCell>();

    /// <summary>
    /// ������������ ���� ������������ �������� Outline,
    /// ��� ��������� ��������� � Inactive.
    /// </summary>
    public void HighlightReachable(IEnumerable<HexCell> reachable, HexCell startCell)
    {
        activeCells.Clear();

        foreach (var cell in gridManager.Cells)
        {
            bool isActive = cell != startCell && reachable.Contains(cell);
            cell.SetOutlineState(isActive
                ? OutlineState.Active
                : OutlineState.Inactive);

            if (isActive)
                activeCells.Add(cell);
        }
    }

    /// <summary>
    /// ����������� Preview-������� �� ������ ����,
    /// �� ������ ���������: activeCells ��������� Active,
    /// ��� ������ � Inactive.
    /// </summary>
    public void PreviewPath(IReadOnlyList<HexCell> path)
    {
        foreach (var cell in gridManager.Cells)
        {
            if (path.Contains(cell))
            {
                cell.SetOutlineState(OutlineState.Preview);
            }
            else if (activeCells.Contains(cell))
            {
                cell.SetOutlineState(OutlineState.Active);
            }
            else
            {
                cell.SetOutlineState(OutlineState.Inactive);
            }
        }
    }

    /// <summary>
    /// �������� ������, ��������� ���� activeCells ��������� Active,
    /// ��������� � Inactive.
    /// </summary>
    public void ResetPreview()
    {
        foreach (var cell in gridManager.Cells)
        {
            cell.SetOutlineState(activeCells.Contains(cell)
                ? OutlineState.Active
                : OutlineState.Inactive);
        }
    }

    /// <summary>
    /// ��������� ���������� ��� ������� � Inactive.
    /// </summary>
    public void ClearHighlights()
    {
        activeCells.Clear();
        foreach (var cell in gridManager.Cells)
            cell.SetOutlineState(OutlineState.Inactive);
    }
}
