using System;
using System.Collections.Generic;

public class HeroStats : ActorStats
{
    public HeroAttributes Attributes;
    private Dictionary<BonusStatType, StatBonus> attributeStatBonuses;
    private Dictionary<BonusStatType, StatBonusCollection> archetypeStatBonuses;

    public HeroStats(Hero hero) : base(hero)
    {
        attributeStatBonuses = new Dictionary<BonusStatType, StatBonus>();
        archetypeStatBonuses = new Dictionary<BonusStatType, StatBonusCollection>();
        Attributes = new HeroAttributes(this);
        BaseHealth = 100;
        BaseMana = 50;
        Attributes.BaseStrength = 10;
        Attributes.BaseDexterity = 10;
        Attributes.BaseIntelligence = 10;
        Attributes.BaseVitality = 5;
        Attributes.BaseWill = 5;
        BaseSpeed = 10;
        BaseArmor = 0;
        BaseMagicArmor = 0;
        BaseDodgeRating = 0;
        BaseAttackPhasing = 0;
        BaseSpellPhasing = 0;
    }

    public void LoadBaseStats(SaveData.HeroSaveData saveData)
    {
        BaseHealth = saveData.baseHealth;
        BaseMana = saveData.baseMana;
        Attributes.BaseStrength = saveData.baseStrength;
        Attributes.BaseIntelligence = saveData.baseIntelligence;
        Attributes.BaseDexterity = saveData.baseDexterity;
        Attributes.BaseVitality = saveData.baseVitality;
        Attributes.BaseWill = saveData.baseWill;
        BaseSpeed = saveData.baseSpeed;
    }

    public void UpdateDefenses(List<Equipment> equipList)
    {
        int ArmorFromEquip = 0;
        int MagicArmorFromEquip = 0;
        int DodgeFromEquip = 0;
        float baseBlock = 0;
        float baseProtection = 0;
        bool hasShield = false;

        foreach (Equipment equipment in equipList)
        {
            if (equipment == null)
                continue;
            if (equipment.GetItemType() == ItemType.Armor)
            {
                Armor armor = equipment as Armor;
                ArmorFromEquip += armor.armor;
                MagicArmorFromEquip += armor.magicArmor;
                DodgeFromEquip += armor.dodgeRating;

                if (equipment.GetTagTypes().Contains(TagType.Shield))
                {
                    hasShield = true;
                    baseBlock = armor.blockChance;
                    baseProtection = armor.blockProtection;
                }
            }
        }

        Armor = Math.Max(GetStatBonus(BonusStatType.Global_Armor).CalculateStat(BaseArmor + ArmorFromEquip), 0);
        MagicArmor = Math.Max(GetStatBonus(BonusStatType.Global_MagicArmor).CalculateStat(BaseMagicArmor + MagicArmorFromEquip), 0);
        DodgeRating = Math.Max(GetStatBonus(BonusStatType.Global_DodgeRating).CalculateStat(BaseDodgeRating + DodgeFromEquip), 0);

        if (hasShield)
        {
            float BlockChanceCap = Math.Min(GetStatBonus(BonusStatType.MaxShieldBlockChance).CalculateStat(Helpers.BASE_BLOCK_CAP), 100f);
            float BlockProtectionCap = Math.Min(GetStatBonus(BonusStatType.MaxShieldBlockProtection).CalculateStat(Helpers.BASE_BLOCK_PROTECTION_CAP), 100f);

            BlockChance = Math.Min(GetStatBonus(BonusStatType.ShieldBlockChance).CalculateStat(baseBlock), BlockChanceCap) / 100f;
            BlockProtection = Math.Min(GetStatBonus(BonusStatType.ShieldBlockProtection).CalculateStat(baseProtection), BlockProtectionCap) / 100f;
        }
        else
        {
            BlockChance = 0;
            BlockProtection = 0;
        }

        HashSet<TagType> actorTags = Actor.GetTagTypes();
        if (actorTags.Contains(TagType.DualWield) || actorTags.Contains(TagType.TwoHandedWeapon) && !actorTags.Contains(TagType.RangedWeapon))
        {
            float attackParryCap = Math.Min(GetStatBonus(BonusStatType.AttackMaxParryChance).CalculateStat(Helpers.BASE_ATTACK_PARRY_CAP), 100f);
            float magicParryCap = Math.Min(GetStatBonus(BonusStatType.MagicParryChance).CalculateStat(Helpers.BASE_MAGIC_PARRY_CAP), 100f);

            AttackParryChance = Math.Min(GetStatBonus(BonusStatType.AttackParryChance).CalculateStat(0f), attackParryCap);
            MagicParryChance = Math.Min(GetStatBonus(BonusStatType.MagicParryChance).CalculateStat(0f), magicParryCap);
        }
        else
        {
            AttackParryChance = 0;
            MagicParryChance = 0;
        }
    }

    public void ApplyArchetypeLevelBonuses(HeroArchetypeData archetype, bool isSecondary)
    {
        if (!isSecondary)
        {
            BaseHealth += archetype.Base.healthGrowth;
            BaseMana += archetype.Base.manaGrowth;
            Attributes.BaseStrength += archetype.Base.strengthGrowth;
            Attributes.BaseIntelligence += archetype.Base.intelligenceGrowth;
            Attributes.BaseDexterity += archetype.Base.dexterityGrowth;
            Attributes.BaseVitality += archetype.Base.vitalityGrowth;
            Attributes.BaseWill += archetype.Base.willGrowth;
            BaseSpeed += archetype.Base.speedGrowth;
        }
        else
        {
            BaseHealth += archetype.Base.healthGrowth / 4;
            BaseMana += archetype.Base.manaGrowth / 4;
            Attributes.BaseStrength += archetype.Base.strengthGrowth / 2;
            Attributes.BaseIntelligence += archetype.Base.intelligenceGrowth / 2;
            Attributes.BaseDexterity += archetype.Base.dexterityGrowth / 2;
            Attributes.BaseVitality += archetype.Base.vitalityGrowth / 2;
            Attributes.BaseWill += archetype.Base.willGrowth / 2;
            BaseSpeed += archetype.Base.speedGrowth / 2;
        }
    }

