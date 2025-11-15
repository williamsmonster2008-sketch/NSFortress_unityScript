using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SocialClassConfig", menuName = "Game/Social Class Config")]
public class SocialClassConfig : ScriptableObject
{
    [Serializable]
    public class ClassDistribution
    {
        public string className;
        [Range(0f, 1f)]
        public float probability;
    }
    
    [Header("传统家族模式的阶层分布")]
    public ClassDistribution[] traditionalFamilyDistribution = new ClassDistribution[]
    {
        new ClassDistribution { className = "平民", probability = 0.45f },
        new ClassDistribution { className = "庶族", probability = 0.15f },
        new ClassDistribution { className = "寒门", probability = 0.25f },
        new ClassDistribution { className = "门阀士族", probability = 0.05f },
        new ClassDistribution { className = "胡族", probability = 0.1f }
    };
    
    public string GetRandomSocialClass(System.Random rand)
    {
        float randomValue = (float)rand.NextDouble();
        float cumulative = 0f;
        
        foreach (var dist in traditionalFamilyDistribution)
        {
            cumulative += dist.probability;
            if (randomValue <= cumulative)
            {
                return dist.className;
            }
        }
        
        return "平民"; // 默认
    }
}