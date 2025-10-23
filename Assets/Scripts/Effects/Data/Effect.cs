using UnityEngine;

public abstract class Effect
{
    public string effectName;
    private EffectData effectData; 
    private int duration;

    public abstract void Apply();
    public abstract void Remove();
}
