using System;
using UnityEngine;

public class MenuUIManager : UIManager
{
    public static MenuUIManager Instance { get; private set; }
    public InventoryView Inventory => InventoryScrollView;

    [SerializeField]
    private Canvas InventoryCanvas;

    [SerializeField]
    private InventoryView InventoryScrollView;

    [SerializeField]
    private GameObject InventoryWindowParent;

    [SerializeField]
    private GameObject InventoryCategoryMenu;

    [SerializeField]
    private ItemDetailWindow ItemDetailWindow;

    [SerializeField]
    public ArchetypeWindow ArchetypeWindow;

    [SerializeField]
    private HeroListWindow HeroListWindow;

    [SerializeField]
    public HeroDetailWindow HeroDetailWindow;

    [SerializeField]
    private TeamFormationWindow TeamFormationWindow;

    private void Awake()
    {
        Instance = this;
    }

    public void OpenInventoryMenu()
    {
        OpenWindow(InventoryCategoryMenu);
    }

    public void ShowInventory()
    {
        OpenWindow(InventoryWindowParent);
    }

    public void ShowItemDetailWindow(Item item)
    {
        ItemDetailWindow.SetItem(item);
        OpenWindow(ItemDetailWindow.gameObject, false);
    }

    public void ShowItemDetailWindow(Item item, Action<Item> callback)
    {
        ItemDetailWindow.SetItem(item, callback);
        OpenWindow(ItemDetailWindow.gameObject, false);
    }

    public void OpenHeroList(Func<Hero, bool> filter = null)
    {
        HeroListWindow.InitializeHeroSlots(filter);
        OpenWindow(HeroListWindow.gameObject);
    }

    public void OpenArchetypeWindow(ArchetypeBase archetypeBase, HeroArchetypeData heroArchetypeData = null)
    {
        OpenWindow(ArchetypeWindow.gameObject);
        ArchetypeWindow.BuildTree(archetypeBase, heroArchetypeData);
    }

    public void OpenTeamFormationWindow()
    {
        OpenWindow(TeamFormationWindow.gameObject);
    }
}