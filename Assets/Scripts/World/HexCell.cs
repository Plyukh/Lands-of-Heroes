using UnityEngine;

public class HexCell : MonoBehaviour
{
    [Header("Main")]
    public int row;
    public int column;

    [Header("Cell State")]
    public CellOccupantType occupantType = CellOccupantType.None;
    public GameObject occupantObject;

    [Header("Spell Effect")]
    public CellEffectType activeEffect = CellEffectType.None;

    [Header("Highlight")]
    [SerializeField] private GameObject highlightObject;

    [Header("Visuals")]
    [SerializeField] private Renderer cellRenderer;

    public void SetOccupant(CellOccupantType type, GameObject obj = null)
    {
        occupantType = type;
        occupantObject = obj;
    }

    public void ClearOccupant()
    {
        occupantType = CellOccupantType.None;
        occupantObject = null;
    }

    public void SetEffect(CellEffectType effect)
    {
        activeEffect = effect;
    }

    public void ClearEffect()
    {
        activeEffect = CellEffectType.None;
    }

    public bool HasEffect(CellEffectType effect)
    {
        return activeEffect == effect;
    }

    public void ToggleHighlight(bool isActive)
    {
        if (highlightObject != null)
            highlightObject.SetActive(isActive);
    }

    public void SetMaterial(Material mat)
    {
        if (cellRenderer != null && mat != null)
        {
            cellRenderer.material = mat;
        }
    }
}
