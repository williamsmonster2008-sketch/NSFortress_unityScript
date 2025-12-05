using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.GraphDesigner.Runtime;

public class RestNode : Action
{
    public float restDuration = 5f;
    
    private NPCController npc;
    private float startTime;
    
    public override void OnAwake()
    {
        npc = GetComponent<NPCController>();
    }
    
    public override void OnStart()
    {
        startTime = Time.time;
        Debug.Log($"[Rest] {npc.characterData.name} 开始休息...");
        
        if (npc.animator != null)
        {
            npc.animator.SetBool("IsResting", true);
        }
    }
    
    public override TaskStatus OnUpdate()
    {
        if (Time.time - startTime >= restDuration)
        {
            Debug.Log($"[Rest] {npc.characterData.name} 休息完成");
            return TaskStatus.Success;
        }
        
        return TaskStatus.Running;
    }
    
    public override void OnEnd()
    {
        if (npc?.animator != null)
        {
            npc.animator.SetBool("IsResting", false);
        }
    }
}