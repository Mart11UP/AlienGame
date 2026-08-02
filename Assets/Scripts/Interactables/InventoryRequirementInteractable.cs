using System;
using System.Collections.Generic;
using Alien.Data;
using Alien.Inventory;
using UnityEngine;
using UnityEngine.Events;

namespace Alien.Interactables
{
    [Serializable]
    public class ItemRequirement
    {
        [SerializeField] private ItemData itemData;
        [SerializeField, Min(1)] private int quantity = 1;

        public ItemData ItemData => itemData;
        public int Quantity => Mathf.Max(1, quantity);
    }

    [DisallowMultipleComponent]
    public class InventoryRequirementInteractable : MonoBehaviour, IInteractable
    {
        private const int DefaultLayer = 0;
        private const string InteractableLayerName = "Interactable";

        [SerializeField] private List<ItemRequirement> requirements = new();
        [SerializeField] private UnityEvent onInteracted = new();

        public IReadOnlyList<ItemRequirement> Requirements => requirements;
        public bool RequirementsMet { get; private set; }

        private void Start()
        {
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.OnInventoryChanged += RefreshAvailability;

            RefreshAvailability();
        }

        public void Interact()
        {
            RefreshAvailability();

            if (RequirementsMet)
                onInteracted.Invoke();
        }

        public void RefreshAvailability()
        {
            RequirementsMet = HasAllRequirements();

            int interactableLayer = LayerMask.NameToLayer(InteractableLayerName);
            gameObject.layer = RequirementsMet && interactableLayer >= 0
                ? interactableLayer
                : DefaultLayer;
        }

        private bool HasAllRequirements()
        {
            if (InventoryManager.Instance == null) return false;

            foreach (ItemRequirement requirement in requirements)
            {
                if (requirement == null || requirement.ItemData == null) return false;
                if (!InventoryManager.Instance.ContainsItem(requirement.ItemData, requirement.Quantity)) return false;
            }

            return true;
        }

        private void OnDestroy()
        {
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.OnInventoryChanged -= RefreshAvailability;
        }
    }
}
