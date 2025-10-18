using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class CreatureManager : MonoBehaviour
{
    public static CreatureManager Instance { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private HexGridManager gridManager;

    [Header("Creature Spawn Configuration")]
    [Tooltip("������ �������, ������� ����� ������� � ������ ���")]
    [SerializeField] private List<CreaturePlacementInfo> creaturePlacements = new List<CreaturePlacementInfo>();

    private List<Creature> allCreatures = new List<Creature>();

    private void Awake()
    {
        // ��������
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // ������� � ��������� ������� �� �����
        SpawnCreatures();
    }

    /// <summary>
    /// ������� ������� �� ������ ������ creaturePlacements � ��������� �� �� ����.
    /// </summary>
    private void SpawnCreatures()
    {
        allCreatures.Clear();

        if (gridManager == null)
        {
            Debug.LogError("[CreatureManager] HexGridManager �� ��������! ���������� ���������� �������.");
            return;
        }

        foreach (var placement in creaturePlacements)
        {
            if (placement.creaturePrefab == null)
            {
                Debug.LogWarning("[CreatureManager] � ����� �� ������ �� �������� ������ ��������. ����������.");
                continue;
            }

            // ������� ������ �� �����������
            string cellId = $"r{placement.row}_c{placement.column}";
            if (!gridManager.CellMap.TryGetValue(cellId, out var cell))
            {
                Debug.LogWarning($"[CreatureManager] ������ � ������������ ({placement.row}, {placement.column}) �� �������. �������� �� ����� �������.");
                continue;
            }

            // ������� ��������� ��������
            Creature newCreature = Instantiate(placement.creaturePrefab);

            // �������������� ��������
            newCreature.Initialize(placement.side, placement.level);

            // ��������� �������� �� ������
            if (newCreature.Side == TargetSide.Ally)
            {
                newCreature.Mover.SetCurrentCell(cell, Quaternion.Euler(180, 270, 270));
            }
            else if(newCreature.Side == TargetSide.Enemy)
            {
                newCreature.Mover.SetCurrentCell(cell, Quaternion.Euler(0,270,270));
            }

            cell.AddOccupant(newCreature.gameObject, CellObjectType.Creature);

            allCreatures.Add(newCreature);
        }
    }

    /// <summary>
    /// ������� ��� Creature � ����� � ��������� ������.
    /// ���� ����� ������ �� ����� ��� �������������, �� ����� ���� ������� ��� �������.
    /// </summary>
    public void RefreshCreaturesList()
    {
        allCreatures = FindObjectsOfType<Creature>().ToList();
    }

    /// <summary>
    /// ���������� ���� ������� ��������� �������.
    /// ���� side == Any � ���������� ����� ������� ������.
    /// </summary>
    public List<Creature> GetBySide(TargetSide side)
    {
        if (side == TargetSide.Any)
            return new List<Creature>(allCreatures);

        return allCreatures
            .Where(c => c.Side == side)
            .ToList();
    }

    /// <summary>
    /// ������� ������
    /// </summary>
    public List<Creature> GetAllies() => GetBySide(TargetSide.Ally);
    public List<Creature> GetEnemies() => GetBySide(TargetSide.Enemy);
}