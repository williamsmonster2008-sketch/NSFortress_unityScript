/// <summary>
/// 成员年龄数据 - 用于年龄结构生成阶段
/// </summary>
[System.Serializable]
public class MemberAgeData
{
    /// <summary>
    /// 临时ID - 用于建立关系网
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
    /// 辈分 (1=高祖, 2=曾祖, 3=祖辈, 4=父辈, 5=子辈)
    /// </summary>
    public int generation;
    
    /// <summary>
    /// 是否本家成员 (false=外来配偶)
    /// </summary>
    public bool isNative;
    
    /// <summary>
    /// 原籍（外来配偶的来源）
    /// </summary>
    public string originalFamily;
    
    /// <summary>
    /// 外来配偶预先分配的姓氏
    /// </summary>
    public string assignedSurname;
    
    /// <summary>
    /// 外来配偶的社会阶层
    /// </summary>
    public string externalSocialClass;
    
    /// <summary>
    /// 出生顺序 (在兄弟姐妹中的排序)
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
        this.assignedSurname = null;
        this.externalSocialClass = null;
        this.birthOrder = 0;
    }
    
    /// <summary>
    /// 指定原籍时的构造函数
    /// </summary>
    public MemberAgeData(string tempId, int age, Gender gender, int generation, bool isNative, string originalFamily)
    {
        this.tempId = tempId;
        this.age = age;
        this.gender = gender;
        this.generation = generation;
        this.isNative = isNative;
        this.originalFamily = originalFamily;
        this.assignedSurname = null;
        this.externalSocialClass = null;
        this.birthOrder = 0;
    }
}
