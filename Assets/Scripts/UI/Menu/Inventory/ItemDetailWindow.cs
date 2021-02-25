using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailWindow : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI nameText;

    [SerializeField]
    private TextMeshProUGUI infoText;

    [SerializeField]
    private TextMeshProUGUI parameterText;

    [SerializeField]
    private GameObject innateHeader;

    [SerializeField]
    private GameObject prefixHeader;

    [SerializeField]
    private GameObject suffixHeader;

    [SerializeField]
    private RectTransform scrollParent;

    [SerializeField]
    private TextMeshProUGUI innateText;

    [SerializeField]
    private TextMeshProUGUI prefixText;

    [SerializeField]
    private TextMeshProUGUI suffixText;

    [SerializeField]
    private Image backgroundImage;

    [SerializeField]
    private Button confirmButton;

    private Item item;
    private Action<Item> callback;

    public void SetItem(Item item, Action<Item> callback = null)
    {
        this.item = item;
        this.callback = callback;
        if (callback != null)
        {
            confirmButton.gameObject.SetActive(true);
            scrollParent.offsetMin = new Vector2(scrollParent.offsetMin.x, 40);
        } else
        {
            confirmButton.gameObject.SetActive(false);
            scrollParent.offsetMin = new Vector2(scrollParent.offsetMin.x, 0);
        }
        UpdateWindow();
    }

    public void OnClick()
    {
        callback?.Invoke(item);
    }

    private void UpdateWindow()
    {
        backgroundImage.color = Helpers.ReturnRarityColor(item.Rarity);
        nameText.text = item.Name;
        infoText.text = "";
        parameterText.text = "";
        innateText.text = "";
        prefixText.text = "";
        suffixText.text = "";

        if (item is Equipment e)
        {
            UpdateAsEquipment(e);
        }
    }

    private void UpdateAsEquipment(Equipment equip)
    {
        if (equip.Rarity != RarityType.Unique)
        {
            infoText.text += equip.Base.LocalizedName + " ";
            infoText.text += "(Drop Level " + equip.Base.dropLevel + ")\n";
        }
        else
        {
            infoText.text += "Drop Level " + equip.Base.dropLevel + "\n";
        }

        string tagString = LocalizationManager.Instance.GetLocalizationText(equip.Base.groupTag);
        string equipTypeString = LocalizationManager.Instance.GetLocalizationText(equip.Base.equipSlot);

        if (tagString.Equals(equipTypeString))
        {
            infoText.text += tagString + '\n';
        }
        else
        {
            if (equip.Base.equipSlot == EquipSlotType.Weapon)
                equipTypeString = LocalizationManager.Instance.GetLocalizationText((equip as Weapon).GetWeaponRange());
            infoText.text += tagString + " (" + equipTypeString + ")\n";
        }

        infoText.text += "Item Level " + equip.ItemLevel + '\n';
        infoText.text += LocalizationManager.Instance.GetRequirementText(equip);

        if (equip.GetItemType() == ItemType.Weapon)
            UpdateWeaponParameters((Weapon)equip);
        else if (equip.GetItemType() == ItemType.Armor)
            UpdateArmorParameters((Armor)equip);

        UpdateAffixes(equip);
    }

    private void UpdateWeaponParameters(Weapon weapon)
    {
        parameterText.text += "Physical ATK\t\t<b>" + weapon.PhysicalDamage + "</b>\n";
        parameterText.text += "Damage Spread\t\t<b>" + weapon.DamageSpread + "%</b>\n";

        int lowerBound = (int)(weapon.PhysicalDamage * (1 - weapon.DamageSpread / 100f));
        string damageRange = lowerBound + " - " + weapon.PhysicalDamage;

        parameterText.text += "Est. Damage Range\t<b>" + damageRange + "</b>\n\n";
        parameterText.text += "Critical Chance\t\t<b>" + weapon.CriticalChance.ToString("N2") + "%</b>";
    }
    private void UpdateArmorParameters(Armor armor)
    {
        if (armor.armor >0)
        {
            parameterText.text += "Armor\t<b>" + armor.armor + "</b>\n";
        }
        if (armor.magicArmor > 0)
        {
            parameterText.text += "Magic Armor\t<b>" + armor.magicArmor + "</b>\n";
        }
        if (armor.dodgeRating > 0)
        {
            parameterText.text += "Dodge Rating\t<b>" + armor.dodgeRating + "</b>\n";
        }
        if (armor.GetTagTypes().Contains(TagType.Shield))
        {
            parameterText.text += "\nBlock Chance\t<b>" + armor.blockChance + "%</b>\n";
            parameterText.text += "Block Protection\t<b>" + armor.blockProtection + "%</b>\n";
        }
    }

    private void UpdateAffixes(Equipment equip)
    {
        if (equip.innate.Count > 0)
        {
            innateHeader.gameObject.SetActive(true);
            foreach (Affix a in equip.innate)
            {
                innateText.text += Affix.BuildAffixString(a.Base, Helpers.AFFIX_STRING_SPACING, a, a.GetAffixValues(), a.GetEffectValues());
            }
        }
        else
        {
            innateHeader.gameObject.SetActive(false);
        }

        if (equip.prefixes.Count > 0)
        {
            prefixHeader.gameObject.SetActive(true);
            if (equip.Rarity == RarityType.Unique)
                prefixHeader.GetComponentInChildren<TextMeshProUGUI>().text = "Affixes";
            else
                prefixHeader.GetComponentInChildren<TextMeshProUGUI>().text = "Prefixes";
            foreach (Affix a in equip.prefixes)
            {
                prefixText.text += Affix.BuildAffixString(a.Base, Helpers.AFFIX_STRING_SPACING, a, a.GetAffixValues(), a.GetEffectValues());
            }
        }
        else
        {
            prefixHeader.gameObject.SetActive(false);
        }

        if (equip.suffixes.Count > 0)
        {
            suffixHeader.gameObject.SetActive(true);
            foreach (Affix a in equip.suffixes)
            {
                suffixText.text += Affix.BuildAffixString(a.Base, Helpers.AFFIX_STRING_SPACING, a, a.GetAffixValues(), a.GetEffectValues());
            }
        }
        else
        {
            suffixHeader.gameObject.SetActive(false);
        }
    }
}