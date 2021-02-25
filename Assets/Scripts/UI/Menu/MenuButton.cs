using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButton : LocalizedButton
{
    public void OnClickInvButton()
    {
        MenuUIManager.Instance.OpenInventoryMenu();
    }

    public void OnClickEquipmentCategory()
    {
        MenuUIManager.Instance.ShowInventory();
        MenuUIManager.Instance.Inventory.ShowEquipment(null, MenuUIManager.Instance.ShowItemDetailWindow);
    }

    public void OnClickHeroButton()
    {
        MenuUIManager.Instance.OpenHeroList();
    }

    public void OnClickArchetypeCategory()
    {

    }

    public void OnClickTeamButton()
    {
        MenuUIManager.Instance.OpenTeamFormationWindow();
    }
}
