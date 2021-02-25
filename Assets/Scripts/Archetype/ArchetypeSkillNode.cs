using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using UnityEngine;

public class ArchetypeSkillNode
{
    [JsonProperty]
    public readonly int id;

    [JsonProperty]
    public readonly string idName;

    [JsonProperty]
    public readonly int initialLevel;

    [JsonProperty]
    public readonly int maxLevel;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly NodeType type;

    [JsonProperty]
    public readonly List<NodeScalingBonusProperty> bonuses;

    [JsonProperty]
    public readonly List<ArchetypeTriggeredEffectNodeProperty> triggeredEffects;

    [JsonProperty]
    public readonly Vector2 nodePosition;

    [JsonProperty]
    public readonly string iconPath;

    [JsonProperty]
    public readonly List<int> children;


    public string GetBonusInfoString(int currentLevel)
    {
        string s = "";
        
        foreach (NodeScalingBonusProperty bonus in bonuses)
        {
            float value = bonus.GetBonusValueAtLevel(currentLevel, maxLevel);
            if (bonus.bonusType < (BonusStatType)Helpers.SPECIAL_BONUS_START
                && value == 0
                && (bonus.modifyType != ModifyType.Multiply && bonus.modifyType != ModifyType.FixedToValue))
            {
                continue;
            }

            s += "\u2022 " + LocalizationManager.Instance.GetLocalizationText_BonusType(bonus.bonusType, bonus.modifyType, value, bonus.restriction);
        }

        foreach (var x in triggeredEffects)
        {
            s += "\u2022 " + LocalizationManager.Instance.GetLocalizationText_TriggeredEffect(x, x.effectGrowthValue);
        }
        
        return s;
    }
}

public struct NodeScalingBonusProperty
{
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly BonusStatType bonusType;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly ModifyType modifyType;

    [JsonProperty]
    public readonly float growthValue;

    [JsonProperty]
    public readonly float finalLevelValue;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly TagType restriction;

    public float GetBonusValueAtLevel(int level, int maxLevel)
    {
        if (maxLevel == 1)
            return growthValue;
        else if (level != maxLevel)
            return growthValue * level;
        else if (level == maxLevel)
            return growthValue * (maxLevel - 1) + finalLevelValue;
        else
            return 0;
    }
}

public enum NodeType
{
    Lesser,
    Greater,
    Master
}