using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 家族生成器 - 完整重构版
/// 基于JS版本的5代家族算法,支持多分支、兄弟姐妹、完整关系网络
/// </summary>
public class FamilyGenerator : MonoBehaviour
{
    [Header("配置")]
    public FertilityConfig fertilityConfig;
    public CharacterNameDatabase nameDatabase;
    public SocialClassConfig socialClassConfig;
    
    private System.Random random;
    
    public void Initialize(int seed)
    {
        random = new System.Random(seed);
        Debug.Log($"🏭 家族生成器初始化 (种子: {seed})");
    }
    
    /// <summary>
    /// 生成难民群体 - 多个家族
    /// </summary>
    public List<CharacterRuntimeData> GenerateRefugeeGroup(int familyCount, int avgFamilySize)
    {
        List<CharacterRuntimeData> allCharacters = new List<CharacterRuntimeData>();
        
        for (int i = 0; i < familyCount; i++)
        {
            // 使用配置随机选择阶层
            string socialClass = socialClassConfig != null 
                ? socialClassConfig.GetRandomSocialClass(random)
                : "平民";
            
            string familyName = GetRandomFamilyName(socialClass);
            int targetSize = avgFamilySize + random.Next(-2, 3);
            
            List<CharacterRuntimeData> family = GenerateCompleteFamily(familyName, socialClass, targetSize);
            allCharacters.AddRange(family);
            
            Debug.Log($"👨‍👩‍👧‍👦 生成家族 {familyName} ({socialClass}): {family.Count}人");
        }
        
        return allCharacters;
    }
    
    /// <summary>
    /// 生成完整的5代家族
    /// </summary>
    public List<CharacterRuntimeData> GenerateCompleteFamily(string familyName, int targetSize, string socialClass)
    {
        // 阶段1: 生成年龄结构和关系网络
        FiveGenerationAgeStructure ageStructure = GenerateFiveGenerationStructure(familyName, targetSize);
        
        Debug.Log($"📊 {familyName} 生成了 {ageStructure.GetTotalCount()} 人 (目标:{targetSize})");
        
        // 阶段2: 创建角色实例
        List<CharacterRuntimeData> members = CreateMembersFromAgeStructure(familyName, socialClass, ageStructure);
        
        // 阶段3: 应用死亡率筛选
        if (ageStructure.GetTotalCount() > targetSize)
        {
            members = ApplyMortalityAndResize(members, targetSize);
        }
        
        return members;
    }
    
    // ==================== 阶段1: 生成5代家族结构 ====================
    
    /// <summary>
    /// 生成5代家族结构 (年龄数据 + 关系网络)
    /// </summary>
    private FiveGenerationAgeStructure GenerateFiveGenerationStructure(string familyName, int targetSize)
    {
        FamilyIdManager idManager = new FamilyIdManager();
        
        // 1. 计算高祖最小年龄
        int minGen1Age = fertilityConfig.minBreedingAge + (fertilityConfig.minGenerationGap * 4);
        
        // 2. 确保年龄范围有效
        if (minGen1Age >= fertilityConfig.maxLifespan)
        {
            Debug.LogError($"❌ 配置错误: minGen1Age({minGen1Age}) >= maxLifespan({fertilityConfig.maxLifespan})");
            minGen1Age = fertilityConfig.maxLifespan - 10; // 调整为安全值
        }
        
        // 3. 生成高祖夫妇年龄 - 确保母亲在最大生育年龄内
        // 高祖母必须在生育年龄内，才能有第2代子女
        int maxMotherAge = fertilityConfig.maxBreedingAge + (fertilityConfig.minGenerationGap * 4);
        int gen1FatherAge = random.Next(minGen1Age, Mathf.Min(fertilityConfig.maxLifespan, maxMotherAge + 10));
        
        // 确保母亲年龄在合理范围内
        int targetMotherAge = gen1FatherAge + random.Next(fertilityConfig.minCoupleAgeDiff, fertilityConfig.maxCoupleAgeDiff);
        int gen1MotherAge = Mathf.Clamp(targetMotherAge, minGen1Age, maxMotherAge);
        
        // 3. 生成前2代结构 (高祖 + 曾祖)
        FamilyStructure structure = GenerateInitialTwoGenerations(
            idManager, familyName, gen1FatherAge, gen1MotherAge
        );
        
        // 4. 递推生成第3-5代
        GenerateRemainingGenerations(structure, idManager);
        
        // 5. 转换为年龄结构数据
        return FiveGenerationAgeStructure.FromFamilyStructure(structure);
    }
    
