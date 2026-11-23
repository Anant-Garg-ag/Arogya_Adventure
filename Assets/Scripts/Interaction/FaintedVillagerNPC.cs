using UnityEngine;

/// <summary>
/// Controls the first fainted NPC near the rocks for Scene 1: The Heatstroke Remedy.
/// Manages collapsed pose, paste application, juice administration, and physical standing recovery.
/// </summary>
public class FaintedVillagerNPC : MonoBehaviour, IInteractable
{
    [Header("Identity")]
    public string npcName = "Fainted Villager";

    [Header("Visual References")]
    public Transform armatureTransform;
    public Vector3 lyingEuler = new Vector3(270.07f, 290.70f, 0f);
    public Vector3 standingEuler = new Vector3(0f, 290.70f, 0f);

    [Header("State")]
    public bool isRecovered = false;
    public bool hasAppliedPaste = false;

    private void Awake()
    {
        if (armatureTransform == null)
        {
            var arm = GameObject.Find("Scene_1_map/Armature");
            if (arm != null) armatureTransform = arm.transform;
        }

        // Fix any legacy 180 inverted rotation so NPC rests on grass surface
        if (Mathf.Abs(lyingEuler.x - 180f) < 5f)
        {
            lyingEuler = new Vector3(270.07f, 290.70f, 0f);
        }
        if (Mathf.Abs(standingEuler.x - 270f) < 5f)
        {
            standingEuler = new Vector3(0f, 290.70f, 0f);
        }

        SetPoseCollapsed();
    }

    public void SetPoseCollapsed()
    {
        if (armatureTransform != null && !isRecovered)
        {
            armatureTransform.localEulerAngles = lyingEuler;
        }
    }

    public void SetPoseStanding()
    {
        if (armatureTransform != null)
        {
            armatureTransform.localEulerAngles = standingEuler;
        }
    }

    public string GetInteractionPrompt()
    {
        if (isRecovered)
        {
            return $"Talk to {npcName}";
        }

        if (MissionManager.Instance != null)
        {
            int mission = MissionManager.Instance.currentMissionId;

            if (mission == 4 || (InventoryManager.Instance != null && InventoryManager.Instance.HasItem("chandana_paste") && !hasAppliedPaste))
            {
                return "Apply Chandana-Rose Paste";
            }

            if (mission == 6 || (InventoryManager.Instance != null && InventoryManager.Instance.HasItem("sugarcane_juice") && hasAppliedPaste))
            {
                return "Give Fresh Sugarcane Juice";
            }
        }

        return $"Tend to {npcName}";
    }

    public bool CanInteract()
    {
        return true;
    }

    public void Interact(GameObject interactor)
    {
        int mission = MissionManager.Instance != null ? MissionManager.Instance.currentMissionId : 1;

        // --- STEP 1: Initial Discovery ---
        if (mission == 1)
        {
            if (InteractionUI.Instance != null)
            {
                InteractionUI.Instance.ShowNotification(
                    "Villager: 'The sun... burns my head... please, cooling Chandana paste... and sugarcane juice...'",
                    5.0f
                );
            }

            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.SetMission(2);
            }
            return;
        }

        // --- STEPS 2 & 3: Waiting for Paste ---
        if (mission < 4 && !hasAppliedPaste)
        {
            // If player already crafted the paste, proceed to apply
            if (InventoryManager.Instance != null && InventoryManager.Instance.HasItem("chandana_paste"))
            {
                ApplyPaste();
                return;
            }

            if (InteractionUI.Instance != null)
            {
                InteractionUI.Instance.ShowNotification(
                    "Villager: 'Sandalwood in the woods... forest rose... well water... grind them in the mortar... please hurry...'",
                    4.5f
                );
            }
            return;
        }

        // --- STEP 4: Apply Paste ---
        if ((mission == 4 || !hasAppliedPaste) && InventoryManager.Instance != null && InventoryManager.Instance.HasItem("chandana_paste"))
        {
            ApplyPaste();
            return;
        }

        // --- STEP 5: Waiting for Juice ---
        if (mission == 5 || (hasAppliedPaste && !isRecovered))
        {
            // If player already prepared the juice, proceed to give
            if (InventoryManager.Instance != null && InventoryManager.Instance.HasItem("sugarcane_juice"))
            {
                GiveJuice();
                return;
            }

            if (InteractionUI.Instance != null)
            {
                InteractionUI.Instance.ShowNotification(
                    "Villager: 'The cooling paste calmed my fever... but my body is dehydrated... sugarcane juice will restore my fluids...'",
                    4.5f
                );
            }
            return;
        }

        // --- STEP 6: Give Juice & Trigger Full Recovery ---
        if (mission == 6 && InventoryManager.Instance != null && InventoryManager.Instance.HasItem("sugarcane_juice"))
        {
            GiveJuice();
            return;
        }

        // --- STEP 7: Fully Recovered ---
        if (isRecovered)
        {
            if (InteractionUI.Instance != null)
            {
                InteractionUI.Instance.ShowNotification(
                    "Recovered Villager: 'Thank you, healer! Please hurry to help the two other villagers afflicted deeper in the village!'",
                    4.5f
                );
            }
        }
    }

    private void ApplyPaste()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RemoveItem("chandana_paste", 1);
        }

        hasAppliedPaste = true;

        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.ShowNotification(
                "You gently apply the cool Chandana and Rose Water paste across his forehead. His burning fever breaks and his breathing eases!",
                5.0f
            );
        }

        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.SetMission(5);
        }
    }

    private void GiveJuice()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RemoveItem("sugarcane_juice", 1);
        }

        isRecovered = true;
        SetPoseStanding();

        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.ShowNotification(
                "Recovered Villager: 'My strength has returned! But beware—two more villagers have collapsed deeper in the village! Please help them!'",
                6.0f
            );
        }

        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.SetMission(7);
        }
    }
}
