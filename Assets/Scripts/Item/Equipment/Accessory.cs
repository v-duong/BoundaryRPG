using System.Collections.Generic;

public class Accessory : Equipment
{
    public Accessory(EquipmentBase e, int ilvl) : base(e, ilvl)
    {
    }

    public override ItemType GetItemType()
    {
        return ItemType.Accessory;
    }

    public override bool UpdateItemStats()
    {
        base.UpdateItemStats();
        return true;
    }

    public override HashSet<TagType> GetTagTypes()
    {
        HashSet<TagType> tags = new HashSet<TagType>
        {
            TagType.AllAccessory,
            Base.groupTag
        };
        return tags;
    }
}