using System;
using UnityEngine;

namespace Alien.Gameplay
{
    public enum GameState
    {
        Gameplay,
        Paused
    }

    [DefaultExecutionOrder(-100)]
    public sealed class GameStateManager : MonoBehaviour
    {
        [SerializeField] private GameState initialState = GameState.Gameplay;

        public static GameStateManager Instance { get; private set; }

        public GameState CurrentState { get; private set; }
        public bool IsPaused => CurrentState == GameState.Paused;

        public event Action<GameState> StateChanged;
        public event Action<bool> PauseChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            CurrentState = initialState;
            ApplyTimeScale();
        }

        private void OnDestroy()
        {
            if (Instance != this) return;

            Instance = null;
            Time.timeScale = 1f;
        }

        public void RequestState(GameState state, bool affectTimeScale = true)
        {
            if (CurrentState == state)
            {
                if (affectTimeScale)
                    ApplyTimeScale();

                return;
            }

            CurrentState = state;

            if (affectTimeScale)
                ApplyTimeScale();

            StateChanged?.Invoke(CurrentState);
            PauseChanged?.Invoke(IsPaused);
        }

        public bool IsState(GameState state) => CurrentState == state;

        public void Pause() => RequestState(GameState.Paused);
        public void ResumeGameplay() => RequestState(GameState.Gameplay);
        public void SetPaused(bool isPaused) => RequestState(isPaused ? GameState.Paused : GameState.Gameplay);
        public void TogglePause() => RequestState(IsPaused ? GameState.Gameplay : GameState.Paused);

        private void ApplyTimeScale() => Time.timeScale = IsPaused ? 0f : 1f;
    }
}