    public void ApplyEquipmentBonuses(Equipment equip)
    {
        List<Affix> affixes = equip.GetAllAffixes();
        foreach (Affix affix in affixes)
        {
            for (int i = 0; i < affix.Base.affixBonuses.Count; i++)
            {
                AffixBonusProperty b = affix.Base.affixBonuses[i];
                //ignore local mods
                if (b.bonusStatType >= (BonusStatType)Helpers.LOCAL_BONUS_START)
                    continue;
                else
                {
                    if (affix.GetAffixValue(i) != 0 || b.modifyType == ModifyType.FixedToValue)
                        AddStatBonus(b.bonusStatType, b.restriction, b.modifyType, affix.GetAffixValue(i));
                }
            }
            for (int i = 0; i < affix.Base.triggeredEffects.Count; i++)
            {
                AffixTriggeredEffectBonusProperty triggeredEffect = affix.Base.triggeredEffects[i];
                TriggeredEffect t = new TriggeredEffect(triggeredEffect, affix.GetEffectValue(i), equip.Id);
                AddTriggeredEffect(triggeredEffect, t);
            }
        }
    }

    public void RemoveEquipmentBonuses(Equipment equip)
    {
        List<Affix> affixes = equip.GetAllAffixes();
        foreach (Affix affix in affixes)
        {
            for (int i = 0; i < affix.Base.affixBonuses.Count; i++)
            {
                AffixBonusProperty b = affix.Base.affixBonuses[i];
                //ignore local mods
                if (b.bonusStatType < (BonusStatType)0x700)
                {
                    if (affix.GetAffixValue(i) != 0 || b.modifyType == ModifyType.FixedToValue)
                        RemoveStatBonus(b.bonusStatType, b.restriction, b.modifyType, affix.GetAffixValue(i));
                }
            }
            for (int i = 0; i < affix.Base.triggeredEffects.Count; i++)
            {
                AffixTriggeredEffectBonusProperty triggeredEffect = affix.Base.triggeredEffects[i];
                RemoveTriggeredEffect(triggeredEffect, equip.Id);
            }
        }
    }

    public void SetAttributesBonus(BonusStatType statType, ModifyType modifyType, int value)
    {
        if (!attributeStatBonuses.ContainsKey(statType))
            attributeStatBonuses.Add(statType, new StatBonus());
        switch (modifyType)
        {
            case ModifyType.Additive:
                attributeStatBonuses[statType].SetAdditive(value);
                break;

            case ModifyType.FlatAddition:
                attributeStatBonuses[statType].SetFlat(value);
                break;

            default:
                break;
        }
    }

    public void AddArchetypeStatBonus(BonusStatType type, TagType restriction, ModifyType modifier, float value)
    {
        if (type >= (BonusStatType)Helpers.SPECIAL_BONUS_START)
        {
            AddSpecialBonus(type);
            return;
        }

        if (!archetypeStatBonuses.ContainsKey(type))
            archetypeStatBonuses[type] = new StatBonusCollection();
        archetypeStatBonuses[type].AddBonus(restriction, modifier, value);
    }

    public void RemoveArchetypeStatBonus(BonusStatType type, TagType restriction, ModifyType modifier, float value)
    {
        if (type >= (BonusStatType)Helpers.SPECIAL_BONUS_START)
        {
            RemoveSpecialBonus(type);
            return;
        }

        if (!archetypeStatBonuses.ContainsKey(type))
            return;
        archetypeStatBonuses[type].RemoveBonus(restriction, modifier, value);
    }

    public override int GetResistance(ElementType element)
    {
        return ElementStats[element];
    }

    public int GetUncapResistance(ElementType element)
    {
        return ElementStats.GetUncapResistance(element);
    }

    public override StatBonus GetTotalStatBonus(BonusStatType type, IEnumerable<TagType> tags, Dictionary<BonusStatType, StatBonusCollection> additionalBonusProperties, StatBonus existingBonus = null)
    {
        StatBonus resultBonus;
        if (existingBonus == null)
            resultBonus = new StatBonus();
        else
            resultBonus = new StatBonus(existingBonus);

        if (statBonuses.TryGetValue(type, out StatBonusCollection statBonus))
            resultBonus.AddBonuses(statBonus.GetTotalStatBonus(tags));
        if (attributeStatBonuses.TryGetValue(type, out StatBonus attributeBonus))
            resultBonus.AddBonuses(attributeBonus);
        if (archetypeStatBonuses.TryGetValue(type, out StatBonusCollection archetypeBonuses))
            resultBonus.AddBonuses(archetypeBonuses.GetTotalStatBonus(tags));
        if (additionalBonusProperties != null && additionalBonusProperties.TryGetValue(type, out StatBonusCollection additionalBonus))
            resultBonus.AddBonuses(additionalBonus.GetTotalStatBonus(tags));
        if (temporaryBonuses.TryGetValue(type, out StatBonus temporaryBonus))
            resultBonus.AddBonuses(temporaryBonus);

        return resultBonus;
    }
}