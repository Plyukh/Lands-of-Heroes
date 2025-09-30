using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class CreatureManager : MonoBehaviour
{
    public static CreatureManager Instance { get; private set; }

    [Header("All Creatures on the Battlefield")]
    [Tooltip("���� ������������� ������� ��� Creature �� �����")]
    [SerializeField]
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

        // �������� ���� Creature, ������� ��� ���� �� �����
        RefreshCreaturesList();
    }

    /// <summary>
    /// ������� ��� Creature � ����� � ��������� ������.
    /// �������� ��� ������/�������� �������, ���� ��� ���������.
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
