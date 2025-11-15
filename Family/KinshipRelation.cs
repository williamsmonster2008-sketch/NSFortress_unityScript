using System;
using System.Collections.Generic;

/// <summary>
/// 完整的亲属关系信息
/// </summary>
[Serializable]
public class KinshipRelation
{
    public string title;                    // 称谓（如"堂兄"）
    public string relationType;             // 关系类型
    public PathType pathType;               // 路径类型
    public int pathLength;                  // 路径长度
    public int generationGap;               // 代际差绝对值
    public int generationDelta;             // 代际差（带正负）
    public int closeness;                   // 亲密度 0-100
    public List<PathStep> path;             // 详细路径（可选）
    
    public KinshipRelation()
    {
        closeness = 50;
        path = new List<PathStep>();
    }
}

/// <summary>
/// 路径类型
/// </summary>
public enum PathType
{
    父系,    // paternal - 只经过父子关系
    母系,    // maternal - 经过母子关系
    姻亲     // in_law - 经过婚姻关系
}