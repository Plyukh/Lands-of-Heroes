using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] CreatureAnimatorController animatorController;

    [Header("Visualization")]
    [SerializeField] List<Renderer> materials;
    [SerializeField] private List<VisualizationGroup> visualGroups;

    private CreatureStatsPerLevel currentStats;

    public CreatureData CreatureData;
    public int index;

    private void Start()
    {
        Initialize(CreatureData.statsPerLevel[index]);
    }

    public void Initialize(CreatureStatsPerLevel stats)
    {
        currentStats = stats;

        ApplyTexture(stats.texture);
        ApplyVisuals(stats.visualizations);
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