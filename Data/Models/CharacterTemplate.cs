using System;
using System.Collections.Generic;

/// <summary>
/// 从JSON加载的角色模板
/// </summary>
[Serializable]
public class CharacterTemplate
{
    public string id;
    public string name;
    public int[] ageRange;
    public string gender;
    public string socialClass;
    public int[] healthRange;
    public List<string> skillPresets;
    public Dictionary<string, int[]> virtueRanges;
}

[Serializable]
public class CharacterTemplateDatabase
{
    public List<CharacterTemplate> templates;
}