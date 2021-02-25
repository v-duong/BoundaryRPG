using System;
using System.Collections.Generic;
using System.Linq;

public class HeroArchetypeData
{
    public Guid Id { get; private set; }
    public ArchetypeBase Base { get; private set; }
    public Hero hero;
    public int AllocatedPoints { get; private set; }
    public string Name => Base.LocalizedName;

    private Dictionary<int, int> nodeLevels;

    public Dictionary<int, int> GetNodeLevels => new Dictionary<int, int>(nodeLevels);

    public HeroArchetypeData(ArchetypeBase archetype, Hero hero)
    {
        Id = Guid.NewGuid();
        Base = archetype;
        this.hero = hero;
        InitializeArchetypeData();
    }

    public HeroArchetypeData(SaveData.HeroArchetypeSaveData saveData, Hero hero)
    {
        Id = saveData.id;
        Base = ResourceManager.Instance.GetArchetypeBase(saveData.archetypeId);
        this.hero = hero;
        InitializeArchetypeData();

        foreach (var savedNode in saveData.nodeLevelData)
        {
            if (Base.GetNode(savedNode.nodeId) == null || hero.ArchetypePoints < 0)
            {
                ResetArchetypeTree();
                break;
            }
            if (savedNode.level == 0)
                continue;
            LoadNodeLevelsFromSave(Base.GetNode(savedNode.nodeId), savedNode.level);
        }
    }

    public void InitializeArchetypeData()
    {
        AllocatedPoints = 0;
        nodeLevels = new Dictionary<int, int>();

        foreach (ArchetypeBase.ArchetypeLearnedAbility learnItem in Base.abilityLearnList.Where(x => x.level <= hero.Level))
        {
            AbilityBase abilityBase = ResourceManager.Instance.GetAbilityBase(learnItem.abilityId);
            if (!hero.abilitiesList.Contains(abilityBase))
                hero.abilitiesList.Add(abilityBase);
        }

        InitializeNodeLevels();
    }

    public void InitializeNodeLevels()
    {
        foreach (var node in Base.nodeList)
        {
            int level = node.initialLevel;
            if (level == 1)
            {
                foreach (var bonus in node.bonuses)
                {
                    hero.Stats.AddArchetypeStatBonus(bonus.bonusType, bonus.restriction, bonus.modifyType, bonus.growthValue);
                }
            }
            nodeLevels.Add(node.id, level);
        }
    }

    private void LoadNodeLevelsFromSave(ArchetypeSkillNode node, int setLevel)
    {
        if (node == null)
            return;
        if (node.initialLevel == setLevel)
            return;

        hero.ModifyArchetypePoints(-(setLevel - node.initialLevel));

        foreach (var bonus in node.bonuses)
        {
            float bonusValue = 0f;
            if (setLevel > node.initialLevel)
            {
                if (setLevel == 1 && node.maxLevel == 1)
                {
                    bonusValue = bonus.growthValue;
                }
                else if (setLevel == node.maxLevel)
                {
                    int difference = setLevel - node.initialLevel - 1;
                    bonusValue = bonus.growthValue * difference + bonus.finalLevelValue;
                }
                else
                {
                    int difference = setLevel - node.initialLevel;
                    bonusValue = bonus.growthValue * difference;
                }
                hero.Stats.AddArchetypeStatBonus(bonus.bonusType, bonus.restriction, bonus.modifyType, bonusValue);
            }
        }

        foreach (var triggeredEffectBonus in node.triggeredEffects)
        {
            TriggeredEffect t = new TriggeredEffect(triggeredEffectBonus, triggeredEffectBonus.effectGrowthValue, this.Id);
            hero.Stats.AddTriggeredEffect(triggeredEffectBonus, t);
        }

        nodeLevels[node.id] = setLevel;
        AllocatedPoints += setLevel;
    }

