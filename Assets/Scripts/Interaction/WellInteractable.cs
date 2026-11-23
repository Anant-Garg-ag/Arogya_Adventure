using UnityEngine;

/// <summary>
/// Interactive Well component implementing IInteractable.
/// Allows the player to draw pure water for remedy preparation.
/// </summary>
public class WellInteractable : MonoBehaviour, IInteractable
{
    [Header("Prompts")]
    public string promptText = "Draw Well Water";
    public string responseMessage = "Drew cool, pure water from the village well.";

    public string GetInteractionPrompt()
    {
        return promptText;
    }

    public bool CanInteract()
    {
        return true;
    }

    public void Interact(GameObject interactor)
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem("well_water", 1);
        }

        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.ShowNotification(responseMessage, 3.5f);
        }

        Debug.Log("[WellInteractable] Player drew well water.");
    }
}