    /// <summary>
    /// 生成前2代 (高祖 + 曾祖)
    /// </summary>
    private FamilyStructure GenerateInitialTwoGenerations(
        FamilyIdManager idManager,
        string familyName,
        int gen1FatherAge,
        int gen1MotherAge)
    {
        FamilyStructure structure = new FamilyStructure();
        structure.familyName = familyName;
        
        // === 第1代: 高祖夫妇 (固定2人) ===
        string gen1Father = idManager.AllocateId("patriarch_gen1");
        string gen1Mother = idManager.AllocateId("matriarch_gen1");
        
        structure.AddMember(new MemberAgeData(gen1Father, gen1FatherAge, Gender.男, 1, true));
        structure.AddMember(new MemberAgeData(gen1Mother, gen1MotherAge, Gender.女, 1, false));
        structure.AddMarriage(gen1Father, gen1Mother, 1);
        
        // === 第2代: 曾祖辈 (可能有多个儿子) ===
        int gen2ChildCount = CalculateChildrenCount(gen1MotherAge);
        List<string> gen2Sons = new List<string>();
        
        for (int i = 0; i < gen2ChildCount; i++)
        {
            // 生成儿子
            string gen2Son = idManager.AllocateId($"son_gen2_{i}");
            int sonAge = CalculateChildAge(gen1MotherAge, gen1FatherAge, i);
            
            var sonData = new MemberAgeData(gen2Son, sonAge, Gender.男, 2, true);
            sonData.birthOrder = i;
            structure.AddMember(sonData);
            structure.AddParentChild(gen1Father, gen1Mother, gen2Son, i);
            gen2Sons.Add(gen2Son);
            
            // 为儿子生成配偶
            if (sonAge >= fertilityConfig.minBreedingAge)
            {
                string gen2Wife = idManager.AllocateId($"wife_gen2_{i}");
                int wifeAge = CalculateSpouseAge(sonAge, fertilityConfig.minBreedingAge);
                
                structure.AddMember(new MemberAgeData(gen2Wife, wifeAge, Gender.女, 2, false));
                structure.AddMarriage(gen2Son, gen2Wife, 2);
            }
        }
        
        // 建立兄弟关系
        if (gen2Sons.Count > 1)
        {
            structure.AddSiblingGroup(gen2Sons, 2);
        }
        
        return structure;
    }
    
    /// <summary>
    /// 递推生成第3-5代
    /// </summary>
    private void GenerateRemainingGenerations(FamilyStructure structure, FamilyIdManager idManager)
    {
        // 第3代 - 基于第2代的每对夫妻
        GenerateGenerationFromParents(structure, idManager, 2, 3);
        
        // 第4代
        GenerateGenerationFromParents(structure, idManager, 3, 4);
        
        // 第5代
        GenerateGenerationFromParents(structure, idManager, 4, 5);
    }
    
    /// <summary>
    /// 基于父代的所有夫妻生成子代
    /// </summary>
    private void GenerateGenerationFromParents(
        FamilyStructure structure,
        FamilyIdManager idManager,
        int parentGeneration,
        int childGeneration)
    {
        var marriages = structure.GetMarriagesOfGeneration(parentGeneration);
        
        foreach (var marriage in marriages)
        {
            GenerateChildrenForCouple(structure, idManager, marriage, childGeneration);
        }
    }
    
