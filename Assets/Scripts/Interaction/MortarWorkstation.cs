using UnityEngine;

/// <summary>
/// Interactive Mortar & Pestle Workstation located in the open village work area.
/// Handles:
/// 1. Ayurvedic Circular Grinding Minigame for Chandana & Rose Water Paste (Step 3).
/// 2. Sugarcane Crushing & Juice extraction into an Empty Glass (Step 5).
/// </summary>
public class MortarWorkstation : MonoBehaviour, IInteractable
{
    [Header("Prompts")]
    public string defaultPrompt = "Examine Mortar & Pestle";
    public string grindPrompt = "Grind Chandana & Rose Water Paste";
    public string juicePrompt = "Crush Sugarcane into Glass";

    public string GetInteractionPrompt()
    {
        if (MissionManager.Instance != null)
        {
            int mission = MissionManager.Instance.currentMissionId;

            // Step 3 or ready to grind paste
            if (mission == 3 || CanCraftPaste())
            {
                return grindPrompt;
            }

            // Step 5 or ready to crush juice
            if (mission == 5 || CanPrepareJuice())
            {
                return juicePrompt;
            }
        }

        return defaultPrompt;
    }

    public bool CanInteract()
    {
        return GrindingMinigameUI.Instance == null || !GrindingMinigameUI.Instance.isMinigameActive;
    }

    public void Interact(GameObject interactor)
    {
        if (!CanInteract()) return;

        if (MissionManager.Instance != null)
        {
            int mission = MissionManager.Instance.currentMissionId;

            // --- FLOW 1: Chandana-Rose Paste Grinding ---
            if (mission == 3 || (mission < 4 && CanCraftPaste()))
            {
                if (CanCraftPaste())
                {
                    StartPasteGrindingMinigame();
                    return;
                }
                else
                {
                    ShowMissingIngredientsMessage(
                        "You need Sandalwood (forest), Forest Rose (forest), and Pure Well Water to grind the cooling paste."
                    );
                    return;
                }
            }

            // --- FLOW 2: Sugarcane Juice Extraction ---
            if (mission == 5 || (mission >= 4 && CanPrepareJuice()))
            {
                if (CanPrepareJuice())
                {
                    PrepareSugarcaneJuice();
                    return;
                }
                else
                {
                    ShowMissingIngredientsMessage(
                        "You need Fresh Sugarcane (lake) and an Empty Glass (work table) to crush and extract sugarcane juice."
                    );
                    return;
                }
            }
        }

        // Generic inspection
        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.ShowNotification(
                "Vaidya's Mortar & Pestle: Used to grind botanical remedies and extract restorative juices.",
                3.5f
            );
        }
    }

    public bool CanCraftPaste()
    {
        if (InventoryManager.Instance == null) return false;
        return InventoryManager.Instance.HasItem("sandalwood")
            && InventoryManager.Instance.HasItem("rose")
            && InventoryManager.Instance.HasItem("well_water");
    }

    public bool CanPrepareJuice()
    {
        if (InventoryManager.Instance == null) return false;
        return InventoryManager.Instance.HasItem("sugarcane")
            && InventoryManager.Instance.HasItem("empty_glass");
    }

    private void StartPasteGrindingMinigame()
    {
        if (GrindingMinigameUI.Instance != null)
        {
            GrindingMinigameUI.Instance.StartGrinding(
                "chandana_paste",
                "Chandana & Rose Water Paste",
                OnPasteGrindingComplete
            );
        }
        else
        {
            // Fallback direct craft if UI component not initialized
            OnPasteGrindingComplete();
        }
    }

    private void OnPasteGrindingComplete()
    {
        if (InventoryManager.Instance != null)
        {
            // Consume raw ingredients
            InventoryManager.Instance.RemoveItem("sandalwood", 1);
            InventoryManager.Instance.RemoveItem("rose", 1);
            InventoryManager.Instance.RemoveItem("well_water", 1);

            // Ensure output paste is in inventory
            if (!InventoryManager.Instance.HasItem("chandana_paste"))
            {
                InventoryManager.Instance.AddItem("chandana_paste", 1);
            }
        }

        // Advance mission to Step 4 (Apply Paste to Villager)
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.SetMission(4);
        }

        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.ShowNotification(
                "Prepared Chandana & Rose Water Paste! Return to the fainted villager to apply it.",
                4.5f
            );
        }
    }

    private void PrepareSugarcaneJuice()
    {
        if (InventoryManager.Instance != null)
        {
            // Consume sugarcane and empty glass
            InventoryManager.Instance.RemoveItem("sugarcane", 1);
            InventoryManager.Instance.RemoveItem("empty_glass", 1);

            // Grant fresh sugarcane juice
            InventoryManager.Instance.AddItem("sugarcane_juice", 1);
        }

        // Advance mission to Step 6 (Revive Villager)
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.SetMission(6);
        }

        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.ShowNotification(
                "Crushed sugarcane and filled the glass with sweet, hydrating juice! Deliver it to the villager.",
                4.5f
            );
        }
    }

    private void ShowMissingIngredientsMessage(string msg)
    {
        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.ShowNotification(msg, 4.0f);
        }
    }
}
