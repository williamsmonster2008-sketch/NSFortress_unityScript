using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
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
        
        try
        {
            var root = JObject.Parse(jsonText);
            var data = new VirtueConfigData();
            
            if (root["virtueCategories"] != null)
            {
                foreach (var categoryPair in ((JObject)root["virtueCategories"]).Properties())
                {
                    var categoryKey = categoryPair.Name;
                    var categoryObj = categoryPair.Value as JObject;
                    
                    var category = new VirtueCategoryDefinition
                    {
                        id = categoryKey,
                        name = categoryObj["name"]?.ToString() ?? "",
                        description = categoryObj["description"]?.ToString() ?? "",
                        color = categoryObj["color"]?.ToString() ?? "#FFFFFF",
                        importance = categoryObj["importance"]?.ToObject<int>() ?? 1
                    };
                    
                    if (categoryObj["traits"] is JArray traitsArray)
                    {
                        foreach (JObject traitObj in traitsArray)
                        {
                            var trait = new VirtueTraitDefinition
                            {
                                id = traitObj["id"]?.ToString() ?? "",
                                name = traitObj["name"]?.ToString() ?? "",
                                negative = traitObj["negative"]?.ToString() ?? "",
                                positive = traitObj["positive"]?.ToString() ?? "",
                                weight = traitObj["weight"]?.ToObject<float>() ?? 1f,
                                stability = traitObj["stability"]?.ToObject<float>() ?? 0.5f,
                                developmentRate = traitObj["developmentRate"]?.ToObject<float>() ?? 0.1f
                            };
                            
                            if (traitObj["initialRange"] is JArray rangeArray && rangeArray.Count >= 2)
                            {
                                trait.initialRange = new Vector2(
                                    rangeArray[0].ToObject<float>(),
                                    rangeArray[1].ToObject<float>()
                                );
                            }
                            
                            category.traits.Add(trait);
                            data.traitMap[trait.id] = trait;
                        }
                    }
                    
                    data.categories[category.id] = category;
                }
            }
            
            Debug.Log($"✓ 成功加载 {data.categories.Count} 个德行类别");
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"VirtueConfigLoader: 解析异常: {e.Message}\n{e.StackTrace}");
            return null;
        }
    }
}