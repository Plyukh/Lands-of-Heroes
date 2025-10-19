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

    public CreatureMover Mover => creatureMover;
    public CreatureKind Kind => creatureData.kind;
    public MovementType MovementType => creatureData.movementType;
    public AttackType AttackType => creatureData.attackType;
    public TargetSide Side => side;
    public GameObject Projectile => currentStats.projectilePrefab;

    public void Initialize(TargetSide side, int lvl)
    {
        this.side = side;

        currentStats = creatureData.statsPerLevel[lvl - 1];
        ApplyTexture(currentStats.texture);
        ApplyVisuals(currentStats.visualizations);
    }

    // Ќовый метод дл€ получени€ любого статуса
    public int GetStat(CreatureStatusType type)
    {
        return currentStats != null
            ? currentStats.GetStat(type)
            : 0;
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