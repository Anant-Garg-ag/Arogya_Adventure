using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Interactable component for NPCs (Village Elder, Villagers, Healers).
/// Shows "Press [E] to Talk to <NPC>" and displays on-screen dialogue feedback upon interaction.
/// </summary>
public class NPCInteractable : MonoBehaviour, IInteractable
{
    [Header("NPC Identity")]
    [Tooltip("Display name of this NPC (e.g. 'Village Elder')")]
    public string npcName = "Village Elder";

    [Tooltip("Action verb shown in the prompt (e.g. 'Talk')")]
    public string promptVerb = "Talk to";

    [Header("Dialogue / Feedback")]
    [TextArea(2, 4)]
    [Tooltip("Sample spoken line shown in the notification banner upon interaction")]
    public string dialogueLine = "Greetings, traveler. A grave affliction has fallen upon Arogya Gram. The well is tainted, and our people are weakening.";

    [Tooltip("Duration in seconds the dialogue banner remains visible")]
    public float displayDuration = 5.0f;

    [Header("Interaction State")]
    [Tooltip("Whether the NPC is currently ready to interact")]
    public bool isInteractable = true;

    [Header("Events")]
    [Tooltip("Event fired when the player talks to this NPC")]
    public UnityEvent<GameObject> onTalk;

    public string GetInteractionPrompt()
    {
        return $"{promptVerb} {npcName}";
    }

    public bool CanInteract()
    {
        return isInteractable;
    }

    public void Interact(GameObject interactor)
    {
        if (!CanInteract()) return;

        // Display visible on-screen dialogue response
        if (InteractionUI.Instance != null && !string.IsNullOrEmpty(dialogueLine))
        {
            InteractionUI.Instance.ShowNotification($"{npcName}: \"{dialogueLine}\"", displayDuration);
        }

        Debug.Log($"[NPCInteractable] Player spoke to {npcName}: \"{dialogueLine}\"");
        onTalk?.Invoke(interactor);
    }
}
