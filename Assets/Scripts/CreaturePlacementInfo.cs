using UnityEngine;

[System.Serializable]
public class CreaturePlacementInfo
{
    public Creature creaturePrefab;
    public int level = 1;
    public TargetSide side;
    public int row;
    public int column;
}
