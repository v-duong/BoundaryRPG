using System;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers
{
    public const int CURRENT_SAVE_VERSION = 0;
    public const int MAX_LEVEL_REQ = 95;
    public const int DEFAULT_RESISTANCE_CAP = 80;
    public const int HARD_RESISTANCE_CAP = 100;
    public const int BASE_ATTACK_PARRY_CAP = 50;
    public const int BASE_MAGIC_PARRY_CAP = 50;
    public const int BASE_BLOCK_CAP = 85;
    public const int BASE_BLOCK_PROTECTION_CAP = 90;
    public const int BASE_CRITICAL_DAMAGE = 50;

    public const int LOCAL_BONUS_START = LOCAL_ARMOR_BONUS_START;
    public const int LOCAL_ARMOR_BONUS_START = 0x700;
    public const int LOCAL_WEAPON_BONUS_START = 0x800;
    public const int LOCAL_BONUS_SEPARATION = 0x100;
    public const int SPECIAL_BONUS_START = 0xC00;
    public const int AFFIX_STRING_SPACING = 1;
    public const string AFFIX_RANGE_COLOR = "#666";

    public static readonly Color UNIQUE_COLOR = new Color(1.0f, 0.5f, 0.2f);
    public static readonly Color EPIC_COLOR = new Color(0.5f, 0.25f, 0.5f);
    public static readonly Color UNCOMMON_COLOR = new Color(0.25f, 0.41f, 0.56f);
    public static readonly Color RARE_COLOR = new Color(0.622f, 0.535f, 0.00f);
    public static readonly Color NORMAL_COLOR = new Color(0.4f, 0.4f, 0.4f);
    public static readonly Color SELECTION_COLOR = new Color(0.2f, 0.9f, 0.82f);

    public static readonly Color STR_ARCHETYPE_COLOR = new Color(0.6f, 0.27f, 0.27f);
    public static readonly Color INT_ARCHETYPE_COLOR = new Color(0.38f, 0.63f, 0.76f);
    public static readonly Color AGI_ARCHETYPE_COLOR = new Color(0.3f, 0.53f, 0.32f);

    public static double SCALING_FACTOR = 1.042;
    public static double LEVEL_SCALING_FACTOR = 0.402;
    public static double ENEMY_SCALING = 1.012;

    public static readonly List<string> ElementStrings = new List<string>() { "Physical", "Fire", "Cold", "Lightning", "Earth", "Divine", "Void" };

    public static bool RollChance(float chance)
    {
        if (chance <= 0f)
        {
            return false;
        }

        if (chance >= 1f)
        {
            return true;
        }
        return UnityEngine.Random.Range(0f, 1f) < chance ? true : false;
    }

    public static HashSet<BonusStatType> GetRelevantDamageBonuses(AbilityBase abilityBase, ElementType element)
    {
        HashSet<BonusStatType> ret = new HashSet<BonusStatType>();

        ret.Add(BonusStatType.Global_Damage);

        string typeString = "";
        if (abilityBase.abilityType == AbilityType.Attack)
        {
            ret.Add(BonusStatType.AttackDamage);
            typeString = "Attack";
        }
        else if (abilityBase.abilityType == AbilityType.Magic)
        {
            ret.Add(BonusStatType.MagicDamage);
            typeString = "Magic";
        }

        string s = element.ToString();

        if (Enum.TryParse(s, true, out TagType tag) && abilityBase.GetTagTypes().Contains(tag))
        {
            if (Enum.TryParse(typeString + s + "Damage", true, out BonusStatType bonus))
            {
                ret.Add(bonus);
            }
            if (Enum.TryParse("Global_" + s + "Damage", true, out BonusStatType globalBonus))
            {
                ret.Add(globalBonus);
            }
        }

        return ret;
    }

    public static double GetEnemyHealthScaling(double level)
    {
        // formula
        // (Scaling*(EnemyScaling))^(level*1.1 - 23) * (level*levelFactor*5) + level*2
        double enemyFactor = Math.Pow(SCALING_FACTOR * ENEMY_SCALING, level * 1.1 - 23) * level * LEVEL_SCALING_FACTOR * 5 + level * 2;

        return enemyFactor * 15;
    }

    public static double GetEnemyDamageScaling(double level)
    {
        // formula
        // (Scaling*(EnemyScaling))^(level/1.5 - 23) * (level*levelFactor) + level*2
        double enemyFactor = Math.Pow(SCALING_FACTOR * ENEMY_SCALING, level / 1.5 - 23) * level * LEVEL_SCALING_FACTOR + level * 2;

        return enemyFactor * 5;
    }

    public static double GetEnemyAccuracyScaling(double level)
    {
        // formula
        // (Scaling*(EnemyScaling))^(level/1.4 - 20) * (level*levelFactor) + level*4
        double enemyFactor = Math.Pow(SCALING_FACTOR * ENEMY_SCALING, level / 1.4 - 20) * level * LEVEL_SCALING_FACTOR + level * 4;

        return enemyFactor * 4;
    }

    public static int GetRequiredExperience(int level)
    {
        double exp = Math.Pow(SCALING_FACTOR, (level - 1) * 1.333 - 30) * (level - 1) * 500;
        return (int)exp;
    }

    public static Color GetArchetypeStatColor(ArchetypeBase archetype)
    {
        List<float> growths = new List<float>() { archetype.strengthGrowth, archetype.intelligenceGrowth, archetype.dexterityGrowth };
        int sameCount = 0, sameGrowthIndex = 0, highestIndex = 0, secondHighestIndex = 0;
        float highest = 0, secondHighest = 0, sum = 0;
        for (int i = 0; i < growths.Count; i++)
        {
            if (growths[i] > highest)
            {
                highest = growths[i];
                highestIndex = i;
            }
            sum += growths[i];
        }

        for (int j = 0; j < growths.Count; j++)
        {
            if (j == highestIndex)
            {
                continue;
            }
            else if (growths[j] == highest)
            {
                sameCount++;
                sameGrowthIndex = j;
            }
            else if (growths[j] > secondHighest)
            {
                secondHighest = growths[j];
                secondHighestIndex = j;
            }
        }

        if (sameCount >= 2)
            return NORMAL_COLOR;
        else if (sameCount == 1)
            return Color.Lerp(GetColorFromStatIndex(highestIndex), GetColorFromStatIndex(sameGrowthIndex), 0.5f);
        else
            return Color.Lerp(GetColorFromStatIndex(highestIndex), Color.Lerp(GetColorFromStatIndex(highestIndex), GetColorFromStatIndex(secondHighestIndex), 0.5f), 0.05f / (highest - growths[secondHighestIndex]));
    }

    public static Color GetColorFromStatIndex(int index)
    {
        switch (index)
        {
            case 0:
                return STR_ARCHETYPE_COLOR;

            case 1:
                return INT_ARCHETYPE_COLOR;

            case 2:
                return AGI_ARCHETYPE_COLOR;

            default:
                return NORMAL_COLOR;
        }
    }

    public static Color ReturnRarityColor(RarityType rarity)
    {
        switch (rarity)
        {
            case RarityType.Unique:
                return UNIQUE_COLOR;

            case RarityType.Epic:
                return EPIC_COLOR;

            case RarityType.Uncommon:
                return UNCOMMON_COLOR;

            case RarityType.Rare:
                return RARE_COLOR;

            case RarityType.Common:
                return NORMAL_COLOR;

            default:
                return Color.black;
        }
    }
}