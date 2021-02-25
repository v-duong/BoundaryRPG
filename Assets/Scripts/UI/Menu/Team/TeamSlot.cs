using TMPro;
using UnityEngine;

public class TeamSlot : MonoBehaviour
{
    public Hero hero;

    [SerializeField]
    private TextMeshProUGUI nameText;

    [SerializeField]
    private TextMeshProUGUI primaryText;

    [SerializeField]
    private TextMeshProUGUI secondaryText;

    [SerializeField]
    private TextMeshProUGUI levelText;

    [SerializeField]
    private TextMeshProUGUI expText;

    [SerializeField]
    private TextMeshProUGUI hpText;

    [SerializeField]
    private TextMeshProUGUI mpText;

    [SerializeField]
    private GameObject textParent;

    [SerializeField]
    private GameObject emptyText;

    public void SetHero(Hero h)
    {
        textParent.SetActive(true);
        emptyText.SetActive(false);
        hero = h;
        secondaryText.text = "";

        nameText.text = hero.Name;
        primaryText.text = hero.PrimaryArchetype.Base.LocalizedName;
        if (hero.SecondaryArchetype != null)
        {
            secondaryText.text = hero.SecondaryArchetype.Base.LocalizedName;
        }
        levelText.text = "Level " + hero.Level;
        expText.text = "XP " + (hero.Experience / Helpers.GetRequiredExperience(hero.Level + 1)).ToString("P2");
        hpText.text = "HP " + hero.Stats.MaximumHealth;
        mpText.text = "MP " + hero.Stats.MaximumMana;
    }

    public void ResetSlot()
    { 
        hero = null;
        textParent.SetActive(false);
        emptyText.SetActive(true);
    }

    public void RemoveHeroFromSlot()
    {
        GameManager.Instance.PlayerStats.RemoveHeroFromTeam(hero);
    }
}