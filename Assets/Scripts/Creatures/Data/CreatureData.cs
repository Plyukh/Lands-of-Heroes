using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Creature")]
public class CreatureData : ScriptableObject
{
    [Header("Main")]
    public string id;
    public string creatureName;
    public Rarity rarity;
    public Faction faction;
    public CreatureKind kind;
    public AttackType attackType;
    public MovementType movementType;
    public Sprite sprite;
    public Sprite backgroundSprite;
    public Vector2 outlineIconSize;
    public GameObject prefab;

    [Header("Persistence")]
    public bool persistInPlayerData = true;

    [Header("Characteristics by Levels")]
    public List<CreatureStatsPerLevel> statsPerLevel;
}