using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 右侧人物信息面板
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
        
        familyText?.SetText($"家族：{character.familyName}氏");
        nameText?.SetText($"姓名：{character.name}");
        ageText?.SetText($"年龄：{character.age}岁");
        genderText?.SetText($"性别：{(character.gender == Gender.Male ? "男" : "女")}");
        generationText?.SetText($"代际：第{character.generation}代");
        
        fatherText?.SetText(BuildParentInfo("父亲", character.fatherId));
        motherText?.SetText(BuildParentInfo("母亲", character.motherId));
        siblingsText?.SetText(BuildRelativeList("兄弟姐妹", CollectSiblings(character)));
        childrenText?.SetText(BuildRelativeList("子女", CollectChildren(character)));
    }
    
    private string BuildParentInfo(string label, string parentId)
    {
        if (string.IsNullOrEmpty(parentId))
        {
            return $"{label}：未知";
        }
        
        var parent = familySystem?.GetCharacter(parentId);
        if (parent == null)
        {
            return $"{label}：未知";
        }
        
        bool living = parent.vitalStatus == "living";
        string status = living ? "在世" : "已故";
        string ageInfo = living ? $"{parent.age}岁" : $"享年{parent.age}岁";
        
        return $"{label}：{parent.name}（{status}，{ageInfo}）";
    }
    
    private string BuildRelativeList(string label, List<string> relatives)
    {
        if (relatives == null || relatives.Count == 0)
        {
            return $"{label}：无";
        }
        
        return $"{label}：{string.Join("、", relatives)}";
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
                    result.Add(BuildBasicLine(child));
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
                list.Add(BuildBasicLine(child));
            }
        }
        return list;
    }
    
    private string BuildBasicLine(CharacterRuntimeData data)
    {
        bool living = data.vitalStatus == "living";
        string status = living ? "在世" : "已故";
        string ageInfo = living ? $"{data.age}岁" : $"享年{data.age}岁";
        return $"{data.name}（{status}，{ageInfo}）";
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
