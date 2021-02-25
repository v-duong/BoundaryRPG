using System;
using System.Collections.Generic;
using System.Linq;

public abstract class ActorStats
{
    public readonly Actor Actor;
    public float BaseHealth { get; protected set; }
    public int MaximumHealth { get; protected set; }
    public float CurrentHealth { get; set; }
    public float HealthRegenRate { get; protected set; }
    public float BaseMana { get; protected set; }
    public int MaximumMana { get; protected set; }
    public float CurrentMana { get; set; }
    public float ManaRegenRate { get; protected set; }

    public int BaseArmor { get; protected set; }
    public int BaseMagicArmor { get; protected set; }
    public int BaseDodgeRating { get; protected set; }
    public int BaseAttackPhasing { get; protected set; }
    public int BaseSpellPhasing { get; protected set; }

    public int Armor { get; protected set; }
    public int MagicArmor { get; protected set; }
    public int DodgeRating { get; protected set; }
    public int AttackPhasing { get; protected set; }
    public int SpellPhasing { get; protected set; }

    public float BlockChance { get; protected set; }
    public float BlockProtection { get; protected set; }

    public float AttackParryChance { get; protected set; }
    public float MagicParryChance { get; protected set; }

    public float DamageTakenModifier { get; protected set; }
    public float AfflictedStatusDamageResistance { get; protected set; }
    public float AfflictedStatusThreshold { get; protected set; }
    public int AfflictedStatusAvoidance { get; protected set; }
    public float AfflictedStatusDuration { get; protected set; }
    public float PoisonResistance { get; protected set; }
    public float DamageOverTimeResistance { get; protected set; }
    public float AggroPriorityModifier { get; protected set; }
    public float BaseSpeed { get; protected set; }
    public int Speed { get; protected set; }
    public int BaseAccuracy { get; protected set; }
    public int Accuracy { get; protected set; }

    protected ActorElementStats ElementStats { get; private set; }

    protected Dictionary<BonusStatType, StatBonusCollection> statBonuses;
    protected Dictionary<BonusStatType, StatBonus> temporaryBonuses;
    protected Dictionary<BonusStatType, int> specialBonuses;

    public Dictionary<TriggerType, List<TriggeredEffect>> TriggeredEffects { get; protected set; }

    public abstract StatBonus GetTotalStatBonus(BonusStatType type, IEnumerable<TagType> tags, Dictionary<BonusStatType, StatBonusCollection> additionalBonusProperties, StatBonus existingBonus = null);

    public abstract int GetResistance(ElementType element);

    protected ActorStats(Actor actor)
    {
        this.Actor = actor;
        ElementStats = new ActorElementStats();
        statBonuses = new Dictionary<BonusStatType, StatBonusCollection>();
        temporaryBonuses = new Dictionary<BonusStatType, StatBonus>();
        specialBonuses = new Dictionary<BonusStatType, int>();
    }

    public void ApplyRegenEffects(int turnCount = 1)
    {
        ModifyCurrentHealth(-HealthRegenRate * turnCount);
        ModifyCurrentMana(-ManaRegenRate * turnCount);
    }

    public void ModifyCurrentHealth(float mod)
    {
        if (mod == 0)
            return;

        if (CurrentHealth - mod > MaximumHealth)
            CurrentHealth = MaximumHealth;
        else
            CurrentHealth -= mod;
    }

    public void ModifyCurrentMana(float mod)
    {
        if (mod == 0)
            return;

        if (CurrentMana - mod > MaximumMana)
            CurrentMana = MaximumMana;
        else
            CurrentMana -= mod;
    }

    public void AddStatBonus(BonusStatType type, TagType restriction, ModifyType modifier, float value)
    {
        if (type >= (BonusStatType)Helpers.SPECIAL_BONUS_START)
        {
            AddSpecialBonus(type);
            return;
        }

        if (!statBonuses.ContainsKey(type))
            statBonuses.Add(type, new StatBonusCollection());
        statBonuses[type].AddBonus(restriction, modifier, value);
    }

    public bool RemoveStatBonus(BonusStatType type, TagType restriction, ModifyType modifier, float value)
    {
        if (type >= (BonusStatType)Helpers.SPECIAL_BONUS_START)
        {
            RemoveSpecialBonus(type);
            return true;
        }
        bool isRemoved = statBonuses[type].RemoveBonus(restriction, modifier, value);
        if (statBonuses[type].IsEmpty())
        {
            statBonuses.Remove(type);
        }
        return isRemoved;
    }

    public void AddTemporaryBonus(float value, BonusStatType type, ModifyType modifier)
    {
        if (!temporaryBonuses.ContainsKey(type))
            temporaryBonuses.Add(type, new StatBonus());
        temporaryBonuses[type].AddBonus(modifier, value);
        Actor.UpdateActor();
    }

    public void RemoveTemporaryBonus(float value, BonusStatType type, ModifyType modifier)
    {
        if (temporaryBonuses.ContainsKey(type))
            temporaryBonuses[type].RemoveBonus(modifier, value);
        Actor.UpdateActor();
    }

