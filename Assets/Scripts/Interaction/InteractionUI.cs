using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Lightweight Screen-Space Overlay UI for displaying interaction prompts and gameplay notifications.
/// Shows "Press [E] to <Prompt>" when looking at an interactable within range, and hides when out of range.
/// Also displays on-screen notification banners (e.g. "Item collected", "Well inspected").
/// Uses standard Unity UI Text for 100% reliability without requiring external font assets.
/// </summary>
public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance { get; private set; }

    [Header("Prompt UI References")]
    [Tooltip("The parent GameObject containing the interaction prompt (toggled on/off)")]
    public GameObject promptRoot;

    [Tooltip("Standard UI Text component displaying the prompt")]
    public Text promptText;

    [Tooltip("Optional TextMeshPro component displaying the prompt (if assigned and valid)")]
    public TMP_Text promptTextTMP;

    [Header("Notification UI References")]
    [Tooltip("The parent GameObject for notification popups (toggled on/off)")]
    public GameObject notificationRoot;

    [Tooltip("Standard UI Text component displaying the notification message")]
    public Text notificationText;

    [Header("Display Settings")]
    [Tooltip("Prefix displayed before the prompt verb")]
    public string keyPrefix = "Press [E] to ";

    private bool _isPromptShowing;
    private Coroutine _notificationCoroutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureInstanceExists()
    {
        if (Instance == null && Object.FindFirstObjectByType<InteractionUI>() == null)
        {
            var uiGo = new GameObject("[InteractionUI_System]");
            uiGo.AddComponent<InteractionUI>();
            DontDestroyOnLoad(uiGo);
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

        // Auto-configure or build UI hierarchy if missing
        BuildOrValidateUI();

        // Hide prompt and notification initially
        Hide();
        HideNotificationImmediate();
    }

    public void BuildOrValidateUI()
    {
        // 1. Ensure Canvas
        var canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
        }

        var scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        // Get builtin font
        Font standardFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (standardFont == null)
            standardFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // 2. Build or find PromptPanel
        if (promptRoot == null)
        {
            var panelTrans = transform.Find("PromptPanel");
            if (panelTrans != null)
            {
                promptRoot = panelTrans.gameObject;
            }
            else
            {
                var panelGo = new GameObject("PromptPanel");
                panelGo.transform.SetParent(transform, false);

                var rect = panelGo.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(0f, 150f);
                rect.sizeDelta = new Vector2(460f, 64f);

                var img = panelGo.AddComponent<Image>();
                img.color = new Color(0.06f, 0.08f, 0.12f, 0.88f);

                // Add prompt text child
                var textGo = new GameObject("PromptText");
                textGo.transform.SetParent(panelGo.transform, false);

                var textRect = textGo.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(16f, 6f);
                textRect.offsetMax = new Vector2(-16f, -6f);

                var txt = textGo.AddComponent<Text>();
                txt.font = standardFont;
                txt.fontSize = 24;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.color = new Color(1f, 0.95f, 0.75f, 1f); // Warm soft gold/white
                txt.text = "Press [E] to Interact";

                promptRoot = panelGo;
                promptText = txt;
            }
        }

        if (promptText == null && promptRoot != null)
        {
            promptText = promptRoot.GetComponentInChildren<Text>(true);
            if (promptText != null && promptText.font == null)
                promptText.font = standardFont;
        }

        // 3. Build or find NotificationPanel
        if (notificationRoot == null)
        {
            var notifTrans = transform.Find("NotificationPanel");
            if (notifTrans != null)
            {
                notificationRoot = notifTrans.gameObject;
            }
            else
            {
                var notifGo = new GameObject("NotificationPanel");
                notifGo.transform.SetParent(transform, false);

                var rect = notifGo.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.85f);
                rect.anchorMax = new Vector2(0.5f, 0.85f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(560f, 60f);

                var img = notifGo.AddComponent<Image>();
                img.color = new Color(0.1f, 0.14f, 0.1f, 0.92f); // Deep forest green/slate

                var textGo = new GameObject("NotificationText");
                textGo.transform.SetParent(notifGo.transform, false);

                var textRect = textGo.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(20f, 6f);
                textRect.offsetMax = new Vector2(-20f, -6f);

                var txt = textGo.AddComponent<Text>();
                txt.font = standardFont;
                txt.fontSize = 22;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.color = new Color(0.7f, 1f, 0.7f, 1f); // Vibrant light green
                txt.text = "";

                notificationRoot = notifGo;
                notificationText = txt;
            }
        }

        if (notificationText == null && notificationRoot != null)
        {
            notificationText = notificationRoot.GetComponentInChildren<Text>(true);
            if (notificationText != null && notificationText.font == null)
                notificationText.font = standardFont;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Displays the interaction prompt with the given action verb (e.g. "Talk to Village Elder", "Inspect Well").
    /// </summary>
    public void Show(string prompt)
    {
        string fullText = keyPrefix + prompt;

        if (promptText != null)
        {
            promptText.text = fullText;
        }

        if (promptTextTMP != null && promptTextTMP.font != null)
        {
            promptTextTMP.text = fullText;
        }

        if (promptRoot != null && !_isPromptShowing)
        {
            promptRoot.SetActive(true);
            _isPromptShowing = true;
        }
    }

    /// <summary>
    /// Hides the interaction prompt.
    /// </summary>
    public void Hide()
    {
        if (promptRoot != null)
        {
            promptRoot.SetActive(false);
            _isPromptShowing = false;
        }
    }

    /// <summary>
    /// Displays an on-screen notification message (e.g. "Item Collected", "Water is Contaminated") for a specified duration.
    /// </summary>
    public void ShowNotification(string message, float duration = 3.5f)
    {
        if (notificationRoot == null || notificationText == null) return;

        notificationText.text = message;
        notificationRoot.SetActive(true);

        if (_notificationCoroutine != null)
            StopCoroutine(_notificationCoroutine);

        _notificationCoroutine = StartCoroutine(HideNotificationAfterDelay(duration));
    }

    private IEnumerator HideNotificationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideNotificationImmediate();
        _notificationCoroutine = null;
    }

    private void HideNotificationImmediate()
    {
        if (notificationRoot != null)
        {
            notificationRoot.SetActive(false);
        }
    }

    public bool IsPromptShowing => _isPromptShowing;
}
