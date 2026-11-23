using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data definition for items in Arogya Adventure.
/// </summary>
[System.Serializable]
public class ItemData
{
    public string itemId;
    public string displayName;
    public string description;
    public string category;

    public ItemData(string id, string name, string desc, string cat = "Herb")
    {
        itemId = id;
        displayName = name;
        description = desc;
        category = cat;
    }

    // Static registry of standard game items for quick access and display fallback
    public static readonly Dictionary<string, ItemData> KnownItems = new Dictionary<string, ItemData>(StringComparer.OrdinalIgnoreCase)
    {
        { "tulsi", new ItemData("tulsi", "Tulsi Leaf", "Sacred holy basil with powerful healing and restorative properties.", "Medicinal Herb") },
        { "pure_water", new ItemData("pure_water", "Pure Spring Water", "Untainted mountain spring water drawn from the highland source.", "Quest Item") },
        { "ashwagandha", new ItemData("ashwagandha", "Ashwagandha Root", "Medicinal root known to restore vigor and vitality.", "Medicinal Herb") },
        { "neem", new ItemData("neem", "Neem Leaf", "Potent bitter herb with strong cleansing and purifying properties.", "Medicinal Herb") },
        { "sanjeevani_kadha", new ItemData("sanjeevani_kadha", "Sanjeevani Kadha", "The ancient medicinal decoction capable of curing the village affliction.", "Medicine") }
    };

    public static string GetDisplayName(string id)
    {
        if (string.IsNullOrEmpty(id)) return "Unknown Item";
        if (KnownItems.TryGetValue(id, out var data)) return data.displayName;
        return id;
    }
}
