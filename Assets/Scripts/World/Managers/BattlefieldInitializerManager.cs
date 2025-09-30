using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class BattlefieldInitializerManager : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Менеджер шестиугольной сетки")]
    [SerializeField] private HexGridManager gridManager;
    [Tooltip("Менеджер поиска пути и зоны досягаемости")]
    [SerializeField] private PathfindingManager pathfindingManager;
    [Tooltip("Контроллер подсветки клеток")]
    [SerializeField] private HighlightController highlightController;

    [Header("Faction Settings")]
    [Tooltip("Настройки препятствий и префабов для каждой фракции")]
    [SerializeField] private List<FactionObstacles> factionObstacles = new List<FactionObstacles>();
    [Tooltip("Материалы для каждой фракции в порядке enum Faction")]
    [SerializeField] private List<Material> factionMaterials = new List<Material>();

    [Header("Field Templates")]
    [Tooltip("Шаблоны расстановки препятствий")]
    [SerializeField] private List<BattlefieldTemplate> templates = new List<BattlefieldTemplate>();

    [Header("Decorative Cells")]
    [Tooltip("Клетки для декоративных элементов")]
    [SerializeField] private List<HexCell> decorativeCells = new List<HexCell>();

    [Header("Faction Decorations")]
    [Tooltip("Игровые объекты декора для каждой фракции")]
    [SerializeField] private List<GameObject> factionDecorations = new List<GameObject>();

    private Faction currentFaction = Faction.None;
    private BattlefieldTemplate currentTemplate;

    private void Awake()
    {
        // Подпишемся на начало хода, чтобы подсвечивать активное существо
        var turnOrder = TurnOrderController.Instance;
        if (turnOrder != null)
            turnOrder.OnTurnStarted += OnTurnStarted;
    }

    private void OnDestroy()
    {
        // Отпишемся от события при уничтожении
        var turnOrder = TurnOrderController.Instance;
        if (turnOrder != null)
            turnOrder.OnTurnStarted -= OnTurnStarted;
    }

    private void Start()
    {
        InitializeField();
    }

    /// <summary>
    /// Полная инициализация поля: фракция, материалы, шаблон, препятствия, декор, обновление проходимости.
    /// </summary>
    public void InitializeField()
    {
        SetRandomFaction();
        ApplyFactionMaterial();
        SetRandomTemplate();
        PlaceFactionObstacles();
        ActivateFactionDecor();
        gridManager.UpdateAllWalkability();
    }

    private void OnTurnStarted(Creature active)
    {
        // Подсветить доступные ходы для нового активного существа
        if (active == null) return;

        var start = active.Mover.CurrentCell;
        int speed = active.GetStat(CreatureStatusType.Speed);
        var moveType = active.MovementType;

        var reachable = pathfindingManager
            .GetReachableCells(start, speed, moveType);

        highlightController.ClearHighlights();
        highlightController.HighlightReachable(reachable, start);
    }

    private void SetRandomFaction()
    {
        if (factionObstacles.Count == 0 || currentFaction != Faction.None)
            return;

        int idx = Random.Range(0, factionObstacles.Count);
        currentFaction = factionObstacles[idx].faction;
        Debug.Log($"[Initializer] Выбрана фракция: {currentFaction}");
    }

    private void ApplyFactionMaterial()
    {
        int idx = (int)currentFaction;
        if (idx < 0 || idx >= factionMaterials.Count)
        {
            Debug.LogWarning("[Initializer] Материал для фракции не найден");
            return;
        }

        var mat = factionMaterials[idx];
        foreach (var cell in gridManager.cells)
            cell?.SetMaterial(mat);

        foreach (var deco in decorativeCells)
            deco?.SetMaterial(mat);
    }

    private void SetRandomTemplate()
    {
        if (templates.Count == 0)
        {
            Debug.LogWarning("[Initializer] Нет доступных шаблонов поля");
            return;
        }

        currentTemplate = templates[Random.Range(0, templates.Count)];
        Debug.Log($"[Initializer] Выбран шаблон: {currentTemplate.templateName}");
    }

    private void PlaceFactionObstacles()
    {
        if (currentTemplate == null)
        {
            Debug.LogWarning("[Initializer] Шаблон поля не установлен");
            return;
        }

        var obstacleData = factionObstacles
            .Find(o => o.faction == currentFaction);
        if (obstacleData == null || obstacleData.obstaclePrefabs.Count == 0)
        {
            Debug.Log($"[Initializer] Нет препятствий для фракции {currentFaction}");
            return;
        }

        foreach (var cellInfo in currentTemplate.obstacleCells)
        {
            string id = $"r{cellInfo.row}_c{cellInfo.column}";
            if (!gridManager.CellMap.TryGetValue(id, out var cell))
                continue;

            var prefab = obstacleData.obstaclePrefabs[
                Random.Range(0, obstacleData.obstaclePrefabs.Count)];
            var obstacle = Instantiate(prefab, cell.transform, false);
            obstacle.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0, 360f));
            cell.SetCellObject(obstacle, CellObjectType.Obstacle);
        }
    }

    private void ActivateFactionDecor()
    {
        foreach (var deco in factionDecorations)
            if (deco != null)
                deco.SetActive(false);

        int idx = (int)currentFaction;
        if (idx >= 0 && idx < factionDecorations.Count && factionDecorations[idx] != null)
            factionDecorations[idx].SetActive(true);
    }
}
