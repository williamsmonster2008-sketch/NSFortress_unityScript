using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

[System.Serializable]
public struct GeneratedNameEntry
{
    public string fullName;
    public string surname;
    public string generationChar;
}

[CreateAssetMenu(fileName = "CharacterNameDatabase", menuName = "Game/Character Name Database")]
public class CharacterNameDatabase : ScriptableObject
{
    [Title("配置文件路径")]
    public TextAsset surnameCSV;
    public TextAsset nameCSV;
    public TextAsset generationJSON;
    
    [Title("运行时数据")]
    [ShowInInspector, ReadOnly]
    private Dictionary<string, List<string>> surnamesByClass = new Dictionary<string, List<string>>();
    
    [ShowInInspector, ReadOnly]
    private Dictionary<string, Dictionary<string, List<string>>> namesByClassAndGender = 
        new Dictionary<string, Dictionary<string, List<string>>>();
    
    [ShowInInspector, ReadOnly]
    private GenerationNameConfig generationConfig;
    
    private Dictionary<string, int> familyGenerationSequence = new Dictionary<string, int>();
    private HashSet<int> usedCommonSequences = new HashSet<int>();
    private HashSet<int> usedEliteSequences = new HashSet<int>();
    
    private bool initialized = false;
    
    public void Initialize()
    {
        Debug.Log("🔍 Initialize() 被调用");
        
        // 即使initialized=true，如果数据为空也要重新加载
        if (initialized && surnamesByClass.Count > 0)
        {
            Debug.Log("⚠️ 已经初始化且有数据，跳过");
            return;
        }
        
        LoadSurnameCSV();
        LoadNameCSV();
        LoadGenerationJSON();
        
        initialized = true;
        Debug.Log("✅ CharacterNameDatabase 初始化完成");        
    }
    
    private void LoadSurnameCSV()
    {
        Debug.Log("🔍 开始加载姓氏CSV");
        if (surnameCSV == null)
        {
            Debug.LogError("❌ surnameCSV 未配置");
            return;
        }
        
        string[] lines = surnameCSV.text.Split('\n');
        
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            
            string[] fields = line.Split(',');
            if (fields.Length < 3) continue;
            
            string socialClass = fields[0].Trim();
            string surname = fields[2].Trim();
            // �������е���
            if (i <= 10) // ֻ��ӡǰ10��
            {
                Debug.Log($"  第{i}行: socialClass='{socialClass}', surname='{surname}'");
            }
            
            if (!surnamesByClass.ContainsKey(socialClass))
            {
                surnamesByClass[socialClass] = new List<string>();
            }
            
            surnamesByClass[socialClass].Add(surname);
        }
        
