using UnityEngine;

[System.Serializable]
public class CellOccupant
{
    public CellObjectType type = CellObjectType.None;
    public GameObject instance;

    public bool IsValid => instance != null && type != CellObjectType.None;

    public void Clear()
    {
        if (instance != null)
            Object.Destroy(instance);

        instance = null;
        type = CellObjectType.None;
    }
}