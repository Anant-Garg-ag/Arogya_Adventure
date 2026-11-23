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

        // Mission 1: Confirmed Story Beat
        _missions[1] = new MissionInfo(
            1,
            "Collect Medicinal Tulsi",
            "Bring 3 Tulsi leaves from near the stream to the Village Elder.",
            "tulsi",
            3
        );

        // Mission 2: Confirmed Story Beat
        _missions[2] = new MissionInfo(
            2,
            "Investigate Village Well",
            "Inspect the tainted village well to identify the source of contamination.",
            "",
            0
        );

        // Missions 3 to 6: Linear milestones per Implementation Plan
        _missions[3] = new MissionInfo(3, "Highland Herb Foraging", "Gather Ashwagandha and Neem along the riverbanks.", "ashwagandha", 2);
        _missions[4] = new MissionInfo(4, "Sacred Waterfall Source", "Reach the mountain waterfall to fetch pure untainted water.", "pure_water", 1);
        _missions[5] = new MissionInfo(5, "Apothecary Decoction", "Brew the Sanjeevani Kadha at the village clinic crafting table.", "sanjeevani_kadha", 1);
        _missions[6] = new MissionInfo(6, "Cure Arogya Gram", "Deliver the Sanjeevani Kadha to heal the village.", "", 0);
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
