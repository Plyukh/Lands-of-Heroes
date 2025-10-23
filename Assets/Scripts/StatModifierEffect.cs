using UnityEngine;

/// <summary>
/// Effect который модифицирует статы как в процентах (Percent) так и плоско (Flat).
/// Создаётся через parameterless ctor + Initialize(...). Apply/Remove используют CreatureEffectManager
/// для регистрации мультипликаторов, чтобы удаление происходило по ссылке на эффект.
/// </summary>
public class StatModifierEffect : Effect
{
    public StatModifierEffect() : base() { }

    public override void Apply(Creature target)
    {
        if (target == null || Stats == null || target.EffectManager == null) return;

        foreach (var stat in Stats.statusTarget)
        {
            float multiplier = 1f;

            switch (Stats.valueType)
            {
                case ValueInterpretationType.Percent:
                    // value = 20 -> +20% (бафф) или -20% (дебафф)
                    multiplier = ComputePercentMultiplier(Stats.value, EffectType);
                    break;

                case ValueInterpretationType.Flat:
                    // value = +5 -> плоское увеличение на 5 единиц.
                    // Конвертируем в мультипликатор относительно базового статa.
                    float baseValue = target.GetStat(stat);
                    if (baseValue != 0)
                        multiplier = (baseValue + Stats.value) / baseValue;
                    else
                        // если базовый 0 — fallback: интерпретируем flat как +value percent (safe)
                        multiplier = 1f + (Stats.value / 100f);
                    break;
            }

            target.EffectManager.AddStatModifier(stat, multiplier, this);
        }
    }

    public override void Remove(Creature target)
    {
        if (target == null || Stats == null || target.EffectManager == null) return;

        foreach (var stat in Stats.statusTarget)
            target.EffectManager.RemoveStatModifier(stat, this);
    }

    private float ComputePercentMultiplier(float rawValue, EffectType effectType)
    {
        bool isDebuff = effectType == EffectType.Debuff
                        || effectType == EffectType.DamageOverTime
                        || effectType == EffectType.Madness;
        float percent = rawValue / 100f;
        float result = isDebuff ? 1f - percent : 1f + percent;
        return Mathf.Max(0.01f, result);
    }
}
