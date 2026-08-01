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

        void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            interactAction = playerInput.actions.FindAction(interactActionName);
        }

        private void OnEnable()
        {
            interactAction.performed += OnInteractPerformed;
        }

        private void OnDisable()
        {
            interactAction.performed -= OnInteractPerformed;
        }

        void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if(!TryGetNearestInteractableObject(out IInteractable interactable)) return;

            interactable.Interact();
        }

        private bool TryGetNearestInteractableObject(out IInteractable interactable)
        {
            Vector3 worldOrigin = transform.TransformPoint(interactionOrigin);

            Collider[] hits = Physics.OverlapSphere(worldOrigin, interactionRadius, interactionMask, QueryTriggerInteraction.Ignore);

            interactable = null;
            float closestSqrDistance = float.MaxValue;

            foreach (Collider hit in hits)
            {
                IInteractable candidate = hit.GetComponentInParent<IInteractable>();

                if (candidate == null) continue;

                float sqrDistance = (hit.ClosestPoint(worldOrigin) - worldOrigin).sqrMagnitude;

                if (sqrDistance < closestSqrDistance)
                {
                    closestSqrDistance = sqrDistance;
                    interactable = candidate;
                }
            }

            return interactable != null;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 worldOrigin = transform.TransformPoint(interactionOrigin);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(worldOrigin, interactionRadius);
        }
    }
}
