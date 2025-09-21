using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Spell")]
public class SpellData : ScriptableObject
{
    [Header("Main")]
    public string id;
    public string spellName;
    public Rarity rarity;
    public Faction faction;
    public ElementType element;
    public SpellType spellType;
    public CreatureKind targetKind;
    public GameObject prefab;

    [Header("Characteristics by Levels")]
    public List<SpellStatsPerLevel> statsPerLevel;
}