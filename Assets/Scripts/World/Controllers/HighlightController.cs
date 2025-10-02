using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class HighlightController : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("�������� ����� ��� ������� �� ���� HexCell")]
    [SerializeField] private HexGridManager gridManager;

    public void ClearHighlights()
    {
        foreach (var cell in gridManager.Cells)
        {
            cell.ShowHighlight(false);
        }
    }

    public void ClearHighlightsImmediate()
    {
        foreach (var cell in gridManager.Cells)
        {
            cell.ResetHighlight();
        }
    }

    public void HighlightReachable(IEnumerable<HexCell> reachable, HexCell startCell)
    {
        foreach (var cell in reachable)
        {
            if (cell != startCell && cell.IsWalkable)
            {
                cell.ShowHighlight(true);
            }
        }
    }

    public void HighlightPath(IReadOnlyList<HexCell> path)
    {
        // 1) ����� _���������_ ��� �������
        ClearHighlights();

        // 2) �������� _���������_ ������ ������ ��������
        foreach (var cell in path)
            cell.ShowHighlight(true);
    }


    public void HighlightTeleportTarget(HexCell target)
    {
        ClearHighlightsImmediate();    // ��������� ��������� ���
        target.ShowHighlight(true);    // �������� ������ ���

    }
}
