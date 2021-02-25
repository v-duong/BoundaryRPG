using System;

public class AbilityCoreItem : Item
{
    public AbilityBase Base { get; private set; }
    public Hero EquippedHero { get; private set; }
    public int EquippedSlot { get; private set; }

    public AbilitySourceType AbilitySourceType => AbilitySourceType.ABILITY_CORE;
    public string SourceName => Name;

    public Guid SourceId => Id;

    protected AbilityCoreItem(AbilityBase b, string name)
    {
        Id = Guid.NewGuid();
        Base = b;
        Name = name;
    }

    public AbilityCoreItem(Guid id, AbilityBase abilityBase, string name)
    {
        Id = id;
        Base = abilityBase;
        Name = name;
    }

    public override ItemType GetItemType()
    {
        return ItemType.Ability;
    }

    /*
    public static AbilityCoreItem CreateAbilityItemFromArchetype(ArchetypeItem archetypeItem, AbilityBase abilityBase)
    {
        if (!archetypeItem.Base.GetArchetypeAbilities(40).Contains(abilityBase))
            return null;
        else
        {
            GameManager.Instance.PlayerStats.RemoveArchetypeFromInventory(archetypeItem);
            string name = archetypeItem.Name + "'s " + abilityBase.LocalizedName;
            return new AbilityCoreItem(abilityBase, name);
        }
    }

    public static AbilityCoreItem CreateAbilityItemFromArchetype(ArchetypeBase archetypeItem, AbilityBase abilityBase)
    {
        {
            string name = archetypeItem.LocalizedName + "'s " + abilityBase.LocalizedName;
            return new AbilityCoreItem(abilityBase, name);
        }
    }

    public void OnAbilityEquip(AbilityBase ability, Hero hero, int slot)
    {
        EquippedHero = hero;
        EquippedSlot = slot;
    }

    public void OnAbilityUnequip(AbilityBase ability, Hero hero, int slot)
    {
        EquippedHero = null;
    }

    public Tuple<Hero, int> GetEquippedHeroAndSlot(AbilityBase ability)
    {
        if (EquippedHero == null)
            return null;
        else
            return new Tuple<Hero, int>(EquippedHero, EquippedSlot);
    }

    public void UnequipFromCurrentHero(AbilityBase abilityBase)
    {
        if (EquippedHero != null)
        {
            EquippedHero.UnequipAbility(EquippedSlot);
        }
    }
    */
}