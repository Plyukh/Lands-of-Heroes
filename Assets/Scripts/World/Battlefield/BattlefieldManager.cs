using System.Collections.Generic;
using UnityEngine;

public class BattlefieldManager : MonoBehaviour
{
    [Header("Main")]
    public Transform gridParent; // �������� ���� HexCell
    public List<HexCell> cells = new List<HexCell>(); // ����� ��������� ������� ��� ����� gridParent

    [Header("Templates")]
    public BattlefieldTemplate currentTemplate;
    public List<BattlefieldTemplate> templates = new List<BattlefieldTemplate>();

    [Header("Faction Materials & Obstacles")]
    public Faction currentFaction = Faction.None;
    public List<Material> factionMaterials = new List<Material>();
    public List<FactionObstacles> factionObstacles = new List<FactionObstacles>();

    [Header("Decorative")]
    public Transform decorativeGridParent; // �������� ���� HexCell
    public List<HexCell> decorativeCells = new List<HexCell>(); // ����� ��������� ������� ��� ����� gridParent
    public List<GameObject> factionDecorations = new List<GameObject>();

    public void Start()
    {
        SetRandomFaction();
        ApplyFactionMaterial();
        SetRandomTemplate();
        PlaceFactionObstaclesFromTemplate();
        ActivateFactionDecor();
    }

    public void SetRandomFaction()
    {
        if (factionObstacles.Count == 0 || currentFaction != Faction.None)
        {
            Debug.LogWarning("��� ��������� �������.");
            return;
        }

        int randomIndex = Random.Range(0, factionObstacles.Count);
        currentFaction = factionObstacles[randomIndex].faction;

        Debug.Log($"������� �������: {currentFaction}");
    }

    public void ApplyFactionMaterial()
    {
        int factionIndex = (int)currentFaction;
        if (factionIndex < 0 || factionIndex >= factionMaterials.Count)
        {
            Debug.LogWarning("�������� ��� ���� ������� �� ������!");
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
            Debug.LogWarning("��� ��������� ��������.");
            return;
        }

        currentTemplate = templates[Random.Range(0, templates.Count)];
        Debug.Log($"������ ������: {currentTemplate.templateName}");
    }

    public void PlaceFactionObstaclesFromTemplate()
    {
        if (currentTemplate == null)
        {
            Debug.LogWarning("������ ���� ��� �� ��������!");
            return;
        }

        var obstaclesData = factionObstacles.Find(o => o.faction == currentFaction);
        if (obstaclesData == null || obstaclesData.obstaclePrefabs.Count == 0)
        {
            Debug.Log($"��� ����������� ��� ������� {currentFaction}");
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
        // ��������� ��
        foreach (var decor in factionDecorations)
        {
            if (decor != null)
                decor.SetActive(false);
        }

        // �������� ������ ������
        int factionIndex = (int)currentFaction;
        if (factionIndex >= 0 && factionIndex < factionDecorations.Count && factionDecorations[factionIndex] != null)
        {
            factionDecorations[factionIndex].SetActive(true);
        }
    }
}