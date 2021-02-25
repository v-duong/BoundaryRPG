using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

public class AbilityBase
{
    [JsonProperty]
    public readonly string idName;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly AbilityType abilityType;

    [JsonProperty]
    public readonly float speedModifier;

    [JsonProperty]
    public readonly float baseCritical;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly AbilityTargetType targetType;
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly AbilityTargetRange rangeType;

    [JsonProperty]
    public readonly float weaponMultiplier;

    [JsonProperty]
    public readonly float weaponMultiplierScaling;

    [JsonProperty]
    public readonly float manaCost;
    [JsonProperty]
    public readonly float manaCostMultiplier;

    [JsonProperty]
    public readonly float cooldownTime;

    [JsonProperty]
    public readonly Dictionary<ElementType, List<int>> damageLevels;

    [JsonProperty]
    public readonly float flatDamageMultiplier;

    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    private readonly List<TagType> groupTypes;

    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public readonly List<TagType> requiredRestrictions;

    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public readonly List<TagType> singleRequireRestrictions;

    [JsonProperty]
    public readonly List<LevelScaledBonusProperty> bonusProperties;

    [JsonProperty]
    public readonly List<LevelScaledAddedEffect> appliedEffects;

    [JsonProperty]
    public readonly List<AffixTriggeredEffectBonusProperty> triggeredEffects;

    [JsonProperty]
    public readonly string effectSprite;

    [JsonProperty]
    public readonly bool useBothWeaponsForDual;

    [JsonProperty]
    public int hitCount;

    [JsonProperty]
    public float hitDamageModifier;

    public string LocalizedName => LocalizationManager.Instance.GetLocalizationText_Ability(idName)[0];
    public string[] LocalizationStrings => LocalizationManager.Instance.GetLocalizationText_Ability(idName);

    public int GetDamageAtLevel(ElementType e, int level)
    {
        if (damageLevels.TryGetValue(e, out List<int> damageBase))
        {
            return damageBase[level];
        }
        else
        {
            return 0;
        }
    }

    public IList<TagType> GetTagTypes()
    {
        return groupTypes.AsReadOnly();
    }

    public string GetAbilityBonusTexts(int abilityLevel)
    {
        string infoText = "";
        /*
        foreach (AbilityScalingBonusProperty bonusProperty in bonusProperties)
        {
            infoText += "○ " + LocalizationManager.Instance.GetLocalizationText_BonusType(bonusProperty.bonusType,
                                                                                        bonusProperty.modifyType,
                                                                                        bonusProperty.initialValue + bonusProperty.growthValue * abilityLevel,
                                                                                        bonusProperty.restriction);
        }

        foreach (AbilityScalingAddedEffect appliedEffect in appliedEffects)
        {
            if (appliedEffect.effectType == EffectType.BUFF || appliedEffect.effectType == EffectType.DEBUFF)
            {
                infoText += "○ " + LocalizationManager.Instance.GetLocalizationText_BonusType(appliedEffect.bonusType,
                                                                             appliedEffect.modifyType,
                                                                             appliedEffect.initialValue + appliedEffect.growthValue * abilityLevel,
                                                                             TagType.NO_GROUP, appliedEffect.duration);
            } else
            {
                infoText += LocalizationManager.Instance.GetLocalizationText_EffectType_Aura(appliedEffect.effectType,
                                                                                             appliedEffect.initialValue + appliedEffect.growthValue * abilityLevel,
                                                                                             appliedEffect.duration,
                                                                                             1,
                                                                                             1);
            }
        }

        foreach (TriggeredEffectBonusProperty triggeredEffect in triggeredEffects)
        {
            infoText += "○ " + LocalizationManager.Instance.GetLocalizationText_TriggeredEffect(triggeredEffect, triggeredEffect.effectMaxValue);
        }
        */

        return infoText;
    }
}

public enum AbilityType
{
    Attack,
    Magic,
    Support
}

public class LevelScaledAddedEffect
{
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly EffectType effectType;

    [JsonProperty]
    public readonly float chance;

    [JsonProperty]
    public readonly float initialValue;

    [JsonProperty]
    public readonly float growthValue;

    [JsonProperty]
    public readonly int stacks;
}

public struct LevelScaledBonusProperty
{
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly BonusStatType bonusType;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly ModifyType modifyType;

    [JsonProperty]
    public readonly float initialValue;

    [JsonProperty]
    public readonly float growthValue;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly TagType restriction;
}
