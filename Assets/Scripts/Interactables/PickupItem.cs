using Alien.Data;
using Alien.Inventory;
using UnityEngine;

namespace Alien.Interactables
{
    public class PickupItem : MonoBehaviour, IInteractable
    {
        [SerializeField] ItemData itemData;
        [SerializeField] int quantity = 1;

        public void Interact()
        {
            if (InventoryManager.Instance.RequestAddItem(itemData, quantity))
                gameObject.SetActive(false);
        }
    }
}
