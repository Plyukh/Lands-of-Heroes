using UnityEngine;

public class StatModifierEffect : Effect
{
    public CreatureStatusType targetStat;
    public float modifier;
    public ValueInterpretationType valueType;

    public override void Apply()
    {
        // �������������� ���� ��������
        //target.GetEffectManager().ModifyStat(targetStat, modifier, valueType);
    }

    public override void Remove()
    {
        // ������� ���� � ��������� ��������
        //target.GetEffectManager().RestoreStat(targetStat, modifier, valueType);
    }
}