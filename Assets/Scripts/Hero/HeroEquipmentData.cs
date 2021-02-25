using System.Collections.Generic;

public class HeroEquipmentData
{
    private Hero hero;
    private List<EquipData> equipList;

    public HeroEquipmentData(Hero hero)
    {
        this.hero = hero;
        equipList = new List<EquipData>();
        for (int i = 0; i < 12; i++)
        {
            equipList.Add(new EquipData());
        }
    }

    public Equipment GetEquipmentInSlot(EquipSlotType slot)
    {
        if (slot == EquipSlotType.Ring)
            return null;
        return equipList[(int)slot].equip;
    }

    public bool EquipToSlot(Equipment equip, EquipSlotType slot)
    {
        if (equip.IsEquipped)
            return false;

        if (equip.Base.equipSlot == EquipSlotType.Ring && slot != EquipSlotType.RingSlot1 && slot != EquipSlotType.RingSlot2)
        {
            return false;
        }
        else if (equip.Base.equipSlot == EquipSlotType.Weapon)
        {
            if (slot == EquipSlotType.Offhand)
            {
                Equipment mainWeapon = equipList[(int)EquipSlotType.Weapon].equip;

                if (mainWeapon == null)
                    slot = EquipSlotType.Weapon;
                else if ((mainWeapon.DoesTagsContain(TagType.MeleeWeapon) && !equip.DoesTagsContain(TagType.MeleeWeapon))
                         || (mainWeapon.DoesTagsContain(TagType.RangedWeapon) && !equip.DoesTagsContain(TagType.RangedWeapon)))
                    return false;
            }
            /*
            else if (GetEquipmentTagTypes(equip).Contains(TagType.SPEAR) && HasSpecialBonus(BonusType.CAN_USE_SPEARS_WITH_SHIELDS) && slot == EquipSlotType.WEAPON)
            {
                if (equipList[(int)EquipSlotType.OFF_HAND].equip == null || !equipList[(int)EquipSlotType.OFF_HAND].equip.GetTagTypes().Contains(TagType.SHIELD))
                    UnequipFromSlot(EquipSlotType.OFF_HAND);
            }
            else if (GetEquipmentTagTypes(equip).Contains(TagType.TWO_HANDED_WEAPON) && !HasSpecialBonus(BonusType.TWO_HANDED_WEAPONS_ARE_ONE_HANDED))
            {
                if (slot == EquipSlotType.WEAPON && equipList[(int)EquipSlotType.OFF_HAND].equip != null)
                    UnequipFromSlot(EquipSlotType.OFF_HAND);
                else if (slot == EquipSlotType.OFF_HAND)
                    slot = EquipSlotType.WEAPON;
            }
            */
        }
        else if (equip.Base.equipSlot != slot)
            return false;

        int slotNum = (int)slot;
        if (equipList[slotNum].equip != null)
            UnequipFromSlot(slot);

        switch (slot)
        {
            case EquipSlotType.Headgear:
            case EquipSlotType.BodyArmor:
            case EquipSlotType.Boots:
            case EquipSlotType.Gloves:
            case EquipSlotType.Belt:
            case EquipSlotType.Necklace:
            case EquipSlotType.Weapon:
            case EquipSlotType.Offhand:
            case EquipSlotType.RingSlot1:
            case EquipSlotType.RingSlot2:
                equipList[slotNum].equip = equip;
                break;

            default:
                return false;
        }
        equip.equippedToHero = hero;

        hero.Stats.ApplyEquipmentBonuses(equip);

        hero.actorTagsDirty = true;
        hero.UpdateActor();

        return true;
    }

    public bool UnequipFromSlot(EquipSlotType slot)
    {
        int slotNum = (int)slot;

        if (equipList[slotNum].equip == null)
            return false;
        Equipment equip = equipList[slotNum].equip;
        equipList[slotNum].equip = null;
        equip.equippedToHero = null;

        if (equipList[slotNum].isDisabled)
        {
            equipList[slotNum].isDisabled = false;
        }
        else
        {
            hero.Stats.RemoveEquipmentBonuses(equip);
        }

        hero.actorTagsDirty = true;
        hero.UpdateActor();
        return true;
    }

