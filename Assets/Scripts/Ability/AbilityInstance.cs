using System.Collections.Generic;

public class AbilityInstance
{
    public AbilityBase abilityBase;
    public int level;
    public Dictionary<BonusStatType, StatBonusCollection> bonusProperties;

    public AbilityInstance(AbilityBase b, int level)
    {
        abilityBase = b;
        this.level = level;
        bonusProperties = new Dictionary<BonusStatType, StatBonusCollection>();

        foreach (LevelScaledBonusProperty bonusProp in abilityBase.bonusProperties)
        {
            if (!bonusProperties.ContainsKey(bonusProp.bonusType))
                bonusProperties.Add(bonusProp.bonusType, new StatBonusCollection());
            bonusProperties[bonusProp.bonusType].AddBonus(bonusProp.restriction, bonusProp.modifyType, bonusProp.initialValue + bonusProp.growthValue * level);
        }
    }
    
    public int GetBaseDamage(ElementType e)
    {
        return abilityBase.damageLevels[e][level];
    }

    public float GetWeaponMultiplier()
    {
        return abilityBase.weaponMultiplier + abilityBase.weaponMultiplierScaling * level;
    }
}