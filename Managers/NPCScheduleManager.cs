using System.Collections.Generic;
using UnityEngine;

namespace GameSystems
{
    /// <summary>
    /// NPC调度管理器 - 监听时辰变化，统一调度所有NPC行为
    /// </summary>
    public class NPCScheduleManager : MonoBehaviour
    {
        public static NPCScheduleManager Instance { get; private set; }
        
        [Header("引用")]
        public TimeSystem timeSystem;
        
        [Header("调试")]
        public bool enableDebugLog = true;
        
        // 注册的NPC列表
        private List<NPCController> registeredNPCs = new List<NPCController>();
        
        // 当前时辰
        private TimeOfDay currentTimeOfDay;
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            // 获取TimeSystem引用
            if (timeSystem == null)
            {
                timeSystem = FindObjectOfType<TimeSystem>();
            }
            
            if (timeSystem == null)
            {
                Debug.LogError("❌ NPCScheduleManager: 找不到TimeSystem!");
                return;
            }
            
            // 订阅时辰变化事件
            timeSystem.OnTimeOfDayChanged += OnTimeOfDayChanged;
            
            // 获取当前时辰
            currentTimeOfDay = timeSystem.GetCurrentTimeOfDay();
            
            Debug.Log($"✅ NPCScheduleManager 初始化完成，当前时辰: {currentTimeOfDay}");
        }
        
        void OnDestroy()
        {
            if (timeSystem != null)
            {
                timeSystem.OnTimeOfDayChanged -= OnTimeOfDayChanged;
            }
        }
        
        /// <summary>
        /// 注册NPC
        /// </summary>
        public void RegisterNPC(NPCController npc)
        {
            if (npc != null && !registeredNPCs.Contains(npc))
            {
                registeredNPCs.Add(npc);
                
                if (enableDebugLog)
                {
                    Debug.Log($"📋 注册NPC: {npc.characterData?.name ?? "未命名"} (总数:{registeredNPCs.Count})");
                }
                
                // 立即设置当前时辰的行为
                npc.OnTimeOfDayChanged(currentTimeOfDay);
            }
        }
        
        /// <summary>
        /// 注销NPC
        /// </summary>
        public void UnregisterNPC(NPCController npc)
        {
            if (npc != null && registeredNPCs.Contains(npc))
            {
                registeredNPCs.Remove(npc);
                
                if (enableDebugLog)
                {
                    Debug.Log($"📋 注销NPC: {npc.characterData?.name ?? "未命名"} (总数:{registeredNPCs.Count})");
                }
            }
        }
        
        /// <summary>
        /// 时辰变化回调
        /// </summary>
        private void OnTimeOfDayChanged(TimeOfDay newTimeOfDay)
        {
            currentTimeOfDay = newTimeOfDay;
            
            if (enableDebugLog)
            {
                Debug.Log($"⏰ NPCScheduleManager: 时辰变化 → {newTimeOfDay}，通知 {registeredNPCs.Count} 个NPC");
            }
            
            // 通知所有注册的NPC
            foreach (var npc in registeredNPCs)
            {
                if (npc != null)
                {
                    npc.OnTimeOfDayChanged(newTimeOfDay);
                }
            }
            
            // 清理已销毁的NPC
            registeredNPCs.RemoveAll(npc => npc == null);
        }
        
        /// <summary>
        /// 获取当前注册的NPC数量
        /// </summary>
        public int GetRegisteredNPCCount()
        {
            return registeredNPCs.Count;
        }
        
        /// <summary>
        /// 获取当前时辰
        /// </summary>
        public TimeOfDay GetCurrentTimeOfDay()
        {
            return currentTimeOfDay;
        }
    }
}