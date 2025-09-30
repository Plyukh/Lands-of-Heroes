using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class HexCell : MonoBehaviour
{
    [Header("Grid Position")]
    [Tooltip("Row index in the grid")]
    public int row;
    [Tooltip("Column index in the grid")]
    public int column;

    [Header("Cell State")]
    [Tooltip("Можно ли ходить по этой клетке; обновляется автоматически при изменении occupants")]
    [SerializeField] private bool isWalkable = true;
    public bool IsWalkable => isWalkable;

    [Header("Occupants")]
    [Tooltip("Список объектов, стоящих на этой клетке")]
    [SerializeField] private List<CellOccupant> occupants = new List<CellOccupant>();
    public IReadOnlyList<CellOccupant> Occupants => occupants;

    [Header("Outline Objects")]
    [Tooltip("Статичный контур")]
    [SerializeField] private GameObject inactiveOutline;
    [Tooltip("Активная подсветка")]
    [SerializeField] private GameObject activeOutline;
    [Tooltip("Эффект частицы для активной подсветки")]
    [SerializeField] private ParticleSystem activeOutlineEffect;

    [Header("Visual")]
    [Tooltip("Renderer для изменения материала клетки")]
    [SerializeField] private Renderer cellRenderer;

    public string CellId => $"r{row}_c{column}";

    public void SetMaterial(Material mat)
    {
        if (cellRenderer != null && mat != null)
            cellRenderer.material = mat;
    }

    public void AddOccupant(GameObject instance, CellObjectType type)
    {
        if (instance == null || type == CellObjectType.None)
            return;

        // Проверяем дубли
        if (occupants.Any(o => o.instance == instance))
            return;

        // Привязываем объект к этой клетке
        instance.transform.SetParent(transform, false);
        instance.transform.position = transform.position;

        occupants.Add(new CellOccupant
        {
            instance = instance,
            type = type
        });

        // Если это существо или препятствие — клетка становится непроходимой
        if (type == CellObjectType.Creature || type == CellObjectType.Obstacle)
            RefreshWalkable();
    }

    public void RemoveOccupant(GameObject instance)
    {
        var occ = occupants.FirstOrDefault(o => o.instance == instance);
        if (occ == null)
            return;

        occupants.Remove(occ);
        // Не уничтожаем сам объект — если нужно, делайте это извне

        RefreshWalkable();
    }

    public void ClearAllOccupants()
    {
        occupants.Clear();
        RefreshWalkable();
    }

    public void RefreshWalkable()
    {
        // Сюда добавьте другие типы, которые должны блокировать клетку
        var blocking = new[]
        {
            CellObjectType.Creature,
            CellObjectType.Obstacle,
            CellObjectType.ForceField
        };

        isWalkable = !occupants.Any(o => blocking.Contains(o.type));
    }

    public void ShowHighlight(bool highlight)
    {
        if (highlight && !isWalkable)
            return;

        inactiveOutline?.SetActive(!highlight);
        activeOutline?.SetActive(highlight);

        if (activeOutlineEffect != null)
        {
            var main = activeOutlineEffect.main;
            main.loop = highlight;
            if (highlight) activeOutlineEffect.Play(true);
            else activeOutlineEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void ResetHighlight()
    {
        ShowHighlight(false);
    }
}
