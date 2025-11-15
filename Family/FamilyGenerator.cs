using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 家族生成器 - 基于JS版本的5代家族算法
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
            
            List<CharacterRuntimeData> family = GenerateCompleteFamily(familyName, targetSize);
            allCharacters.AddRange(family);
            
            Debug.Log($"👨‍👩‍👧‍👦 生成家族 {familyName} ({socialClass}): {family.Count}人");
        }
        
        return allCharacters;
    }
    
    /// <summary>
    /// 生成完整的5代家族
    /// </summary>
    public List<CharacterRuntimeData> GenerateCompleteFamily(string familyName, int targetSize)
    {
        // 阶段1: 生成年龄结构
        FiveGenerationAgeStructure ageStructure = GenerateFiveGenerationAges(targetSize);
        
        // 阶段2: 创建角色实例
        List<CharacterRuntimeData> members = CreateMembersFromAgeStructure(familyName, ageStructure);
        
        // 阶段3: 应用死亡率(暂时跳过,先生成完整家族)
        // members = ApplyMortalityRates(members);
        
        return members;
    }
    
    /// <summary>
    /// 阶段1: 生成5代家族的年龄结构
    /// </summary>
    private FiveGenerationAgeStructure GenerateFiveGenerationAges(int familySize)
    {
        // 1. 计算第1代(高祖)最小年龄
        int minGen1Age = fertilityConfig.minBreedingAge + 
                        (fertilityConfig.minGenerationGap * 4);
        
        // 2. 生成高祖父年龄
        int greatGreatGrandfatherAge = random.Next(minGen1Age, fertilityConfig.maxLifespan);
        
        // 3. 生成高祖母年龄
        int greatGreatGrandmotherAge = CalculateSpouseAge(greatGreatGrandfatherAge, minGen1Age);
        
        // 4. 递归生成其他代的年龄
        FiveGenerationAgeStructure structure = new FiveGenerationAgeStructure();
        structure.generation1Father = greatGreatGrandfatherAge;
        structure.generation1Mother = greatGreatGrandmotherAge;
        
        // 第2代(曾祖)
        int gen2Gap = random.Next(fertilityConfig.minGenerationGap, fertilityConfig.maxGenerationGap);
        int greatGrandfatherAge = greatGreatGrandfatherAge - gen2Gap;
        structure.generation2Father = greatGrandfatherAge;
        structure.generation2Mother = CalculateSpouseAge(greatGrandfatherAge, 
            fertilityConfig.minBreedingAge + fertilityConfig.minGenerationGap * 3);
        
        // 第3代(祖父)
        int gen3Gap = random.Next(fertilityConfig.minGenerationGap, fertilityConfig.maxGenerationGap);
        int grandfatherAge = greatGrandfatherAge - gen3Gap;
        structure.generation3Father = grandfatherAge;
        structure.generation3Mother = CalculateSpouseAge(grandfatherAge,
            fertilityConfig.minBreedingAge + fertilityConfig.minGenerationGap * 2);
        
        // 第4代(父亲)
        int gen4Gap = random.Next(fertilityConfig.minGenerationGap, fertilityConfig.maxGenerationGap);
        int fatherAge = grandfatherAge - gen4Gap;
        structure.generation4Father = fatherAge;
        structure.generation4Mother = CalculateSpouseAge(fatherAge,
            fertilityConfig.minBreedingAge + fertilityConfig.minGenerationGap);
        
        // 第5代(子女) - 基于母亲年龄生成
        structure.generation5Children = GenerateChildrenAges(
            structure.generation4Mother, 
            Mathf.Max(1, familySize - 8) // 至少1个孩子
        );
        
        return structure;
    }
    
    /// <summary>
    /// 计算配偶年龄
    /// </summary>
    private int CalculateSpouseAge(int partnerAge, int minAllowedAge)
    {
        int minAge = partnerAge + fertilityConfig.minCoupleAgeDiff;
        int maxAge = partnerAge + fertilityConfig.maxCoupleAgeDiff;
        
        int spouseAge = random.Next(minAge, maxAge + 1);
        
        // 限制在合理范围内
        spouseAge = Mathf.Clamp(spouseAge, minAllowedAge, fertilityConfig.maxLifespan);
        
        return spouseAge;
    }
    
    /// <summary>
    /// 生成子女年龄列表
    /// </summary>
    private List<int> GenerateChildrenAges(int motherAge, int childCount)
    {
        List<int> ages = new List<int>();
        
        // 计算母亲最大可能的生育年龄
        int maxChildAge = Mathf.Min(
            motherAge - fertilityConfig.minBreedingAge,
            25 // 最老的孩子不超过25岁
        );
        
        if (maxChildAge < 1) maxChildAge = 1;
        
        for (int i = 0; i < childCount; i++)
        {
            // 年龄递减,确保兄弟姐妹年龄合理
            int age = random.Next(1, maxChildAge - i * 2 + 1);
            age = Mathf.Max(1, age);
            ages.Add(age);
        }
        
        ages.Sort((a, b) => b.CompareTo(a)); // 从大到小排序
        
        return ages;
    }
    
    /// <summary>
    /// 阶段2: 从年龄结构创建角色实例
    /// </summary>
    private List<CharacterRuntimeData> CreateMembersFromAgeStructure(
        string familyName, 
        FiveGenerationAgeStructure structure)
    {
        List<CharacterRuntimeData> members = new List<CharacterRuntimeData>();
        
        // 第1代 - 高祖父母
        var gen1Father = CreateCharacter(familyName, Gender.男, structure.generation1Father, 1);
        var gen1Mother = CreateCharacter(familyName, Gender.女, structure.generation1Mother, 1);
        EstablishMarriage(gen1Father, gen1Mother);
        members.Add(gen1Father);
        members.Add(gen1Mother);
        
        // 第2代 - 曾祖父母
        var gen2Father = CreateCharacter(familyName, Gender.男, structure.generation2Father, 2);
        var gen2Mother = CreateCharacter(familyName, Gender.女, structure.generation2Mother, 2);
        EstablishMarriage(gen2Father, gen2Mother);
        EstablishParentChild(gen1Father, gen1Mother, gen2Father);
        members.Add(gen2Father);
        members.Add(gen2Mother);
        
        // 第3代 - 祖父母
        var gen3Father = CreateCharacter(familyName, Gender.男, structure.generation3Father, 3);
        var gen3Mother = CreateCharacter(familyName, Gender.女, structure.generation3Mother, 3);
        EstablishMarriage(gen3Father, gen3Mother);
        EstablishParentChild(gen2Father, gen2Mother, gen3Father);
        members.Add(gen3Father);
        members.Add(gen3Mother);
        
        // 第4代 - 父母
        var gen4Father = CreateCharacter(familyName, Gender.男, structure.generation4Father, 4);
        var gen4Mother = CreateCharacter(familyName, Gender.女, structure.generation4Mother, 4);
        EstablishMarriage(gen4Father, gen4Mother);
        EstablishParentChild(gen3Father, gen3Mother, gen4Father);
        members.Add(gen4Father);
        members.Add(gen4Mother);
        
        // 第5代 - 子女
        foreach (int childAge in structure.generation5Children)
        {
            Gender childGender = random.Next(2) == 0 ? Gender.男 : Gender.女;
            var child = CreateCharacter(familyName, childGender, childAge, 5);
            EstablishParentChild(gen4Father, gen4Mother, child);
            members.Add(child);
        }
        
        return members;
    }
    
    /// <summary>
    /// 创建角色
    /// </summary>
    private CharacterRuntimeData CreateCharacter(string familyName, Gender gender, int age, int generation)
    {
        CharacterRuntimeData character = new CharacterRuntimeData
        {
            characterId = System.Guid.NewGuid().ToString(),
            familyName = familyName,
            name = "", // 先留空，下面生成
            age = age,
            gender = gender,
            generation = generation,
            
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
            
            virtues = new VirtueTraits
            {
                loving_tendency = random.Next(-50, 50),
                altruism_tendency = random.Next(-50, 50),
                social_tendency = random.Next(-50, 50)
            },
            
            skills = new List<CharacterSkill>(),
            childrenIds = new List<string>(),
            
            position = Vector3.zero
        };
        
        // 生成名字（姓 + 辈分字 + 名）
        string socialClass = "庶族"; // 暂时固定为庶族，后续可以根据家族配置
        character.name = nameDatabase.GenerateFullName(socialClass, gender, familyName, generation, random);
        
        return character;
    }
    
    /// <summary>
    /// 建立婚姻关系
    /// </summary>
    private void EstablishMarriage(CharacterRuntimeData husband, CharacterRuntimeData wife)
    {
        husband.spouseId = wife.characterId;
        wife.spouseId = husband.characterId;
    }
    
    /// <summary>
    /// 建立父母子女关系
    /// </summary>
    private void EstablishParentChild(
        CharacterRuntimeData father, 
        CharacterRuntimeData mother, 
        CharacterRuntimeData child)
    {
        child.fatherId = father.characterId;
        child.motherId = mother.characterId;
        
        father.childrenIds.Add(child.characterId);
        mother.childrenIds.Add(child.characterId);
    }
    
    /// <summary>
    /// 获取随机姓氏
    /// </summary>
    private string GetRandomFamilyName(string socialClass)
    {
        return nameDatabase.GetRandomFamilyName(socialClass, random);
    }
}

/// <summary>
/// 5代家族年龄结构
/// </summary>
public class FiveGenerationAgeStructure
{
    public int generation1Father;
    public int generation1Mother;
    public int generation2Father;
    public int generation2Mother;
    public int generation3Father;
    public int generation3Mother;
    public int generation4Father;
    public int generation4Mother;
    public List<int> generation5Children = new List<int>();
}