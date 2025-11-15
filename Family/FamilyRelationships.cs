using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 家族关系网络数据 - 存储marriages/parentChild/siblings三类关系
/// </summary>
[System.Serializable]
public class FamilyRelationships
{
    public List<MarriageRelation> marriages = new List<MarriageRelation>();
    public List<ParentChildRelation> parentChild = new List<ParentChildRelation>();
    public List<SiblingGroup> siblings = new List<SiblingGroup>();
    
    /// <summary>
    /// 添加婚姻关系
    /// </summary>
    public void AddMarriage(string husbandId, string wifeId, int generation)
    {
        marriages.Add(new MarriageRelation
        {
            husband = husbandId,
            wife = wifeId,
            generation = generation
        });
    }
    
    /// <summary>
    /// 添加父母子女关系
    /// </summary>
    public void AddParentChild(string fatherId, string motherId, string childId, int birthOrder)
    {
        parentChild.Add(new ParentChildRelation
        {
            father = fatherId,
            mother = motherId,
            child = childId,
            birthOrder = birthOrder
        });
    }
    
    /// <summary>
    /// 添加兄弟姐妹组
    /// </summary>
    public void AddSiblingGroup(List<string> memberIds, int generation)
    {
        siblings.Add(new SiblingGroup
        {
            memberIds = new List<string>(memberIds),
            generation = generation
        });
    }
    
    /// <summary>
    /// 获取某人的配偶ID
    /// </summary>
    public string GetSpouseId(string personId)
    {
        var marriage = marriages.Find(m => m.husband == personId || m.wife == personId);
        if (marriage == null) return null;
        
        return marriage.husband == personId ? marriage.wife : marriage.husband;
    }
    
    /// <summary>
    /// 检查是否有配偶
    /// </summary>
    public bool HasSpouse(string personId)
    {
        return marriages.Any(m => m.husband == personId || m.wife == personId);
    }
    
    /// <summary>
    /// 获取父母信息
    /// </summary>
    public ParentChildRelation GetParents(string childId)
    {
        return parentChild.Find(pc => pc.child == childId);
    }
    
    /// <summary>
    /// 获取所有子女ID列表
    /// </summary>
    public List<string> GetChildrenIds(string parentId)
    {
        return parentChild
            .Where(pc => pc.father == parentId || pc.mother == parentId)
            .Select(pc => pc.child)
            .ToList();
    }
    
    /// <summary>
    /// 获取某代的所有婚姻
    /// </summary>
    public List<MarriageRelation> GetMarriagesOfGeneration(int generation)
    {
        return marriages.Where(m => m.generation == generation).ToList();
    }
    
    /// <summary>
    /// 获取兄弟姐妹组
    /// </summary>
    public List<string> GetSiblings(string personId)
    {
        var group = siblings.Find(sg => sg.memberIds.Contains(personId));
        if (group == null) return new List<string>();
        
        return group.memberIds.Where(id => id != personId).ToList();
    }
}

/// <summary>
/// 婚姻关系
/// </summary>
[System.Serializable]
public class MarriageRelation
{
    public string husband;
    public string wife;
    public int generation;
}

/// <summary>
/// 父母子女关系
/// </summary>
[System.Serializable]
public class ParentChildRelation
{
    public string father;
    public string mother;
    public string child;
    public int birthOrder; // 出生顺序(0=长子/长女)
}

/// <summary>
/// 兄弟姐妹组
/// </summary>
[System.Serializable]
public class SiblingGroup
{
    public List<string> memberIds = new List<string>();
    public int generation;
}