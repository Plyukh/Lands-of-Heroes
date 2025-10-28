using NUnit.Framework.Internal;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour
{
    [Header("Mover")]
    [SerializeField] CreatureMover creatureMover;

    [Header("Visualization")]
    [SerializeField] List<Renderer> materials;
    [SerializeField] private List<VisualizationGroup> visualGroups;

    [Header("Stats")]
    [SerializeField] CreatureData creatureData;

    private CreatureStatsPerLevel currentStats;
    private TargetSide side;

    [Header("Effect Manager")]
    [SerializeField] private CreatureEffectManager effectManager;

    public CreatureMover Mover => creatureMover;
    public CreatureEffectManager EffectManager => effectManager;
    public CreatureKind Kind => creatureData.kind;
    public MovementType MovementType => creatureData.movementType;
    public AttackType AttackType => creatureData.attackType;
    public TargetSide Side => side;
    public GameObject Projectile => currentStats.projectilePrefab;

    public bool IsDefending { get; set; } = false;

    public void Initialize(TargetSide side, int lvl)
    {
        this.side = side;

        ApplyStats(lvl);
        ApplyPassiveEffects();
        ApplyTexture(currentStats.texture);
        ApplyVisuals(currentStats.visualizations);
    }

    /// <summary>
    /// Получает итоговое значение характеристики с учетом всех эффектов
    /// </summary>
    public int GetStat(CreatureStatusType type)
    {
        if (currentStats == null)
            return 0;

        // Получаем базовое значение
        int baseValue = currentStats.GetStat(type);
        
        // Применяем модификаторы от эффектов
        return effectManager.GetModifiedStat(type, baseValue);
    }

    /// <summary>
    /// Получает базовое значение характеристики БЕЗ учета эффектов
    /// </summary>
    public int GetBaseStat(CreatureStatusType type)
    {
        return currentStats != null
            ? currentStats.GetStat(type)
            : 0;
    }

    public void ApplyStats(int lvl)
    {
        currentStats = creatureData.statsPerLevel[lvl - 1];
    }

    public void ApplyPassiveEffects()
    {
        foreach (var p in currentStats.passiveEffects)
        {
            if (p.effectData == null) continue;
            var effect = EffectFactory.CreatePassiveEffect(this, p.effectData, p.level);
            effectManager.AddEffect(effect);
        }
    }

    private void ApplyTexture(Texture texture)
    {
        foreach (Renderer renderer in materials)
        {
            if (renderer.material && texture != null)
            {
                foreach (var material in renderer.materials)
                {
                    material.mainTexture = texture;
                }
            }
        }
    }

    private void ApplyVisuals(List<Visualization> visualizations)
    {
        foreach (var group in visualGroups)
        {
            foreach (var variation in group.variations)
            {
                foreach (var obj in variation.objects)
                    obj.SetActive(false);
            }
        }

        foreach (var visual in visualizations)
        {
            foreach(var group in visualGroups)
            {
                if(visual.type == group.type)
                {
                    foreach (var variation in group.variations)
                    {
                        variation.objects[visual.index].SetActive(true);
                    }
                }
            }
        }
    }
}