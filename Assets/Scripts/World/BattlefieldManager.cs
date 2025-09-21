using System.Collections.Generic;
using UnityEngine;

public class BattlefieldManager : MonoBehaviour
{
    [Header("Main")]
    public Transform gridParent; // �������� ���� HexCell
    public List<HexCell> cells = new List<HexCell>(); // ����� ��������� ������� ��� ����� gridParent
    public BattlefieldTemplate currentTemplate;

    [Header("Faction Materials & Obstacles")]
    public Faction currentFaction = Faction.None;
    public List<Material> factionMaterials = new List<Material>();
    public List<FactionObstacles> factionObstacles = new List<FactionObstacles>();

    [Header("Decorative")]
    public Transform decorativeGridParent; // �������� ���� HexCell
    public List<HexCell> decorativeCells = new List<HexCell>(); // ����� ��������� ������� ��� ����� gridParent
    public List<GameObject> factionDecorations = new List<GameObject>();

    private void Start()
    {
        ApplyFactionMaterial();
        PlaceFactionObstaclesFromTemplate();
        ActivateFactionDecor();
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
            if (cell != null && cell.occupantType == CellOccupantType.None)
            {
                GameObject prefab = obstaclesData.obstaclePrefabs[Random.Range(0, obstaclesData.obstaclePrefabs.Count)];
                GameObject obstacle = Instantiate(prefab, cell.transform.position, Quaternion.identity, gridParent);
                cell.SetOccupant(CellOccupantType.Obstacle, obstacle);
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