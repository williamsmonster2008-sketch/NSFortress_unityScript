using System;
using UnityEngine;

namespace GameSystems
{
    [System.Serializable]
    public class TimeConfig
    {
        public float secondsPerDay = 225f;      // 现实225秒=游戏1天
        public int daysPerSolarTerm = 3;
        public int solarTermsPerMonth = 2;
        public int monthsPerSeason = 3;
        public int seasonsPerYear = 4;
        public int timeStepsPerDay = 6;         // 6个时辰
    }
    
    /// <summary>
    /// 时辰枚举
    /// </summary>
    public enum TimeOfDay
    {
        黎明 = 0,  // 5-7时 (0.0-0.166)
        上午 = 1,  // 7-11时 (0.166-0.333)
        正午 = 2,  // 11-13时 (0.333-0.5)
        下午 = 3,  // 13-17时 (0.5-0.666)
        黄昏 = 4,  // 17-19时 (0.666-0.833)
        夜晚 = 5   // 19-5时 (0.833-1.0)
    }

    public class TimeSystem : MonoBehaviour
    {
        [Header("时间配置")]
        public TimeConfig config = new TimeConfig();
        
        [Header("当前状态")]
        public float totalSeconds = 0f;
        public float timeScale = 1f;
        public bool isPaused = false;
        
        // 事件
        public event Action<TimeOfDay> OnTimeOfDayChanged;
        public event Action<int> OnNewDay;
        public event Action<string> OnNewSeason;
        
        private TimeOfDay currentTimeOfDay = TimeOfDay.黎明;
        private int currentDay = 0;
        
        void Update()
        {
            if (!isPaused)
            {
                float previousSeconds = totalSeconds;
                totalSeconds += Time.deltaTime * timeScale;
                
                CheckTimeEvents(previousSeconds);
            }
        }
        
        void CheckTimeEvents(float previousSeconds)
        {
            // 检测时辰变化
            TimeOfDay newTimeOfDay = GetCurrentTimeOfDay();
            if (newTimeOfDay != currentTimeOfDay)
            {
                currentTimeOfDay = newTimeOfDay;
                OnTimeOfDayChanged?.Invoke(currentTimeOfDay);
                Debug.Log($"⏰ 时辰变化: {currentTimeOfDay} ({GetTimeOfDayDescription()})");
            }
            
            // 检测新一天
            int newDay = Mathf.FloorToInt(totalSeconds / config.secondsPerDay);
            if (newDay != currentDay)
            {
                currentDay = newDay;
                OnNewDay?.Invoke(currentDay);
                Debug.Log($"📅 新的一天: 第{currentDay}天");
            }
        }
        
        /// <summary>
        /// 获取当前时辰
        /// </summary>
        public TimeOfDay GetCurrentTimeOfDay()
        {
            float dayProgress = GetDayProgress();
            int index = Mathf.FloorToInt(dayProgress * config.timeStepsPerDay);
            index = Mathf.Clamp(index, 0, config.timeStepsPerDay - 1);
            return (TimeOfDay)index;
        }
        
        /// <summary>
        /// 获取一天进度 (0-1)
        /// </summary>
        public float GetDayProgress()
        {
            float secondsInDay = totalSeconds % config.secondsPerDay;
            return secondsInDay / config.secondsPerDay;
        }
        
        /// <summary>
        /// 获取时辰描述
        /// </summary>
        public string GetTimeOfDayDescription()
        {
            switch (currentTimeOfDay)
            {
                case TimeOfDay.黎明: return "鸡鸣时分，万物苏醒";
                case TimeOfDay.上午: return "朝阳初升，劳作时光";
                case TimeOfDay.正午: return "日当中天，午休小憩";
                case TimeOfDay.下午: return "午后斜阳，继续劳作";
                case TimeOfDay.黄昏: return "夕阳西下，归家用餐";
                case TimeOfDay.夜晚: return "月明星稀，安眠时分";
                default: return "";
            }
        }
        
        /// <summary>
        /// 获取当前游戏时间字符串
        /// </summary>
        public string GetCurrentTimeString()
        {
            int day = currentDay + 1;
            float progress = GetDayProgress();
            int hour = Mathf.FloorToInt(progress * 24);
            return $"第{day}天 {hour}时 ({currentTimeOfDay})";
        }
    }
}