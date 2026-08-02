using UnityEngine;
using UnityEngine.Events;

namespace Alien.Gameplay
{
    public sealed class PauseStateListener : MonoBehaviour
    {
        [SerializeField] private bool invertValue;
        [SerializeField] private UnityEvent<bool> onPauseChanged = new();

        private GameStateManager gameStateManager;

        private void OnEnable()
        {
            gameStateManager = GameStateManager.Instance;

            if (gameStateManager == null) return;

            gameStateManager.PauseChanged += SendPauseValue;
            SendPauseValue(gameStateManager.IsPaused);
        }

        private void OnDisable()
        {
            if (gameStateManager != null)
                gameStateManager.PauseChanged -= SendPauseValue;
        }

        private void SendPauseValue(bool isPaused)
        {
            onPauseChanged?.Invoke(invertValue ? !isPaused : isPaused);
        }
    }
}