    public bool LevelUpNode(ArchetypeSkillNode node)
    {
        if (nodeLevels[node.id] == node.maxLevel)
            return false;

        nodeLevels[node.id]++;
        AllocatedPoints++;

        int currentNodeLevel = nodeLevels[node.id];

        foreach (var bonus in node.bonuses)
        {
            float bonusValue = 0f;
            if (bonus.modifyType != ModifyType.Multiply || bonus.modifyType != ModifyType.FixedToValue)
            {
                if (currentNodeLevel == 1)
                {
                    bonusValue = bonus.growthValue;
                }
                else if (currentNodeLevel == node.maxLevel)
                    bonusValue = bonus.finalLevelValue;
                else
                    bonusValue = bonus.growthValue;

                hero.Stats.AddArchetypeStatBonus(bonus.bonusType, bonus.restriction, bonus.modifyType, bonusValue);
            }
            else
            {
                if (currentNodeLevel == 1)
                {
                    bonusValue = bonus.growthValue;
                }
                else
                {
                    if (currentNodeLevel == node.maxLevel)
                        bonusValue = bonus.growthValue * (currentNodeLevel - 1) + bonus.finalLevelValue;
                    else
                        bonusValue = bonus.growthValue * currentNodeLevel;
                    hero.Stats.RemoveArchetypeStatBonus(bonus.bonusType, bonus.restriction, bonus.modifyType, bonus.growthValue * (currentNodeLevel - 1));
                }

                hero.Stats.AddArchetypeStatBonus(bonus.bonusType, bonus.restriction, bonus.modifyType, bonusValue);
            }
        }

        if (currentNodeLevel == 1)
        {
            foreach (ArchetypeTriggeredEffectNodeProperty triggeredEffectBonus in node.triggeredEffects)
            {
                TriggeredEffect t = new TriggeredEffect(triggeredEffectBonus, triggeredEffectBonus.effectGrowthValue, Id);
                hero.Stats.AddTriggeredEffect(triggeredEffectBonus, t);
            }
        }
        else
        {
            foreach (ArchetypeTriggeredEffectNodeProperty triggeredEffectBonus in node.triggeredEffects)
            {
                hero.Stats.UpdateTriggeredEffect(triggeredEffectBonus.effectGrowthValue * currentNodeLevel, triggeredEffectBonus, Id);
            }
        }

        hero.UpdateActor();

        return true;
    }

    public bool DelevelNode(ArchetypeSkillNode node)
    {
        if (nodeLevels[node.id] == node.initialLevel)
            return false;

        int currentNodeLevel = nodeLevels[node.id];

        foreach (var bonus in node.bonuses)
        {
            float bonusValue = 0f;
            if (bonus.modifyType != ModifyType.Multiply || bonus.modifyType != ModifyType.FixedToValue)
            {
                if (currentNodeLevel == 1)
                {
                    bonusValue = bonus.growthValue;
                }
                else if (currentNodeLevel == node.maxLevel)
                    bonusValue = bonus.finalLevelValue;
                else
                    bonusValue = bonus.growthValue;

                hero.Stats.RemoveArchetypeStatBonus(bonus.bonusType, bonus.restriction, bonus.modifyType, bonusValue);
            }
            else
            {
                if (currentNodeLevel == 1)
                {
                    bonusValue = bonus.growthValue;
                }
                else
                {
                    if (currentNodeLevel == node.maxLevel)
                        bonusValue = bonus.growthValue * (currentNodeLevel - 1) + bonus.finalLevelValue;
                    else
                        bonusValue = bonus.growthValue * currentNodeLevel;
                    hero.Stats.AddArchetypeStatBonus(bonus.bonusType, bonus.restriction, bonus.modifyType, bonus.growthValue * (currentNodeLevel - 1));
                }

                hero.Stats.RemoveArchetypeStatBonus(bonus.bonusType, bonus.restriction, bonus.modifyType, bonusValue);
            }
        }

        if (currentNodeLevel == 1)
        {
            foreach (ArchetypeTriggeredEffectNodeProperty triggeredEffectBonus in node.triggeredEffects)
            {
                hero.Stats.RemoveTriggeredEffect(triggeredEffectBonus, Id);
            }
        }
        else
        {
            foreach (ArchetypeTriggeredEffectNodeProperty triggeredEffectBonus in node.triggeredEffects)
            {
                hero.Stats.UpdateTriggeredEffect(triggeredEffectBonus.effectGrowthValue * (currentNodeLevel - 1), triggeredEffectBonus, Id);
            }
        }
        nodeLevels[node.id]--;
        AllocatedPoints--;

        hero.UpdateActor();
        return true;
    }

    public int GetNodeLevel(ArchetypeSkillNode node)
    {
        return nodeLevels[node.id];
    }

    public bool IsNodeMaxLevel(ArchetypeSkillNode node)
    {
        if (nodeLevels[node.id] == node.maxLevel)
            return true;
        else
            return false;
    }

    public void ResetArchetypeTree()
    {
        foreach (ArchetypeSkillNode node in Base.nodeList)
        {
            while (GetNodeLevel(node) > node.initialLevel)
            {
                DelevelNode(node);
                hero.ModifyArchetypePoints(1);
            }
        }
    }
}