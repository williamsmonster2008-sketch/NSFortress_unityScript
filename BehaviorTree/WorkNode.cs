using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.GraphDesigner.Runtime;

public class WorkNode : Action
{
    public float workDuration = 8f;
    
    private NPCController npc;
    private float startTime;
    
    public override void OnAwake()
    {
        npc = GetComponent<NPCController>();
    }
    
    public override void OnStart()
    {
        startTime = Time.time;
        Debug.Log($"[Work] {npc.characterData.name} 开始工作...");
        
        if (npc.animator != null)
        {
            npc.animator.SetBool("IsWorking", true);
        }
    }
    
    public override TaskStatus OnUpdate()
    {
        if (Time.time - startTime >= workDuration)
        {
            Debug.Log($"[Work] {npc.characterData.name} 工作完成");
            return TaskStatus.Success;
        }
        
        return TaskStatus.Running;
    }
    
    public override void OnEnd()
    {
        if (npc?.animator != null)
        {
            npc.animator.SetBool("IsWorking", false);
        }
    }
}