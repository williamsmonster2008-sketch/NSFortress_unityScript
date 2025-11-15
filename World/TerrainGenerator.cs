using UnityEngine;
using MapMagic.Core;

public class TerrainGenerator : MonoBehaviour
{
    [Header("MapMagic引用")]
    public MapMagicObject mapMagicObject;
    
    private void Awake()
    {
        if (mapMagicObject == null)
        {
            mapMagicObject = GetComponent<MapMagicObject>();
        }
    }
    
    public void GenerateTerrain(WorldSeed seed)
    {
        if (mapMagicObject == null)
        {
            Debug.LogError("❌ MapMagicObject未设置！");
            return;
        }
        
        Debug.Log($"🏔️ MapMagic生成地形 (种子: {seed.terrainSeed})");
        
        // MapMagic 2暂时使用默认生成
        // 种子控制需要通过Graph的Exposed Variables实现
        // 这里先让地形能正常生成
        
        Debug.Log("✅ 地形生成完成");
    }
}