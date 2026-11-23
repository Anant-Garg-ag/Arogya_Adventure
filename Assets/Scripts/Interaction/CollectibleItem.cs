using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Reusable collectible item component implementing IInteractable.
/// Used for medicinal herbs, water vessels, and quest collectibles.
/// On interaction, displays an on-screen notification (e.g. "Item collected: Tulsi Leaf"),
/// invokes events, and disables/destroys the collectible so it can only be gathered once.
/// </summary>
public class CollectibleItem : MonoBehaviour, IInteractable
{
    [Header("Item Details")]
    [Tooltip("Unique identifier used by the inventory and mission systems (e.g. 'tulsi', 'pure_water')")]
    public string itemId = "tulsi";

    [Tooltip("Quantity added to inventory on collection")]
    public int quantity = 1;

    [Tooltip("Display name of this collectible item")]
    public string itemName = "Tulsi Leaf (Medicinal Herb)";

    [Tooltip("Action verb displayed in the prompt (e.g. 'Collect', 'Harvest', 'Pick up')")]
    public string promptVerb = "Collect";

    [Header("Behavior")]
    [Tooltip("Duration in seconds the collection notification remains on screen")]
    public float notificationDuration = 3.5f;

    [Tooltip("If true, disables the GameObject upon collection. If false, disables the collider and renderer.")]
    public bool disableGameObject = true;

    [Header("Events")]
    [Tooltip("Fired when this item is collected")]
    public UnityEvent<GameObject> onCollected;

    private bool _isCollected;

    public string GetInteractionPrompt()
    {
        return $"{promptVerb} {itemName}";
    }

    public bool CanInteract()
    {
        return !_isCollected;
    }

    public void Interact(GameObject interactor)
    {
        if (_isCollected) return;

        _isCollected = true;

        // 1. Add to Inventory Manager
        if (InventoryManager.Instance != null && !string.IsNullOrEmpty(itemId))
        {
            InventoryManager.Instance.AddItem(itemId, quantity);
        }

        // 2. Show visible on-screen confirmation banner
        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.ShowNotification($"Item Collected: {itemName}", notificationDuration);
        }

        Debug.Log($"[CollectibleItem] Player collected '{itemName}' from {gameObject.name}!");

        // 2. Fire collection event
        onCollected?.Invoke(interactor);

        // 3. Deactivate or disable so it cannot be collected again
        if (disableGameObject)
        {
            gameObject.SetActive(false);
        }
        else
        {
            var col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            var renderer = GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
        }
    }
}
