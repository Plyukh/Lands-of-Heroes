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
        // ����, ������� ������ ������ ������������
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
            Debug.LogWarning("������� �������� ������ ������ ��� ��� None � ������.");
            return;
        }

        // ������ ������ ��������
        var occupant = new CellOccupant
        {
            type = type,
            instance = obj
        };

        // ����������� ������ � ������
        obj.transform.position = transform.position;
        obj.transform.SetParent(transform);

        // ��������� � ������
        occupants.Add(occupant);

        // ��������� ������������
        UpdateWalkability();
    }
}