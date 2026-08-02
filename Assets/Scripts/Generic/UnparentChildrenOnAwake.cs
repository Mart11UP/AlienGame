using UnityEngine;

namespace Alien.Generic
{
    public sealed class UnparentChildrenOnAwake : MonoBehaviour
    {
        private void Awake() => transform.DetachChildren();
    }
}
