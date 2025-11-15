using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class SaveLoadManager : MonoBehaviour
{
    private string saveDirectory;
    
    private void Awake()
    {
        saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
        Debug.Log($"💾 存档目录: {saveDirectory}");
    }
    
    public bool SaveGame(string saveName, WorldSeed seed)
    {
        try
        {
            var saveData = new
            {
                version = "0.1.0",
                timestamp = System.DateTimeOffset.Now.ToUnixTimeSeconds(),
                saveName = saveName,
                worldSeed = seed
            };
            
            string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            string fileName = $"{saveName}.json";
            string filePath = Path.Combine(saveDirectory, fileName);
            
            File.WriteAllText(filePath, json);
            
            Debug.Log($"💾 游戏已保存: {fileName}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 保存失败: {e.Message}");
            return false;
        }
    }
}