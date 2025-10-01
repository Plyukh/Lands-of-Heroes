using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class HighlightController : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Менеджер сетки для доступа ко всем HexCell")]
    [SerializeField] private HexGridManager gridManager;

    private void Awake()
    {
        if (gridManager == null)
            Debug.LogError("[HighlightController] HexGridManager is not assigned!");
    }

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
}
