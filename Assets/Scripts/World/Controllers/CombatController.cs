using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PathfindingManager pathfindingManager;
    [SerializeField] private HighlightController highlightController;

    /// <summary>
    /// ����������, ����� �������� ��������� ����� (� �������� ���������).
    /// </summary>
    public event Action<Creature> OnCombatComplete;

    /// <summary>
    /// ������ ����� �� ���� ��� ������� �����.
    /// </summary>
    public async void OnCreatureClicked(Creature attacker, Creature target)
    {
        if (attacker == null || target == null || attacker == target)
            return;

        // 1) ������� �����
        if (attacker.AttackType == AttackType.Ranged)
        {
            await PlayAttackSequence(attacker, target);
            return;
        }

        // 2) ������� �����: ���� �������� � ���� ������
        var startCell = attacker.Mover.CurrentCell;
        var targetCell = target.Mover.CurrentCell;

        List<HexCell> neighborCells = pathfindingManager
            .GetReachableCells(targetCell, 1, MovementType.Teleport)
            .Where(c => c.isWalkable)
            .ToList();

        // 3) ���� ��� � �������� � ������� �����
        if (neighborCells.Contains(startCell))
        {
            await PlayAttackSequence(attacker, target);
            return;
        }

        // 4) ����� �������� � ��������� �������� ������
        int speed = attacker.GetStat(CreatureStatusType.Speed);
        var moveType = attacker.MovementType;

        List<HexCell> reachable = pathfindingManager
            .GetReachableCells(startCell, speed, moveType);

        var candidates = neighborCells.Intersect(reachable).ToList();
        if (candidates.Count == 0)
        {
            Debug.Log("CombatController: ��� ���� � ���� ��� ������� �����");
            return;
        }

        HexCell attackPos = candidates
            .OrderBy(c => Vector3.Distance(startCell.transform.position, c.transform.position))
            .First();

        highlightController.ClearHighlights();
        highlightController.HighlightReachable(reachable, startCell);
        attackPos.ShowHighlight(true);

        bool moved = false;
        if (moveType == MovementType.Teleport)
            moved = await attacker.Mover.TeleportToCell(attackPos);
        else
        {
            var path = pathfindingManager.FindPath(startCell, attackPos, moveType);
            if (path != null && path.Count > 0)
                moved = await attacker.Mover.MoveAlongPath(path);
        }

        if (!moved)
            return;

        // ��������� ��������� ����� ������������
        var newReachable = pathfindingManager
            .GetReachableCells(attacker.Mover.CurrentCell, speed, moveType);

        highlightController.ClearHighlights();
        highlightController.HighlightReachable(newReachable, attacker.Mover.CurrentCell);

        // 5) ��������� �������� �����
        await PlayAttackSequence(attacker, target);
    }

    private async Task PlayAttackSequence(Creature attacker, Creature target)
    {
        var mover = attacker.Mover;
        var anim = mover.AnimatorController;
        var attackType = attacker.AttackType;
        var hitTcs = new TaskCompletionSource<bool>();

        // A) ������������ � ����
        await mover.RotateTowardsAsync(target.transform.position);

        // B) ������� ���� � ����������
        anim.SetAttackTarget(target, attacker);

        // C) ��� ������� ������� ������ � ��������
        Action onHit = null;
        if (attackType == AttackType.Ranged)
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
