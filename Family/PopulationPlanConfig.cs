using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PopulationPlanConfig", menuName = "Game/Population Plan")]
public class PopulationPlanConfig : ScriptableObject
{
    [System.Serializable]
    public class MatchRule
    {
        public string targetClass;
        [Tooltip("针对该目标阶层的赘婿概率修正")]
        public float ruzhuiBonus = 0f;
    }
    
    [System.Serializable]
    public class SocialClassRule
    {
        public string socialClass;
        [Tooltip("优先匹配的阶层")]
        public List<MatchRule> matches = new List<MatchRule>();
        [Tooltip("无特定目标阶层时的赘婿概率修正")]
        public float fallbackRuzhuiBonus = 0f;
    }
    
    public string planName = "Refugee";
    [Tooltip("禁止与指定家族同姓的外来配偶")]
    public bool forbidSameSurname = true;
    [Range(0f, 1f)]
    public float baseRuzhuiProbability = 0.2f;
    public List<SocialClassRule> classRules = new List<SocialClassRule>();
    
    public string GetMatchedClass(string hostClass, System.Random rand)
    {
        var rule = GetRule(hostClass);
        if (rule == null || rule.matches == null || rule.matches.Count == 0)
        {
            return hostClass;
        }
        
        var match = rule.matches[rand.Next(rule.matches.Count)];
        return string.IsNullOrEmpty(match.targetClass) ? hostClass : match.targetClass;
    }
    
    public float GetRuzhuiProbability(string hostClass, string targetClass = null)
    {
        float bonus = 0f;
        var rule = GetRule(hostClass);
        if (rule != null)
        {
            if (!string.IsNullOrEmpty(targetClass))
            {
                var match = rule.matches?.Find(m => m.targetClass == targetClass);
                if (match != null)
                {
                    bonus = match.ruzhuiBonus;
                }
                else
                {
                    bonus = rule.fallbackRuzhuiBonus;
                }
            }
            else
            {
                bonus = rule.fallbackRuzhuiBonus;
            }
        }
        
        return Mathf.Clamp01(baseRuzhuiProbability + bonus);
    }
    
    private SocialClassRule GetRule(string hostClass)
    {
        if (classRules == null || string.IsNullOrEmpty(hostClass))
        {
            return null;
        }
        return classRules.Find(r => r.socialClass == hostClass);
    }
}