    /// <summary>
    /// 为一对夫妻生成子女
    /// </summary>
    private void GenerateChildrenForCouple(
        FamilyStructure structure,
        FamilyIdManager idManager,
        MarriageRelation marriage,
        int childGeneration)
    {
        var mother = structure.GetMember(marriage.wife);
        var father = structure.GetMember(marriage.husband);
        
        if (mother == null || father == null) return;
        
        // 计算子女数量
        int childCount = CalculateChildrenCount(mother.age);
        if (childCount == 0) return;
        
        List<string> siblingIds = new List<string>();
        
        for (int i = 0; i < childCount; i++)
        {
            // 随机性别
            Gender childGender = random.Next(2) == 0 ? Gender.男 : Gender.女;
            string childId = idManager.AllocateId($"child_gen{childGeneration}_{marriage.husband}_{i}");
            
            // 计算子女年龄
            int childAge = CalculateChildAge(mother.age, father.age, i);
            
            var childData = new MemberAgeData(childId, childAge, childGender, childGeneration, true);
            childData.birthOrder = i;
            structure.AddMember(childData);
            structure.AddParentChild(marriage.husband, marriage.wife, childId, i);
            siblingIds.Add(childId);
            
            // 如果是儿子且到婚龄,生成配偶
            if (childGender == Gender.男 && childAge >= fertilityConfig.minBreedingAge && childGeneration < 5)
            {
                string wifeId = idManager.AllocateId($"wife_{childId}");
                int wifeAge = CalculateSpouseAge(childAge, fertilityConfig.minBreedingAge);
                
                structure.AddMember(new MemberAgeData(wifeId, wifeAge, Gender.女, childGeneration, false));
                structure.AddMarriage(childId, wifeId, childGeneration);
            }
        }
        
        // 建立兄弟姐妹关系
        if (siblingIds.Count > 1)
        {
            structure.AddSiblingGroup(siblingIds, childGeneration);
        }
    }
    
    /// <summary>
    /// 计算配偶年龄
    /// </summary>
    private int CalculateSpouseAge(int partnerAge, int minAllowedAge)
    {
        int minAge = partnerAge + fertilityConfig.minCoupleAgeDiff;
        int maxAge = partnerAge + fertilityConfig.maxCoupleAgeDiff;
        
        int spouseAge = random.Next(minAge, maxAge + 1);
        spouseAge = Mathf.Clamp(spouseAge, minAllowedAge, fertilityConfig.maxLifespan);
        
        return spouseAge;
    }
    
    /// <summary>
    /// 计算子女数量 - 基于生育窗口而非当前年龄
    /// </summary>
    private int CalculateChildrenCount(int motherAge)
    {
        // 计算生育窗口
        int earliestBirth = fertilityConfig.minBreedingAge;
        int latestBirth = Mathf.Min(motherAge, fertilityConfig.maxBreedingAge);
        
        // 生育年龄跨度
        int fertilitySpan = latestBirth - earliestBirth;
        
        if (fertilitySpan <= 0)
        {
            Debug.Log($"🔍 母亲{motherAge}岁,生育窗口={fertilitySpan} (无法生育)");
            return 0;
        }
        
        // 基于生育窗口计算最大可能子女数
        int maxPossibleChildren = Mathf.Max(1, fertilitySpan / 2); // 至少2年一个孩子
        
        // 基础子女数
        int baseCount = random.Next(fertilityConfig.minChildren, fertilityConfig.maxChildren + 1);
        
        // 限制在生育窗口允许的范围内
        int finalCount = Mathf.Min(baseCount, maxPossibleChildren);
        
        Debug.Log($"🔍 母亲{motherAge}岁,生育窗口={fertilitySpan}年,计算子女数={finalCount}");
        
        return Mathf.Max(1, finalCount);
    }
    
