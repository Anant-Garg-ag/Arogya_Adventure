#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor setup tool that configures the Stage 2 Gameplay Systems in SampleScene:
/// 1. Player interaction, inventory, and kinematic rigidbody alignment.
/// 2. EventSystem and InteractionCanvas (with InteractionUI and InventoryUI).
/// 3. Tulsi Herb Collectibles (3 instances along stream/pathway).
/// 4. Village Apothecary / Clinic (House_1.003) with perimeter colliders, interior floor/table, and animated DoorInteractable.
/// 5. Exterior interaction triggers on remaining 11 houses.
/// 6. Village Elder (NPCInteractable) and Village Well (SimpleInteractable).
/// </summary>
[InitializeOnLoad]
public static class InteractionSetup
{
    static InteractionSetup()
    {
        EditorApplication.delayCall += CheckAndSetup;
    }

    [MenuItem("Tools/Arogya Adventure/Setup Stage 2 Gameplay Systems")]
    public static void RunSetupMenu()
    {
        SetupSceneInteraction(true);
    }

    private static void CheckAndSetup()
    {
        var player = GameObject.Find("Player");
        if (player != null && (player.GetComponent<PlayerInteraction>() == null || GameObject.Find("Village_Workstation") == null || GameObject.Find("Sandalwood_Node") == null))
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

        // 1. Setup PlayerInteraction & align Rigidbody on Player
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

        // Ensure Rigidbody on Player is kinematic so PhysX gravity doesn't compete with CharacterController
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            if (!rb.isKinematic || rb.useGravity)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                isDirty = true;
            }
        }

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
        panelRect.sizeDelta = new Vector2(480f, 64f);

        var panelImg = panelGo.GetComponent<Image>();
        if (panelImg == null) panelImg = panelGo.AddComponent<Image>();
        panelImg.color = new Color(0.06f, 0.08f, 0.12f, 0.90f);

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
        notifRect.sizeDelta = new Vector2(640f, 64f);

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

        // Setup InventoryUI on InteractionCanvas
        var inventoryUI = canvasGo.GetComponent<InventoryUI>();
        if (inventoryUI == null)
        {
            inventoryUI = canvasGo.AddComponent<InventoryUI>();
            isDirty = true;
        }
        inventoryUI.BuildOrValidateUI();

        // 4. Setup Fainted Villager NPC (Scene_1_map/Cube)
        var faintedCube = GameObject.Find("Scene_1_map/Cube");
        if (faintedCube != null)
        {
            faintedCube.transform.position = new Vector3(316.510f, -43.710f, -84.166f);
            var armature = GameObject.Find("Scene_1_map/Armature");
            if (armature != null)
            {
                armature.transform.position = new Vector3(314.093f, -43.723f, -83.260f);
                armature.transform.eulerAngles = new Vector3(180f, 290.7f, 0f); // Collapsed lying pose
            }

            var faintedNpc = faintedCube.GetComponent<FaintedVillagerNPC>();
            if (faintedNpc == null)
            {
                faintedNpc = faintedCube.AddComponent<FaintedVillagerNPC>();
                isDirty = true;
            }
            if (armature != null) faintedNpc.armatureTransform = armature.transform;

            // Also keep NPCInteractable for legacy dialogue/compatibility
            var npc = faintedCube.GetComponent<NPCInteractable>();
            if (npc == null)
            {
                npc = faintedCube.AddComponent<NPCInteractable>();
                isDirty = true;
            }
            npc.npcName = "Fainted Villager";
            npc.promptVerb = "Tend to";
            npc.isVillageElder = false;
            npc.dialogueLine = "The heat is unbearable... cooling Chandana paste and fresh sugarcane juice will revive me...";

            var col = faintedCube.GetComponent<Collider>();
            if (col == null)
            {
                var boxCol = faintedCube.AddComponent<BoxCollider>();
                boxCol.center = new Vector3(0.1f, 2.5f, 0f);
                boxCol.size = new Vector3(6.0f, 6.0f, 6.0f);
                boxCol.isTrigger = true;
                isDirty = true;
            }
            else if (col is BoxCollider bc)
            {
                bc.isTrigger = true;
                bc.size = new Vector3(6.0f, 6.0f, 6.0f);
            }

            // Check if Animator can be attached for pose
            var elderAnimator = faintedCube.GetComponent<Animator>();
            if (elderAnimator == null && armature != null)
            {
                elderAnimator = faintedCube.AddComponent<Animator>();
                var sadhuAvatar = AssetDatabase.LoadAssetAtPath<Avatar>("Assets/Sadhu_rigged_walking_animation.fbx");
                if (sadhuAvatar != null)
                {
                    elderAnimator.avatar = sadhuAvatar;
                }
                isDirty = true;
            }
        }

        // 5. Setup Village Well Interactable (Scene_1_map/Well)
        var well = GameObject.Find("Scene_1_map/Well");
        if (well != null)
        {
            var wellWater = well.GetComponent<WellInteractable>();
            if (wellWater == null)
            {
                wellWater = well.AddComponent<WellInteractable>();
                isDirty = true;
            }

            var wellInteractable = well.GetComponent<SimpleInteractable>();
            if (wellInteractable == null)
            {
                wellInteractable = well.AddComponent<SimpleInteractable>();
                isDirty = true;
            }
            wellInteractable.promptText = "Draw Well Water";
            wellInteractable.responseMessage = "Drew pure, cool water from the village well.";
            wellInteractable.isVillageWell = false;
            wellInteractable.isRepeatable = true;

            var col = well.GetComponent<Collider>();
            if (col == null)
            {
                var boxCol = well.AddComponent<BoxCollider>();
                boxCol.isTrigger = true;
                isDirty = true;
            }
        }

        // 6. Setup Collectibles (Tulsi, Sandalwood, Forest Rose, Sugarcane)
        Vector3[] herbPositions = new Vector3[]
        {
            new Vector3(333.5f, -43.6f, -72.0f),
            new Vector3(338.5f, -43.5f, -79.0f),
            new Vector3(343.5f, -43.4f, -86.5f)
        };
        string[] herbNames = new string[] { "Tulsi_Herb_Test", "Tulsi_Herb_2", "Tulsi_Herb_3" };

        Material herbMat = null;
        for (int i = 0; i < herbNames.Length; i++)
        {
            var h = GameObject.Find(herbNames[i]);
            if (h == null)
            {
                h = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                h.name = herbNames[i];
                h.transform.position = herbPositions[i];
                h.transform.localScale = new Vector3(0.6f, 0.3f, 0.6f);

                var herbCol = h.GetComponent<Collider>();
                if (herbCol != null) herbCol.isTrigger = true;

                if (herbMat == null)
                {
                    herbMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    herbMat.name = "Tulsi_Herb_Mat";
                    herbMat.color = new Color(0.18f, 0.85f, 0.30f, 1f);
                    if (herbMat.HasProperty("_BaseColor")) herbMat.SetColor("_BaseColor", new Color(0.18f, 0.85f, 0.30f, 1f));
                }

                var r = h.GetComponent<MeshRenderer>();
                if (r != null) r.sharedMaterial = herbMat;

                var collectible = h.AddComponent<CollectibleItem>();
                collectible.itemId = "tulsi";
                collectible.itemName = "Tulsi Leaf (Medicinal Herb)";
                collectible.promptVerb = "Collect";
                collectible.quantity = 1;
                collectible.notificationDuration = 3.5f;
                collectible.disableGameObject = true;

                isDirty = true;
            }
        }

        // Sandalwood Wood Node (in forest)
        var sandalwood = GameObject.Find("Sandalwood_Node");
        if (sandalwood == null)
        {
            sandalwood = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            sandalwood.name = "Sandalwood_Node";
            sandalwood.transform.position = new Vector3(325.0f, -43.5f, -60.0f);
            sandalwood.transform.localScale = new Vector3(0.7f, 0.4f, 0.7f);
            var col = sandalwood.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.75f, 0.55f, 0.35f, 1f);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", new Color(0.75f, 0.55f, 0.35f, 1f));
            sandalwood.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var c = sandalwood.AddComponent<CollectibleItem>();
            c.itemId = "sandalwood";
            c.itemName = "Sandalwood Wood";
            c.promptVerb = "Gather";
            c.quantity = 1;
            c.disableGameObject = true;
            isDirty = true;
        }

        // Forest Rose Node (in forest)
        var rose = GameObject.Find("Rose_Node");
        if (rose == null)
        {
            rose = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rose.name = "Rose_Node";
            rose.transform.position = new Vector3(335.0f, -43.5f, -65.0f);
            rose.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            var col = rose.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.95f, 0.15f, 0.25f, 1f);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", new Color(0.95f, 0.15f, 0.25f, 1f));
            rose.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var c = rose.AddComponent<CollectibleItem>();
            c.itemId = "rose";
            c.itemName = "Forest Rose";
            c.promptVerb = "Pick";
            c.quantity = 1;
            c.disableGameObject = true;
            isDirty = true;
        }

        // Sugarcane Node (along lake/stream bank)
        var sugarcane = GameObject.Find("Sugarcane_Node");
        if (sugarcane == null)
        {
            sugarcane = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            sugarcane.name = "Sugarcane_Node";
            sugarcane.transform.position = new Vector3(338.5f, -43.5f, -79.0f);
            sugarcane.transform.localScale = new Vector3(0.4f, 1.6f, 0.4f);
            var col = sugarcane.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.45f, 0.85f, 0.25f, 1f);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", new Color(0.45f, 0.85f, 0.25f, 1f));
            sugarcane.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var c = sugarcane.AddComponent<CollectibleItem>();
            c.itemId = "sugarcane";
            c.itemName = "Fresh Sugarcane";
            c.promptVerb = "Harvest";
            c.quantity = 1;
            c.disableGameObject = true;
            isDirty = true;
        }

        // 7. Setup Village Workstation (Mortar & Pestle and Empty Glass) in open village work area
        var workstation = GameObject.Find("Village_Workstation");
        if (workstation == null)
        {
            workstation = new GameObject("Village_Workstation");
            workstation.transform.position = new Vector3(186.0f, -44.0f, -92.0f);

            // Table
            var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "Workstation_Table";
            table.transform.SetParent(workstation.transform, false);
            table.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            table.transform.localScale = new Vector3(2.4f, 0.8f, 1.4f);

            var tableMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            tableMat.color = new Color(0.45f, 0.32f, 0.20f, 1f);
            if (tableMat.HasProperty("_BaseColor")) tableMat.SetColor("_BaseColor", new Color(0.45f, 0.32f, 0.20f, 1f));
            table.GetComponent<MeshRenderer>().sharedMaterial = tableMat;

            // Mortar
            var mortar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mortar.name = "Workstation_Mortar";
            mortar.transform.SetParent(workstation.transform, false);
            mortar.transform.localPosition = new Vector3(-0.4f, 1.25f, 0f);
            mortar.transform.localScale = new Vector3(0.6f, 0.35f, 0.6f);

            var mortarMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mortarMat.color = new Color(0.35f, 0.35f, 0.35f, 1f);
            if (mortarMat.HasProperty("_BaseColor")) mortarMat.SetColor("_BaseColor", new Color(0.35f, 0.35f, 0.35f, 1f));
            mortar.GetComponent<MeshRenderer>().sharedMaterial = mortarMat;

            // Pestle
            var pestle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pestle.name = "Workstation_Pestle";
            pestle.transform.SetParent(mortar.transform, false);
            pestle.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            pestle.transform.localEulerAngles = new Vector3(15f, 0f, 15f);
            pestle.transform.localScale = new Vector3(0.2f, 0.8f, 0.2f);
            pestle.GetComponent<MeshRenderer>().sharedMaterial = mortarMat;

            // Workstation Trigger Collider
            var wsCol = workstation.AddComponent<BoxCollider>();
            wsCol.center = new Vector3(0f, 1f, 0f);
            wsCol.size = new Vector3(3.5f, 2.5f, 3.5f);
            wsCol.isTrigger = true;

            workstation.AddComponent<MortarWorkstation>();

            // Empty Glass Collectible on table
            var glass = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            glass.name = "Empty_Glass_Node";
            glass.transform.SetParent(workstation.transform, false);
            glass.transform.localPosition = new Vector3(0.6f, 1.25f, 0f);
            glass.transform.localScale = new Vector3(0.3f, 0.35f, 0.3f);
            var glassCol = glass.GetComponent<Collider>();
            if (glassCol != null) glassCol.isTrigger = true;

            var glassMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            glassMat.color = new Color(0.85f, 0.95f, 1.0f, 0.65f);
            if (glassMat.HasProperty("_BaseColor")) glassMat.SetColor("_BaseColor", new Color(0.85f, 0.95f, 1.0f, 0.65f));
            glass.GetComponent<MeshRenderer>().sharedMaterial = glassMat;

            var glassCollectible = glass.AddComponent<CollectibleItem>();
            glassCollectible.itemId = "empty_glass";
            glassCollectible.itemName = "Empty Glass";
            glassCollectible.promptVerb = "Take";
            glassCollectible.quantity = 1;
            glassCollectible.disableGameObject = true;

            isDirty = true;
        }

        // 8. Setup Grinding Minigame UI
        var minigame = canvasGo.GetComponent<GrindingMinigameUI>();
        if (minigame == null)
        {
            minigame = canvasGo.AddComponent<GrindingMinigameUI>();
            isDirty = true;
        }
        minigame.EnsureUIExists();

        // 7. Setup Village Apothecary / Clinic (House_1.003)
        var clinicHouse = GameObject.Find("Scene_1_map/House_1.003");
        if (clinicHouse != null)
        {
            // Remove monolithic BoxCollider that blocks entrance
            var solidCol = clinicHouse.GetComponent<BoxCollider>();
            if (solidCol != null)
            {
                Object.DestroyImmediate(solidCol);
                isDirty = true;
            }

            // Setup Clinic_Colliders child hierarchy
            var collidersRoot = clinicHouse.transform.Find("Clinic_Colliders");
            if (collidersRoot == null)
            {
                var cGo = new GameObject("Clinic_Colliders");
                cGo.transform.SetParent(clinicHouse.transform, false);
                collidersRoot = cGo.transform;
                isDirty = true;

                // Segmented wall colliders (leaving entrance clear)
                // Left Wall
                CreateBoxColliderChild(collidersRoot, "Wall_Left", new Vector3(-3.5f, 0f, 3.8f), new Vector3(0.5f, 15f, 7.8f));
                // Right Wall
                CreateBoxColliderChild(collidersRoot, "Wall_Right", new Vector3(4.2f, 0f, 3.8f), new Vector3(0.5f, 15f, 7.8f));
                // Back Wall
                CreateBoxColliderChild(collidersRoot, "Wall_Back", new Vector3(0.4f, 0f, 7.7f), new Vector3(7.4f, 15f, 0.5f));
                // Roof
                CreateBoxColliderChild(collidersRoot, "Roof_Collider", new Vector3(0.4f, 7.5f, 3.8f), new Vector3(7.5f, 0.5f, 7.8f));
                // Front Wall Left of Door
                CreateBoxColliderChild(collidersRoot, "Front_Left", new Vector3(-2.4f, 0f, 0.0f), new Vector3(2.5f, 15f, 0.5f));
                // Front Wall Right of Door
                CreateBoxColliderChild(collidersRoot, "Front_Right", new Vector3(3.1f, 0f, 0.0f), new Vector3(2.5f, 15f, 0.5f));
            }

            // Setup Clinic_Interior child hierarchy
            var interiorRoot = clinicHouse.transform.Find("Clinic_Interior");
            if (interiorRoot == null)
            {
                var iGo = new GameObject("Clinic_Interior");
                iGo.transform.SetParent(clinicHouse.transform, false);
                interiorRoot = iGo.transform;
                isDirty = true;

                // Interior Floor
                var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.name = "Interior_Floor";
                floor.transform.SetParent(interiorRoot, false);
                floor.transform.localPosition = new Vector3(0.4f, -4.5f, 3.8f);
                floor.transform.localScale = new Vector3(7.2f, 0.3f, 7.5f);

                // Medicine Crafting Table
                var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
                table.name = "Medicine_Crafting_Table";
                table.transform.SetParent(interiorRoot, false);
                table.transform.localPosition = new Vector3(0.4f, -3.2f, 6.2f);
                table.transform.localScale = new Vector3(3.2f, 1.2f, 1.2f);

                // Interactive Station on Table
                var tableInteractable = table.AddComponent<SimpleInteractable>();
                tableInteractable.promptText = "Examine Medicine Table";
                tableInteractable.responseMessage = "Vaidya's Crafting Table: Used to grind herbs and brew curative remedies.";

                // Mortar prop on table
                var mortar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                mortar.name = "Herbal_Mortar";
                mortar.transform.SetParent(interiorRoot, false);
                mortar.transform.localPosition = new Vector3(0.4f, -2.4f, 6.2f);
                mortar.transform.localScale = new Vector3(0.5f, 0.3f, 0.5f);
            }

            // Setup Functional Door at entrance
            var doorRoot = clinicHouse.transform.Find("Clinic_Door");
            if (doorRoot == null)
            {
                var dGo = new GameObject("Clinic_Door");
                dGo.transform.SetParent(clinicHouse.transform, false);
                dGo.transform.localPosition = new Vector3(0.4f, -4.5f, 0.0f);
                doorRoot = dGo.transform;
                isDirty = true;

                // Door Hinge Pivot
                var hingeGo = new GameObject("Door_Hinge");
                hingeGo.transform.SetParent(doorRoot, false);
                hingeGo.transform.localPosition = new Vector3(-0.9f, 0f, 0f);

                // Door Panel Mesh & Collider
                var panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                panel.name = "Door_Panel";
                panel.transform.SetParent(hingeGo.transform, false);
                panel.transform.localPosition = new Vector3(0.9f, 2.2f, 0f); // Offset so hinge is on the side
                panel.transform.localScale = new Vector3(1.8f, 4.4f, 0.15f);

                var doorInteractable = panel.AddComponent<DoorInteractable>();
                doorInteractable.hingeTransform = hingeGo.transform;
                doorInteractable.openAngle = 90f;
                doorInteractable.transitionDuration = 0.7f;
                doorInteractable.openPrompt = "Open Clinic Door";
                doorInteractable.closePrompt = "Close Clinic Door";
            }
        }

        // 8. Setup Exterior Doorway Triggers on the other 11 houses
        string[] otherHouseNames = new string[]
        {
            "House_1", "House_1.001", "House_1.002",
            "House_2", "House_2.001", "House_2.002", "House_2.003",
            "House_3", "House_3.001", "House_3.002", "House_3.003"
        };

        foreach (var hName in otherHouseNames)
        {
            var hGo = GameObject.Find("Scene_1_map/" + hName);
            if (hGo != null)
            {
                var trig = hGo.transform.Find("Doorway_Knock_Trigger");
                if (trig == null)
                {
                    var tGo = new GameObject("Doorway_Knock_Trigger");
                    tGo.transform.SetParent(hGo.transform, false);
                    tGo.transform.localPosition = new Vector3(0f, 0f, 0f);

                    var bc = tGo.AddComponent<BoxCollider>();
                    bc.isTrigger = true;
                    bc.size = new Vector3(4.0f, 4.0f, 4.0f);

                    var si = tGo.AddComponent<SimpleInteractable>();
                    si.promptText = "Knock on Door";
                    si.responseMessage = "Villager: 'We are resting and recovering from the sickness. Please speak with the Village Elder.'";
                    si.responseDuration = 4.0f;
                    si.isRepeatable = true;

                    isDirty = true;
                }
            }
        }

        if (isDirty)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[InteractionSetup] Stage 2 gameplay systems successfully configured!");
        }

        if (showDialog)
        {
            EditorUtility.DisplayDialog("Stage 2 Setup Complete", "All Stage 2 systems configured:\n- Inventory & HUD\n- 3 Tulsi Herbs\n- Clinic Interior & Door\n- Exterior House Triggers\n- Elder & Well Mission Loop", "OK");
        }
    }

    private static void CreateBoxColliderChild(Transform parent, string name, Vector3 localPos, Vector3 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        var bc = go.AddComponent<BoxCollider>();
        bc.size = size;
        bc.isTrigger = false;
    }
}
#endif
