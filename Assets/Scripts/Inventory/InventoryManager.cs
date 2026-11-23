using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton Inventory Manager for Arogya Adventure.
/// Stores item quantities in a dictionary keyed by item ID.
/// Notifies listeners (HUD, Quest/Mission Manager) on item changes.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    // Internal inventory storage: itemId -> quantity
    private readonly Dictionary<string, int> _items = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Event fired whenever any item quantity changes (itemId, newTotalQuantity).
    /// </summary>
    public event Action<string, int> OnInventoryChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureInstanceExists()
    {
        if (Instance == null && UnityEngine.Object.FindFirstObjectByType<InventoryManager>() == null)
        {
            var go = new GameObject("[InventoryManager]");
            go.AddComponent<InventoryManager>();
            DontDestroyOnLoad(go);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Adds a quantity of an item to the inventory.
    /// </summary>
    public bool AddItem(string itemId, int quantity = 1)
    {
        if (string.IsNullOrEmpty(itemId) || quantity <= 0) return false;

        if (_items.ContainsKey(itemId))
        {
            _items[itemId] += quantity;
        }
        else
        {
            _items[itemId] = quantity;
        }

        int newTotal = _items[itemId];
        Debug.Log($"[InventoryManager] Added {quantity}x '{itemId}'. New total: {newTotal}");

        OnInventoryChanged?.Invoke(itemId, newTotal);
        return true;
    }

    /// <summary>
    /// Removes a quantity of an item from the inventory. Returns false if insufficient quantity.
    /// </summary>
    public bool RemoveItem(string itemId, int quantity = 1)
    {
        if (string.IsNullOrEmpty(itemId) || quantity <= 0) return false;
        if (!_items.ContainsKey(itemId) || _items[itemId] < quantity) return false;

        _items[itemId] -= quantity;
        int newTotal = _items[itemId];

        if (newTotal <= 0)
        {
            _items.Remove(itemId);
            newTotal = 0;
        }

        Debug.Log($"[InventoryManager] Removed {quantity}x '{itemId}'. Remaining: {newTotal}");
        OnInventoryChanged?.Invoke(itemId, newTotal);
        return true;
    }

    /// <summary>
    /// Returns the current quantity of the given item ID.
    /// </summary>
    public int GetItemCount(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return 0;
        return _items.TryGetValue(itemId, out int count) ? count : 0;
    }

    /// <summary>
    /// Checks if the inventory contains at least the specified quantity of an item.
    /// </summary>
    public bool HasItem(string itemId, int requiredQuantity = 1)
    {
        return GetItemCount(itemId) >= requiredQuantity;
    }

    /// <summary>
    /// Returns a snapshot of all non-empty items in inventory.
    /// </summary>
    public IReadOnlyDictionary<string, int> GetAllItems()
    {
        return _items;
    }

    /// <summary>
    /// Clears all inventory contents (useful for resets or testing).
    /// </summary>
    public void ClearInventory()
    {
        var keys = new List<string>(_items.Keys);
        _items.Clear();
        foreach (var key in keys)
        {
            OnInventoryChanged?.Invoke(key, 0);
        }
    }
}
