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
    
    // Система контратак - количество оставшихся контратак в текущем ходу
    private int remainingCounterattacks;
    public int RemainingCounterattacks => remainingCounterattacks;

    // Система боеприпасов для дальнего боя
    private int remainingShots;
    private int maxShots;
    public int RemainingShots => remainingShots;
    public int MaxShots => maxShots;

    public void Initialize(TargetSide side, int lvl)
    {
        this.side = side;

        ApplyStats(lvl);
        ApplyPassiveEffects();
        ApplyTexture(currentStats.texture);
        ApplyVisuals(currentStats.visualizations);

        // Инициализация боеприпасов для дальников
        if (AttackType == AttackType.Ranged)
        {
            maxShots = GetStat(CreatureStatusType.Shots);
            remainingShots = maxShots;
        }
        else
        {
            maxShots = 0;
            remainingShots = 0;
        }
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

    /// <summary>
    /// Восстанавливает контратаки в начале хода существа
    /// </summary>
    public void RefreshCounterattacks()
    {
        remainingCounterattacks = GetStat(CreatureStatusType.Counterattack);
    }

    /// <summary>
    /// Использует одну контратаку. Возвращает true если контратака доступна
    /// </summary>
    public bool UseCounterattack()
    {
        if (remainingCounterattacks > 0)
        {
            remainingCounterattacks--;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Проверяет, может ли существо контратаковать
    /// </summary>
    public bool CanCounterattack()
    {
        return remainingCounterattacks > 0;
    }

    /// <summary>
    /// Проверяет, может ли существо стрелять (есть ли боеприпасы)
    /// </summary>
    public bool CanShoot()
    {
        // Существа ближнего боя всегда могут атаковать
        if (AttackType == AttackType.Melee)
            return true;
        
        return remainingShots > 0;
    }

    /// <summary>
    /// Использует один выстрел. Возвращает true если выстрел доступен
    /// </summary>
    public bool UseShot()
    {
        // Существа ближнего боя не тратят выстрелы
        if (AttackType == AttackType.Melee)
            return true;

        if (remainingShots > 0)
        {
            remainingShots--;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Восстанавливает боеприпасы в начале нового раунда
    /// </summary>
    public void RefreshShots()
    {
        if (AttackType == AttackType.Ranged)
        {
            remainingShots = maxShots;
        }
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