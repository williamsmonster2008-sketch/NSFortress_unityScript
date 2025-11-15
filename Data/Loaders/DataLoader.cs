using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class DataLoader : MonoBehaviour
{
    public static DataLoader Instance { get; private set; }
    
    private string dataPath;
    
    void Awake()
    {
        Instance = this;
        dataPath = Path.Combine(Application.dataPath, "_Project/Data/Templates");
    }
    
    public T LoadJson<T>(string fileName)
    {
        string filePath = Path.Combine(dataPath, fileName);
        
        if (!File.Exists(filePath))
        {
            Debug.LogError($"❌ 文件不存在: {filePath}");
            return default(T);
        }
        
        string json = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<T>(json);
    }
    
    public CharacterTemplateDatabase LoadCharacterTemplates()
    {
        return LoadJson<CharacterTemplateDatabase>("character_templates.json");
    }
}