using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class BattlefieldController : MonoBehaviour
{
    [Header("Core Controllers")]
    [Tooltip("������������ ������ �����������")]
    [SerializeField] private MovementController movementController;

    [Tooltip("������������ ������ ��� (�������� + �����)")]
    [SerializeField] private CombatController combatController;

    [Tooltip("�������������� ���� (�������, �����������, �����, ��������� ���������)")]
    [SerializeField] private BattlefieldInitializerManager initializerManager;

    private void Awake()
    {
        if (movementController == null ||
            combatController == null ||
            initializerManager == null)
        {
            Debug.LogError("[BattlefieldController] �� ��� ����������� ��������� � ����������!");
        }
    }

    public void OnCellClicked(HexCell cell)
    {
        var active = TurnOrderController.Instance.CurrentCreature;
        if (active == null || cell == null)
            return;
        if (!TurnOrderController.Instance.IsCurrentTurn(active))
            return;

        movementController.OnCellClicked(active, cell);
    }

    public void OnCreatureClicked(Creature target)
    {
        var attacker = TurnOrderController.Instance.CurrentCreature;
        if (attacker == null || target == null || attacker == target)
            return;
        if (!TurnOrderController.Instance.IsCurrentTurn(attacker))
            return;

        if (attacker.AttackType == AttackType.Ranged)
        {
            // ����������� ����� �� ���� ��� ��������
            StartCoroutine(RangedAttackCoroutine(attacker, target));
        }
        // else � melee-����� �������������� DragAttackInputController
    }
    private IEnumerator RangedAttackCoroutine(Creature attacker, Creature target)
    {
        // ��� ranged ExecuteCombat ������������� ����������� � ����� ������� PlayAttackSequence
        var task = combatController.ExecuteCombat(
            attacker,
            target,
            attacker.Mover.CurrentCell   // attackCell �� ����� ��� ranged
        );
        yield return new WaitUntil(() => task.IsCompleted);
    }
}
