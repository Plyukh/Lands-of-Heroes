using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpellStatsPerLevel
{
    [Header("Main")]
    public int level;

    [Header("Characteristics")]
    public int manaCost;
    public TargetSide targetSide;
    public TargetType targetType;
    public int targetCount;
    public int areaRadius;

    [Header("Offensive & Restoration Characteristics")]

    [Tooltip("Damage or resurrection modifier: multiplied by the relevant player attribute")]
    public int power;
    [Tooltip("Bonus to total damage or restored health: added after applying the power modifier")]
    public int damage;

    [Header("Creature")]
    public SummonSettings summonSettings;

    [Header("Clone")]
    public CloneSettings cloneSettings;

    [Header("Zone")]
    public ZoneSettings zoneSettings;

    [Header("Clearance")]
    public ClearanceSettings clearanceSettings;

    [Header("Effects")]
    public List<StatusEffectData> statusEffects;
}