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
        InventoryUIController inventoryUIController;
        Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        public void Setup(InventoryEntry inventoryEntry, InventoryUIController controller)
        {
            this.InventoryEntry = inventoryEntry;
            inventoryUIController = controller;
            highlight.SetActive(false);

            icon.sprite = inventoryEntry.ItemData.Icon;
            icon.gameObject.SetActive(true);
            amountText.text = inventoryEntry.Quantity.ToString();
            amountText.gameObject.SetActive(inventoryEntry.Quantity > 1);
            button.onClick.AddListener(RequestSelectItem);
        }

        public void SetHighlight(bool enabled) => highlight.SetActive(enabled);

        void RequestSelectItem() => inventoryUIController.SelectItem(this);

        private void OnDestroy()
        {
            button.onClick.RemoveListener(RequestSelectItem);
        }
    }
}
