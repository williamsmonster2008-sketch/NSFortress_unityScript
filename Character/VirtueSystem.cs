using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 德行系统 - 负责角色人格数据的初始化与更新
/// </summary>
public class VirtueSystem : MonoBehaviour
{
    [Header("配置")]
    public TextAsset virtueConfigJson;
    
    private VirtueConfigData configData;
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
        var profile = new VirtueProfile(character.characterId);
        foreach (var trait in configData.traitMap.Values)
        {
            float value = character.virtues?.GetValue(trait.id) ?? RandomizeInitialValue(trait);
            profile.SetTrait(trait, value);
        }
        return profile;
    }
    
    private float RandomizeInitialValue(VirtueTraitDefinition definition)
    {
        return Random.Range(definition.initialRange.x, definition.initialRange.y);
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
