using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Third-person player movement using CharacterController.
/// Uses the existing InputSystem_Actions.inputactions asset (Move, Sprint, Jump actions).
/// CharacterController is the sole physics driver. Rigidbody must be kinematic.
/// Movement direction is relative to the ThirdPersonCamera facing direction.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector-exposed settings
    // -------------------------------------------------------------------------

    [Header("Input")]
    [Tooltip("Assign Assets/InputSystem_Actions.inputactions here")]
    public InputActionAsset inputActionsAsset;

    [Header("Camera")]
    [Tooltip("Assign the ThirdPersonCamera component from Main Camera")]
    public ThirdPersonCamera thirdPersonCamera;

    [Header("Movement Settings")]
    [Tooltip("Walking speed (units/sec)")]
    public float walkSpeed = 3.5f;

    [Tooltip("Running speed while holding Sprint (units/sec)")]
    public float runSpeed = 6.0f;

    [Tooltip("Smooth time for speed transitions (seconds)")]
    public float speedSmoothTime = 0.1f;

    [Tooltip("Speed at which the character rotates toward movement direction (deg/sec)")]
    public float rotationSpeed = 15f;

    [Header("Jump & Gravity")]
    [Tooltip("Jump height in world units")]
    public float jumpHeight = 1.2f;

    [Tooltip("Gravity acceleration (should be negative)")]
    public float gravity = -9.81f;

    [Tooltip("Downward velocity clamp while grounded (prevents drift)")]
    public float groundedGravityClamp = -2f;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private CharacterController _controller;
    private Animator            _animator;

    // Input actions
    private InputAction _moveAction;
    private InputAction _sprintAction;
    private InputAction _jumpAction;

    // Movement state
    private Vector3 _velocity;          // accumulated vertical velocity (gravity + jump)
    private float   _currentSpeed;      // smoothed horizontal speed
    private float   _speedVelocity;     // used by SmoothDamp

    // Ground state
    private bool _isGrounded;
    private bool _jumpRequested;

    // Cached animator parameter hashes (avoids string allocations per frame)
    private static readonly int AnimIsMoving        = Animator.StringToHash("isMoving");
    private static readonly int AnimIsWalking       = Animator.StringToHash("isWalking");
    private static readonly int AnimIsSprinting     = Animator.StringToHash("isSprinting");
    private static readonly int AnimIsGrounded      = Animator.StringToHash("isGrounded");
    private static readonly int AnimSpeed           = Animator.StringToHash("speed");
    private static readonly int AnimVerticalVelocity = Animator.StringToHash("verticalVelocity");
    private static readonly int AnimJump            = Animator.StringToHash("jump");

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator   = GetComponent<Animator>();

        // Resolve input actions from the assigned asset
        if (inputActionsAsset != null)
        {
            var playerMap = inputActionsAsset.FindActionMap("Player", throwIfNotFound: true);
            _moveAction   = playerMap.FindAction("Move",   throwIfNotFound: true);
            _sprintAction = playerMap.FindAction("Sprint", throwIfNotFound: true);
            _jumpAction   = playerMap.FindAction("Jump",   throwIfNotFound: true);
        }
        else
        {
            Debug.LogWarning("[PlayerMovement] InputActionAsset not assigned in Inspector. " +
                             "Assign Assets/InputSystem_Actions.inputactions to the 'Input Actions Asset' field.");
        }
    }

    private void OnEnable()
    {
        _moveAction?.Enable();
        _sprintAction?.Enable();
        _jumpAction?.Enable();

        // Register jump as an event (fires once per press — prevents hold-to-spam)
        if (_jumpAction != null)
            _jumpAction.performed += OnJumpPerformed;
    }

    private void OnDisable()
    {
        if (_jumpAction != null)
            _jumpAction.performed -= OnJumpPerformed;

        _moveAction?.Disable();
        _sprintAction?.Disable();
        _jumpAction?.Disable();
    }

    // -------------------------------------------------------------------------
    // Input callbacks
    // -------------------------------------------------------------------------

    /// <summary>Called once per Space press (not per frame). Queues a jump.</summary>
    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        if (_isGrounded)
        {
            _jumpRequested = true;
            if (_animator != null)
                _animator.SetTrigger(AnimJump);
        }
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    private void Update()
    {
        if (_controller == null || !_controller.enabled) return;

        UpdateGroundState();
        UpdateHorizontalMovement();
        UpdateVerticalMovement();
        UpdateAnimator();
    }

    // -------------------------------------------------------------------------
    // Ground detection
    // -------------------------------------------------------------------------

    private void UpdateGroundState()
    {
        _isGrounded = _controller.isGrounded;

        // Clamp vertical velocity accumulation when on the ground
        if (_isGrounded && _velocity.y < groundedGravityClamp)
            _velocity.y = groundedGravityClamp;
    }

    // -------------------------------------------------------------------------
    // Horizontal movement (walk / run / diagonal)
    // -------------------------------------------------------------------------

    private void UpdateHorizontalMovement()
    {
        // Read 2D input (WASD / arrows / gamepad stick)
        Vector2 rawInput = _moveAction != null
            ? _moveAction.ReadValue<Vector2>()
            : Vector2.zero;

        // Use camera-relative directions so WASD moves relative to where the
        // camera is pointing, not where the character model is facing.
        Vector3 forward, right;
        if (thirdPersonCamera != null)
        {
            forward = thirdPersonCamera.HorizontalForward;
            right   = thirdPersonCamera.HorizontalRight;
        }
        else
        {
            // Fallback: use player's own facing (original Phase 2 behaviour)
            forward = transform.forward;
            right   = transform.right;
        }

        // rawInput.x = strafe (A/D), rawInput.y = forward/back (W/S)
        Vector3 moveDir = right * rawInput.x + forward * rawInput.y;

        // Clamp to magnitude 1 so diagonal movement isn't faster than cardinal
        if (moveDir.sqrMagnitude > 1f)
            moveDir.Normalize();

        bool isSprinting  = (_sprintAction != null) && _sprintAction.IsPressed();
        float targetSpeed = (moveDir.sqrMagnitude > 0.01f)
            ? (isSprinting ? runSpeed : walkSpeed)
            : 0f;

        // Smooth speed transitions (feels more natural, avoids instant stop)
        _currentSpeed = Mathf.SmoothDamp(_currentSpeed, targetSpeed, ref _speedVelocity, speedSmoothTime);

        // Rotate character to face the movement direction (smooth, not instant)
        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime);
        }

        _controller.Move(moveDir * _currentSpeed * Time.deltaTime);
    }

    // -------------------------------------------------------------------------
    // Vertical movement (jump + gravity)
    // -------------------------------------------------------------------------

    private void UpdateVerticalMovement()
    {
        // Process queued jump
        if (_jumpRequested && _isGrounded)
        {
            // v = sqrt(h * -2 * g)  — standard physics formula
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            _jumpRequested = false;
        }

        // Apply gravity every frame (CharacterController doesn't do this automatically)
        _velocity.y += gravity * Time.deltaTime;

        // Apply vertical displacement
        _controller.Move(_velocity * Time.deltaTime);
    }

    // -------------------------------------------------------------------------
    // Animator
    // -------------------------------------------------------------------------

    private void UpdateAnimator()
    {
        if (_animator == null) return;

        bool isMoving    = _currentSpeed > 0.1f;
        bool isSprinting = isMoving && (_sprintAction != null && _sprintAction.IsPressed());

        _animator.SetBool(AnimIsMoving, isMoving);
        _animator.SetBool(AnimIsWalking, isMoving); // Preserved for backward compatibility
        _animator.SetBool(AnimIsSprinting, isSprinting);
        _animator.SetBool(AnimIsGrounded, _isGrounded);
        _animator.SetFloat(AnimSpeed, _currentSpeed);
        _animator.SetFloat(AnimVerticalVelocity, _velocity.y);
    }
}
