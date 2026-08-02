using System;
using Alien.Data;
using Alien.Inventory;
using UnityEngine;

namespace Alien.Interactables
{
    public class PickupItem : MonoBehaviour, IInteractable
    {
        [SerializeField] ItemData itemData;
        [SerializeField] int quantity = 1;
        public bool IsTargeted { get; private set; }
        public event Action<bool> TargetedChanged;

        public void Interact()
        {
            if (InventoryManager.Instance.RequestAddItem(itemData, quantity))
                gameObject.SetActive(false);
        }

        public void SetTargeted(bool isTargeted)
        {
            if (IsTargeted == isTargeted) return;

            IsTargeted = isTargeted;
            TargetedChanged?.Invoke(IsTargeted);
        }

        private void OnDisable()
        {
            SetTargeted(false);
        }
    }
}
