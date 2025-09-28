using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    [Header("Main")]
    public int row;
    public int column;
    public bool isWalkable;

    [Header("Occupants")]
    public List<CellOccupant> occupants = new List<CellOccupant>();

    [Header("Outline Objects")]
    [Tooltip("Обычный контур (неактивный)")]
    [SerializeField] private GameObject inactiveOutline;
    [Tooltip("Подсветка (активный)")]
    [SerializeField] private GameObject activeOutline;
    [Tooltip("Подсветка (активный) еффект")]
    [SerializeField] private ParticleSystem activeOutlineEffect;

    [Header("Visuals")]
    [SerializeField] private Renderer cellRenderer;

    public string CellId => $"r{row}_c{column}";

    public void SetMaterial(Material mat)
    {
        if (cellRenderer != null && mat != null)
        {
            cellRenderer.material = mat;
        }
    }

    public void SetCellObject(GameObject obj, CellObjectType type)
    {
        if (obj == null || type == CellObjectType.None)
        {
            Debug.LogWarning("Попытка добавить пустой объект или тип None в клетку.");
            return;
        }

        // Создаём нового носителя
        var occupant = new CellOccupant
        {
            type = type,
            instance = obj
        };

        // Привязываем объект к клетке
        obj.transform.position = transform.position;
        obj.transform.SetParent(transform);

        // Добавляем в список
        occupants.Add(occupant);
    }

    public void ShowHighlight(bool highlight)
    {
        // Если пытаются подсветить непроходимую клетку — игнорируем
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