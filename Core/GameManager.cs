using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("世界配置")]
    public WorldSeed currentSeed;
    public bool useRandomSeed = true;
    
    [Header("引用")]
    public TerrainGenerator terrainGenerator;
    public DataLoader dataLoader;
    
    [Header("角色系统")]
    public FamilyGenerator familyGenerator;
    public CharacterManager characterManager; // 添加这行
    
    [Header("人口配置")]
    public int initialFamilyCount = 4;
    public int avgFamilySize = 5;

    [Header("家族系统")]
    public FamilyManager familyManager;
    
    
    private List<CharacterRuntimeData> allCharacters = new List<CharacterRuntimeData>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        TestDataLoading();
        InitializeGame();
    }
    
    private void TestDataLoading()
    {
        Debug.Log("🧪 开始测试数据加载");
        
        if (dataLoader == null)
        {
            Debug.LogError("❌ DataLoader未设置！");
            return;
        }
        
        var templates = dataLoader.LoadCharacterTemplates();
        if (templates != null && templates.templates.Count > 0)
        {
            Debug.Log($"✅ 成功加载 {templates.templates.Count} 个角色模板");
        }
        else
        {
            Debug.LogError("❌ 角色模板加载失败");
        }
    }
    
    private void InitializeGame()
    {
        Debug.Log("🎮 游戏初始化开始");
        
        // 生成世界种子
        if (useRandomSeed)
        {
            currentSeed = new WorldSeed();
            Debug.Log($"🎲 生成随机种子: {currentSeed.shareCode}");
        }
        
        // 生成地形
        if (terrainGenerator != null)
        {
            terrainGenerator.GenerateTerrain(currentSeed);
        }
        
        
        if (familyGenerator != null)  // 👈 改这里
        {
            Debug.Log("📋 开始初始化 FamilyGenerator");
            familyGenerator.Initialize(currentSeed.populationSeed);  // 👈 改这里
            Debug.Log("📋 开始生成难民群体");
            allCharacters = familyGenerator.GenerateRefugeeGroup(initialFamilyCount, avgFamilySize);
            
            Debug.Log($"📋 生成了 {allCharacters.Count} 个角色数据");
        }
        else
        {
            Debug.LogError("❌ FamilyGenerator 未配置!");
        }

        // 调试: 输出前3个角色的家族信息
        if (allCharacters != null && allCharacters.Count > 0)
        {
            for (int i = 0; i < Mathf.Min(3, allCharacters.Count); i++)
            {
                var c = allCharacters[i];
                Debug.Log($"  [{i}] {c.name}: 父={c.fatherId ?? "无"}, 母={c.motherId ?? "无"}, 代={c.generation}");
            }
        }
        else
        {
            Debug.LogError("❌ allCharacters 为空或没有数据！");
        }
        
            // 初始化家族系统（新增）
        if (familyManager != null)
        {
            familyManager.Initialize(allCharacters);
            
            // 测试关系计算
            familyManager.TestRelationships();
        }

        // 在场景中生成NPC（延迟执行，等地形加载完成）
        Debug.Log($"🔍 检查 - characterManager: {(characterManager != null ? "存在" : "null")}");
        if (characterManager != null)
        {
            Debug.Log("⏰ 将在2秒后生成NPC");
            Invoke("SpawnNPCs", 2f); // 2秒后生成NPC
        }
        else
        {
            Debug.LogError("❌ characterManager 为空！");
        }
        
        Debug.Log("✅ 游戏初始化完成");
    }
    
    private void SpawnNPCs()
    {
        Debug.Log($"🎭 SpawnNPCs 被调用，角色数量: {allCharacters.Count}");
        characterManager.InitializePopulation(allCharacters);
        Debug.Log("🎭 NPC已在场景中生成");
    }
}