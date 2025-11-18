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

    public class TimeSystem : MonoBehaviour
    {
        [Header("时间配置")]
        public TimeConfig config = new TimeConfig();
        
        [Header("当前状态")]
        public float totalSeconds = 0f;
        public float timeScale = 1f;
        public bool isPaused = false;
        
        // 当前时间组成
        private int currentYear;
        private int currentSeason;
        private int currentDay;
        private int currentTimeOfDay;
        
        // 事件
        public event Action<int> OnNewDay;
        public event Action<string> OnNewTimeOfDay;
        public event Action<string> OnNewSeason;
        
        void Update()
        {
            if (!isPaused)
            {
                UpdateTime(Time.deltaTime);
            }
        }
        
        void UpdateTime(float deltaTime)
        {
            float previousSeconds = totalSeconds;
            totalSeconds += deltaTime * timeScale;
            
            CheckTimeEvents(previousSeconds);
        }
        
        void CheckTimeEvents(float previousSeconds)
        {
            // 检测新时辰、新一天、新季节等
            // 下一步实现
        }
        
        public string GetCurrentTimeOfDay()
        {
            string[] periods = {"黎明", "上午", "正午", "下午", "黄昏", "夜晚"};
            return periods[currentTimeOfDay];
        }
    }
}