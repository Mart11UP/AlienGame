using Alien.Interactables;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Alien.UI
{
    public class ItemRequirementUI : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text amountText;

        public void Setup(ItemRequirement requirement, int ownedQuantity)
        {
            if (requirement == null)
            {
                icon.enabled = false;
                amountText.text = "-";
                return;
            }

            int requiredQuantity = requirement.Quantity;

            icon.sprite = requirement.ItemData != null ? requirement.ItemData.Icon : null;
            icon.enabled = icon.sprite != null;
            amountText.text = $"{Mathf.Clamp(ownedQuantity, 0, requiredQuantity)}/{requiredQuantity}";
        }
    }
}