    public void AddSpecialBonus(BonusStatType type)
    {
        if (!specialBonuses.ContainsKey(type))
            specialBonuses.Add(type, 0);
        specialBonuses[type]++;
        Actor.UpdateActor();
    }

    public void RemoveSpecialBonus(BonusStatType type)
    {
        if (!specialBonuses.ContainsKey(type))
            return;

        specialBonuses[type]--;

        if (specialBonuses[type] == 0)
            specialBonuses.Remove(type);

        Actor.UpdateActor();
    }

    public bool HasSpecialBonus(BonusStatType type)
    {
        return specialBonuses.ContainsKey(type) && specialBonuses[type] > 0;
    }

    public void ClearTemporaryBonuses(bool updateActorData)
    {
        temporaryBonuses.Clear();
        if (updateActorData)
            Actor.UpdateActor();
    }

    public void UpdateTriggeredEffect(float updatedValue, TriggeredEffectBonusBase triggeredEffect, Guid sourceGuid)
    {
        TriggeredEffect t = TriggeredEffects[triggeredEffect.triggerType].Find(x => x.SourceGuid == sourceGuid && x.BaseEffect == triggeredEffect);
        t.UpdateTriggerVariable(updatedValue);
    }

    public void AddTriggeredEffect(TriggeredEffectBonusBase triggeredEffect, TriggeredEffect effectInstance)
    {
        TriggeredEffects[triggeredEffect.triggerType].Add(effectInstance);
    }

    public void RemoveTriggeredEffect(TriggeredEffectBonusBase triggeredEffect, Guid sourceGuid)
    {
        TriggeredEffect t = TriggeredEffects[triggeredEffect.triggerType].Find(x => x.SourceGuid == sourceGuid && x.BaseEffect == triggeredEffect);
        TriggeredEffects[triggeredEffect.triggerType].Remove(t);
    }

    public virtual void UpdateStats()
    {
        ApplyHealthBonuses();
        ApplyManaBonuses();
        ApplyElementalBonuses();

        AfflictedStatusAvoidance = GetStatBonus(BonusStatType.AfflictedStatusAvoidance).CalculateStat(0);
        AfflictedStatusDuration = Math.Max(GetStatBonus(BonusStatType.AfflictedStatusDuration).CalculateStat(1f), 0.01f);
        AfflictedStatusDamageResistance = Math.Min(GetStatBonus(BonusStatType.AfflictedStatusDamageResistance).CalculateStat(0f), 100) / 100f;
        PoisonResistance = Math.Min(GetStatBonus(BonusStatType.PoisonResistance).CalculateStat(0), 100) / 100f;
        DamageTakenModifier = Math.Max(GetStatBonus(BonusStatType.DamageTaken).CalculateStat(1f), 0.01f);
        Speed = (int)Math.Max(GetStatBonus(BonusStatType.Speed).CalculateStat(BaseSpeed), 1);

        AggroPriorityModifier = Math.Max(GetStatBonus(BonusStatType.AggroPriorityRate).CalculateStat(1f), 0);
    }

    protected void ApplyHealthBonuses()
    {
        float percentage = CurrentHealth / MaximumHealth;
        MaximumHealth = (int)Math.Max(Math.Round(GetStatBonus(BonusStatType.MaxHealth).CalculateStat(BaseHealth), 0), 1);
        CurrentHealth = MaximumHealth * percentage;

        float percentHealthRegen = GetStatBonus(BonusStatType.HealthRegenPercent).CalculateStat(0f) / 100f;
        HealthRegenRate = GetStatBonus(BonusStatType.HealthRegen).CalculateStat(percentHealthRegen * MaximumHealth);
    }

    protected void ApplyManaBonuses()
    {
        float percentage = CurrentMana / MaximumMana;
        MaximumMana = (int)Math.Max(GetStatBonus(BonusStatType.MaxMana).CalculateStat(BaseMana), 0);
        CurrentMana = MaximumMana * percentage;
        float percentManaRegen = GetStatBonus(BonusStatType.ManaRegenPercent).CalculateStat(0f) / 100f;
        ManaRegenRate = GetStatBonus(BonusStatType.ManaRegen).CalculateStat(percentManaRegen * ManaRegenRate);
    }

