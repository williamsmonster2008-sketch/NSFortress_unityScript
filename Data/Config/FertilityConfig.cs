using UnityEngine;

/// <summary>
/// 生育配置 - 扩展版
/// 添加子女数量配置和死亡率配置
/// </summary>
[CreateAssetMenu(fileName = "FertilityConfig", menuName = "Game/Fertility Config")]
public class FertilityConfig : ScriptableObject
{
    [Header("生育年龄范围")]
    [Tooltip("最小生育年龄")]
    public int minBreedingAge = 16;
    
    [Tooltip("最大生育年龄")]
    public int maxBreedingAge = 45;
    
    [Tooltip("最大寿命")]
    public int maxLifespan = 80;
    
    [Header("代际间隔")]
    [Tooltip("最小代际间隔(年)")]
    public int minGenerationGap = 18;
    
    [Tooltip("最大代际间隔(年)")]
    public int maxGenerationGap = 35;
    
    [Header("夫妻年龄差")]
    [Tooltip("最小夫妻年龄差(年) - 负数表示妻子比丈夫年轻")]
    public int minCoupleAgeDiff = -5;
    
    [Tooltip("最大夫妻年龄差(年)")]
    public int maxCoupleAgeDiff = 10;
    
    [Header("子女数量配置")]
    [Tooltip("最少子女数")]
    public int minChildren = 1;
    
    [Tooltip("最多子女数")]
    public int maxChildren = 5;
    
    [Tooltip("平均子女数")]
    public float averageChildren = 3f;
    
    [Header("生育率曲线")]
    [Tooltip("按母亲年龄的生育概率曲线")]
    public AnimationCurve fertilityByAge = AnimationCurve.Linear(16, 0.5f, 45, 1f);
    
    [Header("多胎概率")]
    [Tooltip("双胞胎概率")]
    [Range(0f, 0.2f)]
    public float twinProbability = 0.03f;
    
    [Tooltip("三胞胎概率")]
    [Range(0f, 0.1f)]
    public float tripletProbability = 0.01f;
    
    [Header("死亡率配置")]
    [Tooltip("高死亡率家族的死亡率")]
    [Range(0f, 1f)]
    public float highMortalityRate = 0.65f;
    
    [Tooltip("低死亡率家族的死亡率")]
    [Range(0f, 1f)]
    public float lowMortalityRate = 0.15f;
    
    [Header("各代存活率")]
    [Tooltip("第1代(高祖)存活率")]
    [Range(0f, 1f)]
    public float generation1SurvivalRate = 0.3f;
    
    [Tooltip("第2代(曾祖)存活率")]
    [Range(0f, 1f)]
    public float generation2SurvivalRate = 0.5f;
    
    [Tooltip("第3代(祖父母)存活率")]
    [Range(0f, 1f)]
    public float generation3SurvivalRate = 0.7f;
    
    [Tooltip("第4代(父母)存活率")]
    [Range(0f, 1f)]
    public float generation4SurvivalRate = 0.9f;
    
    [Tooltip("第5代(子女)存活率")]
    [Range(0f, 1f)]
    public float generation5SurvivalRate = 0.95f;
    
    /// <summary>
    /// 获取某代的存活率
    /// </summary>
    public float GetSurvivalRate(int generation)
    {
        switch (generation)
        {
            case 1: return generation1SurvivalRate;
            case 2: return generation2SurvivalRate;
            case 3: return generation3SurvivalRate;
            case 4: return generation4SurvivalRate;
            case 5: return generation5SurvivalRate;
            default: return 0.5f;
        }
    }
}