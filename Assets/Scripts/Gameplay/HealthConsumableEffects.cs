using System;
using Alien.Data;
using UnityEngine;

namespace Alien.Gameplay
{
    [Serializable]
    public sealed class HealEffect : ConsumableEffect
    {
        [SerializeField, Min(0f)] private float amount = 10f;

        public override void Apply(GameObject target)
        {
            if (target != null && target.TryGetComponent(out Health health))
                health.Heal(amount);
        }
    }

    [Serializable]
    public sealed class DamageEffect : ConsumableEffect
    {
        [SerializeField, Min(0f)] private float amount = 10f;

        public override void Apply(GameObject target)
        {
            if (target != null && target.TryGetComponent(out Health health))
                health.TakeDamage(amount);
        }
    }
}
