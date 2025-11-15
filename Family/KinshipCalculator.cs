using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 亲属关系计算器 - 核心算法
/// </summary>
public class KinshipCalculator : MonoBehaviour
{
    private PathFinder pathFinder;
    private Dictionary<string, CharacterRuntimeData> characters;
    
    public void Initialize(Dictionary<string, CharacterRuntimeData> characterDict)
    {
        characters = characterDict;
        pathFinder = new PathFinder(characters);
    }
    
    /// <summary>
    /// 获取两个角色之间的关系
    /// </summary>
    public KinshipRelation GetRelationship(string fromId, string toId)
    {
        // 1. 查找所有路径
        List<List<PathStep>> allPaths = pathFinder.FindAllPaths(fromId, toId);
        
        if (allPaths == null || allPaths.Count == 0)
        {
            return null; // 无关系
        }
        
        // 2. 选择最优路径
        List<PathStep> bestPath = pathFinder.SelectBestPath(allPaths);
        
        if (bestPath == null)
        {
            return null;
        }
        
        // 3. 分类路径
        PathType pathType = pathFinder.ClassifyPath(bestPath);
        
        // 4. 获取角色数据
        CharacterRuntimeData fromChar = characters[fromId];
        CharacterRuntimeData toChar = characters[toId];
        
        // 5. 根据路径类型计算称谓
        KinshipRelation relation = new KinshipRelation
        {
            pathType = pathType,
            pathLength = bestPath.Count,
            generationDelta = toChar.generation - fromChar.generation,
            generationGap = Mathf.Abs(toChar.generation - fromChar.generation),
            path = bestPath
        };
        
        switch (pathType)
        {
            case PathType.父系:
                DeterminePaternalTitle(relation, fromChar, toChar);
                break;
            case PathType.母系:
                DetermineMaternalTitle(relation, fromChar, toChar);
                break;
            case PathType.姻亲:
                DetermineInLawTitle(relation, fromChar, toChar);
                break;
        }
        
        return relation;
    }
    
    /// <summary>
    /// 计算父系血亲称谓
    /// </summary>
    private void DeterminePaternalTitle(KinshipRelation relation, CharacterRuntimeData from, CharacterRuntimeData to)
    {
        int pathLength = relation.pathLength;
        int gap = relation.generationGap;
        bool isOlder = relation.generationDelta > 0; // 对方是否是长辈
        bool isMale = to.gender == Gender.男;
        
        // 根据pathLength和gap查表
        string baseTitle = GetPaternalBaseTitle(pathLength, gap, isOlder, isMale);
        
        // 添加前缀
        if (pathLength >= 7)
        {
            relation.title = "族" + baseTitle;
        }
        else if (pathLength == 6 && gap == 0)
        {
            relation.title = "从堂" + baseTitle;
        }
        else if (pathLength >= 4 && gap <= 1)
        {
            // 堂兄弟、堂叔等
            if (pathLength == 4 && gap == 0)
            {
                relation.title = "堂" + baseTitle;
            }
            else if (pathLength == 5 && gap == 1)
            {
                relation.title = "堂" + baseTitle;
            }
            else
            {
                relation.title = baseTitle;
            }
        }
        else
        {
            relation.title = baseTitle;
        }
        
        relation.relationType = DetermineRelationType(gap, isOlder);
        relation.closeness = CalculateCloseness(pathLength, PathType.父系);
    }
    
