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
        if (player != null && (player.GetComponent<PlayerInteraction>() == null || GameObject.Find("Tulsi_Herb_2") == null || GameObject.Find("Clinic_Door") == null || GameObject.Find("Elder_Door") == null))
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
            npc.isVillageElder = true;
            npc.mission1IncompleteDialogue = "I am worried about the sickness spreading through our village. Please bring me three Tulsi leaves from near the stream.";
            npc.mission1CompleteDialogue = "Thank you. The Tulsi will help us prepare a remedy. Now, I need you to investigate the village well. Something is wrong with its water.";
            npc.mission2ActiveDialogue = "Please inspect the village well. We must discover why its water is tainted.";

            var col = elderCube.GetComponent<BoxCollider>();
            if (col == null)
            {
                col = elderCube.AddComponent<BoxCollider>();
            }
            col.center = new Vector3(0.5f, 6.0f, 0f);
            col.size = new Vector3(14.0f, 15.0f, 12.0f);
            col.isTrigger = true;
            isDirty = true;

            var smr = elderCube.GetComponent<SkinnedMeshRenderer>();
            if (smr != null)
            {
                smr.updateWhenOffscreen = true;
            }

            // Relocate Elder inside House_2.001 on the Dais
            var elderHouse = GameObject.Find("Scene_1_map/House_2.001");
            var armature = GameObject.Find("Scene_1_map/Armature");
            if (elderHouse != null)
            {
                Vector3 daisLocal = new Vector3(4.12f, 4.5f, -3.20f);
                Vector3 daisWorld = elderHouse.transform.TransformPoint(daisLocal);
                if (Vector3.Distance(elderCube.transform.position, daisWorld) > 3.0f)
                {
                    Vector3 delta = daisWorld - elderCube.transform.position;
                    elderCube.transform.position += delta;
                    if (armature != null)
                    {
                        armature.transform.position += delta;
                    }
                    isDirty = true;
                }
                if (armature != null)
                {
                    armature.transform.eulerAngles = new Vector3(270f, 193f, 0f);
                    isDirty = true;
                }
            }

            // Check if Animator can be attached for idle pose
            var elderAnimator = elderCube.GetComponent<Animator>();
            if (elderAnimator == null && armature != null)
            {
                elderAnimator = elderCube.AddComponent<Animator>();
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
            var wellInteractable = well.GetComponent<SimpleInteractable>();
            if (wellInteractable == null)
            {
                wellInteractable = well.AddComponent<SimpleInteractable>();
                isDirty = true;
            }
            wellInteractable.promptText = "Inspect Well";
            wellInteractable.isVillageWell = true;
            wellInteractable.mission1ActiveMessage = "The well water is stagnant and murky. The Village Elder needs the medicinal Tulsi leaves before we can investigate properly.";
            wellInteractable.mission2InvestigationMessage = "You examine the well closely: A toxic dark residue coats the stones. The water source is contaminated! (Mission 2 Completed)";
            wellInteractable.isRepeatable = true;

            var col = well.GetComponent<Collider>();
            if (col == null)
            {
                var boxCol = well.AddComponent<BoxCollider>();
                boxCol.isTrigger = true;
                isDirty = true;
            }
        }

        // 6. Setup 3 Tulsi Herb Collectibles along the stream/pathway
        Vector3[] herbPositions = new Vector3[]
        {
            new Vector3(333.5f, -43.6f, -72.0f), // Near spawn
            new Vector3(338.5f, -43.5f, -79.0f), // Along stream bank
            new Vector3(343.5f, -43.4f, -86.5f)  // Further down stream bank
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
            else
            {
                var collectible = h.GetComponent<CollectibleItem>();
                if (collectible != null)
                {
                    collectible.itemId = "tulsi";
                    collectible.itemName = "Tulsi Leaf (Medicinal Herb)";
                    collectible.quantity = 1;
                }
            }
        }

        Material planksMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Wood_Planks_Mat.mat");
        Material furnitureMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Wood_Furniture_Mat.mat");

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

            // Clean up any legacy children
            var oldColliders = clinicHouse.transform.Find("Clinic_Colliders");
            if (oldColliders != null) Object.DestroyImmediate(oldColliders.gameObject);
            var oldInterior = clinicHouse.transform.Find("Clinic_Interior");
            if (oldInterior != null) Object.DestroyImmediate(oldInterior.gameObject);
            var oldDoor = clinicHouse.transform.Find("Clinic_Door");
            if (oldDoor != null) Object.DestroyImmediate(oldDoor.gameObject);

            // Recreate Clinic_Colliders child hierarchy (segmented perimeter walls, doorway clear)
            var cGo = new GameObject("Clinic_Colliders");
            cGo.transform.SetParent(clinicHouse.transform, false);
            var collidersRoot = cGo.transform;

            CreateBoxColliderChild(collidersRoot, "Wall_Left", new Vector3(-3.4f, -0.3f, 3.8f), new Vector3(0.4f, 16.0f, 7.6f));
            CreateBoxColliderChild(collidersRoot, "Wall_Right", new Vector3(4.5f, -0.3f, 3.8f), new Vector3(0.4f, 16.0f, 7.6f));
            CreateBoxColliderChild(collidersRoot, "Wall_Back", new Vector3(0.55f, 8.2f, 3.8f), new Vector3(7.5f, 0.4f, 7.6f));
            CreateBoxColliderChild(collidersRoot, "Roof_Collider", new Vector3(0.55f, -0.3f, 7.6f), new Vector3(7.5f, 16.0f, 0.4f));
            CreateBoxColliderChild(collidersRoot, "Front_Left", new Vector3(-2.2f, -8.2f, 3.8f), new Vector3(2.2f, 0.4f, 7.6f));
            CreateBoxColliderChild(collidersRoot, "Front_Right", new Vector3(3.3f, -8.2f, 3.8f), new Vector3(2.2f, 0.4f, 7.6f));
            CreateBoxColliderChild(collidersRoot, "Front_Lintel", new Vector3(0.55f, -8.2f, 5.8f), new Vector3(3.3f, 0.4f, 3.6f));

            // Setup Clinic_Interior child hierarchy
            var iGo = new GameObject("Clinic_Interior");
            iGo.transform.SetParent(clinicHouse.transform, false);
            var interiorRoot = iGo.transform;

            // Interior Floor (horizontal in world space: thickness in local Z)
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Interior_Floor";
            floor.transform.SetParent(interiorRoot, false);
            floor.transform.localPosition = new Vector3(0.55f, -0.3f, 0.05f);
            floor.transform.localScale = new Vector3(7.4f, 16.0f, 0.1f);
            if (planksMat != null) floor.GetComponent<MeshRenderer>().sharedMaterial = planksMat;

            // Medicine Crafting Table
            var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "Medicine_Crafting_Table";
            table.transform.SetParent(interiorRoot, false);
            table.transform.localPosition = new Vector3(0.55f, 4.5f, 0.7f);
            table.transform.localScale = new Vector3(3.0f, 1.2f, 0.8f);
            if (furnitureMat != null) table.GetComponent<MeshRenderer>().sharedMaterial = furnitureMat;

            var tableInteractable = table.AddComponent<SimpleInteractable>();
            tableInteractable.promptText = "Examine Medicine Table";
            tableInteractable.responseMessage = "Vaidya's Crafting Table: Used to grind herbs and brew curative remedies.";
            tableInteractable.isRepeatable = true;

            // Mortar prop on table
            var mortar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mortar.name = "Herbal_Mortar";
            mortar.transform.SetParent(interiorRoot, false);
            mortar.transform.localPosition = new Vector3(0.55f, 4.5f, 1.25f);
            mortar.transform.localScale = new Vector3(0.4f, 0.3f, 0.4f);
            if (furnitureMat != null) mortar.GetComponent<MeshRenderer>().sharedMaterial = furnitureMat;

            // Medicine Shelves along left wall
            var shelves = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shelves.name = "Medicine_Shelves";
            shelves.transform.SetParent(interiorRoot, false);
            shelves.transform.localPosition = new Vector3(-3.0f, 1.0f, 1.5f);
            shelves.transform.localScale = new Vector3(0.6f, 3.0f, 3.0f);
            if (furnitureMat != null) shelves.GetComponent<MeshRenderer>().sharedMaterial = furnitureMat;

            // Setup Functional Door at entrance
            var dGo = new GameObject("Clinic_Door");
            dGo.transform.SetParent(clinicHouse.transform, false);
            dGo.transform.localPosition = new Vector3(0.55f, -8.2f, 0.0f);
            var doorRoot = dGo.transform;

            var hingeGo = new GameObject("Door_Hinge");
            hingeGo.transform.SetParent(doorRoot, false);
            hingeGo.transform.localPosition = new Vector3(-1.0f, 0f, 0f);
            hingeGo.transform.localEulerAngles = new Vector3(90f, 0f, 0f);

            var panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = "Door_Panel";
            panel.transform.SetParent(hingeGo.transform, false);
            panel.transform.localPosition = new Vector3(1.0f, 1.8f, 0f);
            panel.transform.localScale = new Vector3(2.0f, 3.6f, 0.15f);
            if (planksMat != null) panel.GetComponent<MeshRenderer>().sharedMaterial = planksMat;

            var doorInteractable = panel.AddComponent<DoorInteractable>();
            doorInteractable.hingeTransform = hingeGo.transform;
            doorInteractable.openAngle = -90f;
            doorInteractable.transitionDuration = 0.7f;
            doorInteractable.openPrompt = "Open Clinic Door";
            doorInteractable.closePrompt = "Close Clinic Door";

            isDirty = true;
        }

        // 8. Setup Elder's House (House_2.001)
        var elderHouseGo = GameObject.Find("Scene_1_map/House_2.001");
        if (elderHouseGo != null)
        {
            // Remove monolithic BoxCollider
            var solidCol = elderHouseGo.GetComponent<BoxCollider>();
            if (solidCol != null)
            {
                Object.DestroyImmediate(solidCol);
                isDirty = true;
            }

            // Remove any old generic trigger or children
            var knockTrig = elderHouseGo.transform.Find("Doorway_Knock_Trigger");
            if (knockTrig != null) Object.DestroyImmediate(knockTrig.gameObject);
            var oldColliders = elderHouseGo.transform.Find("Elder_Colliders");
            if (oldColliders != null) Object.DestroyImmediate(oldColliders.gameObject);
            var oldInterior = elderHouseGo.transform.Find("Elder_Interior");
            if (oldInterior != null) Object.DestroyImmediate(oldInterior.gameObject);
            var oldDoor = elderHouseGo.transform.Find("Elder_Door");
            if (oldDoor != null) Object.DestroyImmediate(oldDoor.gameObject);

            // Setup Elder_Colliders child hierarchy
            var cGo = new GameObject("Elder_Colliders");
            cGo.transform.SetParent(elderHouseGo.transform, false);
            var collidersRoot = cGo.transform;

            // Segmented wall colliders (leaving portals clear)
            CreateBoxColliderChild(collidersRoot, "Wall_Left", new Vector3(-0.75f, 0f, 0.85f), new Vector3(0.4f, 19.0f, 8.5f));
            CreateBoxColliderChild(collidersRoot, "Wall_Right", new Vector3(8.95f, 0f, 0.85f), new Vector3(0.4f, 19.0f, 8.5f));
            CreateBoxColliderChild(collidersRoot, "Wall_Back", new Vector3(4.12f, 9.5f, 0.85f), new Vector3(9.5f, 0.4f, 8.5f));
            CreateBoxColliderChild(collidersRoot, "Roof_Collider", new Vector3(4.12f, 0f, 5.1f), new Vector3(9.5f, 19.0f, 0.4f));
            CreateBoxColliderChild(collidersRoot, "Front_Left", new Vector3(0.35f, -9.5f, 0.85f), new Vector3(2.2f, 0.4f, 8.5f));
            CreateBoxColliderChild(collidersRoot, "Front_Right", new Vector3(7.85f, -9.5f, 0.85f), new Vector3(2.2f, 0.4f, 8.5f));
            CreateBoxColliderChild(collidersRoot, "Front_Center", new Vector3(4.12f, -9.5f, 0.85f), new Vector3(2.0f, 0.4f, 8.5f));
            CreateBoxColliderChild(collidersRoot, "Portal_Left_Lintel", new Vector3(2.3f, -9.5f, 2.8f), new Vector3(1.8f, 0.4f, 4.5f));
            CreateBoxColliderChild(collidersRoot, "Portal_Right_Lintel", new Vector3(5.9f, -9.5f, 2.8f), new Vector3(1.8f, 0.4f, 4.5f));

            // Setup Elder_Interior child hierarchy
            var iGo = new GameObject("Elder_Interior");
            iGo.transform.SetParent(elderHouseGo.transform, false);
            var interiorRoot = iGo.transform;

            // Interior Floor (horizontal in world space: thickness in local Z)
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Interior_Floor";
            floor.transform.SetParent(interiorRoot, false);
            floor.transform.localPosition = new Vector3(4.12f, 0.0f, -3.50f);
            floor.transform.localScale = new Vector3(9.4f, 19.0f, 0.1f);
            if (planksMat != null) floor.GetComponent<MeshRenderer>().sharedMaterial = planksMat;

            // Elder Dais (raised wooden platform at back of the room)
            var dais = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dais.name = "Elder_Dais";
            dais.transform.SetParent(interiorRoot, false);
            dais.transform.localPosition = new Vector3(4.12f, 4.5f, -3.35f);
            dais.transform.localScale = new Vector3(4.0f, 3.5f, 0.3f);
            if (furnitureMat != null) dais.GetComponent<MeshRenderer>().sharedMaterial = furnitureMat;

            // Elder Council / Meeting Table
            var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "Elder_Meeting_Table";
            table.transform.SetParent(interiorRoot, false);
            table.transform.localPosition = new Vector3(4.12f, 1.5f, -3.10f);
            table.transform.localScale = new Vector3(3.0f, 1.4f, 0.7f);
            if (furnitureMat != null) table.GetComponent<MeshRenderer>().sharedMaterial = furnitureMat;

            var tableInteractable = table.AddComponent<SimpleInteractable>();
            tableInteractable.promptText = "Examine Council Table";
            tableInteractable.responseMessage = "The Village Council Table: Ancient maps and Ayurvedic scrolls are laid out for consultation.";
            tableInteractable.isRepeatable = true;

            // Visitor Benches
            var benchLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            benchLeft.name = "Visitor_Bench_Left";
            benchLeft.transform.SetParent(interiorRoot, false);
            benchLeft.transform.localPosition = new Vector3(1.5f, 1.5f, -3.25f);
            benchLeft.transform.localScale = new Vector3(0.8f, 2.5f, 0.45f);
            if (furnitureMat != null) benchLeft.GetComponent<MeshRenderer>().sharedMaterial = furnitureMat;

            var benchRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            benchRight.name = "Visitor_Bench_Right";
            benchRight.transform.SetParent(interiorRoot, false);
            benchRight.transform.localPosition = new Vector3(6.7f, 1.5f, -3.25f);
            benchRight.transform.localScale = new Vector3(0.8f, 2.5f, 0.45f);
            if (furnitureMat != null) benchRight.GetComponent<MeshRenderer>().sharedMaterial = furnitureMat;

            // Setup Functional Door at main left entrance portal
            var dGo = new GameObject("Elder_Door");
            dGo.transform.SetParent(elderHouseGo.transform, false);
            dGo.transform.localPosition = new Vector3(2.3f, -9.5f, -3.5f);
            var doorRoot = dGo.transform;

            var hingeGo = new GameObject("Door_Hinge");
            hingeGo.transform.SetParent(doorRoot, false);
            hingeGo.transform.localPosition = new Vector3(-0.8f, 0f, 0f);
            hingeGo.transform.localEulerAngles = new Vector3(90f, 0f, 0f);

            var panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = "Door_Panel";
            panel.transform.SetParent(hingeGo.transform, false);
            panel.transform.localPosition = new Vector3(0.8f, 1.8f, 0f);
            panel.transform.localScale = new Vector3(1.6f, 3.6f, 0.15f);
            if (planksMat != null) panel.GetComponent<MeshRenderer>().sharedMaterial = planksMat;

            var doorInteractable = panel.AddComponent<DoorInteractable>();
            doorInteractable.hingeTransform = hingeGo.transform;
            doorInteractable.openAngle = -90f;
            doorInteractable.transitionDuration = 0.7f;
            doorInteractable.openPrompt = "Open Elder's House";
            doorInteractable.closePrompt = "Close Door";

            isDirty = true;
        }

        // 9. Setup Exterior Doorway Triggers on the other 10 houses
        string[] otherHouseNames = new string[]
        {
            "House_1", "House_1.001", "House_1.002",
            "House_2", "House_2.002", "House_2.003",
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
