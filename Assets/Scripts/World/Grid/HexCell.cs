using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    [Header("Grid Position")]
    public int row;
    public int column;

    [Header("Walkability")]
    [SerializeField] private bool isWalkable = true;
    public bool IsWalkable => isWalkable;

    [Header("Occupants")]
    [SerializeField] private List<CellOccupant> occupants = new List<CellOccupant>();

    [Header("Visual")]
    [SerializeField] private Renderer cellRenderer;

    [Header("Outlines & Animator")]
    [SerializeField] private Animator outlineAnimator;
    [SerializeField] private GameObject inactiveHexOutline;
    [SerializeField] private GameObject activeHexOutline;
    [SerializeField] private GameObject previewHexOutline;

    private static readonly CellObjectType[] blockingTypes =
        { CellObjectType.Creature, CellObjectType.Obstacle, CellObjectType.ForceField };

    private static readonly int EnableHash = Animator.StringToHash("Enable");
    private static readonly int DisableHash = Animator.StringToHash("Disable");

    public string CellId => $"r{row}_c{column}";

    private static readonly int TrigEnable = Animator.StringToHash("Enable");
    private static readonly int TrigDisable = Animator.StringToHash("Disable");

    // теперь public
    public void RefreshWalkable()
    {
        isWalkable = !occupants.Any(o => blockingTypes.Contains(o.type));
    }

    public void AddOccupant(GameObject go, CellObjectType type)
    {
        if (go == null || type == CellObjectType.None) return;
        if (occupants.Any(o => o.instance == go)) return;

        go.transform.SetParent(transform, false);
        go.transform.position = transform.position;
        occupants.Add(new CellOccupant { instance = go, type = type });

        if (blockingTypes.Contains(type))
            RefreshWalkable();
    }

    public void RemoveOccupant(GameObject go)
    {
        var occ = occupants.FirstOrDefault(o => o.instance == go);
        if (occ == null) return;

        occupants.Remove(occ);
        RefreshWalkable();
    }

    public void SetOutlineState(OutlineState state)
    {
        // Сбрасываем все ожидающие триггеры
        outlineAnimator.ResetTrigger(TrigEnable);
        outlineAnimator.ResetTrigger(TrigDisable);

        // Устанавливаем нужный триггер
        switch (state)
        {
            case OutlineState.Active:
                outlineAnimator.SetTrigger(TrigEnable);
                break;
            case OutlineState.Inactive:
                outlineAnimator.SetTrigger(TrigDisable);
                break;
        }
    }

    public void SetCellMaterial(Material mat)
    {
        if (cellRenderer != null && mat != null)
            cellRenderer.material = mat;
    }
}