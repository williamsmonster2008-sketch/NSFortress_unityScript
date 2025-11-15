using System;
using System.Collections.Generic;

[Serializable]
public class GenerationNameSequence
{
    public List<string> names;
}

[Serializable]
public class GenerationNameConfig
{
    public List<GenerationNameSequence> commonGenerationNames;
    public List<GenerationNameSequence> eliteGenerationNames;
}

[Serializable]
public class SurnameRow
{
    public string social_class;
    public string surname_type;
    public string surname;
}

[Serializable]
public class NameRow
{
    public string social_class;
    public string gender;
    public string name;
}