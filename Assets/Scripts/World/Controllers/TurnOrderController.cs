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

        // ������������� �� ���������� ��������
        movementController.OnMovementComplete += OnCreatureActionComplete;
        combatController.OnCombatComplete += OnCreatureActionComplete;
    }

    private void Start()
    {
        StartNewRound();
    }

    private void StartNewRound()
    {
        // ���� ���� ������� � ��������� �� ��������
        var all = creatureManager.GetBySide(TargetSide.Any);
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

        // ������� ���� �� Bravery (Living, bravery < 0)
        if (creature.Kind == CreatureKind.Living)
        {
            int bravery = creature.GetStat(CreatureStatusType.Bravery);
            if (bravery < 0 && RollChance(GetBraveryChance(bravery)))
            {
                Debug.Log($"[Bravery] {creature.name} ���������� ��� ({bravery})");
                NextTurn();
                return;
            }
        }

        // ���������� ���
        currentCreature = creature;
        CurrentCreature = creature;
        OnTurnStarted?.Invoke(creature);

        // ������������ ���� ������������ ����� HighlightController
        HighlightActiveCreature();
    }

    private void OnCreatureActionComplete(Creature creature)
    {
        if (creature != currentCreature)
            return;

        // �������������� ��� �� Bravery (Living, bravery > 0)
        if (creature.Kind == CreatureKind.Living)
        {
            int bravery = creature.GetStat(CreatureStatusType.Bravery);
            if (bravery > 0 && RollChance(GetBraveryChance(bravery)))
            {
                Debug.Log($"[Bravery] {creature.name} �������� ���. ��� ({bravery})");
                turnQueue = new Queue<Creature>(new[] { creature }.Concat(turnQueue));
            }
        }

        NextTurn();
    }

    private void HighlightActiveCreature()
    {
        var startCell = currentCreature.Mover.CurrentCell;
        int speed = currentCreature.GetStat(CreatureStatusType.Speed);
        var moveType = currentCreature.MovementType;
        var reachableCells = pathfindingManager.GetReachableCells(startCell, speed, moveType);

        // HighlightReachable ������������� ������� ������ �������� ����
        highlightController.HighlightReachable(reachableCells, startCell);
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
        => UnityEngine.Random.Range(0, 100) < percent;

    public bool IsCurrentTurn(Creature creature)
        => creature == currentCreature;
}
