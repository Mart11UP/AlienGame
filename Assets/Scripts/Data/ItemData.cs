using UnityEngine;
using NaughtyAttributes;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Alien.Data
{
    [CreateAssetMenu( fileName = "New Item", menuName = "Inventory/Item Data")]
    public class ItemData : ScriptableObject
    {
        [SerializeField, ReadOnly] string id;
        [SerializeField] string displayName;
        [SerializeField, TextArea] string description;
        [SerializeField] Sprite icon;

        [Header("Inventory")]
        [SerializeField] private InventoryItemUsage usage;
        [SerializeField, Min(1)] private int maxStackSize = 1;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public InventoryItemUsage Usage => usage;
        public int MaxStackSize => maxStackSize;

#if UNITY_EDITOR
        private void OnValidate()
        {
            GenerateId();
        }

        private void GenerateId()
        {
            string assetPath = AssetDatabase.GetAssetPath(this);

            if (string.IsNullOrEmpty(assetPath)) return;

            string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);

            if (string.IsNullOrEmpty(assetGuid) || id == assetGuid) return;

            id = assetGuid;
            EditorUtility.SetDirty(this);
        }
#endif
    }
}