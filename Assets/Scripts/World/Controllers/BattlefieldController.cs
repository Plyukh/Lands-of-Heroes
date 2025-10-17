using System.IO;
using System.Linq;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

[DisallowMultipleComponent]
public class BattlefieldController : MonoBehaviour
{
    [Header("Core Controllers")]
    [Tooltip("Обрабатывает логику перемещения")]
    [SerializeField] private MovementController movementController;
    [Tooltip("Обрабатывает логику боя")]
    [SerializeField] private CombatController combatController;
    [Tooltip("Менеджер поиска пути и зоны досягаемости")]
    [SerializeField] private PathfindingManager pathfindingManager;
    [Tooltip("Инициализирует поле (фракции, препятствия, декор, начальную подсветку)")]
    [SerializeField] private BattlefieldInitializerManager initializerManager;

    private void Awake()
    {
        if (movementController == null ||
            combatController == null ||
            pathfindingManager == null ||
            initializerManager == null)
        {
            Debug.LogError("[BattlefieldController] Не все зависимости назначены в инспекторе!");
        }
    }

    /// <summary>
    /// Вызывается при клике по пустой клетке.
    /// Собирает путь и запускает перемещение activeCreature.
    /// </summary>
    public void OnCellClicked(HexCell cell)
    {
        var active = TurnOrderController.Instance.CurrentCreature;
        if (active == null || cell == null)
            return;

        // Ходим только в свой ход
        if (!TurnOrderController.Instance.IsCurrentTurn(active))
            return;

        var start = active.Mover.CurrentCell;
        int speed = active.GetStat(CreatureStatusType.Speed);
        var type = active.MovementType;
        var reachable = pathfindingManager
            .GetReachableCells(start, speed, type)
            .ToHashSet();

        // Если цель вне досягаемости — ничего не делаем
        if (!reachable.Contains(cell))
            return;

        // Строим маршрут и запускаем движение
        var path = pathfindingManager.FindPath(start, cell, type);
        if (path == null || path.Count == 0)
            return;

        movementController.MoveAlongPath(active, path);
    }

    /// <summary>
    /// Вызывается при клике по враждебной сущности.
    /// Немедленно запускает атаку (дальний или ближний бой).
    /// </summary>
    public void OnCreatureClicked(Creature target)
    {
        var active = TurnOrderController.Instance.CurrentCreature;
        if (active == null || target == null || active == target)
            return;

        // Атакуем только в свой ход
        if (!TurnOrderController.Instance.IsCurrentTurn(active))
            return;

           // 1) Берём тип атаки из данных существа
        AttackType selected = active.AttackType;

           // 3) Вызываем с учётом выбранного типа
        combatController.OnCreatureClicked(active, target, selected);
    }
}
