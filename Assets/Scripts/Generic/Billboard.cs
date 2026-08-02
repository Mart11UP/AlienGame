using UnityEngine;

namespace Alien.Generic
{
    public class Billboard : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private bool lockYRotation;

        private void LateUpdate()
        {
            Camera cameraToUse = targetCamera != null
                ? targetCamera
                : Camera.main;

            if (cameraToUse == null)
                return;

            Vector3 direction = transform.position - cameraToUse.transform.position;

            if (lockYRotation)
                direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
                return;

            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
    }
}