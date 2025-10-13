using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class TurnOrderController : MonoBehaviour
{
    public static TurnOrderController Instance { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private CreatureManager creatureManager;
    [SerializeField] private PathfindingManager pathfindingManager;
    [SerializeField] private MovementController movementController;
    [SerializeField] private CombatController combatController;
    [SerializeField] private HighlightController highlightController;

    private readonly System.Random rng = new System.Random();
    private Queue<Creature> turnQueue;
    private Creature currentCreature;

    /// <summary>Текущее активное существо.</summary>
    public Creature CurrentCreature { get; private set; }

    /// <summary>Вызывается при начале хода нового существа.</summary>
    public event Action<Creature> OnTurnStarted;

    private void Awake()
    {
        // Синглтон
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // Подписываемся на события конца действия
        movementController.OnMovementComplete += OnCreatureActionComplete;
        combatController.OnCombatComplete += OnCreatureActionComplete;
    }

    private void Start()
    {
        StartNewRound();
    }

    private void StartNewRound()
    {
        // Все существа на поле (включая союзников и врагов)
        var all = creatureManager.GetBySide(TargetSide.Any);

        // Сортируем по скорости (убывание), при равной — по рандому
        var ordered = all
            .OrderByDescending(c => c.GetStat(CreatureStatusType.Speed))
            .ThenBy(_ => rng.Next())
            .ToList();

        turnQueue = new Queue<Creature>(ordered);
        NextTurn();
    }

    private void NextTurn()
    {
        if (turnQueue.Count == 0)
        {
            StartNewRound();
            return;
        }

        var creature = turnQueue.Dequeue();

        // 1) Пропуск хода по Bravery (только для Living)
        if (creature.Kind == CreatureKind.Living)
        {
            int bravery = creature.GetStat(CreatureStatusType.Bravery);
            if (bravery < 0 && RollChance(GetBraveryChance(bravery)))
            {
                Debug.Log($"[Bravery] {creature.name} пропускает ход ({bravery})");
                NextTurn(); // сразу переходим к следующему существу
                return;
            }
        }

        // 2) Стандартная активация хода
        currentCreature = creature;
        CurrentCreature = creature;
        OnTurnStarted?.Invoke(creature);

        // 3) Подсветка reachable
        HighlightActiveCreature();
    }

    private void OnCreatureActionComplete(Creature creature)
    {
        if (creature != currentCreature)
            return;

        // Дополнительный ход по Bravery (только для Living)
        if (creature.Kind == CreatureKind.Living)
        {
            int bravery = creature.GetStat(CreatureStatusType.Bravery);
            if (bravery > 0 && RollChance(GetBraveryChance(bravery)))
            {
                Debug.Log($"[Bravery] {creature.name} получает доп. ход ({bravery})");
                // Вставляем существо в начало очереди
                turnQueue = new Queue<Creature>(new[] { creature }.Concat(turnQueue));
            }
        }

        NextTurn();
    }

    private void HighlightActiveCreature()
    {
        highlightController.ClearHighlights();

        var startCell = currentCreature.Mover.CurrentCell;
        int speed = currentCreature.GetStat(CreatureStatusType.Speed);
        var moveType = currentCreature.MovementType;

        var reachable = pathfindingManager
            .GetReachableCells(startCell, speed, moveType);

        highlightController.HighlightReachable(reachable, startCell);
    }

    private int GetBraveryChance(int bravery)
    {
        switch (Mathf.Abs(bravery))
        {
            case 1: return 5;
            case 2: return 10;
            case 3: return 25;
            default: return 0;
        }
    }

    private bool RollChance(int percent)
    {
        return UnityEngine.Random.Range(0, 100) < percent;
    }

    public bool IsCurrentTurn(Creature creature)
        => creature == currentCreature;
}
