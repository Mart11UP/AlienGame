using Alien.Gameplay;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace Alien.UI
{
    public class PlayerHealthUI : MonoBehaviour
    {
        [SerializeField, Tag] string playerTag = "Player";
        [SerializeField] Slider healthSlider;
        Health playerHealth;

        private void Awake()
        {
            playerHealth = GameObject.FindGameObjectWithTag(playerTag).GetComponent<Health>();
            playerHealth.OnHealthChanged.AddListener(UpdateHealth);
        }

        private void OnDestroy()
        {
            playerHealth.OnHealthChanged.RemoveListener(UpdateHealth);
        }

        void UpdateHealth(float currentHealth) => healthSlider.value = playerHealth.CurrentHealthNormalized;
    }
}
