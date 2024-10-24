using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public enum MovementStates
    {
        Idle,
        Walking,
        Crouching,
        CrouchWalking,
        Prone,
        ProneMoving,
        Jumping,
        JumpMoving,
        Running,
        RunningJump,
        Dash,
        Lean
    }

    [SerializeField] private float walkingSpeed = 500f;
    [SerializeField] private float runningSpeed = 1000f;
    [SerializeField] private float dashSpeed = 50f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldownDuration = 5f;
    [SerializeField] private Transform cameraTransform;

    private float currentSpeed;
    private float currentSpeedPercentage;
    private float verticalRotation;
    private float horizontalRotation;

    private bool isGrounded = true;
    private bool isDashing = false;
    private bool canDash = true;
    private bool isCooldown = false;
    private float dashCooldownTimer = 5f;

    private Rigidbody rb;
    private Vector3 movementInput;
    private ControlManager inputManager;
    private MovementStates currentState = MovementStates.Idle;

    public MovementStates CurrentState => currentState;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        inputManager = GetComponent<ControlManager>();
        ResetRotation();
        LockCursor();
        UpdateSpeed();
    }

    private void Update()
    {
        HandleMovementInput();
        HandleCameraRotation();
        HandleJump();
        UpdateSpeed();
        UpdateCooldown();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void HandleMovementInput()
    {
        Vector2 input = inputManager.GetMovementInput();
        movementInput = new Vector3(input.x, 0f, input.y);

        if (isDashing || currentState == MovementStates.RunningJump) return;

        if (!inputManager.GetRunInput() && !isCooldown) canDash = true;

        if (isGrounded)
        {
            if (movementInput.magnitude > 0f)
            {
                if (input.y > 0 && inputManager.GetRunInput()) currentState = MovementStates.Running;
                else if (!isDashing && !isCooldown && inputManager.GetRunInput() && input.y <= 0 && canDash) StartCoroutine(PerformDash());
                else currentState = MovementStates.Walking;
            }
            else currentState = MovementStates.Idle;
        }
        else
        {
            currentState = movementInput.magnitude > 0f ? MovementStates.JumpMoving : MovementStates.Jumping;
        }
    }

    private void MovePlayer()
    {
        if (isDashing) return;

        if (movementInput.magnitude > 1f) movementInput.Normalize();

        Vector3 moveDirection = transform.TransformDirection(movementInput) * (currentSpeed / 100f) * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveDirection);
    }

    private void HandleJump()
    {
        if (isGrounded && inputManager.GetJumpInput())
        {
            if (currentState == MovementStates.Running)
            {
                currentState = MovementStates.RunningJump;
                rb.AddForce(Vector3.up * jumpForce * 1.1f, ForceMode.Impulse);
            }
            else if (movementInput.magnitude > 0f)
            {
                currentState = MovementStates.JumpMoving;
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
            else
            {
                currentState = MovementStates.Jumping;
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }

            isGrounded = false;
        }
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;
        canDash = false;
        isCooldown = true;
        currentState = MovementStates.Dash;

        Vector3 dashDirection = (transform.forward * movementInput.z + transform.right * movementInput.x).normalized;

        rb.velocity = dashDirection * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.velocity = Vector3.zero;
        isDashing = false;

        currentState = movementInput.magnitude > 0f ? MovementStates.Walking : MovementStates.Idle;
    }

    private void UpdateCooldown()
    {
        if (isCooldown)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0)
            {
                dashCooldownTimer = dashCooldownDuration;
                isCooldown = false;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            currentState = movementInput.magnitude > 0f ? (inputManager.GetRunInput() ? MovementStates.Running : MovementStates.Walking) : MovementStates.Idle;
        }
    }

    private void UpdateSpeed()
    {
        currentSpeed = currentState switch
        {
            MovementStates.Idle => 0f,
            MovementStates.Walking => walkingSpeed,
            MovementStates.Running => runningSpeed,
            MovementStates.Dash => dashSpeed,
            _ => currentSpeed
        };
        currentSpeedPercentage = currentSpeed / runningSpeed * 100f;
    }

    private void HandleCameraRotation()
    {
        Vector2 mouseInput = inputManager.GetMouseInput();
        float mouseSensitivity = inputManager.GetMouseSensitivity();

        horizontalRotation = (horizontalRotation + mouseInput.x * mouseSensitivity * Time.deltaTime) % 360f;
        if (horizontalRotation < 0f) horizontalRotation += 360f;

        transform.localRotation = Quaternion.Euler(0f, horizontalRotation, 0f);

        verticalRotation = Mathf.Clamp(verticalRotation - mouseInput.y * mouseSensitivity * Time.deltaTime, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    private void ResetRotation()
    {
        verticalRotation = 0f;
        horizontalRotation = 0f;
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public float GetCurrentSpeedPercentage()
    {
        return currentSpeedPercentage;
    }
}
