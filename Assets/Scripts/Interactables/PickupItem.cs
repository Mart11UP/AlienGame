using UnityEngine;

namespace Alien.Interactables
{
    public class PickupItem : MonoBehaviour, IInteractable
    {
        public void Interact()
        {
            gameObject.SetActive(false);
        }
    }
}
