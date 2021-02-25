using System;
using System.Collections.Generic;
using System.Linq;

public abstract class Actor
{
    public Guid Id { get; protected set; }
    public int Level { get; set; }
    public int Experience { get; set; }
    public string Name { get; set; }
    public string spriteName;

    public ActorStats Stats { get; protected set; }

    private readonly List<StatusEffect> statusEffects = new List<StatusEffect>();
    public readonly List<AbilityBase> abilitiesList = new List<AbilityBase>();

    protected HashSet<TagType> actorTags = new HashSet<TagType>();
    protected HashSet<TagType> statusTags = new HashSet<TagType>();
    public bool actorTagsDirty = true;

    public abstract HashSet<TagType> GetTagTypes();

    public abstract ActorType GetActorType();

    public abstract void Death();

    public float GetCurrentHealth()
    {
        return Stats.CurrentHealth;
    }

    public bool IsAlive
    {
        get { return GetCurrentHealth() > 0.0f; }
    }

    public bool IsDead
    {
        get { return GetCurrentHealth() <= 0.0f; }
    }

    public virtual void OnTurnStart()
    {
        UpdateStatusEffects();
        Stats.ApplyRegenEffects();

        if (IsDead)
            Death();
    }

    public virtual void OnTurnEnd()
    {
    }

    public void UpdateStatusEffects(int turnCount = 1)
    {
        int index = statusEffects.Count - 1;
        bool needsUpdate = false;

        while (index >= 0)
        {
            statusEffects[index].Update(turnCount);
            if (statusEffects[index].duration <= 0)
            {
                RemoveStatusEffect(statusEffects[index]);
                needsUpdate = true;
            }
            index--;
        }

        if (needsUpdate)
            UpdateActor();
    }

    public void AddStatusEffect(StatusEffect statusEffect)
    {
        List<StatusEffect> existingStatus = statusEffects.FindAll(x => x.GetType() == statusEffect.GetType());
        /*
        if (statusEffect.Source.GetActorType() != this.GetActorType())
            statusEffect.duration += Stats.AfflictedStatusDuration;
        */
        int stackCount;
        if (existingStatus.Count > 0 && statusEffect.StacksIncrementExistingEffect)
        {
            stackCount = existingStatus[0].Stacks;
            if (stackCount > 0 && stackCount < statusEffect.MaxStacks)
            {
                existingStatus[0].SetStacks(existingStatus[0].Stacks + 1);
                UpdateActor();
            }
            return;
        }
        else
        {
            stackCount = existingStatus.Count;
        }

        if (stackCount == statusEffect.MaxStacks)
        {
            StatusEffect effectToRemove = existingStatus.OrderBy(x => x.duration).First();
            statusEffects.Remove(effectToRemove);
        }
        else if (stackCount > statusEffect.MaxStacks)
            return;

        statusEffects.Add(statusEffect);
        //Debug.Log(Data.Name + " " + statusEffect + " " + statusEffect.GetSimpleEffectValue());
        statusEffect.OnApply();
        statusTags.Add(statusEffect.StatusTag);
        actorTagsDirty = true;
        UpdateActor();
    }

    public void RemoveStatusEffect(StatusEffect statusEffect)
    {
        statusEffect.OnExpire();

        statusEffects.Remove(statusEffect);

        if (GetStatusEffect(statusEffect.effectType) == null)
        {
            statusTags.Remove(statusEffect.StatusTag);
            actorTagsDirty = true;
        }
        UpdateActor();
    }

    public StatusEffect GetStatusEffect(EffectType effect)
    {
        List<StatusEffect> actorEffects = statusEffects.FindAll(x => x.effectType == effect);
        if (actorEffects.Count == 0)
            return null;
        else if (actorEffects.Count == 1)
            return actorEffects[0];
        else
        {
            actorEffects.OrderBy(x => x.GetSimpleEffectValue());
            return actorEffects[0];
        }
    }

    public List<StatusEffect> GetStatusEffectAll(EffectType effect)
    {
        return statusEffects.FindAll(x => x.effectType == effect);
    }

    public List<StatusEffect> GetStatusEffectAll(params EffectType[] effects)
    {
        return statusEffects.FindAll(x => effects.Contains(x.effectType));
    }

    public void ClearStatusEffects(bool useExpireEffects)
    {
        if (useExpireEffects)
        {
            foreach (StatusEffect effect in statusEffects)
            {
                effect.OnExpire();
            }
        }

        statusEffects.Clear();

        UpdateActor();
    }

    public virtual void UpdateActor()
    {
        Stats.UpdateStats();
    }

    public int GetBaseAbilityLevel()
    {
        if (Level != 100)
            return (int)((Level - 1) / 2d);
        else
            return 50;
    }

    public virtual void UseAbilityOnTarget(AbilityBase abilityBase, int level, Actor target)
    {
        DamageContainer damageDict;
        if (abilityBase.abilityType == AbilityType.Attack)
        {
        }
    }

    public abstract DamageContainer CalculateAttackDamage(AbilityInstance ability, Actor target);

    public virtual DamageContainer CalculateMagicDamage(AbilityInstance ability, Actor target)
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

public enum ActorType
{
    ENEMY,
    ALLY
}

public class DamageContainer
{
    public Dictionary<ElementType, int> damageDict;

    public DamageContainer()
    {
        damageDict = new Dictionary<ElementType, int>();
        foreach (ElementType e in Enum.GetValues(typeof(ElementType)))
        {
            damageDict.Add(e, 0);
        }
    }

    public int this[ElementType e]
    {
        get { return damageDict[e]; }
        set { damageDict[e] = value; }
    }
}