using UnityEngine;

namespace Alien.Player
{
    [RequireComponent(typeof(PlayerMovement), typeof(PlayerInteractor))]
    public sealed class PlayerAudio : MonoBehaviour
    {
        [SerializeField] private AudioSource footstepsAudio;
        [SerializeField] private AudioSource interactionAudio;

        private PlayerMovement playerMovement;
        private PlayerInteractor playerInteractor;

        private void Awake()
        {
            playerMovement = GetComponent<PlayerMovement>();
            playerInteractor = GetComponent<PlayerInteractor>();

            if (footstepsAudio != null)
            {
                footstepsAudio.playOnAwake = false;
                footstepsAudio.loop = true;
            }

            if (interactionAudio != null)
                interactionAudio.playOnAwake = false;
        }

        private void OnEnable() => playerInteractor.Interacted += PlayInteractionSound;

        private void OnDisable()
        {
            playerInteractor.Interacted -= PlayInteractionSound;
            footstepsAudio?.Stop();
        }

        private void Update()
        {
            if (footstepsAudio == null || footstepsAudio.clip == null) return;

            bool shouldPlay = playerMovement.IsGrounded && playerMovement.MovementInputMagnitude > 0.1f;

            if (shouldPlay && !footstepsAudio.isPlaying)
                footstepsAudio.Play();
            else if (!shouldPlay && footstepsAudio.isPlaying)
                footstepsAudio.Stop();
        }

        private void PlayInteractionSound()
        {
            if (interactionAudio == null || interactionAudio.clip == null) return;

            interactionAudio.PlayOneShot(interactionAudio.clip);
        }
    }
}
