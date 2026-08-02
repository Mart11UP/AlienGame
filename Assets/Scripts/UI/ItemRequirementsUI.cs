using System.Collections.Generic;
using Alien.Interactables;
using Alien.Inventory;
using UnityEngine;

namespace Alien.UI
{
    public class ItemRequirementsUI : MonoBehaviour
    {
        [SerializeField] private InventoryRequirementInteractable requirementsSource;
        [SerializeField] private Transform container;
        [SerializeField] private ItemRequirementUI itemReference;

        private readonly List<ItemRequirementUI> itemsUI = new();

        private void Start()
        {
            itemReference.gameObject.SetActive(false);
            CreateItemsUI();
            RefreshItemsUI();

            if (InventoryManager.Instance != null)
                InventoryManager.Instance.OnInventoryChanged += RefreshItemsUI;
        }

        private void CreateItemsUI()
        {
            foreach (ItemRequirement requirement in requirementsSource.Requirements)
            {
                ItemRequirementUI instance = Instantiate(itemReference, container);
                instance.gameObject.SetActive(true);
                itemsUI.Add(instance);
            }
        }

        private void RefreshItemsUI()
        {
            if (InventoryManager.Instance == null) return;

            IReadOnlyList<ItemRequirement> requirements = requirementsSource.Requirements;

            for (int i = 0; i < itemsUI.Count; i++)
            {
                ItemRequirement requirement = requirements[i];
                int ownedQuantity = requirement != null ? InventoryManager.Instance.GetItemQuantity(requirement.ItemData) : 0;

                itemsUI[i].Setup(requirement, ownedQuantity);
            }
        }

        private void OnDestroy()
        {
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.OnInventoryChanged -= RefreshItemsUI;
        }
    }
}
