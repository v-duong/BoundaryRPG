using System.Collections.Generic;
using UnityEngine;

public abstract class StatusEffect
{
    protected readonly Actor target;
    public Actor Source { get; protected set; }

    public EffectType effectType;
    public float duration;

    public abstract void OnApply();

    public abstract void OnExpire();

    public abstract void Update(float deltaTime);

    public abstract float GetEffectValue();

    public abstract float GetSimpleEffectValue();

    public abstract TagType StatusTag { get; }
    public int MaxStacks = 1;
    public virtual bool StacksIncrementExistingEffect => false;
    public int Stacks { get; protected set; }

    public virtual void SetStacks(int newStackValue)
    {
        Stacks = newStackValue;
        if (newStackValue > MaxStacks)
            Stacks = MaxStacks;
    }

    protected float DurationUpdate(int turn)
    {
        if (turn > duration)
        {
            float oldDuration = duration;
            duration = 0;
            return oldDuration;
        }
        else
        {
            duration -= turn;
            return turn;
        }
    }

    public virtual void RefreshDuration(float duration)
    {
        if (this.duration < duration)
            this.duration = duration;
    }

    public StatusEffect(Actor target, Actor source)
    {
        this.target = target;
        Source = source;
        Stacks = 1;
    }

    public StatusEffect Clone()
    {
        return (StatusEffect)MemberwiseClone();
    }
}

