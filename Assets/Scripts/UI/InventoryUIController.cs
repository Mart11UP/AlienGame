using Alien.Inventory;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Alien.UI
{
    public class InventoryUIController : MonoBehaviour
    {
        [SerializeField] Transform container;
        [SerializeField] InventoryItemUI itemUIReference;

        [Header("Info Container")]
        [SerializeField] Image icon;
        [SerializeField] TMP_Text itemNameText;
        [SerializeField] TMP_Text descriptionText;
        [SerializeField] Button useButton;
        [SerializeField] TMP_Text useButtonText;
        [SerializeField] Button discardButton;

        InventoryItemUI selectedUIItem;
        List<InventoryItemUI> currentItemsUI = new();

        private void Awake()
        {
            itemUIReference.gameObject.SetActive(false);
            SetupItemsUI();
            SelectItem(currentItemsUI.FirstOrDefault(itemUI => itemUI.InventoryEntry != null));
            InventoryManager.Instance.OnInventoryChanged += SetupItemsUI;

            useButton.onClick.AddListener(RequestUseSelectedItem);
            discardButton.onClick.AddListener(RequestDiscardSelectedItem);
        }

        private void OnDestroy()
        {
            InventoryManager.Instance.OnInventoryChanged -= SetupItemsUI;

            useButton.onClick.RemoveListener(RequestUseSelectedItem);
            discardButton.onClick.RemoveListener(RequestDiscardSelectedItem);
        }

        void SetupItemsUI()
        {
            InventoryEntry lastSelectedEntry = selectedUIItem != null ? selectedUIItem.InventoryEntry : null;

            EnsureItemsUICount();
            IReadOnlyList<InventoryEntry> entries = InventoryManager.Instance.Entries;
            for (int i = 0; i < entries.Count; i++)
            {
                InventoryEntry entry = entries[i];
                InventoryItemUI itemUI = currentItemsUI[i];

                itemUI.Setup(entry);
            }

            TryGetSelectedUIItem(lastSelectedEntry, out selectedUIItem);
            SelectItem(selectedUIItem);
        }

        private bool TryGetSelectedUIItem(InventoryEntry entry, out InventoryItemUI inventoryItemUI)
        {
            if (entry == null)
            {
                inventoryItemUI = null;
                return false;
            }

            if(!InventoryManager.Instance.Entries.Contains(entry))
            {
                inventoryItemUI = null;
                return false;
            }

            foreach (InventoryItemUI itemUI in currentItemsUI)
                if (itemUI.InventoryEntry == entry)
                {
                    inventoryItemUI = itemUI;
                    return true;
                }

            inventoryItemUI = null;
            return false;
        }

        public void SelectItem(InventoryItemUI inventoryItemUI = null)
        {
            if (inventoryItemUI == null || inventoryItemUI.InventoryEntry == null)
            {
                SetupInfoContainer();
                if (selectedUIItem) selectedUIItem.SetHighlight(false);
                selectedUIItem = null;
                return;
            }

            if (selectedUIItem) selectedUIItem.SetHighlight(false);

            SetupInfoContainer(inventoryItemUI.InventoryEntry);
            inventoryItemUI.SetHighlight(true);

            selectedUIItem = inventoryItemUI;
        }

        void RequestDiscardSelectedItem() => InventoryManager.Instance.RequestRemoveEntry(selectedUIItem.InventoryEntry);

        void RequestUseSelectedItem() => InventoryManager.Instance.RequestUseItem(selectedUIItem.InventoryEntry);

        void SetupInfoContainer(InventoryEntry entry = null)
        {
            icon.transform.parent.gameObject.SetActive(entry != null);
            itemNameText.gameObject.SetActive(entry != null);
            descriptionText.gameObject.SetActive(entry != null);
            useButton.gameObject.SetActive(entry != null);
            discardButton.gameObject.SetActive(entry != null);

            if (entry == null) return;

            icon.sprite = entry.ItemData.Icon;
            itemNameText.text = entry.ItemData.DisplayName;
            descriptionText.text = entry.ItemData.Description;
            useButton.gameObject.SetActive(entry.ItemData.Usage != Data.InventoryItemUsage.None);
        }

        private void OnItemUIReordered(DraggableGridItem draggableItem, int previousIndex, int newIndex)
        {
            if (!draggableItem.TryGetComponent(out InventoryItemUI itemUI)) return;
            InventoryManager.Instance.RequestMoveEntry(itemUI.InventoryEntry, newIndex);
        }

        void EnsureItemsUICount()
        {
            int requiredCount = InventoryManager.Instance.Entries.Count;
            int currentCount = currentItemsUI.Count;

            if (currentCount < requiredCount)
            {
                int amountToCreate = requiredCount - currentCount;

                for (int i = 0; i < amountToCreate; i++)
                {
                    InventoryItemUI instance = Instantiate(itemUIReference, container);
                    instance.gameObject.SetActive(true);
                    instance.Initialize(this);
                    instance.DraggableItem.OnReordered += OnItemUIReordered;

                    currentItemsUI.Add(instance);
                }
            }
            else if (currentCount > requiredCount)
            {
                int amountToRemove = currentCount - requiredCount;

                for (int i = 0; i < amountToRemove; i++)
                {
                    int lastIndex = currentItemsUI.Count - 1;
                    InventoryItemUI instance = currentItemsUI[lastIndex];

                    instance.DraggableItem.OnReordered -= OnItemUIReordered;

                    currentItemsUI.RemoveAt(lastIndex);
                    Destroy(instance != null ? instance.gameObject : null);
                }
            }

            itemUIReference.transform.SetSiblingIndex(container.childCount);
        }
    }
}
