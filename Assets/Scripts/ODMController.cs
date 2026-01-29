using UnityEngine;
using UnityEngine.InputSystem;

public class ODMController : MonoBehaviour
{
    [Header("Hooks")]
    public GrapplingHook leftHook;
    public GrapplingHook rightHook;

    [Header("Gas Settings")]
    public float gasForce = 50f;
    public float airControlForce = 10f;
    public float reelSpeed = 15f;

    [Header("References")]
    public Transform cameraTransform;
    public PlayerInput playerInput;

    private Rigidbody rb;
    private InputAction grappleLeftAction;
    private InputAction grappleRightAction;
    private InputAction gasBoostAction;
    private InputAction moveAction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        grappleLeftAction = playerInput.actions["GrappleLeft"];
        grappleRightAction = playerInput.actions["GrappleRight"];
        gasBoostAction = playerInput.actions["Jump"];
        moveAction = playerInput.actions["Move"];
    }

    private void Update()
    {
        if (grappleLeftAction.WasPressedThisFrame()) leftHook.StartGrapple();
        if (grappleLeftAction.WasReleasedThisFrame()) leftHook.StopGrapple();

        if (grappleRightAction.WasPressedThisFrame()) rightHook.StartGrapple();
        if (grappleRightAction.WasReleasedThisFrame()) rightHook.StopGrapple();

        if (gasBoostAction.triggered && IsInAir())
        {
            ApplyGasBoost();
        }
    }

    private void FixedUpdate()
    {
        // HandleReeling();
        HandleAirControl();
    }

    private void HandleReeling()
    {
        // If grappling, reel in slightly to pull player towards points
        if (leftHook.IsGrappling() || rightHook.IsGrappling())
        {
            Vector3 reelDir = Vector3.zero;
            int hooks = 0;

            if (leftHook.IsGrappling())
            {
                reelDir += (leftHook.GetGrapplePoint() - transform.position).normalized;
                hooks++;
            }
            if (rightHook.IsGrappling())
            {
                reelDir += (rightHook.GetGrapplePoint() - transform.position).normalized;
                hooks++;
            }

            rb.AddForce(reelDir.normalized * reelSpeed, ForceMode.Acceleration);
        }
    }

    private void HandleAirControl()
    {
        if (IsInAir())
        {
            Vector2 input = moveAction.ReadValue<Vector2>();
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = 0;
            right.y = 0;
            
            Vector3 moveDir = (forward.normalized * input.y + right.normalized * input.x).normalized;
            rb.AddForce(moveDir * airControlForce, ForceMode.Acceleration);
        }
    }

    private void ApplyGasBoost()
    {
        rb.AddForce(cameraTransform.forward * gasForce, ForceMode.Impulse);
    }

    private bool IsInAir()
    {
        // Simple check, can be refined based on Player.isGrounded
        return !Physics.Raycast(transform.position, Vector3.down, 1.2f);
    }
}
