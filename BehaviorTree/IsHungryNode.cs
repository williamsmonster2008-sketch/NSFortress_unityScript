using UnityEngine;
using Opsive.BehaviorDesigner.Runtime;
using Opsive.BehaviorDesigner.Runtime.Components;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.BehaviorDesigner.Runtime.Tasks.Conditionals;
using Opsive.GraphDesigner.Runtime;

/// <summary>
/// Day 1 练习：检查NPC是否饥饿
/// 这是你的第一个自定义条件节点
/// </summary>
public class IsHungryNode : Conditional
{
    [Tooltip("饥饿阈值，超过这个值返回Success")]
    public float hungerThreshold = 70f;
    
    private NPCController npc;
    
    public override void OnAwake()
    {
        // 获取NPC组件（只执行一次）
        npc = GetComponent<NPCController>();
        
        if (npc == null)
        {
            Debug.LogError("IsHungryNode: 找不到NPCController组件！");
        }
    }
    
    public override TaskStatus OnUpdate()
    {
        if (npc == null)
        {
            return TaskStatus.Failure;
        }
        
        // 检查饥饿度
        float currentHunger = npc.characterData.physical.hunger;
        
        // 调试信息
        Debug.Log($"[IsHungry] 当前饥饿度: {currentHunger}, 阈值: {hungerThreshold}");
        
        // 饥饿度超过阈值 → Success，否则 → Failure
        if (currentHunger > hungerThreshold)
        {
            return TaskStatus.Success;
        }
        
        return TaskStatus.Failure;
    }
}