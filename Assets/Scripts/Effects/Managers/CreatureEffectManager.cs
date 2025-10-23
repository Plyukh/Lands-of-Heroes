using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CreatureEffectManager : MonoBehaviour
{
    private List<Effect> activeEffects = new List<Effect>();
    private Dictionary<CreatureStatusType, float> statModifiers = new Dictionary<CreatureStatusType, float>();

    public void AddEffect(Effect effect)
    {
        if (CanStackEffect(effect))
        {
            activeEffects.Add(effect);
            effect.Apply();
        }
    }

    public void RemoveEffect(string effectName)
    {
        var effect = activeEffects.FirstOrDefault(e => e.effectName == effectName);
        if (effect != null)
        {
            effect.Remove();
            activeEffects.Remove(effect);
        }
    }

    public int GetModifiedStat(CreatureStatusType stat, int baseValue)
    {
        return statModifiers.ContainsKey(stat)
            ? Mathf.RoundToInt(baseValue * statModifiers[stat])
            : baseValue;
    }

    public bool CanStackEffect(Effect effect)
    {
        return false;
    }
}