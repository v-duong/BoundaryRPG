using System;
using System.Collections.Generic;
using System.Linq;

public class Hero : Actor
{
    public HeroEquipmentData EquipmentData;
    public new HeroStats Stats;
    public HeroArchetypeData PrimaryArchetype;
    public HeroArchetypeData SecondaryArchetype;

    public int ArchetypePoints { get; private set; }
    public int assignedTeam;
    public int killCount;
    public bool deferActorUpdates = false;
    public bool isLocked;

    public Hero(string name)
    {
        Id = Guid.NewGuid();
        Initialize(name);
    }

    public static Hero CreateNewHero(string name, ArchetypeBase primaryArchetype, ArchetypeBase subArchetype = null)
    {
        Hero hero = new Hero(name);
        hero.PrimaryArchetype = new HeroArchetypeData(primaryArchetype, hero);
        if (subArchetype != null)
            hero.SecondaryArchetype = new HeroArchetypeData(subArchetype, hero);
        hero.LevelUp();
        return hero;
    }

    public Hero(SaveData.HeroSaveData heroSaveData)
    {
        deferActorUpdates = true;
        Initialize(heroSaveData.name);
        Id = heroSaveData.id;
        Level = heroSaveData.level;
        ArchetypePoints = (int)(Level * 1.2f);
        Experience = heroSaveData.experience;
        Stats.LoadBaseStats(heroSaveData);

        PrimaryArchetype = new HeroArchetypeData(heroSaveData.primaryArchetypeData, this);

        if (heroSaveData.secondaryArchetypeData != null)
            SecondaryArchetype = new HeroArchetypeData(heroSaveData.secondaryArchetypeData, this);

        foreach (EquipSlotType equipSlot in Enum.GetValues(typeof(EquipSlotType)))
        {
            if (equipSlot == EquipSlotType.Ring)
                continue;

            if (heroSaveData.equipList[(int)equipSlot] != Guid.Empty)
            {
                EquipmentData.EquipToSlot(GameManager.Instance.PlayerStats.GetEquipmentByGuid(heroSaveData.equipList[(int)equipSlot]), equipSlot);
            }
        }

        deferActorUpdates = false;
        UpdateActor();
    }

    private void Initialize(string name)
    {
        Stats = new HeroStats(this);
        base.Stats = Stats;
        EquipmentData = new HeroEquipmentData(this);
        PrimaryArchetype = null;
        SecondaryArchetype = null;
        Name = name;
        Level = 0;
        Experience = 0;
        ArchetypePoints = 0;
        assignedTeam = -1;
    }

    private void LevelUp()
    {
        if (Level == 100)
            return;
        Level++;
        ModifyArchetypePoints(1);
        if (Level % 5 == 0)
            ModifyArchetypePoints(1);

        Stats.ApplyArchetypeLevelBonuses(PrimaryArchetype, false);
        GetNewArchetypeAbilities(PrimaryArchetype);

        if (SecondaryArchetype != null)
        {
            Stats.ApplyArchetypeLevelBonuses(SecondaryArchetype, true);
            GetNewArchetypeAbilities(SecondaryArchetype);
        }

        UpdateActor();
    }

    private void GetNewArchetypeAbilities(HeroArchetypeData archetypeData)
    {
        foreach (var learnItem in archetypeData.Base.abilityLearnList.Where(x => x.level == Level))
        {
            AbilityBase abilityBase = ResourceManager.Instance.GetAbilityBase(learnItem.abilityId);
            if (!abilitiesList.Contains(abilityBase))
                abilitiesList.Add(abilityBase);
        }
    }

    public int GetAbilityLevel(AbilityBase ability)
    {
        int level = GetBaseAbilityLevel();
        int levelBonuses = 0;

        HashSet<BonusStatType> bonusTypes = new HashSet<BonusStatType>
        {
            BonusStatType.AllAbilityLevel
        };

        var tagTypes = ability.GetTagTypes();

        if (tagTypes.Contains(TagType.Physical))
            bonusTypes.Add(BonusStatType.PhysicalAbilityLevel);
        if (tagTypes.Contains(TagType.Fire))
            bonusTypes.Add(BonusStatType.FireAbilityLevel);
        if (tagTypes.Contains(TagType.Cold))
            bonusTypes.Add(BonusStatType.ColdAbilityLevel);
        if (tagTypes.Contains(TagType.Lightning))
            bonusTypes.Add(BonusStatType.LightningAbilityLevel);
        if (tagTypes.Contains(TagType.Earth))
            bonusTypes.Add(BonusStatType.EarthAbilityLevel);
        if (tagTypes.Contains(TagType.Divine))
            bonusTypes.Add(BonusStatType.DivineAbilityLevel);
        if (tagTypes.Contains(TagType.Void))
            bonusTypes.Add(BonusStatType.VoidAbilityLevel);

        if (ability.abilityType == AbilityType.Attack)
            bonusTypes.Add(BonusStatType.AttackAbilityLevel);
        if (ability.abilityType == AbilityType.Magic)
            bonusTypes.Add(BonusStatType.MagicAbilityLevel);

        levelBonuses += Stats.GetStatBonus(bonusTypes.ToArray()).CalculateStat(0);

        return level + levelBonuses;
    }

    public void AddExperience(int experience)
    {
        if (Level >= 100)
            return;
        Experience += experience;
        while (Experience >= Helpers.GetRequiredExperience(Level + 1))
        {
            LevelUp();
            if (Level >= 100)
            {
                Experience = Helpers.GetRequiredExperience(100);
                break;
            }
        }
    }

    public override void UpdateActor()
    {
        if (deferActorUpdates)
            return;

        EquipmentData.CheckEquipmentValidity();
        Stats.Attributes.UpdateAttributes();

        int loopCount = 0;
        while (EquipmentData.CheckAllEquipmentRequirements() > 0 && loopCount < 25)
        {
            Stats.Attributes.UpdateAttributes();
            loopCount++;
        }

        Stats.UpdateDefenses(EquipmentData.GetAllEquipment());

        base.UpdateActor();
    }

    public override HashSet<TagType> GetTagTypes()
    {
        if (actorTagsDirty)
        {
            actorTags = EquipmentData.GetAllEquipmentTags(true);
            actorTags.UnionWith(statusTags);
            actorTagsDirty = false;
        }
        return actorTags;
    }

    public bool ModifyArchetypePoints(int mod, bool force = false)
    {
        ArchetypePoints += mod;
        if (GameManager.Instance != null)
            GameManager.Instance.heroAPIsDirty = true;
        return true;
    }

    public override void Death()
    {
        throw new System.NotImplementedException();
    }

    public override ActorType GetActorType()
    {
        return ActorType.ALLY;
    }

    public override DamageContainer CalculateAttackDamage(AbilityInstance ability, Actor target)
    {
        DamageContainer damageContainer = new DamageContainer();

        float criticalChance = Stats.GetStatBonusForAbility(ability, target, BonusStatType.Global_CriticalChance, BonusStatType.MagicCriticalChance).CalculateStat(ability.abilityBase.baseCritical);
        int criticalDamage = Stats.GetStatBonusForAbility(ability, target, BonusStatType.Global_CriticalDamage, BonusStatType.MagicCriticalDamage).CalculateStat(Helpers.BASE_CRITICAL_DAMAGE);

        foreach (ElementType e in Enum.GetValues(typeof(ElementType)))
        {
            damageContainer[e] = Stats.GetStatBonusForAbility(ability, target, Helpers.GetRelevantDamageBonuses(ability.abilityBase, e).ToArray()).CalculateStat(ability.GetBaseDamage(e));
        }

        return damageContainer;
    }
}