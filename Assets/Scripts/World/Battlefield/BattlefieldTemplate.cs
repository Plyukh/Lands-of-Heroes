using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Battle/BattlefieldTemplate")]
public class BattlefieldTemplate : ScriptableObject
{
    public string templateName;

    public List<ObstacleCell> obstacleCells = new List<ObstacleCell>();
}