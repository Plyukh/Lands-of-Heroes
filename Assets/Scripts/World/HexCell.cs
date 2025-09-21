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

    [Header("Highlight")]
    [SerializeField] private GameObject highlightObject;

    [Header("Visuals")]
    [SerializeField] private Renderer cellRenderer;

    private void UpdateWalkability()
    {
        // Типы, которые делают клетку непроходимой
        var blockingTypes = new HashSet<CellObjectType>
        {
             CellObjectType.Obstacle,
             CellObjectType.ForceField,
             CellObjectType.Creature
        };

        isWalkable = true;

        foreach (var occupant in occupants)
        {
            if (blockingTypes.Contains(occupant.type))
            {
                isWalkable = false;
                break;
            }
        }
    }


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

        // Обновляем проходимость
        UpdateWalkability();
    }
}