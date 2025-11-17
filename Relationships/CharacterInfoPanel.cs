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
    public TextMeshProUGUI topVirtuesText;
    
    [Header("动态亲属列表")]
    public Transform siblingsContainer;
    public Transform childrenContainer;
    public GameObject relativeItemPrefab; // 需要一个包含3个TMP的预制：姓名/年龄/称谓
    public TextMeshProUGUI siblingsEmptyText;
    public TextMeshProUGUI childrenEmptyText;
    
    private FamilySystem familySystem;
    
    public void ShowCharacter(CharacterRuntimeData character, FamilySystem system)
    {
        familySystem = system;
        
        if (panelRoot != null)
        {
            panelRoot.SetActive(character != null);
        }
        if (character == null)
        {
            return;
        }
        
        familyText?.SetText($"家族: {character.familyName}");
        nameText?.SetText($"{character.name}");
        ageText?.SetText($"{character.age}岁");
        genderText?.SetText($"{(character.gender == Gender.Male ? "男" : "女")}");
        generationText?.SetText(BuildGenerationLine(character));
        
        fatherText?.SetText(BuildParentInfo("父亲", character.fatherId));
        motherText?.SetText(BuildParentInfo("母亲", character.motherId));
        
        RenderRelatives(siblingsContainer, siblingsEmptyText, CollectSiblings(character), isChildList: false);
        RenderRelatives(childrenContainer, childrenEmptyText, CollectChildren(character), isChildList: true);
        
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
    
    private void RenderRelatives(Transform container, TextMeshProUGUI emptyText, List<CharacterRuntimeData> relatives, bool isChildList)
    {
        // 清空旧项
        if (container != null)
        {
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
        }
        
        bool hasData = relatives != null && relatives.Count > 0;
        if (emptyText != null)
        {
            emptyText.gameObject.SetActive(!hasData);
            if (!hasData)
            {
                emptyText.SetText(isChildList ? "子女：无" : "手足：无");
            }
        }
        if (!hasData || container == null)
        {
            return;
        }
        
        // 排序：按年龄降序
        var sorted = relatives
            .Where(r => r != null)
            .OrderByDescending(r => r.age)
            .ToList();
        
        for (int i = 0; i < sorted.Count; i++)
        {
            CreateRelativeItem(container, sorted[i], i, isChildList);
        }
    }
    
    private void CreateRelativeItem(Transform container, CharacterRuntimeData data, int index, bool isChildList)
    {
        if (relativeItemPrefab == null || container == null)
        {
            // 无预制体时，降级为文本拼接
            string title = isChildList ? GetChildTitle(index, data.gender) : GetSiblingTitle(index, data.gender);
            var line = $"{BuildBasicLine(data)} {title}";
            var fallbackText = isChildList ? childrenEmptyText : siblingsEmptyText;
            if (fallbackText != null)
            {
                // 追加行显示
                string prefix = fallbackText.text ?? string.Empty;
                fallbackText.SetText(string.IsNullOrEmpty(prefix) ? line : $"{prefix}\n{line}");
                fallbackText.gameObject.SetActive(true);
            }
            return;
        }
        
        var go = Instantiate(relativeItemPrefab, container);
        var texts = go.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts != null && texts.Length >= 3)
        {
            texts[0].SetText(data.name);
            bool living = data.vitalStatus == "living";
            string ageInfo = living ? $"{data.age}岁" : $"殁年{data.age}岁";
            texts[1].SetText(ageInfo);
            texts[2].SetText(isChildList ? GetChildTitle(index, data.gender) : GetSiblingTitle(index, data.gender));
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
        
        // 父母只显示姓名 + 年龄/殁年
        return $"{label}: {BuildBasicLine(parent)}";
    }
    
    private List<CharacterRuntimeData> CollectSiblings(CharacterRuntimeData character)
    {
        HashSet<string> ids = new HashSet<string>();
        var result = new List<CharacterRuntimeData>();
        
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
                    result.Add(child);
                }
            }
        }
        
        AddFromParent(character.fatherId);
        AddFromParent(character.motherId);
        return result;
    }
    
    private List<CharacterRuntimeData> CollectChildren(CharacterRuntimeData character)
    {
        var list = new List<CharacterRuntimeData>();
        if (character.childrenIds == null || character.childrenIds.Count == 0)
        {
            return list;
        }
        
        foreach (var childId in character.childrenIds)
        {
            var child = familySystem?.GetCharacter(childId);
            if (child != null)
            {
                list.Add(child);
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

    private string GetSiblingTitle(int index, Gender gender)
    {
        string[] ranks = { "大", "二", "三", "四", "五", "六", "七", "八", "九" };
        string rank = index < ranks.Length ? ranks[index] : $"{index + 1}";
        return gender == Gender.Male ? $"{rank}哥" : $"{rank}姐";
    }

    private string GetChildTitle(int index, Gender gender)
    {
        string[] ranks = { "长", "次", "三", "四", "五", "六", "七", "八", "九" };
        string rank = index < ranks.Length ? ranks[index] : $"{index + 1}";
        return gender == Gender.Male ? $"{rank}子" : $"{rank}女";
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
