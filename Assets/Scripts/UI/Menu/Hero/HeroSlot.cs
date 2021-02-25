using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroSlot : MonoBehaviour
{
    public Hero hero;
    public Image slotImage;
    [SerializeField]
    private TextMeshProUGUI nameText;
    [SerializeField]
    private TextMeshProUGUI levelText;
    [SerializeField]
    private TextMeshProUGUI primaryText;
    [SerializeField]
    private TextMeshProUGUI secondaryText;
    [SerializeField]
    private TextMeshProUGUI teamText;

    public Action<Hero> callback;

    public void SetSlot(Hero h)
    {
        this.hero = h;
        nameText.text = hero.Name;
        levelText.text = hero.Level.ToString();
        primaryText.text = hero.PrimaryArchetype.Base.LocalizedName;
        if (hero.SecondaryArchetype != null)
        {
            secondaryText.text = hero.SecondaryArchetype.Base.LocalizedName;
        }
        else
        {
            secondaryText.text = "";
        }
        if (hero.assignedTeam!=-1)
        {
            teamText.text = "Assigned to Team " + hero.assignedTeam;
        } else
        {
            teamText.text = "";
        }
        
    }

    public void SetMiniSlot(Hero h)
    {
        this.hero = h;
        nameText.text = hero.Name;
        levelText.text = "Level " + hero.Level;
        primaryText.text = hero.PrimaryArchetype.Base.LocalizedName;
        if (hero.SecondaryArchetype != null)
        {
            secondaryText.text = hero.SecondaryArchetype.Base.LocalizedName;
        } else
        {
            secondaryText.text = "";
        }
        if (hero.assignedTeam != -1)
        {
            teamText.text = "Assigned to Team " + hero.assignedTeam;
        }
        else
        {
            teamText.text = "";
        }
    }

    public void OnHeroSlotClick()
    {
        if (callback != null)
        {
            callback.Invoke(hero);
            return;
        } else
        {
            MenuUIManager.Instance.HeroDetailWindow.OpenWindow(hero);
        }
    }
}
