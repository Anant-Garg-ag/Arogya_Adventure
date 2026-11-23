using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lightweight HUD panel that displays the player's collected herbs and quest items.
/// Automatically listens to InventoryManager.OnInventoryChanged and refreshes the display.
/// Uses standard Unity UI Text with built-in font for 100% compatibility.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("UI Hierarchy References")]
    [Tooltip("Parent panel container for the inventory HUD (top-left or bottom-left)")]
    public GameObject inventoryPanel;

    [Tooltip("Text component displaying the inventory list")]
    public Text inventoryText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        BuildOrValidateUI();
    }

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += HandleInventoryChanged;
        }
        RefreshDisplay();
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= HandleInventoryChanged;
        }
    }

    private void Start()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= HandleInventoryChanged;
            InventoryManager.Instance.OnInventoryChanged += HandleInventoryChanged;
        }
        RefreshDisplay();
    }

    private void HandleInventoryChanged(string itemId, int totalCount)
    {
        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        if (inventoryText == null) return;

        if (InventoryManager.Instance == null)
        {
            inventoryText.text = "Herbs: [None]";
            return;
        }

        var allItems = InventoryManager.Instance.GetAllItems();
        if (allItems.Count == 0)
        {
            inventoryText.text = "Herbs: [None]";
            return;
        }

        // Format items nicely: e.g. "Tulsi: 2 | Pure Water: 1"
        var parts = new List<string>();
        foreach (var kvp in allItems)
        {
            if (kvp.Value > 0)
            {
                string name = ItemData.GetDisplayName(kvp.Key);
                parts.Add($"{name}: {kvp.Value}");
            }
        }

        if (parts.Count == 0)
        {
            inventoryText.text = "Herbs: [None]";
        }
        else
        {
            inventoryText.text = "Herbs: " + string.Join(" | ", parts);
        }
    }

    public void BuildOrValidateUI()
    {
        // 1. Ensure Canvas
        var canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 45;
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

        // Built-in standard font
        Font standardFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (standardFont == null)
            standardFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // 2. Build InventoryPanel if not assigned
        if (inventoryPanel == null)
        {
            var panelTrans = transform.Find("InventoryPanel");
            if (panelTrans != null)
            {
                inventoryPanel = panelTrans.gameObject;
            }
            else
            {
                var panelGo = new GameObject("InventoryPanel");
                panelGo.transform.SetParent(transform, false);

                var rect = panelGo.AddComponent<RectTransform>();
                // Anchor to Top-Left
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(30f, -30f);
                rect.sizeDelta = new Vector2(360f, 48f);

                var img = panelGo.AddComponent<Image>();
                img.color = new Color(0.06f, 0.10f, 0.08f, 0.85f); // Soft herbal slate background

                var textGo = new GameObject("InventoryText");
                textGo.transform.SetParent(panelGo.transform, false);

                var textRect = textGo.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(16f, 4f);
                textRect.offsetMax = new Vector2(-16f, -4f);

                var txt = textGo.AddComponent<Text>();
                txt.font = standardFont;
                txt.fontSize = 20;
                txt.alignment = TextAnchor.MiddleLeft;
                txt.color = new Color(0.85f, 1f, 0.85f, 1f); // Gentle botanical green
                txt.text = "Herbs: [None]";

                inventoryPanel = panelGo;
                inventoryText = txt;
            }
        }

        if (inventoryText == null && inventoryPanel != null)
        {
            inventoryText = inventoryPanel.GetComponentInChildren<Text>(true);
            if (inventoryText != null && inventoryText.font == null)
                inventoryText.font = standardFont;
        }
    }
}
