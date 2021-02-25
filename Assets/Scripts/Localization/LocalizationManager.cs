using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    private static readonly string defaultLang = "en-US";

    private static Dictionary<string, string> commonLocalizationData = new Dictionary<string, string>();
    private static Dictionary<string, string> archetypeLocalizationData = new Dictionary<string, string>();
    private static Dictionary<string, string> equipmentLocalizationData = new Dictionary<string, string>();
    private static Dictionary<string, string> uniqueLocalizationData = new Dictionary<string, string>();
    private static Dictionary<string, string> abilityLocalizationData = new Dictionary<string, string>();
    private static Dictionary<string, string> enemyLocalizationData = new Dictionary<string, string>();
    private static Dictionary<string, string> helpLocalizationData = new Dictionary<string, string>();
    private static ItemGenLocalization itemGenLocalization;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        LoadLocalization();
    }

    private static void LoadLocalization(string locale = "en-US")
    {
        if (locale == null)
            locale = defaultLang;
        string path = "json/localization/common." + locale;
        commonLocalizationData = JsonConvert.DeserializeObject<Dictionary<string, string>>(Resources.Load<TextAsset>(path).text);

        path = "json/localization/ability." + locale;
        abilityLocalizationData = JsonConvert.DeserializeObject<Dictionary<string, string>>(Resources.Load<TextAsset>(path).text);

        path = "json/localization/archetype." + locale;
        archetypeLocalizationData = JsonConvert.DeserializeObject<Dictionary<string, string>>(Resources.Load<TextAsset>(path).text);

        path = "json/localization/equipment." + locale;
        equipmentLocalizationData = JsonConvert.DeserializeObject<Dictionary<string, string>>(Resources.Load<TextAsset>(path).text);

        path = "json/localization/unique." + locale;
        uniqueLocalizationData = JsonConvert.DeserializeObject<Dictionary<string, string>>(Resources.Load<TextAsset>(path).text);

        path = "json/localization/enemy." + locale;
        enemyLocalizationData = JsonConvert.DeserializeObject<Dictionary<string, string>>(Resources.Load<TextAsset>(path).text);

        path = "json/localization/help." + locale;
        helpLocalizationData = JsonConvert.DeserializeObject<Dictionary<string, string>>(Resources.Load<TextAsset>(path).text);

        path = "json/localization/itemgen." + locale;
        itemGenLocalization = JsonConvert.DeserializeObject<ItemGenLocalization>(Resources.Load<TextAsset>(path).text);
    }

    public string GetLocalizationText_HelpString(string helpId)
    {
        if (helpLocalizationData.TryGetValue(helpId, out string value))
        {
            MatchCollection regexMatches = Regex.Matches(value, @"{([^}]*)}");
            if (regexMatches.Count > 0)
            {
                foreach (Match y in regexMatches)
                {
                    if (y.Groups[1].Value == helpId)
                        continue;
                    value = value.Replace(y.Groups[0].Value, GetLocalizationText_HelpString(y.Groups[1].Value));
                }
            }
            return value;
        }
        else
            return "";
    }

    public string GetLocalizationText(string stringId)
    {
        if (commonLocalizationData.TryGetValue(stringId, out string value))
        {
            if (value == "")
                return stringId;
            return value;
        }
        else
        {
            return stringId;
        }
    }

    public string GetLocalizationText(AbilityTargetType targetType)
    {
        string stringId = "targetType." + targetType.ToString();
        if (commonLocalizationData.TryGetValue(stringId, out string value))
        {
            if (value == "")
                return stringId;
            return value;
        }
        else
        {
            return stringId;
        }
    }

    public string GetLocalizationText(EquipSlotType equipSlot)
    {
        return GetLocalizationText("slotType." + equipSlot.ToString());
    }

    public string[] GetLocalizationText_Ability(string stringId)
    {
        string[] output = new string[3];
        if (abilityLocalizationData.TryGetValue("ability." + stringId + ".name", out string value))
        {
            if (value == "")
                output[0] = stringId;
            else
                output[0] = value;
        }
        else
        {
            output[0] = stringId;
        }

        if (abilityLocalizationData.TryGetValue("ability." + stringId + ".text", out value))
        {
            output[1] = value;
        }
        else
        {
            output[1] = "";
        }

        return output;
    }

    public string GetLocalizationText_Element(ElementType type)
    {
        return GetLocalizationText("elementType." + type.ToString());
    }

    public static string BuildElementalDamageString(string s, ElementType element)
    {
        switch (element)
        {
            case ElementType.Fire:
                s = "<color=#e03131>" + s + "</color>";
                break;

            case ElementType.Cold:
                s = "<color=#33b7e8>" + s + "</color>";
                break;

            case ElementType.Lightning:
                s = "<color=#9da800>" + s + "</color>";
                break;

            case ElementType.Earth:
                s = "<color=#7c5916>" + s + "</color>";
                break;

            case ElementType.Divine:
                s = "<color=#f29e02>" + s + "</color>";
                break;

            case ElementType.Void:
                s = "<color=#56407c>" + s + "</color>";
                break;

            default:
                break;
        }

        s = "<sprite=" + (int)element + "> " + s;
        return s;
    }

    /*
    public string GetLocalizationText_AbilityBaseDamage(int level, AbilityBase ability)
    {
        string s = "";
        string weaponDamageText, baseDamageText;

        if (ability.abilityType == AbilityType.AURA || ability.abilityType == AbilityType.SELF_BUFF)
        {
            baseDamageText = GetLocalizationText("UI_ADD_DAMAGE");
            foreach (KeyValuePair<ElementType, AbilityDamageBase> damage in ability.damageLevels)
            {
                var d = damage.Value.damage[level];
                s += string.Format(baseDamageText, BuildElementalDamageString("<b>" + d.min + "~" + d.max + "</b>", damage.Key)) + "\n";
            }
        }
        else
        {
            if (ability.abilityType == AbilityType.ATTACK)
            {
                weaponDamageText = GetLocalizationText("UI_DEAL_DAMAGE_WEAPON");
                float d = ability.weaponMultiplier + ability.weaponMultiplierScaling * level;
                s += string.Format(weaponDamageText, d) + "\n";
            }

            baseDamageText = GetLocalizationText("UI_DEAL_DAMAGE_FIXED");
            foreach (KeyValuePair<ElementType, AbilityDamageBase> damage in ability.damageLevels)
            {
                var d = damage.Value.damage[level];
                s += string.Format(baseDamageText, BuildElementalDamageString("<b>" + d.min + "~" + d.max + "</b>", damage.Key)) + "\n";
            }

            if (ability.hitCount > 1)
            {
                s += "Hits " + ability.hitCount + "x at " + ability.hitDamageModifier.ToString("P1") + " Damage";
            }
        }
        return s;
    }
    */
    /*
    public string GetLocalizationText_AbilityCalculatedDamage(Dictionary<ElementType, ActorAbility.AbilityDamageContainer> damageDict)
    {
        string s = "";
        string damageText = GetLocalizationText("UI_DEAL_DAMAGE_FIXED");
        foreach (KeyValuePair<ElementType, ActorAbility.AbilityDamageContainer> damageData in damageDict)
        {
            if (damageData.Value.calculatedRange.IsZero())
                continue;
            s += string.Format(damageText, BuildElementalDamageString("<b>" + damageData.Value.calculatedRange.min + "~" + damageData.Value.calculatedRange.max + "</b>", damageData.Key)) + "\n";
        }
        return s;
    }
    */

    public string GetLocalizationText(EquipmentBase equipment)
    {
        string stringId = equipment.idName;
        if (equipmentLocalizationData.TryGetValue("equipment." + stringId, out string value))
        {
            if (value == "")
                return stringId;
            return value;
        }
        else
        {
            return stringId;
        }
    }

    public string[] GetLocalizationText(UniqueBase unique)
    {
        string stringId = unique.idName;
        string[] output = new string[2];
        if (uniqueLocalizationData.TryGetValue("unique." + stringId, out string value))
        {
            if (value == "")
                output[0] = stringId;
            else
                output[0] = value;
        }
        else
        {
            output[0] = stringId;
        }

        if (uniqueLocalizationData.TryGetValue("unique." + stringId + ".text", out string desc))
        {
            output[1] = desc;
        }

        return output;
    }

    public string GetLocalizationText_ArchetypeName(string stringId)
    {
        if (archetypeLocalizationData.TryGetValue("archetype." + stringId + ".name", out string value))
        {
            if (value == "")
                return stringId;
            return value;
        }
        else
        {
            return stringId;
        }
    }

    public string GetLocalizationText_Enemy(string stringId, string field)
    {
        if (enemyLocalizationData.TryGetValue("enemy." + stringId + field, out string value))
        {
            if (value == "")
                return stringId;
            return value;
        }
        else
        {
            return stringId;
        }
    }

    public string GetLocalizationText(TagType tagType)
    {
        string stringId = tagType.ToString();
        if (commonLocalizationData.TryGetValue("tagType." + stringId, out string value))
        {
            if (value == "")
                return stringId;
            return value;
        }
        else
        {
            return stringId;
        }
    }

    public string GetLocalizationText_TagTypeRestriction(TagType tagType)
    {
        string stringId = tagType.ToString();
        if (commonLocalizationData.TryGetValue("tagType." + stringId + ".restriction", out string value))
        {
            if (value == "")
            {
                Debug.Log("NOT FOUND " + tagType);
                return stringId;
            }

            if (value.Contains("{plural}"))
                value = value.Replace("{plural}", GetLocalizationText_TagTypePlural(stringId));
            else if (value.Contains("{single}"))
                value = value.Replace("{single}", GetLocalizationText(tagType));

            return value;
        }
        else
        {
            return stringId;
        }
    }

    public string GetLocalizationText_TagTypePlural(string stringId)
    {
        if (commonLocalizationData.TryGetValue("tagType." + stringId + ".plural", out string value))
        {
            if (value == "")
                return stringId;
            return value;
        }
        else
        {
            return stringId;
        }
    }

    private static string ParseBonusTypeFallback(string b)
    {
        Debug.LogWarning(b + " NOT FOUND");
        b = b.Replace("_", " ");
        return b.ToLower();
    }

    public string GetLocalizationText_TriggeredEffect(TriggeredEffectBonusBase triggeredEffect, float value, float? maxValue = null)
    {
        commonLocalizationData.TryGetValue("triggerType." + triggeredEffect.triggerType.ToString(), out string s);
        string valueString, effectString, bonusString;

        return s;
    }

    public string GetLocalizationText_BonusType(BonusStatType type, ModifyType modifyType, float value, TagType restriction, float duration = 0)
    {
        string output = GetBonusTypeString(type);

        if (restriction != TagType.DefaultTag)
        {
            output = GetLocalizationText_TagTypeRestriction(restriction) + ", " + output;
        }
        /*
        if (type >= (BonusStatType)HeroArchetypeData.SpecialBonusStart)
        {
            return output + "\n";
        }
        */
        output += " <nobr>";

        switch (modifyType)
        {
            case ModifyType.FlatAddition:
                if (value >= 0)
                    output += "+" + value;
                else
                    output += value;
                break;

            case ModifyType.Additive:
                if (value >= 0)
                    output += "+" + value + "%";
                else
                    output += value + "%";
                break;

            case ModifyType.Multiply:
                output += "x" + (1 + value / 100d).ToString("0.00##");
                break;

            case ModifyType.FixedToValue:
                output += "is " + value;
                break;
        }

        if (duration > 0)
        {
            output += " for " + duration.ToString("N1") + "s";
        }

        output += "</nobr>\n";

        return output;
    }

    public string GetLocalizationText_BonusType(BonusStatType type, ModifyType modifyType, float minVal, float maxVal, TagType restriction)
    {
        string output = GetBonusTypeString(type);

        if (restriction != TagType.DefaultTag)
        {
            output = GetLocalizationText_TagTypeRestriction(restriction) + ", " + output;
        }

        string valueString;
        if (minVal == maxVal)
            valueString = minVal.ToString();
        else
            valueString = "(" + minVal + "-" + maxVal + ")";

        switch (modifyType)
        {
            case ModifyType.FlatAddition:
                output += " +" + valueString + "\n";
                break;

            case ModifyType.Additive:
                output += " +" + valueString + "%" + "\n";
                break;

            case ModifyType.Multiply:
                output += " x(" + (1 + minVal / 100d).ToString(".00##") + "-" + (1 + maxVal / 100d).ToString(".00##") + ")\n";
                break;

            case ModifyType.FixedToValue:
                output += " is " + valueString + "\n";
                break;
        }

        return output;
    }

    public string GetRequirementText(Equipment equip)
    {
        string s = "Requires ";
        if (equip.levelRequirement > 0)
        {
            s += "Lv" + equip.levelRequirement;
        }
        if (equip.strRequirement > 0)
        {
            s += ", Str " + equip.strRequirement;
        }
        if (equip.intRequirement > 0)
        {
            s += ", Int " + equip.intRequirement;
        }
        if (equip.dexRequirement > 0)
        {
            s += ", Dex " + equip.dexRequirement;
        }
        return s.Trim(' ', ',');
    }

    public string GetBonusTypeString(BonusStatType type)
    {
        if (commonLocalizationData.Count == 0)
            LocalizationManager.LoadLocalization();

        if (commonLocalizationData.TryGetValue("bonusStatType." + type.ToString(), out string output))
        {
            if (output == "")
            {
                output = ParseBonusTypeFallback(type.ToString());
            }
        }
        else
        {
            output = ParseBonusTypeFallback(type.ToString());
        }

        return output;
    }

    public string GenerateRandomItemName(ICollection<TagType> tags)
    {
        if (itemGenLocalization == null)
        {
            LoadLocalization();
        }
        List<string> prefixes = new List<string>(itemGenLocalization.CommonPrefixes);
        List<string> suffixes = new List<string>(itemGenLocalization.CommonSuffixes);

        /*
        if (itemGenLocalization.prefix.TryGetValue(type, out temp))
        {
            prefixes.AddRange(temp);
        }
        */
        foreach (TagType type in tags)
        {
            if (itemGenLocalization.suffix.TryGetValue(type, out List<string> temp))
            {
                suffixes.AddRange(temp);
            }
        }

        string s = "";

        s += prefixes[UnityEngine.Random.Range(0, prefixes.Count)] + " ";
        s += suffixes[UnityEngine.Random.Range(0, suffixes.Count)];

        return s;
    }

    private class ItemGenLocalization
    {
        public Dictionary<TagType, List<string>> prefix;
        public Dictionary<TagType, List<string>> suffix;

        public List<string> CommonPrefixes => prefix[TagType.DefaultTag];
        public List<string> CommonSuffixes => suffix[TagType.DefaultTag];
    }
}