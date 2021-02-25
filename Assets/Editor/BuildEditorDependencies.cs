using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BuildEditorDependencies
{
    [MenuItem("Data Files/Build Editor Dependencies")]
    public static void BuildEditorDependenciesFunc()
    {
        string filepath = "Assets/External/editorDependencies.json";
        
        Dictionary<string, List<string>> typeLists = new Dictionary<string, List<string>>();

        List<string> tagTypes = new List<string>(Enum.GetNames(typeof(TagType)));
        typeLists.Add("TagType", tagTypes);

        List<string> bonusTypes = new List<string>(Enum.GetNames(typeof(BonusStatType)));
        typeLists.Add("BonusStatType", bonusTypes);

        List<string> elementTypes = new List<string>(Enum.GetNames(typeof(ElementType)));
        typeLists.Add("ElementType", elementTypes);

        List<string> rarityTypes = new List<string>(Enum.GetNames(typeof(RarityType)));
        typeLists.Add("RarityType", rarityTypes);

        List<string> effectTypes = new List<string>(Enum.GetNames(typeof(EffectType)));
        typeLists.Add("EffectType", effectTypes);

        List<string> difficultyTypes = new List<string>(Enum.GetNames(typeof(DifficultyType)));
        typeLists.Add("DifficultyType", difficultyTypes);

        string o = JsonConvert.SerializeObject(typeLists);
        System.IO.File.WriteAllText(filepath, o);

    }
}
