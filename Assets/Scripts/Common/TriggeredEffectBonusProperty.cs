using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public abstract class TriggeredEffectBonusBase
{
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly TriggerType triggerType;

    [JsonProperty]
    public readonly TagType restriction;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly AbilityTargetType effectTargetType;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly EffectType effectType;

    [JsonProperty]
    public readonly float triggerChance;

}
public class AffixTriggeredEffectBonusProperty : TriggeredEffectBonusBase
{

    [JsonProperty]
    public readonly float effectMinValue;

    [JsonProperty]
    public readonly float effectMaxValue;

    [JsonProperty]
    public readonly bool readAsFloat;
}

public class ArchetypeTriggeredEffectNodeProperty : TriggeredEffectBonusBase
{

    [JsonProperty]
    public readonly float effectGrowthValue;

    [JsonProperty]
    public readonly float effectFinalValue;

}


public enum TriggerType
{
    OnHit,
    WhenHit,
    OnKill,
    OnCrit,
    OnBlock,
    OnDodge,
    OnParry,
    OnPhase,
    OnDeath,
}
