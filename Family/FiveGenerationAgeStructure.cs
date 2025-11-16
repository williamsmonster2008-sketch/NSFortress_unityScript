using System.Collections.Generic;

/// <summary>
/// 5代家族年龄结构 - 重构版
/// 改为列表存储每代的所有成员,而不是单个成员
/// </summary>
[System.Serializable]
public class FiveGenerationAgeStructure
{
    /// <summary>
    /// 第1代成员 (高祖辈)
    /// </summary>
    public List<MemberAgeData> generation1Members = new List<MemberAgeData>();
    
    /// <summary>
    /// 第2代成员 (曾祖辈)
    /// </summary>
    public List<MemberAgeData> generation2Members = new List<MemberAgeData>();
    
    /// <summary>
    /// 第3代成员 (祖父母辈)
    /// </summary>
    public List<MemberAgeData> generation3Members = new List<MemberAgeData>();
    
    /// <summary>
    /// 第4代成员 (父母辈)
    /// </summary>
    public List<MemberAgeData> generation4Members = new List<MemberAgeData>();
    
    /// <summary>
    /// 第5代成员 (子女辈)
    /// </summary>
    public List<MemberAgeData> generation5Members = new List<MemberAgeData>();
    
    /// <summary>
    /// 完整关系网络
    /// </summary>
    public FamilyRelationships relationships = new FamilyRelationships();
    
    /// <summary>
    /// 家族姓氏
    /// </summary>
    public string familyName;
    public string socialClass;
    
    /// <summary>
    /// 获取所有成员
    /// </summary>
    public List<MemberAgeData> GetAllMembers()
    {
        var allMembers = new List<MemberAgeData>();
        allMembers.AddRange(generation1Members);
        allMembers.AddRange(generation2Members);
        allMembers.AddRange(generation3Members);
        allMembers.AddRange(generation4Members);
        allMembers.AddRange(generation5Members);
        return allMembers;
    }
    
    /// <summary>
    /// 统计总人数
    /// </summary>
    public int GetTotalCount()
    {
        return generation1Members.Count +
               generation2Members.Count +
               generation3Members.Count +
               generation4Members.Count +
               generation5Members.Count;
    }
    
    /// <summary>
    /// 从FamilyStructure转换而来
    /// </summary>
    public static FiveGenerationAgeStructure FromFamilyStructure(FamilyStructure structure)
    {
        var ageStructure = new FiveGenerationAgeStructure();
        ageStructure.familyName = structure.familyName;
        ageStructure.socialClass = structure.socialClass;
        ageStructure.relationships = structure.relationships;
        
        // 按代际分组
        foreach (var member in structure.members.Values)
        {
            switch (member.generation)
            {
                case 1:
                    ageStructure.generation1Members.Add(member);
                    break;
                case 2:
                    ageStructure.generation2Members.Add(member);
                    break;
                case 3:
                    ageStructure.generation3Members.Add(member);
                    break;
                case 4:
                    ageStructure.generation4Members.Add(member);
                    break;
                case 5:
                    ageStructure.generation5Members.Add(member);
                    break;
            }
        }
        
        return ageStructure;
    }
}
