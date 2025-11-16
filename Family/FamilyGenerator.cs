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
    
    [Header("人口规划")]
    public PopulationPlanConfig refugeePlan;
    public PopulationPlanConfig settlementPlan;
    public bool settlementMode = false;
    
    private System.Random random;
    private PopulationPlanConfig ActivePlan => (settlementMode && settlementPlan != null) ? settlementPlan : refugeePlan;
    private readonly Dictionary<string, HashSet<string>> externalSurnameCache = new Dictionary<string, HashSet<string>>();
    
    public void SetSettlementMode(bool enabled)
    {
        settlementMode = enabled;
    }
    
    public void Initialize(int seed)
    {
        random = new System.Random(seed);
        
        if (nameDatabase != null)
        {
            nameDatabase.Initialize();
        }
        
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
    public List<CharacterRuntimeData> GenerateCompleteFamily(string familyName, string socialClass, int targetSize)
    {
        PrepareExternalSurnameCache(familyName);
        // 阶段1: 生成年龄结构和关系网络
        FiveGenerationAgeStructure ageStructure = GenerateFiveGenerationStructure(familyName, targetSize, socialClass);
        
        Debug.Log($"📊 {familyName} 生成了 {ageStructure.GetTotalCount()} 人 (目标:{targetSize})");
        
        // 阶段2: 创建角色实例
        List<CharacterRuntimeData> members = CreateMembersFromAgeStructure(familyName, socialClass, ageStructure);
        
        // 阶段2.5: 根据代际存活率筛选
        members = ApplyGenerationSurvival(members);
        
        // 阶段3: 应用死亡率筛选
        members = ApplyMortalityAndResize(members, targetSize);
        
        // 阶段3.5: 控制青少年占比
        members = ApplyYouthRatioLimit(members, 0.10f);
        
        LogAgeDistribution(members, $"{familyName}");
        
        return members;
    }
    
    // ==================== 阶段1: 生成5代家族结构 ====================
    
    /// <summary>
    /// 生成5代家族结构 (年龄数据 + 关系网络)
    /// </summary>
    private FiveGenerationAgeStructure GenerateFiveGenerationStructure(string familyName, int targetSize, string socialClass)
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
            idManager, familyName, socialClass, gen1FatherAge, gen1MotherAge
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
        string socialClass,
        int gen1FatherAge,
        int gen1MotherAge)
    {
        FamilyStructure structure = new FamilyStructure();
        structure.familyName = familyName;
        structure.socialClass = socialClass;
        
        // === 第1代: 高祖夫妇 (固定2人) ===
        string gen1Father = idManager.AllocateId("patriarch_gen1");
        string gen1Mother = idManager.AllocateId("matriarch_gen1");
        
        structure.AddMember(new MemberAgeData(gen1Father, gen1FatherAge, Gender.Male, 1, true));
        structure.AddMember(new MemberAgeData(gen1Mother, gen1MotherAge, Gender.Female, 1, false));
        structure.AddMarriage(gen1Father, gen1Mother, 1);
        
        // === 第2代: 曾祖辈 (可能有多个儿子) ===
        int gen2ChildCount = CalculateChildrenCount(gen1MotherAge);
        List<string> gen2Sons = new List<string>();
        
        for (int i = 0; i < gen2ChildCount; i++)
        {
            // 生成儿子
            string gen2Son = idManager.AllocateId($"son_gen2_{i}");
            int sonAge = CalculateChildAge(gen1MotherAge, gen1FatherAge, i);
            
            var sonData = new MemberAgeData(gen2Son, sonAge, Gender.Male, 2, true);
            sonData.birthOrder = i;
            structure.AddMember(sonData);
            structure.AddParentChild(gen1Father, gen1Mother, gen2Son, i);
            gen2Sons.Add(gen2Son);
            
            // 为儿子生成配偶
            if (sonAge >= fertilityConfig.minBreedingAge)
            {
                string gen2Wife = idManager.AllocateId($"wife_gen2_{i}");
                int wifeAge = CalculateSpouseAge(sonAge, fertilityConfig.minBreedingAge);
                
                var wifeData = new MemberAgeData(gen2Wife, wifeAge, Gender.Female, 2, false);
                AssignExternalIdentity(wifeData, familyName, socialClass);
                structure.AddMember(wifeData);
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
        
        if (childGeneration >= 4)
        {
            float modifier = Mathf.Clamp01(fertilityConfig.refugeeFertilityRate);
            float scaled = childCount * modifier;
            int adjusted = Mathf.FloorToInt(scaled);
            float fractional = scaled - adjusted;
            if (random.NextDouble() < fractional)
            {
                adjusted++;
            }
            childCount = Mathf.Max(0, adjusted);
        }
        if (childCount == 0) return;
        
        List<int> childAges = GenerateChildAgeSequence(mother.age, father.age, childCount);
        List<string> siblingIds = new List<string>();
        
        for (int i = 0; i < childCount; i++)
        {
            // 随机性别
            Gender childGender = random.Next(2) == 0 ? Gender.Male : Gender.Female;
            string childId = idManager.AllocateId($"child_gen{childGeneration}_{marriage.husband}_{i}");
            
            // 计算子女年龄
            int childAge = childAges.Count > i ? childAges[i] : 1;
            
            var childData = new MemberAgeData(childId, childAge, childGender, childGeneration, true);
            childData.birthOrder = i;
            structure.AddMember(childData);
            structure.AddParentChild(marriage.husband, marriage.wife, childId, i);
            siblingIds.Add(childId);
            
            // 如果是儿子且到婚龄,生成配偶
            if (childGender == Gender.Male && childAge >= fertilityConfig.minBreedingAge && childGeneration < 5)
            {
                string wifeId = idManager.AllocateId($"wife_{childId}");
                int wifeAge = CalculateSpouseAge(childAge, fertilityConfig.minBreedingAge);
                
                var wifeData = new MemberAgeData(wifeId, wifeAge, Gender.Female, childGeneration, false);
                AssignExternalIdentity(wifeData, structure.familyName, structure.socialClass);
                structure.AddMember(wifeData);
                structure.AddMarriage(childId, wifeId, childGeneration);
            }
            else if (childGender == Gender.Female && childAge >= fertilityConfig.minBreedingAge && childGeneration < 5)
            {
                TryAssignRuzhuiSpouse(structure, idManager, childId, childGeneration);
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
        int maxPossibleChildren = Mathf.Max(0, fertilitySpan / 2); // 至少2年一个孩子
        if (maxPossibleChildren == 0)
        {
            return 0;
        }
        
        int slotCount = Mathf.Max(1, maxPossibleChildren * 2);
        float slotSize = fertilitySpan / (float)slotCount;
        float[] slotWeights = new float[slotCount];
        float totalWeight = 0f;
        
        for (int i = 0; i < slotCount; i++)
        {
            float sampleAge = earliestBirth + (i + 0.5f) * slotSize;
            float weight = Mathf.Max(0f, fertilityConfig.fertilityByAge.Evaluate(sampleAge));
            slotWeights[i] = weight;
            totalWeight += weight;
        }
        
        if (totalWeight <= Mathf.Epsilon)
        {
            return 0;
        }
        
        float normalizedSpan = fertilitySpan / Mathf.Max(1f, fertilityConfig.maxBreedingAge - fertilityConfig.minBreedingAge);
        float desiredChildren = Mathf.Clamp(fertilityConfig.averageChildren * normalizedSpan, 0f, fertilityConfig.maxChildren);
        float probabilityScale = desiredChildren / totalWeight;
        
        int childrenCount = 0;
        for (int i = 0; i < slotCount; i++)
        {
            float probability = Mathf.Clamp01(slotWeights[i] * probabilityScale);
            if (random.NextDouble() < probability)
            {
                childrenCount++;
                if (childrenCount >= maxPossibleChildren)
                {
                    break;
                }
            }
        }
        
        int finalCount = Mathf.Clamp(childrenCount, 0, Mathf.Min(fertilityConfig.maxChildren, maxPossibleChildren));
        
        Debug.Log($"🔍 母亲{motherAge}岁,生育窗口={fertilitySpan}年,计算子女数={finalCount}");
        
        return finalCount;
    }

    /// <summary>
    /// 生成子女年龄分布序列
    /// </summary>
    private List<int> GenerateChildAgeSequence(int motherAge, int fatherAge, int childCount)
    {
        List<int> ages = new List<int>();
        if (childCount <= 0)
        {
            return ages;
        }
        
        int earliestBirth = fertilityConfig.minBreedingAge;
        int latestBirth = Mathf.Min(motherAge, fertilityConfig.maxBreedingAge);
        if (latestBirth <= earliestBirth)
        {
            for (int i = 0; i < childCount; i++)
            {
                ages.Add(1);
            }
            return ages;
        }
        
        float fertilitySpan = latestBirth - earliestBirth;
        float firstBirthMin = earliestBirth + fertilitySpan * 0.1f;
        float firstBirthMax = earliestBirth + fertilitySpan * 0.35f;
        float currentBirthAge = Mathf.Clamp(
            firstBirthMin + (float)random.NextDouble() * (firstBirthMax - firstBirthMin),
            earliestBirth,
            latestBirth);
        
        int parentGap = Mathf.Max(10, fertilityConfig.minGenerationGap);
        
        for (int i = 0; i < childCount; i++)
        {
            if (i > 0)
            {
                int remainingChildren = childCount - i;
                float remainingSpan = Mathf.Max(1f, latestBirth - currentBirthAge);
                float avgGap = Mathf.Max(1.5f, remainingSpan / (remainingChildren + 1));
                float gapVariance = avgGap * 0.35f;
                float randomOffset = (float)(random.NextDouble() * 2 - 1) * gapVariance;
                float gap = Mathf.Max(1f, avgGap + randomOffset);
                currentBirthAge = Mathf.Min(latestBirth, currentBirthAge + gap);
            }
            
            int childAge = Mathf.Max(1, Mathf.RoundToInt(motherAge - currentBirthAge));
            
            int motherCap = Mathf.Max(1, motherAge - parentGap);
            int fatherCap = Mathf.Max(1, fatherAge - parentGap);
            childAge = Mathf.Min(childAge, motherCap);
            childAge = Mathf.Min(childAge, fatherCap);
            
            if (ages.Count > 0 && childAge >= ages[ages.Count - 1])
            {
                childAge = Mathf.Max(1, ages[ages.Count - 1] - random.Next(1, 3));
            }
            
            ages.Add(childAge);
        }
        
        return ages;
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
        bool external = !memberData.isNative;
        string assignedSocialClass = socialClass;
        string birthSurname = familyName;
        if (external)
        {
            assignedSocialClass = !string.IsNullOrEmpty(memberData.externalSocialClass)
                ? memberData.externalSocialClass
                : ResolveExternalSocialClass(socialClass);
            
            bool enforceUnique = ActivePlan == null || ActivePlan.forbidSameSurname;
            birthSurname = !string.IsNullOrEmpty(memberData.assignedSurname)
                ? memberData.assignedSurname
                : GetDistinctSurname(assignedSocialClass, familyName, enforceUnique);
        }
        
        CharacterRuntimeData character = new CharacterRuntimeData
        {
            characterId = System.Guid.NewGuid().ToString(),
            familyName = familyName,
            age = memberData.age,
            gender = memberData.gender,
            generation = memberData.generation,
            
            physical = new PhysicalState
            {
                health = random.Next(60, 100),
                energy = random.Next(60, 100),
                hunger = random.Next(50, 80)
            },
            
            emotional = new EmotionalState
            {
                happiness = random.Next(30, 70)
            },
            
            skills = new List<CharacterSkill>(),
            childrenIds = new List<string>(),
            vitalStatus = "living",
            position = Vector3.zero
        };
        
        if (nameDatabase != null)
        {
            var nameEntry = nameDatabase.GenerateNameEntry(
                assignedSocialClass,
                memberData.gender,
                birthSurname,
                memberData.generation,
                random,
                birthSurname);
            character.name = nameEntry.fullName;
            character.surname = nameEntry.surname;
        }
        else
        {
            character.name = $"{birthSurname}{random.Next(1000, 9999)}";
            character.surname = birthSurname;
        }
        
        character.socialClass = assignedSocialClass;
        character.isExternalSpouse = external;
        character.isRuzhui = external && memberData.gender == Gender.Male;
        
        if (external)
        {
            if (string.IsNullOrEmpty(character.originalFamily))
            {
                character.originalFamily = !string.IsNullOrEmpty(memberData.originalFamily)
                    ? memberData.originalFamily
                    : $"{birthSurname}氏";
            }
        }
        else if (string.IsNullOrEmpty(character.originalFamily))
        {
            character.originalFamily = $"{familyName}氏";
        }
        
        if (character.gender == Gender.Female && string.IsNullOrEmpty(character.maidenFamily))
        {
            character.maidenFamily = character.originalFamily;
        }
        
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
    
    /// <summary>
    /// 按代际存活率筛选
    /// </summary>
    private List<CharacterRuntimeData> ApplyGenerationSurvival(List<CharacterRuntimeData> members)
    {
        if (members == null)
        {
            return new List<CharacterRuntimeData>();
        }
        
        Dictionary<int, int> deathsByGeneration = new Dictionary<int, int>();
        
        foreach (var character in members)
        {
            if (character == null)
            {
                continue;
            }
            
            float survivalRate = Mathf.Clamp01(fertilityConfig.GetSurvivalRate(character.generation));
            double roll = random != null ? random.NextDouble() : UnityEngine.Random.value;
            bool survives = roll <= survivalRate;
            character.vitalStatus = survives ? "living" : "deceased";
            
            if (!survives)
            {
                if (deathsByGeneration.ContainsKey(character.generation))
                {
                    deathsByGeneration[character.generation]++;
                }
                else
                {
                    deathsByGeneration[character.generation] = 1;
                }
            }
        }
        
        if (deathsByGeneration.Count > 0)
        {
            string summary = string.Join(", ", deathsByGeneration.Select(kvp => $"{kvp.Key}代淘汰{kvp.Value}人"));
            Debug.Log($"[GenerationSurvival] {summary}");
        }
        
        return members;
    }
    
    /// <summary>
    /// 控制青少年（5-14岁）占比
    /// </summary>
    private List<CharacterRuntimeData> ApplyYouthRatioLimit(List<CharacterRuntimeData> members, float maxRatio)
    {
        if (members == null || members.Count == 0)
        {
            return members ?? new List<CharacterRuntimeData>();
        }
        
        var livingMembers = members.Where(c => c.vitalStatus == "living").ToList();
        if (livingMembers.Count == 0)
        {
            return members;
        }
        
        var youth = livingMembers.Where(c => c.age >= 5 && c.age <= 14).ToList();
        int allowedYouth = Mathf.CeilToInt(livingMembers.Count * Mathf.Clamp01(maxRatio));
        if (youth.Count <= allowedYouth)
        {
            return members;
        }
        
        int excess = youth.Count - allowedYouth;
        var selected = youth.OrderBy(_ => random.Next()).Take(excess).ToList();
        foreach (var child in selected)
        {
            child.vitalStatus = "deceased";
        }
        
        return members;
    }
    
    /// <summary>
    /// 输出年龄段和代际分布
    /// </summary>
    public static void LogAgeDistribution(List<CharacterRuntimeData> members, string label)
    {
        if (members == null || members.Count == 0)
        {
            Debug.Log($"📊 {label} 年龄段统计: 无成员");
            return;
        }
        
        int total = members.Count;
        int age55Plus = members.Count(c => c.age >= 55);
        int age45To54 = members.Count(c => c.age >= 45 && c.age <= 54);
        int age25To44 = members.Count(c => c.age >= 25 && c.age <= 44);
        int age15To24 = members.Count(c => c.age >= 15 && c.age <= 24);
        int age5To14 = members.Count(c => c.age >= 5 && c.age <= 14);
        int ageUnder5 = members.Count(c => c.age < 5);
        
        string Format(int count) => $"{count}人({(count * 100f / total):F1}%)";
        var generationCounts = members
            .GroupBy(c => c.generation)
            .OrderBy(g => g.Key)
            .Select(g => $"第{g.Key}代 {Format(g.Count())}");
        
        Debug.Log(
            $"📊 {label} 年龄段统计 (总数:{total}) | " +
            $"55+: {Format(age55Plus)} | " +
            $"45-54: {Format(age45To54)} | " +
            $"25-44: {Format(age25To44)} | " +
            $"15-24: {Format(age15To24)} | " +
            $"5-14: {Format(age5To14)} | " +
            $"<5: {Format(ageUnder5)}"
        );
        Debug.Log($"🔢 {label} 代际统计 | {string.Join(" | ", generationCounts)}");
    }
    
    // ==================== 阶段3: 死亡率筛选 ====================
    
    /// <summary>
    /// 应用死亡率并调整到目标规模
    /// </summary>
    private List<CharacterRuntimeData> ApplyMortalityAndResize(
        List<CharacterRuntimeData> fullFamily,
        int targetSize)
    {
        if (fullFamily == null || fullFamily.Count == 0)
        {
            return fullFamily ?? new List<CharacterRuntimeData>();
        }
        
        int livingCount = fullFamily.Count(c => c.vitalStatus == "living");
        if (livingCount <= targetSize)
        {
            return fullFamily;
        }
        
        int deathCount = livingCount - targetSize;
        float mortalityRate = (float)deathCount / Mathf.Max(1, livingCount);
        
        Debug.Log($"💀 应用死亡率: {fullFamily.Count}人 → {targetSize}人 (死亡率:{mortalityRate:P0})");
        
        // 按代际和年龄加权选择死者
        List<CharacterRuntimeData> deceased = SelectDeceasedMembers(fullFamily, deathCount);
        
        // 标记死者
        foreach (var character in deceased)
        {
            character.vitalStatus = "deceased";
        }
        
        // 返回所有成员(包括已故)
        int finalLivingCount = fullFamily.Count(c => c.vitalStatus == "living");
        Debug.Log($"✓ 最终: 存活{finalLivingCount}人, 已故{fullFamily.Count - finalLivingCount}人");
        
        return fullFamily; // 修改这里:返回全部,不过滤
    }
    
    /// <summary>
    /// 选择死亡成员
    /// </summary>
    private List<CharacterRuntimeData> SelectDeceasedMembers(
        List<CharacterRuntimeData> family,
        int deathCount)
    {
        List<CharacterRuntimeData> candidates = family
            .Where(c => c != null && c.vitalStatus == "living")
            .ToList();
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
    
    private void PrepareExternalSurnameCache(string familyName)
    {
        var set = GetExternalSurnameSet(familyName);
        set.Clear();
        if (!string.IsNullOrEmpty(familyName))
        {
            set.Add(familyName);
        }
    }
    
    private HashSet<string> GetExternalSurnameSet(string familyName)
    {
        string key = string.IsNullOrEmpty(familyName) ? "_default" : familyName;
        if (!externalSurnameCache.TryGetValue(key, out var set))
        {
            set = new HashSet<string>();
            externalSurnameCache[key] = set;
        }
        return set;
    }
    
    private void AssignExternalIdentity(
        MemberAgeData member,
        string hostFamilyName,
        string hostSocialClass,
        string forcedSocialClass = null)
    {
        if (member == null)
        {
            return;
        }
        
        var identity = GenerateExternalIdentity(hostFamilyName, hostSocialClass, forcedSocialClass);
        member.assignedSurname = identity.surname;
        member.originalFamily = identity.originalFamily;
        member.externalSocialClass = identity.socialClass;
    }
    
    private ExternalIdentity GenerateExternalIdentity(string hostFamilyName, string hostSocialClass, string preferredClass = null)
    {
        string resolvedClass = ResolveExternalSocialClass(hostSocialClass, preferredClass);
        bool enforceUnique = ActivePlan == null || ActivePlan.forbidSameSurname;
        string surname = GetDistinctSurname(resolvedClass, hostFamilyName, enforceUnique);
        if (string.IsNullOrEmpty(surname))
        {
            surname = hostFamilyName;
        }
        
        return new ExternalIdentity
        {
            surname = surname,
            originalFamily = string.IsNullOrEmpty(surname) ? hostFamilyName : $"{surname}氏",
            socialClass = resolvedClass
        };
    }
    
    private string ResolveExternalSocialClass(string hostSocialClass, string preferredClass = null)
    {
        if (!string.IsNullOrEmpty(preferredClass))
        {
            return preferredClass;
        }
        
        var plan = ActivePlan;
        if (plan == null || string.IsNullOrEmpty(hostSocialClass))
        {
            return hostSocialClass;
        }
        
        return plan.GetMatchedClass(hostSocialClass, random);
    }
    
    private void TryAssignRuzhuiSpouse(FamilyStructure structure, FamilyIdManager idManager, string daughterId, int generation)
    {
        if (!settlementMode || ActivePlan == null || structure == null || idManager == null)
        {
            return;
        }
        
        if (structure.relationships.HasSpouse(daughterId))
        {
            return;
        }
        
        string preferredClass = ActivePlan.GetMatchedClass(structure.socialClass, random);
        float ruzhuiProbability = ActivePlan.GetRuzhuiProbability(structure.socialClass, preferredClass);
        if (ruzhuiProbability <= 0f)
        {
            return;
        }
        
        if (random.NextDouble() > ruzhuiProbability)
        {
            return;
        }
        
        var daughter = structure.GetMember(daughterId);
        if (daughter == null)
        {
            return;
        }
        
        string husbandId = idManager.AllocateId($"ruzhui_{daughterId}");
        int husbandAge = CalculateSpouseAge(daughter.age, fertilityConfig.minBreedingAge);
        var husbandData = new MemberAgeData(husbandId, husbandAge, Gender.Male, generation, false);
        AssignExternalIdentity(husbandData, structure.familyName, structure.socialClass, preferredClass);
        structure.AddMember(husbandData);
        structure.AddMarriage(husbandId, daughterId, generation);
    }
    
    private string GetDistinctSurname(string desiredClass, string hostFamilyName, bool enforceUnique)
    {
        if (nameDatabase == null)
        {
            return hostFamilyName;
        }
        
        HashSet<string> usedSet = enforceUnique ? GetExternalSurnameSet(hostFamilyName) : null;
        string surname = null;
        int attempts = 0;
        do
        {
            string targetClass = string.IsNullOrEmpty(desiredClass) ? "平民" : desiredClass;
            surname = nameDatabase.GetRandomFamilyName(targetClass, random);
            attempts++;
            bool matchesHost = !string.IsNullOrEmpty(hostFamilyName) && surname == hostFamilyName;
            bool repeats = enforceUnique && usedSet != null && usedSet.Contains(surname);
            if (!matchesHost && !repeats)
            {
                break;
            }
        } while (attempts < 24);
        
        if (string.IsNullOrEmpty(surname))
        {
            surname = string.IsNullOrEmpty(hostFamilyName) ? "李" : hostFamilyName;
        }
        
        if (enforceUnique && usedSet != null && !string.IsNullOrEmpty(surname))
        {
            usedSet.Add(surname);
        }
        
        return surname;
    }
    
    private struct ExternalIdentity
    {
        public string surname;
        public string originalFamily;
        public string socialClass;
    }
    
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
