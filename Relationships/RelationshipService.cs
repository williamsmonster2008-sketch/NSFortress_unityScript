using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 关系服务 - 用于组合血缘、情感、社会身份
/// </summary>
public class RelationshipService : MonoBehaviour
{
    [SerializeField] private FamilySystem familySystem;
    
    private IEmotionalRelationshipProvider emotionalProvider;
    private ISocialRelationshipProvider socialProvider;
    
    /// <summary>
    /// 初始化服务
    /// </summary>
    public void Initialize(FamilySystem system,
                           IEmotionalRelationshipProvider emotional = null,
                           ISocialRelationshipProvider social = null)
    {
        familySystem = system;
        emotionalProvider = emotional;
        socialProvider = social;
    }
    
    /// <summary>
    /// 设置情感关系提供�?
    /// </summary>
    public void SetEmotionalProvider(IEmotionalRelationshipProvider provider)
    {
        emotionalProvider = provider;
    }
    
    /// <summary>
    /// 设置社会身份提供�?
    /// </summary>
    public void SetSocialProvider(ISocialRelationshipProvider provider)
    {
        socialProvider = provider;
    }
    
    /// <summary>
    /// 获取完整关系描述
    /// </summary>
    public RelationshipDescriptor GetRelationship(string fromCharacterId, string toCharacterId)
    {
        if (string.IsNullOrEmpty(fromCharacterId) || string.IsNullOrEmpty(toCharacterId))
        {
            return null;
        }
        
        var descriptor = new RelationshipDescriptor
        {
            fromCharacterId = fromCharacterId,
            toCharacterId = toCharacterId
        };
        
        descriptor.kinship = familySystem?.GetKinship(fromCharacterId, toCharacterId);
        
        if (emotionalProvider != null)
        {
            descriptor.emotional = emotionalProvider.GetEmotionalRelationship(fromCharacterId, toCharacterId);
        }
        
        if (socialProvider != null)
        {
            descriptor.social = socialProvider.GetSocialRelationship(fromCharacterId, toCharacterId);
        }
        
        descriptor.displayTitle = descriptor.kinship?.title ?? "陌生人";
        descriptor.primaryCategory = DeterminePrimaryCategory(descriptor);
        descriptor.tags = BuildTags(descriptor);
        
        return descriptor;
    }
    
    private string DeterminePrimaryCategory(RelationshipDescriptor descriptor)
    {
        if (descriptor.kinship != null)
        {
            return "family";
        }
        
        if (descriptor.social != null && !string.IsNullOrEmpty(descriptor.social.identityType))
        {
            return "social";
        }
        
        if (descriptor.emotional != null && !string.IsNullOrEmpty(descriptor.emotional.state))
        {
            return "emotional";
        }
        
        return "unknown";
    }
    
    private List<string> BuildTags(RelationshipDescriptor descriptor)
    {
        var tags = new List<string>();
        
        if (descriptor.kinship != null)
        {
            tags.Add("血缘");
            if (!string.IsNullOrEmpty(descriptor.kinship.relationType))
            {
                tags.Add(descriptor.kinship.relationType);
            }
        }
        
        if (descriptor.social != null && !string.IsNullOrEmpty(descriptor.social.identityType))
        {
            tags.Add("身份");
            tags.Add(descriptor.social.identityType);
        }
        
        if (descriptor.emotional != null && !string.IsNullOrEmpty(descriptor.emotional.state))
        {
            tags.Add("情感");
            tags.Add(descriptor.emotional.state);
        }
        
        return tags;
    }
}

/// <summary>
/// 关系描述结果
/// </summary>
public class RelationshipDescriptor
{
    public string fromCharacterId;
    public string toCharacterId;
    public string displayTitle;
    public string primaryCategory;
    public KinshipRelation kinship;
    public EmotionalRelationshipInfo emotional;
    public SocialRelationshipInfo social;
    public List<string> tags = new List<string>();
}

/// <summary>
/// 情感关系占位数据
/// </summary>
public class EmotionalRelationshipInfo
{
    public string state;
    public float affection;
    public float trust;
}

/// <summary>
/// 社会身份占位数据
/// </summary>
public class SocialRelationshipInfo
{
    public string identityType;
    public string hierarchy;
}

/// <summary>
/// 情感关系提供�?
/// </summary>
public interface IEmotionalRelationshipProvider
{
    EmotionalRelationshipInfo GetEmotionalRelationship(string fromCharacterId, string toCharacterId);
}

/// <summary>
/// 社会身份提供�?
/// </summary>
public interface ISocialRelationshipProvider
{
    SocialRelationshipInfo GetSocialRelationship(string fromCharacterId, string toCharacterId);
}
