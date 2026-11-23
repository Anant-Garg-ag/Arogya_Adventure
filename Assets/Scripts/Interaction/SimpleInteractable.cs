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

    [Header("Mission Link (Village Well)")]
    [Tooltip("If true, coordinates with MissionManager for Well progression")]
    public bool isVillageWell = false;

    [TextArea(2, 4)]
    public string mission1ActiveMessage = "The well water is stagnant and murky. The Village Elder needs the medicinal Tulsi leaves before we can investigate properly.";

    [TextArea(2, 4)]
    public string mission2InvestigationMessage = "You examine the well closely: A toxic dark residue coats the stones. The water source is contaminated! (Mission 2 Completed)";

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

        string messageToDisplay = responseMessage;

        // Dynamic mission evaluation for the Village Well
        if (isVillageWell && MissionManager.Instance != null)
        {
            if (MissionManager.Instance.currentMissionId == 1)
            {
                messageToDisplay = mission1ActiveMessage;
            }
            else if (MissionManager.Instance.currentMissionId == 2)
            {
                messageToDisplay = mission2InvestigationMessage;
                MissionManager.Instance.CompleteCurrentMissionAndAdvance();
            }
        }

        // Display visible on-screen confirmation
        if (InteractionUI.Instance != null && !string.IsNullOrEmpty(messageToDisplay))
        {
            InteractionUI.Instance.ShowNotification(messageToDisplay, responseDuration);
        }

        Debug.Log($"[SimpleInteractable] Interacted with '{gameObject.name}': {messageToDisplay} (count: {_interactionCount})");

        onInteract?.Invoke(interactor);

        if (!isRepeatable)
        {
            isInteractable = false;
        }
    }
}
