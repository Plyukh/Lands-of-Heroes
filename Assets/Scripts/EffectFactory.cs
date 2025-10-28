using System;
using UnityEngine;

public static class EffectFactory
{
    public static Effect CreatePassiveEffect(Creature owner, EffectData data, int level)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        var stats = data.levelsData.Find(s => s.level == level) ?? (data.levelsData.Count > 0 ? data.levelsData[0] : null);

        Effect effect;
        switch (data.effectType)
        {
            case EffectType.DoubleShot:
            case EffectType.DoubleBlow:
                effect = new DoubleAttackEffect();
                break;

            case EffectType.Buff:
            case EffectType.Debuff:
                effect = new StatModifierEffect();
                break;

            case EffectType.AreaStrike:
            case EffectType.PiercingStrike:
                effect = new StrikeEffect();
                break;

            case EffectType.UnansweredStrike:
                effect = new UnansweredStrikeEffect();
                break;

            case EffectType.ExplosiveShot:
            case EffectType.ExplosiveShotLiving:
                effect = new ExplosiveShotEffect();
                break;

            // ������ ������������������ ���� �������� ���� �� ���� �������������:
            // case EffectType.Regeneration: effect = new RegenerationEffect(); break;
            // case EffectType.Vampirism:    effect = new VampirismEffect();    break;

            default:
                effect = new StatModifierEffect();
                break;
        }

        effect.Initialize(owner, data, stats, level, isPassive: true, duration: 0);
        return effect;
    }
}
