using System.Collections.Generic;
using System.Linq;
using UnityEditor.VersionControl;
using UnityEngine;

[DisallowMultipleComponent]
public class HighlightController : MonoBehaviour
{
    [Header("Grid Reference")]
    [SerializeField] private HexGridManager gridManager;

    // Клетки, окрашенные как active
    private HashSet<HexCell> activeCells = new HashSet<HexCell>();

    /// <summary>
    /// Подсвечивает зону досягаемости активным Outline,
    /// все остальные переводит в Inactive.
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
    /// Накладывает Preview-обводку на клетки пути,
    /// не трогая остальных: activeCells останутся Active,
    /// все прочие — Inactive.
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
    /// Отменяет превью, возвращая всем activeCells состояние Active,
    /// остальным — Inactive.
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
    /// Полностью сбрасывает все обводки в Inactive.
    /// </summary>
    public void ClearHighlights()
    {
        activeCells.Clear();
        foreach (var cell in gridManager.Cells)
            cell.SetOutlineState(OutlineState.Inactive);
    }
}
