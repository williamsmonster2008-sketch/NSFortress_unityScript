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
    /// 获取家族成员列表（默认：仅在世，排除外姓配偶）
    /// </summary>
    public IReadOnlyCollection<CharacterRuntimeData> GetFamilyMembers(string familyName)
    {
        return GetFamilyMembers(familyName, includeDeceased: false, includeExternalSpouses: false);
    }
    
    /// <summary>
    /// 获取家族成员列表，可选包含已故、外姓配偶
    /// </summary>
    public IReadOnlyCollection<CharacterRuntimeData> GetFamilyMembers(
        string familyName,
        bool includeDeceased,
        bool includeExternalSpouses)
    {
        if (string.IsNullOrEmpty(familyName))
        {
            return Array.Empty<CharacterRuntimeData>();
        }
        
        if (familyCaches.TryGetValue(familyName, out var cache))
        {
            return cache.GetMembers(includeDeceased, includeExternalSpouses);
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
    
    /// <summary>
    /// 获取祖先链（向上：父母-祖父母...）
    /// </summary>
    public List<CharacterRuntimeData> GetAncestors(string characterId, int maxDepth = 5)
    {
        List<CharacterRuntimeData> result = new List<CharacterRuntimeData>();
        void AddParent(string pid, int depth)
        {
            if (depth > maxDepth || string.IsNullOrEmpty(pid)) return;
            var p = GetCharacter(pid);
            if (p == null) return;
            result.Add(p);
            AddParent(p.fatherId, depth + 1);
            AddParent(p.motherId, depth + 1);
        }
        AddParent(characterId, 1);
        return result;
    }
    
    /// <summary>
    /// 获取子孙链（向下：子-孙...）
    /// </summary>
    public List<CharacterRuntimeData> GetDescendants(string characterId, int maxDepth = 5, bool includeDeceased = true)
    {
        List<CharacterRuntimeData> result = new List<CharacterRuntimeData>();
        void AddChildren(string cid, int depth)
        {
            if (depth > maxDepth || string.IsNullOrEmpty(cid)) return;
            var c = GetCharacter(cid);
            if (c == null || c.childrenIds == null) return;
            foreach (var childId in c.childrenIds)
            {
                var child = GetCharacter(childId);
                if (child == null) continue;
                if (includeDeceased || child.vitalStatus == "living")
                {
                    result.Add(child);
                }
                AddChildren(childId, depth + 1);
            }
        }
        AddChildren(characterId, 1);
        return result;
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
        if (string.IsNullOrEmpty(characterId))
        {
            return FamilyIdentity.Empty;
        }
        return BuildFamilyIdentity(GetCharacter(characterId));
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
    
    public IReadOnlyCollection<CharacterRuntimeData> GetMembers(bool includeDeceased, bool includeExternal)
    {
        return members.Values
            .Where(m => m != null)
            .Where(m => includeDeceased || m.vitalStatus == "living")
            .Where(m => includeExternal || !m.isExternalSpouse)
            .ToList();
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
            ? $" {Mathf.Max(1, member.age)}岁"
            : $"殁年{Mathf.Max(1, member.age)}岁";
        
        string origin = ResolveOrigin(member);
        string tag = string.Empty;
        if (member.isRuzhui)
        {
            tag = "赘婿";
        }
        else if (member.isExternalSpouse)
        {
            tag = "妻室";
        }
        
        return new FamilyIdentity
        {
            statusText = status,
            originText = string.IsNullOrEmpty(origin) ? string.Empty : $"{origin}",
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
