using System.Collections.Generic;
using UnityEngine;

public class HighlightController : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Ссылка на менеджер сетки для доступа ко всем HexCell")]
    [SerializeField] private HexGridManager gridManager;

    /// <summary>
    /// Убирает подсветку на всех клетках поля.
    /// </summary>
    public void ClearHighlights()
    {
        foreach (var cell in gridManager.cells)
        {
            cell.ShowHighlight(false);
        }
    }

    /// <summary>
    /// Подсвечивает все достижимые клетки, кроме стартовой.
    /// </summary>
    /// <param name="reachable">Клетки, до которых можно дойти</param>
    /// <param name="startCell">Клетка, на которой стоит существо (не подсвечивается)</param>
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
