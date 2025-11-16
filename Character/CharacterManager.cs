using UnityEngine;
using System.Collections.Generic;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }
    
    [Header("角色Prefab配置")]
    public GameObject maleAdultPrefab;
    public GameObject maleYoungAdultPrefab;
    public GameObject maleElderPrefab;
    public GameObject maleKidPrefab;
    public GameObject femaleAdultPrefab;
    public GameObject femaleYoungAdultPrefab;
    public GameObject femaleElderPrefab;
    public GameObject femaleKidPrefab;
    
    [Header("其他配置")]
    public Transform npcParent;
    
    private Dictionary<string, CharacterRuntimeData> characters = new Dictionary<string, CharacterRuntimeData>();
    private Dictionary<string, NPCController> npcControllers = new Dictionary<string, NPCController>();
    
    void Awake()
    {
        Instance = this;
    }
    
    public void InitializePopulation(List<CharacterRuntimeData> characterList)
    {
        Debug.Log($"👥 开始初始化人口: {characterList.Count}人");
        
        foreach (var character in characterList)
        {
            AddCharacter(character);
        }
        
        Debug.Log($"✅ 人口初始化完成: {characters.Count}人");
    }
    
    public void AddCharacter(CharacterRuntimeData character)
    {
        characters[character.characterId] = character;
        GameManager.Instance?.familySystem?.RegisterCharacter(character);
        SpawnNPC(character);
    }
    
    public void RemoveCharacter(string characterId)
    {
        if (!characters.ContainsKey(characterId))
        {
            return;
        }
        
        characters.Remove(characterId);
        if (npcControllers.TryGetValue(characterId, out var controller) && controller != null)
        {
            Destroy(controller.gameObject);
            npcControllers.Remove(characterId);
        }
        
        GameManager.Instance?.familySystem?.RemoveCharacter(characterId);
    }
    
    private void SpawnNPC(CharacterRuntimeData character)
    {
        // 根据角色属性选择Prefab
        GameObject prefabToUse = SelectPrefab(character);
        
        if (prefabToUse == null)
        {
            Debug.LogError($"❌ 找不到适合的Prefab: {character.gender}, {character.age}岁");
            return;
        }
        
        Vector3 spawnPos = GetRandomSpawnPosition();
        character.position = spawnPos;
        
        GameObject npcObj = Instantiate(prefabToUse, spawnPos, Quaternion.identity, npcParent);
        npcObj.name = character.name;
        
        // 确保有可选中的Collider
        if (npcObj.GetComponentInChildren<Collider>() == null)
        {
            var collider = npcObj.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 1f, 0f);
            collider.height = 2f;
            collider.radius = 0.4f;
        }
        
        NPCController controller = npcObj.GetComponent<NPCController>();
        if (controller == null)
        {
            controller = npcObj.AddComponent<NPCController>();
        }
        controller.Initialize(character);
        
        npcControllers[character.characterId] = controller;
    }
    
    private GameObject SelectPrefab(CharacterRuntimeData character)
    {
        // 根据年龄和性别选择Prefab
        bool isMale = character.gender == Gender.Male;
        int age = character.age;
        
        if (age < 16)
        {
            // 儿童
            return isMale ? maleKidPrefab : femaleKidPrefab;
        }
        else if (age < 25)
        {
            // 青年
            return isMale ? maleYoungAdultPrefab : femaleYoungAdultPrefab;
        }
        else if (age < 60)
        {
            // 成年
            return isMale ? maleAdultPrefab : femaleAdultPrefab;
        }
        else
        {
            // 老年
            return isMale ? maleElderPrefab : femaleElderPrefab;
        }
    }
    
    private Vector3 GetRandomSpawnPosition()
    {
        // 调整到您地形中确定平坦且有NavMesh的位置
        Vector3 center = new Vector3(332, 205, 230); // 根据实际情况修改
        
        // 缩小生成范围
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(0f, 3f); // 从5米改为3米，更集中
        
        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * distance,
            0,
            Mathf.Sin(angle) * distance
        );
        
        Vector3 spawnPos = center + offset;
        
        // 增加检测范围
        RaycastHit hit;
        if (Physics.Raycast(spawnPos + Vector3.up * 100f, Vector3.down, out hit, 150f))
        {
            // 检查是否在NavMesh上
            UnityEngine.AI.NavMeshHit navHit;
            if (UnityEngine.AI.NavMesh.SamplePosition(hit.point, out navHit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            {
                return navHit.position + Vector3.up * 0.5f;
            }
        }
        
        return spawnPos;
    }
    
    public CharacterRuntimeData GetCharacter(string id)
    {
        return characters.ContainsKey(id) ? characters[id] : null;
    }
    
    public List<CharacterRuntimeData> GetAllCharacters()
    {
        return new List<CharacterRuntimeData>(characters.Values);
    }
}
