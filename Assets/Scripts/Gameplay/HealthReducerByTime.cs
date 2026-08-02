using UnityEngine;

namespace Alien.Gameplay
{
    [RequireComponent(typeof(Health))]
    public class HealthReducerByTime : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float healthReductionPerSecond = 1f;

        private Health health;

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void Update()
        {
            health.TakeDamage(healthReductionPerSecond * Time.deltaTime);
        }
    }
}
