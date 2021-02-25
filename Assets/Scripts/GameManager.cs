using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public PlayerStats PlayerStats { get; private set; }

    public float aspectRatio;
    public int selectedTeamNum;
    public bool isInBattle;
    public bool isInMainMenu;
    public bool heroAPIsDirty = true;

    private string currentSceneName = "";
    private Coroutine currentCoroutine;

    private WeightList<ConsumableType> consumableWeightList;

    public void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void Start()
    {
        aspectRatio = (float)Screen.height / (float)Screen.width;
        QualitySettings.vSyncCount = 1;

        currentSceneName = "mainMenu";

        StartCoroutine(StartRoutine());

        isInBattle = false;
        isInMainMenu = true;

        
        for (int i = 0; i < 50; i++)
        {
            Equipment equipment = Equipment.CreateRandomEquipment(1);
            equipment.SetRarity((RarityType)Random.Range(0, 4));
            equipment.RerollAffixesAtRarity();
            PlayerStats.AddEquipmentToInventory(equipment);
        }
        
#if UNITY_EDITOR
        CheckForBonuses();
#endif
    }

    private IEnumerator StartRoutine()
    {
        PlayerStats = new PlayerStats();
        if (!SaveManager.Load())
        {
            AddStartingData();
            SaveManager.SaveAll();
        }

#if !UNITY_EDITOR
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("mainMenu", LoadSceneMode.Additive);
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
#endif

        if (!PlayerStats.hasSeenStartingMessage)
            OpenTutorialMessage();

        yield break;
    }

    public void InitializePlayerStats()
    {
        PlayerStats = new PlayerStats();
        AddStartingData();
        SaveManager.SaveAll();
        StartCoroutine(ReloadMainMenu());
    }

    private IEnumerator ReloadMainMenu()
    {
        AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync("mainMenu");
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
        SceneManager.LoadScene("mainMenu", LoadSceneMode.Additive);
        yield return null;
        if (!PlayerStats.hasSeenStartingMessage)
            OpenTutorialMessage();
    }

    private void AddStartingData()
    {
        Equipment startingSword = Equipment.CreateEquipmentFromBase(ResourceManager.Instance.GetEquipmentBase("OneHandedSword1"), 1);
        Equipment startingBow = Equipment.CreateEquipmentFromBase(ResourceManager.Instance.GetEquipmentBase("Bow1"), 1);
        Equipment startingWand = Equipment.CreateEquipmentFromBase(ResourceManager.Instance.GetEquipmentBase("Wand1"), 1);

        startingSword.SetRarity(RarityType.Uncommon);
        startingSword.AddAffix(ResourceManager.Instance.GetAffixBase("LocalPhysicalDamageAdditive1", AffixType.Prefix));

        startingBow.SetRarity(RarityType.Uncommon);
        startingBow.AddAffix(ResourceManager.Instance.GetAffixBase("LocalPhysicalDamageAdditive1", AffixType.Prefix));

        startingWand.SetRarity(RarityType.Uncommon);
        startingWand.AddAffix(ResourceManager.Instance.GetAffixBase("MagicDamage1", AffixType.Prefix));

        PlayerStats.AddEquipmentToInventory(startingSword);
        PlayerStats.AddEquipmentToInventory(startingBow);
        PlayerStats.AddEquipmentToInventory(startingWand);

        Hero startingSoldier = Hero.CreateNewHero("Warrior", ResourceManager.Instance.GetArchetypeBase("Warrior"), ResourceManager.Instance.GetArchetypeBase("Novice"));
        Hero startingRanger = Hero.CreateNewHero("Ranger", ResourceManager.Instance.GetArchetypeBase("Ranger"), ResourceManager.Instance.GetArchetypeBase("Novice"));
        Hero startingMage = Hero.CreateNewHero("Magician", ResourceManager.Instance.GetArchetypeBase("Magician"), ResourceManager.Instance.GetArchetypeBase("Novice"));

        startingSoldier.EquipmentData.EquipToSlot(startingSword, EquipSlotType.Weapon);
        startingRanger.EquipmentData.EquipToSlot(startingBow, EquipSlotType.Weapon);
        startingMage.EquipmentData.EquipToSlot(startingWand, EquipSlotType.Weapon);

        PlayerStats.AddHeroToList(startingSoldier);
        PlayerStats.AddHeroToList(startingRanger);
        PlayerStats.AddHeroToList(startingMage);

        PlayerStats.AddHeroToTeam(startingSoldier, 0, BattlePosition.Front);
        PlayerStats.AddHeroToTeam(startingRanger, 0, BattlePosition.Back);
        PlayerStats.AddHeroToTeam(startingMage, 0, BattlePosition.Back);
        PlayerStats.hasSeenStartingMessage = false;
    }

    private void OpenTutorialMessage()
    {
        /*
        PopUpWindow popUpWindow = UIManager.Instance.PopUpWindow;
        popUpWindow.OpenTextWindow("");
        popUpWindow.SetButtonValues(null, null, "Close", delegate
        {
            UIManager.Instance.CloseCurrentWindow();
            PlayerStats.hasSeenStartingMessage = true;
            SaveManager.CurrentSave.SavePlayerData();
            SaveManager.Save();
        });
        popUpWindow.textField.text = "You should start by assigning each hero their starting ability and weapons. You'll find them by tapping the Heroes button.\n\nIn the corner of some windows, there is a question mark you can tap to bring up a help page.\n\nThis game is still under development and only up to World 3 has been added.";
        popUpWindow.textField.fontSize = 18;
        popUpWindow.textField.paragraphSpacing = 8;
        popUpWindow.textField.alignment = TextAlignmentOptions.Left;
        */
    }

    private void CheckForBonuses()
    {
        if (ResourceManager.Instance.ArchetypeBasesList != null)
        foreach (ArchetypeBase archetypeBase in ResourceManager.Instance.ArchetypeBasesList)
        {
            /*
            PlayerStats.AddArchetypeToInventory(ArchetypeItem.CreateArchetypeItem(archetypeBase, 100));
            foreach (var ability in archetypeBase.GetArchetypeAbilities(false))
            {
                PlayerStats.AddAbilityToInventory(AbilityCoreItem.CreateAbilityItemFromArchetype(archetypeBase, ability));
            }
            */
            foreach (var x in archetypeBase.nodeList)
            {
                foreach (var y in x.bonuses)
                {
                    LocalizationManager.Instance.GetBonusTypeString(y.bonusType);
                    if (y.restriction != TagType.DefaultTag)
                        LocalizationManager.Instance.GetLocalizationText_TagTypeRestriction(y.restriction);
                }
            }
        }
    }

    public ConsumableType GetRandomConsumable()
    {
        if (consumableWeightList == null)
        {
            consumableWeightList = new WeightList<ConsumableType>();
            consumableWeightList.Add(ConsumableType.AFFIX_REROLLER, 3000);
            consumableWeightList.Add(ConsumableType.AFFIX_CRAFTER, 900);
        }
        return consumableWeightList.ReturnWeightedRandom();
    }

    public void AddRandomConsumableToInventory()
    {
        PlayerStats.consumables[GetRandomConsumable()] += 1;
    }


    public static void SetTimescale(float value)
    {
        Time.timeScale = value;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }
}