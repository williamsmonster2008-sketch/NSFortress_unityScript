using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameSystems;

/// <summary>
/// 时辰显示UI - 显示古代十二时辰和刻数
/// </summary>
public class TimeDisplayUI : MonoBehaviour
{
    [Header("引用")]
    public TimeSystem timeSystem;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI descriptionText;  // 时辰描述
    public Image backgroundImage;  // 背景底纹
    
    [Header("配置")]
    public bool showDetailedTime = true;  // 显示刻数
    public bool showDescription = true;   // 显示时辰描述
    public bool autoCreateBackground = true;  // 自动创建背景
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.6f);  // 半透明黑色
    
    void Start()
    {
        if (timeSystem == null)
        {
            timeSystem = FindObjectOfType<TimeSystem>();
        }
        
        if (timeSystem == null)
        {
            Debug.LogError("❌ TimeDisplayUI: 找不到TimeSystem!");
            enabled = false;
            return;
        }
        
        if (timeText == null)
        {
            Debug.LogError("❌ TimeDisplayUI: timeText未设置!");
            enabled = false;
            return;
        }
        
        // 自动创建背景
        if (autoCreateBackground && backgroundImage == null)
        {
            CreateBackground();
        }
        
        // 设置背景颜色
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
        }
    }
    
    void Update()
    {
        if (timeSystem != null && timeText != null)
        {
            UpdateTimeDisplay();
        }
    }
    
    void UpdateTimeDisplay()
    {
        float dayProgress = timeSystem.GetDayProgress();
        
        // 获取十二时辰和刻数
        string shiChen = GetShiChenName(dayProgress, out int ke);
        string keText = GetChineseNumber(ke + 1);  // 显示为1-8刻
        
        // 更新主时间显示
        if (showDetailedTime)
        {
            timeText.text = $"{shiChen}{keText}刻";
        }
        else
        {
            timeText.text = shiChen;
        }
        
        // 更新时辰描述
        if (showDescription && descriptionText != null)
        {
            descriptionText.text = GetTimeOfDayDescription(timeSystem.GetCurrentTimeOfDay());
        }
    }
    
    /// <summary>
    /// 自动创建背景Image
    /// </summary>
    void CreateBackground()
    {
        GameObject bgObject = new GameObject("TimeDisplay_Background");
        bgObject.transform.SetParent(timeText.transform, false);
        
        // 添加Image组件
        backgroundImage = bgObject.AddComponent<Image>();
        backgroundImage.color = backgroundColor;
        
        // 设置为Text的兄弟节点，并置于Text之前（显示在下层）
        bgObject.transform.SetSiblingIndex(0);
        
        // 调整RectTransform，比文字稍大
        RectTransform bgRect = bgObject.GetComponent<RectTransform>();
        RectTransform textRect = timeText.GetComponent<RectTransform>();
        
        bgRect.anchorMin = textRect.anchorMin;
        bgRect.anchorMax = textRect.anchorMax;
        bgRect.pivot = textRect.pivot;
        bgRect.anchoredPosition = textRect.anchoredPosition;
        
        // 比文字大一圈（padding）
        bgRect.sizeDelta = textRect.sizeDelta + new Vector2(20f, 10f);
        
        Debug.Log("✅ 已自动创建时间UI背景");
    }
    
    /// <summary>
    /// 根据一天进度获取十二时辰名称和刻数
    /// </summary>
    /// <param name="dayProgress">一天进度 0-1</param>
    /// <param name="ke">输出刻数 0-7（显示时+1变为一刻到八刻）</param>
    /// <returns>时辰名称</returns>
    string GetShiChenName(float dayProgress, out int ke)
    {
        // 一天24小时 = 12时辰
        // 每个时辰 = 2小时 = 8刻
        
         // 直接使用TimeSystem的计算结果
        TimeOfDay currentTime = timeSystem.GetCurrentTimeOfDay();
        
        // 时辰对照表
        // 子时: 23-1时
        // 丑时: 1-3时
        // 寅时: 3-5时
        // ...
        
        // 计算刻数
        float hour = dayProgress * 24f;
        float adjustedHour = (hour + 1f) % 24f;
        float hourInShiChen = adjustedHour % 2f;
        ke = Mathf.FloorToInt(hourInShiChen / 0.25f);
        ke = Mathf.Clamp(ke, 0, 7);
        
        return currentTime.ToString();      
    }
    
    /// <summary>
    /// 将数字转换为中文数字
    /// </summary>
    string GetChineseNumber(int num)
    {
        string[] chineseNumbers = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九", "十" };
        
        if (num >= 0 && num <= 10)
        {
            return chineseNumbers[num];
        }
        
        return num.ToString();
    }
    
    /// <summary>
    /// 获取时辰描述
    /// </summary>
    string GetTimeOfDayDescription(TimeOfDay timeOfDay)
    {
        switch (timeOfDay)
        {
            case TimeOfDay.子时: return "夜半，又名子夜、中夜";
            case TimeOfDay.丑时: return "鸡鸣，又名荒鸡";
            case TimeOfDay.寅时: return "平旦，又名黎明、早晨";
            case TimeOfDay.卯时: return "日出，又名日始、破晓";
            case TimeOfDay.辰时: return "食时，又名早食";
            case TimeOfDay.巳时: return "隅中，又名日禺";
            case TimeOfDay.午时: return "日中，又名日正、中午";
            case TimeOfDay.未时: return "日昳，又名日跌、日央";
            case TimeOfDay.申时: return "哺时，又名日铺、夕食";
            case TimeOfDay.酉时: return "日入，又名日落、黄昏";
            case TimeOfDay.戌时: return "黄昏，又名日夕、日暮";
            case TimeOfDay.亥时: return "人定，又名定昏";
            default: return "";
        }
    }
}