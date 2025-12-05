using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.BehaviorDesigner.Runtime.Tasks.Conditionals;
using Opsive.GraphDesigner.Runtime;
using GameSystems;

public class IsSuggestedBehaviorNode : Conditional
{
    public NPCBehaviorState targetBehavior;
    
    private NPCController npc;
    
    public override void OnAwake()
    {
        npc = GetComponent<NPCController>();
    }
    
    public override TaskStatus OnUpdate()
    {
        if (npc == null)
        {
            return TaskStatus.Failure;
        }
        
        return npc.suggestedBehavior == targetBehavior 
            ? TaskStatus.Success 
            : TaskStatus.Failure;
    }
}