using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.GraphDesigner.Runtime;

public class SleepNode : Action
{
    private NPCController npc;
    
    public override void OnAwake()
    {
        npc = GetComponent<NPCController>();
    }
    
    public override void OnStart()
    {
        Debug.Log($"[Sleep] {npc.characterData.name} 开始睡觉...");
        // 禁用RandomWalk
        var randomWalk = npc.GetComponent<NPCRandomWalk>();
        if (randomWalk != null)
        {
            randomWalk.enabled = false;
        }
        
        // 停止移动
        var agent = npc.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }

        if (npc.animator != null)
        {
            npc.animator.SetBool("IsSleeping", true);
        }
    }
    
    public override TaskStatus OnUpdate()
    {
        // 睡眠是持续状态，一直返回Running
        // 直到时辰改变，Sequence被中断
        return TaskStatus.Running;
    }
    
    public override void OnEnd()
    {
        Debug.Log($"[Sleep] {npc.characterData.name} 醒来");
        
        // 恢复RandomWalk和移动
        var randomWalk = npc.GetComponent<NPCRandomWalk>();
        if (randomWalk != null)
        {
            randomWalk.enabled = true;
        }
        
        if (npc?.animator != null)
        {
            npc.animator.SetBool("IsSleeping", false);
        }
    }
}