        Debug.Log($"✅ 加载 {surnamesByClass.Count} 个阶层的姓氏");
        Debug.Log($"📊 姓氏池详细统计:");
        foreach (var socialClass in surnamesByClass.Keys)
        {
            Debug.Log($"  {socialClass}: {surnamesByClass[socialClass].Count}个姓氏");
            if (surnamesByClass[socialClass].Count > 0)
            {
                int showCount = Mathf.Min(5, surnamesByClass[socialClass].Count);
                string[] samples = new string[showCount];
                for (int i = 0; i < showCount; i++) samples[i] = surnamesByClass[socialClass][i];
                Debug.Log($"    示例: {string.Join(", ", samples)}");
            }
        }
    }
    
    private void LoadNameCSV()
    {
        if (nameCSV == null)
        {
            Debug.LogError("❌ nameCSV 未配置");
            return;
        }
        
        string[] lines = nameCSV.text.Split('\n');
        
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            
            string[] fields = line.Split(',');
            if (fields.Length < 3) continue;
            
            string socialClass = fields[0].Trim();
            string gender = fields[1].Trim();
            string name = fields[2].Trim();
            
            if (!namesByClassAndGender.ContainsKey(socialClass))
            {
                namesByClassAndGender[socialClass] = new Dictionary<string, List<string>>();
            }
            
            if (!namesByClassAndGender[socialClass].ContainsKey(gender))
            {
                namesByClassAndGender[socialClass][gender] = new List<string>();
            }
            
            namesByClassAndGender[socialClass][gender].Add(name);
        }
        
        Debug.Log($"✅ 加载 {namesByClassAndGender.Count} 个阶层的名字");

       // 添加详细统计
        Debug.Log($"📊 名字池详细统计:");
        foreach (var socialClass in namesByClassAndGender.Keys)
        {
            int maleCount = namesByClassAndGender[socialClass].ContainsKey("男") ? 
                namesByClassAndGender[socialClass]["男"].Count : 0;
            int femaleCount = namesByClassAndGender[socialClass].ContainsKey("女") ? 
                namesByClassAndGender[socialClass]["女"].Count : 0;
            Debug.Log($"  {socialClass}: 男{maleCount}个, 女{femaleCount}个");
            
            // 显示前5个男名和女名
            if (maleCount > 0)
            {
                var maleNames = namesByClassAndGender[socialClass]["男"].Take(5);
                Debug.Log($"     男名示例: {string.Join(", ", maleNames)}");
            }
            if (femaleCount > 0)
            {
                var femaleNames = namesByClassAndGender[socialClass]["女"].Take(5);
                Debug.Log($"    女名示例: {string.Join(", ", femaleNames)}");
            }
        }
    }
    
    private void LoadGenerationJSON()
    {
        if (generationJSON == null)
        {
            Debug.LogError("❌ generationJSON 未配置");
            return;
        }
        
        generationConfig = JsonUtility.FromJson<GenerationNameConfig>(generationJSON.text);
        
        if (generationConfig == null)
        {
            Debug.LogError("❌ generationConfig 解析失败");
            return;
        }
        
        Debug.Log($"✅ 加载 {generationConfig.commonGenerationNames.Count} 组平民辈分字");
        Debug.Log($"✅ 加载 {generationConfig.eliteGenerationNames.Count} 组士族辈分字");
    }
    
    public GeneratedNameEntry GenerateNameEntry(string socialClass, Gender gender, string familyName, int generation, System.Random rand, string forcedSurname = null)
    {
        if (!initialized) Initialize();
        
        string surname = !string.IsNullOrEmpty(forcedSurname)
            ? forcedSurname
            : (string.IsNullOrEmpty(familyName)
                ? GetRandomSurname(socialClass, rand)
                : familyName);
        
        string generationChar = string.Empty;
        if (socialClass != "胡族")
        {
            generationChar = GetGenerationChar(surname, socialClass, generation, rand);
        }
        
        string givenName = GetRandomGivenName(socialClass, gender, rand);
        int attempts = 0;
        while (givenName == generationChar && attempts < 5)
        {
            givenName = GetRandomGivenName(socialClass, gender, rand);
            attempts++;
        }
        
        if (givenName == generationChar)
        {
            givenName = gender == Gender.Male ? "之" : "华";
        }
        
        return new GeneratedNameEntry
        {
            fullName = surname + generationChar + givenName,
            surname = surname,
            generationChar = generationChar
        };
    }
    
    public string GenerateFullName(string socialClass, Gender gender, string familyName, int generation, System.Random rand)
    {
        return GenerateNameEntry(socialClass, gender, familyName, generation, rand).fullName;
    }
    
    private string GetGenerationChar(string familyName, string socialClass, int generation, System.Random rand)
    {
        if (generationConfig == null) return "";
        
        if (!familyGenerationSequence.ContainsKey(familyName))
        {
            AssignGenerationSequence(familyName, socialClass, rand);
        }
        
        int sequenceIndex = familyGenerationSequence[familyName];
        
        // 修改这里：使用CSV中的实际阶层名称
        bool isElite = socialClass == "门阀士族" || socialClass == "庶族";
        var sequences = isElite ? generationConfig.eliteGenerationNames : generationConfig.commonGenerationNames;
        
        if (sequenceIndex >= sequences.Count) sequenceIndex = 0;
        
        var sequence = sequences[sequenceIndex].names;
        
        int charIndex = (generation - 1) % sequence.Count;
        
        return sequence[charIndex];
    }

    private void AssignGenerationSequence(string familyName, string socialClass, System.Random rand)
    {
        // 修改这里：使用CSV中的实际阶层名称
        bool isElite = socialClass == "门阀士族" || socialClass == "庶族";
        var sequences = isElite ? generationConfig.eliteGenerationNames : generationConfig.commonGenerationNames;
        var usedSet = isElite ? usedEliteSequences : usedCommonSequences;
        
        int sequenceIndex = -1;
        for (int i = 0; i < sequences.Count; i++)
        {
            if (!usedSet.Contains(i))
            {
                sequenceIndex = i;
                usedSet.Add(i);
                break;
                }
        }
        
        if (sequenceIndex == -1)
        {
            sequenceIndex = rand.Next(sequences.Count);
        }
        
        familyGenerationSequence[familyName] = sequenceIndex;
    }
    
    public string GetRandomSurname(string socialClass, System.Random rand)
    {
        if (!initialized) Initialize();
        
        Debug.Log($"🔍 GetRandomSurname - socialClass: '{socialClass}', 是否包含key: {surnamesByClass.ContainsKey(socialClass)}");
        
        if (surnamesByClass.ContainsKey(socialClass))
        {            
            if (surnamesByClass[socialClass].Count > 0)
            {
                int index = rand.Next(surnamesByClass[socialClass].Count);
                string surname = surnamesByClass[socialClass][index];                
                return surname;
            }
        }
        
        Debug.LogWarning($"⚠️ 未找到 '{socialClass}' 的姓氏，使用默认");
        return "李";
    }
    
    public string GetRandomGivenName(Gender gender, System.Random rand)
    {
        return GetRandomGivenName("平民", gender, rand);  
    }
    
    public string GetRandomGivenName(string socialClass, Gender gender, System.Random rand)
    {
        if (!initialized) Initialize();
        
        string genderKey = gender == Gender.Male ? "男" : "女";        
        
        if (namesByClassAndGender.ContainsKey(socialClass))
        {           
            if (namesByClassAndGender[socialClass].ContainsKey(genderKey))
            {
                int count = namesByClassAndGender[socialClass][genderKey].Count;                
                if (count > 0)
                {
                    int index = rand.Next(count);
                    string name = namesByClassAndGender[socialClass][genderKey][index];                    
                    return name;
                }
            }
        }
        
        Debug.LogWarning($"⚠️ 未找到名字，使用默认");
        
        return gender == Gender.Male ? "明" : "华";
    }
    
    public string GetRandomFamilyName(string socialClass, System.Random rand)
    {
        return GetRandomSurname(socialClass, rand);
    }
}
