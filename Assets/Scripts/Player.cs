using UnityEngine;

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Look Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("References")]
    [SerializeField] private PlayerInput playerInput;

    private Rigidbody rb;
    private bool isGrounded;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Prevents the player from falling over
        rb.interpolation = RigidbodyInterpolation.Interpolate; // For smooth movement

        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        // Setup input actions
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        sprintAction = playerInput.actions["Sprint"];

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        CheckGrounded();
        HandleRotation();
        
        if (jumpAction.triggered && isGrounded)
        {
            HandleJump();
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void CheckGrounded()
    {
        // Simple raycast check for ground
        float rayLength = 1.1f; // Adjust based on player height
        isGrounded = Physics.Raycast(transform.position, Vector3.down, rayLength, groundLayer);
    }

    private void HandleMovement()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        float currentSpeed = sprintAction.IsPressed() ? sprintSpeed : moveSpeed;

        // If we are grappling, we should have less ground-based control to let physics take over
        bool isGrappling = GetComponent<ODMController>() != null && 
                          (GetComponent<ODMController>().leftHook.IsGrappling() || 
                           GetComponent<ODMController>().rightHook.IsGrappling());

        // Get camera direction (projected onto the horizontal plane)
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * input.y + right * input.x).normalized;
        
        if (isGrounded)
        {
            Vector3 targetVelocity = moveDirection * currentSpeed;
            targetVelocity.y = rb.linearVelocity.y;
            rb.linearVelocity = targetVelocity;
        }
        else
        {
            // In air, we rely more on ODMController's air control, but can add a bit here too
            // rb.AddForce(moveDirection * (moveSpeed * 0.5f), ForceMode.Acceleration);
        }
    }

    private void HandleJump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void HandleRotation()
    {
        // Smoothly rotate the player to face the looking direction (horizontal)
        Vector3 forward = cameraTransform.forward;
        forward.y = 0;
        
        if (forward != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
