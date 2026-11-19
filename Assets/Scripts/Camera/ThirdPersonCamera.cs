using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Free-orbit third-person camera for Arogya Adventure.
///
/// Design constraints:
///   - Main Camera is reparented to scene root (no longer a child of Player).
///   - Pivot is computed from CharacterController.transform.TransformPoint(cc.center)
///     so the camera orbits the actual capsule centre, not the Player root or mesh.
///   - MouseLook.cs must be DISABLED (not deleted) before this component runs.
///   - Look input comes from the InputActionAsset "Player/Look" action
///     (<Pointer>/delta = per-frame pixel delta — do NOT multiply by Time.deltaTime).
///   - SphereCast from pivot outward prevents camera clipping through geometry.
///   - Camera scale is reset to (1,1,1) on Awake (original was negative-X).
/// </summary>
[RequireComponent(typeof(Camera))]
public class ThirdPersonCamera : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector fields
    // -------------------------------------------------------------------------

    [Header("References")]
    [Tooltip("Assign Assets/InputSystem_Actions.inputactions here")]
    public InputActionAsset inputActionsAsset;

    [Tooltip("The CharacterController on the Player GameObject")]
    public CharacterController playerController;

    [Header("Mouse Look")]
    [Tooltip("Mouse sensitivity (pixels → degrees, no Time.deltaTime needed)")]
    public float mouseSensitivity = 0.2f;

    [Tooltip("Minimum vertical orbit angle (looking down)")]
    public float minVerticalAngle = -30f;

    [Tooltip("Maximum vertical orbit angle (looking up)")]
    public float maxVerticalAngle = 70f;

    [Header("Camera Position")]
    [Tooltip("Distance from the camera pivot when unobstructed")]
    public float cameraDistance = 5f;

    [Tooltip("Additional height offset added to the pivot (CharacterController centre)")]
    public float heightOffset = 0.5f;

    [Tooltip("How quickly the camera catches up to the player (SmoothDamp time)")]
    public float followSmoothTime = 0.08f;

    [Header("Collision")]
    [Tooltip("SphereCast radius for camera collision detection")]
    public float collisionRadius = 0.25f;

    [Tooltip("Layers the camera collides with (default: Everything)")]
    public LayerMask collisionLayers = ~0;

    [Tooltip("Minimum camera distance when fully obstructed")]
    public float minCameraDistance = 0.5f;

    [Tooltip("Speed at which camera returns to full distance after obstruction clears")]
    public float collisionReturnSpeed = 4f;

    [Header("Character Rotation")]
    [Tooltip("Speed at which the character rotates to face the movement direction")]
    public float characterRotationSpeed = 15f;

    // -------------------------------------------------------------------------
    // Public read-only accessors for PlayerMovement camera-relative directions
    // -------------------------------------------------------------------------

    /// <summary>Camera's forward projected onto the XZ plane (normalised).</summary>
    public Vector3 HorizontalForward
    {
        get
        {
            Vector3 f = transform.forward;
            f.y = 0f;
            return f.sqrMagnitude > 0.001f ? f.normalized : Vector3.forward;
        }
    }

    /// <summary>Camera's right projected onto the XZ plane (normalised).</summary>
    public Vector3 HorizontalRight
    {
        get
        {
            Vector3 r = transform.right;
            r.y = 0f;
            return r.sqrMagnitude > 0.001f ? r.normalized : Vector3.right;
        }
    }

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private InputAction _lookAction;

    private float _yaw;           // current horizontal angle around player
    private float _pitch;         // current vertical angle

    private float _currentDistance;      // actual camera distance (may shrink on collision)
    private Vector3 _pivotVelocity;      // used by SmoothDamp for pivot follow

    private Vector3 _smoothedPivot;      // smoothed pivot world position

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        // Fix negative X scale that was left on the camera from the original setup
        transform.localScale = Vector3.one;

        _currentDistance = cameraDistance;

        // Initialise yaw from current camera facing so it doesn't snap on start
        Vector3 toCamera = transform.position - GetPivot();
        if (toCamera.sqrMagnitude > 0.001f)
        {
            _yaw   = Mathf.Atan2(toCamera.x, toCamera.z) * Mathf.Rad2Deg;
            _pitch = Mathf.Asin(Mathf.Clamp(toCamera.normalized.y, -1f, 1f)) * Mathf.Rad2Deg;
            _pitch = Mathf.Clamp(_pitch, minVerticalAngle, maxVerticalAngle);
        }
        else
        {
            _yaw   = 0f;
            _pitch = 15f; // slight downward default
        }

        _smoothedPivot = GetPivot();

        // Wire up input
        if (inputActionsAsset != null)
        {
            var playerMap = inputActionsAsset.FindActionMap("Player", throwIfNotFound: true);
            _lookAction   = playerMap.FindAction("Look", throwIfNotFound: true);
        }
        else
        {
            Debug.LogWarning("[ThirdPersonCamera] InputActionAsset not assigned. Assign in Inspector.");
        }

        // Ensure cursor is locked for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    private void OnEnable()
    {
        _lookAction?.Enable();
    }

    private void OnDisable()
    {
        _lookAction?.Disable();
    }

    /// <summary>
    /// All camera positioning happens in LateUpdate so it runs after
    /// PlayerMovement.Update() has already moved the character this frame.
    /// </summary>
    private void LateUpdate()
    {
        if (playerController == null) return;

        ReadLookInput();
        UpdatePivot();
        PositionCamera();
    }

    // -------------------------------------------------------------------------
    // Input
    // -------------------------------------------------------------------------

    private void ReadLookInput()
    {
        if (_lookAction == null) return;

        // <Pointer>/delta gives pixel delta per frame — NOT per second.
        // Multiplying by sensitivity converts to degrees of orbit per frame.
        // Do NOT multiply by Time.deltaTime here.
        Vector2 delta = _lookAction.ReadValue<Vector2>();

        _yaw   += delta.x * mouseSensitivity;
        _pitch -= delta.y * mouseSensitivity;   // subtract so mouse-up = look up
        _pitch  = Mathf.Clamp(_pitch, minVerticalAngle, maxVerticalAngle);
    }

    // -------------------------------------------------------------------------
    // Pivot follow
    // -------------------------------------------------------------------------

    /// <summary>Returns the world-space orbit pivot (CC capsule centre + height offset).</summary>
    private Vector3 GetPivot()
    {
        if (playerController == null) return Vector3.zero;

        // TransformPoint converts local CharacterController.center → world space.
        // This handles arbitrary Player position and rotation, including the
        // manually-offset CharacterController.center that compensates for the
        // mesh being locally offset from the Player root.
        Vector3 ccCenter = playerController.transform.TransformPoint(playerController.center);
        return ccCenter + Vector3.up * heightOffset;
    }

    private void UpdatePivot()
    {
        Vector3 targetPivot = GetPivot();
        _smoothedPivot = Vector3.SmoothDamp(
            _smoothedPivot,
            targetPivot,
            ref _pivotVelocity,
            followSmoothTime);
    }

    // -------------------------------------------------------------------------
    // Camera positioning + collision
    // -------------------------------------------------------------------------

    private void PositionCamera()
    {
        // Compute desired camera position from yaw/pitch/distance
        Quaternion rotation     = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3    desiredOffset = rotation * Vector3.back * cameraDistance;
        Vector3    desiredPos    = _smoothedPivot + desiredOffset;

        // --- Collision: SphereCast from pivot toward desired camera position ---
        float actualDistance = cameraDistance;
        Vector3 castDir = (desiredPos - _smoothedPivot).normalized;
        float   castLen = cameraDistance - collisionRadius;

        if (castLen > 0f)
        {
            if (Physics.SphereCast(
                    _smoothedPivot,
                    collisionRadius,
                    castDir,
                    out RaycastHit hit,
                    castLen,
                    collisionLayers,
                    QueryTriggerInteraction.Ignore))
            {
                // Pull camera in to just before the hit surface
                actualDistance = Mathf.Clamp(hit.distance + collisionRadius, minCameraDistance, cameraDistance);
            }
        }

        // Smoothly restore distance when obstruction clears
        _currentDistance = Mathf.Lerp(_currentDistance, actualDistance, collisionReturnSpeed * Time.deltaTime);

        // Apply final position
        Vector3 finalPos = _smoothedPivot + rotation * Vector3.back * _currentDistance;
        transform.position = finalPos;

        // Always look at the smoothed pivot
        transform.LookAt(_smoothedPivot, Vector3.up);

        // Handle Escape to free the cursor during Editor testing
#if UNITY_EDITOR
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }
#endif
    }
}
