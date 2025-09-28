using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance;

    public int Level { get; private set; }
    public int Experience { get; private set; }
    public int Gold { get; private set; }
    public Dictionary<string, int> CreatureLevels { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetPlayerStats(int level, int exp, int gold, Dictionary<string, int> creatures)
    {
        Level = level;
        Experience = exp;
        Gold = gold;
        CreatureLevels = creatures;
    }

    public int GetCreatureLevel(string id)
    {
        return CreatureLevels.ContainsKey(id) ? CreatureLevels[id] : 0;
    }
}
