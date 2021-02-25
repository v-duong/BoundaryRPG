using System;
using System.Collections.Generic;

public abstract class Equipment : AffixedItem
{
    
    public EquipmentBase Base;
    public float costModifier;
    public int levelRequirement;
    public int strRequirement;
    public int intRequirement;
    public int dexRequirement;
    public Hero equippedToHero;
    public List<Affix> innate;

    public bool IsEquipped
    {
        get
        {
            if (equippedToHero == null)
                return false;
            else
                return true;
        }
    }

    protected Equipment(EquipmentBase e, int ilvl)
    {
        Id = Guid.NewGuid();
        Base = e;
        Name = e.LocalizedName;
        costModifier = e.sellValue;
        strRequirement = e.strengthReq;
        intRequirement = e.intelligenceReq;
        dexRequirement = e.dexterityReq;
        levelRequirement = Math.Min(e.dropLevel, Helpers.MAX_LEVEL_REQ);
        Rarity = RarityType.Common;
        ItemLevel = ilvl;
        prefixes = new List<Affix>();
        suffixes = new List<Affix>();
        innate = new List<Affix>();
        equippedToHero = null;

        if (e.hasInnate)
        {
            Affix newInnate = new Affix(ResourceManager.Instance.GetAffixBase(e.innateAffixId, AffixType.Innate));
            innate.Add(newInnate);
        }
    }

    public static Equipment CreateEquipmentFromBase(EquipmentBase equipmentBase, int ilvl)
    {
        if (equipmentBase == null)
            return null;

        Equipment e;
        if (equipmentBase.equipSlot == EquipSlotType.Weapon)
        {
            e = new Weapon(equipmentBase, ilvl);
        }
        else if ((int)equipmentBase.equipSlot >= 6)
        {
            e = new Accessory(equipmentBase, ilvl);
        }
        else
        {
            e = new Armor(equipmentBase, ilvl);
        }
        return e;
    }

    public static Equipment CreateEquipmentFromBase(string equipmentString, int ilvl, RarityType rarity = RarityType.Common)
    {
        EquipmentBase equipmentBase = ResourceManager.Instance.GetEquipmentBase(equipmentString);
        if (equipmentBase == null)
            return null;

        Equipment equip = CreateEquipmentFromBase(equipmentBase, ilvl);

        if (rarity > RarityType.Common)
        {
            equip.Rarity = rarity;
            equip.RerollAffixesAtRarity();
        }

        return equip;
    }

    public static Equipment CreateRandomEquipment(int ilvl, TagType? group = null)
    {
        Equipment equip = CreateEquipmentFromBase(ResourceManager.Instance.GetRandomEquipmentBase(ilvl, group), ilvl);

        return equip;
    }

    public static Equipment CreateRandomEquipment_EvenSlotWeight(int ilvl, TagType? group = null, float baseLevelSkew = 1f)
    {
        EquipSlotType slot = (EquipSlotType)UnityEngine.Random.Range(0, 9);
        if (slot == EquipSlotType.RingSlot1)
            slot = EquipSlotType.Ring;

        Equipment equip = CreateEquipmentFromBase(ResourceManager.Instance.GetRandomEquipmentBase(ilvl, group, slot, baseLevelSkew), ilvl);
        return equip;
    }

    public static Equipment CreateRandomUnique(int ilvl, TagType? group = null)
    {
        UniqueBase uniqueBase = ResourceManager.Instance.GetRandomUniqueBase(ilvl, group);
        if (uniqueBase == null)
            return null;
        return CreateUniqueFromBase(uniqueBase, ilvl);
    }

    public static Equipment CreateUniqueFromBase(UniqueBase uniqueBase, int ilvl)
    {
        Equipment e = CreateEquipmentFromBase(uniqueBase, ilvl);

        if (e == null)
            return null;

        e.Rarity = RarityType.Unique;
        foreach (AffixBase affixBase in uniqueBase.fixedUniqueAffixes)
        {
            e.prefixes.Add(new Affix(affixBase));
        }
        e.UpdateItemStats();
        return e;
    }

    public static Equipment CreateUniqueFromBase(string uniqueId, int ilvl, int uniqueVersion)
    {
        UniqueBase uniqueBase = ResourceManager.Instance.GetUniqueBase(uniqueId);
        if (uniqueBase == null)
            return null;
        if (uniqueVersion == uniqueBase.uniqueVersion)
        {
            Equipment e = CreateEquipmentFromBase(uniqueBase, ilvl);
            e.Rarity = RarityType.Unique;
            return e;
        }
        else
        {
            return CreateUniqueFromBase(uniqueBase, ilvl);
        }
    }

    public override bool UpgradeRarity()
    {
        if (Rarity < RarityType.Epic)
            Rarity++;
        else
            return false;
        UpdateName();
        return true;
    }

    public override void UpdateName()
    {
        if (Rarity == RarityType.Rare || Rarity == RarityType.Epic)
        {
            Name = LocalizationManager.Instance.GenerateRandomItemName(GetTagTypes());
        }
        else if (Rarity == RarityType.Uncommon)
        {
        }
        else
        {
            Name = Base.LocalizedName;
        }
    }

    public override bool UpdateItemStats()
    {
        int req = Base.dropLevel;

        List<Affix> affixes = new List<Affix>();
        affixes.AddRange(prefixes);
        affixes.AddRange(suffixes);

        foreach (Affix affix in affixes)
        {
            if (affix.Base.spawnLevel > req)
                req = affix.Base.spawnLevel;
        }

        levelRequirement = Math.Min(req, Helpers.MAX_LEVEL_REQ);

        return true;
    }

