using UnityEngine;
using UnityEngine.UI;
using Alien.Inventory;
using TMPro;

namespace Alien.UI
{
    public class InventoryItemUI : MonoBehaviour
    {
        [SerializeField] Image icon;
        [SerializeField] GameObject highlight;
        [SerializeField] TMP_Text amountText;
        public InventoryEntry InventoryEntry { get; private set; }
        public DraggableGridItem DraggableItem { get; private set; }
        InventoryUIController inventoryUIController;
        Button button;

        public void Initialize(InventoryUIController controller)
        {
            button = GetComponent<Button>();
            DraggableItem = GetComponent<DraggableGridItem>();

            button.onClick.AddListener(RequestSelectItem);
            inventoryUIController = controller;
        }

        public void Setup(InventoryEntry inventoryEntry)
        {
            InventoryEntry = inventoryEntry;
            highlight.SetActive(false);

            icon.sprite = inventoryEntry.ItemData.Icon;
            icon.gameObject.SetActive(true);
            amountText.text = inventoryEntry.Quantity.ToString();
            amountText.gameObject.SetActive(inventoryEntry.Quantity > 1);
        }

        public void SetHighlight(bool enabled) => highlight.SetActive(enabled);

        void RequestSelectItem() => inventoryUIController.SelectItem(this);

        private void OnDestroy()
        {
            if (button) button.onClick.RemoveListener(RequestSelectItem);
        }
    }
}
