using UnityEngine;
using UnityEngine.Events;

namespace Alien.Gameplay
{
    public enum HealthDepletionAction
    {
        None,
        DisableGameObject,
        DestroyGameObject,
        ReloadCurrentScene
    }

    public class Health : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField] private HealthDepletionAction depletionAction = HealthDepletionAction.None;

        [Header("Events")]
        public UnityEvent<float> OnHealthChanged = new();
        public UnityEvent OnHealthDepleted = new();

        public float CurrentHealth { get; private set; }
        public float CurrentHealthNormalized => CurrentHealth / MaxHealth;
        public float MaxHealth => maxHealth;
        public bool IsDepleted => CurrentHealth <= 0f;

        private void Awake()
        {
            maxHealth = Mathf.Max(1f, maxHealth);
            CurrentHealth = maxHealth;
        }

        private void Start()
        {
            OnHealthChanged?.Invoke(CurrentHealth);
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0f || IsDepleted) return;

            SetHealth(CurrentHealth - amount);
        }

        public void Heal(float amount)
        {
            if (amount <= 0f || CurrentHealth >= maxHealth) return;

            SetHealth(CurrentHealth + amount);
        }

        public void ResetHealth() => SetHealth(maxHealth);

        private void SetHealth(float value)
        {
            float newHealth = Mathf.Clamp(value, 0f, maxHealth);

            if (Mathf.Approximately(CurrentHealth, newHealth)) return;

            bool wasDepleted = IsDepleted;
            CurrentHealth = newHealth;
            OnHealthChanged?.Invoke(CurrentHealth);

            if (wasDepleted || !IsDepleted) return;

            OnHealthDepleted?.Invoke();
            ApplyDepletionAction();
        }

        private void ApplyDepletionAction()
        {
            switch (depletionAction)
            {
                case HealthDepletionAction.DisableGameObject:
                    gameObject.SetActive(false);
                    break;

                case HealthDepletionAction.DestroyGameObject:
                    Destroy(gameObject);
                    break;

                case HealthDepletionAction.ReloadCurrentScene:
                    SceneReload.ReloadCurrentScene();
                    break;
            }
        }
    }
}
