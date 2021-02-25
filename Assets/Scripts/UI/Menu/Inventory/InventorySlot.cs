using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Item item;
    public Action<Item> callback;

    [SerializeField]
    private TextMeshProUGUI nameText;

    [SerializeField]
    private TextMeshProUGUI baseNameText;

    [SerializeField]
    private TextMeshProUGUI iLvlText;

    [SerializeField]
    private TextMeshProUGUI dLvlText;

    [SerializeField]
    private List<TextMeshProUGUI> parameterTexts;

    [SerializeField]
    private TextMeshProUGUI parameter1Text;

    [SerializeField]
    private TextMeshProUGUI parameter2Text;

    [SerializeField]
    private TextMeshProUGUI parameter3Text;

    [SerializeField]
    private TextMeshProUGUI parameter4Text;

    [SerializeField]
    private TextMeshProUGUI parameter5Text;

    [SerializeField]
    private TextMeshProUGUI prefixText;

    [SerializeField]
    private TextMeshProUGUI suffixText;

    [SerializeField]
    private TextMeshProUGUI equippedText;

    [SerializeField]
    private Image slotImage;

    public void SetItem(Item item, Action<Item> callback)
    {
        this.item = item;
        this.callback = callback;
        UpdateSlot();
    }

    public void ClearSlot()
    {
        nameText.text = "Empty";
        baseNameText.text = "";
        iLvlText.text = "";
        dLvlText.text = "";
        parameter1Text.text = "";
        parameter2Text.text = "";
        parameter3Text.text = "";
        parameter4Text.text = "";
        parameter5Text.text = "";
        parameter2Text.gameObject.SetActive(false);
        parameter3Text.gameObject.SetActive(false);
        parameter4Text.gameObject.SetActive(false);
        parameter5Text.gameObject.SetActive(false);
        prefixText.text = "";
        suffixText.text = "";
        equippedText.text = "";
    }

    private void SetParameterText(int i, string s)
    {
        parameterTexts[i].text = s;
        parameterTexts[i].gameObject.SetActive(true);
    }

    public void OnItemSlotClick()
    {
        if (callback != null)
        {
            callback?.Invoke(item);
            return;
        }
    }

    public void UpdateSlot()
    {
        ClearSlot();
        nameText.text = item.Name;

        if (item is Equipment equipment)
        {
            slotImage.color = Helpers.ReturnRarityColor(equipment.Rarity);

            if (equipment.Rarity == RarityType.Rare || equipment.Rarity == RarityType.Epic)
            {
                baseNameText.gameObject.SetActive(true);
                baseNameText.text = equipment.Base.LocalizedName;
                iLvlText.text = "<size=7>itemLvl</size><br>" + equipment.ItemLevel;
                dLvlText.text = "<size=7>dropLvl</size><br>" + equipment.Base.dropLevel;
            }
            else
            {
                nameText.text = equipment.Base.LocalizedName;
                baseNameText.gameObject.SetActive(false);
                iLvlText.text = "Item Level: " + equipment.ItemLevel;
                dLvlText.text = "Drop Level: " + equipment.Base.dropLevel;
            }

            switch (item.GetItemType())
            {
                case ItemType.Armor:
                    int index = 0;
                    Armor armor = item as Armor;
                    if (armor.armor > 0)
                    {
                        SetParameterText(index, "AR: " + armor.armor);
                        index++;
                    }
                    if (armor.magicArmor > 0)
                    {
                        SetParameterText(index, "MA: " + armor.magicArmor);
                        index++;
                    }
                    if (armor.dodgeRating > 0)
                    {
                        SetParameterText(index, "DR: " + armor.dodgeRating);
                        index++;
                    }
                    if (armor.Base.groupTag == TagType.Shield)
                    {
                        SetParameterText(index, "BC: " + armor.blockChance + "%");
                        index++;
                        SetParameterText(index, "BP: " + armor.blockProtection + "%");
                    }
                    break;

                case ItemType.Weapon:
                    Weapon weapon = item as Weapon;
                    SetParameterText(0, "ATK: " + weapon.PhysicalDamage);
                    SetParameterText(1, "Crit: " + weapon.CriticalChance.ToString("N2") + "%");
                    break;

                case ItemType.Accessory:
                    break;

                default:
                    break;
            }

            foreach(Affix affix in equipment.prefixes)
            {
                prefixText.text += "T" + affix.Base.tier + " ";
                prefixText.text += Affix.GetAffixShortString(affix.Base) + '\n';
            }

            prefixText.text = prefixText.text.Trim('\n');

            foreach (Affix affix in equipment.suffixes)
            {
                suffixText.text += "T" + affix.Base.tier + " ";
                suffixText.text += Affix.GetAffixShortString(affix.Base) + '\n';
            }

            suffixText.text = suffixText.text.Trim('\n');

            if (equipment.IsEquipped && equipment.equippedToHero!=null)
            {
                equippedText.text = "Equipped to " + equipment.equippedToHero.Name;
            }
        }
    }
}