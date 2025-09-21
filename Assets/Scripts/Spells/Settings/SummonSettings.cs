using UnityEngine;

[System.Serializable]
public class SummonSettings
{
    public CreatureData creatureData;

    [Tooltip("Level of the summoned creature. The number of summoned units is calculated by multiplying the creature's level by the relevant player attribute.")]
    public int creatureLevel;
}