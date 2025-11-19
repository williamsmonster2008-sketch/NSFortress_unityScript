using UnityEngine;

namespace GameSystems
{
    /// <summary>
    /// NPC行为状态
    /// </summary>
    public enum NPCBehaviorState
    {
        Idle,       // 待机
        Sleeping,   // 睡觉
        Eating,     // 进食
        Working,    // 工作（通用）
        Resting,    // 休息
        Walking     // 行走
    }
    
    /// <summary>
    /// 行为状态的配置数据
    /// </summary>
    [System.Serializable]
    public class BehaviorStateConfig
    {
        public NPCBehaviorState state;
        public string animationTrigger;     // 动画触发器名称
        public string animationBool;        // 动画布尔参数名称
        public bool requiresDestination;    // 是否需要移动到目的地
        public float energyCost;            // 能量消耗/恢复（负数=恢复）
        public float hungerCost;            // 饥饿度变化
        
        public BehaviorStateConfig(NPCBehaviorState state, string animTrigger = "", string animBool = "")
        {
            this.state = state;
            this.animationTrigger = animTrigger;
            this.animationBool = animBool;
            this.requiresDestination = false;
            this.energyCost = 0f;
            this.hungerCost = 0f;
        }
    }
    
    /// <summary>
    /// 行为状态配置数据库
    /// </summary>
    public static class BehaviorStateDatabase
    {
        public static BehaviorStateConfig GetConfig(NPCBehaviorState state)
        {
            switch (state)
            {
                case NPCBehaviorState.Idle:
                    return new BehaviorStateConfig(state, "", "")
                    {
                        requiresDestination = false,
                        energyCost = -0.01f,  // 缓慢恢复
                        hungerCost = 0.05f
                    };
                    
                case NPCBehaviorState.Sleeping:
                    return new BehaviorStateConfig(state, "Sleep", "IsSleeping")
                    {
                        requiresDestination = true,  // 需要回家
                        energyCost = -0.2f,  // 快速恢复
                        hungerCost = 0.02f
                    };
                    
                case NPCBehaviorState.Eating:
                    return new BehaviorStateConfig(state, "Eat", "IsEating")
                    {
                        requiresDestination = true,
                        energyCost = -0.05f,
                        hungerCost = -0.3f  // 恢复饥饿度
                    };
                    
                case NPCBehaviorState.Working:
                    return new BehaviorStateConfig(state, "Work", "IsWorking")
                    {
                        requiresDestination = true,
                        energyCost = 0.1f,  // 消耗能量
                        hungerCost = 0.15f
                    };
                    
                case NPCBehaviorState.Resting:
                    return new BehaviorStateConfig(state, "Rest", "IsResting")
                    {
                        requiresDestination = false,
                        energyCost = -0.1f,
                        hungerCost = 0.03f
                    };
                    
                case NPCBehaviorState.Walking:
                    return new BehaviorStateConfig(state, "", "")
                    {
                        requiresDestination = true,
                        energyCost = 0.05f,
                        hungerCost = 0.08f
                    };
                    
                default:
                    return new BehaviorStateConfig(state);
            }
        }
    }
}