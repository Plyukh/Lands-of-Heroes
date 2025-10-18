using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class CreatureManager : MonoBehaviour
{
    public static CreatureManager Instance { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private HexGridManager gridManager;

    [Header("Creature Spawn Configuration")]
    [Tooltip("Список существ, которые будут созданы в начале боя")]
    [SerializeField] private List<CreaturePlacementInfo> creaturePlacements = new List<CreaturePlacementInfo>();

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

        // Создаем и размещаем существ на сцене
        SpawnCreatures();
    }

    /// <summary>
    /// Создает существ на основе списка creaturePlacements и размещает их на поле.
    /// </summary>
    private void SpawnCreatures()
    {
        allCreatures.Clear();

        if (gridManager == null)
        {
            Debug.LogError("[CreatureManager] HexGridManager не назначен! Невозможно разместить существ.");
            return;
        }

        foreach (var placement in creaturePlacements)
        {
            if (placement.creaturePrefab == null)
            {
                Debug.LogWarning("[CreatureManager] В одном из слотов не назначен префаб существа. Пропускаем.");
                continue;
            }

            // Находим клетку по координатам
            string cellId = $"r{placement.row}_c{placement.column}";
            if (!gridManager.CellMap.TryGetValue(cellId, out var cell))
            {
                Debug.LogWarning($"[CreatureManager] Клетка с координатами ({placement.row}, {placement.column}) не найдена. Существо не будет создано.");
                continue;
            }

            // Создаем экземпляр существа
            Creature newCreature = Instantiate(placement.creaturePrefab);

            // Инициализируем существо
            newCreature.Initialize(placement.side, placement.level);

            // Размещаем существо на клетке
            if (newCreature.Side == TargetSide.Ally)
            {
                newCreature.Mover.SetCurrentCell(cell, Quaternion.Euler(180, 270, 270));
            }
            else if(newCreature.Side == TargetSide.Enemy)
            {
                newCreature.Mover.SetCurrentCell(cell, Quaternion.Euler(0,270,270));
            }

            cell.AddOccupant(newCreature.gameObject, CellObjectType.Creature);

            allCreatures.Add(newCreature);
        }
    }

    /// <summary>
    /// Находит все Creature в сцене и обновляет список.
    /// Этот метод больше не нужен для инициализации, но может быть полезен для отладки.
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