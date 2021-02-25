using System;
using System.Collections.Generic;

public class Armor : Equipment
{

    public int armor;
    public int magicArmor;
    public int dodgeRating;
    public int blockChance;
    public int blockProtection;

    public Armor(EquipmentBase e, int ilvl) : base(e, ilvl)
    {
        armor = e.armor;
        magicArmor = e.magicArmor;
        dodgeRating = e.dodgeRating;
        blockChance = (int)e.blockChance;
        blockProtection = e.blockProtection;
    }

    public override ItemType GetItemType()
    {
        return ItemType.Armor;
    }

    public override bool UpdateItemStats()
    {
        base.UpdateItemStats();
        Dictionary<BonusStatType, StatBonus> bonusTotals = new Dictionary<BonusStatType, StatBonus>();
        GetLocalModValues(bonusTotals, GetAllAffixes(), ItemType.Armor);

        armor = CalculateStat(Base.armor, bonusTotals, BonusStatType.LocalArmor);
        magicArmor = CalculateStat(Base.magicArmor, bonusTotals, BonusStatType.LocalMagicArmor);
        dodgeRating = CalculateStat(Base.dodgeRating, bonusTotals, BonusStatType.LocalDodgeRating);
        blockChance = (int)CalculateStat(Base.blockChance, bonusTotals, BonusStatType.LocalBlockChance);
        blockProtection = CalculateStat(Base.blockProtection, bonusTotals, BonusStatType.LocalBlockProtection);

        return true;
    }

    public override HashSet<TagType> GetTagTypes()
    {
        HashSet<TagType> tags = new HashSet<TagType>
        {
            Base.groupTag
        };

        switch (Base.equipSlot)
        {
            case EquipSlotType.BodyArmor:
                tags.Add(TagType.BodyArmor);
                tags.Add(TagType.AllArmor);
                break;

            case EquipSlotType.Weapon:
                break;

            case EquipSlotType.Offhand:
                switch (Base.groupTag)
                {
                    case TagType.Shield:
                        tags.Add(TagType.AllArmor);
                        break;
                    default:
                        break;
                }
                break;

            case EquipSlotType.Headgear:
                tags.Add(TagType.Headgear);
                tags.Add(TagType.AllArmor);
                break;

            case EquipSlotType.Gloves:
                tags.Add(TagType.Gloves);
                tags.Add(TagType.AllArmor);
                break;

            case EquipSlotType.Boots:
                tags.Add(TagType.Boots);
                tags.Add(TagType.AllArmor);
                break;

            case EquipSlotType.Belt:
                tags.Add(TagType.Belt);
                tags.Add(TagType.AllAccessory);
                break;

            case EquipSlotType.Necklace:
                tags.Add(TagType.Necklace);
                tags.Add(TagType.AllAccessory);
                break;

            case EquipSlotType.RingSlot1:
            case EquipSlotType.RingSlot2:
            case EquipSlotType.Ring:
                tags.Add(TagType.Ring);
                tags.Add(TagType.AllAccessory);
                break;
        }
        return tags;
    }
}