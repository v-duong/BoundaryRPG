using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class AffixedItem : Item
{
    public List<Affix> prefixes;
    public List<Affix> suffixes;
    private bool currentPrefixesAreImmutable = false;
    private bool currentSuffixesAreImmutable = false;

    public abstract bool UpgradeRarity();

    public abstract bool UpdateItemStats();

    public abstract HashSet<TagType> GetTagTypes();

    public abstract void UpdateName();

    public virtual List<Affix> GetAllAffixes()
    {
        List<Affix> affixes = new List<Affix>();
        affixes.AddRange(prefixes);
        affixes.AddRange(suffixes);
        return affixes;
    }

    public bool DoesTagsContain(TagType t) => GetTagTypes().Contains(t);

    public bool RerollValues()
    {
        if (prefixes.Count == 0 && suffixes.Count == 0)
            return false;

        if (!currentPrefixesAreImmutable)
            foreach (Affix affix in prefixes.Where(x => !x.IsLocked))
                affix.RerollValue();

        if (!currentSuffixesAreImmutable)
            foreach (Affix affix in suffixes.Where(x => !x.IsLocked))
                affix.RerollValue();

        UpdateItemStats();

        return true;
    }

    public Affix GetRandomAffix()
    {
        if (Rarity == RarityType.Unique)
            return null;

        List<Affix> temp = prefixes.Concat(suffixes).Where(x => !x.IsLocked).ToList();
        if (temp.Count > 0)
        {
            return temp[Random.Range(0, temp.Count)];
        }
        else
            return null;
    }

    public Affix GetRandomAffix(AffixType type)
    {
        List<Affix> listToRemoveFrom;
        if (type == AffixType.Prefix && prefixes.Count > 0)
        {
            listToRemoveFrom = prefixes;
        }
        else if (type == AffixType.Suffix && suffixes.Count > 0)
        {
            listToRemoveFrom = suffixes;
        }
        else
        {
            return null;
        }

        var newList = listToRemoveFrom.Where(x => !x.IsLocked).ToList();

        if (newList.Count == 0)
            return null;

        return newList[Random.Range(0, newList.Count)];
    }

    public bool RemoveRandomAffix()
    {
        Affix affixToRemove;

        if (currentPrefixesAreImmutable && currentSuffixesAreImmutable || Rarity == RarityType.Unique)
            return false;
        else if (currentPrefixesAreImmutable)
            affixToRemove = GetRandomAffix(AffixType.Suffix);
        else if (currentSuffixesAreImmutable)
            affixToRemove = GetRandomAffix(AffixType.Prefix);
        else
            affixToRemove = GetRandomAffix();

        return RemoveAffix(affixToRemove);
    }

    public bool RemoveAffix(Affix affix)
    {
        if (affix == null)
            return false;
        if (affix.AffixType == AffixType.Prefix && prefixes.Contains(affix))
        {
            prefixes.Remove(affix);
        }
        else if (affix.AffixType == AffixType.Suffix && suffixes.Contains(affix))
        {
            suffixes.Remove(affix);
        }
        else
            return false;
        SortAffixes();
        UpdateItemStats();
        return true;
    }

    public bool ClearAffixes(bool setRarityToNormal = true)
    {
        if (prefixes.Count == 0 && suffixes.Count == 0 && Rarity == RarityType.Common || Rarity == RarityType.Unique)
            return false;

        if (!currentPrefixesAreImmutable)
            prefixes.RemoveAll(x => !x.IsLocked);
        if (!currentSuffixesAreImmutable)
            suffixes.RemoveAll(x => !x.IsLocked);

        if (setRarityToNormal && prefixes.Count == 0 && suffixes.Count == 0)
        {
            Rarity = RarityType.Common;
            UpdateName();
        }
        else if (setRarityToNormal && (prefixes.Count + suffixes.Count) <= 2)
        {
            Rarity = RarityType.Uncommon;
            UpdateName();
        }

        UpdateItemStats();

        return true;
    }

    public bool SetRarityToNormal()
    {
        return ClearAffixes(true);
    }

    public bool RerollAffixesAtRarity(Dictionary<TagType, float> weightModifiers = null, float affixLevelSkewFactor = 1f, HashSet<TagType> additionalTypes = null)
    {
        int affixCap = GetAffixCap();

        if (Rarity == RarityType.Common || Rarity == RarityType.Unique)
            return false;

        int affixCount;

        if (Rarity == RarityType.Uncommon)
        {
            ClearAffixes(false);
            affixCount = prefixes.Count + suffixes.Count;
            if (affixCount == 0)
                AddRandomAffix(weightModifiers, affixLevelSkewFactor, additionalTypes);
            if (affixCount == 1 && Random.Range(0, 2) == 0)
                AddRandomAffix(weightModifiers, affixLevelSkewFactor, additionalTypes);
            UpdateName();
            return true;
        }
        else if (Rarity == RarityType.Rare || Rarity == RarityType.Epic)
        {
            ClearAffixes(false);
            affixCount = prefixes.Count + suffixes.Count;

            // rolls the mimimum of 4 for rare and 5 for epics
            for (int j = affixCount; j < affixCap + 1; j++)
                AddRandomAffix(weightModifiers, affixLevelSkewFactor, additionalTypes);

            affixCount = prefixes.Count + suffixes.Count;

            // 50% roll to continue rolling affixes
            while ((Random.Range(0, 2) == 0) && affixCount < (affixCap * 2))
            {
                AddRandomAffix(weightModifiers, affixLevelSkewFactor, additionalTypes);
                affixCount = prefixes.Count + suffixes.Count;
            }
            UpdateName();
            return true;
        }
        return false;
    }

    public virtual bool AddRandomAffix(Dictionary<TagType, float> weightModifiers = null, float affixLevelSkewFactor = 1f, HashSet<TagType> additionalGroupTypes = null)
    {
        AffixType? affixType = GetRandomOpenAffixType();
        if (affixType == null)
            return false;

        HashSet<TagType> groupTypes = GetTagTypes();
        if (additionalGroupTypes != null)
            groupTypes.UnionWith(additionalGroupTypes);

        return AddAffix(ResourceManager.Instance.GetRandomAffixBase((AffixType)affixType, ItemLevel, groupTypes, GetBonusTagTypeList((AffixType)affixType), weightModifiers, affixLevelSkewFactor));
    }

    public List<string> GetBonusTagTypeList(AffixType type)
    {
        List<Affix> list;
        List<string> returnList = new List<string>();
        if (type == AffixType.Prefix)
            list = prefixes;
        else
            list = suffixes;
        foreach (Affix affix in list)
        {
            returnList.Add(affix.Base.AffixBonusStatTypeString);
        }
        return returnList;
    }

    public bool UpgradeNormalToUncommon()
    {
        if (Rarity != RarityType.Common)
            return false;
        Rarity = RarityType.Uncommon;
        AddRandomAffix();
        if (Random.Range(0, 2) == 0)
            AddRandomAffix();
        UpdateName();
        return true;
    }

    public virtual int GetAffixCap()
    {
        switch (Rarity)
        {
            case RarityType.Epic:
                return 4;

            case RarityType.Rare:
                return 3;

            case RarityType.Uncommon:
                return 1;

            case RarityType.Common:
            default:
                return 0;
        }
    }

    public AffixType? GetRandomOpenAffixType()
    {
        if (Rarity == RarityType.Unique)
            return null;

        int affixCap = GetAffixCap();

        if (prefixes.Count < affixCap && suffixes.Count < affixCap)
        {
            int i = Random.Range(0, 2);
            if (i == 0)
                return AffixType.Prefix;
            else
                return AffixType.Suffix;
        }
        else if (prefixes.Count >= affixCap && suffixes.Count < affixCap)
        {
            return AffixType.Suffix;
        }
        else if (suffixes.Count >= affixCap && prefixes.Count < affixCap)
        {
            return AffixType.Prefix;
        }
        else
            return null;
    }

    public bool AddAffix(AffixBase affix)
    {
        if (affix == null)
            return false;

        if (affix.affixType == AffixType.Prefix)
        {
            prefixes.Add(new Affix(affix, false));
        }
        else if (affix.affixType == AffixType.Suffix)
        {
            suffixes.Add(new Affix(affix, false));
        }
        else
            return false;
        SortAffixes();
        UpdateItemStats();
        return true;
    }

    private void SortAffixes()
    {
        if (Rarity == RarityType.Unique)
            return;
        prefixes = prefixes.OrderBy(x => x.Base.affixBonuses.Count > 0 ? (int)x.Base.affixBonuses[0].bonusStatType : int.MaxValue).ToList();
        suffixes = suffixes.OrderBy(x => x.Base.affixBonuses.Count > 0 ? (int)x.Base.affixBonuses[0].bonusStatType : int.MaxValue).ToList();
    }

    public WeightList<AffixBase> GetAllPossiblePrefixes(Dictionary<TagType, float> weightModifiers, float levelSkew)
    {
        return ResourceManager.Instance.GetPossibleAffixes(AffixType.Prefix, ItemLevel, GetTagTypes(), GetBonusTagTypeList(AffixType.Prefix), weightModifiers, levelSkew);
    }

    public WeightList<AffixBase> GetAllPossibleSuffixes(Dictionary<TagType, float> weightModifiers, float levelSkew)
    {
        return ResourceManager.Instance.GetPossibleAffixes(AffixType.Suffix, ItemLevel, GetTagTypes(), GetBonusTagTypeList(AffixType.Suffix), weightModifiers, levelSkew);
    }

    public void RemoveAllAffixLocks()
    {
        foreach (Affix affix in GetAllAffixes())
            affix.SetAffixLock(false);
    }

    public int GetLockCount()
    {
        int lockCount = 0;
        foreach (Affix affix in GetAllAffixes())
            if (affix.IsLocked)
                lockCount++;
        return lockCount;
    }

    public static int GetToNormalCost(AffixedItem currentItem)
    {
        return currentItem.ItemLevel * 3;
    }

    public static int GetRerollAffixCost(AffixedItem currentItem)
    {
        switch (currentItem.Rarity)
        {
            case RarityType.Uncommon:
                return currentItem.ItemLevel * 4;

            case RarityType.Rare:
                return currentItem.ItemLevel * 15;

            case RarityType.Epic:
                return currentItem.ItemLevel * 75;

            default:
                return 0;
        }
    }

    public static int GetAddAffixCost(AffixedItem currentItem)
    {
        switch (currentItem.Rarity)
        {
            case RarityType.Uncommon:
                return currentItem.ItemLevel * 3;

            case RarityType.Rare:
                return currentItem.ItemLevel * 25;

            case RarityType.Epic:
                return currentItem.ItemLevel * 150;

            default:
                return 0;
        }
    }

    public static int GetRemoveAffixCost(AffixedItem currentItem)
    {
        switch (currentItem.Rarity)
        {
            case RarityType.Uncommon:
                return currentItem.ItemLevel * 3;

            case RarityType.Rare:
                return currentItem.ItemLevel * 15;

            case RarityType.Epic:
                return currentItem.ItemLevel * 75;

            default:
                return 0;
        }
    }

    public static int GetRerollValuesCost(AffixedItem currentItem)
    {
        switch (currentItem.Rarity)
        {
            case RarityType.Uncommon:
                return currentItem.ItemLevel * 1;

            case RarityType.Rare:
                return currentItem.ItemLevel * 2;

            case RarityType.Epic:
                return currentItem.ItemLevel * 3;

            case RarityType.Unique:
                return currentItem.ItemLevel * 2;

            default:
                return 0;
        }
    }

    public static int GetUpgradeCost(AffixedItem currentItem)
    {
        switch (currentItem.Rarity)
        {
            case RarityType.Common:
                return currentItem.ItemLevel * 1;

            case RarityType.Uncommon:
                return currentItem.ItemLevel * 8;

            case RarityType.Rare:
                return currentItem.ItemLevel * 500;

            default:
                return 0;
        }
    }

    public static int GetLockCost(AffixedItem currentItem, int lockedAffixesCount = -1)
    {
        int currentCostMod = 1;

        if (lockedAffixesCount == -1)
        {
            lockedAffixesCount = 0;
            foreach (Affix affix in currentItem.GetAllAffixes())
            {
                if (affix.IsLocked)
                {
                    lockedAffixesCount++;
                }
            }
        }

        if (lockedAffixesCount == 1)
            currentCostMod = 2;
        if (lockedAffixesCount == 2)
            currentCostMod = 5;
        if (lockedAffixesCount >= 3)
            return 0;

        switch (currentItem.Rarity)
        {
            case RarityType.Uncommon:
                return currentItem.ItemLevel * 15 * currentCostMod;

            case RarityType.Rare:
                return currentItem.ItemLevel * 50 * currentCostMod;

            case RarityType.Epic:
                return currentItem.ItemLevel * 300 * currentCostMod;

            case RarityType.Unique:
                return currentItem.ItemLevel * 20 * currentCostMod;

            default:
                return 0;
        }
    }
}