    /// <summary>
    /// 计算子女年龄 - 基于生育时年龄倒推
    /// 参考JS版本: childAge = motherAge - birthAge
    /// </summary>
    private int CalculateChildAge(int motherAge, int fatherAge, int birthOrder)
    {
        // 计算生育窗口
        int earliestBirth = fertilityConfig.minBreedingAge;
        int latestBirth = Mathf.Min(motherAge, fertilityConfig.maxBreedingAge);
        int fertilitySpan = latestBirth - earliestBirth;
        
        if (fertilitySpan <= 0)
        {
            return 1; // 兜底
        }
        
        // 计算这个孩子的生育时年龄
        int birthAge;
        
        if (birthOrder == 0)
        {
            // 第一个孩子 - 在生育窗口的前1/3
            int rangeStart = earliestBirth;
            int rangeEnd = earliestBirth + Mathf.Max(1, fertilitySpan / 3);
            birthAge = random.Next(rangeStart, rangeEnd + 1);
        }
        else
        {
            // 后续孩子 - 均匀分布在剩余窗口
            int rangeStart = earliestBirth + (fertilitySpan * birthOrder / 5);
            int rangeEnd = latestBirth - 2; // 留2年余地
            rangeEnd = Mathf.Max(rangeStart + 1, rangeEnd);
            birthAge = random.Next(rangeStart, rangeEnd + 1);
        }
        
        // 倒推当前年龄 = 母亲当前年龄 - 生育时年龄
        int childAge = motherAge - birthAge;
        
        return Mathf.Max(1, childAge);
    }
    
    // ==================== 阶段2: 创建角色实例 ====================
    
    /// <summary>
    /// 从年龄结构创建角色实例
    /// </summary>
    private List<CharacterRuntimeData> CreateMembersFromAgeStructure(
        string familyName,
        string socialClass,
        FiveGenerationAgeStructure structure)
    {
        List<CharacterRuntimeData> characters = new List<CharacterRuntimeData>();
        Dictionary<string, string> tempIdToRealId = new Dictionary<string, string>();
        
        // 遍历所有成员,创建角色
        var allMembers = structure.GetAllMembers();
        
        foreach (var memberData in allMembers)
        {
            var character = CreateCharacter(familyName, socialClass, memberData);
            characters.Add(character);
            
            // 记录临时ID到真实ID的映射
            tempIdToRealId[memberData.tempId] = character.characterId;
        }
        
        // 建立关系引用
        BuildRelationshipReferences(characters, tempIdToRealId, structure.relationships);
        
        return characters;
    }
    
    /// <summary>
    /// 创建单个角色
    /// </summary>
    private CharacterRuntimeData CreateCharacter(string familyName, string socialClass, MemberAgeData memberData)
    {
        CharacterRuntimeData character = new CharacterRuntimeData
        {
            characterId = System.Guid.NewGuid().ToString(),
            familyName = familyName,
            age = memberData.age,
            gender = memberData.gender,
            generation = memberData.generation,
            
            // 生理状态
            physical = new PhysicalState
            {
                health = random.Next(60, 100),
                energy = random.Next(60, 100),
                hunger = random.Next(50, 80)
            },
            
            // 情绪状态
            emotional = new EmotionalState
            {
                happiness = random.Next(30, 70)
            },
            
            // 德行
            virtues = new VirtueTraits
            {
                loving_tendency = random.Next(-50, 50),
                altruism_tendency = random.Next(-50, 50),
                social_tendency = random.Next(-50, 50)
            },
            
            skills = new List<CharacterSkill>(),
            childrenIds = new List<string>(),
            
            vitalStatus = "living",
            position = Vector3.zero
        };
        
        // 生成名字
        character.name = nameDatabase.GenerateFullName(
            socialClass, 
            memberData.gender, 
            familyName, 
            memberData.generation, 
            random
        );
        
        return character;
    }
    
