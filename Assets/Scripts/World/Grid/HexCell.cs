using System.Collections;
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
    public Creature GetOccupantCreature()
    {
        foreach (var item in occupants)
        {
            if(item.type == CellObjectType.Creature)
            {
                return item.instance.GetComponent<Creature>();
            }
        }
        return null;
    }

    [Header("Outline Object")]
    [SerializeField] private Animator outlineAnimator;
    [Tooltip("Активная подсветка")]
    [SerializeField] private GameObject activeOutline;

    [Header("Visual")]
    [Tooltip("Renderer для изменения материала клетки")]
    [SerializeField] private Renderer cellRenderer;

    public string CellId => $"r{row}_c{column}";

    public GameObject ActiveOutline => activeOutline;

    private static readonly int EnableHash = Animator.StringToHash("Enable");
    private static readonly int DisableHash = Animator.StringToHash("Disable");
    private bool isDisabling;

    public void SetMaterial(Material mat)
    {
        if (cellRenderer != null && mat != null)
            cellRenderer.material = mat;
    }

    public void AddOccupant(GameObject instance, CellObjectType type)
    {
        if (instance == null || type == CellObjectType.None)
            return;

        if (occupants.Any(o => o.instance == instance))
            return;

        instance.transform.SetParent(transform, false);
        instance.transform.position = transform.position;

        occupants.Add(new CellOccupant
        {
            instance = instance,
            type = type
        });

        if (type == CellObjectType.Creature || type == CellObjectType.Obstacle)
            RefreshWalkable();
    }

    public void RemoveOccupant(GameObject instance)
    {
        var occ = occupants.FirstOrDefault(o => o.instance == instance);
        if (occ == null)
            return;

        occupants.Remove(occ);
        RefreshWalkable();
    }

    public void ClearAllOccupants()
    {
        occupants.Clear();
        RefreshWalkable();
    }

    public void RefreshWalkable()
    {
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
        // Игнорируем попытку включить там, где не проходимо
        if (highlight && !IsWalkable)
            return;

        // Сбрасываем противоположный триггер и ставим нужный
        if (highlight)
        {
            outlineAnimator.ResetTrigger(DisableHash);
            outlineAnimator.SetTrigger(EnableHash);
        }
        else
        {
            outlineAnimator.ResetTrigger(EnableHash);
            outlineAnimator.SetTrigger(DisableHash);
        }
    }

    public void ResetHighlight()
    {
        outlineAnimator.ResetTrigger(EnableHash);
        outlineAnimator.ResetTrigger(DisableHash);
        // При помощи StateMachineBehaviour или AnimationEvent сразу спрячем
        // activeOutline и inactiveOutline (ниже).
    }
}
