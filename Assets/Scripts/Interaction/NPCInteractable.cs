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

    [Header("Mission Dialogue Link (Village Elder)")]
    [Tooltip("If true, evaluates MissionManager and displays progression dialogue")]
    public bool isVillageElder = true;

    [TextArea(2, 4)]
    public string mission1IncompleteDialogue = "I am worried about the sickness spreading through our village. Please bring me three Tulsi leaves from near the stream.";

    [TextArea(2, 4)]
    public string mission1CompleteDialogue = "Thank you. The Tulsi will help us prepare a remedy. Now, I need you to investigate the village well. Something is wrong with its water.";

    [TextArea(2, 4)]
    public string mission2ActiveDialogue = "Please inspect the village well. We must discover why its water is tainted.";

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

        string spokenLine = dialogueLine;

        // Dynamic mission evaluation for the Village Elder
        if (isVillageElder && MissionManager.Instance != null)
        {
            if (MissionManager.Instance.currentMissionId == 1)
            {
                if (MissionManager.Instance.IsMission1ReadyForTurnIn())
                {
                    spokenLine = mission1CompleteDialogue;
                    MissionManager.Instance.CompleteCurrentMissionAndAdvance();
                }
                else
                {
                    spokenLine = mission1IncompleteDialogue;
                }
            }
            else if (MissionManager.Instance.currentMissionId == 2)
            {
                spokenLine = mission2ActiveDialogue;
            }
        }

        // Display visible on-screen dialogue response
        if (InteractionUI.Instance != null && !string.IsNullOrEmpty(spokenLine))
        {
            InteractionUI.Instance.ShowNotification($"{npcName}: \"{spokenLine}\"", displayDuration);
        }

        Debug.Log($"[NPCInteractable] Player spoke to {npcName}: \"{spokenLine}\"");
        onTalk?.Invoke(interactor);
    }
}
