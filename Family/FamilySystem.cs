using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 家族系统 - 维护血缘缓存并提供查询入口
/// </summary>
[RequireComponent(typeof(KinshipCalculator))]
public class FamilySystem : MonoBehaviour
{
    private readonly Dictionary<string, CharacterRuntimeData> characters = new Dictionary<string, CharacterRuntimeData>();
    private readonly Dictionary<string, FamilyCache> familyCaches = new Dictionary<string, FamilyCache>();
    
    private KinshipCalculator kinshipCalculator;
    
    private void Awake()
    {
        kinshipCalculator = GetComponent<KinshipCalculator>();
    }
    
    /// <summary>
    /// 初始化缓存
    /// </summary>
    public void Initialize(IEnumerable<CharacterRuntimeData> initialCharacters)
    {
        characters.Clear();
        if (initialCharacters != null)
        {
            foreach (var character in initialCharacters)
            {
                if (character == null || string.IsNullOrEmpty(character.characterId))
                {
                    continue;
                }
                characters[character.characterId] = character;
            }
        }
        
        RebuildFamilyCaches();
        RefreshKinshipCalculator();
    }
    
    /// <summary>
    /// 注册或更新角色
    /// </summary>
    public void RegisterCharacter(CharacterRuntimeData character)
    {
        if (character == null || string.IsNullOrEmpty(character.characterId))
        {
            return;
        }
        
        characters[character.characterId] = character;
        UpdateFamilyCache(character.familyName);
        RefreshKinshipCalculator();
    }
    
    /// <summary>
    /// 移除角色
    /// </summary>
    public void RemoveCharacter(string characterId)
    {
        if (string.IsNullOrEmpty(characterId))
        {
            return;
        }
        
        if (!characters.TryGetValue(characterId, out var character))
        {
            return;
        }
        
        characters.Remove(characterId);
        UpdateFamilyCache(character.familyName);
        RefreshKinshipCalculator();
    }
    
    /// <summary>
    /// 获取亲属关系
    /// </summary>
    public KinshipRelation GetKinship(string fromId, string toId)
    {
        if (kinshipCalculator == null || string.IsNullOrEmpty(fromId) || string.IsNullOrEmpty(toId))
        {
            return null;
        }
        
        return kinshipCalculator.GetRelationship(fromId, toId);
    }
    
    /// <summary>
    /// 获取家族成员列表
    /// </summary>
    public IReadOnlyCollection<CharacterRuntimeData> GetFamilyMembers(string familyName)
    {
        if (string.IsNullOrEmpty(familyName))
        {
            return Array.Empty<CharacterRuntimeData>();
        }
        
        if (familyCaches.TryGetValue(familyName, out var cache))
        {
            return cache.GetMembers();
        }
        
        return Array.Empty<CharacterRuntimeData>();
    }
    
    /// <summary>
    /// 获取家族概览
    /// </summary>
    public FamilySummary GetFamilySummary(string familyName)
    {
        if (string.IsNullOrEmpty(familyName) || !familyCaches.TryGetValue(familyName, out var cache))
        {
            return default;
        }
        
        return new FamilySummary
        {
            familyName = familyName,
            totalMembers = cache.MemberCount,
            livingMembers = cache.LivingCount,
            generationSpread = cache.GetGenerationSpread()
        };
    }

    public List<CharacterRuntimeData> GetAllCharacters()
    {
        return new List<CharacterRuntimeData>(characters.Values);
    }
    
    private void RebuildFamilyCaches()
    {
        familyCaches.Clear();
        foreach (var group in characters.Values.Where(c => c != null && !string.IsNullOrEmpty(c.familyName))
                                               .GroupBy(c => c.familyName))
        {
            var cache = new FamilyCache(group.Key);
            foreach (var member in group)
            {
                cache.AddMember(member);
            }
            familyCaches[cache.FamilyName] = cache;
        }
    }
    
    private void UpdateFamilyCache(string familyName)
    {
        if (string.IsNullOrEmpty(familyName))
        {
            return;
        }
        
        if (!familyCaches.TryGetValue(familyName, out var cache))
        {
            cache = new FamilyCache(familyName);
            familyCaches[familyName] = cache;
        }
        cache.Rebuild(characters.Values.Where(c => c != null && c.familyName == familyName));
    }
    
    private void RefreshKinshipCalculator()
    {
        if (kinshipCalculator != null)
        {
            kinshipCalculator.Initialize(new Dictionary<string, CharacterRuntimeData>(characters));
        }
    }
    
