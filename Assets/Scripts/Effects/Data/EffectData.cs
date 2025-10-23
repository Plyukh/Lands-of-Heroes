using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Status Effect")]
public class EffectData : ScriptableObject
{
    [Header("Main")]
    public string effectName;
    public EffectType effectType;
    public Sprite icon;

    [Header("Characteristics by Levels")]
    public List<EffectStatsPerLevel> levelsData;
}