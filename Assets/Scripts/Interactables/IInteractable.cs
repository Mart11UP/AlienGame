using UnityEngine;

namespace Alien.Interactables
{
    public interface IInteractable
    {
        public void Interact();
    }

    public interface ITargetable
    {
        public void SetTargeted(bool isTargeted);
    }
}
