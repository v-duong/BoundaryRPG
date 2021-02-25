using Newtonsoft.Json;
using System;
using UnityEngine;

public class HeroAttributes
{
    public HeroStats heroStats;
    private AttributesFloat baseAttributes = new AttributesFloat();
    private AttributesInt calculatedAttributes = new AttributesInt();

    public float BaseStrength { get => baseAttributes.strength; set => baseAttributes.strength = value; }
    public float BaseIntelligence { get => baseAttributes.intelligence; set => baseAttributes.intelligence = value; }
    public float BaseDexterity { get => baseAttributes.dexterity; set => baseAttributes.dexterity = value; }
    public float BaseVitality { get => baseAttributes.vitality; set => baseAttributes.vitality = value; }
    public float BaseWill { get => baseAttributes.will; set => baseAttributes.will = value; }

    public int Strength { get => calculatedAttributes.strength; set => calculatedAttributes.strength = value; }
    public int Intelligence { get => calculatedAttributes.intelligence; set => calculatedAttributes.intelligence = value; }
    public int Dexterity { get => calculatedAttributes.dexterity; set => calculatedAttributes.dexterity = value; }
    public int Vitality { get => calculatedAttributes.vitality; set => calculatedAttributes.vitality = value; }
    public int Will { get => calculatedAttributes.will; set => calculatedAttributes.will = value; }

    public HeroAttributes(HeroStats heroStats)
    {
        this.heroStats = heroStats;
    }

    public void UpdateAttributes()
    {
        Strength = (int)Math.Max(heroStats.GetStatBonus(BonusStatType.Strength).CalculateStat(BaseStrength), 1f);
        Intelligence = (int)Math.Max(heroStats.GetStatBonus(BonusStatType.Intelligence).CalculateStat(BaseIntelligence), 1f);
        Dexterity = (int)Math.Max(heroStats.GetStatBonus(BonusStatType.Dexterity).CalculateStat(BaseDexterity), 1f);
        Vitality = (int)Math.Max(heroStats.GetStatBonus(BonusStatType.Vitality).CalculateStat(BaseVitality), 1f);
        Will = (int)Math.Max(heroStats.GetStatBonus(BonusStatType.Will).CalculateStat(BaseWill), 1f);

        ApplyStrengthBonuses();
        ApplyIntelligenceBonuses();
        ApplyDexterityBonuses();
        ApplyVitalityBonuses();
        ApplyWillBonuses();
    }

    private void ApplyStrengthBonuses()
    {
        /*
         * +1% Armor per 10 Str
         * +1% Attack Damage per 10 Str
         */
        int armorMod = (int)Math.Round(Strength / 10f, MidpointRounding.AwayFromZero);
        int attackDamageMod = (int)Math.Round(Strength / 10f, MidpointRounding.AwayFromZero);

        heroStats.SetAttributesBonus(BonusStatType.Global_Armor, ModifyType.Additive, armorMod);
        heroStats.SetAttributesBonus(BonusStatType.AttackDamage, ModifyType.Additive, attackDamageMod);
    }

    private void ApplyIntelligenceBonuses()
    {
        /*
         * +1% Magic Armor per 10 Int
         * +1% Magic Damage per 10 Int
         */
        int magicArmorMod = (int)Math.Round(Intelligence / 10f, MidpointRounding.AwayFromZero);
        int magicDamageMod = (int)Math.Round(Intelligence / 10f, MidpointRounding.AwayFromZero);

        heroStats.SetAttributesBonus(BonusStatType.Global_MagicArmor, ModifyType.Additive, magicArmorMod);
        heroStats.SetAttributesBonus(BonusStatType.MagicDamage, ModifyType.Additive, magicDamageMod);
    }

    private void ApplyDexterityBonuses()
    {
        /*
         * +1 Accuracy per 4 Dex
         * +1% Dodge Rating per 10 Dex
         */
        int accuracyMod = (int)Math.Round(Dexterity / 4f, MidpointRounding.AwayFromZero);
        int dodgeMod = (int)Math.Round(Dexterity / 10f, MidpointRounding.AwayFromZero);

        heroStats.SetAttributesBonus(BonusStatType.Accuracy, ModifyType.FlatAddition, accuracyMod);
        heroStats.SetAttributesBonus(BonusStatType.Global_DodgeRating, ModifyType.Additive, dodgeMod);
    }

    private void ApplyVitalityBonuses()
    {
        /*
         * +1 Health Regen per 5 Vit
         * +1 Afflicted Damage Resist per 15 Vit
         */
        int flatHealthRegen = (int)(Math.Round(Vitality / 5f, MidpointRounding.AwayFromZero));
        int afflictionMod = (int)Math.Round(Vitality / 10f, MidpointRounding.AwayFromZero);

        heroStats.SetAttributesBonus(BonusStatType.HealthRegen, ModifyType.FlatAddition, flatHealthRegen);
        heroStats.SetAttributesBonus(BonusStatType.AfflictedStatusDamageResistance, ModifyType.FlatAddition, afflictionMod);
    }

    private void ApplyWillBonuses()
    {
        /*
         * +1 Mana On Basic Attack per 5 Will
         * +1 Affliction Avoidance per 15 Will
         */
        int flatManaRegen = (int)Math.Round(Will / 5f, MidpointRounding.AwayFromZero);
        int afflictionMod = (int)Math.Round(Will / 15f, MidpointRounding.AwayFromZero);

        heroStats.SetAttributesBonus(BonusStatType.ManaGainOnAttack, ModifyType.FlatAddition, flatManaRegen);
        heroStats.SetAttributesBonus(BonusStatType.AfflictedStatusAvoidance, ModifyType.FlatAddition, afflictionMod);
    }
}

public class AttributesFloat
{
    [JsonProperty]
    public float strength;

    [JsonProperty]
    public float intelligence;

    [JsonProperty]
    public float dexterity;

    [JsonProperty]
    public float vitality;

    [JsonProperty]
    public float will;
}

public class AttributesInt
{
    [JsonProperty] public int strength;
    [JsonProperty] public int intelligence;
    [JsonProperty] public int dexterity;
    [JsonProperty] public int vitality;
    [JsonProperty] public int will;
}