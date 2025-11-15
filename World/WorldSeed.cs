using System;
using UnityEngine;

[Serializable]
public class WorldSeed
{
    public int terrainSeed;
    public int populationSeed;
    public int vegetationSeed;
    public string shareCode;
    public string version = "0.1.0";
    
    public WorldSeed()
    {
        GenerateRandomSeeds();
    }
    
    public WorldSeed(string code)
    {
        ParseFromShareCode(code);
    }
    
    private void GenerateRandomSeeds()
    {
        System.Random rand = new System.Random((int)DateTimeOffset.Now.ToUnixTimeSeconds());
        terrainSeed = rand.Next(0, 999999);
        populationSeed = rand.Next(0, 999999);
        vegetationSeed = rand.Next(0, 999999);
        GenerateShareCode();
    }
    
    private void GenerateShareCode()
    {
        shareCode = $"NB-{terrainSeed:X4}-{populationSeed:X4}";
    }
    
    private void ParseFromShareCode(string code)
    {
        // 简化版解析
        shareCode = code;
        terrainSeed = UnityEngine.Random.Range(0, 999999);
        populationSeed = UnityEngine.Random.Range(0, 999999);
        vegetationSeed = UnityEngine.Random.Range(0, 999999);
    }
}