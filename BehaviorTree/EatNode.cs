using UnityEngine;
using Opsive.BehaviorDesigner.Runtime;
using Opsive.BehaviorDesigner.Runtime.Components;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.GraphDesigner.Runtime;


/// <summary>
/// Day 1 练习：NPC进食行为
/// 这是你的第一个自定义动作节点
/// </summary>
public class EatNode : Action
{
    [Tooltip("每次进食恢复的饥饿度")]
    public float hungerRecovery = 30f;
    
    [Tooltip("进食持续时间（秒）")]
    public float eatDuration = 2f;
    
    private NPCController npc;
    private float startTime;
    private bool isEating;
    
    public override void OnAwake()
    {
        npc = GetComponent<NPCController>();
        
        if (npc == null)
        {
            Debug.LogError("EatNode: 找不到NPCController组件！");
        }
    }
    
    public override void OnStart()
    {
        // 开始进食
        isEating = true;
        startTime = Time.time;
        
        Debug.Log($"[Eat] {npc.characterData.name} 开始进食...");
        
        // 播放进食动画（如果有Animator的话）
        if (npc.animator != null)
        {
            npc.animator.SetBool("IsEating", true);
        }
    }
    
    public override TaskStatus OnUpdate()
    {
        if (npc == null)
        {
            return TaskStatus.Failure;
        }
        
        // 检查是否进食完成
        float elapsedTime = Time.time - startTime;
        
        if (elapsedTime >= eatDuration)
        {
            // 进食完成，恢复饥饿度
            npc.characterData.physical.hunger -= hungerRecovery;
            npc.characterData.physical.hunger = Mathf.Clamp(
                npc.characterData.physical.hunger, 
                0, 
                100
            );
            
            Debug.Log($"[Eat] {npc.characterData.name} 进食完成！饥饿度: {npc.characterData.physical.hunger}");
            
            return TaskStatus.Success;
        }
        
        // 还在进食中
        return TaskStatus.Running;
    }
    
    public override void OnEnd()
    {
        // 停止进食动画
        if (npc != null && npc.animator != null)
        {
            npc.animator.SetBool("IsEating", false);
        }
        
        isEating = false;
    }
}