using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reusable circular grinding minigame for Ayurvedic preparation (Mortar & Pestle).
/// Used in Scene 1 for Chandana-Rose paste and reusable in Scene 2 for Turmeric grinding.
/// Requires the player to complete 3 consecutive rotations with > 85% radial tracking accuracy.
/// </summary>
public class GrindingMinigameUI : MonoBehaviour
{
    public static GrindingMinigameUI Instance { get; private set; }

    [Header("Minigame State")]
    public bool isMinigameActive = false;
    public int requiredConsecutiveRotations = 3;
    public float requiredAccuracy = 0.85f; // Must be > 85%
    public int currentConsecutiveRotations = 0;

    [Header("Item Rewards")]
    public string targetResultItemId = "chandana_paste";
    public string targetResultItemName = "Chandana & Rose Water Paste";
    public Action onCompleteCallback;

    [Header("UI Elements")]
    public GameObject minigameRoot;
    public RectTransform mortarCenter;
    public RectTransform pestleHandle;
    public Text titleText;
    public Text statusText;
    public Text rotationCounterText;
    public Text accuracyText;
    public Button closeButton;

    // Radius of circular grinding track in pixels
    public float targetTrackRadius = 110f;

    // Tracking variables
    private float _lastAngle = 0f;
    private float _accumulatedAngle = 0f;
    private bool _hasStartedRotation = false;
    private List<float> _currentRotationAccuracies = new List<float>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        EnsureUIExists();
    }

    private void Start()
    {
        if (minigameRoot != null)
        {
            minigameRoot.SetActive(false);
        }
    }

    public void EnsureUIExists()
    {
        if (minigameRoot != null) return;

        // Find or create within InteractionCanvas
        var canvasGo = GameObject.Find("InteractionCanvas");
        if (canvasGo == null)
        {
            var c = FindFirstObjectByType<Canvas>();
            if (c != null) canvasGo = c.gameObject;
        }

        if (canvasGo == null) return;

        // Create container root
        minigameRoot = new GameObject("GrindingMinigame_Root");
        minigameRoot.transform.SetParent(canvasGo.transform, false);

        var rootRect = minigameRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = new Vector2(480, 520);

        // Background panel
        var bg = minigameRoot.AddComponent<Image>();
        bg.color = new Color(0.12f, 0.12f, 0.14f, 0.94f);

        Font standardFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (standardFont == null) standardFont = Font.CreateDynamicFontFromOSFont("Arial", 16);

        // Title
        var titleGo = new GameObject("Title");
        titleGo.transform.SetParent(minigameRoot.transform, false);
        titleText = titleGo.AddComponent<Text>();
        titleText.font = standardFont;
        titleText.fontSize = 22;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.95f, 0.85f, 0.5f, 1f);
        titleText.text = "Ayurvedic Mortar & Pestle Grinding";
        var tRect = titleGo.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0, 1);
        tRect.anchorMax = new Vector2(1, 1);
        tRect.pivot = new Vector2(0.5f, 1f);
        tRect.sizeDelta = new Vector2(0, 45);
        tRect.anchoredPosition = new Vector2(0, -10);

        // Status / Instructions
        var statusGo = new GameObject("Status");
        statusGo.transform.SetParent(minigameRoot.transform, false);
        statusText = statusGo.AddComponent<Text>();
        statusText.font = standardFont;
        statusText.fontSize = 15;
        statusText.alignment = TextAnchor.MiddleCenter;
        statusText.color = Color.white;
        statusText.text = "Drag the pestle in smooth clockwise circles around the mortar track.\nMust complete 3 consecutive rotations with > 85% accuracy!";
        var sRect = statusGo.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0, 1);
        sRect.anchorMax = new Vector2(1, 1);
        sRect.pivot = new Vector2(0.5f, 1f);
        sRect.sizeDelta = new Vector2(-20, 50);
        sRect.anchoredPosition = new Vector2(0, -55);

        // Mortar Center
        var centerGo = new GameObject("MortarCenter");
        centerGo.transform.SetParent(minigameRoot.transform, false);
        mortarCenter = centerGo.AddComponent<RectTransform>();
        mortarCenter.anchorMin = new Vector2(0.5f, 0.5f);
        mortarCenter.anchorMax = new Vector2(0.5f, 0.5f);
        mortarCenter.pivot = new Vector2(0.5f, 0.5f);
        mortarCenter.sizeDelta = new Vector2(240, 240);
        mortarCenter.anchoredPosition = new Vector2(0, 10);
        var mortarImg = centerGo.AddComponent<Image>();
        mortarImg.color = new Color(0.25f, 0.22f, 0.20f, 0.9f);

        // Track Ring
        var trackGo = new GameObject("TrackRing");
        trackGo.transform.SetParent(centerGo.transform, false);
        var trackRect = trackGo.AddComponent<RectTransform>();
        trackRect.anchorMin = new Vector2(0.5f, 0.5f);
        trackRect.anchorMax = new Vector2(0.5f, 0.5f);
        trackRect.pivot = new Vector2(0.5f, 0.5f);
        trackRect.sizeDelta = new Vector2(targetTrackRadius * 2f, targetTrackRadius * 2f);
        var trackImg = trackGo.AddComponent<Image>();
        trackImg.color = new Color(0.4f, 0.7f, 0.4f, 0.35f);

        // Pestle Handle
        var pestleGo = new GameObject("PestleHandle");
        pestleGo.transform.SetParent(centerGo.transform, false);
        pestleHandle = pestleGo.AddComponent<RectTransform>();
        pestleHandle.anchorMin = new Vector2(0.5f, 0.5f);
        pestleHandle.anchorMax = new Vector2(0.5f, 0.5f);
        pestleHandle.pivot = new Vector2(0.5f, 0.5f);
        pestleHandle.sizeDelta = new Vector2(36, 36);
        pestleHandle.anchoredPosition = new Vector2(0, targetTrackRadius);
        var pestleImg = pestleGo.AddComponent<Image>();
        pestleImg.color = new Color(0.95f, 0.85f, 0.3f, 1f);

        // Counter Text
        var counterGo = new GameObject("RotationCounter");
        counterGo.transform.SetParent(minigameRoot.transform, false);
        rotationCounterText = counterGo.AddComponent<Text>();
        rotationCounterText.font = standardFont;
        rotationCounterText.fontSize = 20;
        rotationCounterText.fontStyle = FontStyle.Bold;
        rotationCounterText.alignment = TextAnchor.MiddleCenter;
        rotationCounterText.color = new Color(0.7f, 1f, 0.7f, 1f);
        rotationCounterText.text = "Consecutive Rotations: 0 / 3";
        var cRect = counterGo.GetComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0, 0);
        cRect.anchorMax = new Vector2(1, 0);
        cRect.pivot = new Vector2(0.5f, 0f);
        cRect.sizeDelta = new Vector2(0, 30);
        cRect.anchoredPosition = new Vector2(0, 65);

        // Accuracy Text
        var accGo = new GameObject("AccuracyText");
        accGo.transform.SetParent(minigameRoot.transform, false);
        accuracyText = accGo.AddComponent<Text>();
        accuracyText.font = standardFont;
        accuracyText.fontSize = 17;
        accuracyText.alignment = TextAnchor.MiddleCenter;
        accuracyText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        accuracyText.text = "Current Accuracy: 100% (Target: > 85%)";
        var aRect = accGo.GetComponent<RectTransform>();
        aRect.anchorMin = new Vector2(0, 0);
        aRect.anchorMax = new Vector2(1, 0);
        aRect.pivot = new Vector2(0.5f, 0f);
        aRect.sizeDelta = new Vector2(0, 25);
        aRect.anchoredPosition = new Vector2(0, 38);

        // Close / Cancel Button
        var btnGo = new GameObject("CloseButton");
        btnGo.transform.SetParent(minigameRoot.transform, false);
        var btnRect = btnGo.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0);
        btnRect.anchorMax = new Vector2(0.5f, 0);
        btnRect.pivot = new Vector2(0.5f, 0f);
        btnRect.sizeDelta = new Vector2(120, 30);
        btnRect.anchoredPosition = new Vector2(0, 6);
        var btnImg = btnGo.AddComponent<Image>();
        btnImg.color = new Color(0.35f, 0.15f, 0.15f, 1f);
        closeButton = btnGo.AddComponent<Button>();
        closeButton.onClick.AddListener(CancelMinigame);

        var btnTextGo = new GameObject("Text");
        btnTextGo.transform.SetParent(btnGo.transform, false);
        var btnText = btnTextGo.AddComponent<Text>();
        btnText.font = standardFont;
        btnText.fontSize = 14;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        btnText.text = "Cancel";
        var btRect = btnTextGo.GetComponent<RectTransform>();
        btRect.anchorMin = Vector2.zero;
        btRect.anchorMax = Vector2.one;
        btRect.sizeDelta = Vector2.zero;

        minigameRoot.SetActive(false);
    }

    public void StartGrinding(string resultItemId, string resultItemName, Action onComplete)
    {
        EnsureUIExists();

        targetResultItemId = resultItemId;
        targetResultItemName = resultItemName;
        onCompleteCallback = onComplete;

        currentConsecutiveRotations = 0;
        _accumulatedAngle = 0f;
        _hasStartedRotation = false;
        _currentRotationAccuracies.Clear();

        if (titleText != null)
        {
            titleText.text = $"Grinding {resultItemName}";
        }
        UpdateUI();

        if (minigameRoot != null)
        {
            minigameRoot.SetActive(true);
        }
        isMinigameActive = true;
    }

    private void Update()
    {
        if (!isMinigameActive) return;

        // Player input tracking
        if (Input.GetMouseButton(0) && mortarCenter != null)
        {
            Vector2 localMouse;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mortarCenter,
                Input.mousePosition,
                null,
                out localMouse
            );

            ProcessPointerInput(localMouse);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _hasStartedRotation = false;
        }

        // Cancel with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelMinigame();
        }
    }

    /// <summary>
    /// Processes pointer position relative to mortar center.
    /// Tracks clockwise angular rotation and radial accuracy against targetTrackRadius.
    /// </summary>
    public void ProcessPointerInput(Vector2 localPos)
    {
        float currentRadius = localPos.magnitude;
        float currentAngle = Mathf.Atan2(localPos.y, localPos.x) * Mathf.Rad2Deg;
        if (currentAngle < 0f) currentAngle += 360f;

        // Position visual pestle
        if (pestleHandle != null)
        {
            pestleHandle.anchoredPosition = localPos;
        }

        // Calculate sample accuracy: 1.0 at ideal radius, drops toward 0 as distance deviates
        float radialDeviation = Mathf.Abs(currentRadius - targetTrackRadius);
        float sampleAccuracy = Mathf.Clamp01(1.0f - (radialDeviation / (targetTrackRadius * 0.7f)));
        _currentRotationAccuracies.Add(sampleAccuracy);

        if (_hasStartedRotation)
        {
            // Clockwise movement = decreasing angle in standard coordinates
            float delta = Mathf.DeltaAngle(_lastAngle, currentAngle);
            if (delta < 0f) // Moving clockwise
            {
                _accumulatedAngle += Mathf.Abs(delta);
            }
            else if (delta > 5f) // Reverse direction penalty
            {
                _currentRotationAccuracies.Add(0.2f);
            }

            // Check if one full 360 degree rotation has completed
            if (_accumulatedAngle >= 360f)
            {
                CompleteCurrentRotation();
            }
        }
        else
        {
            _hasStartedRotation = true;
        }

        _lastAngle = currentAngle;
        UpdateUI();
    }

    /// <summary>
    /// Evaluates completed 360-degree rotation against the 85% accuracy threshold.
    /// </summary>
    private void CompleteCurrentRotation()
    {
        float avgAccuracy = CalculateCurrentAverageAccuracy();
        _accumulatedAngle = 0f;
        _currentRotationAccuracies.Clear();

        if (avgAccuracy >= requiredAccuracy)
        {
            currentConsecutiveRotations++;
            if (statusText != null)
            {
                statusText.text = $"Excellent! Rotation {currentConsecutiveRotations}/3 succeeded with {avgAccuracy:P0} accuracy!";
                statusText.color = new Color(0.5f, 1f, 0.5f);
            }

            if (currentConsecutiveRotations >= requiredConsecutiveRotations)
            {
                FinishMinigameSuccess();
                return;
            }
        }
        else
        {
            // FAILED ROTATION: Did not achieve required 85% accuracy!
            currentConsecutiveRotations = 0; // Strict streak reset
            if (statusText != null)
            {
                statusText.text = $"Grind failed! Accuracy was only {avgAccuracy:P0} (Must be > {requiredAccuracy:P0}). Consecutive streak reset to 0/3!";
                statusText.color = new Color(1f, 0.4f, 0.4f);
            }
        }

        UpdateUI();
    }

    private float CalculateCurrentAverageAccuracy()
    {
        if (_currentRotationAccuracies.Count == 0) return 0f;
        float sum = 0f;
        for (int i = 0; i < _currentRotationAccuracies.Count; i++)
        {
            sum += _currentRotationAccuracies[i];
        }
        return sum / _currentRotationAccuracies.Count;
    }

    private void FinishMinigameSuccess()
    {
        isMinigameActive = false;
        if (statusText != null)
        {
            statusText.text = $"Success! 3 Consecutive Rotations achieved! Prepared {targetResultItemName}!";
            statusText.color = new Color(0.4f, 1f, 0.4f);
        }

        // Grant item to player inventory
        if (InventoryManager.Instance != null && !string.IsNullOrEmpty(targetResultItemId))
        {
            InventoryManager.Instance.AddItem(targetResultItemId, 1);
        }

        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.ShowNotification($"Crafted: {targetResultItemName}!", 4.0f);
        }

        onCompleteCallback?.Invoke();

        // Close after small display delay
        Invoke(nameof(CloseUI), 1.2f);
    }

    public void CancelMinigame()
    {
        isMinigameActive = false;
        CloseUI();
    }

    private void CloseUI()
    {
        if (minigameRoot != null)
        {
            minigameRoot.SetActive(false);
        }
    }

    private void UpdateUI()
    {
        if (rotationCounterText != null)
        {
            rotationCounterText.text = $"Consecutive Rotations: {currentConsecutiveRotations} / {requiredConsecutiveRotations}";
        }

        if (accuracyText != null)
        {
            float acc = CalculateCurrentAverageAccuracy();
            if (_currentRotationAccuracies.Count == 0) acc = 1.0f;
            accuracyText.text = $"Rotation Accuracy: {acc:P0} (Must achieve > {requiredAccuracy:P0})";
            accuracyText.color = acc >= requiredAccuracy ? new Color(0.6f, 1f, 0.6f) : new Color(1f, 0.6f, 0.6f);
        }
    }

    /// <summary>
    /// Programmatic execution hook for Play Mode testing.
    /// Simulates completing a single 360-degree rotation with the specified accuracy.
    /// </summary>
    public bool SimulateRotation(float simulatedAccuracy)
    {
        if (!isMinigameActive) return false;

        _currentRotationAccuracies.Clear();
        // Seed 100 accuracy samples at the desired average
        for (int i = 0; i < 100; i++)
        {
            _currentRotationAccuracies.Add(simulatedAccuracy);
        }
        _accumulatedAngle = 360f;
        CompleteCurrentRotation();
        return currentConsecutiveRotations > 0;
    }
}
