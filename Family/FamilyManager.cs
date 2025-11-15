using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 家族系统管理器
/// </summary>
public class FamilyManager : MonoBehaviour
{
    public static FamilyManager Instance { get; private set; }
    
    private Dictionary<string, CharacterRuntimeData> characters;
    private KinshipCalculator calculator;
    
    void Awake()
    {
        Instance = this;
    }
    
    public void Initialize(List<CharacterRuntimeData> characterList)
    {
        // 构建角色字典
        characters = new Dictionary<string, CharacterRuntimeData>();
        foreach (var character in characterList)
        {
            characters[character.characterId] = character;
        }
        
        // 初始化计算器
        if (calculator == null)
        {
            calculator = gameObject.AddComponent<KinshipCalculator>();
        }
        calculator.Initialize(characters);
        
        Debug.Log($"👨‍👩‍👧‍👦 家族系统初始化完成，共{characters.Count}人");
    }
    
    /// <summary>
    /// 查询两人关系
    /// </summary>
    public KinshipRelation GetRelationship(string fromId, string toId)
    {
        if (calculator == null)
        {
            Debug.LogError("❌ 家族系统未初始化");
            return null;
        }
        
        return calculator.GetRelationship(fromId, toId);
    }
    
    /// <summary>
    /// 获取角色的所有关系
    /// </summary>
    public List<KinshipRelation> GetAllRelationships(string characterId)
    {
        List<KinshipRelation> relations = new List<KinshipRelation>();
        
        foreach (var kvp in characters)
        {
            if (kvp.Key == characterId) continue;
            
            KinshipRelation relation = GetRelationship(characterId, kvp.Key);
            if (relation != null)
            {
                relations.Add(relation);
            }
        }
        
        return relations;
    }
    
    /// <summary>
    /// 测试关系计算
    /// </summary>
    public void TestRelationships()
    {
        Debug.Log("🧪 开始测试关系计算");

        // 先检查数据完整性
        Debug.Log("📊 数据检查:");
        int hasFather = 0;
        int hasMother = 0;
        int hasSpouse = 0;
        
        foreach (var kvp in characters)
        {
            var c = kvp.Value;
            if (!string.IsNullOrEmpty(c.fatherId)) hasFather++;
            if (!string.IsNullOrEmpty(c.motherId)) hasMother++;
            if (!string.IsNullOrEmpty(c.spouseId)) hasSpouse++;
            
            Debug.Log($"  {c.name}: 父={c.fatherId ?? "无"}, 母={c.motherId ?? "无"}, 代={c.generation}");
        }
        
        Debug.Log($"  统计: {hasFather}人有父亲, {hasMother}人有母亲, {hasSpouse}人有配偶");
        
        int testCount = 0;
        int successCount = 0;
        
        List<string> ids = new List<string>(characters.Keys);
        
        // 测试前5对关系
        for (int i = 0; i < Mathf.Min(5, ids.Count); i++)
        {
            for (int j = i + 1; j < Mathf.Min(5, ids.Count); j++)
            {
                string id1 = ids[i];
                string id2 = ids[j];
                
                testCount++;
                
                KinshipRelation relation = GetRelationship(id1, id2);
                
                if (relation != null)
                {
                    successCount++;
                    CharacterRuntimeData char1 = characters[id1];
                    CharacterRuntimeData char2 = characters[id2];
                    
                    Debug.Log($"  {char1.name} → {char2.name}: " +
                             $"{relation.title} (路径:{relation.pathLength}, 代差:{relation.generationGap}, 类型:{relation.pathType})");
                }
            }
        }
        
        Debug.Log($"✅ 测试完成: {successCount}/{testCount} 个关系计算成功");
    }
}