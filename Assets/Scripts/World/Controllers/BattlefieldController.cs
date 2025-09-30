using UnityEngine;

[DisallowMultipleComponent]
public class BattlefieldController : MonoBehaviour
{
    [Header("Core Controllers")]
    [Tooltip("������������ ������ �����������")]
    [SerializeField] private MovementController movementController;

    [Tooltip("������������ ������ ���")]
    [SerializeField] private CombatController combatController;

    [Tooltip("�������������� ���� (�������, �����������, �����, ��������� ���������)")]
    [SerializeField] private BattlefieldInitializerManager initializerManager;

    private void Awake()
    {
        // ��������� ������������ ������������
        if (movementController == null ||
            combatController == null ||
            initializerManager == null)
        {
            Debug.LogError("[BattlefieldController] �� ��� ����������� ��������� � ����������!");
        }
    }

    private void Start()
    {
        // ���������� ���� ����� ������ �����
        initializerManager.InitializeField();
    }

    public void OnCellClicked(HexCell cell)
    {
        var active = TurnOrderController.Instance.CurrentCreature;
        if (active == null || cell == null)
            return;

        // ��������� ���� �� �� �������
        if (!TurnOrderController.Instance.IsCurrentTurn(active))
            return;

        movementController.OnCellClicked(active, cell);
    }

    public void OnCreatureClicked(Creature target)
    {
        var active = TurnOrderController.Instance.CurrentCreature;
        if (active == null || target == null || active == target)
            return;

        // ��������� ���� �� �� �������
        if (!TurnOrderController.Instance.IsCurrentTurn(active))
            return;

        combatController.OnCreatureClicked(active, target);
    }
}