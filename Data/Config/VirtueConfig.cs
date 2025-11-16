using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using UnityEngine;

public class VirtueConfigData
{
    public Dictionary<string, VirtueCategoryDefinition> categories = new Dictionary<string, VirtueCategoryDefinition>();
    public Dictionary<string, VirtueTraitDefinition> traitMap = new Dictionary<string, VirtueTraitDefinition>();
}

[System.Serializable]
public class VirtueCategoryDefinition
{
    public string id;
    public string name;
    public string description;
    public string color;
    public int importance;
    public List<VirtueTraitDefinition> traits = new List<VirtueTraitDefinition>();
}

[System.Serializable]
public class VirtueTraitDefinition
{
    public string id;
    public string name;
    public string negative;
    public string positive;
    public float weight;
    public float stability;
    public Vector2 initialRange;
    public float developmentRate;
}

public static class VirtueConfigLoader
{
    public static VirtueConfigData Load(TextAsset jsonAsset)
    {
        if (jsonAsset == null)
        {
            Debug.LogError("VirtueConfigLoader: 未提供配置");
            return null;
        }
        
        var jsonText = jsonAsset.text ?? string.Empty;
        if (jsonText.Length > 0 && jsonText[0] == '\ufeff')
        {
            jsonText = jsonText.Substring(1);
        }
        
        var root = Json.Deserialize(jsonText) as Dictionary<string, object>;
        if (root == null)
        {
            Debug.LogError("VirtueConfigLoader: 解析失败");
            return null;
        }
        
        var data = new VirtueConfigData();
        
        if (root.TryGetValue("virtueCategories", out var categoriesObj) && categoriesObj is Dictionary<string, object> categoriesDict)
        {
            foreach (var pair in categoriesDict)
            {
                var category = new VirtueCategoryDefinition
                {
                    id = pair.Key
                };
                if (pair.Value is Dictionary<string, object> catDict)
                {
                    category.name = catDict.GetString("name");
                    category.description = catDict.GetString("description");
                    category.color = catDict.GetString("color");
                    category.importance = Mathf.RoundToInt(catDict.GetFloat("importance"));
                    
                    if (catDict.TryGetValue("traits", out var traitsObj) && traitsObj is IList traitList)
                    {
                        foreach (var traitEntry in traitList)
                        {
                            if (traitEntry is Dictionary<string, object> traitDict)
                            {
                                var trait = new VirtueTraitDefinition
                                {
                                    id = traitDict.GetString("id"),
                                    name = traitDict.GetString("name"),
                                    negative = traitDict.GetString("negative"),
                                    positive = traitDict.GetString("positive"),
                                    weight = traitDict.GetFloat("weight", 1f),
                                    stability = traitDict.GetFloat("stability", 0.5f),
                                    developmentRate = traitDict.GetFloat("developmentRate", 0.1f),
                                    initialRange = traitDict.GetVector2("initialRange", new Vector2(-10f, 10f))
                                };
                                category.traits.Add(trait);
                                data.traitMap[trait.id] = trait;
                            }
                        }
                    }
                }
                data.categories[category.id] = category;
            }
        }
        
        return data;
    }
}

internal static class VirtueConfigExtensions
{
    public static string GetString(this Dictionary<string, object> dict, string key)
    {
        return dict != null && dict.TryGetValue(key, out var value) ? value as string ?? string.Empty : string.Empty;
    }
    
    public static float GetFloat(this Dictionary<string, object> dict, string key, float defaultValue = 0f)
    {
        if (dict != null && dict.TryGetValue(key, out var value))
        {
            if (value is float f) return f;
            if (value is double d) return (float)d;
            if (value is long l) return l;
            if (value is int i) return i;
        }
        return defaultValue;
    }
    
    public static Vector2 GetVector2(this Dictionary<string, object> dict, string key, Vector2 defaultValue)
    {
        if (dict != null && dict.TryGetValue(key, out var value) && value is IList list && list.Count >= 2)
        {
            float x = ParseFloat(list[0]);
            float y = ParseFloat(list[1]);
            return new Vector2(x, y);
        }
        return defaultValue;
    }
    
    private static float ParseFloat(object obj)
    {
        switch (obj)
        {
            case float f:
                return f;
            case double d:
                return (float)d;
            case long l:
                return l;
            case int i:
                return i;
            default:
                return 0f;
        }
    }
}
