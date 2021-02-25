using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

public class AffixBase
{
    [JsonProperty]
    public readonly string idName;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly AffixType affixType;

    [JsonProperty]
    public readonly int tier;

    [JsonProperty]
    public readonly int spawnLevel;

    [JsonProperty]
    public readonly List<AffixBonusProperty> affixBonuses;

    [JsonProperty]
    public readonly List<AffixTriggeredEffectBonusProperty> triggeredEffects;

    [JsonProperty]
    public readonly List<AffixWeight> spawnWeight;

    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public readonly List<TagType> groupTags;

    public string AffixBonusStatTypeString { get; private set; }

    public void SetAffixBonusStatTypeString()
    {
        int i = 0;
        string temp = "";
        foreach (AffixBonusProperty x in affixBonuses)
        {
            temp += x.bonusStatType.ToString();
            temp += "_";
            temp += x.modifyType.ToString();
            if (i + 1 != affixBonuses.Count)
            {
                temp += "_";
                i++;
            }
        }
        AffixBonusStatTypeString = temp;
    }
}

public struct AffixBonusProperty
{
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly BonusStatType bonusStatType;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly ModifyType modifyType;

    [JsonProperty]
    public readonly float minValue;

    [JsonProperty]
    public readonly float maxValue;

    [JsonProperty]
    public readonly bool readAsFloat;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly TagType restriction;
}

public struct AffixWeight
{
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly TagType type;

    [JsonProperty]
    public readonly int weight;
}

public enum AffixType
{
    Prefix,
    Suffix,
    Enchantment,
    Innate,
    MonsterMod,
    Unique
}