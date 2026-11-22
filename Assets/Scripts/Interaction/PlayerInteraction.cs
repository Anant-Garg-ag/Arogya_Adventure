using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles detecting nearby IInteractable objects and invoking interaction via input (E / Gamepad).
/// Attached to the Player GameObject. Operates alongside PlayerMovement without interfering with it.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector settings
    // -------------------------------------------------------------------------

    [Header("Input")]
    [Tooltip("Assign Assets/InputSystem_Actions.inputactions here")]
    public InputActionAsset inputActionsAsset;

    [Header("Detection Settings")]
    [Tooltip("Maximum distance from the player to detect interactables (world units)")]
    public float interactionDistance = 3.5f;

    [Tooltip("SphereCast radius for detection thickness")]
    public float interactionRadius = 0.8f;

    [Tooltip("Vertical height offset from player base for the detection origin (chest height)")]
    public float originHeightOffset = 2.5f;

    [Tooltip("Minimum dot product between player forward and direction to interactable (cone filter)")]
    [Range(0f, 1f)]
    public float forwardAngleThreshold = 0.3f;

    [Tooltip("Layer mask for interactable objects (default: Everything)")]
    public LayerMask interactableLayers = ~0;

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    private InputAction _interactAction;
    private IInteractable _currentInteractable;
    private Collider _currentCollider;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        // Auto-inherit input asset from PlayerMovement if not explicitly assigned
        if (inputActionsAsset == null)
        {
            var pm = GetComponent<PlayerMovement>();
            if (pm != null && pm.inputActionsAsset != null)
            {
                inputActionsAsset = pm.inputActionsAsset;
            }
        }

        if (inputActionsAsset != null)
        {
            var playerMap = inputActionsAsset.FindActionMap("Player", throwIfNotFound: true);
            _interactAction = playerMap.FindAction("Interact", throwIfNotFound: true);
        }
        else
        {
            Debug.LogWarning("[PlayerInteraction] InputActionAsset not assigned. Assign Assets/InputSystem_Actions.inputactions.");
        }
    }

    private void OnEnable()
    {
        if (_interactAction != null)
        {
            _interactAction.performed += OnInteractPerformed;
            _interactAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (_interactAction != null)
        {
            _interactAction.performed -= OnInteractPerformed;
            _interactAction.Disable();
        }

        ClearCurrentInteractable();
    }

    private void Update()
    {
        DetectInteractable();
    }

    // -------------------------------------------------------------------------
    // Detection logic
    // -------------------------------------------------------------------------

    private void DetectInteractable()
    {
        Vector3 origin = transform.position + Vector3.up * originHeightOffset;
        Vector3 forward = transform.forward;

        IInteractable candidate = null;
        Collider candidateCollider = null;

        // 1. First attempt: Direct forward SphereCast
        if (Physics.SphereCast(origin, interactionRadius, forward, out RaycastHit hit, interactionDistance, interactableLayers, QueryTriggerInteraction.Collide))
        {
            candidate = hit.collider.GetComponentInParent<IInteractable>();
            if (candidate != null && candidate.CanInteract())
            {
                candidateCollider = hit.collider;
            }
            else
            {
                candidate = null;
            }
        }

        // 2. Second attempt: OverlapSphere proximity check with forward cone filter
        // Handles cases where player is standing very close or slightly angled
        if (candidate == null)
        {
            Collider[] colliders = Physics.OverlapSphere(origin, interactionDistance, interactableLayers, QueryTriggerInteraction.Collide);
            float closestDist = float.MaxValue;

            foreach (var col in colliders)
            {
                // Skip self colliders
                if (col.transform.IsChildOf(transform)) continue;

                var interactable = col.GetComponentInParent<IInteractable>();
                if (interactable == null || !interactable.CanInteract()) continue;

                Vector3 targetPoint = col.bounds.center;
                Vector3 toTarget = targetPoint - origin;
                float dist = toTarget.magnitude;

                if (dist > interactionDistance) continue;

                Vector3 dirToTarget = toTarget.normalized;
                float dot = Vector3.Dot(forward, dirToTarget);

                // Check if target is in front of the player
                if (dot >= forwardAngleThreshold)
                {
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        candidate = interactable;
                        candidateCollider = col;
                    }
                }
            }
        }

        // Update current interactable state and UI
        if (candidate != _currentInteractable)
        {
            _currentInteractable = candidate;
            _currentCollider = candidateCollider;

            if (_currentInteractable != null && _currentInteractable.CanInteract())
            {
                if (InteractionUI.Instance != null)
                {
                    InteractionUI.Instance.Show(_currentInteractable.GetInteractionPrompt());
                }
            }
            else
            {
                if (InteractionUI.Instance != null)
                {
                    InteractionUI.Instance.Hide();
                }
            }
        }
        else if (_currentInteractable != null)
        {
            // If still pointing at same interactable, check if CanInteract became false
            if (!_currentInteractable.CanInteract())
            {
                ClearCurrentInteractable();
            }
        }
    }

    private void ClearCurrentInteractable()
    {
        _currentInteractable = null;
        _currentCollider = null;

        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.Hide();
        }
    }

    // -------------------------------------------------------------------------
    // Interaction execution
    // -------------------------------------------------------------------------

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if (_currentInteractable != null && _currentInteractable.CanInteract())
        {
            _currentInteractable.Interact(gameObject);

            // Re-check after interaction (prompt text or CanInteract may have changed)
            if (_currentInteractable != null && _currentInteractable.CanInteract())
            {
                if (InteractionUI.Instance != null)
                {
                    InteractionUI.Instance.Show(_currentInteractable.GetInteractionPrompt());
                }
            }
            else
            {
                ClearCurrentInteractable();
            }
        }
    }

    // -------------------------------------------------------------------------
    // Gizmos
    // -------------------------------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position + Vector3.up * originHeightOffset;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin, interactionRadius);
        Gizmos.DrawLine(origin, origin + transform.forward * interactionDistance);
        Gizmos.DrawWireSphere(origin + transform.forward * interactionDistance, interactionRadius);
    }
}
