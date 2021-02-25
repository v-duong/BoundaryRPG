using System;
using System.Collections.Generic;

public class TriggeredEffect
{
    public Guid SourceGuid { get; private set; }
    public TriggeredEffectBonusBase BaseEffect { get; private set; }
    public float TriggerVariable { get; private set; }

    public TriggeredEffect(TriggeredEffectBonusBase baseEffect, float value, Guid guid)
    {
        BaseEffect = baseEffect;
        TriggerVariable = value;
        SourceGuid = guid;
    }

    public void UpdateTriggerVariable(float value)
    {
        TriggerVariable = value;
    }
    
    public bool RollTriggerChance()
    {
        return Helpers.RollChance(BaseEffect.triggerChance + TriggerVariable);
    }

    public void OnTrigger(Actor target, Actor source)
    {
        if (!RollTriggerChance())
        {
            return;
        }

        switch (BaseEffect.effectTargetType)
        {
            case AbilityTargetType.Self:
                target = source;
                ApplyEffect(target, source);
                return;

            case AbilityTargetType.Enemy:
                ApplyEffect(target, source);
                return;
            case AbilityTargetType.Ally:
                break;
            case AbilityTargetType.All:
                break;
            case AbilityTargetType.None:
                break;
            default:
                break;
        }
    }

    private void ApplyEffect(Actor target, Actor source)
    {
        switch (BaseEffect.effectType)
        {
            case EffectType.Bleed:
            case EffectType.Burn:
            case EffectType.Chill:
            case EffectType.Electrocute:
            case EffectType.Fracture:
            case EffectType.Pacify:
            case EffectType.Radiation:
            case EffectType.Poison:
                break;
            case EffectType.RetaliationDamage:
                break;
        }
    }
}