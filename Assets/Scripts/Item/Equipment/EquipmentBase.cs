using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

public class EquipmentBase
{
    [JsonProperty]
    public readonly string idName;

    [JsonProperty]
    public readonly int dropLevel;

    [JsonProperty]
    public readonly int sellValue;

    [JsonProperty]
    public readonly int strengthReq;

    [JsonProperty]
    public readonly int intelligenceReq;

    [JsonProperty]
    public readonly int dexterityReq;

    [JsonProperty]
    public readonly int damage;
    [JsonProperty]
    public readonly int damageSpread;

    [JsonProperty]
    public readonly float speedModifier;

    [JsonProperty]
    public readonly float criticalChance;

    [JsonProperty]
    public readonly int armor;

    [JsonProperty]
    public readonly int magicArmor;

    [JsonProperty]
    public readonly int dodgeRating;

    [JsonProperty]
    public readonly float blockChance;

    [JsonProperty]
    public readonly int blockProtection;

    [JsonProperty]
    [JsonConverter(typeof(StringEnumConverter))]
    public readonly EquipSlotType equipSlot;

    [JsonProperty]
    [JsonConverter(typeof(StringEnumConverter))]
    public readonly TagType groupTag;
    [JsonProperty]
    [JsonConverter(typeof(StringEnumConverter))]
    public readonly TagType additionalTag;

    [JsonProperty]
    public readonly bool hasInnate;

    [JsonProperty]
    public readonly string innateAffixId;

    [JsonProperty]
    public readonly int spawnWeight;

    public virtual string LocalizedName => LocalizationManager.Instance.GetLocalizationText(this);
}

public class UniqueBase : EquipmentBase
{
    [JsonProperty]
    public readonly List<AffixBase> fixedUniqueAffixes;

    [JsonProperty]
    public readonly List<AffixBase> randomUniqueAffixes;

    [JsonProperty]
    public readonly int randomAffixesToSpawn;

    [JsonProperty]
    public readonly int uniqueVersion;

    public override string LocalizedName => LocalizationManager.Instance.GetLocalizationText(this)[0];
}

public enum EquipSlotType
{
    Weapon,
    Offhand,
    BodyArmor,
    Headgear,
    Gloves,
    Boots,
    Belt,
    Necklace,
    RingSlot1,
    RingSlot2,
    Ring,
}