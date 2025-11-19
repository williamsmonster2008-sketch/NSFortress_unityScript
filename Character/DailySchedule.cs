using System.Collections.Generic;
using UnityEngine;

namespace GameSystems
{
    /// <summary>
    /// 时辰行为配置
    /// </summary>
    [System.Serializable]
    public class TimeSlotBehavior
    {
        public TimeOfDay timeOfDay;
        public NPCBehaviorState behavior;
        public string locationTag;  // 目标地点标签 (如 "Home", "WorkField", "Square")
        
        public TimeSlotBehavior(TimeOfDay time, NPCBehaviorState behavior, string location = "")
        {
            this.timeOfDay = time;
            this.behavior = behavior;
            this.locationTag = location;
        }
    }
    
    /// <summary>
    /// NPC日程表 - 定义一天的作息
    /// </summary>
    [System.Serializable]
    public class DailySchedule
    {
        public string scheduleName = "默认作息";
        public List<TimeSlotBehavior> schedule = new List<TimeSlotBehavior>();
        
        /// <summary>
        /// 获取指定时辰的行为
        /// </summary>
        public TimeSlotBehavior GetBehaviorForTime(TimeOfDay timeOfDay)
        {
            foreach (var slot in schedule)
            {
                if (slot.timeOfDay == timeOfDay)
                {
                    return slot;
                }
            }
            
            // 默认行为
            return new TimeSlotBehavior(timeOfDay, NPCBehaviorState.Idle);
        }
        
        /// <summary>
        /// 创建默认农民作息
        /// </summary>
        public static DailySchedule CreateDefaultFarmerSchedule()
        {
            DailySchedule schedule = new DailySchedule();
            schedule.scheduleName = "农民作息";
            
            schedule.schedule.Add(new TimeSlotBehavior(TimeOfDay.黎明, NPCBehaviorState.Sleeping, "Home"));
            schedule.schedule.Add(new TimeSlotBehavior(TimeOfDay.上午, NPCBehaviorState.Working, "WorkField"));
            schedule.schedule.Add(new TimeSlotBehavior(TimeOfDay.正午, NPCBehaviorState.Resting, "Home"));
            schedule.schedule.Add(new TimeSlotBehavior(TimeOfDay.下午, NPCBehaviorState.Working, "WorkField"));
            schedule.schedule.Add(new TimeSlotBehavior(TimeOfDay.黄昏, NPCBehaviorState.Eating, "Home"));
            schedule.schedule.Add(new TimeSlotBehavior(TimeOfDay.夜晚, NPCBehaviorState.Sleeping, "Home"));
            
            return schedule;
        }
        
        /// <summary>
        /// 创建简单测试作息（用于调试）
        /// </summary>
        public static DailySchedule CreateSimpleTestSchedule()
        {
            DailySchedule schedule = new DailySchedule();
            schedule.scheduleName = "测试作息";
            
            schedule.schedule.Add(new TimeSlotBehavior(TimeOfDay.黎明, NPCBehaviorState.Idle));
            schedule.schedule.Add(new TimeSlotBehavior(TimeOfDay.上午, NPCBehaviorState.Working));
            schedule.schedule.Add(new TimeSlotBehavior(TimeOfDay.正午, NPCBehaviorState.Resting));
            schedule.schedule.Add(new TimeSlotBehavior(TimeOfDay.下午, NPCBehaviorState.Working));
            schedule.schedule.Add(new TimeSlotBehavior(TimeOfDay.黄昏, NPCBehaviorState.Eating));
            schedule.schedule.Add(new TimeSlotBehavior(TimeOfDay.夜晚, NPCBehaviorState.Sleeping));
            
            return schedule;
        }
    }
}