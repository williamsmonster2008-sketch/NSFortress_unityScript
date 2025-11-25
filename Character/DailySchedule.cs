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
        /// 创建默认农民作息（12时辰）
        /// 按你的需求：
        /// 亥时-卯时(21-7时): 睡觉
        /// 辰时(7-9时): eating
        /// 巳时(9-11时): idle
        /// 午时-未时(11-15时): working
        /// 申时(15-17时): resting
        /// 酉时(17-19时): eating
        /// 戌时(19-21时): idle
        /// </summary>
        public static DailySchedule CreateDefaultFarmerSchedule()
        {
            DailySchedule schedule = new DailySchedule();
            schedule.scheduleName = "农民作息";
            
            // 亥时-卯时: 睡觉 (21-7时，共5个时辰)
            schedule.schedule.Add(new TimeSlotBehavior(TimeOfDay.亥时, NPCBehaviorState.Sleeping, "Home"));
            schedule.schedule.Add(new TimeSlotBehavior(TimeOfDay.子时, NPCBehaviorState.Sleeping, "Home"));
            schedule.schedule.Add(new TimeSlotBehavior(TimeOfDay.丑时, NPCBehaviorState.Sleeping, "Home"));
            schedule.schedule.Add(new TimeSlotBehavior(TimeOfDay.寅时, NPCBehaviorState.Sleeping, "Home"));
            schedule.schedule.Add(new TimeSlotBehavior(TimeOfDay.卯时, NPCBehaviorState.Sleeping, "Home"));
            
            // 辰时: eating (7-9时)
            schedule.schedule.Add(new TimeSlotBehavior(TimeOfDay.辰时, NPCBehaviorState.Eating, "Home"));
            
            // 巳时: idle (9-11时)
            schedule.schedule.Add(new TimeSlotBehavior(TimeOfDay.巳时, NPCBehaviorState.Idle));
            
            // 午时-未时: working (11-15时，2个时辰)
            schedule.schedule.Add(new TimeSlotBehavior(TimeOfDay.午时, NPCBehaviorState.Working, "WorkField"));
            schedule.schedule.Add(new TimeSlotBehavior(TimeOfDay.未时, NPCBehaviorState.Working, "WorkField"));
            
            // 申时: resting (15-17时)
            schedule.schedule.Add(new TimeSlotBehavior(TimeOfDay.申时, NPCBehaviorState.Resting, "Home"));
            
            // 酉时: eating (17-19时)
            schedule.schedule.Add(new TimeSlotBehavior(TimeOfDay.酉时, NPCBehaviorState.Eating, "Home"));
            
            // 戌时: idle (19-21时)
            schedule.schedule.Add(new TimeSlotBehavior(TimeOfDay.戌时, NPCBehaviorState.Idle));
            
            return schedule;
        }
        
        /// <summary>
        /// 创建简单测试作息（12时辰）
        /// </summary>
        public static DailySchedule CreateSimpleTestSchedule()
        {
            DailySchedule schedule = new DailySchedule();
            schedule.scheduleName = "测试作息";
            
            // 使用默认农民作息
            return CreateDefaultFarmerSchedule();
        }
    }
}