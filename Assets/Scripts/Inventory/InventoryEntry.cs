using System;
using Alien.Data;
using UnityEngine;

namespace Alien.Inventory
{
    [Serializable]
    public class InventoryEntry
    {
        [SerializeField] private ItemData itemData;
        [SerializeField] private int quantity;

        public ItemData ItemData => itemData;

        public int Quantity
        {
            get => quantity;
            set => quantity = Mathf.Max(0, value);
        }

        public InventoryEntry(ItemData itemData, int quantity)
        {
            this.itemData = itemData;
            this.quantity = Mathf.Max(1, quantity);
        }
    }
}
