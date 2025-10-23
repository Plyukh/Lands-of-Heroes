using UnityEngine;

/// <summary>
/// Включает булевые флаги DoubleShot/DoubleBlow в AnimatorController сразу при Apply
/// и выключает их при Remove. Поддерживает конкретный тип атаки в EffectType.
/// </summary>
public class DoubleAttackEffect : Effect
{
    public DoubleAttackEffect() : base() { }

    public override void Apply(Creature target)
    {
        if (target == null) return;
        var ac = target.Mover?.AnimatorController;
        if (ac == null) return;

        // Включаем соответствующий бул флаг в зависимости от EffectType
        switch (Data?.effectType)
        {
            case EffectType.DoubleShot:
                ac.PlayDoubleShot(); // ставит бул в true
                break;

            case EffectType.DoubleBlow:
                ac.PlayDoubleBlow(); // ставит бул в true
                break;

            default:
                // Если указан All или неизвестный, включаем оба
                ac.PlayDoubleShot();
                ac.PlayDoubleBlow();
                break;
        }
    }

    public override void Remove(Creature target)
    {
        if (target == null) return;
        var ac = target.Mover?.AnimatorController;
        if (ac == null) return;

        // Выключаем булы напрямую (предполагаем, что в контроллере есть методы/доступ)
        // Если в AnimatorController нет публичных методов для выключения, можно использовать SetBool напрямую через его API.
        // Здесь отключаем через доступ к Animator (защищаемся от null).
        var animatorField = ac.GetType().GetField("animator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (animatorField != null)
        {
            var animator = animatorField.GetValue(ac) as Animator;
            if (animator != null)
            {
                switch (Data?.effectType)
                {
                    case EffectType.DoubleShot:
                        animator.SetBool("DoubleShot", false);
                        break;
                    case EffectType.DoubleBlow:
                        animator.SetBool("DoubleBlow", false);
                        break;
                    default:
                        animator.SetBool("DoubleShot", false);
                        animator.SetBool("DoubleBlow", false);
                        break;
                }
                return;
            }
        }

        // Фоллбек: если у AnimatorController есть публичные методы для сброса, вызываем их.
        // Попробуем вызвать условные методы ResetDoubleShot/ResetDoubleBlow, если они реализованы.
        var resetShot = ac.GetType().GetMethod("ResetDoubleShot", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var resetBlow = ac.GetType().GetMethod("ResetDoubleBlow", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        if (Data?.effectType == EffectType.DoubleShot && resetShot != null)
            resetShot.Invoke(ac, null);
        else if (Data?.effectType == EffectType.DoubleBlow && resetBlow != null)
            resetBlow.Invoke(ac, null);
        else
        {
            if (resetShot != null) resetShot.Invoke(ac, null);
            if (resetBlow != null) resetBlow.Invoke(ac, null);
        }
    }
}
