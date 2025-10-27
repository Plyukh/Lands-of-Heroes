using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CreatureEffectManager : MonoBehaviour
{
    private List<Effect> activeEffects = new List<Effect>();
    // stat -> list of (effect, multiplier)
    private Dictionary<CreatureStatusType, List<(Effect effect, float multiplier)>> statModifiers = new Dictionary<CreatureStatusType, List<(Effect, float)>>();

    public void AddEffect(Effect effect)
    {
        if (!CanStackEffect(effect)) return;

        activeEffects.Add(effect);
        effect.Apply(effect.Owner);
    }

    public void RemoveEffect(string effectName)
    {
        var effect = activeEffects.FirstOrDefault(e => e.EffectName == effectName);
        if (effect != null)
        {
            effect.Remove(effect.Owner);
            activeEffects.Remove(effect);
        }
    }

    public int GetModifiedStat(CreatureStatusType stat, int baseValue)
    {
        if (!statModifiers.TryGetValue(stat, out var list) || list.Count == 0) return baseValue;
        float total = 1f;
        foreach (var kv in list) total *= kv.multiplier;
        return Mathf.RoundToInt(baseValue * total);
    }

    public bool CanStackEffect(Effect effect)
    {
        var stats = effect.Stats;
        if (stats == null) return true;
        if (stats.isStackable) return true;
        return !activeEffects.Any(e => e.Data == effect.Data && e.Level == effect.Level);
    }

    public void AddStatModifier(CreatureStatusType stat, float multiplier, Effect source)
    {
        if (!statModifiers.TryGetValue(stat, out var list))
        {
            list = new List<(Effect, float)>();
            statModifiers[stat] = list;
        }
        list.Add((source, multiplier));
    }

    public void RemoveStatModifier(CreatureStatusType stat, Effect source)
    {
        if (!statModifiers.TryGetValue(stat, out var list)) return;
        list.RemoveAll(t => t.effect == source);
        if (list.Count == 0) statModifiers.Remove(stat);
    }

    public void RemoveAllEffect(Effect effect)
    {
        effect.Remove(effect.Owner);
        activeEffects.Remove(effect);
    }

    /// <summary>
    /// Проверяет, есть ли у существа эффект указанного типа
    /// </summary>
    public bool HasEffectOfType(EffectType effectType)
    {
        return activeEffects.Any(e => e.Data?.effectType == effectType);
    }
}