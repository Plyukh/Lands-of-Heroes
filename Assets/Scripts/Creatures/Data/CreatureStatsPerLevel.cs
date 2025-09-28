using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CreatureStatsPerLevel
{
    [Header("Main")]
    public int level;

    [Header("Characteristics")]
    public int attack;
    public int defense;
    public int minDamage;
    public int maxDamage;
    public int health;
    public int speed;
    public int stackSize;

    [Header("Range Combat")]

    [Tooltip("It only works if the type of attack is ranged combat")]
    public int shots;
    [Tooltip("It only works if the type of attack is ranged combat")]
    public GameObject projectilePrefab;

    [Header("Visualization")]
    public Texture texture;
    public List<Visualization> visualizations;

    public int GetStat(CreatureStatusType type)
    {
        switch (type)
        {
            case CreatureStatusType.Attack: return attack;
            case CreatureStatusType.Defense: return defense;
            case CreatureStatusType.MinDamage: return minDamage;
            case CreatureStatusType.MaxDamage: return maxDamage;
            case CreatureStatusType.Health: return health;
            case CreatureStatusType.Speed: return speed;
            case CreatureStatusType.StackSize: return stackSize;
            case CreatureStatusType.Shots: return shots;
            default: return 0;
        }
    }
}