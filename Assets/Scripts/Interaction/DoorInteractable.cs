using System.Collections;
using UnityEngine;

/// <summary>
/// Reusable interactive door component implementing IInteractable.
/// Handles smooth rotation around a hinge pivot and dynamically updates interaction prompts.
/// </summary>
public class DoorInteractable : MonoBehaviour, IInteractable
{
    [Header("Door State")]
    [Tooltip("Is the door currently open?")]
    public bool isOpen = false;

    [Tooltip("Is the door locked?")]
    public bool isLocked = false;

    [Tooltip("Required item ID in inventory to unlock (leave empty if not locked)")]
    public string requiredKeyItemId = "";

    [Header("Rotation Settings")]
    [Tooltip("Target rotation angle when open (relative to initial closed rotation, in degrees)")]
    public float openAngle = 90.0f;

    [Tooltip("Local axis around which the hinge rotates. Defaults to Vector3.up (0,1,0). Use Vector3.forward (0,0,1) if parent is rotated -90 on X.")]
    public Vector3 rotationAxis = Vector3.up;

    [Tooltip("Speed of opening/closing transition in seconds")]
    public float transitionDuration = 0.8f;

    [Tooltip("Transform acting as the hinge pivot. Defaults to this transform.")]
    public Transform hingeTransform;

    [Header("Prompts & Messages")]
    public string openPrompt = "Open Door";
    public string closePrompt = "Close Door";
    public string lockedPrompt = "Door is Locked";

    private Quaternion _closedRotation;
    private Quaternion _openRotation;
    private Coroutine _transitionCoroutine;
    private bool _isTransitioning;

    private void Awake()
    {
        if (hingeTransform == null)
        {
            hingeTransform = transform;
        }

        _closedRotation = hingeTransform.localRotation;
        Vector3 axis = rotationAxis != Vector3.zero ? rotationAxis.normalized : Vector3.up;
        _openRotation = _closedRotation * Quaternion.AngleAxis(openAngle, axis);

        if (isOpen)
        {
            hingeTransform.localRotation = _openRotation;
        }
    }

    public string GetInteractionPrompt()
    {
        if (isLocked)
        {
            if (!string.IsNullOrEmpty(requiredKeyItemId) && InventoryManager.Instance != null && InventoryManager.Instance.HasItem(requiredKeyItemId))
            {
                return $"Unlock with {ItemData.GetDisplayName(requiredKeyItemId)}";
            }
            return lockedPrompt;
        }

        return isOpen ? closePrompt : openPrompt;
    }

    public bool CanInteract()
    {
        return !_isTransitioning;
    }

    public void Interact(GameObject interactor)
    {
        if (_isTransitioning) return;

        // Check if locked and player has key
        if (isLocked)
        {
            if (!string.IsNullOrEmpty(requiredKeyItemId) && InventoryManager.Instance != null && InventoryManager.Instance.HasItem(requiredKeyItemId))
            {
                isLocked = false;
                if (InteractionUI.Instance != null)
                {
                    InteractionUI.Instance.ShowNotification($"Unlocked door with {ItemData.GetDisplayName(requiredKeyItemId)}!", 3.0f);
                }
            }
            else
            {
                if (InteractionUI.Instance != null)
                {
                    InteractionUI.Instance.ShowNotification("The door is locked firmly.", 2.5f);
                }
                return;
            }
        }

        // Toggle open/closed
        isOpen = !isOpen;
        Quaternion targetRotation = isOpen ? _openRotation : _closedRotation;

        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
        }

        _transitionCoroutine = StartCoroutine(AnimateDoor(targetRotation));

        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.Show(GetInteractionPrompt());
        }
    }

    private IEnumerator AnimateDoor(Quaternion targetRotation)
    {
        _isTransitioning = true;
        Quaternion startRotation = hingeTransform.localRotation;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            // Smooth step interpolation
            t = t * t * (3f - 2f * t);

            hingeTransform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        hingeTransform.localRotation = targetRotation;
        _isTransitioning = false;
        _transitionCoroutine = null;

        // Refresh UI prompt after settling
        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.Show(GetInteractionPrompt());
        }
    }
}