    protected void ApplyElementalBonuses()
    {
        BonusStatType[] elementalMaxResistances = { BonusStatType.MaxElementalResistances, BonusStatType.MaxAllNonPhysicalResistances };
        BonusStatType[] astralMaxResistances = { BonusStatType.MaxAstralResistances, BonusStatType.MaxAllNonPhysicalResistances };

        ElementStats.SetResistanceCap(ElementType.Fire, GetStatBonus(elementalMaxResistances.Append(BonusStatType.MaxFireResistance).ToArray()).CalculateStat(Helpers.DEFAULT_RESISTANCE_CAP));
        ElementStats.SetResistanceCap(ElementType.Cold, GetStatBonus(elementalMaxResistances.Append(BonusStatType.MaxColdResistance).ToArray()).CalculateStat(Helpers.DEFAULT_RESISTANCE_CAP));
        ElementStats.SetResistanceCap(ElementType.Lightning, GetStatBonus(elementalMaxResistances.Append(BonusStatType.MaxLightningResistance).ToArray()).CalculateStat(Helpers.DEFAULT_RESISTANCE_CAP));
        ElementStats.SetResistanceCap(ElementType.Earth, GetStatBonus(elementalMaxResistances.Append(BonusStatType.MaxEarthResistance).ToArray()).CalculateStat(Helpers.DEFAULT_RESISTANCE_CAP));
        ElementStats.SetResistanceCap(ElementType.Divine, GetStatBonus(astralMaxResistances.Append(BonusStatType.MaxDivineResistance).ToArray()).CalculateStat(Helpers.DEFAULT_RESISTANCE_CAP));
        ElementStats.SetResistanceCap(ElementType.Void, GetStatBonus(astralMaxResistances.Append(BonusStatType.MaxVoidResistance).ToArray()).CalculateStat(Helpers.DEFAULT_RESISTANCE_CAP));

        BonusStatType[] elementalResistances = { BonusStatType.ElementalResistances, BonusStatType.AllNonPhysicalResistances };
        BonusStatType[] astralResistances = { BonusStatType.AstralResistances, BonusStatType.AllNonPhysicalResistances };

        ElementStats[ElementType.Physical] = (int)GetStatBonus(BonusStatType.PhysicalResistance).CalculateStat(0f);
        ElementStats[ElementType.Fire] = (int)GetStatBonus(elementalResistances.Append(BonusStatType.FireResistance).ToArray()).CalculateStat(0f);
        ElementStats[ElementType.Cold] = (int)GetStatBonus(elementalResistances.Append(BonusStatType.ColdResistance).ToArray()).CalculateStat(0f);
        ElementStats[ElementType.Lightning] = (int)GetStatBonus(elementalResistances.Append(BonusStatType.LightningResistance).ToArray()).CalculateStat(0f);
        ElementStats[ElementType.Earth] = (int)GetStatBonus(elementalResistances.Append(BonusStatType.EarthResistance).ToArray()).CalculateStat(0f);
        ElementStats[ElementType.Divine] = (int)GetStatBonus(astralResistances.Append(BonusStatType.DivineResistance).ToArray()).CalculateStat(0f);
        ElementStats[ElementType.Void] = (int)GetStatBonus(astralResistances.Append(BonusStatType.VoidResistance).ToArray()).CalculateStat(0f);

        ElementStats.SetNegation(ElementType.Physical, GetStatBonus(BonusStatType.PhysicalResistanceNegation).CalculateStat(0));
        ElementStats.SetNegation(ElementType.Fire, GetStatBonus(BonusStatType.FireResistanceNegation, BonusStatType.ElementalResistanceNegation).CalculateStat(0));
        ElementStats.SetNegation(ElementType.Cold, GetStatBonus(BonusStatType.ColdResistanceNegation, BonusStatType.ElementalResistanceNegation).CalculateStat(0));
        ElementStats.SetNegation(ElementType.Lightning, GetStatBonus(BonusStatType.LightningResistanceNegation, BonusStatType.ElementalResistanceNegation).CalculateStat(0));
        ElementStats.SetNegation(ElementType.Earth, GetStatBonus(BonusStatType.EarthResistanceNegation, BonusStatType.ElementalResistanceNegation).CalculateStat(0));
        ElementStats.SetNegation(ElementType.Divine, GetStatBonus(BonusStatType.DivineResistanceNegation, BonusStatType.AstralResistanceNegation).CalculateStat(0));
        ElementStats.SetNegation(ElementType.Void, GetStatBonus(BonusStatType.VoidResistanceNegation, BonusStatType.AstralResistanceNegation).CalculateStat(0));
    }

    public StatBonus GetStatBonus(IEnumerable<TagType> tags, params BonusStatType[] types)
    {
        StatBonus bonus = new StatBonus();
        foreach (BonusStatType bonusType in types)
        {
            bonus = GetTotalStatBonus(bonusType, tags, null, bonus);
        }
        return bonus;
    }

    public StatBonus GetStatBonus(params BonusStatType[] types)
    {
        StatBonus bonus = new StatBonus();
        foreach (BonusStatType bonusType in types)
        {
            bonus = GetTotalStatBonus(bonusType, Actor.GetTagTypes(), null, bonus);
        }
        return bonus;
    }

    public StatBonus GetStatBonusForAbility(AbilityInstance abilityInstance, Actor target, params BonusStatType[] types)
    {
        var tags = Actor.GetTagTypes();

        if (target != null)
            tags.UnionWith(target.GetTagTypes());

        StatBonus bonus = new StatBonus();
        foreach (BonusStatType bonusType in types)
        {
            bonus = GetTotalStatBonus(bonusType, tags, abilityInstance.bonusProperties, bonus);
        }
        return bonus;
    }
}