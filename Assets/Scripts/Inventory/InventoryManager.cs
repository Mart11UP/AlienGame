using System;
using System.Collections.Generic;
using System.Linq;
using Alien.Data;
using NaughtyAttributes;
using UnityEngine;

namespace Alien.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        private const string ItemDataResourcesPath = "ItemData";
        private const string InventoryDataGroupId = "Inventory";

        [Header("Runtime Data")]
        [SerializeField, ReadOnly] private List<InventoryEntry> entries = new();
        [SerializeField, ReadOnly] private ItemData[] allItemData;

        public IReadOnlyList<InventoryEntry> Entries => entries;
        public IReadOnlyList<ItemData> AllItemData => allItemData;

        public event Action OnInventoryChanged;
        public event Action<ItemData> ItemUsed;
        public event Action<ItemData> ItemEquipped;

        public static InventoryManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            LoadItemData();
            TryLoadInventoryData();
            ValidateInventoryEntries();
        }

        private void LoadItemData()
        {
            allItemData = Resources.LoadAll<ItemData>(ItemDataResourcesPath).OrderBy(item => item.Id).ToArray();

            ValidateLoadedItems();

            Debug.Log($"Loaded {allItemData.Length} inventory items from " + $"Resources/{ItemDataResourcesPath}.", this);
        }

        public bool RequestAddItem(ItemData itemData, int quantity = 1)
        {
            if (!ValidateRequest(itemData, quantity)) return false;

            int remainingQuantity = AddToExistingStacks(itemData, quantity);

            while (remainingQuantity > 0)
            {
                int amountToAdd = Mathf.Min(remainingQuantity, itemData.MaxStackSize);
                entries.Add(new InventoryEntry(itemData, amountToAdd, entries.Count));
                remainingQuantity -= amountToAdd;
            }

            TrySaveInventoryData();
            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool RequestRemoveItem(ItemData itemData, int quantity = 1)
        {
            if (!ValidateRequest(itemData, quantity)) return false;
            if (GetItemQuantity(itemData) < quantity) return false;

            int remainingQuantity = quantity;

            for (int i = entries.Count - 1; i >= 0 && remainingQuantity > 0; i--)
            {
                InventoryEntry entry = entries[i];

                if (entry.ItemData != itemData)
                    continue;

                int amountToRemove = Mathf.Min(entry.Quantity, remainingQuantity);

                entry.Quantity -= amountToRemove;
                remainingQuantity -= amountToRemove;

                if (entry.Quantity <= 0)
                    entries.RemoveAt(i);
            }

            CompleteInventoryModification();
            return true;
        }

        public bool RequestRemoveEntry(InventoryEntry entry, int quantity = -1)
        {
            if (entry == null) return false;

            int entryIndex = entries.IndexOf(entry);

            if (entryIndex < 0) return false;

            if (quantity < 0 || quantity >= entry.Quantity)
                entries.RemoveAt(entryIndex);
            else
                entry.Quantity -= quantity;

            CompleteInventoryModification();
            return true;
        }

        private void CompleteInventoryModification()
        {
            UpdateEntryIndexes();
            TrySaveInventoryData();
            OnInventoryChanged?.Invoke();
        }

        public bool RequestUseItem(InventoryEntry entry)
        {
            if (entry == null || entry.ItemData == null) return false;
            if (!entries.Contains(entry)) return false;

            ItemData itemData = entry.ItemData;

            switch (itemData.Usage)
            {
                case InventoryItemUsage.Consumable:
                    return UseConsumable(entry);

                case InventoryItemUsage.Equippable:
                    return EquipItem(entry);

                case InventoryItemUsage.None:
                    Debug.Log( $"{itemData.DisplayName} cannot be used from the inventory.", this);

                    return false;

                default:
                    return false;
            }
        }

        public bool ContainsItem(ItemData itemData, int quantity = 1)
        {
            if (itemData == null || quantity <= 0) return false;

            return GetItemQuantity(itemData) >= quantity;
        }

        public int GetItemQuantity(ItemData itemData)
        {
            if (itemData == null) return 0;

            int totalQuantity = 0;

            foreach (InventoryEntry entry in entries)
                if (entry.ItemData == itemData) totalQuantity += entry.Quantity;

            return totalQuantity;
        }

        public InventoryEntry GetFirstEntry(ItemData itemData)
        {
            if (itemData == null) return null;

            return entries.Find(entry => entry.ItemData == itemData);
        }

        public bool TryGetFirstEntry(ItemData itemData, out InventoryEntry entry)
        {
            entry = GetFirstEntry(itemData);
            return entry != null;
        }

        public ItemData GetItemDataById(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return null;

            foreach (ItemData itemData in allItemData)
                if (itemData.Id == itemId) return itemData;

            return null;
        }

        public bool TryGetItemDataById(string itemId, out ItemData itemData)
        {
            itemData = GetItemDataById(itemId);
            return itemData != null;
        }

        private bool UseConsumable(InventoryEntry entry)
        {
            ItemData itemData = entry.ItemData;

            if (!RequestRemoveEntry(entry, 1)) return false;

            ItemUsed?.Invoke(itemData);
            Debug.Log($"Used {itemData.DisplayName}.", this);

            return true;
        }

        private bool EquipItem(InventoryEntry entry)
        {
            ItemData itemData = entry.ItemData;

            ItemEquipped?.Invoke(itemData);
            Debug.Log($"Equipped {itemData.DisplayName}.", this);

            return true;
        }

        private int AddToExistingStacks(ItemData itemData, int quantity)
        {
            foreach (InventoryEntry entry in entries)
            {
                if (entry.ItemData != itemData) continue;
                if (entry.Quantity >= itemData.MaxStackSize) continue;

                int availableSpace = itemData.MaxStackSize - entry.Quantity;
                int amountToAdd = Mathf.Min(quantity, availableSpace);

                entry.Quantity += amountToAdd;
                quantity -= amountToAdd;

                if (quantity <= 0) break;
            }

            return quantity;
        }

        private void UpdateEntryIndexes()
        {
            for (int i = 0; i < entries.Count; i++)
                entries[i].index = i;
        }

        private bool ValidateRequest(ItemData itemData, int quantity)
        {
            if (itemData == null)
            {
                Debug.LogWarning("The requested ItemData is null.", this);
                return false;
            }

            if (!IsRegisteredItem(itemData))
            {
                Debug.LogWarning($"The item '{itemData.name}' was not found in " + $"Resources/{ItemDataResourcesPath}.", itemData);
                return false;
            }

            if (quantity <= 0)
            {
                Debug.LogWarning("The item quantity must be greater than zero.", this);
                return false;
            }

            return true;
        }

        private bool IsRegisteredItem(ItemData itemData)
        {
            if (itemData == null || allItemData == null) return false;

            return Array.IndexOf(allItemData, itemData) >= 0;
        }

        private void ValidateLoadedItems()
        {
            HashSet<string> registeredIds = new();

            foreach (ItemData itemData in allItemData)
            {
                if (itemData == null)
                {
                    Debug.LogError("A null ItemData asset was loaded.", this);
                    continue;
                }

                if (string.IsNullOrEmpty(itemData.Id))
                {
                    Debug.LogError($"ItemData asset '{itemData.name}' has no valid ID.", itemData);

                    continue;
                }

                if (registeredIds.Add(itemData.Id)) continue;

                Debug.LogError($"Multiple ItemData assets use ID '{itemData.Id}'. " + "Every item must have a unique ID.", itemData);
            }
        }

        private void ValidateInventoryEntries()
        {
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                InventoryEntry entry = entries[i];

                if (entry == null || entry.ItemData == null || entry.Quantity <= 0)
                {
                    entries.RemoveAt(i);
                    continue;
                }

                if (!IsRegisteredItem(entry.ItemData))
                {
                    Debug.LogWarning($"Removing inventory entry '{entry.ItemData.name}' " + "because its ItemData is not registered.", entry.ItemData);
                    entries.RemoveAt(i);
                }
            }

            UpdateEntryIndexes();
        }
        private bool TrySaveInventoryData()
        {
            List<InventoryEntrySaveData> saveData = new();

            foreach (InventoryEntry entry in entries)
                saveData.Add(new InventoryEntrySaveData(entry.ItemData.Id, entry.Quantity, entry.index));

            return PersistentDataManager.TrySaveGroup(InventoryDataGroupId, saveData);
        }

        private bool TryLoadInventoryData()
        {
            if (!PersistentDataManager.TryGetGroup(InventoryDataGroupId, out List<InventoryEntrySaveData> saveData)) return false;
            if (saveData == null) return false;

            entries.Clear();

            foreach (InventoryEntrySaveData entryData in saveData.OrderBy(entry => entry.Index))
            {
                ItemData itemData = GetItemDataById(entryData.ItemId);

                if (itemData == null)
                {
                    Debug.LogWarning($"The saved inventory item '{entryData.ItemId}' was not found.", this);
                    continue;
                }

                if (entryData.Quantity <= 0) continue;

                entries.Add(new InventoryEntry(itemData, entryData.Quantity, entries.Count));
            }

            return true;
        }

        [Button]
        public void RemoveAllInventoryData()
        {
            entries.Clear();
            PersistentDataManager.TryDeleteGroup(InventoryDataGroupId);
            OnInventoryChanged?.Invoke();
        }

        [Serializable]
        private class InventoryEntrySaveData
        {
            public string ItemId;
            public int Quantity;
            public int Index;

            public InventoryEntrySaveData(string itemId, int quantity, int index)
            {
                ItemId = itemId;
                Quantity = quantity;
                Index = index;
            }
        }
    }
}