using Alien.Inventory;
using System.Collections.Generic;
using UnityEngine;

namespace Alien.UI
{
    public class InventoryUIController : MonoBehaviour
    {
        [SerializeField] Transform container;
        [SerializeField] InventoryItemUI itemUIReference;
        List<InventoryItemUI> currentItemsUI = new();


        void Start()
        {
            itemUIReference.gameObject.SetActive(false);
            SetupItemsUI();
            InventoryManager.Instance.OnInventoryChanged += SetupItemsUI;
        }

        private void OnDestroy()
        {
            InventoryManager.Instance.OnInventoryChanged += SetupItemsUI;
        }

        void SetupItemsUI()
        {
            EnsureItemsUICount();
            IReadOnlyList<InventoryEntry> entries = InventoryManager.Instance.Entries;
            for (int i = 0; i < entries.Count; i++)
            {
                InventoryEntry entry = entries[i];
                InventoryItemUI itemUI = currentItemsUI[i];

                itemUI.Setup(entry, this);
            }
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
                    instance.Setup();
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

                    currentItemsUI.RemoveAt(lastIndex);
                    Destroy(instance.gameObject);
                }
            }
        }
    }
}
