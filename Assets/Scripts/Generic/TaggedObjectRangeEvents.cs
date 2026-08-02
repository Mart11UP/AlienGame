using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

namespace Alien.Generic
{
    public class TaggedObjectRangeEvents : MonoBehaviour
    {
        [SerializeField, Tag] private string trackedTag = "Player";
        [SerializeField, Min(0f)] private float minimumDistance;
        [SerializeField, Min(0f)] private float maximumDistance = 5f;

        [SerializeField] private UnityEvent onEnteredRange = new();
        [SerializeField] private UnityEvent onExitedRange = new();

        private Transform trackedTransform;
        private bool isInsideRange;

        private void OnEnable()
        {
            FindTrackedObject();
            UpdateRangeState();
        }

        private void Update()
        {
            if (trackedTransform == null || !trackedTransform.gameObject.activeInHierarchy)
                FindTrackedObject();

            UpdateRangeState();
        }

        private void OnDisable()
        {
            trackedTransform = null;
            isInsideRange = false;
        }

        private void FindTrackedObject()
        {
            if (string.IsNullOrEmpty(trackedTag))
            {
                trackedTransform = null;
                return;
            }

            GameObject trackedObject;

            try
            {
                trackedObject = GameObject.FindWithTag(trackedTag);
            }
            catch (UnityException)
            {
                Debug.LogError($"Tag '{trackedTag}' is not defined.", this);
                enabled = false;
                return;
            }

            trackedTransform = trackedObject != null ? trackedObject.transform : null;
        }

        private void UpdateRangeState()
        {
            if (trackedTransform == null)
            {
                SetInsideRange(false);
                return;
            }

            float sqrDistance = (trackedTransform.position - transform.position).sqrMagnitude;
            bool isWithinMaximum = sqrDistance <= maximumDistance * maximumDistance;
            bool isAboveMinimum = sqrDistance >= minimumDistance * minimumDistance;

            SetInsideRange(isWithinMaximum && isAboveMinimum);
        }

        private void SetInsideRange(bool value)
        {
            if (isInsideRange == value) return;

            isInsideRange = value;

            if (isInsideRange)
                onEnteredRange.Invoke();
            else
                onExitedRange.Invoke();
        }

        private void OnValidate()
        {
            minimumDistance = Mathf.Max(0f, minimumDistance);
            maximumDistance = Mathf.Max(minimumDistance, maximumDistance);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, maximumDistance);

            if (minimumDistance <= 0f) return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, minimumDistance);
        }
    }
}
