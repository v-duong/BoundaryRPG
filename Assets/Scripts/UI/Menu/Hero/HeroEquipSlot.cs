using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroEquipSlot : MonoBehaviour
{
    [SerializeField]
    public EquipSlotType slotType;

    [SerializeField]
    private TextMeshProUGUI labelText;

    [SerializeField]
    private Image headerImage;

    [SerializeField]
    private TextMeshProUGUI headerText;

    [SerializeField]
    private List<TextMeshProUGUI> infoText;

    [SerializeField]
    private TextMeshProUGUI prefixText;

    [SerializeField]
    private TextMeshProUGUI suffixText;

    [SerializeField]
    private GameObject detailsParent;

    [SerializeField]
    private GameObject emptyText;

    [SerializeField]
    public Button buttonComponent;

    [SerializeField]
    public Button unequipButton;

    public Hero hero;

    public void ClearSlot()
    {
        headerText.text = "";
        foreach (var x in infoText)
        {
            x.text = "";
        }
        prefixText.text = "";
        suffixText.text = "";
        detailsParent.SetActive(false);
        emptyText.SetActive(true);
        unequipButton.gameObject.SetActive(false);
    }

    public void SetSlot(Equipment equipment, Hero hero)
    {
        ClearSlot();

        this.hero = hero;

        if (equipment != null)
        {
            unequipButton.gameObject.SetActive(true);
            unequipButton.interactable = buttonComponent.interactable;

            detailsParent.SetActive(true);
            emptyText.SetActive(false);

            headerImage.color = Helpers.ReturnRarityColor(equipment.Rarity);
            headerText.text = equipment.Base.LocalizedName;

            if (equipment is Armor armor)
            {
                int i = 0;
                if (armor.armor > 0)
                    infoText[i++].text = "AR: " + armor.armor.ToString();
                if (armor.magicArmor > 0)
                    infoText[i++].text = "MA: " + armor.magicArmor.ToString();
                if (armor.dodgeRating > 0)
                    infoText[i++].text = "DR: " + armor.dodgeRating.ToString();
            }
            else if (equipment is Weapon weapon)
            {
                int i = 0;
                infoText[i++].text = "pATK: " + weapon.PhysicalDamage.ToString();
            }

            foreach (var affix in equipment.prefixes)
            {
                prefixText.text += Affix.GetAffixShortString(affix.Base) + '\n';
            }
            foreach (var affix in equipment.suffixes)
            {
                suffixText.text += Affix.GetAffixShortString(affix.Base) + '\n';
            }
            prefixText.text = prefixText.text.Trim('\n');
            suffixText.text = suffixText.text.Trim('\n');
        }
    }

    public void OnClickEquipSlot()
    {
        if (slotType == EquipSlotType.RingSlot1 || slotType == EquipSlotType.RingSlot2)
            MenuUIManager.Instance.Inventory.ShowEquipmentForHero(hero, x => !x.IsEquipped && x.Base.equipSlot == EquipSlotType.Ring, ItemSlotCallback);
        else if (slotType == EquipSlotType.Offhand)
        {
            OffhandFilterResolver.SetHero(hero);
            MenuUIManager.Instance.Inventory.ShowEquipmentForHero(hero, OffhandFilterResolver.Filter, ItemSlotCallback);
        }
        else
            MenuUIManager.Instance.Inventory.ShowEquipmentForHero(hero, x => !x.IsEquipped && x.Base.equipSlot == slotType, ItemSlotCallback);

        MenuUIManager.Instance.ShowInventory();
    }

    private void ItemSlotCallback(Item item)
    {
        if (item == null)
            return;

        if (item is Equipment e)
        {
            MenuUIManager.Instance.ShowItemDetailWindow(e, EquipmentDetailCallback);
        }
    }

    private void EquipmentDetailCallback(Item item)
    {
        if (item is Equipment e)
        {
            hero.EquipmentData.EquipToSlot(e, slotType);
            // close windows because inventory and item detail page is still open
            MenuUIManager.Instance.CloseCurrentWindow();
            MenuUIManager.Instance.CloseCurrentWindow();
        }
    }

    public void UnequipButtonClick()
    {
        hero.EquipmentData.UnequipFromSlot(slotType);
        MenuUIManager.Instance.HeroDetailWindow.heroEquipmentPage.UpdateWindow();
    }
}