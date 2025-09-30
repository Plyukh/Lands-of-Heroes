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

    /// <summary>
    /// Формирует очередь на новый раунд и запускает первый ход.
    /// </summary>
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

    /// <summary>
    /// Переходит к следующему существу в очереди.
    /// </summary>
    private void NextTurn()
    {
        // Если очередь пуста — новый раунд
        if (turnQueue.Count == 0)
        {
            StartNewRound();
            return;
        }

        // Берём следующее существо и делаем его активным
        currentCreature = turnQueue.Dequeue();
        CurrentCreature = currentCreature;
        OnTurnStarted?.Invoke(currentCreature);

        Debug.Log($"[TurnOrder] Ходит: {currentCreature.name}");

        // Подсвечиваем всю зону передвижения активного существа
        HighlightActiveCreature();
    }

    /// <summary>
    /// Вызывается, когда текущее существо завершило своё действие.
    /// </summary>
    private void OnCreatureActionComplete(Creature creature)
    {
        if (creature != currentCreature)
            return;

        NextTurn();
    }

    /// <summary>
    /// Подсвечивает reachable-клетки для currentCreature.
    /// </summary>
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

    /// <summary>
    /// Проверить, чьё сейчас право хода.
    /// </summary>
    public bool IsCurrentTurn(Creature creature)
        => creature == currentCreature;
}
