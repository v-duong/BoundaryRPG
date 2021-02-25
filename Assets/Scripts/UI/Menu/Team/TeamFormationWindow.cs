using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamFormationWindow : MonoBehaviour
{
    private TeamSlot currentTeamSlot;
    private HeroSlot currentHeroSlot;
    private BattlePosition currentLineSelection;
    private int currentTeam = 0;

    private readonly List<HeroSlot> SlotsInUse = new List<HeroSlot>();
    private readonly Queue<HeroSlot> AvailableSlots = new Queue<HeroSlot>();

    [SerializeField]
    private HeroSlot miniHeroSlotPrefab;

    [SerializeField]
    private List<TeamSlot> frontLineSlots;

    [SerializeField]
    private List<TeamSlot> backLineSlots;

    [SerializeField]
    private GameObject scrollParent;

    private void OnEnable()
    {
        InitializeHeroList();
        InitializeTeamSlots();
    }

    private void OnDisable()
    {
        SaveManager.CurrentSave.SavePlayerData();
        SaveManager.Save();
    }

    private void InitializeTeamSlots()
    {
        var heroTeam = GameManager.Instance.PlayerStats.heroTeams[currentTeam];
        currentHeroSlot = null;
        currentTeamSlot = null;
        for (int i = 0; i < frontLineSlots.Count; i++)
        {
            frontLineSlots[i].ResetSlot();
            frontLineSlots[i].GetComponent<Outline>().enabled = false;
            if (heroTeam.frontLine.Count > i)
            {
                frontLineSlots[i].SetHero((Hero)heroTeam.frontLine[i]);
            }
        }
        for (int i = 0; i < backLineSlots.Count; i++)
        {
            backLineSlots[i].ResetSlot();
            backLineSlots[i].GetComponent<Outline>().enabled = false;
            if (heroTeam.backLine.Count > i)
            {
                backLineSlots[i].SetHero((Hero)heroTeam.backLine[i]);
            }
        }
    }

    public void InitializeHeroList()
    {
        foreach (HeroSlot slot in SlotsInUse)
        {
            slot.gameObject.SetActive(false);
            AvailableSlots.Enqueue(slot);
        }
        SlotsInUse.Clear();
        foreach (Hero hero in GameManager.Instance.PlayerStats.HeroList)
        {
            AddHeroSlot(hero);
        }
    }

    private void AddHeroSlot(Hero hero)
    {
        HeroSlot slot;
        if (AvailableSlots.Count > 0)
        {
            slot = AvailableSlots.Dequeue();
        }
        else
        {
            slot = Instantiate(miniHeroSlotPrefab, scrollParent.transform);
        }
        slot.gameObject.SetActive(true);
        SlotsInUse.Add(slot);
        slot.SetMiniSlot(hero);
        slot.GetComponent<Button>().onClick.AddListener(() => OnClickHeroSlot(slot));
    }

    public void OnClickFrontLine(TeamSlot slot)
    {
        currentLineSelection = BattlePosition.Front;
        SetCurrentTeamSlot(slot);
    }

    public void OnClickBackLine(TeamSlot slot)
    {
        currentLineSelection = BattlePosition.Back;
        SetCurrentTeamSlot(slot);
    }

    private void SetCurrentTeamSlot(TeamSlot slot)
    {
        if (currentTeamSlot != slot)
        {
            if (currentTeamSlot != null)
                currentTeamSlot.GetComponent<Outline>().enabled = false;
            slot.GetComponent<Outline>().enabled = true;
            currentTeamSlot = slot;
        }
        else
        {
            currentTeamSlot.GetComponent<Outline>().enabled = false;
            currentTeamSlot = null;
        }
        CheckIfSelectionMade();
    }

    public void OnClickHeroSlot(HeroSlot heroSlot)
    {
        if (currentHeroSlot != heroSlot)
        {
            if (currentHeroSlot != null)
                currentHeroSlot.GetComponent<Outline>().enabled = false;
            heroSlot.GetComponent<Outline>().enabled = true;
            currentHeroSlot = heroSlot;
        }
        else
        {
            currentHeroSlot.GetComponent<Outline>().enabled = false;
            currentHeroSlot = null;
        }
        CheckIfSelectionMade();
    }

    public void RemoveHeroButton(TeamSlot slot)
    {
        GameManager.Instance.PlayerStats.RemoveHeroFromTeam(slot.hero);
        FindAndUpdateHeroSlot(slot.hero);
        slot.ResetSlot();
    }

    private void FindAndUpdateHeroSlot(Hero hero)
    {
        foreach (var slot in SlotsInUse)
        {
            if (slot.hero == hero)
                slot.SetMiniSlot(hero);
        }
    }

    private void CheckIfSelectionMade()
    {
        if (currentHeroSlot != null && currentTeamSlot != null)
        {
            currentHeroSlot.GetComponent<Outline>().enabled = false;
            currentTeamSlot.GetComponent<Outline>().enabled = false;
            currentTeamSlot = null;
            Hero hero = currentHeroSlot.hero;
            if (hero.assignedTeam != -1)
            {
                bool mustRefresh = hero.assignedTeam == currentTeam;
                GameManager.Instance.PlayerStats.RemoveHeroFromTeam(hero);
                if (mustRefresh)
                    InitializeTeamSlots();
            }

            List<TeamSlot> slotLine;

            switch (currentLineSelection)
            {
                case BattlePosition.Front:
                    slotLine = frontLineSlots;
                    break;

                case BattlePosition.Back:
                    slotLine = backLineSlots;
                    break;

                default:
                    slotLine = frontLineSlots;
                    break;
            }

            foreach (TeamSlot slot in slotLine)
            {
                if (slot.hero == null)
                {
                    slot.SetHero(hero);
                    GameManager.Instance.PlayerStats.AddHeroToTeam(hero, currentTeam, currentLineSelection);
                    break;
                }
            }
            FindAndUpdateHeroSlot(hero);
            currentHeroSlot = null;
        }
    }
}