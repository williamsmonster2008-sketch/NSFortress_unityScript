using UnityEngine;
using Opsive.BehaviorDesigner.Runtime;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.BehaviorDesigner.Runtime.Tasks.Conditionals;
using Opsive.GraphDesigner.Runtime;
using GameSystems;

public class IsTimeOfDayNode : Conditional
{
    public TimeOfDay targetTime;
    
    private TimeSystem timeSystem;
    
    public override void OnAwake()
    {
        timeSystem = Object.FindObjectOfType<TimeSystem>();
        
        if (timeSystem == null)
        {
            Debug.LogError("IsTimeOfDayNode: 找不到TimeSystem！");
        }
    }
    
    public override TaskStatus OnUpdate()
    {
        if (timeSystem == null)
        {
            return TaskStatus.Failure;
        }
        
        TimeOfDay currentTime = timeSystem.GetCurrentTimeOfDay();
        
        return currentTime == targetTime 
            ? TaskStatus.Success 
            : TaskStatus.Failure;
    }
}