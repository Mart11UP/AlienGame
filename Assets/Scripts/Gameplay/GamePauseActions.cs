using UnityEngine;

namespace Alien.Gameplay
{
    public sealed class GamePauseActions : MonoBehaviour
    {
        public void Pause() => RequestState(GameState.Paused, true);
        public void PauseWithoutTimeScale() => RequestState(GameState.Paused, false);
        public void Resume() => RequestState(GameState.Gameplay, true);
        public void ResumeWithoutTimeScale() => RequestState(GameState.Gameplay, false);

        private static void RequestState(GameState state, bool affectTimeScale)
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.RequestState(state, affectTimeScale);
        }
    }
}
