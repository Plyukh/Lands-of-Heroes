using System.Collections.Generic;

public class PlayerData
{
    public int Level { get; }
    public int Experience { get; }
    public int Gold { get; }
    public Dictionary<string, int> CreatureLevels { get; }

    public PlayerData(int level, int exp, int gold, Dictionary<string, int> creatureLevels)
    {
        Level = level;
        Experience = exp;
        Gold = gold;
        CreatureLevels = creatureLevels;
    }
}