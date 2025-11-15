using System;
using UnityEngine; // 添加这行
using Sirenix.OdinInspector;

[Serializable]
public class PhysicalState
{
    [ProgressBar(0, 100)]
    public float health = 100f;
    
    [ProgressBar(0, 100)]
    public float energy = 100f;
    
    [ProgressBar(0, 100)]
    public float hunger = 100f;
}

[Serializable]
public class EmotionalState
{
    [Range(0, 100)] public float happiness = 50f;
    [Range(0, 100)] public float sadness = 0f;
    [Range(0, 100)] public float anger = 0f;
}

[Serializable]
public class VirtueTraits
{
    [BoxGroup("仁")] public int loving_tendency = 0;
    [BoxGroup("义")] public int altruism_tendency = 0;
    [BoxGroup("礼")] public int social_tendency = 0;
}