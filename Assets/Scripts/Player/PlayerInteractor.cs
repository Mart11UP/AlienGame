using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using Alien.Interactables;

namespace Alien.Player
{
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField, ReadOnly] PlayerInput playerInput;
        [SerializeField] private Vector3 interactionOrigin;
        [SerializeField] private float interactionRadius = 2f;
        [SerializeField] private LayerMask interactionMask = ~0;

        [Header("Input Actions")]
        [SerializeField] string interactActionName = "Interact";
        InputAction interactAction;
        IInteractable currentInteractable;
        Component currentInteractableComponent;
        ITargetable currentTargetable;

        void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            interactAction = playerInput.actions.FindAction(interactActionName);
        }

        private void OnEnable()
        {
            interactAction.performed += OnInteractPerformed;
            RefreshTarget();
        }

        private void OnDisable()
        {
            interactAction.performed -= OnInteractPerformed;
            SetTarget(null, null);
        }

        private void Update()
        {
            RefreshTarget();
        }

        void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (currentInteractableComponent == null || !currentInteractableComponent.gameObject.activeInHierarchy)
            {
                SetTarget(null, null);
                return;
            }

            currentInteractable.Interact();

            if (currentInteractableComponent == null || !currentInteractableComponent.gameObject.activeInHierarchy)
                SetTarget(null, null);
        }

        private void RefreshTarget()
        {
            TryGetNearestInteractableObject(out IInteractable interactable, out Component interactableComponent);
            SetTarget(interactable, interactableComponent);
        }

        private void SetTarget(IInteractable interactable, Component interactableComponent)
        {
            if (currentInteractableComponent == interactableComponent) return;

            currentTargetable?.SetTargeted(false);

            currentInteractable = interactable;
            currentInteractableComponent = interactableComponent;
            currentTargetable = currentInteractableComponent != null
                ? currentInteractableComponent.GetComponent<ITargetable>()
                : null;

            currentTargetable?.SetTargeted(true);
        }

        private bool TryGetNearestInteractableObject(out IInteractable interactable, out Component interactableComponent)
        {
            Vector3 worldOrigin = transform.TransformPoint(interactionOrigin);

            Collider[] hits = Physics.OverlapSphere(worldOrigin, interactionRadius, interactionMask);

            interactable = null;
            interactableComponent = null;
            float closestSqrDistance = float.MaxValue;

            foreach (Collider hit in hits)
            {
                IInteractable candidate = hit.GetComponentInParent<IInteractable>();
                Component candidateComponent = candidate as Component;

                if (candidateComponent == null) continue;

                float sqrDistance = (hit.ClosestPoint(worldOrigin) - worldOrigin).sqrMagnitude;

                if (sqrDistance < closestSqrDistance)
                {
                    closestSqrDistance = sqrDistance;
                    interactable = candidate;
                    interactableComponent = candidateComponent;
                }
            }

            return interactableComponent != null;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 worldOrigin = transform.TransformPoint(interactionOrigin);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(worldOrigin, interactionRadius);
        }
    }
}
