using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PopulationPlanConfig", menuName = "Game/Population Plan")]
public class PopulationPlanConfig : ScriptableObject
{
    [System.Serializable]
    public class MatchRule
    {
        public string targetClass;
        [Range(0.01f, 10f)]
        public float weight = 1f;
        [Tooltip("针对该门第的赘婿概率修正")]
        public float ruzhuiBonus = 0f;
    }
    
    [System.Serializable]
    public class SocialClassRule
    {
        public string socialClass;
        [Tooltip("优先招募赘婿的门第")]
        public List<MatchRule> matches = new List<MatchRule>();
        [Tooltip("未在列表内时的赘婿概率修正")]
        public float fallbackRuzhuiBonus = 0f;
    }
    
    [System.Serializable]
    public class MarriagePreferenceRule
    {
        public string hostClass;
        [Tooltip("男方家族娶妻时的优先门第")]
        public List<MatchRule> femaleMatches = new List<MatchRule>();
        [Tooltip("女方家族招婿时的优先门第")]
        public List<MatchRule> maleMatches = new List<MatchRule>();
        [Range(0f, 1f)]
        [Tooltip("接受门第差距的容忍度, 1为最开明")]
        public float doorGapTolerance = 0.2f;
        [Range(0f, 1f)]
        [Tooltip("需要的好感阈值, 达到后可放宽门第要求")]
        public float affectionOverrideThreshold = 0.6f;
        [Range(0f, 1f)]
        [Tooltip("好感满足时额外的容忍补正")]
        public float affectionDoorGapBonus = 0.2f;
    }
    
    public string planName = "Refugee";
    [Tooltip("禁止与指定家族同姓的外来配偶")]
    public bool forbidSameSurname = true;
    [Header("赘婿概率")]
    [Range(0f, 1f)]
    public float baseRuzhuiProbability = 0.2f;
    public List<SocialClassRule> classRules = new List<SocialClassRule>();
    
    [Header("普通婚姻偏好")]
    public List<MarriagePreferenceRule> marriageRules = new List<MarriagePreferenceRule>();
    
    public string GetMatchedClass(string hostClass, System.Random rand)
    {
        var rule = GetRule(hostClass);
        if (rule == null || rule.matches == null || rule.matches.Count == 0)
        {
            return hostClass;
        }
        
        return SelectByWeight(rule.matches, rand, hostClass);
    }
    
    public string GetMarriageTargetClass(string hostClass, Gender spouseGender, System.Random rand)
    {
        var rule = GetMarriageRule(hostClass);
        if (rule == null)
        {
            return hostClass;
        }
        
        var matches = spouseGender == Gender.Female ? rule.femaleMatches : rule.maleMatches;
        if (matches == null || matches.Count == 0)
        {
            return hostClass;
        }
        
        return SelectByWeight(matches, rand, hostClass);
    }
    
    public float GetDoorGapTolerance(string hostClass)
    {
        var rule = GetMarriageRule(hostClass);
        return rule?.doorGapTolerance ?? 0f;
    }
    
    public (float threshold, float bonus) GetAffectionOverride(string hostClass)
    {
        var rule = GetMarriageRule(hostClass);
        if (rule == null)
        {
            return (1f, 0f);
        }
        return (rule.affectionOverrideThreshold, rule.affectionDoorGapBonus);
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
                bonus = match != null ? match.ruzhuiBonus : rule.fallbackRuzhuiBonus;
            }
            else
            {
                bonus = rule.fallbackRuzhuiBonus;
            }
        }
        
        return Mathf.Clamp01(baseRuzhuiProbability + bonus);
    }
    
    private string SelectByWeight(List<MatchRule> matches, System.Random rand, string fallback)
    {
        float total = matches.Sum(m => Mathf.Max(0.0001f, m.weight));
        if (total <= 0f)
        {
            return fallback;
        }
        
        float roll = (float)rand.NextDouble() * total;
        float cumulative = 0f;
        foreach (var match in matches)
        {
            float weight = Mathf.Max(0.0001f, match.weight);
            cumulative += weight;
            if (roll <= cumulative)
            {
                return string.IsNullOrEmpty(match.targetClass) ? fallback : match.targetClass;
            }
        }
        
        return fallback;
    }
    
    private SocialClassRule GetRule(string hostClass)
    {
        if (classRules == null || string.IsNullOrEmpty(hostClass))
        {
            return null;
        }
        return classRules.Find(r => r.socialClass == hostClass);
    }
    
    private MarriagePreferenceRule GetMarriageRule(string hostClass)
    {
        if (marriageRules == null || string.IsNullOrEmpty(hostClass))
        {
            return null;
        }
        return marriageRules.Find(r => r.hostClass == hostClass);
    }
}
