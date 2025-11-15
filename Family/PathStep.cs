using System;

/// <summary>
/// 路径中的一步
/// </summary>
[Serializable]
public class PathStep
{
    public string from;              // 起点角色ID
    public string to;                // 终点角色ID
    public RelationType relationType; // 关系类型
    public Gender gender;             // 这一步经过的人的性别
    public bool throughMarriage;      // 是否通过婚姻跳转
    
    public PathStep(string from, string to, RelationType relationType, Gender gender, bool throughMarriage = false)
    {
        this.from = from;
        this.to = to;
        this.relationType = relationType;
        this.gender = gender;
        this.throughMarriage = throughMarriage;
    }
}

/// <summary>
/// 基础关系类型（用于路径构建）
/// </summary>
public enum RelationType
{
    父子,      // father_child
    母子,      // mother_child
    配偶,      // spouse
    兄弟姐妹   // sibling
}