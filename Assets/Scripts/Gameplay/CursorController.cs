using UnityEngine;

namespace Alien.Gameplay
{
    public sealed class CursorController : MonoBehaviour
    {
        private GameStateManager gameStateManager;
        private bool releasedByEscape;

        private void OnEnable()
        {
            gameStateManager = GameStateManager.Instance;

            if (gameStateManager == null) return;

            gameStateManager.PauseChanged += ApplyPauseState;
            ApplyPauseState(gameStateManager.IsPaused);
        }

        private void OnDisable()
        {
            if (gameStateManager != null)
                gameStateManager.PauseChanged -= ApplyPauseState;

            SetCursorLocked(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                releasedByEscape = true;
                SetCursorLocked(false);
                return;
            }

            if (gameStateManager != null && !gameStateManager.IsPaused && Input.GetMouseButtonDown(0))
            {
                releasedByEscape = false;
                SetCursorLocked(true);
            }
        }

        private void ApplyPauseState(bool isPaused) => SetCursorLocked(!isPaused && !releasedByEscape);

        private static void SetCursorLocked(bool isLocked)
        {
            Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isLocked;
        }
    }
}
