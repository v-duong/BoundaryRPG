using System;
using System.Collections.Generic;

public class Weapon : Equipment
{
    private Dictionary<ElementType, int> weaponDamage;
    public float CriticalChance { get; private set; }
    public float AttackSpeed { get; private set; }
    public float DamageSpread { get; private set; }

    public int PhysicalDamage
    {
        get
        {
            return weaponDamage[0];
        }
    }

    public Weapon(EquipmentBase e, int ilvl) : base(e, ilvl)
    {
        weaponDamage = new Dictionary<ElementType, int>();
        foreach (ElementType element in Enum.GetValues(typeof(ElementType)))
        {
            weaponDamage[element] = new int();
        }
        weaponDamage[ElementType.Physical] = e.damage;
        CriticalChance = e.criticalChance;
        AttackSpeed = e.speedModifier;
        DamageSpread = e.damageSpread;
    }

    public override ItemType GetItemType()
    {
        return ItemType.Weapon;
    }

    public override bool UpdateItemStats()
    {
        base.UpdateItemStats();
        Dictionary<BonusStatType, StatBonus> localBonusTotals = new Dictionary<BonusStatType, StatBonus>();
        GetLocalModValues(localBonusTotals, GetAllAffixes(), ItemType.Weapon);

        weaponDamage[ElementType.Physical] = CalculateStat(Base.damage, localBonusTotals, BonusStatType.LocalPhysicalATK);
        weaponDamage[ElementType.Physical] = CalculateStat(weaponDamage[ElementType.Physical], localBonusTotals, BonusStatType.LocalPhysicalATK);

        weaponDamage[ElementType.Fire] = CalculateStat(Base.damage, localBonusTotals, BonusStatType.LocalFireATK, BonusStatType.LocalElementalATK);

        weaponDamage[ElementType.Cold] = CalculateStat(Base.damage, localBonusTotals, BonusStatType.LocalColdATK, BonusStatType.LocalElementalATK);

        weaponDamage[ElementType.Lightning] = CalculateStat(Base.damage, localBonusTotals, BonusStatType.LocalLightningATK, BonusStatType.LocalElementalATK);

        weaponDamage[ElementType.Earth] = CalculateStat(Base.damage, localBonusTotals, BonusStatType.LocalEarthATK, BonusStatType.LocalElementalATK);

        weaponDamage[ElementType.Divine] = CalculateStat(Base.damage, localBonusTotals, BonusStatType.LocalDivineATK, BonusStatType.LocalAstralATK);

        weaponDamage[ElementType.Void] = CalculateStat(Base.damage, localBonusTotals, BonusStatType.LocalVoidATK, BonusStatType.LocalAstralATK);

        CriticalChance = CalculateStat(Base.criticalChance, localBonusTotals, BonusStatType.LocalCriticalChance);
        AttackSpeed = CalculateStat(Base.speedModifier, localBonusTotals, BonusStatType.LocalAttackSpeed);
        DamageSpread = CalculateStat(Base.damageSpread, localBonusTotals, BonusStatType.LocalDamageSpread);

        return true;
    }

    private void ResetDamageValues()
    {
    }

    public int GetWeaponDamage(ElementType e)
    {
        if (weaponDamage.TryGetValue(e, out int damageValue))
            return damageValue;
        else
            return 0;
    }

    public override HashSet<TagType> GetTagTypes()
    {
        HashSet<TagType> tags = new HashSet<TagType>
        {
            TagType.Weapon,
            Base.groupTag
        };

        switch (Base.groupTag)
        {
            case TagType.OneHandedAxe:
            case TagType.OneHandedSword:
            case TagType.OneHandedMace:
                tags.Add(TagType.AttackWeapon);
                tags.Add(TagType.MeleeWeapon);
                tags.Add(TagType.OneHandedWeapon);
                break;

            case TagType.TwoHandedAxe:
            case TagType.TwoHandedSword:
            case TagType.TwoHandedMace:
            case TagType.Spear:
                tags.Add(TagType.AttackWeapon);
                tags.Add(TagType.MeleeWeapon);
                tags.Add(TagType.TwoHandedWeapon);
                break;

            case TagType.Bow:
            case TagType.TwoHandedGun:
                tags.Add(TagType.RangedWeapon);
                tags.Add(TagType.AttackWeapon);
                tags.Add(TagType.TwoHandedWeapon);
                break;

            case TagType.OneHandedWeapon:
                tags.Add(TagType.RangedWeapon);
                tags.Add(TagType.AttackWeapon);
                tags.Add(TagType.OneHandedWeapon);
                break;

            case TagType.Wand:
                tags.Add(TagType.CasterWeapon);
                tags.Add(TagType.RangedWeapon);
                tags.Add(TagType.OneHandedWeapon);
                break;

            case TagType.Staff:
                tags.Add(TagType.CasterWeapon);
                tags.Add(TagType.AttackWeapon);
                tags.Add(TagType.MeleeWeapon);
                tags.Add(TagType.TwoHandedWeapon);
                break;
        }
        switch (Base.groupTag)
        {
            case TagType.OneHandedAxe:
            case TagType.TwoHandedAxe:
                tags.Add(TagType.AxeType);
                break;

            case TagType.OneHandedSword:
            case TagType.TwoHandedSword:
                tags.Add(TagType.SwordType);
                break;

            case TagType.OneHandedMace:
            case TagType.TwoHandedMace:
                tags.Add(TagType.MaceType);
                break;

            case TagType.OneHandedGun:
            case TagType.TwoHandedGun:
                tags.Add(TagType.GunType);
                break;

            case TagType.Bow:
                tags.Add(TagType.BowType);
                break;
        }

        return tags;
    }

    public float GetPhysicalDPS()
    {
        float dps = PhysicalDamage * AttackSpeed;
        return dps;
    }

    public float GetElementalDPS()
    {
        int dmg = weaponDamage[ElementType.Fire]+ weaponDamage[ElementType.Cold] + weaponDamage[ElementType.Lightning] + weaponDamage[ElementType.Earth];
        float dps = dmg * AttackSpeed;
        return dps;
    }

    public float GetPrimordialDPS()
    {
        int dmg = weaponDamage[ElementType.Divine] + weaponDamage[ElementType.Void];
        float dps = dmg * AttackSpeed;
        return dps;
    }

    public TagType GetWeaponRange()
    {
        if (GetTagTypes().Contains(TagType.MeleeWeapon))
        {
            return TagType.MeleeWeapon;
        } else
        {
            return TagType.RangedWeapon;
        }
    }
}