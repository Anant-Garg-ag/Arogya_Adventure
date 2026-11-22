#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class InteractionSetup
{
    static InteractionSetup()
    {
        EditorApplication.delayCall += CheckAndSetup;
    }

    [MenuItem("Tools/Arogya Adventure/Setup Interaction System")]
    public static void RunSetupMenu()
    {
        SetupSceneInteraction(true);
    }

    private static void CheckAndSetup()
    {
        var player = GameObject.Find("Player");
        if (player != null && (player.GetComponent<PlayerInteraction>() == null || GameObject.Find("Tulsi_Herb_Test") == null))
        {
            SetupSceneInteraction(false);
        }
    }

    public static void SetupSceneInteraction(bool showDialog)
    {
        var player = GameObject.Find("Player");
        if (player == null)
        {
            if (showDialog) EditorUtility.DisplayDialog("Setup", "Player GameObject not found in active scene.", "OK");
            return;
        }

        var scene = player.scene;
        bool isDirty = false;

        var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");

        // 1. Setup PlayerInteraction on Player
        var playerInteraction = player.GetComponent<PlayerInteraction>();
        if (playerInteraction == null)
        {
            playerInteraction = player.AddComponent<PlayerInteraction>();
            isDirty = true;
        }
        playerInteraction.inputActionsAsset = inputAsset;
        playerInteraction.interactionDistance = 3.5f;
        playerInteraction.interactionRadius = 0.8f;
        playerInteraction.originHeightOffset = 2.5f;
        playerInteraction.forwardAngleThreshold = 0.3f;
        playerInteraction.interactableLayers = ~0;

        // 2. Setup EventSystem
        var eventSystem = Object.FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            var esGo = new GameObject("EventSystem");
            eventSystem = esGo.AddComponent<EventSystem>();
            var inputModule = esGo.AddComponent<InputSystemUIInputModule>();
            inputModule.actionsAsset = inputAsset;
            isDirty = true;
        }

        // 3. Setup InteractionCanvas with standard reliable UI Text
        Font standardFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (standardFont == null)
            standardFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        var canvasGo = GameObject.Find("InteractionCanvas");
        if (canvasGo == null)
        {
            canvasGo = new GameObject("InteractionCanvas");
            isDirty = true;
        }

        var canvas = canvasGo.GetComponent<Canvas>();
        if (canvas == null) canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        if (canvasGo.GetComponent<GraphicRaycaster>() == null)
        {
            canvasGo.AddComponent<GraphicRaycaster>();
            isDirty = true;
        }

        var interactionUI = canvasGo.GetComponent<InteractionUI>();
        if (interactionUI == null)
        {
            interactionUI = canvasGo.AddComponent<InteractionUI>();
            isDirty = true;
        }

        // Build / Update PromptPanel
        var panelTrans = canvasGo.transform.Find("PromptPanel");
        GameObject panelGo;
        if (panelTrans == null)
        {
            panelGo = new GameObject("PromptPanel");
            panelGo.transform.SetParent(canvasGo.transform, false);
            isDirty = true;
        }
        else
        {
            panelGo = panelTrans.gameObject;
        }

        var panelRect = panelGo.GetComponent<RectTransform>();
        if (panelRect == null) panelRect = panelGo.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 150f);
        panelRect.sizeDelta = new Vector2(460f, 64f);

        var panelImg = panelGo.GetComponent<Image>();
        if (panelImg == null) panelImg = panelGo.AddComponent<Image>();
        panelImg.color = new Color(0.06f, 0.08f, 0.12f, 0.90f);

        // Build / Update PromptText with reliable standard Text
        var textTrans = panelGo.transform.Find("PromptText");
        GameObject textGo;
        if (textTrans == null)
        {
            textGo = new GameObject("PromptText");
            textGo.transform.SetParent(panelGo.transform, false);
            isDirty = true;
        }
        else
        {
            textGo = textTrans.gameObject;
        }

        // Remove broken/unsupported TMP component if present on PromptText
        var oldTmp = textGo.GetComponent("TextMeshProUGUI");
        if (oldTmp != null)
        {
            Object.DestroyImmediate(oldTmp);
            isDirty = true;
        }

        var textRect = textGo.GetComponent<RectTransform>();
        if (textRect == null) textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(16f, 6f);
        textRect.offsetMax = new Vector2(-16f, -6f);

        var standardText = textGo.GetComponent<Text>();
        if (standardText == null) standardText = textGo.AddComponent<Text>();
        standardText.font = standardFont;
        standardText.fontSize = 24;
        standardText.alignment = TextAnchor.MiddleCenter;
        standardText.color = new Color(1f, 0.95f, 0.75f, 1f);
        standardText.text = "Press [E] to Interact";

        panelGo.SetActive(false);

        // Build / Update NotificationPanel
        var notifTrans = canvasGo.transform.Find("NotificationPanel");
        GameObject notifGo;
        if (notifTrans == null)
        {
            notifGo = new GameObject("NotificationPanel");
            notifGo.transform.SetParent(canvasGo.transform, false);
            isDirty = true;
        }
        else
        {
            notifGo = notifTrans.gameObject;
        }

        var notifRect = notifGo.GetComponent<RectTransform>();
        if (notifRect == null) notifRect = notifGo.AddComponent<RectTransform>();
        notifRect.anchorMin = new Vector2(0.5f, 0.85f);
        notifRect.anchorMax = new Vector2(0.5f, 0.85f);
        notifRect.pivot = new Vector2(0.5f, 0.5f);
        notifRect.anchoredPosition = Vector2.zero;
        notifRect.sizeDelta = new Vector2(620f, 64f);

        var notifImg = notifGo.GetComponent<Image>();
        if (notifImg == null) notifImg = notifGo.AddComponent<Image>();
        notifImg.color = new Color(0.08f, 0.14f, 0.10f, 0.94f);

        var notifTextTrans = notifGo.transform.Find("NotificationText");
        GameObject notifTextGo;
        if (notifTextTrans == null)
        {
            notifTextGo = new GameObject("NotificationText");
            notifTextGo.transform.SetParent(notifGo.transform, false);
            isDirty = true;
        }
        else
        {
            notifTextGo = notifTextTrans.gameObject;
        }

        var notifTextRect = notifTextGo.GetComponent<RectTransform>();
        if (notifTextRect == null) notifTextRect = notifTextGo.AddComponent<RectTransform>();
        notifTextRect.anchorMin = Vector2.zero;
        notifTextRect.anchorMax = Vector2.one;
        notifTextRect.offsetMin = new Vector2(20f, 6f);
        notifTextRect.offsetMax = new Vector2(-20f, -6f);

        var notifTextComp = notifTextGo.GetComponent<Text>();
        if (notifTextComp == null) notifTextComp = notifTextGo.AddComponent<Text>();
        notifTextComp.font = standardFont;
        notifTextComp.fontSize = 22;
        notifTextComp.alignment = TextAnchor.MiddleCenter;
        notifTextComp.color = new Color(0.75f, 1f, 0.75f, 1f);
        notifTextComp.text = "";

        notifGo.SetActive(false);

        // Bind references to InteractionUI
        interactionUI.promptRoot = panelGo;
        interactionUI.promptText = standardText;
        interactionUI.notificationRoot = notifGo;
        interactionUI.notificationText = notifTextComp;
        interactionUI.keyPrefix = "Press [E] to ";

        // 4. Setup Elder NPC Interactable (Scene_1_map/Cube)
        var elderCube = GameObject.Find("Scene_1_map/Cube");
        if (elderCube != null)
        {
            var npc = elderCube.GetComponent<NPCInteractable>();
            if (npc == null)
            {
                npc = elderCube.AddComponent<NPCInteractable>();
                isDirty = true;
            }
            npc.npcName = "Village Elder";
            npc.promptVerb = "Talk to";
            npc.dialogueLine = "Greetings, traveler. A grave affliction has fallen upon Arogya Gram. The well is tainted, and our people are weakening.";

            var col = elderCube.GetComponent<Collider>();
            if (col == null)
            {
                var boxCol = elderCube.AddComponent<BoxCollider>();
                boxCol.center = new Vector3(0.1f, 2.5f, 0f);
                boxCol.size = new Vector3(2.0f, 5.0f, 2.0f);
                boxCol.isTrigger = true;
                isDirty = true;
            }
        }

        // 5. Setup Village Well Interactable (Scene_1_map/Well)
        var well = GameObject.Find("Scene_1_map/Well");
        if (well != null)
        {
            var wellInteractable = well.GetComponent<SimpleInteractable>();
            if (wellInteractable == null)
            {
                wellInteractable = well.AddComponent<SimpleInteractable>();
                isDirty = true;
            }
            wellInteractable.promptText = "Inspect Well";
            wellInteractable.responseMessage = "The well water is stagnant and murky. It cannot be used safely.";
            wellInteractable.isRepeatable = true;

            var col = well.GetComponent<Collider>();
            if (col == null)
            {
                var boxCol = well.AddComponent<BoxCollider>();
                boxCol.isTrigger = true;
                isDirty = true;
            }
        }

        // 6. Setup Test Collectible Herb (Tulsi_Herb_Test)
        var herb = GameObject.Find("Tulsi_Herb_Test");
        if (herb == null)
        {
            herb = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            herb.name = "Tulsi_Herb_Test";
            herb.transform.position = new Vector3(333.5f, -43.6f, -72.0f); // Near player spawn on the pathway
            herb.transform.localScale = new Vector3(0.6f, 0.3f, 0.6f);

            var herbCol = herb.GetComponent<Collider>();
            if (herbCol != null) herbCol.isTrigger = true;

            // Give it a vibrant herbal green material
            var renderer = herb.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.name = "Tulsi_Herb_Mat";
                mat.color = new Color(0.18f, 0.75f, 0.25f, 1f);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", new Color(0.18f, 0.75f, 0.25f, 1f));
                renderer.sharedMaterial = mat;
            }

            var collectible = herb.AddComponent<CollectibleItem>();
            collectible.itemName = "Tulsi Leaf (Medicinal Herb)";
            collectible.promptVerb = "Collect";
            collectible.notificationDuration = 4.0f;
            collectible.disableGameObject = true;

            isDirty = true;
        }

        if (isDirty)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[InteractionSetup] Successfully updated interaction system with UI Text and Test Collectible!");
        }

        if (showDialog)
        {
            EditorUtility.DisplayDialog("Interaction Setup", "Interaction system updated with reliable UI Text, Notification banner, and Tulsi Leaf Collectible!", "OK");
        }
    }
}
#endif
