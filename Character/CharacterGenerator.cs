using UnityEngine;
using System.Collections.Generic;

public class CharacterGenerator : MonoBehaviour
{
    [Header("配置")]
    public CharacterNameDatabase nameDatabase;
    public SocialClassConfig socialClassConfig;
    
    private System.Random random;
    private CharacterTemplateDatabase templates;
    
    public void Initialize(int seed)
    {
        random = new System.Random(seed);
        
        // 加载数据
        templates = DataLoader.Instance.LoadCharacterTemplates();
        
        // 初始化名称数据库
        if (nameDatabase != null)
        {
            nameDatabase.Initialize();
        }
        else
        {
            Debug.LogError("❌ CharacterNameDatabase 未配置");
        }
        
        Debug.Log($"🏭 角色生成器初始化完成 (种子: {seed})");
    }
    
    /// <summary>
    /// 生成难民群体
    /// </summary>
    public List<CharacterRuntimeData> GenerateRefugeeGroup(int familyCount, int avgSize)
    {
        List<CharacterRuntimeData> characters = new List<CharacterRuntimeData>();
        
        for (int i = 0; i < familyCount; i++)
        {
            var family = GenerateFamily(avgSize);
            characters.AddRange(family);
        }
        
        Debug.Log($"✅ 生成了 {characters.Count} 个角色 ({familyCount} 个家庭)");
        return characters;
    }
    
    private List<CharacterRuntimeData> GenerateFamily(int targetSize)
    {
        List<CharacterRuntimeData> family = new List<CharacterRuntimeData>();
        // 使用概率分布选择社会阶层
        string familySocialClass = socialClassConfig != null 
            ? socialClassConfig.GetRandomSocialClass(random)
            : "平民";

         // 根据阶层选择姓氏
        string familyName = GetRandomFamilyName(familySocialClass); 

        Debug.Log($"🎲 生成家族 - 姓氏: {familyName}, 阶层: {familySocialClass}");
        // 传递社会阶层
        var father = GenerateCharacterFromTemplate("adult_male_farmer", familyName, familySocialClass, 1, Gender.Male);
        var mother = GenerateCharacterFromTemplate("adult_female_farmer", familyName, familySocialClass, 1, Gender.Female);
        
        // 设置夫妻关系
        father.spouseId = mother.characterId;
        mother.spouseId = father.characterId;
        
        family.Add(father);
        family.Add(mother);
        
        // 生成子女
        int childCount = Mathf.Max(0, targetSize - 2);
        for (int i = 0; i < childCount; i++)
        {
            // 70%概率是儿童，30%是老人
            string templateId = random.NextDouble() < 0.7 ? "child" : "elder";
            var child = GenerateCharacterFromTemplate(templateId, familyName, familySocialClass, templateId == "child" ? 2 : 1, null);
            
            if (child == null)
            {
                continue;
            }
            
            // 如果是儿童，建立父母关系
            if (templateId == "child" || child.age < 18)
            {
                child.fatherId = father.characterId;
                child.motherId = mother.characterId;
                
                // 父母添加子女
                father.childrenIds.Add(child.characterId);
                mother.childrenIds.Add(child.characterId);
            }
            else
            {
                // 老人是第一代
                child.generation = 1;
            }
            
            family.Add(child);
        }
        
        Debug.Log($"👨‍👩‍👧‍👦 生成家族 {familyName}: 父母+{childCount}人");
        
        return family;
    }
    
    private CharacterRuntimeData GenerateCharacterFromTemplate(string templateId, string familyName, string socialClass, int generation, Gender? forcedGender)
    {
        var template = templates.templates.Find(t => t.id == templateId);
        if (template == null)
        {
            Debug.LogError($"❌ 找不到模板: {templateId}");
            return null;
        }
        
        // 生成角色
        var character = new CharacterRuntimeData
        {
            characterId = System.Guid.NewGuid().ToString(),
            familyName = familyName,
            age = random.Next(template.ageRange[0], template.ageRange[1] + 1),
            gender = DetermineGender(template.gender, forcedGender),
            generation = generation
        };
        
        // 使用新的名称生成系统（姓 + 辈分字 + 名）        
        Debug.Log($"🔍 生成名字 - familyName参数: '{familyName}', socialClass: {socialClass}, generation: {generation}");
        var nameEntry = nameDatabase.GenerateNameEntry(socialClass, character.gender, familyName, generation, random);
        character.name = nameEntry.fullName;
        character.surname = nameEntry.surname;
        if (string.IsNullOrEmpty(character.familyName))
        {
            character.familyName = nameEntry.surname;
        }
        if (string.IsNullOrEmpty(character.originalFamily))
        {
            character.originalFamily = $"{character.surname}氏";
        }
        if (character.gender == Gender.Female && string.IsNullOrEmpty(character.maidenFamily))
        {
            character.maidenFamily = character.originalFamily;
        }
        character.socialClass = socialClass;
        character.isExternalSpouse = false;
        character.isRuzhui = false;
        Debug.Log($"✅ 生成结果: {character.name}");

        // 生成状态
        character.physical = new PhysicalState
        {
            health = random.Next(template.healthRange[0], template.healthRange[1] + 1),
            energy = random.Next(60, 100),
            hunger = random.Next(50, 80)
        };
        
        character.emotional = new EmotionalState
        {
            happiness = random.Next(30, 70)
        };
        
        // 生成技能
        character.skills = new List<CharacterSkill>();
        if (template.skillPresets != null)
        {
            foreach (var skillName in template.skillPresets)
            {
                character.skills.Add(new CharacterSkill
                {
                    skillName = skillName,
                    skillLevel = random.Next(10, 40)
                });
            }
        }
        
        // 初始化子女列表
        character.childrenIds = new List<string>();
        
        // 初始位置
        character.position = Vector3.zero;
        character.currentLocation = "广场";
        
        return character;
    }
    
    private string GetRandomFamilyName(string socialClass)  // 添加参数
    {
        if (nameDatabase == null)
        {
            Debug.LogError("❌ nameDatabase 未配置");
            return "李";
        }
        
        string surname = nameDatabase.GetRandomFamilyName(socialClass, random);
        Debug.Log($"🎲 CharacterGenerator获得姓氏: {surname} (阶层: {socialClass})");
        return surname;
    }

    private Gender DetermineGender(string templateGender, Gender? forced)
    {
        if (forced.HasValue)
        {
            return forced.Value;
        }

        string token = templateGender ?? string.Empty;
        token = token.Trim().ToLowerInvariant();

        if (string.IsNullOrEmpty(token) || token == "随机" || token == "random")
        {
            return random.Next(2) == 0 ? Gender.Male : Gender.Female;
        }

        if (token == "男" || token == "male")
        {
            return Gender.Male;
        }

        return Gender.Female;
    }
}
