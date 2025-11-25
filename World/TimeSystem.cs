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
        public int timeStepsPerDay = 12;        // 12个时辰
    }
    
    /// <summary>
    /// 时辰枚举（12时辰）
    /// </summary>
    public enum TimeOfDay
    {
        子时 = 0,   // 23-1时 (夜半)
        丑时 = 1,   // 1-3时 (鸡鸣)
        寅时 = 2,   // 3-5时 (平旦)
        卯时 = 3,   // 5-7时 (日出)
        辰时 = 4,   // 7-9时 (食时)
        巳时 = 5,   // 9-11时 (隅中)
        午时 = 6,   // 11-13时 (日中)
        未时 = 7,   // 13-15时 (日昳)
        申时 = 8,   // 15-17时 (哺时)
        酉时 = 9,   // 17-19时 (日入)
        戌时 = 10,  // 19-21时 (黄昏)
        亥时 = 11   // 21-23时 (人定)
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
        
        private TimeOfDay currentTimeOfDay = TimeOfDay.子时;
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
            
            // 子时特殊处理：23-24时和0-1时
            // dayProgress: 0.958-1.0 和 0.0-0.042 都是子时
            if (dayProgress >= 0.958f || dayProgress < 0.042f)
            {
                return TimeOfDay.子时;
            }
            
            // 其他时辰：从丑时(1)开始
            // 丑时开始于 dayProgress = 0.042
            int index = Mathf.FloorToInt((dayProgress - 0.042f) / (1f / 12f)) + 1;
            index = Mathf.Clamp(index, 1, 11);
            
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
                case TimeOfDay.子时: return "夜半，又名子夜、中夜";
                case TimeOfDay.丑时: return "鸡鸣，又名荒鸡";
                case TimeOfDay.寅时: return "平旦，又名黎明、早晨";
                case TimeOfDay.卯时: return "日出，又名日始、破晓";
                case TimeOfDay.辰时: return "食时，又名早食";
                case TimeOfDay.巳时: return "隅中，又名日禺";
                case TimeOfDay.午时: return "日中，又名日正、中午";
                case TimeOfDay.未时: return "日昳，又名日跌、日央";
                case TimeOfDay.申时: return "哺时，又名日铺、夕食";
                case TimeOfDay.酉时: return "日入，又名日落、黄昏";
                case TimeOfDay.戌时: return "黄昏，又名日夕、日暮";
                case TimeOfDay.亥时: return "人定，又名定昏";
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