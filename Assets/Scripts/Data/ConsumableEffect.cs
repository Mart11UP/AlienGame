using System;
using UnityEngine;

namespace Alien.Data
{
    [Serializable]
    public abstract class ConsumableEffect
    {
        public abstract void Apply(GameObject target);
    }

    [Serializable]
    public sealed class ConsumableEffectEntry
    {
        [SerializeReference] private ConsumableEffect effect;

        public void Apply(GameObject target) => effect?.Apply(target);
    }
}