    public override List<Affix> GetAllAffixes()
    {
        List<Affix> affixes = new List<Affix>();
        affixes.AddRange(prefixes);
        affixes.AddRange(suffixes);
        affixes.AddRange(innate);
        return affixes;
    }

    public override int GetItemValue()
    {
        float affixValueMultiplier = 1;
        float rollValue;
        int actualAffixCount;
        if (Rarity != RarityType.Unique)
        {
            foreach (Affix affix in GetAllAffixes())
            {
                if (affix.AffixType == AffixType.Innate)
                    continue;
                int weightValue = 0;
                foreach (var weight in affix.Base.spawnWeight)
                {
                    if (GetTagTypes().Contains(weight.type))
                    {
                        weightValue = weight.weight;
                        break;
                    }
                }

                float weightModifier;
                if (weightValue > 0)
                {
                    weightModifier = 1000f / weightValue;
                }
                else
                {
                    weightModifier = 5f;
                }

                affixValueMultiplier += weightModifier;

                if (affix.Base.affixBonuses.Count == 0)
                    continue;
                rollValue = 0;

                for (int i = 0; i < affix.Base.affixBonuses.Count; i++)
                {
                    //UnityEngine.Debug.Log(affix.Base.idName + " " + +rollValueMultiplier + " " + affix.GetAffixValue(i) + " " + affix.Base.affixBonuses[i].minValue);
                    float valueDifference = affix.Base.affixBonuses[i].maxValue - affix.Base.affixBonuses[i].minValue;
                    if (valueDifference == 0)
                    {
                        rollValue += 0.7f;
                        continue;
                    }
                    rollValue += (affix.GetAffixValue(i) - affix.Base.affixBonuses[i].minValue) / valueDifference * 0.7f;
                }
                rollValue /= affix.Base.affixBonuses.Count;
                affixValueMultiplier += rollValue;
            }
        }
        else
        {
            UniqueBase uniqueBase = Base as UniqueBase;
            float rollValueMultiplier = 0f;
            actualAffixCount = 0;
            foreach (Affix affix in GetAllAffixes())
            {
                if (affix.Base.affixBonuses.Count == 0)
                    continue;
                rollValue = 0;
                for (int i = 0; i < affix.Base.affixBonuses.Count; i++)
                {
                    //UnityEngine.Debug.Log(affix.Base.idName + " " + +rollValueMultiplier + " " + affix.GetAffixValue(i) + " " + affix.Base.affixBonuses[i].minValue);
                    float valueDifference = affix.Base.affixBonuses[i].maxValue - affix.Base.affixBonuses[i].minValue;
                    if (valueDifference == 0)
                    {
                        rollValue += 0.5f;
                        continue;
                    }
                    rollValue += (affix.GetAffixValue(i) - affix.Base.affixBonuses[i].minValue) / valueDifference;
                }
                rollValueMultiplier += rollValue / affix.Base.affixBonuses.Count;
                actualAffixCount++;
            }
            rollValueMultiplier = 1 + rollValueMultiplier / actualAffixCount;

            if (uniqueBase.spawnWeight == 0)
                affixValueMultiplier = 500f * rollValueMultiplier;
            else
                affixValueMultiplier = Math.Min((1000f / uniqueBase.spawnWeight) * (uniqueBase.dropLevel / 100f) * 50f * rollValueMultiplier, 500f);
        }

        return Math.Max((int)(affixValueMultiplier * ItemLevel * 1.5f), 1);
    }

    protected static void GetLocalModValues(Dictionary<BonusStatType, StatBonus> dic, List<Affix> affixes, ItemType itemType)
    {
        int startValue;
        switch (itemType)
        {
            case ItemType.Armor:
                startValue = Helpers.LOCAL_ARMOR_BONUS_START;
                break;

            case ItemType.Weapon:
                startValue = Helpers.LOCAL_WEAPON_BONUS_START;
                break;

            default:
                return;
        }

        foreach (Affix affix in affixes)
        {
            for (int i = 0; i < affix.Base.affixBonuses.Count; i++)
            {
                AffixBonusProperty prop = affix.Base.affixBonuses[i];
                if ((int)prop.bonusStatType >= startValue && (int)prop.bonusStatType < startValue + 0x100)
                {
                    if (!dic.ContainsKey(prop.bonusStatType))
                        dic.Add(prop.bonusStatType, new StatBonus());
                    dic[prop.bonusStatType].AddBonus(prop.modifyType, affix.GetAffixValue(i));
                }
            }
        }
    }

    protected static int CalculateStat(int stat, Dictionary<BonusStatType, StatBonus> dic, params BonusStatType[] BonusStatTypes)
    {
        return (int)CalculateStat((float)stat, dic, BonusStatTypes);
    }

    protected static float CalculateStat(float stat, Dictionary<BonusStatType, StatBonus> dic, params BonusStatType[] BonusStatTypes)
    {
        StatBonus totalBonus = new StatBonus();
        foreach (BonusStatType BonusStatType in BonusStatTypes)
        {
            if (dic.TryGetValue(BonusStatType, out StatBonus bonus))
            {
                totalBonus.AddBonuses(bonus);
            }
        }
        return totalBonus.CalculateStat(stat);
    }
}