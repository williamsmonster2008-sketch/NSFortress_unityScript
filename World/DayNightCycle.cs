using UnityEngine;

namespace GameSystems
{
    public class DayNightCycle : MonoBehaviour
    {
        [Header("星空")]        
        public Material nightSkybox;
        public Material daySkybox;  // 添加白天天空盒引用  

        [Header("引用")]
        public Light sunLight;              // 太阳光源
        public Light moonLight;             // 月光光源
        public Transform sunTransform;      // 太阳Transform
        public Transform moonTransform;     // 月亮Transform
        
        [Header("光照配置")]
        [Range(0, 8)]
        public float maxSunIntensity = 1.5f;  // 增强白天亮度
        [Range(0, 2)]
        public float maxMoonIntensity = 1.0f;
        
        public Color dawnColor = new Color(1f, 0.7f, 0.5f);      // 黎明橙红
        public Color noonColor = new Color(1f, 0.95f, 0.9f);     // 正午明亮
        public Color duskColor = new Color(1f, 0.5f, 0.3f);      // 黄昏橙
        public Color nightColor = new Color(0.3f, 0.4f, 0.6f);   // 夜晚蓝
        
        
        
        private TimeSystem timeSystem;
        private Material currentSkybox;
        // private float skyboxBlend = 0f;  // 0=夜晚, 1=白天
        
        void Start()
        {
            timeSystem = GetComponent<TimeSystem>();
            
            if (sunLight) sunLight.shadows = LightShadows.Soft;
            if (moonLight) moonLight.shadows = LightShadows.None;
            
            // 获取当前天空盒作为白天天空盒
            daySkybox = RenderSettings.skybox;
            currentSkybox = daySkybox;
        }
        
        void Update()
        {
            if (timeSystem != null)
            {
                UpdateCelestialBodies();
                UpdateLighting();
            }
        }
        
        void UpdateCelestialBodies()
        {
            float dayProgress = GetDayProgress();
            
            // 增大旋转半径，以Terrain中心为原点
            float radius = 800f;  // 大于Terrain高度的4倍
            float sunAngle = dayProgress * 360f - 90f;  // -90度从东方开始
            
            // 太阳位置（圆周运动）
            float sunX = Mathf.Cos(sunAngle * Mathf.Deg2Rad) * radius;
            float sunY = Mathf.Sin(sunAngle * Mathf.Deg2Rad) * radius + 200f;  // +200偏移到Terrain高度
            sunTransform.position = new Vector3(sunX, sunY, 0);
            sunTransform.LookAt(Vector3.zero + Vector3.up * 200f);  // 朝向Terrain中心
            
            // 月亮位置（与太阳相对）
            float moonAngle = sunAngle + 180f;
            float moonX = Mathf.Cos(moonAngle * Mathf.Deg2Rad) * radius;
            float moonY = Mathf.Sin(moonAngle * Mathf.Deg2Rad) * radius + 200f;
            Vector3 moonPos = new Vector3(moonX, moonY, 0);
            
            // 计算月亮到地面中心的方向
            Vector3 targetPos = Vector3.zero + Vector3.up * 200f;
            if (moonTransform)
            {
                moonTransform.position = moonPos;
                moonTransform.LookAt(targetPos);
            }
            
            
            // 同步Directional Light方向
            if (sunLight) sunLight.transform.rotation = sunTransform.rotation;
            if (moonLight)
            {
                moonLight.transform.position = moonPos;
                // 月光方向：从地面中心指向月亮（反向）
                Vector3 lightDirection = (targetPos - moonPos).normalized;
                moonLight.transform.rotation = Quaternion.LookRotation(lightDirection);
            }
        }
        
        void UpdateLighting()
        {
            float dayProgress = GetDayProgress();
            
            // 计算太阳高度
            float sunHeight = Mathf.Sin((dayProgress * 360f - 90f) * Mathf.Deg2Rad);
            
            // 根据时间决定谁是主光源（有阴影）
            bool isNight = (dayProgress < 0.25f || dayProgress > 0.75f);

            // 太阳光强度
            float sunIntensity = Mathf.Clamp01((sunHeight + 0.2f) / 1.2f);
            sunIntensity = Mathf.Pow(sunIntensity, 0.5f) * maxSunIntensity;

            if (sunLight)
            {
                sunLight.intensity = sunIntensity;
                sunLight.color = GetSunColor(dayProgress);
                sunLight.shadows = isNight ? LightShadows.None : LightShadows.Soft;
            }
                        
            // 重新计算月光强度（夜晚时达到最大值）
            float moonIntensity;
            if (isNight)
            {
                // 深夜时达到最大值
                if (dayProgress < 0.5f)  // 前半夜
                {
                    // 0.25 -> 0: 从0渐变到最大
                    moonIntensity = Mathf.Lerp(0f, maxMoonIntensity, (0.25f - dayProgress) / 0.25f);
                }
                else  // 后半夜
                {
                    // 0.75 -> 1.0: 从0渐变到最大
                    moonIntensity = Mathf.Lerp(0f, maxMoonIntensity, (dayProgress - 0.75f) / 0.25f);
                }
            }
            else
            {
                moonIntensity = 0f;
            }            
                        
            if (moonLight)
            {
                moonLight.intensity = moonIntensity;
                moonLight.color = new Color(0.7f, 0.8f, 1f);
                moonLight.shadows = isNight ? LightShadows.Soft : LightShadows.None;  // 新增
            }
            
            // 环境光
            float minAmbient = 0.65f;
            RenderSettings.ambientIntensity = Mathf.Lerp(minAmbient, 1.0f, sunIntensity / maxSunIntensity);
            // 夜晚环境光偏蓝一点
            Color nightAmbient = new Color(0.45f, 0.5f, 0.65f);  // 淡蓝色
            RenderSettings.ambientLight = Color.Lerp(nightAmbient, noonColor, sunIntensity / maxSunIntensity);
    

            // 天空盒切换
            Material targetSkybox = isNight ? nightSkybox : daySkybox;
            Light targetSunSource = isNight ? moonLight : sunLight;
            
            if (targetSkybox != currentSkybox && targetSkybox != null)
            {
                RenderSettings.skybox = targetSkybox;
                currentSkybox = targetSkybox;
                RenderSettings.sun = targetSunSource;
                DynamicGI.UpdateEnvironment();
            }
        }

        Color GetSunColor(float dayProgress)
        {
            // 0.0-0.25: 夜晚到黎明
            // 0.25-0.5: 黎明到正午
            // 0.5-0.75: 正午到黄昏
            // 0.75-1.0: 黄昏到夜晚
            
            if (dayProgress < 0.25f)  // 夜晚->黎明
            {
                float t = dayProgress / 0.25f;
                return Color.Lerp(nightColor, dawnColor, t);
            }
            else if (dayProgress < 0.5f)  // 黎明->正午
            {
                float t = (dayProgress - 0.25f) / 0.25f;
                return Color.Lerp(dawnColor, noonColor, t);
            }
            else if (dayProgress < 0.75f)  // 正午->黄昏
            {
                float t = (dayProgress - 0.5f) / 0.25f;
                return Color.Lerp(noonColor, duskColor, t);
            }
            else  // 黄昏->夜晚
            {
                float t = (dayProgress - 0.75f) / 0.25f;
                return Color.Lerp(duskColor, nightColor, t);
            }
        }       
        
        
       float GetDayProgress()
        {
            float secondsInDay = timeSystem.totalSeconds % timeSystem.config.secondsPerDay;
            return secondsInDay / timeSystem.config.secondsPerDay;
        }
    }
}