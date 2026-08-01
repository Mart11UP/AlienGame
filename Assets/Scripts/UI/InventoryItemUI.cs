using UnityEngine;
using UnityEngine.UI;
using Alien.Inventory;

namespace Alien.UI
{
    public class InventoryItemUI : MonoBehaviour
    {
        [SerializeField] Image icon;
        [SerializeField] GameObject highlight;
        InventoryUIController inventoryUIController;
        Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        public void Setup(InventoryEntry inventoryEntry = null, InventoryUIController controller = null)
        {
            if (inventoryEntry == null)
            {
                icon.gameObject.SetActive(false);
                return;
            }

            inventoryUIController = controller;
            highlight.SetActive(false);

            icon.sprite = inventoryEntry.ItemData.Icon;
            icon.gameObject.SetActive(true);
        }
    }
}
