using NaughtyAttributes;
using Unity.Cinemachine;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Alien.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, ReadOnly] Rigidbody playerRigidbody;
        [SerializeField, ReadOnly] PlayerInput playerInput;
        [SerializeField, ReadOnly] CinemachineCamera cameraReference;

        [Header("Movement")]
        [SerializeField] float moveForce = 35f;
        [SerializeField] float jumpForce = 20f;
        [SerializeField] float rotationSpeed = 5f;
        [SerializeField] float additionalFallGravity = 28f;
        [SerializeField] float additionalJumpGravity;

        [Header("Ground Check")]
        [SerializeField, Min(0f)] float groundCheckDistance = 0.5f;
        [SerializeField, Min(0f)] float radius = 0.49f;
        [SerializeField] LayerMask groundLayers;

        [Header("Spring")]
        [SerializeField] float rideHeight = 1f;
        [SerializeField] float rayToGroundLength = 2f;
        [SerializeField] float rideSpringStrength = 50f;
        [SerializeField] float rideSpringDamper = 5f;

        [Header("Jump Assistance")]
        [SerializeField] float jumpInputBufferTime = 0.5f;
        [SerializeField] float coyoteTime = 0.1f;

        [Header("Input Actions")]
        [SerializeField] string moveActionName = "Move";
        [SerializeField] string jumpActionName = "Jump";

        InputAction moveAction;
        InputAction jumpAction;

        Vector2 movementInput;
        bool jumpPressed;

        bool applySpringForce = true;
        float jumpInputBufferTimer;
        float coyoteTimer;

        Vector3 gravitationalForce;

        private void Awake()
        {
            playerRigidbody = GetComponent<Rigidbody>();
            playerInput = GetComponent<PlayerInput>();
            cameraReference = GetComponentInChildren<CinemachineCamera>();

            gravitationalForce = Physics.gravity * playerRigidbody.mass;

            GetInputActions();
            InitializeCamera();
        }

        private void OnEnable()
        {
            jumpAction.performed += OnJumpPerformed;
        }

        private void OnDisable()
        {
            jumpAction.performed -= OnJumpPerformed;
        }

        private void Update()
        {
            movementInput = moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        }

        private void FixedUpdate()
        {
            float deltaTime = Time.fixedDeltaTime;

            UpdateMovement(deltaTime);
            UpdateJumpTimers(deltaTime);

            jumpPressed = false;
        }

        private void GetInputActions()
        {
            moveAction = playerInput.actions.FindAction(moveActionName);

            jumpAction = playerInput.actions.FindAction(jumpActionName);
        }

        private void InitializeCamera()
        {
            cameraReference.transform.SetParent(null);
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            jumpPressed = true;
        }

        private void UpdateMovement(float deltaTime)
        {
            Vector3 moveDirection = GetCameraRelativeMovementDirection();

            ApplyMovement(moveDirection, deltaTime);

            if (playerRigidbody.linearVelocity.y < 0f) applySpringForce = true;

            if (jumpPressed) jumpInputBufferTimer = jumpInputBufferTime;

            if (IsGrounded()) coyoteTimer = coyoteTime;

            TryJump();
            ApplySpringOrAdditionalGravity();
        }

        private void ApplyMovement(Vector3 moveDirection, float deltaTime)
        {
            if (moveDirection.sqrMagnitude < 0.001f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            Quaternion smoothedRotation = Quaternion.Lerp(playerRigidbody.rotation, targetRotation, rotationSpeed * deltaTime);

            playerRigidbody.MoveRotation(smoothedRotation);
            playerRigidbody.AddForce(moveDirection * moveForce);
        }

        private void TryJump()
        {
            if (jumpInputBufferTimer == 0f || coyoteTimer == 0f) return;

            jumpInputBufferTimer = 0f;
            coyoteTimer = 0f;

            Vector3 velocity = playerRigidbody.linearVelocity;
            velocity.y = 0f;
            playerRigidbody.linearVelocity = velocity;

            playerRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            applySpringForce = false;
        }

        private void ApplySpringOrAdditionalGravity()
        {
            bool foundGround = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, rayToGroundLength, groundLayers, QueryTriggerInteraction.Ignore);

            if (foundGround && applySpringForce)
            {
                ApplySpringForce(hit);
                return;
            }

            if (playerRigidbody.linearVelocity.y <= 0f)
                playerRigidbody.AddForce(Vector3.down * additionalFallGravity, ForceMode.Acceleration);
            else
                playerRigidbody.AddForce(Vector3.down * additionalJumpGravity, ForceMode.Acceleration);
        }

        private void ApplySpringForce(RaycastHit hit)
        {
            Vector3 playerVelocity = playerRigidbody.linearVelocity;
            Rigidbody hitRigidbody = hit.rigidbody;
            Vector3 otherVelocity = hitRigidbody != null ? hitRigidbody.linearVelocity : Vector3.zero;
            float playerVerticalVelocity = Vector3.Dot(Vector3.down, playerVelocity);
            float otherVerticalVelocity = Vector3.Dot(Vector3.down, otherVelocity);
            float relativeVelocity = playerVerticalVelocity - otherVerticalVelocity;
            float heightDifference = hit.distance - rideHeight;
            float springForce = heightDifference * rideSpringStrength - relativeVelocity * rideSpringDamper;
            Vector3 maintainHeightForce = -gravitationalForce + springForce * Vector3.down;

            playerRigidbody.AddForce(maintainHeightForce);
        }

        private Vector3 GetCameraRelativeMovementDirection()
        {
            if (movementInput.sqrMagnitude < 0.001f) return Vector3.zero;

            movementInput = Vector2.ClampMagnitude(movementInput, 1f);
            Transform cameraTransform = cameraReference.transform;
            Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            Vector3 moveDirection = forward * movementInput.y + right * movementInput.x;

            return moveDirection.normalized;
        }

        private bool IsGrounded()
        {
            Vector3 checkPosition = transform.position + Vector3.down * groundCheckDistance;

            return Physics.CheckSphere(checkPosition, radius, groundLayers, QueryTriggerInteraction.Ignore);
        }

        private void UpdateJumpTimers(float deltaTime)
        {
            jumpInputBufferTimer = Mathf.Max(0f, jumpInputBufferTimer - deltaTime);
            coyoteTimer = Mathf.Max(0f, coyoteTimer - deltaTime);
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 checkPosition = transform.position + Vector3.down * groundCheckDistance;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(checkPosition, radius);

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, Vector3.down * rayToGroundLength);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + Vector3.down * rideHeight, 0.1f);
        }
    }
}
