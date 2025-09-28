using System.Collections.Generic;

[System.Serializable]
public class SerializableDictionary<TKey, TValue>
{
    public List<TKey> Keys = new List<TKey>();
    public List<TValue> Values = new List<TValue>();

    public SerializableDictionary() { }

    public SerializableDictionary(Dictionary<TKey, TValue> dict)
    {
        foreach (var kv in dict)
        {
            Keys.Add(kv.Key);
            Values.Add(kv.Value);
        }
    }

    public Dictionary<TKey, TValue> ToDictionary()
    {
        var dict = new Dictionary<TKey, TValue>();
        for (int i = 0; i < Keys.Count; i++)
            dict[Keys[i]] = Values[i];
        return dict;
    }
}