using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryView : MonoBehaviour
{
    [SerializeField]
    private InventorySlot InventorySlotPrefab;

    private List<InventorySlot> SlotsInUse = new List<InventorySlot>();
    private InventorySlotPool _slotPool;

    private InventorySlotPool SlotPool
    {
        get
        {
            if (_slotPool == null)
                _slotPool = new InventorySlotPool(InventorySlotPrefab, GameManager.Instance.PlayerStats.EquipmentInventory.Count);
            return _slotPool;
        }
    }

    private void ClearSlots()
    {
        foreach (InventorySlot i in SlotsInUse)
        {
            SlotPool.ReturnToPool(i);
        }
        SlotsInUse.Clear();
    }

    private void DeactivateSlotsInPool()
    {
        SlotPool.DeactivateObjectsInPool();
    }

    private bool DefaultCallback()
    {
        return true;
    }

    public void ShowEquipment(Func<Equipment, bool> filter = null, Action<Item> callback = null)
    {
        ClearSlots();

        if (filter == null)
            filter = x => true;

        foreach (Equipment equip in GameManager.Instance.PlayerStats.EquipmentInventory.Where(filter))
        {
            AddInventorySlot(equip, callback);
        }

        DeactivateSlotsInPool();
    }

    public void ShowEquipmentForHero(Hero hero, Func<Equipment, bool> filter = null, Action<Item> callback = null)
    {
        ClearSlots();

        if (filter == null)
            filter = x => true;

        foreach (Equipment equip in GameManager.Instance.PlayerStats.EquipmentInventory.Where(filter))
        {
            AddInventorySlot(equip, callback);
        }

        DeactivateSlotsInPool();
    }

    private void AddInventorySlot(Item item, Action<Item> callback)
    {
        InventorySlot slot = SlotPool.GetSlot(false);
        slot.gameObject.transform.SetParent(transform, false);
        slot.gameObject.transform.SetAsLastSibling();
        SlotsInUse.Add(slot);

        slot.SetItem(item, callback);
        slot.gameObject.SetActive(true);
    }

    public class InventorySlotPool : StackObjectPool<InventorySlot>
    {
        public InventorySlotPool(InventorySlot prefab, int i) : base(prefab, 50)
        {
        }

        public InventorySlot GetSlot(bool activeState)
        {
            return Get(activeState);
        }

        public override void ReturnToPool(InventorySlot item)
        {
            Return(item);
        }

        public void ReturnToPoolWhileActive(InventorySlot item)
        {
            ReturnWithoutDeactivate(item);
        }

        public void DeactivateObjectsInPool()
        {
            DeactivatePooledObjects();
        }
    }
}