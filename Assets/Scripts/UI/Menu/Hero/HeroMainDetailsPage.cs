using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroMainDetailsPage : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI levelText;

    [SerializeField]
    private TextMeshProUGUI expText;

    [SerializeField]
    private TextMeshProUGUI apText;

    [SerializeField]
    private HeroArchetypeHeader primaryArchetypeHeader;

    [SerializeField]
    private HeroArchetypeHeader secondaryArchetypeHeader;

    [SerializeField]
    private Image xpBarFill;

    [SerializeField]
    private HeroStatBox healthBox;

    [SerializeField]
    private HeroStatBox manaBox;

    [SerializeField]
    private List<HeroStatBox> attributeBoxes;

    [SerializeField]
    private List<HeroStatBox> defenseBoxes;

    [SerializeField]
    private List<HeroStatBox> resistanceBoxes;

    [SerializeField]
    private List<HeroStatBox> shieldBoxes;

    [SerializeField]
    private GameObject shieldBoxParent;

    [SerializeField]
    private List<HeroStatBox> approxDefensesBoxes;

    private Hero hero;

    public void OnEnable()
    {
        if (hero != null)
            UpdateWindow();
    }

    public void Update()
    {
        if (hero != null)
        {
            if (hero.ArchetypePoints > 0)
            {
                primaryArchetypeHeader.lowerBannerImage.color = new Color(0, 0, 0, Mathf.PingPong(Time.time / 2, 0.5f));
                secondaryArchetypeHeader.lowerBannerImage.color = new Color(0, 0, 0, Mathf.PingPong(Time.time / 2, 0.5f));
            }
            else
            {
                primaryArchetypeHeader.lowerBannerImage.color = new Color(0, 0, 0, 0.3f);
                secondaryArchetypeHeader.lowerBannerImage.color = new Color(0, 0, 0, 0.3f);
            }
        }
    }

    public void ShowPage(Hero hero)
    {
        this.hero = hero;
        gameObject.SetActive(true);
    }

    private void UpdateWindow()
    {
        primaryArchetypeHeader.SetArchetype(hero.PrimaryArchetype);
        if (hero.SecondaryArchetype != null)
        {
            secondaryArchetypeHeader.gameObject.SetActive(true);
            secondaryArchetypeHeader.SetArchetype(hero.SecondaryArchetype);
        }
        else
        {
            secondaryArchetypeHeader.gameObject.SetActive(false);
        }

        levelText.text = "Level <b>" + hero.Level + "</b>";
        float requiredExp = Helpers.GetRequiredExperience(hero.Level + 1);
        float currentLevelExp = Helpers.GetRequiredExperience(hero.Level);
        expText.text = "Exp: " + hero.Experience.ToString("N0");
        if (hero.Level < 100)
        {
            expText.text += "/" + requiredExp.ToString("N0") + "\n";
            xpBarFill.fillAmount = (hero.Experience - currentLevelExp) / (requiredExp - currentLevelExp);
        }
        else
        {
            expText.text += " (MAX)";
            xpBarFill.fillAmount = 1f;
        }

        apText.text = "AP <b>" + hero.ArchetypePoints + "</b>";

        healthBox.statText.text = hero.Stats.MaximumHealth.ToString("N0");
        manaBox.statText.text = hero.Stats.MaximumMana.ToString("N0");

        attributeBoxes[0].statText.text = hero.Stats.Attributes.Strength.ToString("N0");
        attributeBoxes[1].statText.text = hero.Stats.Attributes.Intelligence.ToString("N0");
        attributeBoxes[2].statText.text = hero.Stats.Attributes.Dexterity.ToString("N0");
        attributeBoxes[3].statText.text = hero.Stats.Attributes.Vitality.ToString("N0");
        attributeBoxes[4].statText.text = hero.Stats.Attributes.Will.ToString("N0");

        defenseBoxes[0].statText.text = hero.Stats.Armor.ToString("N0");
        defenseBoxes[1].statText.text = hero.Stats.MagicArmor.ToString("N0");
        defenseBoxes[2].statText.text = hero.Stats.DodgeRating.ToString("N0");

        foreach (ElementType element in Enum.GetValues(typeof(ElementType)))
        {
            float resistance = hero.Stats.GetResistance(element);
            float uncapResistance = hero.Stats.GetUncapResistance(element);

            resistanceBoxes[(int)element].statText.text = resistance.ToString() + "%";

            if (uncapResistance > resistance)
            {
                resistanceBoxes[(int)element].statText.text += " (" + uncapResistance + ")";
            }
        }
    }
}