    /// <summary>
    /// 获取角色数据
    /// </summary>
    public CharacterRuntimeData GetCharacter(string characterId)
    {
        if (string.IsNullOrEmpty(characterId))
        {
            return null;
        }
        
        characters.TryGetValue(characterId, out var data);
        return data;
    }
    
    public FamilyIdentity BuildFamilyIdentity(string characterId)
    {
        if (string.IsNullOrEmpty(characterId) || !characters.TryGetValue(characterId, out var data))
        {
            return FamilyIdentity.Empty;
        }
        return FamilyIdentity.Create(data);
    }
    
    public FamilyIdentity BuildFamilyIdentity(CharacterRuntimeData member)
    {
        return FamilyIdentity.Create(member);
    }
}

/// <summary>
/// 家族缓存
/// </summary>
internal class FamilyCache
{
    private readonly Dictionary<string, CharacterRuntimeData> members = new Dictionary<string, CharacterRuntimeData>();
    private readonly Dictionary<int, int> generationSpread = new Dictionary<int, int>();
    
    public string FamilyName { get; }
    
    public int MemberCount => members.Count;
    public int LivingCount => members.Values.Count(m => m != null && m.vitalStatus == "living");
    
    public FamilyCache(string familyName)
    {
        FamilyName = familyName;
    }
    
    public void AddMember(CharacterRuntimeData member)
    {
        if (member == null || string.IsNullOrEmpty(member.characterId))
        {
            return;
        }
        
        members[member.characterId] = member;
        RecalculateGenerations();
    }
    
    public void RemoveMember(string characterId)
    {
        if (string.IsNullOrEmpty(characterId))
        {
            return;
        }
        
        members.Remove(characterId);
        RecalculateGenerations();
    }
    
    public void Rebuild(IEnumerable<CharacterRuntimeData> source)
    {
        members.Clear();
        if (source != null)
        {
            foreach (var member in source)
            {
                if (member == null || string.IsNullOrEmpty(member.characterId))
                {
                    continue;
                }
                members[member.characterId] = member;
            }
        }
        RecalculateGenerations();
    }
    
    public IReadOnlyCollection<CharacterRuntimeData> GetMembers()
    {
        return members.Values.ToList();
    }
    
    public IReadOnlyDictionary<int, int> GetGenerationSpread()
    {
        return new Dictionary<int, int>(generationSpread);
    }
    
    private void RecalculateGenerations()
    {
        generationSpread.Clear();
        foreach (var member in members.Values)
        {
            if (member == null)
            {
                continue;
            }
            
            if (!generationSpread.ContainsKey(member.generation))
            {
                generationSpread[member.generation] = 0;
            }
            generationSpread[member.generation]++;
        }
    }
}

/// <summary>
/// 家族摘要信息
/// </summary>
public struct FamilySummary
{
    public string familyName;
    public int totalMembers;
    public int livingMembers;
    public IReadOnlyDictionary<int, int> generationSpread;
}

public struct FamilyIdentity
{
    public string statusText;
    public string originText;
    public string tagText;
    public bool isLiving;
    public bool isExternal;
    public bool isRuzhui;
    
    public static FamilyIdentity Empty => new FamilyIdentity();
    
    public static FamilyIdentity Create(CharacterRuntimeData member)
    {
        if (member == null)
        {
            return Empty;
        }
        
        bool living = member.vitalStatus == "living";
        string status = living
            ? $"在世 {Mathf.Max(1, member.age)}岁"
            : $"已故 享年{Mathf.Max(1, member.age)}岁";
        
        string origin = ResolveOrigin(member);
        string tag = string.Empty;
        if (member.isRuzhui)
        {
            tag = "赘婿";
        }
        else if (member.isExternalSpouse)
        {
            tag = "外姓配偶";
        }
        
        return new FamilyIdentity
        {
            statusText = status,
            originText = string.IsNullOrEmpty(origin) ? string.Empty : $"原籍{origin}",
            tagText = tag,
            isLiving = living,
            isExternal = member.isExternalSpouse,
            isRuzhui = member.isRuzhui
        };
    }
    
    private static string ResolveOrigin(CharacterRuntimeData member)
    {
        string hostTag = string.IsNullOrEmpty(member.familyName) ? string.Empty : $"{member.familyName}氏";
        
        if (!string.IsNullOrEmpty(member.originalFamily) && member.originalFamily != hostTag)
        {
            return member.originalFamily;
        }
        
        if (member.gender == Gender.Female && !string.IsNullOrEmpty(member.maidenFamily) && member.maidenFamily != hostTag)
        {
            return member.maidenFamily;
        }
        
        return string.Empty;
    }
}
