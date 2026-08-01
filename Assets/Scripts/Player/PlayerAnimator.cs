using NaughtyAttributes;
using UnityEngine;

namespace Alien.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField, ReadOnly] PlayerMovement playerMovement;
        [SerializeField] Animator animator;
        [SerializeField, AnimatorParam("animator")] string velocityParam = "Velocity";
        [SerializeField, AnimatorParam("animator")] string jumpParam = "Jump";
        [SerializeField, AnimatorParam("animator")] string isGroundedParam = "IsGrounded";
        private void Awake()
        {
            playerMovement = GetComponent<PlayerMovement>();
            playerMovement.OnJump.AddListener(TriggerJumpAnimation);
        }

        private void OnDestroy()
        {
            playerMovement.OnJump.RemoveListener(TriggerJumpAnimation);
        }

        private void OnValidate()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            animator.SetFloat(velocityParam, playerMovement.MovementInputMagnitude);
            animator.SetBool(isGroundedParam, playerMovement.IsGrounded);
        }

        void TriggerJumpAnimation() => animator.SetTrigger(jumpParam);
    }
}
