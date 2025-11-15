using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 家族生成测试脚本 - 用于验证新的5代家族算法
/// </summary>
public class FamilyGeneratorTest : MonoBehaviour
{
    [Header("测试配置")]
    public FamilyGenerator familyGenerator;
    public int testFamilyCount = 3;
    public int avgFamilySize = 15;
    
    [Header("测试结果")]
    public int totalGenerated = 0;
    public Dictionary<string, int> familySizes = new Dictionary<string, int>();
    
    void Start()
    {
        if (familyGenerator == null)
        {
            familyGenerator = GetComponent<FamilyGenerator>();
        }
        
        Debug.Log("🧪 开始家族生成测试...");
        TestFamilyGeneration();
    }
    
    [ContextMenu("运行测试")]
    public void TestFamilyGeneration()
    {
        // 初始化生成器
        familyGenerator.Initialize(System.DateTime.Now.Millisecond);
        
        // 生成多个家族
        var allCharacters = familyGenerator.GenerateRefugeeGroup(testFamilyCount, avgFamilySize);
        
        totalGenerated = allCharacters.Count;
        
        Debug.Log($"\n========== 测试结果 ==========");
        Debug.Log($"✅ 总共生成: {totalGenerated} 人");
        Debug.Log($"✅ 目标家族数: {testFamilyCount}");
        Debug.Log($"✅ 平均家族规模: {avgFamilySize}");
        
        // 统计各家族人数
        AnalyzeFamilies(allCharacters);
        
        // 检查关系网络
        CheckRelationships(allCharacters);
        
        Debug.Log($"==============================\n");
    }
    
    /// <summary>
    /// 分析各家族情况
    /// </summary>
    private void AnalyzeFamilies(List<CharacterRuntimeData> characters)
    {
        Dictionary<string, List<CharacterRuntimeData>> familyGroups = new Dictionary<string, List<CharacterRuntimeData>>();
        
        // 按家族分组
        foreach (var character in characters)
        {
            if (!familyGroups.ContainsKey(character.familyName))
            {
                familyGroups[character.familyName] = new List<CharacterRuntimeData>();
            }
            familyGroups[character.familyName].Add(character);
        }
        
        Debug.Log($"\n📊 各家族详情:");
        
        foreach (var kvp in familyGroups)
        {
            string familyName = kvp.Key;
            var members = kvp.Value;
            
            // 统计各代人数
            Dictionary<int, int> generationCounts = new Dictionary<int, int>();
            int livingCount = 0;
            int deceasedCount = 0;
            
            foreach (var member in members)
            {
                // 统计代际
                if (!generationCounts.ContainsKey(member.generation))
                {
                    generationCounts[member.generation] = 0;
                }
                generationCounts[member.generation]++;
                
                // 统计生死
                if (member.vitalStatus == "living")
                    livingCount++;
                else
                    deceasedCount++;
            }
            
            Debug.Log($"\n  🏠 {familyName}家族: {members.Count}人");
            Debug.Log($"     存活: {livingCount}人, 已故: {deceasedCount}人");
            
            for (int gen = 1; gen <= 5; gen++)
            {
                if (generationCounts.ContainsKey(gen))
                {
                    Debug.Log($"     第{gen}代: {generationCounts[gen]}人");
                }
            }
        }
    }
    
    /// <summary>
    /// 检查关系网络完整性
    /// </summary>
    private void CheckRelationships(List<CharacterRuntimeData> characters)
    {
        Debug.Log($"\n🔗 关系网络检查:");
        
        int marriageCount = 0;
        int parentChildCount = 0;
        int brokenRelations = 0;
        
        // 建立ID到角色的映射
        Dictionary<string, CharacterRuntimeData> idMap = new Dictionary<string, CharacterRuntimeData>();
        foreach (var character in characters)
        {
            idMap[character.characterId] = character;
        }
        
        foreach (var character in characters)
        {
            // 检查配偶关系
            if (!string.IsNullOrEmpty(character.spouseId))
            {
                if (idMap.ContainsKey(character.spouseId))
                {
                    marriageCount++;
                }
                else
                {
                    Debug.LogWarning($"  ⚠️ {character.name} 的配偶ID无效: {character.spouseId}");
                    brokenRelations++;
                }
            }
            
            // 检查父母关系
            if (!string.IsNullOrEmpty(character.fatherId))
            {
                if (idMap.ContainsKey(character.fatherId))
                {
                    parentChildCount++;
                }
                else
                {
                    Debug.LogWarning($"  ⚠️ {character.name} 的父亲ID无效: {character.fatherId}");
                    brokenRelations++;
                }
            }
            
            if (!string.IsNullOrEmpty(character.motherId))
            {
                if (idMap.ContainsKey(character.motherId))
                {
                    parentChildCount++;
                }
                else
                {
                    Debug.LogWarning($"  ⚠️ {character.name} 的母亲ID无效: {character.motherId}");
                    brokenRelations++;
                }
            }
        }
        
        Debug.Log($"  婚姻关系: {marriageCount / 2} 对"); // 除以2因为每个婚姻计算了2次
        Debug.Log($"  父母子女关系: {parentChildCount} 条");
        
        if (brokenRelations > 0)
        {
            Debug.LogError($"  ❌ 发现 {brokenRelations} 个无效关系引用!");
        }
        else
        {
            Debug.Log($"  ✅ 所有关系引用有效!");
        }
    }
}