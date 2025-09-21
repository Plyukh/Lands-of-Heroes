using UnityEngine;

[System.Serializable]
public class ZoneSettings
{
    public CellEffectType cellObjectType;
    public int objectCount;
    public bool isRandomPlacement;
    public bool isBlockingCell;
    public bool disappearOnTrigger;
    public bool skipTurnOnTrigger;
}