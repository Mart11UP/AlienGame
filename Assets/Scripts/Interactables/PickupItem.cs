using Alien.Data;
using Alien.Inventory;
using UnityEngine;

namespace Alien.Interactables
{
    public class PickupItem : MonoBehaviour, IInteractable
    {
        [SerializeField] ItemData itemData;

        public void Interact()
        {
            gameObject.SetActive(false);
            InventoryManager.Instance.RequestAddItem(itemData);
        }
    }
}
