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
        // Prototype / Baseline Items
        { "tulsi", new ItemData("tulsi", "Tulsi Leaf", "Sacred holy basil with powerful healing and restorative properties.", "Medicinal Herb") },
        { "pure_water", new ItemData("pure_water", "Pure Spring Water", "Untainted mountain spring water drawn from the highland source.", "Quest Item") },
        { "ashwagandha", new ItemData("ashwagandha", "Ashwagandha Root", "Medicinal root known to restore vigor and vitality.", "Medicinal Herb") },
        { "neem", new ItemData("neem", "Neem Leaf", "Potent bitter herb with strong cleansing and purifying properties.", "Medicinal Herb") },
        { "sanjeevani_kadha", new ItemData("sanjeevani_kadha", "Sanjeevani Kadha", "The ancient medicinal decoction capable of curing the village affliction.", "Medicine") },

        // Scene 1: The Heatstroke Remedy Items
        // Raw Ingredients
        { "sandalwood", new ItemData("sandalwood", "Sandalwood Wood", "Fragrant cooling wood from the forest, renowned in Ayurveda for reducing Pitta and bodily heat.", "Raw Ingredient") },
        { "rose", new ItemData("rose", "Forest Rose", "Delicate wild roses rich in cooling essence and fragrant moisture.", "Raw Ingredient") },
        { "well_water", new ItemData("well_water", "Pure Well Water", "Cool water drawn fresh from the village well, ideal for preparing herbal pastes.", "Raw Ingredient") },
        { "sugarcane", new ItemData("sugarcane", "Fresh Sugarcane", "Sweet, hydrating sugarcane stalks gathered near the water, rich in electrolytes.", "Raw Ingredient") },
        { "empty_glass", new ItemData("empty_glass", "Empty Glass", "A clean glass vessel ready to hold freshly prepared botanical juice.", "Utensil") },

        // Processed Remedies
        { "chandana_paste", new ItemData("chandana_paste", "Chandana & Rose Water Paste", "A finely ground cooling paste of Sandalwood and Rose Water, prescribed to pacify severe heatstroke and fever.", "Processed Remedy") },
        { "sugarcane_juice", new ItemData("sugarcane_juice", "Fresh Sugarcane Juice", "Sweet, cold-pressed sugarcane juice that swiftly rehydrates the body and restores vital prana.", "Processed Remedy") }
    };

    public bool IsRawIngredient()
    {
        return category == "Raw Ingredient" || category == "Medicinal Herb";
    }

    public bool IsProcessedRemedy()
    {
        return category == "Processed Remedy" || category == "Medicine";
    }

    public static string GetDisplayName(string id)
    {
        if (string.IsNullOrEmpty(id)) return "Unknown Item";
        if (KnownItems.TryGetValue(id, out var data)) return data.displayName;
        return id;
    }
}
