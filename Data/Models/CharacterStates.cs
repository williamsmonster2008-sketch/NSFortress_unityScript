using System;
using System.Collections.Generic;
using UnityEngine;
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
    public List<VirtueTraitValue> traits = new List<VirtueTraitValue>();
    
    public float? GetValue(string traitId)
    {
        var entry = traits.Find(t => t.id == traitId);
        return entry != null ? entry.value : (float?)null;
    }
    
    public void SetValue(string traitId, float value)
    {
        var entry = traits.Find(t => t.id == traitId);
        if (entry != null)
        {
            entry.value = value;
        }
        else
        {
            traits.Add(new VirtueTraitValue { id = traitId, value = value });
        }
    }
}

[Serializable]
public class VirtueTraitValue
{
    public string id;
    public float value;
}
