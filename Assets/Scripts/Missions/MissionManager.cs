using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data container for a single gameplay mission.
/// </summary>
[System.Serializable]
public class MissionInfo
{
    public int missionId;
    public string title;
    [TextArea(2, 4)]
    public string description;
    public string targetItemId;
    public int requiredItemCount;
    public bool isCompleted;

    public MissionInfo(int id, string title, string desc, string targetItem = "", int reqCount = 0)
    {
        this.missionId = id;
        this.title = title;
        this.description = desc;
        this.targetItemId = targetItem;
        this.requiredItemCount = reqCount;
        this.isCompleted = false;
    }
}

/// <summary>
/// Singleton Mission State Manager for Arogya Adventure.
/// Coordinates quest progression across Missions 1 to 6, handles objective tracking,
/// and fires events for HUD and dialogue synchronization.
/// </summary>
public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    [Header("Current State")]
    [Tooltip("Active mission number (1 to 6)")]
    public int currentMissionId = 1;

    public event Action<int, string, string> OnMissionStarted;
    public event Action<int, int, string> OnObjectiveUpdated;
    public event Action<int> OnMissionCompleted;

    private readonly Dictionary<int, MissionInfo> _missions = new Dictionary<int, MissionInfo>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureInstanceExists()
    {
        if (Instance == null && UnityEngine.Object.FindFirstObjectByType<MissionManager>() == null)
        {
            var go = new GameObject("[MissionManager]");
            go.AddComponent<MissionManager>();
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
        InitializeMissions();
    }

    private void Start()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += HandleInventoryChanged;
        }

        NotifyCurrentMissionState();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= HandleInventoryChanged;
        }
    }

    private void InitializeMissions()
    {
        _missions.Clear();

        // Scene 1: The Heatstroke Remedy (Authoritative Story Sequence from Entire_Plot.docx)
        // Step 1: Starting NPC discovery
        _missions[1] = new MissionInfo(
            1,
            "The Heatstroke Victim",
            "Examine the collapsed villager lying near the rocks by the village entrance.",
            "",
            0
        );

        // Step 2: Ingredient Gathering
        _missions[2] = new MissionInfo(
            2,
            "Gather Cooling Ingredients",
            "Collect Sandalwood and Forest Roses from the woods, draw Well Water, and find an Empty Glass.",
            "sandalwood",
            1
        );

        // Step 3: Circular Grinding Minigame
        _missions[3] = new MissionInfo(
            3,
            "Grind Chandana-Rose Paste",
            "Take Sandalwood and Rose Water to the village mortar and pestle and complete the circular grinding minigame.",
            "chandana_paste",
            1
        );

        // Step 4: Apply Paste
        _missions[4] = new MissionInfo(
            4,
            "Apply Cooling Paste",
            "Return to the fainted villager and apply the soothing Chandana-Rose paste to reduce his burning fever.",
            "",
            0
        );

        // Step 5: Sugarcane Juice Preparation
        _missions[5] = new MissionInfo(
            5,
            "Prepare Sugarcane Juice",
            "Gather fresh Sugarcane near the lake, crush it using the mortar and pestle, and pour it into the glass.",
            "sugarcane_juice",
            1
        );

        // Step 6: Administer Juice and Revive
        _missions[6] = new MissionInfo(
            6,
            "Revive the Villager",
            "Administer the fresh Sugarcane juice to the villager to restore his electrolytes and prana.",
            "",
            0
        );

        // Step 7: Scene 1 Completed & Unlock Next Story Beat
        _missions[7] = new MissionInfo(
            7,
            "Scene 1 Complete: Next Affliction",
            "The recovered villager reveals that two more villagers have collapsed deeper in the village. Proceed to their aid.",
            "",
            0
        );
    }

    public void SetMission(int missionId)
    {
        currentMissionId = missionId;
        NotifyCurrentMissionState();
    }

    private void HandleInventoryChanged(string itemId, int totalCount)
    {
        if (_missions.TryGetValue(currentMissionId, out var mission))
        {
            if (!string.IsNullOrEmpty(mission.targetItemId) && mission.targetItemId.Equals(itemId, StringComparison.OrdinalIgnoreCase))
            {
                int current = Mathf.Min(totalCount, mission.requiredItemCount);
                OnObjectiveUpdated?.Invoke(current, mission.requiredItemCount, $"{ItemData.GetDisplayName(itemId)}: {current}/{mission.requiredItemCount}");
            }
        }
    }

    public MissionInfo GetCurrentMission()
    {
        return _missions.TryGetValue(currentMissionId, out var m) ? m : null;
    }

    public bool IsMission1ReadyForTurnIn()
    {
        if (currentMissionId > 1) return true;
        if (InventoryManager.Instance != null)
        {
            return InventoryManager.Instance.GetItemCount("tulsi") >= 3;
        }
        return false;
    }

    /// <summary>
    /// Advances from the current mission to the next sequential mission.
    /// </summary>
    public void CompleteCurrentMissionAndAdvance()
    {
        if (_missions.TryGetValue(currentMissionId, out var mission))
        {
            mission.isCompleted = true;
            OnMissionCompleted?.Invoke(currentMissionId);
            Debug.Log($"[MissionManager] Mission {currentMissionId} ('{mission.title}') Completed!");
        }

        currentMissionId++;
        NotifyCurrentMissionState();
    }

    public void NotifyCurrentMissionState()
    {
        if (_missions.TryGetValue(currentMissionId, out var nextMission))
        {
            OnMissionStarted?.Invoke(currentMissionId, nextMission.title, nextMission.description);
            Debug.Log($"[MissionManager] Active Mission {currentMissionId}: '{nextMission.title}' - {nextMission.description}");

            if (!string.IsNullOrEmpty(nextMission.targetItemId) && InventoryManager.Instance != null)
            {
                int current = Mathf.Min(InventoryManager.Instance.GetItemCount(nextMission.targetItemId), nextMission.requiredItemCount);
                OnObjectiveUpdated?.Invoke(current, nextMission.requiredItemCount, $"{ItemData.GetDisplayName(nextMission.targetItemId)}: {current}/{nextMission.requiredItemCount}");
            }
        }
    }

    public string GetCurrentObjectiveText()
    {
        if (_missions.TryGetValue(currentMissionId, out var mission))
        {
            if (currentMissionId == 1)
            {
                int count = InventoryManager.Instance != null ? InventoryManager.Instance.GetItemCount("tulsi") : 0;
                return $"Objective: Gather Tulsi Leaves ({count}/3)";
            }
            if (currentMissionId == 2)
            {
                return "Objective: Inspect the Village Well";
            }
            return $"Objective: {mission.title}";
        }
        return "All missions completed!";
    }
}
