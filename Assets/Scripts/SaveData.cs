using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class SaveData
{
    [NonSerialized]
    private Dictionary<Guid, EquipSaveData> equipDict = new Dictionary<Guid, EquipSaveData>();

    [NonSerialized]
    private Dictionary<Guid, HeroSaveData> heroDict = new Dictionary<Guid, HeroSaveData>();

    private readonly List<ConsumablesContainer> consumableList = new List<ConsumablesContainer>();
    private readonly List<EquipSaveData> equipList = new List<EquipSaveData>();
    private readonly List<AbilityCoreSaveData> abilityCoreList = new List<AbilityCoreSaveData>();
    private readonly List<HeroSaveData> heroList = new List<HeroSaveData>();
    private readonly List<ArchetypeItemSaveData> archetypeItemList = new List<ArchetypeItemSaveData>();
    private readonly List<TeamFormationSaveData> heroTeamList = new List<TeamFormationSaveData>();
    private Dictionary<string, int> stageClearInfo = new Dictionary<string, int>();
    private Dictionary<int, bool> worldUnlockInfo = new Dictionary<int, bool>();
    public int expStock;
    public int itemFragments;
    public int archetypeFragments;
    public int lastPlayedWorld;
    public bool hasSeenStartingMessage;
    public bool showDamageNumbers;
    public int saveVersion = Helpers.CURRENT_SAVE_VERSION;

    public void SaveAll()
    {
        SaveAllEquipmentData();
        SaveAllAbilityCoreData();
        SaveAllArchetypeItemData();
        SaveAllHero();
        SavePlayerData();
    }

    public void LoadAll()
    {
        LoadEquipmentData();
        LoadAbilityCoreData();
        LoadArchetypeItemData();
        LoadHero();
        LoadPlayerData();
    }

    public void SavePlayerData()
    {
        PlayerStats ps = GameManager.Instance.PlayerStats;

        foreach (KeyValuePair<ConsumableType, int> pair in ps.consumables)
        {
            ConsumablesContainer c = consumableList.Find(x => x.consumable == pair.Key);
            if (c != null)
                c.value = pair.Value;
            else
                consumableList.Add(new ConsumablesContainer(pair.Key, pair.Value));
        }

        expStock = ps.ExpStock;
        itemFragments = ps.ItemFragments;
        archetypeFragments = ps.ArchetypeFragments;

        stageClearInfo = ps.stageClearInfo;
        worldUnlockInfo = ps.worldUnlockInfo;
        lastPlayedWorld = ps.lastPlayedWorld;
        hasSeenStartingMessage = ps.hasSeenStartingMessage;
        showDamageNumbers = ps.showDamageNumbers;

        heroTeamList.Clear();
        for (int i = 0; i < PlayerStats.HERO_TEAM_MAX_NUM; i++)
        {
            heroTeamList.Add( new TeamFormationSaveData(ps.heroTeams[i]));
        }
    }

    public void LoadPlayerData()
    {
        PlayerStats ps = GameManager.Instance.PlayerStats;

        foreach (ConsumablesContainer c in consumableList)
            ps.consumables[c.consumable] = c.value;
        ps.SetExpStock(expStock);
        ps.SetArchetypeFragments(archetypeFragments);
        ps.SetItemFragments(itemFragments);
        ps.stageClearInfo = stageClearInfo;
        ps.worldUnlockInfo = worldUnlockInfo;
        ps.lastPlayedWorld = lastPlayedWorld;
        ps.hasSeenStartingMessage = hasSeenStartingMessage;
        ps.showDamageNumbers = showDamageNumbers;

        if (heroTeamList != null && heroTeamList.Count > 0)
        {
            List<Hero> heroes = ps.HeroList.ToList();
            for (int i = 0; i < heroTeamList.Count; i++)
            {
                TeamFormationSaveData formationSaveData = (TeamFormationSaveData)heroTeamList[i];
                foreach (Guid guid in formationSaveData.frontLine)
                {
                    Hero hero = heroes.Find(y => y.Id == guid);
                    if (hero != null)
                        ps.heroTeams[i].AddToFrontLine(hero);
                }
                foreach (Guid guid in formationSaveData.backLine)
                {
                    Hero hero = heroes.Find(y => y.Id == guid);
                    if (hero != null)
                        ps.heroTeams[i].AddToBackLine(hero);
                }
            }
        }
    }

    public void SaveAllAbilityCoreData()
    {
        abilityCoreList.Clear();
        foreach (AbilityCoreItem abilityCore in GameManager.Instance.PlayerStats.AbilityInventory)
        {
            SaveAbilityCoreData(abilityCore);
        }
    }

    public void SaveAbilityCoreData(AbilityCoreItem abilityCore)
    {
        abilityCoreList.Add(new AbilityCoreSaveData(abilityCore.Id, abilityCore.Base.idName, abilityCore.Name));
    }

    public void RemoveAbilityCoreData(AbilityCoreItem abilityCore)
    {
        AbilityCoreSaveData saveData = abilityCoreList.Find(x => x.id == abilityCore.Id);
        if (saveData != null)
            abilityCoreList.Remove(saveData);
    }

    public void LoadAbilityCoreData()
    {
        GameManager.Instance.PlayerStats.ClearAbilityInventory();
        foreach (AbilityCoreSaveData coreData in abilityCoreList)
        {
            AbilityBase abilityBase = ResourceManager.Instance.GetAbilityBase(coreData.baseId);
            if (abilityBase == null)
                continue;
            GameManager.Instance.PlayerStats.AddAbilityToInventory(new AbilityCoreItem(coreData.id, abilityBase, coreData.name));
        }
    }

    public void SaveAllArchetypeItemData()
    {
        archetypeItemList.Clear();
        foreach (ArchetypeItem archetypeItem in GameManager.Instance.PlayerStats.ArchetypeInventory)
        {
            SaveArchetypeItemData(archetypeItem);
        }
    }

    public void SaveArchetypeItemData(ArchetypeItem archetypeItem)
    {
        archetypeItemList.Add(new ArchetypeItemSaveData(archetypeItem.Id, archetypeItem.Base.idName));
    }

    public void RemoveArchetypeItemData(ArchetypeItem archetypeItem)
    {
        ArchetypeItemSaveData saveData = archetypeItemList.Find(x => x.id == archetypeItem.Id);
        if (saveData != null)
            archetypeItemList.Remove(saveData);
    }

    public void LoadArchetypeItemData()
    {
        GameManager.Instance.PlayerStats.ClearArchetypeItemInventory();
        foreach (ArchetypeItemSaveData archetypeData in archetypeItemList)
        {
            GameManager.Instance.PlayerStats.AddArchetypeToInventory(new ArchetypeItem(archetypeData.id, archetypeData.baseId));
        }
    }

    public void SaveAllEquipmentData()
    {
        equipDict.Clear();
        equipList.Clear();
        foreach (Equipment equipItem in GameManager.Instance.PlayerStats.EquipmentInventory)
        {
            SaveEquipmentData(equipItem);
        }
    }

    public void SaveEquipmentData(Equipment equipItem)
    {
        EquipSaveData equipData;
        if (!equipDict.ContainsKey(equipItem.Id))
        {
            equipData = new EquipSaveData
            {
                id = equipItem.Id,
                baseId = equipItem.Base.idName,
                ilvl = (byte)equipItem.ItemLevel
            };
            equipDict[equipItem.Id] = equipData;
            equipList.Add(equipData);
        }
        else
            equipData = equipDict[equipItem.Id];

        equipData.name = equipItem.Name;
        equipData.rarity = equipItem.Rarity;
        if (equipItem.Rarity == RarityType.Unique)
        {
            UniqueBase uniqueBase = equipItem.Base as UniqueBase;
            equipData.uniqueVersion = (byte)uniqueBase.uniqueVersion;
        }
        equipData.prefixes.Clear();
        foreach (Affix affix in equipItem.prefixes)
        {
            equipData.prefixes.Add(new AffixSaveData(affix.Base.idName, affix.GetAffixValues(), affix.GetEffectValues(), affix.IsCrafted, affix.IsLocked));
        }

        equipData.suffixes.Clear();
        foreach (Affix affix in equipItem.suffixes)
        {
            equipData.suffixes.Add(new AffixSaveData(affix.Base.idName, affix.GetAffixValues(), affix.GetEffectValues(), affix.IsCrafted, affix.IsLocked));
        }
    }

    public void RemoveEquipmentData(Equipment equipment)
    {
        if (equipDict.ContainsKey(equipment.Id))
        {
            EquipSaveData temp = equipDict[equipment.Id];
            equipDict.Remove(equipment.Id);
            equipList.Remove(temp);
        }
    }

    public void LoadEquipmentData()
    {
        if (equipDict == null)
            equipDict = new Dictionary<Guid, EquipSaveData>();
        else
            equipDict.Clear();

        GameManager.Instance.PlayerStats.ClearEquipmentInventory();
        foreach (EquipSaveData equipData in equipList)
        {
            Equipment equipment = null;
            if (equipData.rarity == RarityType.Unique)
                equipment = Equipment.CreateUniqueFromBase(equipData.baseId, equipData.ilvl, equipData.uniqueVersion);
            else
                equipment = Equipment.CreateEquipmentFromBase(equipData.baseId, equipData.ilvl);

            if (equipment == null)
                continue;

            GameManager.Instance.PlayerStats.AddEquipmentToInventory(equipment);

            equipment.SetId(equipData.id);
            equipment.Name = equipData.name;
            equipment.SetRarity(equipData.rarity);

            if (equipData.rarity == RarityType.Unique)
            {
                UniqueBase uniqueBase = equipment.Base as UniqueBase;
                if (uniqueBase.uniqueVersion == equipData.uniqueVersion)
                {
                    for (int i = 0; i < uniqueBase.fixedUniqueAffixes.Count; i++)
                    {
                        AffixBase affixBase = uniqueBase.fixedUniqueAffixes[i];
                        equipment.prefixes.Add(new Affix(affixBase, equipData.prefixes[i].affixValues, equipData.prefixes[i].triggerValues, false, false));
                    }
                }
            }
            else
                LoadEquipmentAffixes(equipData, equipment);

            equipment.UpdateItemStats();

            equipDict.Add(equipData.id, equipData);
        }
    }

    private static void LoadEquipmentAffixes(EquipSaveData equipData, Equipment equipment)
    {
        foreach (AffixSaveData affixData in equipData.prefixes)
        {
            Affix newAffix = new Affix(ResourceManager.Instance.GetAffixBase(affixData.baseId, AffixType.Prefix), affixData.affixValues, affixData.triggerValues, affixData.isCrafted, affixData.isLocked);
            if (newAffix != null)
                equipment.prefixes.Add(newAffix);
        }
        foreach (AffixSaveData affixData in equipData.suffixes)
        {
            Affix newAffix = new Affix(ResourceManager.Instance.GetAffixBase(affixData.baseId, AffixType.Suffix), affixData.affixValues, affixData.triggerValues, affixData.isCrafted, affixData.isLocked);
            if (newAffix != null)
                equipment.suffixes.Add(newAffix);
        }
    }

    public void SaveAllHero()
    {
        heroDict.Clear();
        heroList.Clear();
        foreach (Hero hero in GameManager.Instance.PlayerStats.HeroList)
        {
            SaveHero(hero);
        }
    }

    public void RemoveHero(Hero hero)
    {
        if (heroDict.ContainsKey(hero.Id))
        {
            HeroSaveData temp = heroDict[hero.Id];
            heroDict.Remove(hero.Id);
            heroList.Remove(temp);
        }
    }

    public void SaveHero(Hero hero)
    {
        HeroSaveData heroSaveData;
        if (!heroDict.ContainsKey(hero.Id))
        {
            heroSaveData = new HeroSaveData
            {
                id = hero.Id
            };
            heroDict[hero.Id] = heroSaveData;
            heroList.Add(heroSaveData);
        }
        else
            heroSaveData = heroDict[hero.Id];

        heroSaveData.name = hero.Name;
        heroSaveData.level = hero.Level;
        heroSaveData.experience = hero.Experience;
        heroSaveData.baseHealth = hero.Stats.BaseHealth;
        heroSaveData.baseMana = hero.Stats.BaseMana;
        heroSaveData.baseStrength = hero.Stats.Attributes.BaseStrength;
        heroSaveData.baseIntelligence = hero.Stats.Attributes.BaseIntelligence;
        heroSaveData.baseDexterity = hero.Stats.Attributes.BaseDexterity;
        heroSaveData.baseWill = hero.Stats.Attributes.BaseWill;
        heroSaveData.killCount = hero.killCount;
        heroSaveData.spriteName = hero.spriteName;

        foreach (EquipSlotType equipSlot in Enum.GetValues(typeof(EquipSlotType)))
        {
            if (equipSlot == EquipSlotType.Ring)
                continue;

            if (hero.EquipmentData.GetEquipmentInSlot(equipSlot) != null)
            {
                heroSaveData.equipList[(int)equipSlot] = hero.EquipmentData.GetEquipmentInSlot(equipSlot).Id;
            }
            else
            {
                heroSaveData.equipList[(int)equipSlot] = Guid.Empty;
            }
        }

        if (hero.PrimaryArchetype != null)
        {
            HeroArchetypeData archetypeData = hero.PrimaryArchetype;
            HeroArchetypeSaveData archetypeSaveData = new HeroArchetypeSaveData()
            {
                id = archetypeData.Id,
                archetypeId = archetypeData.Base.idName
            };
            archetypeSaveData.nodeLevelData = new List<HeroArchetypeSaveData.NodeLevelSaveData>();
            foreach (KeyValuePair<int, int> pair in archetypeData.GetNodeLevels)
            {
                archetypeSaveData.nodeLevelData.Add(new HeroArchetypeSaveData.NodeLevelSaveData(pair.Key, pair.Value));
            }
            heroSaveData.primaryArchetypeData = archetypeSaveData;
        }

        if (hero.SecondaryArchetype != null)
        {
            HeroArchetypeData archetypeData = hero.SecondaryArchetype;
            HeroArchetypeSaveData archetypeSaveData = new HeroArchetypeSaveData()
            {
                id = archetypeData.Id,
                archetypeId = archetypeData.Base.idName
            };
            archetypeSaveData.nodeLevelData = new List<HeroArchetypeSaveData.NodeLevelSaveData>();
            foreach (KeyValuePair<int, int> pair in archetypeData.GetNodeLevels)
            {
                archetypeSaveData.nodeLevelData.Add(new HeroArchetypeSaveData.NodeLevelSaveData(pair.Key, pair.Value));
            }
            heroSaveData.secondaryArchetypeData = archetypeSaveData;
        }
    }

    public void LoadHero()
    {
        GameManager.Instance.PlayerStats.ClearHeroList();

        if (heroDict == null)
            heroDict = new Dictionary<Guid, HeroSaveData>();
        else
            heroDict.Clear();

        foreach (HeroSaveData heroSaveData in heroList)
        {
            GameManager.Instance.PlayerStats.AddHeroToList(new Hero(heroSaveData), false);
            heroDict.Add(heroSaveData.id, heroSaveData);
        }
    }

    [Serializable]
    public class HeroSaveData
    {
        public Guid id;
        public string spriteName;
        public string name;
        public int level;
        public int experience;
        public float baseHealth;
        public float baseMana;
        public float baseStrength;
        public float baseIntelligence;
        public float baseDexterity;
        public float baseVitality;
        public float baseWill;
        public float baseSpeed;
        public int killCount;

        public Guid[] equipList = new Guid[10];

        public HeroArchetypeSaveData primaryArchetypeData;
        public HeroArchetypeSaveData secondaryArchetypeData;
    }

    [Serializable]
    public class HeroArchetypeSaveData
    {
        public Guid id;
        public string archetypeId;
        public List<NodeLevelSaveData> nodeLevelData = new List<NodeLevelSaveData>();

        [Serializable]
        public class NodeLevelSaveData
        {
            public int nodeId;
            public int level;

            public NodeLevelSaveData(int nodeId, int level)
            {
                this.nodeId = nodeId;
                this.level = level;
            }
        }
    }

    [Serializable]
    private class EquipSaveData
    {
        public Guid id;
        public string name;
        public string baseId;
        public byte ilvl;
        public byte uniqueVersion;
        public RarityType rarity;
        public List<AffixSaveData> prefixes = new List<AffixSaveData>();
        public List<AffixSaveData> suffixes = new List<AffixSaveData>();
    }

    [Serializable]
    private class AffixSaveData
    {
        public string baseId;
        public bool isCrafted;
        public bool isLocked;
        public List<float> affixValues = new List<float>();
        public List<float> triggerValues = new List<float>();

        public AffixSaveData(string id, IList<float> values, IList<float> triggervalues, bool isCrafted, bool isLocked)
        {
            baseId = id;
            affixValues = values.ToList();
            this.isCrafted = isCrafted;
            this.isLocked = isLocked;
            if (triggervalues != null)
                triggerValues = triggervalues.ToList();
        }
    }

    [Serializable]
    private class AbilityCoreSaveData
    {
        public Guid id;
        public string baseId;
        public string name;

        public AbilityCoreSaveData(Guid id, string baseId, string name)
        {
            this.id = id;
            this.baseId = baseId ?? throw new ArgumentNullException(nameof(baseId));
            this.name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }

    [Serializable]
    private class ArchetypeItemSaveData
    {
        public Guid id;
        public string baseId;

        public ArchetypeItemSaveData(Guid id, string baseId)
        {
            this.id = id;
            this.baseId = baseId ?? throw new ArgumentNullException(nameof(baseId));
        }
    }

    [Serializable]
    private class ConsumablesContainer
    {
        public ConsumableType consumable;
        public int value;

        public ConsumablesContainer(ConsumableType type, int quantity)
        {
            consumable = type;
            value = quantity;
        }
    }

    [Serializable]
    private class TeamFormationSaveData
    {
        public List<Guid> frontLine = new List<Guid>();
        public List<Guid> backLine = new List<Guid>();

        public TeamFormationSaveData(TeamFormation teamFormation)
        {
            foreach (var x in teamFormation.frontLine)
            {
                frontLine.Add(x.Id);
            }
            foreach (var x in teamFormation.backLine)
            {
                backLine.Add(x.Id);
            }
        }
    }
}