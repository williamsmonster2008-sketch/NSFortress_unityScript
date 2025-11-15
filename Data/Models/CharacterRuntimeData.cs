using System;
using System.Collections.Generic; // 添加这行
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public class CharacterRuntimeData
{
    [ReadOnly] public string characterId;
    public string name;
    public string familyName;
    public int age;
    public Gender gender;
    
    [FoldoutGroup("家族")]
    public string fatherId;           // 父亲ID
    public string motherId;           // 母亲ID
    public string spouseId;           // 配偶ID
    public List<string> childrenIds = new List<string>(); // 子女ID列表
    public int generation = 1;        // 代数（第几代）
    
    /// <summary>
    /// 生命状态 ("living" 或 "deceased")
    /// </summary>
    public string vitalStatus = "living";
    
    [FoldoutGroup("状态")]
    public PhysicalState physical;
    
    [FoldoutGroup("情绪")]
    public EmotionalState emotional;
    
    [FoldoutGroup("人格")]
    public VirtueTraits virtues;
    
    [FoldoutGroup("技能")]
    // 改用普通List，更简单
    public List<CharacterSkill> skills = new List<CharacterSkill>();
    
    [FoldoutGroup("世界")]
    public Vector3 position;
    public string currentLocation;
}

// 添加技能数据类
[Serializable]
public class CharacterSkill
{
    public string skillName;
    [Range(0, 100)]
    public int skillLevel;
}

public enum Gender { 男, 女 }