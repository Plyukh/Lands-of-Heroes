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
        // 1) Гасим _анимацией_ все контуры
        ClearHighlights();

        // 2) Включаем _анимацией_ только клетки маршрута
        foreach (var cell in path)
            cell.ShowHighlight(true);
    }


    public void HighlightTeleportTarget(HexCell target)
    {
        ClearHighlightsImmediate();    // мгновенно выключить все
        target.ShowHighlight(true);    // включить только эту

    }
}
