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

    /// <summary>������� �������� ��������.</summary>
    public Creature CurrentCreature { get; private set; }

    /// <summary>���������� ��� ������ ���� ������ ��������.</summary>
    public event Action<Creature> OnTurnStarted;

    private void Awake()
    {
        // ��������
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // ������������� �� ������� ����� ��������
        movementController.OnMovementComplete += OnCreatureActionComplete;
        combatController.OnCombatComplete += OnCreatureActionComplete;
    }

    private void Start()
    {
        StartNewRound();
    }

    /// <summary>
    /// ��������� ������� �� ����� ����� � ��������� ������ ���.
    /// </summary>
    private void StartNewRound()
    {
        // ��� �������� �� ���� (������� ��������� � ������)
        var all = creatureManager.GetBySide(TargetSide.Any);

        // ��������� �� �������� (��������), ��� ������ � �� �������
        var ordered = all
            .OrderByDescending(c => c.GetStat(CreatureStatusType.Speed))
            .ThenBy(_ => rng.Next())
            .ToList();

        turnQueue = new Queue<Creature>(ordered);
        NextTurn();
    }

    /// <summary>
    /// ��������� � ���������� �������� � �������.
    /// </summary>
    private void NextTurn()
    {
        // ���� ������� ����� � ����� �����
        if (turnQueue.Count == 0)
        {
            StartNewRound();
            return;
        }

        // ���� ��������� �������� � ������ ��� ��������
        currentCreature = turnQueue.Dequeue();
        CurrentCreature = currentCreature;
        OnTurnStarted?.Invoke(currentCreature);

        Debug.Log($"[TurnOrder] �����: {currentCreature.name}");

        // ������������ ��� ���� ������������ ��������� ��������
        HighlightActiveCreature();
    }

    /// <summary>
    /// ����������, ����� ������� �������� ��������� ��� ��������.
    /// </summary>
    private void OnCreatureActionComplete(Creature creature)
    {
        if (creature != currentCreature)
            return;

        NextTurn();
    }

    /// <summary>
    /// ������������ reachable-������ ��� currentCreature.
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
    /// ���������, ��� ������ ����� ����.
    /// </summary>
    public bool IsCurrentTurn(Creature creature)
        => creature == currentCreature;
}