    public void CheckEquipmentValidity()
    {
        hero.deferActorUpdates = true;

        Equipment mainHand = GetEquipmentInSlot(EquipSlotType.Weapon);
        Equipment offHand = GetEquipmentInSlot(EquipSlotType.Offhand);
        if (mainHand == null && offHand is Weapon)
        {
            UnequipFromSlot(EquipSlotType.Offhand);
            EquipToSlot(offHand, EquipSlotType.Weapon);
        }
        else if (mainHand is Weapon && offHand != null)
        {
            HashSet<TagType> mainTypes = GetEquipmentTagTypes(mainHand);
            if (mainTypes.Contains(TagType.TwoHandedWeapon) && !hero.Stats.HasSpecialBonus(BonusStatType.TwoHandedWeaponsAreOneHanded))
            {
                if (!mainTypes.Contains(TagType.Spear) || !offHand.GetTagTypes().Contains(TagType.Shield) || !hero.Stats.HasSpecialBonus(BonusStatType.CanUseSpearWithShield))
                    UnequipFromSlot(EquipSlotType.Offhand);
            }
            else if (offHand is Weapon)
            {
                if (mainTypes.Contains(TagType.MeleeWeapon) && !GetEquipmentTagTypes(offHand).Contains(TagType.MeleeWeapon))
                    UnequipFromSlot(EquipSlotType.Offhand);
                else if (mainTypes.Contains(TagType.RangedWeapon) && !GetEquipmentTagTypes(offHand).Contains(TagType.RangedWeapon))
                    UnequipFromSlot(EquipSlotType.Offhand);
            }
        }

        hero.deferActorUpdates = false;
    }

    public int CheckAllEquipmentRequirements()
    {
        int changedCount = 0;
        foreach (var equipData in equipList)
        {
            if (equipData.equip == null)
                continue;
            if (!equipData.isDisabled && !CanEquipItem(equipData.equip))
            {
                equipData.isDisabled = true;
                hero.Stats.RemoveEquipmentBonuses(equipData.equip);
                changedCount++;
            }
            else if (equipData.isDisabled && CanEquipItem(equipData.equip))
            {
                equipData.isDisabled = false;
                hero.Stats.ApplyEquipmentBonuses(equipData.equip);
                changedCount++;
            }
        }

        if (changedCount > 0)
        {
            hero.actorTagsDirty = true;
        }

        return changedCount;
    }

    public bool CanEquipItem(Equipment equip)
    {
        HeroAttributes attributes = hero.Stats.Attributes;
        if (equip.strRequirement > attributes.Strength || equip.intRequirement > attributes.Intelligence || equip.dexRequirement > attributes.Dexterity || equip.levelRequirement > hero.Level)
            return false;
        return true;
    }

    public HashSet<TagType> GetEquipmentTagTypes(Equipment equip)
    {
        HashSet<TagType> groupTypes = equip.GetTagTypes();
        HashSet<TagType> additionalTypes = new HashSet<TagType>();

        if (hero.Stats.HasSpecialBonus(BonusStatType.OneHandedWeaponsAreTwoHanded) && groupTypes.Contains(TagType.OneHandedWeapon))
            additionalTypes.Add(TagType.TwoHandedWeapon);
        if (hero.Stats.HasSpecialBonus(BonusStatType.TwoHandedWeaponsAreOneHanded) && groupTypes.Contains(TagType.TwoHandedWeapon))
            additionalTypes.Add(TagType.OneHandedWeapon);

        groupTypes.UnionWith(additionalTypes);

        return groupTypes;
    }

    public HashSet<TagType> GetAllEquipmentTags(bool includeWeapons)
    {
        HashSet<TagType> types = new HashSet<TagType>() { TagType.DefaultTag };

        foreach (EquipData equipment in equipList)
        {
            if (equipment.equip == null || equipment.isDisabled)
                continue;
            if (equipment.equip is Weapon && !includeWeapons)
                continue;

            types.UnionWith(GetEquipmentTagTypes(equipment.equip));
        }

        if (GetEquipmentInSlot(EquipSlotType.Weapon) is Weapon && !equipList[(int)EquipSlotType.Weapon].isDisabled
            && GetEquipmentInSlot(EquipSlotType.Offhand) is Weapon && !equipList[(int)EquipSlotType.Offhand].isDisabled)
            types.Add(TagType.DualWield);

        return types;
    }

    public List<Equipment> GetAllEquipment()
    {
        List<Equipment> retList = new List<Equipment>();
        foreach (EquipData equipData in equipList)
        {
            if (!equipData.isDisabled)
                retList.Add(equipData.equip);
        }

        return retList;
    }

    private class EquipData
    {
        public Equipment equip;
        public bool isDisabled;

        public EquipData()
        {
            isDisabled = false;
        }
    }
}