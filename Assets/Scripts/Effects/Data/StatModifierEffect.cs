using UnityEngine;

public class StatModifierEffect : Effect
{
    public CreatureStatusType targetStat;
    public float modifier;
    public ValueInterpretationType valueType;

    public override void Apply()
    {
        // Модифицировать стат существа
        //target.GetEffectManager().ModifyStat(targetStat, modifier, valueType);
    }

    public override void Remove()
    {
        // Вернуть стат к исходному значению
        //target.GetEffectManager().RestoreStat(targetStat, modifier, valueType);
    }
}