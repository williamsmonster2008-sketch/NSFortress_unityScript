using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

/// <summary>
/// 角色信息面板
/// </summary>
public class CharacterInfoPanel : MonoBehaviour
{
    [Header("UI 引用")]
    public GameObject panelRoot;
    public TextMeshProUGUI familyText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI ageText;
    public TextMeshProUGUI genderText;
    public TextMeshProUGUI generationText;
    public TextMeshProUGUI fatherText;
    public TextMeshProUGUI motherText;
    public TextMeshProUGUI siblingsText;
    public TextMeshProUGUI childrenText;
    public TextMeshProUGUI topVirtuesText;
    
    private FamilySystem familySystem;
    
    public void ShowCharacter(CharacterRuntimeData character, FamilySystem system)
    {
        familySystem = system;
        
        if (system != null)
        {
            var allChars = system.GetAllCharacters();
            var deceasedCount = allChars.Count(c => c.vitalStatus != "living");
            Debug.Log($"角色总数: {allChars.Count}, 已故: {deceasedCount}");
        }
        
        if (panelRoot != null)
        {
            panelRoot.SetActive(character != null);
        }
        
        if (character == null)
        {
            return;
        }
        
        familyText?.SetText($"家族: {character.familyName}氏");
        nameText?.SetText($"{character.name}");
        ageText?.SetText($"{character.age}岁");
        genderText?.SetText($"{(character.gender == Gender.Male ? "男" : "女")}");
        generationText?.SetText(BuildGenerationLine(character));
        
        fatherText?.SetText(BuildParentInfo("父亲", character.fatherId));
        motherText?.SetText(BuildParentInfo("母亲", character.motherId));
        siblingsText?.SetText(BuildRelativeList("手足", CollectSiblings(character)));
        childrenText?.SetText(BuildRelativeList("子女", CollectChildren(character)));
        
        var profile = GameManager.Instance.virtueSystem.GetProfile(character.characterId);
        if (profile != null)
        {
            var topCategory = profile.GetTopCategory(GameManager.Instance.virtueSystem.configData);
            var topTraits = profile.GetTopTraitDescriptions(3);
            
            string display = $"品格: {topCategory.categoryName}\n";
            display += string.Join("\n", topTraits);
            topVirtuesText.SetText(display);
        }
        else
        {
            topVirtuesText.SetText("品格特征: 暂无");
        }
    }
    
    private string BuildParentInfo(string label, string parentId)
    {
        if (string.IsNullOrEmpty(parentId))
        {
            return $"{label}: 不详";
        }
        
        var parent = familySystem?.GetCharacter(parentId);
        if (parent == null)
        {
            return $"{label}: 不详";
        }
        
        // 父母只显示姓名 + 存活状态 + 年龄
        return $"{label}: {BuildBasicLine(parent)}";
    }
    
    private string BuildRelativeList(string label, List<string> relatives)
    {
        if (relatives == null || relatives.Count == 0)
        {
            return $"{label}: 无";
        }
        
        return $"{label}: {string.Join("、", relatives)}";
    }
    
    private List<string> CollectSiblings(CharacterRuntimeData character)
    {
        HashSet<string> ids = new HashSet<string>();
        var result = new List<string>();
        
        void AddFromParent(string parentId)
        {
            if (string.IsNullOrEmpty(parentId))
            {
                return;
            }
            
            var parent = familySystem?.GetCharacter(parentId);
            if (parent == null || parent.childrenIds == null)
            {
                return;
            }
            
            foreach (var childId in parent.childrenIds)
            {
                if (childId == character.characterId || !ids.Add(childId))
                {
                    continue;
                }
                
                var child = familySystem?.GetCharacter(childId);
                if (child != null)
                {
                    result.Add(BuildIdentityLine(child));
                }
            }
        }
        
        AddFromParent(character.fatherId);
        AddFromParent(character.motherId);
        return result;
    }
    
    private List<string> CollectChildren(CharacterRuntimeData character)
    {
        var list = new List<string>();
        if (character.childrenIds == null || character.childrenIds.Count == 0)
        {
            return list;
        }
        
        foreach (var childId in character.childrenIds)
        {
            var child = familySystem?.GetCharacter(childId);
            if (child != null)
            {
                list.Add(BuildIdentityLine(child));
            }
        }
        return list;
    }
    
    private string BuildIdentityLine(CharacterRuntimeData data)
    {
        if (data == null)
        {
            return "不详";
        }
        
        FamilyIdentity identity = familySystem != null
            ? familySystem.BuildFamilyIdentity(data)
            : FamilyIdentity.Create(data);
        
        List<string> tags = new List<string>();
        if (!string.IsNullOrEmpty(identity.tagText))
        {
            tags.Add(identity.tagText);
        }
        if (!string.IsNullOrEmpty(identity.originText))
        {
            tags.Add(identity.originText);
        }
        
        string tagSection = tags.Count > 0 ? $" ({string.Join(" | ", tags)})" : string.Empty;
        string status = string.IsNullOrEmpty(identity.statusText)
            ? (data.vitalStatus == "living" ? $"{data.age}岁" : $"殁年{data.age}岁")
            : identity.statusText;
        
        return $"{data.name}{tagSection} {status}";
    }

    private string BuildBasicLine(CharacterRuntimeData data)
    {
        if (data == null)
        {
            return "不详";
        }
        bool living = data.vitalStatus == "living";       
        string ageInfo = living ? $"{data.age}岁" : $"殁年{data.age}岁";
        return $"{data.name} {ageInfo}";
    }

    private string BuildGenerationLine(CharacterRuntimeData data)
    {
        if (data == null)
        {
            return "族辈: 不详";
        }

        if (data.isExternalSpouse && data.gender == Gender.Female)
        {
            return $"妻室: 第{data.generation}代";
        }

        if (data.isRuzhui || (data.isExternalSpouse && data.gender == Gender.Male))
        {
            return $"赘婿: 第{data.generation}代";
        }

        return $"族辈: 第{data.generation}代";
    }
}

internal static class TextExtensions
{
    public static void SetText(this TextMeshProUGUI text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }
}
