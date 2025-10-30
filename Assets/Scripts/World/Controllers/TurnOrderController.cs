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
    [SerializeField] private BattlefieldController battlefieldController;

    private readonly System.Random rng = new System.Random();
    private Queue<Creature> turnQueue;
    private Creature currentCreature;

    /// <summary>������� �������� ��������.</summary>
    public Creature CurrentCreature { get; private set; }

    /// <summary>������� ������� �����. ������ ��� ������.</summary>
    public Queue<Creature> TurnQueue => turnQueue;

    /// <summary>���������� ��� ������ ���� ������ ��������.</summary>
    public event Action<Creature> OnTurnStarted;
    
    /// <summary>���������� ��� ������ ���� ������ ���� (��� ��������� ����������� �� �������)</summary>
    public event Action<Creature> OnTurnStart;

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
        battlefieldController.OnActionComplete += OnCreatureActionComplete;
    }

    private void OnDestroy() // ������� �������� - ������������ �� �������
    {
        if (movementController != null)
            movementController.OnMovementComplete -= OnCreatureActionComplete;
        if (combatController != null)
            combatController.OnCombatComplete -= OnCreatureActionComplete;
        if (battlefieldController != null)
            battlefieldController.OnActionComplete -= OnCreatureActionComplete;
    }

    private void Start()
    {
        StartNewRound();
    }

    private void StartNewRound()
    {
        // ��� �������� �� ���� (������� ��������� � ������)
        var all = creatureManager.GetBySide(TargetSide.Any);

        // Восстанавливаем контратаки у всех существ в начале раунда
        // Боеприпасы НЕ восстанавливаются - это ресурс на всю игровую сессию!
        foreach (var creature in all)
        {
            creature.RefreshCounterattacks();
        }

        // ��������� �� �������� (��������), ��� ������ � �� �������
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

        // ������� ������ ������ � ������ ������ ����
        if (creature.IsDefending)
        {
            creature.IsDefending = false;
        }

        // 1) ������� ���� �� Bravery (������ ��� Living)
        if (creature.Kind == CreatureKind.Living)
        {
            int bravery = creature.GetStat(CreatureStatusType.Bravery);
            if (bravery < 0 && RollChance(GetBraveryChance(bravery)))
            {
                Debug.Log($"[Bravery] {creature.name} ���������� ��� ({bravery})");
                NextTurn(); // ����� ��������� � ���������� ��������
                return;
            }
        }

        // 2) ����������� ��������� ����
        currentCreature = creature;
        CurrentCreature = creature;
        OnTurnStarted?.Invoke(creature);
        OnTurnStart?.Invoke(creature);

        // 3) ��������� reachable
        HighlightActiveCreature();
    }

    private void OnCreatureActionComplete(Creature creature)
    {
        if (creature != currentCreature)
            return;

        // Если движение было для атаки, не завершаем ход (атака ещё не произошла)
        if (movementController.IsMovementForAttack)
        {
            return;
        }

        // �������������� ��� �� Bravery (������ ��� Living)
        if (creature.Kind == CreatureKind.Living)
        {
            int bravery = creature.GetStat(CreatureStatusType.Bravery);
            if (bravery > 0 && RollChance(GetBraveryChance(bravery)))
            {
                Debug.Log($"[Bravery] {creature.name} �������� ���. ��� ({bravery})");
                // ��������� �������� � ������ �������
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
