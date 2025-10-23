using UnityEngine;

/// <summary>
/// �������� ������� ����� DoubleShot/DoubleBlow � AnimatorController ����� ��� Apply
/// � ��������� �� ��� Remove. ������������ ���������� ��� ����� � EffectType.
/// </summary>
public class DoubleAttackEffect : Effect
{
    public DoubleAttackEffect() : base() { }

    public override void Apply(Creature target)
    {
        if (target == null) return;
        var ac = target.Mover?.AnimatorController;
        if (ac == null) return;

        // �������� ��������������� ��� ���� � ����������� �� EffectType
        switch (Data?.effectType)
        {
            case EffectType.DoubleShot:
                ac.PlayDoubleShot(); // ������ ��� � true
                break;

            case EffectType.DoubleBlow:
                ac.PlayDoubleBlow(); // ������ ��� � true
                break;

            default:
                // ���� ������ All ��� �����������, �������� ���
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

        // ��������� ���� �������� (������������, ��� � ����������� ���� ������/������)
        // ���� � AnimatorController ��� ��������� ������� ��� ����������, ����� ������������ SetBool �������� ����� ��� API.
        // ����� ��������� ����� ������ � Animator (���������� �� null).
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

        // �������: ���� � AnimatorController ���� ��������� ������ ��� ������, �������� ��.
        // ��������� ������� �������� ������ ResetDoubleShot/ResetDoubleBlow, ���� ��� �����������.
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
