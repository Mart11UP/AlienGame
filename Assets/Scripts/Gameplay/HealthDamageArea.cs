using System.Collections.Generic;
using UnityEngine;

namespace Alien.Gameplay
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class HealthDamageArea : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float damagePerSecond = 10f;
        [SerializeField] private LayerMask affectedLayers = ~0;

        private readonly HashSet<Health> damagedHealth = new();

        private BoxCollider areaCollider;
        private Health ownerHealth;

        private void Awake()
        {
            areaCollider = GetComponent<BoxCollider>();
            ownerHealth = GetComponentInParent<Health>();
        }

        private void FixedUpdate()
        {
            if (!areaCollider.enabled || damagePerSecond <= 0f) return;

            damagedHealth.Clear();

            Transform areaTransform = areaCollider.transform;
            Vector3 scale = areaTransform.lossyScale;
            Vector3 absoluteScale = new(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            Vector3 halfExtents = Vector3.Scale(areaCollider.size * 0.5f, absoluteScale);

            Collider[] overlaps = Physics.OverlapBox(
                areaTransform.TransformPoint(areaCollider.center),
                halfExtents,
                areaTransform.rotation,
                affectedLayers,
                QueryTriggerInteraction.Collide);

            float damage = damagePerSecond * Time.fixedDeltaTime;

            foreach (Collider overlap in overlaps)
            {
                Health health = overlap.GetComponentInParent<Health>();

                if (health == null || health == ownerHealth || !damagedHealth.Add(health)) continue;

                health.TakeDamage(damage);
            }
        }
    }
}
