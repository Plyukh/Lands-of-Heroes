using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattlefieldManager : MonoBehaviour
{
    public static BattlefieldManager Instance { get; private set; }

    [Header("Test Creature")]
    [Tooltip("Существо, для которого подсвечиваем движение")]
    public Creature testCreature;

    [Header("Main")]
    public List<HexCell> cells = new List<HexCell>(); // Можно заполнить вручную или через gridParent
    public Dictionary<string, HexCell> CellMap { get; private set; }

    [Header("Templates")]
    public BattlefieldTemplate currentTemplate;
    public List<BattlefieldTemplate> templates = new List<BattlefieldTemplate>();

    [Header("Faction Materials & Obstacles")]
    public Faction currentFaction = Faction.None;
    public List<Material> factionMaterials = new List<Material>();
    public List<FactionObstacles> factionObstacles = new List<FactionObstacles>();

    [Header("Decorative")]
    public Transform decorativeGridParent; // Родитель всех HexCell
    public List<HexCell> decorativeCells = new List<HexCell>(); // Можно заполнить вручную или через gridParent
    public List<GameObject> factionDecorations = new List<GameObject>();

    private void Awake()
    {
        // Синглтон-паттерн
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Строим словарь
        CellMap = cells.ToDictionary(c => c.CellId);
    }
    public void Start()
    {
        SetRandomFaction();
        ApplyFactionMaterial();
        SetRandomTemplate();
        PlaceFactionObstaclesFromTemplate();
        ActivateFactionDecor();

        UpdateAllWalkability();
        TryHighlightCreature();
    }

    public async void OnCellClicked(HexCell targetCell)
    {
        // 1. Получаем текущее существо и его параметры
        var creature = testCreature;
        if (creature == null) return;

        var mover = creature.Mover;
        var start = mover.CurrentCell;
        int speed = creature.GetStat(CreatureStatusType.Speed);
        var moveType = creature.MovementType;

        // 2. Проверяем, что целевая клетка подсвечена (доступна)
        var reachable = GetReachableCells(start, speed, moveType);
        if (!reachable.Contains(targetCell))
        {
            Debug.Log("BattlefieldManager: клетка недоступна для хода");
            return;
        }

        // 3. Строим путь BFS с родительскими ссылками
        List<HexCell> path = FindPath(start, targetCell, moveType);
        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("BattlefieldManager: не удалось построить путь");
            return;
        }

        // 4. Запускаем анимацию движения
        bool success = await mover.MoveAlongPath(path);
        if (!success)
            Debug.LogWarning("BattlefieldManager: существо не смогло дойти до цели");
    }

    private List<HexCell> FindPath(
        HexCell start,
        HexCell target,
        MovementType moveType)
    {
        var queue = new Queue<HexCell>();
        var parent = new Dictionary<HexCell, HexCell>();
        var visited = new HashSet<HexCell> { start };

        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var cell = queue.Dequeue();
            if (cell == target) break;

            foreach (var nb in GetNeighbors(cell))
            {
                bool canTraverse = nb.isWalkable
                                   || moveType == MovementType.Flying
                                   || moveType == MovementType.Teleport;

                if (canTraverse && visited.Add(nb))
                {
                    parent[nb] = cell;
                    queue.Enqueue(nb);
                }
            }
        }

        if (!parent.ContainsKey(target))
            return null;

        // Восстанавливаем путь
        var path = new List<HexCell>();
        var cur = target;
        while (cur != start)
        {
            path.Add(cur);
            cur = parent[cur];
        }
        path.Reverse();
        return path;
    }

    public void SetRandomFaction()
    {
        if (factionObstacles.Count == 0 || currentFaction != Faction.None)
        {
            Debug.LogWarning("Нет доступных фракций.");
            return;
        }

        int randomIndex = Random.Range(0, factionObstacles.Count);
        currentFaction = factionObstacles[randomIndex].faction;

        Debug.Log($"Выбрана фракция: {currentFaction}");
    }

    public void ApplyFactionMaterial()
    {
        int factionIndex = (int)currentFaction;
        if (factionIndex < 0 || factionIndex >= factionMaterials.Count)
        {
            Debug.LogWarning("Материал для этой фракции не найден!");
            return;
        }

        Material mat = factionMaterials[factionIndex];

        foreach (var cell in cells)
        {
            if (cell != null)
                cell.SetMaterial(mat);
        }
        foreach (var cell in decorativeCells)
        {
            if (cell != null)
                cell.SetMaterial(mat);
        }
    }

    public void SetRandomTemplate()
    {
        if (templates.Count == 0)
        {
            Debug.LogWarning("Нет доступных шаблонов.");
            return;
        }

        currentTemplate = templates[Random.Range(0, templates.Count)];
        Debug.Log($"Выбран шаблон: {currentTemplate.templateName}");
    }

    public void PlaceFactionObstaclesFromTemplate()
    {
        if (currentTemplate == null)
        {
            Debug.LogWarning("Шаблон поля боя не назначен!");
            return;
        }

        var obstaclesData = factionObstacles.Find(o => o.faction == currentFaction);
        if (obstaclesData == null || obstaclesData.obstaclePrefabs.Count == 0)
        {
            Debug.Log($"Нет препятствий для фракции {currentFaction}");
            return;
        }

        foreach (var cellData in currentTemplate.obstacleCells)
        {
            HexCell cell = cells.Find(c => c.row == cellData.row && c.column == cellData.column);
            if (cell != null)
            {
                GameObject prefab = obstaclesData.obstaclePrefabs[Random.Range(0, obstaclesData.obstaclePrefabs.Count)];
                GameObject obstacle = Instantiate(prefab, cell.transform, false);

                float randomZRotation = Random.Range(0f, 360f);
                obstacle.transform.localRotation = Quaternion.Euler(0f, 0f, randomZRotation);

                cell.SetCellObject(obstacle, CellObjectType.Obstacle);
            }
        }
    }

    public void ActivateFactionDecor()
    {
        // Выключаем всё
        foreach (var decor in factionDecorations)
        {
            if (decor != null)
                decor.SetActive(false);
        }

        // Включаем только нужное
        int factionIndex = (int)currentFaction;
        if (factionIndex >= 0 && factionIndex < factionDecorations.Count && factionDecorations[factionIndex] != null)
        {
            factionDecorations[factionIndex].SetActive(true);
        }
    }

    // Очищает все outline
    public void ClearAllHighlights()
    {
        foreach (var cell in cells)
            cell.ShowHighlight(false);
    }

    // Возвращает список клеток, до которых можно дойти за maxSteps шагов (BFS)
    public List<HexCell> GetReachableCells(
        HexCell start,
        int maxSteps,
        MovementType moveType)
    {
        var result = new List<HexCell>();
        var visited = new HashSet<HexCell> { start };
        var q = new Queue<(HexCell cell, int step)>();
        q.Enqueue((start, 0));

        while (q.Count > 0)
        {
            var (cell, step) = q.Dequeue();
            result.Add(cell);

            if (step >= maxSteps)
                continue;

            foreach (var neigh in GetNeighbors(cell))
            {
                bool canTraverse = neigh.isWalkable
                                   || moveType == MovementType.Flying
                                   || moveType == MovementType.Teleport;

                if (canTraverse && visited.Add(neigh))
                    q.Enqueue((neigh, step + 1));
            }
        }
        return result;
    }


    private readonly (int dr, int dc)[] oddRowOffsets = {
    (-1, 0), (-1, +1),
    ( 0, -1), ( 0, +1),
    (+1, 0), (+1, +1)
};


    private readonly (int dr, int dc)[] evenRowOffsets = {
    (-1, -1), (-1, 0),
    ( 0, -1), ( 0, +1),
    (+1, -1), (+1, 0)
};

    private IEnumerable<HexCell> GetNeighbors(HexCell cell)
    {
        // считаем, что row1 → индекс 0, row2 → индекс 1 и т.п.
        bool oddIndexed = ((cell.row - 1) % 2) != 0;

        var offsets = oddIndexed ? oddRowOffsets : evenRowOffsets;

        foreach (var (dr, dc) in offsets)
        {
            string id = $"r{cell.row + dr}_c{cell.column + dc}";
            if (CellMap.TryGetValue(id, out var neigh))
                yield return neigh;
        }
    }

    public void HighlightMovementRange(
        HexCell startCell,
        int speed,
        MovementType moveType)
    {
        ClearAllHighlights();
        UpdateAllWalkability();

        var reachable = GetReachableCells(startCell, speed, moveType);
        foreach (var cell in reachable)
            if (cell != startCell)
                cell.ShowHighlight(true);
    }

    private void TryHighlightCreature()
    {
        if (testCreature == null)
        {
            Debug.LogWarning("BattlefieldManager: не назначено testCreature");
            return;
        }

        HighlightCreatureMovement(testCreature);
    }

    private void HighlightCreatureMovement(Creature creature)
    {
        var mover = creature.Mover;
        var start = mover.CurrentCell;
        int speed = creature.GetStat(CreatureStatusType.Speed);
        var moveType = creature.MovementType;

        HighlightMovementRange(start, speed, moveType);
    }

    public void UpdateAllWalkability()
    {
        // Определяем, какие типы объектов блокируют клетку
        var blockingTypes = new HashSet<CellObjectType>
        {
            CellObjectType.Obstacle,
            CellObjectType.ForceField,
            CellObjectType.Creature
        };

        // Перебираем все клетки
        foreach (var cell in cells)
        {
            // Если хоть один occupant имеет blockingType — клетка непроходима
            bool walkable = !cell.occupants
                .Any(o => blockingTypes.Contains(o.type));

            cell.isWalkable = walkable;
        }
    }
}