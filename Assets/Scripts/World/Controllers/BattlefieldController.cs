using System.IO;
using System.Linq;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

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

    private void Awake()
    {
        if (movementController == null ||
            combatController == null ||
            pathfindingManager == null ||
            initializerManager == null)
        {
            Debug.LogError("[BattlefieldController] �� ��� ����������� ��������� � ����������!");
        }
    }

    /// <summary>
    /// ���������� ��� ����� �� ������ ������.
    /// �������� ���� � ��������� ����������� activeCreature.
    /// </summary>
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
}
