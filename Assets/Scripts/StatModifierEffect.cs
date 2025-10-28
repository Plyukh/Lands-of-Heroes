using UnityEngine;

/// <summary>
/// Effect ������� ������������ ����� ��� � ��������� (Percent) ��� � ������ (Flat).
/// �������� ����� parameterless ctor + Initialize(...). Apply/Remove ���������� CreatureEffectManager
/// ��� ����������� ����������������, ����� �������� ����������� �� ������ �� ������.
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
                    // value = 20 -> +20% (����) ��� -20% (������)
                    multiplier = ComputePercentMultiplier(Stats.value, EffectType);
                    break;

                case ValueInterpretationType.Flat:
                    // value = +5 -> ������� ���������� �� 5 ������.
                    // ������������ � �������������� ������������ �������� ����a.
                    float baseValue = target.GetBaseStat(stat);
                    if (baseValue != 0)
                        multiplier = (baseValue + Stats.value) / baseValue;
                    else
                        // ���� ������� 0 � fallback: �������������� flat ��� +value percent (safe)
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