    /// <summary>
    /// 获取父系基础称谓（不含前缀）
    /// </summary>
    private string GetPaternalBaseTitle(int pathLength, int gap, bool isOlder, bool isMale)
    {
        // pathLength = 1: 父子关系
        if (pathLength == 1)
        {
            if (isOlder)
            {
                return isMale ? "父亲" : "母亲";
            }
            else
            {
                return isMale ? "儿子" : "女儿";
            }
        }
        
        // pathLength = 2
        if (pathLength == 2)
        {
            if (gap == 0)
            {
                // 兄弟姐妹
                return GetSiblingTitle(isMale, isOlder);
            }
            else if (gap == 2 && isOlder)
            {
                // 祖父母
                return isMale ? "祖父" : "祖母";
            }
            else if (gap == 2 && !isOlder)
            {
                // 孙子女
                return isMale ? "孙子" : "孙女";
            }
        }
        
        // pathLength = 3
        if (pathLength == 3)
        {
            if (gap == 1 && isOlder)
            {
                // 叔伯姑
                return isMale ? "叔父" : "姑母"; // 简化处理，实际需要区分伯叔
            }
            else if (gap == 1 && !isOlder)
            {
                // 侄子女
                return isMale ? "侄子" : "侄女";
            }
            else if (gap == 3 && isOlder)
            {
                // 曾祖
                return isMale ? "曾祖父" : "曾祖母";
            }
            else if (gap == 3 && !isOlder)
            {
                // 曾孙
                return isMale ? "曾孙" : "曾孙女";
            }
        }
        
        // pathLength = 4
        if (pathLength == 4)
        {
            if (gap == 0)
            {
                // 堂兄弟姐妹
                return GetSiblingTitle(isMale, isOlder);
            }
            else if (gap == 2 && isOlder)
            {
                // 叔祖
                return isMale ? "叔祖父" : "叔祖母";
            }
            else if (gap == 2 && !isOlder)
            {
                // 侄孙
                return isMale ? "侄孙" : "侄孙女";
            }
            else if (gap == 4 && isOlder)
            {
                // 高祖
                return isMale ? "高祖父" : "高祖母";
            }
            else if (gap == 4 && !isOlder)
            {
                // 玄孙
                return isMale ? "玄孙" : "玄孙女";
            }
        }
        
        // pathLength >= 5, 简化处理
        if (gap == 0)
        {
            return GetSiblingTitle(isMale, isOlder);
        }
        else if (isOlder)
        {
            return isMale ? "长辈" : "长辈";
        }
        else
        {
            return isMale ? "晚辈" : "晚辈";
        }
    }
    
    /// <summary>
    /// 获取兄弟姐妹称谓
    /// </summary>
    private string GetSiblingTitle(bool isMale, bool isOlder)
    {
        if (isMale)
        {
            return isOlder ? "兄" : "弟";
        }
        else
        {
            return isOlder ? "姐" : "妹";
        }
    }
    
    /// <summary>
    /// 计算母系血亲称谓（简化版）
    /// </summary>
    private void DetermineMaternalTitle(KinshipRelation relation, CharacterRuntimeData from, CharacterRuntimeData to)
    {
        int pathLength = relation.pathLength;
        int gap = relation.generationGap;
        bool isOlder = relation.generationDelta > 0;
        bool isMale = to.gender == Gender.男;
        
        // 简化实现：外字前缀
        string baseTitle = GetPaternalBaseTitle(pathLength, gap, isOlder, isMale);
        
        if (gap >= 2 && isOlder)
        {
            // 外祖父母等
            relation.title = "外" + baseTitle;
        }
        else if (gap == 0 && pathLength == 4)
        {
            // 表兄弟
            relation.title = "表" + GetSiblingTitle(isMale, isOlder);
        }
        else if (gap == 1 && pathLength == 3)
        {
            // 舅父、姨母
            relation.title = isMale ? "舅父" : "姨母";
        }
        else if (gap == 1 && !isOlder)
        {
            // 外甥
            relation.title = isMale ? "外甥" : "外甥女";
        }
        else
        {
            relation.title = "表" + baseTitle;
        }
        
        relation.relationType = DetermineRelationType(gap, isOlder);
        relation.closeness = CalculateCloseness(pathLength, PathType.母系);
    }
    
    /// <summary>
    /// 计算姻亲称谓（简化版）
    /// </summary>
    private void DetermineInLawTitle(KinshipRelation relation, CharacterRuntimeData from, CharacterRuntimeData to)
    {
        relation.title = "姻亲";
        relation.relationType = "in_law";
        relation.closeness = CalculateCloseness(relation.pathLength, PathType.姻亲);
    }
    
    /// <summary>
    /// 判断关系类型
    /// </summary>
    private string DetermineRelationType(int gap, bool isOlder)
    {
        if (gap == 0)
        {
            return "sibling"; // 同辈
        }
        else if (isOlder)
        {
            return "ancestor"; // 长辈
        }
        else
        {
            return "descendant"; // 晚辈
        }
    }
    
    /// <summary>
    /// 计算亲密度
    /// </summary>
    private int CalculateCloseness(int pathLength, PathType pathType)
    {
        int baseCloseness = 100 - (pathLength * 10);
        
        // 路径类型调整
        switch (pathType)
        {
            case PathType.父系:
                baseCloseness += 0;
                break;
            case PathType.母系:
                baseCloseness -= 10;
                break;
            case PathType.姻亲:
                baseCloseness -= 20;
                break;
        }
        
        return Mathf.Clamp(baseCloseness, 10, 100);
    }
}