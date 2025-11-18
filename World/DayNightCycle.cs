using UnityEngine;

namespace GameSystems
{
    public class DayNightCycle : MonoBehaviour
    {
        [Header("星空")]
        public Material starDomeMaterial;  // 拖入StarDomeMaterial

        [Header("引用")]
        public Light sunLight;              // 太阳光源
        public Light moonLight;             // 月光光源
        public Transform sunTransform;      // 太阳Transform
        public Transform moonTransform;     // 月亮Transform
        
        [Header("光照配置")]
        [Range(0, 8)]
        public float maxSunIntensity = 1.5f;  // 增强白天亮度
        [Range(0, 1)]
        public float maxMoonIntensity = 0.1f;
        
        public Color dawnColor = new Color(1f, 0.7f, 0.5f);      // 黎明橙红
        public Color noonColor = new Color(1f, 0.95f, 0.9f);     // 正午明亮
        public Color duskColor = new Color(1f, 0.5f, 0.3f);      // 黄昏橙
        public Color nightColor = new Color(0.3f, 0.4f, 0.6f);   // 夜晚蓝
        
        [Header("天空盒过渡")]
        [Range(0, 1)]
        public float skyboxBlendSpeed = 0.1f;  // 平滑过渡速度
        
        private TimeSystem timeSystem;
        private Material currentSkybox;
        private float skyboxBlend = 0f;  // 0=夜晚, 1=白天
        
        void Start()
        {
            timeSystem = GetComponent<TimeSystem>();
            
            // 初始化光源
            if (sunLight) sunLight.shadows = LightShadows.Soft;
            if (moonLight) moonLight.shadows = LightShadows.None;
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
            moonTransform.position = new Vector3(moonX, moonY, 0);
            moonTransform.LookAt(Vector3.zero + Vector3.up * 200f);
            
            // 同步Directional Light方向
            if (sunLight) sunLight.transform.rotation = sunTransform.rotation;
            if (moonLight) moonLight.transform.rotation = moonTransform.rotation;
        }
        
        void UpdateLighting()
        {
            float dayProgress = GetDayProgress();
            
            // 计算太阳高度（-1到1）
            float sunHeight = Mathf.Sin((dayProgress * 360f - 90f) * Mathf.Deg2Rad);
            
            // 太阳光强度（平滑曲线）
            float sunIntensity = Mathf.Clamp01((sunHeight + 0.2f) / 1.2f);  // 地平线以下也有微光
            sunIntensity = Mathf.Pow(sunIntensity, 0.5f) * maxSunIntensity;  // 平方根曲线，更自然
            
            if (sunLight)
            {
                sunLight.intensity = sunIntensity;
                sunLight.color = GetSunColor(dayProgress);
            }
            
            // 月光强度（太阳下山后）
            float moonIntensity = Mathf.Clamp01((-sunHeight + 0.1f) / 1.1f) * maxMoonIntensity;
            if (moonLight)
            {
                moonLight.intensity = moonIntensity;
                moonLight.color = new Color(0.7f, 0.8f, 1f);  // 淡蓝色月光
            }
            
            // 环境光（重要！）
            RenderSettings.ambientIntensity = Mathf.Lerp(0.3f, 1.0f, sunIntensity / maxSunIntensity);
            RenderSettings.ambientLight = Color.Lerp(nightColor, noonColor, sunIntensity / maxSunIntensity);

            // 控制星空可见度
            if (starDomeMaterial != null)
            {
                float starAlpha = Mathf.Clamp01((-sunHeight + 0.3f) / 1.3f);  // 太阳下山后显示
                starDomeMaterial.SetFloat("_Alpha", starAlpha);
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