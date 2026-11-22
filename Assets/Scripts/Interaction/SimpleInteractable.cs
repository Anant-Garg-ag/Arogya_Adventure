using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// General-purpose interactable component for static objects, props, shrines, and triggers.
/// Supports inspector prompt configuration, visible on-screen response text, and UnityEvent execution.
/// </summary>
public class SimpleInteractable : MonoBehaviour, IInteractable
{
    [Header("Prompt Settings")]
    [Tooltip("Prompt verb or text shown in the HUD (e.g. 'Inspect', 'Drink', 'Open')")]
    public string promptText = "Inspect";

    [Header("Interaction Response")]
    [TextArea(2, 4)]
    [Tooltip("Message displayed on screen when interacted with (e.g. 'The well water is contaminated.')")]
    public string responseMessage = "The well water is stagnant and murky. It cannot be used safely.";

    [Tooltip("How long the confirmation message stays on screen")]
    public float responseDuration = 4.0f;

    [Header("Interaction Behavior")]
    [Tooltip("Whether this object can be interacted with multiple times")]
    public bool isRepeatable = true;

    [Tooltip("Whether the object is currently active and can be interacted with")]
    public bool isInteractable = true;

    [Header("Events")]
    [Tooltip("Event invoked when interaction occurs")]
    public UnityEvent<GameObject> onInteract;

    private int _interactionCount;

    public string GetInteractionPrompt()
    {
        return promptText;
    }

    public bool CanInteract()
    {
        if (!isInteractable) return false;
        if (!isRepeatable && _interactionCount > 0) return false;
        return true;
    }

    public void Interact(GameObject interactor)
    {
        if (!CanInteract()) return;

        _interactionCount++;

        // Display visible on-screen confirmation
        if (InteractionUI.Instance != null && !string.IsNullOrEmpty(responseMessage))
        {
            InteractionUI.Instance.ShowNotification(responseMessage, responseDuration);
        }

        Debug.Log($"[SimpleInteractable] Interacted with '{gameObject.name}': {responseMessage} (count: {_interactionCount})");

        onInteract?.Invoke(interactor);

        if (!isRepeatable)
        {
            isInteractable = false;
        }
    }
}
