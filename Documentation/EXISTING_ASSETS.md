# Existing Assets & Content Audit – Arogya Adventure

**Date:** November 19, 2026  
**Project Path:** `E:\Arogya_Adventure_Real\Arogya_Adventure_Real`  

---

## 1. 3D Models & Meshes

| Asset File | Format | Contents & Hierarchy | Utilization |
|---|---|---|---|
| `Assets/Sadhu_rigged_walking_animation.fbx` | FBX | Character meshes (`Cube`, `Cube.001`, `Cube.002`, `Dhoti_01`), Mixamo humanoid skeleton (`Armature.001`), and animation clip `Armature\|ArmatureAction`. | Active Player character model and walking motion. |
| `Assets/Scene_1_map.blend` | Blender | Full terrain, village houses (`House_1` to `House_3`), 3 bridges, river, lake, waterfall, trees (`Tree2` to `Tree5`), rocks, fences, walls, well, and an embedded NPC character (`Cube` + `Armature`). | Active game environment (`Scene_1_map`). |

---

## 2. Textures & Materials

### 2.1 Materials
* **Total Dedicated `.mat` Files in Assets:** `0`
* All scene materials are embedded directly within `Scene_1_map.blend` (53 materials) and `Sadhu_rigged_walking_animation.fbx` (3 materials).
* All materials are bound to the **Universal Render Pipeline/Lit** shader (`Universal Render Pipeline/Lit`), rendering accurately with standard PBR lighting in Unity 6.

### 2.2 Textures (`Assets/textures/`)
The project includes 35 source textures covering PBR channels (Base Color/Diffuse, Normal, Roughness, Metallic):
* **Environment Architecture:** `thatched_roof_2k_*`, `medieval_blocks_03_*`, `rock_wall_08_*`, `planks_*`, `weathered_brown_planks_*`, `roof.jpg`, `wood*`.
* **Nature & Terrain:** `grass.jpg`, `rock.jpg`, `atlas_2k_*`.
* **Character Texture:** `Villager_1.png` (PBR texture map for village NPC).

---

## 3. Character & NPC Audit

### 3.1 Player Character (Sadhu)
* **Visual Components:** Head, Torso (`Cube.001`), Limbs (`Cube.002`), Dhoti cloth (`Dhoti_01.001`).
* **Rig:** Mixamo Humanoid standard armature (`mixamorig:Hips` down to individual finger and toe bones).
* **Avatar:** `Sadhu_rigged_walking_animationAvatar`.
* **Status:** Fully rigged and functional, but lacks idle motion and has non-centered pivot offsets.

### 3.2 Village NPC (Elder / Resident)
* **Location in Scene:** `Scene_1_map/Cube` and `Scene_1_map/Armature` at world position `(316.51, -43.71, -84.17)` near the village entrance.
* **Mesh & Texture:** Humanoid skinned mesh (`Cube`) textured with `Villager_1.png`.
* **Rig:** Fully rigged Mixamo humanoid armature (`Scene_1_map/Armature`), identical bone naming to player.
* **Current Status:** Lacks an `Animator` component (stands in rest/T-pose). Can immediately be turned into an interactive Quest Giver / Elder NPC by adding an Animator sharing the Sadhu avatar or humanoid idle controller.

---

## 4. Environment Landmarks & Interactive Props

The `Scene_1_map.blend` contains distinct, reusable landmark meshes suitable for quest objectives:

| Landmark Name | World Position `(X, Y, Z)` | Mission Suitability |
|---|---|---|
| `Path_to_village` | `(282.65, -43.77, -63.75)` | Tutorial / Player Arrival route |
| `Bridge` (Main Entrance) | `(294.61, -43.77, -60.75)` | Village gateway and orientation |
| `Well` | `(174.63, -44.02, -94.83)` | Mission 2: Pure Water source / Well restoration |
| `House_1`, `House_2`, `House_3` (Clusters) | `(171, -43, -33)` to `(243, -30, -85)` | NPC residences, apothecary clinic, herbal workshop |
| `Lake` & `River` | `(128.74, -43.03, -255.28)` | Mission 3: Riparian herbal foraging grounds |
| `Bridge.001` & `Bridge.002` | `(276, -43, -304)` & `(92, -43, -243)` | Mountain passage transitions |
| `WaterFall` | `(341.45, -7.00, -395.20)` | Mission 4: Sacred Mountain Waterfall / Altar |
| `Tree2`, `Tree3`, `Tree4`, `Tree5` | Spread across valley | Forest boundaries and herb spawn anchors |
| `Bundle_of_rocks`, `block.001` | Spread near cliffs and well | Obstacles or herbal harvest rocks |

---

## 5. Asset Gap Analysis

### 5.1 Reusable Assets (Already Present)
1. **Full 3D Environment:** Complete terrain, village buildings, rivers, bridges, and mountain backdrop.
2. **2 Rigged Humanoid Characters:** Player (Sadhu) and Village NPC (Villager 1).
3. **PBR Texture Library:** High-resolution architectural and organic materials.
4. **Walking Animation:** Functioning humanoid walk cycle in `Sadhu_rigged_walking_animation.fbx`.

### 5.2 Genuinely Missing Assets & Pragmatic Solutions

| Missing Asset | Impact | Practical Development Solution |
|---|---|---|
| **Humanoid Idle Animation** | Player walks in place when idle. | Sample frame 0 of the walk cycle into a 1-frame looping clip, export a static humanoid pose clip, or source a standard public-domain CC0 humanoid idle. |
| **Medicinal Herb Models** | No standalone botanical pickup meshes. | Instantiate and scale existing foliage sub-elements, or create clean low-poly herb prefabs using Unity primitives with distinctive colored emissive URP materials. |
| **UI Canvas & Icons** | No HUD, dialogue boxes, or quest trackers. | Construct clean UGUI Canvas screens (Text, Panels, Sliders) using Unity built-in sprites and modern styling. |
| **Audio (SFX & Music)** | No ambient nature sounds, footsteps, or dialogue chimes. | Utilize lightweight procedural/royalty-free audio clips (or fallback visual UI audio cues) for collection and quest milestones. |
| **Quest Item Icons** | No 2D sprites for inventory items. | Generate crisp 2D procedural UI icons or render snapshot sprites of herb prefabs. |
