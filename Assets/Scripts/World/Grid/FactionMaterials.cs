using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class FactionMaterials
{
    public Faction faction;
    [Tooltip("3 материала одной цветовой гаммы, но разной насыщенности")]
    public List<Material> variants = new List<Material>(3);
}