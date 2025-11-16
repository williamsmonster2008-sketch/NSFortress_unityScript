using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    
    [Header("其它配置")]
    public Transform npcParent;
    
    private readonly Dictionary<string, CharacterRuntimeData> characters = new Dictionary<string, CharacterRuntimeData>();
    private readonly Dictionary<string, NPCController> npcControllers = new Dictionary<string, NPCController>();
    
    private void Awake()
    {
        Instance = this;
    }
    
    public void InitializePopulation(List<CharacterRuntimeData> characterList)
    {
        Debug.Log($"👥 开始初始化人口: {characterList.Count}人");
        
        foreach (var character in characterList)
        {
            bool shouldSpawn = character != null && character.vitalStatus == "living";
            AddCharacter(character, shouldSpawn);
        }
        
        int livingCount = characters.Values.Count(c => c.vitalStatus == "living");
        Debug.Log($"✅ 人口初始化完成: 总数 {characters.Count} 人 在世 {livingCount} 人 已故 {characters.Count - livingCount} 人");
    }
    
    public void AddCharacter(CharacterRuntimeData character, bool spawnNpc = true)
    {
        if (character == null || string.IsNullOrEmpty(character.characterId))
        {
            return;
        }
        
        characters[character.characterId] = character;
        GameManager.Instance?.familySystem?.RegisterCharacter(character);
        
        if (spawnNpc)
        {
            SpawnNPC(character);
        }
    }
    
    public void RemoveCharacter(string characterId)
    {
        if (string.IsNullOrEmpty(characterId) || !characters.ContainsKey(characterId))
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
        if (character == null || character.vitalStatus != "living")
        {
            return;
        }
        
        GameObject prefabToUse = SelectPrefab(character);
        if (prefabToUse == null)
        {
            Debug.LogError($"❌ 找不到合适的Prefab: {character.gender}, {character.age}岁");
            return;
        }
        
        Vector3 spawnPos = GetRandomSpawnPosition();
        character.position = spawnPos;
        
        GameObject npcObj = Instantiate(prefabToUse, spawnPos, Quaternion.identity, npcParent);
        npcObj.name = character.name;
        
        if (npcObj.GetComponentInChildren<Collider>() == null)
        {
            var collider = npcObj.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 1f, 0f);
            collider.height = 2f;
            collider.radius = 0.4f;
        }
        
        var controller = npcObj.GetComponent<NPCController>();
        if (controller == null)
        {
            controller = npcObj.AddComponent<NPCController>();
        }
        controller.Initialize(character);
        
        npcControllers[character.characterId] = controller;
    }
    
    private GameObject SelectPrefab(CharacterRuntimeData character)
    {
        if (character == null)
        {
            return null;
        }
        
        bool isMale = character.gender == Gender.Male;
        int age = character.age;
        
        if (age < 16)
        {
            return isMale ? maleKidPrefab : femaleKidPrefab;
        }
        if (age < 25)
        {
            return isMale ? maleYoungAdultPrefab : femaleYoungAdultPrefab;
        }
        if (age < 60)
        {
            return isMale ? maleAdultPrefab : femaleAdultPrefab;
        }
        
        return isMale ? maleElderPrefab : femaleElderPrefab;
    }
    
    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 center = new Vector3(332f, 205f, 230f);
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(0f, 3f);
        Vector3 offset = new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);
        Vector3 spawnPos = center + offset;
        
        if (Physics.Raycast(spawnPos + Vector3.up * 100f, Vector3.down, out var hit, 150f))
        {
            if (UnityEngine.AI.NavMesh.SamplePosition(hit.point, out var navHit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            {
                return navHit.position + Vector3.up * 0.5f;
            }
        }
        
        return spawnPos;
    }
    
    public CharacterRuntimeData GetCharacter(string id)
    {
        return characters.TryGetValue(id, out var data) ? data : null;
    }
    
    public List<CharacterRuntimeData> GetAllCharacters()
    {
        return characters.Values.ToList();
    }
}
