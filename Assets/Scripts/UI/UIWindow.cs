using Alien.Gameplay;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Alien.UI
{
    // Based on a script from a previous project

    public class UIWindow : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private bool hideOnAwake = true; 
        [SerializeField] private bool pauseWhileVisible = false;
        [SerializeField] private Button[] toggleButtons;
        [SerializeField] private InputActionReference[] toggleActions;

        protected GameObject Root => root;

        protected virtual void Awake()
        {
            if (hideOnAwake && root != null)
                root.SetActive(false);

            RegisterCloseButtons();
            RegisterInputActions();

            OnAwakeInternal();
        }

        protected virtual void OnDestroy()
        {
            UnregisterCloseButtons();
            UnregisterInputActions();
        }

        public virtual void Show()
        {
            if (root == null) return;

            if (pauseWhileVisible) GameStateManager.Instance.Pause();
            root.SetActive(true);
            OnShown();
        }

        public virtual void Hide()
        {
            if (root == null) return;

            if (pauseWhileVisible) GameStateManager.Instance.ResumeGameplay();
            root.SetActive(false);
            OnHidden();
        }

        public virtual void Toggle()
        {
            if (root == null) return;

            if (root.activeSelf)
                Hide();
            else
                Show();
        }

        protected virtual void OnShown() { }
        protected virtual void OnHidden() { }
        protected virtual void OnAwakeInternal() { }

        private void RegisterCloseButtons()
        {
            if (toggleButtons == null) return;

            foreach (Button button in toggleButtons)
                if (button != null) button.onClick.AddListener(Toggle);
        }

        private void UnregisterCloseButtons()
        {
            if (toggleButtons == null) return;

            foreach (Button button in toggleButtons)
                if (button != null) button.onClick.RemoveListener(Toggle);
        }

        private void RegisterInputActions()
        {
            if (toggleActions == null) return;

            foreach (InputActionReference actionReference in toggleActions)
            {
                if (actionReference?.action == null) continue;

                actionReference.action.performed += OnTogglePerformed;
                actionReference.action.Enable();
            }
        }

        private void UnregisterInputActions()
        {
            if (toggleActions == null) return;

            foreach (InputActionReference actionReference in toggleActions)
            {
                if (actionReference?.action == null) continue;

                actionReference.action.performed -= OnTogglePerformed;
            }
        }

        private void OnTogglePerformed(InputAction.CallbackContext _) => Toggle();
    }
}
