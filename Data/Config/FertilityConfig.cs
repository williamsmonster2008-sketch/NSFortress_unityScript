using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "FertilityConfig", menuName = "Game/Fertility Config")]
public class FertilityConfig : ScriptableObject
{
    [Title("生育年龄")]
    public int minBreedingAge = 16;
    public int maxBreedingAgeMale = 70;
    public int maxBreedingAgeFemale = 50;
    
    [Title("代际间隔")]
    public int minGenerationGap = 18;
    public int maxGenerationGap = 35;
    
    [Title("夫妻年龄差")]
    public int minCoupleAgeDiff = -5; // 负数表示妻子年龄可以大于丈夫
    public int maxCoupleAgeDiff = 15;
    
    [Title("寿命")]
    public int maxLifespan = 85;
    
    [Title("生育曲线 [年龄上限, 生育率]")]
    [TableList]
    public FertilityCurvePoint[] fertilityCurve = new FertilityCurvePoint[]
    {
        new FertilityCurvePoint { ageLimit = 20, fertilityRate = 0.05f },
        new FertilityCurvePoint { ageLimit = 25, fertilityRate = 0.15f },
        new FertilityCurvePoint { ageLimit = 30, fertilityRate = 0.25f },
        new FertilityCurvePoint { ageLimit = 35, fertilityRate = 0.30f },
        new FertilityCurvePoint { ageLimit = 40, fertilityRate = 0.20f },
        new FertilityCurvePoint { ageLimit = 45, fertilityRate = 0.05f },
        new FertilityCurvePoint { ageLimit = 50, fertilityRate = 0.00f }
    };
    
    [Title("多胎概率 [概率, 胎数]")]
    [TableList]
    public MultipleBirthRate[] multipleBirthRates = new MultipleBirthRate[]
    {
        new MultipleBirthRate { probability = 0.85f, count = 1 },
        new MultipleBirthRate { probability = 0.12f, count = 2 },
        new MultipleBirthRate { probability = 0.03f, count = 3 }
    };
    
    [Title("存活率配置")]
    [TableList]
    public GenerationSurvivalRate[] survivalRates = new GenerationSurvivalRate[]
    {
        new GenerationSurvivalRate { generation = 1, survivalRate = 0.30f },
        new GenerationSurvivalRate { generation = 2, survivalRate = 0.50f },
        new GenerationSurvivalRate { generation = 3, survivalRate = 0.70f },
        new GenerationSurvivalRate { generation = 4, survivalRate = 0.85f },
        new GenerationSurvivalRate { generation = 5, survivalRate = 0.95f }
    };
}

[System.Serializable]
public class FertilityCurvePoint
{
    public int ageLimit;
    [Range(0, 1)]
    public float fertilityRate;
}

[System.Serializable]
public class MultipleBirthRate
{
    [Range(0, 1)]
    public float probability;
    public int count;
}

[System.Serializable]
public class GenerationSurvivalRate
{
    public int generation;
    [Range(0, 1)]
    public float survivalRate;
}