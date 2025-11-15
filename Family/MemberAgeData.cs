/// <summary>
/// 成员年龄数据 - 用于年龄结构生成阶段
/// </summary>
[System.Serializable]
public class MemberAgeData
{
    /// <summary>
    /// 临时ID - 用于建立关系网络
    /// </summary>
    public string tempId;
    
    /// <summary>
    /// 年龄
    /// </summary>
    public int age;
    
    /// <summary>
    /// 性别
    /// </summary>
    public Gender gender;
    
    /// <summary>
    /// 代际 (1=高祖, 2=曾祖, 3=祖父母, 4=父母, 5=子女)
    /// </summary>
    public int generation;
    
    /// <summary>
    /// 是否本家成员 (false=外来配偶)
    /// </summary>
    public bool isNative;
    
    /// <summary>
    /// 原生家族姓氏 (外来配偶的原姓)
    /// </summary>
    public string originalFamily;
    
    /// <summary>
    /// 出生顺序 (同辈兄弟姐妹中的排行)
    /// </summary>
    public int birthOrder;
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public MemberAgeData(string tempId, int age, Gender gender, int generation, bool isNative)
    {
        this.tempId = tempId;
        this.age = age;
        this.gender = gender;
        this.generation = generation;
        this.isNative = isNative;
        this.originalFamily = null;
        this.birthOrder = 0;
    }
    
    /// <summary>
    /// 带原生家族的构造函数
    /// </summary>
    public MemberAgeData(string tempId, int age, Gender gender, int generation, bool isNative, string originalFamily)
    {
        this.tempId = tempId;
        this.age = age;
        this.gender = gender;
        this.generation = generation;
        this.isNative = isNative;
        this.originalFamily = originalFamily;
        this.birthOrder = 0;
    }
}