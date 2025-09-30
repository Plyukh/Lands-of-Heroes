using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[DisallowMultipleComponent]
public class CombatController : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("�������� ������ ���� � ���� ������������")]
    [SerializeField] private PathfindingManager pathfindingManager;
    [Tooltip("���������� ��������� ������")]
    [SerializeField] private HighlightController highlightController;

    /// <summary>
    /// �������, ����� �������� ��������� ����� � ������ ��������� ���������.
    /// </summary>
    public event Action<Creature> OnCombatComplete;

    /// <summary>
    /// ���������� ��� ����� �� ��������-����: ��������� ����� ��� ������.
    /// </summary>
    public async void OnCreatureClicked(Creature attacker, Creature target)
    {
        if (attacker == null || target == null || attacker == target)
            return;

        // ��������� ������� ��������� ��� ������ ����
        if (!TurnOrderController.Instance.IsCurrentTurn(attacker))
            return;

        var startCell = attacker.Mover.CurrentCell;
        var targetCell = target.Mover.CurrentCell;
        int speed = attacker.GetStat(CreatureStatusType.Speed);
        var moveType = attacker.MovementType;

        // 1) ������� ��� � ����� ��������
        if (attacker.AttackType == AttackType.Ranged)
        {
            await PlayAttackSequence(attacker, target);
            return;
        }

        // 2) ��� ��������: ������� ��� �������� � ���� ��������� ������
        var neighborCells = pathfindingManager
            .GetReachableCells(targetCell, 1, MovementType.Teleport)
            .Where(c => c.IsWalkable)
            .ToList();

        // 3) ���� ��� � ����� �� �������� � ����� �������
        if (neighborCells.Contains(startCell))
        {
            await PlayAttackSequence(attacker, target);
            return;
        }

        // 4) ���� �� reachable ���������� ������
        var reachable = pathfindingManager
            .GetReachableCells(startCell, speed, moveType);

        var candidates = neighborCells.Intersect(reachable).ToList();
        if (candidates.Count == 0)
        {
            Debug.Log($"[CombatController] ������ ������� � {target.name} ��� ������� �����");
            return;
        }

        // 5) �������� ��������� �������� ������
        var attackPos = candidates
            .OrderBy(c => Vector3.Distance(startCell.transform.position, c.transform.position))
            .First();

        // 6) ��������� ���� ���� � ��������� ������
        highlightController.ClearHighlights();
        highlightController.HighlightReachable(reachable, startCell);
        attackPos.ShowHighlight(true);

        // 7) ��������� ��� ��������������� �� attackPos
        bool moved = false;
        if (moveType == MovementType.Teleport)
        {
            moved = await attacker.Mover.TeleportToCell(attackPos);
        }
        else
        {
            var path = pathfindingManager.FindPath(startCell, attackPos, moveType);
            if (path != null && path.Count > 0)
                moved = await attacker.Mover.MoveAlongPath(path);
        }

        if (!moved)
            return;

        // 8) ��������� occupants ������
        startCell.RemoveOccupant(attacker.gameObject);
        attackPos.AddOccupant(attacker.gameObject, CellObjectType.Creature);

        // 9) ��������� ��������� �� ����� ������� (�����������)
        var newReachable = pathfindingManager
            .GetReachableCells(attackPos, speed, moveType);
        highlightController.ClearHighlights();
        highlightController.HighlightReachable(newReachable, attackPos);

        // 10) ��������� ���� �������� �����
        await PlayAttackSequence(attacker, target);
    }

    /// <summary>
    /// ������������, ��� ������ ��������� � �������� � ������ �������.
    /// </summary>
    private async Task PlayAttackSequence(Creature attacker, Creature target)
    {
        var mover = attacker.Mover;
        var anim = mover.AnimatorController;
        var hitTcs = new TaskCompletionSource<bool>();
        bool isRanged = attacker.AttackType == AttackType.Ranged;

        // A) ������������ � ����
        await mover.RotateTowardsAsync(target.transform.position);

        // B) ������� ��������
        anim.SetAttackTarget(target, attacker);

        // C) ��� ������� ������� ��� ����������
        Action onHit = null;
        if (isRanged)
        {
            onHit = () =>
            {
                anim.OnAttackHit -= onHit;
                hitTcs.TrySetResult(true);
            };
            anim.OnAttackHit += onHit;
            anim.PlayAttack();
        }
        else
        {
            onHit = () =>
            {
                anim.OnMeleeAttackHit -= onHit;
                hitTcs.TrySetResult(true);
            };
            anim.OnMeleeAttackHit += onHit;
            anim.PlayMeleeAttack();
        }

        await hitTcs.Task;

        OnCombatComplete?.Invoke(attacker);
    }
}
