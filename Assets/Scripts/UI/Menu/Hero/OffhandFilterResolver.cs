public static class OffhandFilterResolver
{
    public static bool doesMainHandHaveWeapon = false;
    public static TagType mainHandWeaponType = TagType.MeleeWeapon;
    public static bool isSpearWithSpecialBonus = false;
    public static bool isMainTwoHander = false;
    public static bool hasTwoHandToOneHand = false;
    public static bool hasOneHandToTwoHand = false;

    public static void SetHero(Hero hero)
    {
        doesMainHandHaveWeapon = false;
        isSpearWithSpecialBonus = false;
        isMainTwoHander = false;
        hasTwoHandToOneHand = false;
        hasOneHandToTwoHand = false;

        if (hero.EquipmentData.GetEquipmentInSlot(EquipSlotType.Weapon) is Weapon mainHand)
        {
            doesMainHandHaveWeapon = true;

            if (mainHand.DoesTagsContain(TagType.Spear) && hero.Stats.HasSpecialBonus(BonusStatType.CanUseSpearWithShield))
                isSpearWithSpecialBonus = true;
            if (hero.EquipmentData.GetEquipmentTagTypes(mainHand).Contains(TagType.TwoHandedWeapon))
                isMainTwoHander = true;

            hasTwoHandToOneHand = hero.Stats.HasSpecialBonus(BonusStatType.TwoHandedWeaponsAreOneHanded);
            hasOneHandToTwoHand = hero.Stats.HasSpecialBonus(BonusStatType.OneHandedWeaponsAreTwoHanded);

            if (mainHand.DoesTagsContain(TagType.MeleeWeapon))
                mainHandWeaponType = TagType.MeleeWeapon;
            else if (mainHand.DoesTagsContain(TagType.RangedWeapon))
                mainHandWeaponType = TagType.RangedWeapon;
        }
    }

    public static bool Filter(Equipment e)
    {
        if (e.IsEquipped)
            return false;

        if (e.Base.equipSlot == EquipSlotType.Offhand)
        {
            //Checks if spear and whether hero has spear and shield bonus
            if (e.DoesTagsContain(TagType.Shield) && isSpearWithSpecialBonus)
                return true;
            //Check if one handed, or two handed w/ bonus
            if (!isMainTwoHander || (isMainTwoHander && hasTwoHandToOneHand))
                return true;
        } else if (e.Base.equipSlot == EquipSlotType.Weapon)
        {
            //check if mainhand has weapon equipped
            if (!doesMainHandHaveWeapon)
                return true;
            //check if mainhand's weapon type is same (melee/ranged) as new item
            if (!e.DoesTagsContain(mainHandWeaponType))
                return false;
            //check if mainhand or new item is two handed without bonus
            if ((isMainTwoHander || e.DoesTagsContain(TagType.TwoHandedWeapon)) && !hasTwoHandToOneHand)
                return false;

            return true;

        }

        return false;
    }


}