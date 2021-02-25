using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroEquipmentPage : MonoBehaviour
{
    [SerializeField]
    private List<HeroEquipSlot> equipSlots;

    private HeroEquipSlot _offHandSlot;

    private HeroEquipSlot OffHandSlot
    {
        get
        {
            if (_offHandSlot == null)
            {
                _offHandSlot = equipSlots.Find(x => x.slotType == EquipSlotType.Offhand);
            }
            return _offHandSlot;
        }
    }

    private Hero hero;

    private void OnEnable()
    {
        UpdateWindow();
    }

    public void ShowPage(Hero hero)
    {
        this.hero = hero;
        this.gameObject.SetActive(true);
    }

    public void UpdateWindow()
    {
        bool skipOffhandUpdate = false;
        foreach (HeroEquipSlot slot in equipSlots)
        {

            if (slot.slotType == EquipSlotType.Offhand && skipOffhandUpdate)
                continue;

            Equipment equip = hero.EquipmentData.GetEquipmentInSlot(slot.slotType);

            slot.SetSlot(equip, hero);

            if (slot.slotType == EquipSlotType.Weapon)
            {
                if (equip == null
                    || !hero.EquipmentData.GetEquipmentTagTypes(equip).Contains(TagType.TwoHandedWeapon)
                    || hero.Stats.HasSpecialBonus(BonusStatType.TwoHandedWeaponsAreOneHanded)
                    || (hero.Stats.HasSpecialBonus(BonusStatType.CanUseSpearWithShield) && equip.GetTagTypes().Contains(TagType.Spear)))
                {
                    OffHandSlot.buttonComponent.interactable = true;
                }
                else
                {
                    OffHandSlot.buttonComponent.interactable = false;
                    OffHandSlot.SetSlot(equip, hero);
                    skipOffhandUpdate = true;
                }
            }
        }
    }


}