    /// <summary>
    /// 建立关系引用 (从临时ID转换到真实角色ID)
    /// </summary>
    private void BuildRelationshipReferences(
        List<CharacterRuntimeData> characters,
        Dictionary<string, string> tempIdToRealId,
        FamilyRelationships relationships)
    {
        // 建立临时ID到角色的映射
        Dictionary<string, CharacterRuntimeData> tempIdToCharacter = new Dictionary<string, CharacterRuntimeData>();
        
        foreach (var character in characters)
        {
            var tempId = tempIdToRealId.FirstOrDefault(kvp => kvp.Value == character.characterId).Key;
            if (tempId != null)
            {
                tempIdToCharacter[tempId] = character;
            }
        }
        
        // 遍历所有角色,建立关系
        foreach (var kvp in tempIdToCharacter)
        {
            string tempId = kvp.Key;
            var character = kvp.Value;
            
            // 配偶关系
            string spouseTempId = relationships.GetSpouseId(tempId);
            if (spouseTempId != null && tempIdToRealId.ContainsKey(spouseTempId))
            {
                character.spouseId = tempIdToRealId[spouseTempId];
            }
            
            // 父母关系
            var parents = relationships.GetParents(tempId);
            if (parents != null)
            {
                if (tempIdToRealId.ContainsKey(parents.father))
                {
                    character.fatherId = tempIdToRealId[parents.father];
                }
                if (tempIdToRealId.ContainsKey(parents.mother))
                {
                    character.motherId = tempIdToRealId[parents.mother];
                }
            }
            
            // 子女关系
            var childrenTempIds = relationships.GetChildrenIds(tempId);
            foreach (var childTempId in childrenTempIds)
            {
                if (tempIdToRealId.ContainsKey(childTempId))
                {
                    character.childrenIds.Add(tempIdToRealId[childTempId]);
                }
            }
        }
    }
    
    // ==================== 阶段3: 死亡率筛选 ====================
    
    /// <summary>
    /// 应用死亡率并调整到目标规模
    /// </summary>
    private List<CharacterRuntimeData> ApplyMortalityAndResize(
        List<CharacterRuntimeData> fullFamily,
        int targetSize)
    {
        if (fullFamily.Count <= targetSize)
        {
            return fullFamily;
        }
        
        int deathCount = fullFamily.Count - targetSize;
        float mortalityRate = (float)deathCount / fullFamily.Count;
        
        Debug.Log($"💀 应用死亡率: {fullFamily.Count}人 → {targetSize}人 (死亡率:{mortalityRate:P0})");
        
        // 按代际和年龄加权选择死者
        List<CharacterRuntimeData> deceased = SelectDeceasedMembers(fullFamily, deathCount);
        
        // 标记死者
        foreach (var character in deceased)
        {
            character.vitalStatus = "deceased";
        }
        
        // 返回存活成员
        return fullFamily.Where(c => c.vitalStatus == "living").ToList();
    }
    
    /// <summary>
    /// 选择死亡成员
    /// </summary>
    private List<CharacterRuntimeData> SelectDeceasedMembers(
        List<CharacterRuntimeData> family,
        int deathCount)
    {
        List<CharacterRuntimeData> candidates = new List<CharacterRuntimeData>(family);
        List<CharacterRuntimeData> deceased = new List<CharacterRuntimeData>();
        
        // 按死亡权重排序 (年龄越大权重越高)
        candidates = candidates.OrderByDescending(c => CalculateDeathWeight(c)).ToList();
        
        // 选择前N个
        for (int i = 0; i < deathCount && i < candidates.Count; i++)
        {
            deceased.Add(candidates[i]);
        }
        
        return deceased;
    }
    
    /// <summary>
    /// 计算死亡权重 (年龄越大权重越高)
    /// </summary>
    private float CalculateDeathWeight(CharacterRuntimeData character)
    {
        float ageWeight = character.age / (float)fertilityConfig.maxLifespan;
        float generationWeight = (6 - character.generation) / 5f;
        
        return ageWeight * 0.7f + generationWeight * 0.3f + (float)random.NextDouble() * 0.1f;
    }
    
    // ==================== 辅助方法 ====================
    
    /// <summary>
    /// 获取随机姓氏
    /// </summary>
    private string GetRandomFamilyName(string socialClass)
    {
        if (nameDatabase == null)
        {
            Debug.LogError("❌ CharacterNameDatabase 未配置");
            return "李";
        }
        
        return nameDatabase.GetRandomFamilyName(socialClass, random);
    }
}