using System;

public interface IAbilitySource
{
    string SourceName { get; }
    Guid SourceId { get; }
    AbilitySourceType AbilitySourceType { get; }
    void OnAbilityEquip(AbilityBase ability, Hero hero, int slot);
    void OnAbilityUnequip(AbilityBase ability, Hero hero, int slot);
    void UnequipFromCurrentHero(AbilityBase abilityBase);
    Tuple<Hero, int> GetEquippedHeroAndSlot(AbilityBase ability);
}