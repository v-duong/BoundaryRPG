using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

public class EnemyBase
{
    [JsonProperty]
    public readonly string idName;

    [JsonProperty]
    public readonly int level;

    [JsonProperty]
    public readonly int experience;

    [JsonProperty]
    public readonly bool isBoss;

    [JsonProperty]
    public readonly float healthMultiplier;

    [JsonProperty]
    public readonly float manaMultiplier;

    [JsonProperty]
    public readonly int[] resistances;

    [JsonProperty]
    [JsonConverter(typeof(StringEnumConverter))]
    public readonly EnemyType enemyType;

    [JsonProperty]
    [JsonConverter(typeof(StringEnumConverter))]
    public readonly PrimaryTargetingType targetingPriority;

    [JsonProperty]
    public readonly List<EnemyAbilityBase> abilitiesList;

    [JsonProperty]
    public readonly List<LevelScaledBonusProperty> leveledBonusProperties;

    [JsonProperty]
    public readonly string spriteName;

    [JsonProperty]
    public readonly float armorMultiplier;

    [JsonProperty]
    public readonly float magicArmorMultiplier;

    [JsonProperty]
    public readonly float dodgeMultiplier;

    [JsonProperty]
    public readonly float accuracyMultiplier;

    [JsonProperty]
    public readonly float baseSpeed;

    [JsonProperty]
    public readonly float attackDamageMultiplier;

    [JsonProperty]
    public readonly float attackCriticalChance;

    [JsonProperty]
    public readonly float attackDamageSpread;

    public string LocalizedName => LocalizationManager.Instance.GetLocalizationText_Enemy(idName, ".name");

    public class EnemyAbilityBase
    {
        [JsonProperty]
        public string abilityName;

        [JsonProperty]
        public float damageMultiplier;

        [JsonProperty]
        public float speedDelayModifier;
    }
}

public enum EnemyType
{
    NON_ATTACKER,
    TARGET_ATTACKER,
    HIT_AND_RUN,
    AURA_USER,
    DEBUFFER
}