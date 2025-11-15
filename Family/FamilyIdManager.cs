/// <summary>
/// 家族ID管理器 - 负责生成临时ID
/// 临时ID在生成过程中用于建立关系,最后转换为真实角色ID
/// </summary>
public class FamilyIdManager
{
    private int counter = 0;
    
    /// <summary>
    /// 分配一个新的临时ID
    /// </summary>
    /// <param name="role">角色描述(可选,用于调试)</param>
    /// <returns>临时ID</returns>
    public string AllocateId(string role = "member")
    {
        counter++;
        return $"temp_{role}_{counter}";
    }
    
    /// <summary>
    /// 重置计数器
    /// </summary>
    public void Reset()
    {
        counter = 0;
    }
}