using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class CreatureManager : MonoBehaviour
{
    public static CreatureManager Instance { get; private set; }

    [Header("All Creatures on the Battlefield")]
    [Tooltip("Сюда автоматически попадут все Creature на сцене")]
    [SerializeField]
    private List<Creature> allCreatures = new List<Creature>();

    private void Awake()
    {
        // Синглтон
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // Собираем всех Creature, которые уже есть на сцене
        RefreshCreaturesList();
    }

    /// <summary>
    /// Находит все Creature в сцене и обновляет список.
    /// Вызовите при спавне/удалении существ, если это динамично.
    /// </summary>
    public void RefreshCreaturesList()
    {
        allCreatures = FindObjectsOfType<Creature>().ToList();
    }

    /// <summary>
    /// Возвращает всех существ указанной стороны.
    /// Если side == Any — возвращает копию полного списка.
    /// </summary>
    public List<Creature> GetBySide(TargetSide side)
    {
        if (side == TargetSide.Any)
            return new List<Creature>(allCreatures);

        return allCreatures
            .Where(c => c.Side == side)
            .ToList();
    }

    /// <summary>
    /// Удобные методы
    /// </summary>
    public List<Creature> GetAllies() => GetBySide(TargetSide.Ally);
    public List<Creature> GetEnemies() => GetBySide(TargetSide.Enemy);
}
