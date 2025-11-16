using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 德行系统 - 负责角色人格数据的初始化与更新
/// </summary>
public class VirtueSystem : MonoBehaviour
{
    [Header("配置")]
    public TextAsset virtueConfigJson;
    
    public VirtueConfigData configData { get; private set; }
    private readonly Dictionary<string, VirtueProfile> profiles = new Dictionary<string, VirtueProfile>();
    
    public void Initialize(IEnumerable<CharacterRuntimeData> characters)
    {
        if (configData == null)
        {
            configData = VirtueConfigLoader.Load(virtueConfigJson);
            if (configData == null)
            {
                Debug.LogError("VirtueSystem: 无法加载德行配置");
                return;
            }
        }
        
        profiles.Clear();
        if (characters == null) return;
        
        foreach (var character in characters)
        {
            if (character == null || string.IsNullOrEmpty(character.characterId))
            {
                continue;
            }
            
            profiles[character.characterId] = CreateProfile(character);
        }
        
        Debug.Log($"VirtueSystem: 初始化 {profiles.Count} 个德行档案");
    }
    
    public VirtueProfile GetProfile(string characterId)
    {
        if (string.IsNullOrEmpty(characterId))
        {
            return null;
        }
        profiles.TryGetValue(characterId, out var profile);
        return profile;
    }
    
    public float GetTraitValue(string characterId, string traitId)
    {
        var profile = GetProfile(characterId);
        return profile?.GetTraitValue(traitId) ?? 0f;
    }
    
    public void ApplyInfluence(string characterId, string traitId, float change, float intensity = 1f)
    {
        var profile = GetProfile(characterId);
        profile?.ApplyInfluence(traitId, change, intensity);
    }
    
    private VirtueProfile CreateProfile(CharacterRuntimeData character)
    {
        if (character.virtues == null)
        {
            character.virtues = new VirtueTraits();
        }
        
        var profile = new VirtueProfile(character.characterId);
        foreach (var trait in configData.traitMap.Values)
        {
            float value = character.virtues.GetValue(trait.id) ?? RandomizeInitialValue(trait);
            profile.SetTrait(trait, value);
            character.virtues.SetValue(trait.id, value);
        }
        return profile;
    }
    
    private float RandomizeInitialValue(VirtueTraitDefinition definition)
    {
        return Random.Range(definition.initialRange.x, definition.initialRange.y);
    }
    
    public void LogProfileSummary(CharacterRuntimeData character, int topTraits = 5)
    {
        if (character == null)
        {
            Debug.LogWarning("VirtueSystem.LogProfileSummary: character is null");
            return;
        }
        
        var profile = GetProfile(character.characterId);
        if (profile == null)
        {
            Debug.LogWarning($"VirtueSystem: 缺少 {character.name} ({character.characterId}) 的德行档案");
            return;
        }
        
        var summary = profile
            .EnumerateTraits()
            .OrderByDescending(t => Mathf.Abs(t.value))
            .Take(Mathf.Max(1, topTraits))
            .Select(t => $"{t.definition.name}:{t.value:F1}");
        
        Debug.Log($"VirtueSystem: {character.name} 的主要德行 -> {string.Join(" | ", summary)}");
    }
}

public class VirtueProfile
{
    private readonly Dictionary<string, VirtueTraitState> traits = new Dictionary<string, VirtueTraitState>();
    public string characterId { get; }
    
    public VirtueProfile(string characterId)
    {
        this.characterId = characterId;
    }
    
    public void SetTrait(VirtueTraitDefinition definition, float value)
    {
        traits[definition.id] = new VirtueTraitState(definition, value);
    }
    
    public float GetTraitValue(string traitId)
    {
        return traits.TryGetValue(traitId, out var trait) ? trait.value : 0f;
    }

    public void ApplyInfluence(string traitId, float change, float intensity)
    {
        if (traits.TryGetValue(traitId, out var trait))
        {
            trait.ApplyInfluence(change, intensity);
        }
    }
    
    public IEnumerable<VirtueTraitState> EnumerateTraits()
    {
        return traits.Values;
    }

    // 获取最高德行类别
    public (string categoryName, float score) GetTopCategory(VirtueConfigData configData)
    {
        var categoryScores = new Dictionary<string, float>();
        
        foreach (var category in configData.categories.Values)
        {
            float total = 0f;
            int count = 0;
            
            foreach (var trait in category.traits)
            {
                if (traits.TryGetValue(trait.id, out var state))
                {
                    total += Mathf.Abs(state.value);
                    count++;
                }
            }
            
            if (count > 0)
                categoryScores[category.name] = total / count;
        }
        
        var top = categoryScores.OrderByDescending(x => x.Value).FirstOrDefault();
        return (top.Key ?? "无", top.Value);
    }

    // 获取最突出的3个trait描述
    public List<string> GetTopTraitDescriptions(int count = 3)
    {
        return traits.Values
            .Where(t => Mathf.Abs(t.value) >= 30) // 只显示明显倾向
            .OrderByDescending(t => Mathf.Abs(t.value))
            .Take(count)
            .Select(t => t.value > 0 ? t.definition.positive : t.definition.negative)
            .ToList();
    }
}

public class VirtueTraitState
{
    public VirtueTraitDefinition definition { get; }
    public float value { get; private set; }
    public float stability => definition.stability;
    public float developmentRate => definition.developmentRate;
    
    private readonly List<VirtueInfluenceRecord> history = new List<VirtueInfluenceRecord>();
    
    public VirtueTraitState(VirtueTraitDefinition def, float initialValue)
    {
        definition = def;
        value = Mathf.Clamp(initialValue, -100f, 100f);
    }
    
    public void ApplyInfluence(float change, float intensity)
    {
        float actualChange = change * intensity * developmentRate;
        float stabilityFactor = 1f - Mathf.Clamp01(stability);
        float finalChange = actualChange * stabilityFactor;
        
        float oldValue = value;
        value = Mathf.Clamp(value + finalChange, -100f, 100f);
        
        history.Add(new VirtueInfluenceRecord
        {
            delta = finalChange,
            previousValue = oldValue,
            newValue = value,
            timestamp = Time.time
        });
        
        if (history.Count > 50)
        {
            history.RemoveAt(0);
        }
    }
    
    public IReadOnlyList<VirtueInfluenceRecord> GetHistory()
    {
        return history;
    }
}

public struct VirtueInfluenceRecord
{
    public float delta;
    public float previousValue;
    public float newValue;
    public float timestamp;
}
