using Alien.Data;
using Alien.Inventory;
using UnityEngine;

namespace Alien.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerConsumableEffectReceiver : MonoBehaviour
    {
        private void OnEnable()
        {
            InventoryManager.Instance.ItemUsed += ApplyConsumableEffects;
        }

        private void OnDisable()
        {
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.ItemUsed -= ApplyConsumableEffects;
        }

        private void ApplyConsumableEffects(ItemData itemData)
        {
            if (itemData?.ConsumableEffects == null) return;

            foreach (ConsumableEffectEntry effect in itemData.ConsumableEffects)
                effect?.Apply(gameObject);
        }
    }
}
