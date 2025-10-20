using System;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class BattlefieldController : MonoBehaviour
{
    [Header("Core Controllers")]
    [Tooltip("������������ ������ �����������")]
    [SerializeField] private MovementController movementController;
    [Tooltip("������������ ������ ���")]
    [SerializeField] private CombatController combatController;
    [Tooltip("�������� ������ ���� � ���� ������������")]
    [SerializeField] private PathfindingManager pathfindingManager;
    [Tooltip("�������������� ���� (�������, �����������, �����, ��������� ���������)")]
    [SerializeField] private BattlefieldInitializerManager initializerManager;

    public event Action<Creature> OnActionComplete;

    public void OnCellClicked(HexCell cell)
    {
        var active = TurnOrderController.Instance.CurrentCreature;
        if (active == null || cell == null)
            return;

        // ����� ������ � ���� ���
        if (!TurnOrderController.Instance.IsCurrentTurn(active))
            return;

        var start = active.Mover.CurrentCell;
        int speed = active.GetStat(CreatureStatusType.Speed);
        var type = active.MovementType;
        var reachable = pathfindingManager
            .GetReachableCells(start, speed, type)
            .ToHashSet();

        // ���� ���� ��� ������������ � ������ �� ������
        if (!reachable.Contains(cell))
            return;

        // ������ ������� � ��������� ��������
        var path = pathfindingManager.FindPath(start, cell, type);
        if (path == null || path.Count == 0)
            return;

        movementController.MoveAlongPath(active, path);
    }

    /// <summary>
    /// ���������� ��� ����� �� ���������� ��������.
    /// ���������� ��������� ����� (������� ��� ������� ���).
    /// </summary>
    public void OnCreatureClicked(Creature target)
    {
        var active = TurnOrderController.Instance.CurrentCreature;
        if (active == null || target == null || active == target)
            return;

        // ������� ������ � ���� ���
        if (!TurnOrderController.Instance.IsCurrentTurn(active))
            return;

           // 1) ���� ��� ����� �� ������ ��������
        AttackType selected = active.AttackType;

           // 3) �������� � ������ ���������� ����
        combatController.OnCreatureClicked(active, target, selected);
    }

    public void OnDefendAction(Creature creature)
    {
        creature.IsDefending = true;
        OnActionComplete?.Invoke(creature);
    }

    public void OnWaitAction(Creature creature)
    {
        OnActionComplete?.Invoke(creature);
    }
}
