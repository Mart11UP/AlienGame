using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace Alien.UI
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class DraggableGridItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] bool modifyHierarchy = true;
        [Header("References")]
        [SerializeField] private RectTransform gridContainer;
        [SerializeField] private Canvas rootCanvas;

        [Header("Drag Visuals")]
        [SerializeField, Range(0f, 1f)] private float originalAlphaWhileDragging = 0.35f;

        public event Action<DraggableGridItem, int, int> OnReordered;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;

        private GameObject dragVisual;
        private RectTransform dragVisualRect;

        private int originalSiblingIndex;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (gridContainer == null || rootCanvas == null) return;

            originalSiblingIndex = transform.GetSiblingIndex();

            CreateDragVisual();

            canvasGroup.alpha = originalAlphaWhileDragging;
            canvasGroup.blocksRaycasts = false;

            UpdateDragVisualPosition(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragVisualRect == null) return;

            UpdateDragVisualPosition(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            DraggableGridItem target = FindItemUnderPointer(eventData);

            if (target != null && target != this)
            {
                int targetIndex = target.transform.GetSiblingIndex();

                if (modifyHierarchy) transform.SetSiblingIndex(targetIndex);

                OnReordered?.Invoke(this, originalSiblingIndex, targetIndex);
            }
            else
                if (modifyHierarchy) transform.SetSiblingIndex(originalSiblingIndex); 

            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            DestroyDragVisual();

            LayoutRebuilder.ForceRebuildLayoutImmediate(gridContainer);
        }

        private void CreateDragVisual()
        {
            dragVisual = Instantiate(gameObject, rootCanvas.transform);
            dragVisual.name = $"{name}_DragVisual";

            if (dragVisual.TryGetComponent(out DraggableGridItem draggableComponent)) Destroy(draggableComponent);

            if (!dragVisual.TryGetComponent(out CanvasGroup dragCanvasGroup)) dragCanvasGroup = dragVisual.AddComponent<CanvasGroup>();

            dragCanvasGroup.alpha = 0.8f;
            dragCanvasGroup.blocksRaycasts = false;
            dragCanvasGroup.interactable = false;

            if (dragVisual.TryGetComponent(out Button dragButton)) dragButton.interactable = false;

            dragVisualRect = dragVisual.GetComponent<RectTransform>();
            dragVisualRect.anchorMin = new Vector2(0.5f, 0.5f);
            dragVisualRect.anchorMax = new Vector2(0.5f, 0.5f);
            dragVisualRect.pivot = new Vector2(0.5f, 0.5f);
            dragVisualRect.sizeDelta = rectTransform.rect.size;

            dragVisual.transform.SetAsLastSibling();
        }

        private void UpdateDragVisualPosition(PointerEventData eventData)
        {
            RectTransform canvasRect = rootCanvas.transform as RectTransform;

            Camera eventCamera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventCamera, out Vector2 localPosition))
                dragVisualRect.localPosition = localPosition;
        }

        private DraggableGridItem FindItemUnderPointer(PointerEventData eventData)
        {
            List<RaycastResult> results = new();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (RaycastResult result in results)
            {
                DraggableGridItem item = result.gameObject.GetComponentInParent<DraggableGridItem>();

                if (item != null && item != this && item.gridContainer == gridContainer) return item;
            }

            return null;
        }

        private void DestroyDragVisual()
        {
            if (dragVisual != null) Destroy(dragVisual);

            dragVisual = null;
            dragVisualRect = null;
        }
    }
}