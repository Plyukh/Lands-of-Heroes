using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class BattlefieldInitializerManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private HexGridManager gridManager;
    [SerializeField] private PathfindingManager pathfindingManager;
    [SerializeField] private HighlightController highlightController;

    [Header("Faction Obstacles")]
    [SerializeField] private List<FactionObstacles> factionObstacles = new List<FactionObstacles>();

    [Header("Faction Materials (3 per faction)")]
    [SerializeField] private List<FactionMaterials> factionMaterials = new List<FactionMaterials>();

    [Header("Field Templates")]
    [SerializeField] private List<BattlefieldTemplate> templates = new List<BattlefieldTemplate>();

    [Header("Decorative Cells")]
    [SerializeField] private List<HexCell> decorativeCells = new List<HexCell>();

    [Header("Faction Decorations")]
    [SerializeField] private List<GameObject> factionDecorations = new List<GameObject>();

    private Faction currentFaction = Faction.None;
    private BattlefieldTemplate currentTemplate;

    private void Awake()
    {
        var turnOrder = TurnOrderController.Instance;
        if (turnOrder != null)
            turnOrder.OnTurnStarted += OnTurnStarted;
    }

    private void OnDestroy()
    {
        var turnOrder = TurnOrderController.Instance;
        if (turnOrder != null)
            turnOrder.OnTurnStarted -= OnTurnStarted;
    }

    private void Start()
    {
        InitializeField();
    }

    public void InitializeField()
    {
        SetRandomFaction();
        ApplyFactionMaterial();
        SetRandomTemplate();
        PlaceFactionObstacles();
        ActivateFactionDecor();
        gridManager.UpdateAllWalkability();
    }

    private void SetRandomFaction()
    {
        if (factionObstacles.Count == 0 || currentFaction != Faction.None)
            return;

        int idx = UnityEngine.Random.Range(0, factionObstacles.Count);
        currentFaction = factionObstacles[idx].faction;
        Debug.Log($"[Initializer] �������: {currentFaction}");
    }

    private void ApplyFactionMaterial()
    {
        var fm = factionMaterials.Find(m => m.faction == currentFaction);
        if (fm == null || fm.variants.Count < 3)
            return;

        foreach (var cell in gridManager.Cells)
        {
            if (cell == null)
                continue;

            int offset = cell.row % 2;
            int matIndex = (cell.column + offset) % fm.variants.Count;
            cell.SetCellMaterial(fm.variants[matIndex]);
        }

        foreach (var deco in decorativeCells)
        {
            // decorativeCells � ���� �� ����, ��� ���� HexCell
            if (deco != null)
                deco.SetCellMaterial(fm.variants[1]);
        }
    }

    private void SetRandomTemplate()
    {
        if (templates.Count == 0)
        {
            Debug.LogWarning("[Initializer] ��� �������� ����");
            return;
        }

        currentTemplate = templates[UnityEngine.Random.Range(0, templates.Count)];
        Debug.Log($"[Initializer] ������: {currentTemplate.templateName}");
    }

    private void PlaceFactionObstacles()
    {
        if (currentTemplate == null)
        {
            Debug.LogWarning("[Initializer] ������ �� ����������");
            return;
        }

        var obstacleData = factionObstacles.Find(o => o.faction == currentFaction);
        if (obstacleData == null || obstacleData.obstaclePrefabs.Count == 0)
        {
            Debug.Log($"[Initializer] ����������� ��� ��� {currentFaction}");
            return;
        }

        foreach (var cellInfo in currentTemplate.obstacleCells)
        {
            string id = $"r{cellInfo.row}_c{cellInfo.column}";
            if (!gridManager.CellMap.TryGetValue(id, out var cell))
                continue;

            var prefab = obstacleData.obstaclePrefabs[
                UnityEngine.Random.Range(0, obstacleData.obstaclePrefabs.Count)];
            var obstacle = Instantiate(prefab, cell.transform, false);
            obstacle.transform.localRotation =
                Quaternion.Euler(0, 0, UnityEngine.Random.Range(0f, 360f));

            cell.AddOccupant(obstacle, CellObjectType.Obstacle);
        }
    }

    private void ActivateFactionDecor()
    {
        foreach (var deco in factionDecorations)
            deco?.SetActive(false);

        int idx = (int)currentFaction;
        if (idx >= 0 && idx < factionDecorations.Count)
            factionDecorations[idx]?.SetActive(true);
    }

    /// <summary>
    /// ��� ������ ���� ��������� �������� ������������ ���� ������������
    /// � �������������� activeMaterial.
    /// </summary>
    private void OnTurnStarted(Creature active)
    {
        if (active == null)
            return;

        // ���������� ����� preview-���������
        highlightController.ResetPreview();

        // ��������� ���� ������������
        var start = active.Mover.CurrentCell;
        int speed = active.GetStat(CreatureStatusType.Speed);
        var moveType = active.MovementType;
        var reachable = pathfindingManager
            .GetReachableCells(start, speed, moveType)
            .Where(c => c.IsWalkable);

        // ���������� ������ Active-Outline � ���������� �����
        highlightController.ClearHighlights();
        highlightController.HighlightReachable(reachable, start);
    }
}