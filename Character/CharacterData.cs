using System;
using UnityEngine;

[Serializable]
public class CharacterData
{
    public string characterId;
    public string name;
    public int age;
    public string gender;
    public string socialClass;
    public string familyName;
    public Vector3 spawnPosition;
    
    // 从JS数据迁移的构造函数
    public CharacterData(string id, string charName, int charAge, string charGender)
    {
        characterId = id;
        name = charName;
        age = charAge;
        gender = charGender;
        socialClass = "平民";
        spawnPosition = Vector3.zero;
    }
}