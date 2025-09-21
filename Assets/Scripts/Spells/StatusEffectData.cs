using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatusEffectData
{
    [Header("Main")]
    public string effectName;
    public EffectType effectType;

    [Header("Status Modification")]
    public List<CreatureStatusType> statusTarget;
    public ValueInterpretationType valueType;
    public float value;
    public int duration;

    [Header("Stacking & Dispel")]
    public bool isStackable;
    public bool isDispellable;

    [Header("Magic Protection")]
    public ElementType protectedElement;
    [Range(0, 100)]
    public int magicProtectionPercent;

    [Header("Physical Protection")]
    public AttackType protectionType;
    [Range(0, 100)]
    public int physicalProtectionPercent;
}