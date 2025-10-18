using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class HighlightController : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Менеджер сетки для доступа ко всем HexCell")]
    [SerializeField] private HexGridManager gridManager;

    public void ClearHighlights()
    {
        foreach (var cell in gridManager.Cells)
        {
            cell.ShowHighlight(false);
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
        ClearHighlights();

        // Animate the highlight for each cell in the path
        foreach (var cell in path)
            cell.ShowHighlight(true);
    }

    public void HighlightTeleportTarget(HexCell target)
    {
        ClearHighlights();          // Instantly turn off all other highlights
        target.ShowHighlight(true); // Turn on the target highlight
    }
}
