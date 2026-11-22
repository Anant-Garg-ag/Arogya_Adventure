using UnityEngine;

/// <summary>
/// Reusable interface for all interactable objects in Arogya Adventure
/// (NPCs, herbs, well, crafting stations, shrines, doors).
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Action verb or short description displayed in the HUD prompt
    /// (e.g. "Talk", "Inspect", "Harvest", "Brew", "Drink").
    /// </summary>
    string GetInteractionPrompt();

    /// <summary>
    /// Whether this object can currently be interacted with.
    /// Returns false if already collected, dialogue in progress, or conditions not met.
    /// </summary>
    bool CanInteract();

    /// <summary>
    /// Invoked when the player presses the interaction key (E / Gamepad North).
    /// </summary>
    /// <param name="interactor">The player GameObject initiating the interaction.</param>
    void Interact(GameObject interactor);
}
