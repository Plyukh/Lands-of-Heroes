using UnityEngine;

public abstract class Effect
{
    public string EffectName { get; protected set; }
    public EffectType EffectType { get; protected set; }
    public EffectData Data { get; protected set; }
    public EffectStatsPerLevel Stats { get; protected set; }
    public int Level { get; protected set; }
    public bool IsPassive { get; protected set; }
    public int Duration { get; protected set; } // 0 = infinite for passive

    public Creature Owner { get; private set; }

    protected Effect() { }

    public void Initialize(Creature owner, EffectData data, EffectStatsPerLevel stats, int level, bool isPassive, int duration)
    {
        Owner = owner;
        Data = data;
        Stats = stats;
        Level = level;
        IsPassive = isPassive;
        Duration = duration;
        EffectName = data.effectName;
        EffectType = data.effectType;
    }

    public abstract void Apply(Creature target);
    public abstract void Remove(Creature target);
}

