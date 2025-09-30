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
    public Creature CurrentCreature { get; private set; }

    public event Action<Creature> OnTurnStarted;
    private void Awake()
    {
        // Синглтон
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Подписываемся на сигналы об окончании действия
        movementController.OnMovementComplete += OnCreatureActionComplete;
        combatController.OnCombatComplete += OnCreatureActionComplete;
    }

    private void Start()
    {
        StartNewRound();
    }

    private void StartNewRound()
    {
        // Берём ВСЕХ существ на поле
        List<Creature> all = creatureManager.GetBySide(TargetSide.Any);

        // Сортируем по убыванию скорости, при равенстве — рандом
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

        currentCreature = turnQueue.Dequeue();
        CurrentCreature = currentCreature;
        OnTurnStarted?.Invoke(currentCreature);

        currentCreature = turnQueue.Dequeue();
        HighlightCurrentCreature();

        // Разрешаем инпут только для currentCreature
        highlightController.ClearHighlights();
        highlightController.HighlightReachable(
            pathfindingManager.GetReachableCells(
                    currentCreature.Mover.CurrentCell,
                    currentCreature.GetStat(CreatureStatusType.Speed),
                    currentCreature.MovementType),
            currentCreature.Mover.CurrentCell);
    }

    private void OnCreatureActionComplete(Creature creature)
    {
        // Игнорируем чужие сигналы
        if (creature != currentCreature) return;

        NextTurn();
    }

    private void HighlightCurrentCreature()
    {
        // Здесь вы можете, например, выделить модель или UI над головой
        // simplest: вспомогательный метод в Creature
        //currentCreature.ShowAsActive(true);
    }

    /// <summary>
    /// Позволяет другим контроллерам проверить,
    /// чья сейчас очередь хода.
    /// </summary>
    public bool IsCurrentTurn(Creature creature)
        => creature == currentCreature;
}
