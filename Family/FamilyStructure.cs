using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 家族结构 - 用于生成过程的中间数据
/// 包含所有成员的年龄数据 + 完整关系网络
/// </summary>
public class FamilyStructure
{
    /// <summary>
    /// 所有成员的年龄数据 (按临时ID索引)
    /// </summary>
    public Dictionary<string, MemberAgeData> members = new Dictionary<string, MemberAgeData>();
    
    /// <summary>
    /// 关系网络
    /// </summary>
    public FamilyRelationships relationships = new FamilyRelationships();
    
    /// <summary>
    /// 家族姓氏
    /// </summary>
    public string familyName;
    public string socialClass;
    
    /// <summary>
    /// 添加成员
    /// </summary>
    public void AddMember(MemberAgeData member)
    {
        members[member.tempId] = member;
    }
    
    /// <summary>
    /// 获取成员
    /// </summary>
    public MemberAgeData GetMember(string tempId)
    {
        return members.ContainsKey(tempId) ? members[tempId] : null;
    }
    
    /// <summary>
    /// 添加婚姻关系
    /// </summary>
    public void AddMarriage(string husbandId, string wifeId, int generation)
    {
        relationships.AddMarriage(husbandId, wifeId, generation);
    }
    
    /// <summary>
    /// 添加父母子女关系
    /// </summary>
    public void AddParentChild(string fatherId, string motherId, string childId, int birthOrder)
    {
        relationships.AddParentChild(fatherId, motherId, childId, birthOrder);
    }
    
    /// <summary>
    /// 添加兄弟姐妹组
    /// </summary>
    public void AddSiblingGroup(List<string> siblingIds, int generation)
    {
        relationships.AddSiblingGroup(siblingIds, generation);
    }
    
    /// <summary>
    /// 获取某代的所有儿子ID
    /// </summary>
    public List<string> GetSonsOfGeneration(int generation)
    {
        return members.Values
            .Where(m => m.generation == generation && m.gender == Gender.Male && m.isNative)
            .Select(m => m.tempId)
            .ToList();
    }
    
    /// <summary>
    /// 获取某代的所有成员
    /// </summary>
    public List<MemberAgeData> GetMembersOfGeneration(int generation)
    {
        return members.Values
            .Where(m => m.generation == generation)
            .ToList();
    }
    
    /// <summary>
    /// 获取某代的所有婚姻
    /// </summary>
    public List<MarriageRelation> GetMarriagesOfGeneration(int generation)
    {
        return relationships.GetMarriagesOfGeneration(generation);
    }
    
    /// <summary>
    /// 统计总人数
    /// </summary>
    public int GetTotalMemberCount()
    {
        return members.Count;
    }
    
    /// <summary>
    /// 统计各代人数
    /// </summary>
    public Dictionary<int, int> GetGenerationCounts()
    {
        var counts = new Dictionary<int, int>();
        for (int gen = 1; gen <= 5; gen++)
        {
            counts[gen] = members.Values.Count(m => m.generation == gen);
        }
        return counts;
    }
}
