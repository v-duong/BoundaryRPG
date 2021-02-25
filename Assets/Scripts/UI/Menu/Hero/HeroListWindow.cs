using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HeroListWindow : MonoBehaviour
{
    [SerializeField]
    private HeroSlot SlotPrefab;
    [SerializeField]
    private GameObject ScrollParent;

    private readonly List<HeroSlot> SlotsInUse = new List<HeroSlot>();
    private readonly Queue<HeroSlot> AvailableSlots = new Queue<HeroSlot>();

    private void Start()
    {
        SetGridCellSize();
    }

    private void SetGridCellSize()
    {
        GridLayoutGroup grid = ScrollParent.GetComponent<GridLayoutGroup>();
        float ySize = 150;
        if (GameManager.Instance.aspectRatio >= 1.92)
        {
            grid.cellSize = new Vector2(180, ySize);
        }
        else if (GameManager.Instance.aspectRatio >= 1.85)
        {
            grid.cellSize = new Vector2(200, ySize);
        }
        else
        {
            grid.cellSize = new Vector2(230, ySize);
        }
    }

    public void InitializeHeroSlots(Func<Hero, bool> filter = null)
    {
        if (filter == null)
            filter = x => true;

        foreach (HeroSlot slot in SlotsInUse)
        {
            slot.gameObject.SetActive(false);
            AvailableSlots.Enqueue(slot);
        }
        SlotsInUse.Clear();
        foreach (Hero hero in GameManager.Instance.PlayerStats.HeroList.Where(filter))
        {
            AddHeroSlot(hero);
        }
    }

    public void AddHeroSlot(Hero hero)
    {
        HeroSlot slot;
        if (AvailableSlots.Count > 0)
        {
            slot = AvailableSlots.Dequeue();
        }
        else
        {
            slot = Instantiate(SlotPrefab, ScrollParent.transform);
        }
        slot.gameObject.SetActive(true);
        SlotsInUse.Add(slot);
        slot.callback = null;
        slot.SetSlot(hero);
    }
}