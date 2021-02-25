using Newtonsoft.Json;
using System.Collections.Generic;

public class ArchetypeBase
{
    [JsonProperty]
    public readonly string idName;

    [JsonProperty]
    public readonly int stars;

    [JsonProperty]
    public readonly int dropLevel;

    [JsonProperty]
    public readonly float healthGrowth;

    [JsonProperty]
    public readonly float manaGrowth;

    [JsonProperty]
    public readonly float strengthGrowth;

    [JsonProperty]
    public readonly float intelligenceGrowth;
    [JsonProperty]
    public readonly float dexterityGrowth;

    [JsonProperty]
    public readonly float vitalityGrowth;
    [JsonProperty]
    public readonly float willGrowth;

    [JsonProperty]
    public readonly float speedGrowth;

    [JsonProperty]
    public readonly int spawnWeight;

    [JsonProperty]
    public readonly List<ArchetypeSkillNode> nodeList;
    [JsonProperty]
    public readonly List<ArchetypeLearnedAbility> abilityLearnList;

    public string LocalizedName => LocalizationManager.Instance.GetLocalizationText_ArchetypeName(idName);

    public ArchetypeSkillNode GetNode(int nodeId)
    {
        return nodeList.Find(x => x.id == nodeId);
    }

    public List<AbilityBase> GetArchetypeAbilities(int currentLevel = 100)
    {
        List<AbilityBase> ret = new List<AbilityBase>();
        foreach(var x in abilityLearnList)
        {
            if (currentLevel >= x.level)
                ret.Add(ResourceManager.Instance.GetAbilityBase(x.abilityId));
        }
        return ret;
    }

    public class ArchetypeLearnedAbility
    {

        [JsonProperty]
        public readonly int level;
        [JsonProperty]
        public readonly string abilityId;
    }
}