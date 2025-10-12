using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class BattlefieldController : MonoBehaviour
{
    [Header("Core Controllers")]
    [Tooltip("Обрабатывает логику перемещения")]
    [SerializeField] private MovementController movementController;

    [Tooltip("Обрабатывает логику боя (движение + атака)")]
    [SerializeField] private CombatController combatController;

    [Tooltip("Инициализирует поле (фракции, препятствия, декор, начальную подсветку)")]
    [SerializeField] private BattlefieldInitializerManager initializerManager;

    private void Awake()
    {
        if (movementController == null ||
            combatController == null ||
            initializerManager == null)
        {
            Debug.LogError("[BattlefieldController] Не все контроллеры назначены в инспекторе!");
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
            // Немедленная атака по цели без движения
            StartCoroutine(RangedAttackCoroutine(attacker, target));
        }
        // else — melee-атака обрабатывается DragAttackInputController
    }
    private IEnumerator RangedAttackCoroutine(Creature attacker, Creature target)
    {
        // Для ranged ExecuteCombat проигнорирует перемещение и сразу вызовет PlayAttackSequence
        var task = combatController.ExecuteCombat(
            attacker,
            target,
            attacker.Mover.CurrentCell   // attackCell не важна для ranged
        );
        yield return new WaitUntil(() => task.IsCompleted);
    }
}
