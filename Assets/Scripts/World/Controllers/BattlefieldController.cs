using UnityEngine;

[DisallowMultipleComponent]
public class BattlefieldController : MonoBehaviour
{
    [Header("Core Controllers")]
    [Tooltip("Обрабатывает логику перемещения")]
    [SerializeField] private MovementController movementController;

    [Tooltip("Обрабатывает логику боя")]
    [SerializeField] private CombatController combatController;

    [Tooltip("Инициализирует поле (фракции, препятствия, декор, начальную подсветку)")]
    [SerializeField] private BattlefieldInitializerManager initializerManager;

    private void Awake()
    {
        // Валидация обязательных зависимостей
        if (movementController == null ||
            combatController == null ||
            initializerManager == null)
        {
            Debug.LogError("[BattlefieldController] Не все контроллеры назначены в инспекторе!");
        }
    }

    private void Start()
    {
        // Подготовка поля перед первым ходом
        initializerManager.InitializeField();
    }

    public void OnCellClicked(HexCell cell)
    {
        var active = TurnOrderController.Instance.CurrentCreature;
        if (active == null || cell == null)
            return;

        // Блокируем ходы не по очереди
        if (!TurnOrderController.Instance.IsCurrentTurn(active))
            return;

        movementController.OnCellClicked(active, cell);
    }

    public void OnCreatureClicked(Creature target)
    {
        var active = TurnOrderController.Instance.CurrentCreature;
        if (active == null || target == null || active == target)
            return;

        // Блокируем ходы не по очереди
        if (!TurnOrderController.Instance.IsCurrentTurn(active))
            return;

        combatController.OnCreatureClicked(active, target);
    }
}