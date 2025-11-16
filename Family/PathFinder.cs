using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 路径搜索器 - 使用BFS查找两人之间的最短路径
/// </summary>
public class PathFinder
{
    private Dictionary<string, CharacterRuntimeData> characters;
    
    public PathFinder(Dictionary<string, CharacterRuntimeData> characterDict)
    {
        characters = characterDict;
    }
    
    /// <summary>
    /// 查找两个角色之间的所有路径
    /// </summary>
    public List<List<PathStep>> FindAllPaths(string fromId, string toId, int maxDepth = 10)
    {
        List<List<PathStep>> allPaths = new List<List<PathStep>>();
        
        if (!characters.ContainsKey(fromId) || !characters.ContainsKey(toId))
        {
            return allPaths;
        }
        
        if (fromId == toId)
        {
            return allPaths; // 自己和自己没有路径
        }
        
        // BFS搜索
        Queue<PathSearchNode> queue = new Queue<PathSearchNode>();
        queue.Enqueue(new PathSearchNode
        {
            currentId = fromId,
            path = new List<PathStep>(),
            visited = new HashSet<string> { fromId }
        });
        
        while (queue.Count > 0)
        {
            PathSearchNode node = queue.Dequeue();
            
            // 深度限制
            if (node.path.Count >= maxDepth)
            {
                continue;
            }
            
            CharacterRuntimeData current = characters[node.currentId];
            
            // 获取所有相邻节点
            List<PathStep> neighbors = GetNeighbors(current);
            
            foreach (PathStep step in neighbors)
            {
                // 避免循环
                if (node.visited.Contains(step.to))
                {
                    continue;
                }
                
                // 构建新路径
                List<PathStep> newPath = new List<PathStep>(node.path);
                newPath.Add(step);
                
                // 找到目标
                if (step.to == toId)
                {
                    allPaths.Add(newPath);
                    continue; // 继续搜索其他路径
                }
                
                // 继续搜索
                HashSet<string> newVisited = new HashSet<string>(node.visited);
                newVisited.Add(step.to);
                
                queue.Enqueue(new PathSearchNode
                {
                    currentId = step.to,
                    path = newPath,
                    visited = newVisited
                });
            }
        }
        
        return allPaths;
    }
    
    /// <summary>
    /// 获取一个角色的所有相邻关系
    /// </summary>
    private List<PathStep> GetNeighbors(CharacterRuntimeData character)
    {
        List<PathStep> neighbors = new List<PathStep>();
        
        // 父亲
        if (!string.IsNullOrEmpty(character.fatherId) && characters.ContainsKey(character.fatherId))
        {
            neighbors.Add(new PathStep(
                character.characterId,
                character.fatherId,
                RelationType.父子,
                Gender.Male,
                false
            ));
        }
        
        // 母亲
        if (!string.IsNullOrEmpty(character.motherId) && characters.ContainsKey(character.motherId))
        {
            neighbors.Add(new PathStep(
                character.characterId,
                character.motherId,
                RelationType.母子,
                Gender.Female,
                false
            ));
        }
        
        // 配偶
        if (!string.IsNullOrEmpty(character.spouseId) && characters.ContainsKey(character.spouseId))
        {
            CharacterRuntimeData spouse = characters[character.spouseId];
            neighbors.Add(new PathStep(
                character.characterId,
                character.spouseId,
                RelationType.配偶,
                spouse.gender,
                true // 婚姻是跳转
            ));
        }
        
        // 子女
        foreach (string childId in character.childrenIds)
        {
            if (characters.ContainsKey(childId))
            {
                CharacterRuntimeData child = characters[childId];
                neighbors.Add(new PathStep(
                    character.characterId,
                    childId,
                    character.gender == Gender.Male ? RelationType.父子 : RelationType.母子,
                    child.gender,
                    false
                ));
            }
        }
        
        return neighbors;
    }
    
    /// <summary>
    /// 选择最优路径
    /// </summary>
    public List<PathStep> SelectBestPath(List<List<PathStep>> paths)
    {
        if (paths == null || paths.Count == 0)
        {
            return null;
        }
        
        // 路径分类和打分
        List<PathStep> bestPath = null;
        int bestScore = int.MinValue;
        
        foreach (var path in paths)
        {
            PathType type = ClassifyPath(path);
            int score = CalculatePathScore(path, type);
            
            if (score > bestScore)
            {
                bestScore = score;
                bestPath = path;
            }
        }
        
        return bestPath;
    }
    
    /// <summary>
    /// 路径分类
    /// </summary>
    public PathType ClassifyPath(List<PathStep> path)
    {
        bool hasFemaleAncestor = false;
        bool hasMarriage = false;
        
        foreach (PathStep step in path)
        {
            // 检查是否通过婚姻
            if (step.throughMarriage)
            {
                hasMarriage = true;
                break;
            }
            
            // 检查是否经过母系（向上追溯时遇到母子关系）
            if (step.relationType == RelationType.母子)
            {
                hasFemaleAncestor = true;
            }
        }
        
        if (hasMarriage) return PathType.姻亲;
        if (hasFemaleAncestor) return PathType.母系;
        return PathType.父系;
    }
    
    /// <summary>
    /// 计算路径得分（用于选择最优路径）
    /// </summary>
    private int CalculatePathScore(List<PathStep> path, PathType type)
    {
        int score = 0;
        
        // 路径类型优先级：父系 > 母系 > 姻亲
        switch (type)
        {
            case PathType.父系:
                score += 1000;
                break;
            case PathType.母系:
                score += 500;
                break;
            case PathType.姻亲:
                score += 100;
                break;
        }
        
        // 路径越短越好
        score -= path.Count * 10;
        
        return score;
    }
}

/// <summary>
/// 路径搜索节点（内部使用）
/// </summary>
internal class PathSearchNode
{
    public string currentId;
    public List<PathStep> path;
    public HashSet<string> visited;
}
