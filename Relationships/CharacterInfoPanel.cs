using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

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
    public TextMeshProUGUI bloodlineText;   // 手足+子女
    public TextMeshProUGUI topVirtuesText;
    
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
        nameText?.SetText(character.name);
        ageText?.SetText($"{character.age}岁");
        genderText?.SetText(character.gender == Gender.Male ? "男" : "女");
        generationText?.SetText(BuildGenerationLine(character));
        
        fatherText?.SetText(BuildParentInfo("父亲", character.fatherId));
        motherText?.SetText(BuildParentInfo("母亲", character.motherId));
        
        if (bloodlineText != null)
        {
            string siblingsBlock = BuildRelativeBlock("手足", CollectSiblings(character), false, character);
            string childrenBlock = BuildRelativeBlock("子女", CollectChildren(character), true, character);
            bloodlineText.SetText($"{siblingsBlock}\n{childrenBlock}");
        }
        
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
        
        string line = BuildBasicLineRaw(parent, out bool living);
        if (!living)
        {
            line = WrapDeceased(line);
        }
        return $"{label}: {line}";
    }
    
    private string BuildRelativeBlock(string title, List<CharacterRuntimeData> relatives, bool isChildList, CharacterRuntimeData self)
    {
        if (relatives == null || relatives.Count == 0)
        {
            return $"{title}: 无";
        }
        
        var sorted = relatives.Where(r => r != null)
            .OrderByDescending(r => r.age)
            .ToList();
        
        Dictionary<string, int> olderIndex = new Dictionary<string, int>();
        Dictionary<string, int> youngerIndex = new Dictionary<string, int>();
        if (!isChildList && self != null)
        {
            var olderGroup = sorted.Where(r => r.age > self.age).ToList();
            var youngerGroup = sorted.Where(r => r.age <= self.age).ToList();
            for (int i = 0; i < olderGroup.Count; i++)
            {
                string key = olderGroup[i].characterId ?? olderGroup[i].name;
                olderIndex[key] = i;
            }
            for (int i = 0; i < youngerGroup.Count; i++)
            {
                string key = youngerGroup[i].characterId ?? youngerGroup[i].name;
                youngerIndex[key] = i;
            }
        }
        
        List<string> lines = new List<string> { $"{title}:" };
        for (int i = 0; i < sorted.Count; i++)
        {
            var r = sorted[i];
            string baseLine = BuildBasicLineRaw(r, out bool living);
            string fullLine = isChildList
                ? $"{baseLine} {GetChildTitle(i, r.gender)}"
                : $"{baseLine} {GetSiblingTitleRelative(r, self, olderIndex, youngerIndex)}";
            if (!living)
            {
                fullLine = WrapDeceased(fullLine);
            }
            lines.Add($"  - {fullLine}");
        }
        return string.Join("\n", lines);
    }
    
    private string BuildBasicLineRaw(CharacterRuntimeData data, out bool living)
    {
        living = true;
        if (data == null)
        {
            return "不详";
        }
        living = data.vitalStatus == "living";
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
    
    private string GetChildTitle(int index, Gender gender)
    {
        string[] ranks = { "长", "次", "三", "四", "五", "六", "七", "八", "九" };
        string rank = index < ranks.Length ? ranks[index] : $"{index + 1}";
        return gender == Gender.Male ? $"{rank}子" : $"{rank}女";
    }
    
    private string GetSiblingTitleRelative(CharacterRuntimeData sibling, CharacterRuntimeData self, Dictionary<string, int> olderIndex, Dictionary<string, int> youngerIndex)
    {
        if (self == null || sibling == null)
        {
            return GetSiblingTitle(0, sibling != null ? sibling.gender : Gender.Male);
        }
        
        string id = sibling.characterId ?? sibling.name;
        bool older = sibling.age > self.age;
        string[] ranks = { "大", "二", "三", "四", "五", "六", "七", "八", "九" };
        
        int idx = 0;
        if (older)
        {
            if (olderIndex != null && olderIndex.TryGetValue(id, out var i))
            {
                idx = i;
            }
            string rank = idx < ranks.Length ? ranks[idx] : $"{idx + 1}";
            return sibling.gender == Gender.Male ? $"{rank}哥" : $"{rank}姐";
        }
        else
        {
            if (youngerIndex != null && youngerIndex.TryGetValue(id, out var i))
            {
                idx = i;
            }
            string rank = idx < ranks.Length ? ranks[idx] : $"{idx + 1}";
            return sibling.gender == Gender.Male ? $"{rank}弟" : $"{rank}妹";
        }
    }
    
    private string GetSiblingTitle(int index, Gender gender)
    {
        string[] ranks = { "大", "二", "三", "四", "五", "六", "七", "八", "九" };
        string rank = index < ranks.Length ? ranks[index] : $"{index + 1}";
        return gender == Gender.Male ? $"{rank}哥" : $"{rank}姐";
    }
    
    private string WrapDeceased(string line)
    {
        return $"<color=#A9E7E3>{line}</color>";
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
