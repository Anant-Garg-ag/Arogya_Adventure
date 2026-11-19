# Technical Risks & Missing Features – Arogya Adventure

**Date:** November 19, 2026  
**Project Path:** `E:\Arogya_Adventure_Real\Arogya_Adventure_Real`  

---

## 1. Critical Technical Risks & Bugs

### 1.1 Physics Conflict: Dual Rigidbody & CharacterController
* **Risk:** The `Player` GameObject currently has both a non-kinematic `Rigidbody` with active gravity AND a `CharacterController` with code-driven gravity in [PlayerMovement.cs](file:///e:/Arogya_Adventure_Real/Arogya_Adventure_Real/Assets/PlayerMovement.cs).
* **Failure Mode:** Jittery movement, erratic velocity stacking on slopes, double-gravity acceleration downward, and conflict between PhysX physics queries and character displacement.
* **Mitigation:** In Stage 1, remove or disable the `Rigidbody` (or mark it `isKinematic = true` with all constraints frozen) and rely exclusively on `CharacterController.Move()`.

### 1.2 Pervasive Lack of Environmental Colliders
* **Risk:** Across all 212 GameObjects in `Scene_1_map`, only `Ground` has a collider.
* **Failure Mode:** The player will walk directly through all houses, stone walls, trees, rocks, and will fall into water or off bridge edges.
* **Mitigation:** Prioritize an environment collider pass on key navigable routes (main village bridge, house footprints, well boundary, waterfall ledge).

### 1.3 Negative Camera Scale & Rigid Parent Structure
* **Risk:** `Player/Main Camera` has a local scale of `(-1.2142, 1, 1)`.
* **Failure Mode:** Negative scale mirrors projection coordinates, inverts polygon culling on some platforms, and produces visual rendering artifacts. The rigid child attachment also causes the camera to clip through roofs and terrain when pitching up/down.
* **Mitigation:** Normalize camera local scale to `(1, 1, 1)` and introduce a smoothed orbit follow script with sphere-cast obstacle detection.

### 1.4 Offset Player Visual Mesh & Armature
* **Risk:** Visual character children (`Armature.001`, `Cube.001`, `Cube.002`, `Dhoti_01.001`) are offset by `~ (12.8, 0.3, -7.3)` from the `Player` GameObject origin.
* **Failure Mode:** Character rotation occurs around an axis 13 units away from the visual body, causing wide swing arcs when turning.
* **Mitigation:** Re-center child meshes and rig to `(0, 0, 0)` relative to `Player` parent, and reset `CharacterController.center` to `(0, height/2, 0)`.

### 1.5 Animator State Configuration Flaw
* **Risk:** `Assets/PlayerWalk.controller` assigns the same walking clip to both `Idle` and `Walk`.
* **Failure Mode:** Player appears to be walking vigorously even when standing completely still.
* **Mitigation:** Assign a true idle pose/clip to the `Idle` state, or dampen animator speed to 0 when `isWalking == false`.

### 1.6 Bypassed Input System
* **Risk:** `Assets/InputSystem_Actions.inputactions` is installed and registered in project settings, but [PlayerMovement.cs](file:///e:/Arogya_Adventure_Real/Arogya_Adventure_Real/Assets/PlayerMovement.cs) and [MouseLook.cs](file:///e:/Arogya_Adventure_Real/Arogya_Adventure_Real/Assets/MouseLook.cs) poll `Keyboard.current` and `Mouse.current` directly.
* **Failure Mode:** Controller/gamepad support is broken; null-reference exceptions occur if input devices disconnect or run in non-standard window focus.
* **Mitigation:** Migrate controls to standard action callbacks or encapsulate input reading with null-guards.

---

## 2. Missing Features & Asset Gaps

| Feature Area | Current Gap | Recommended Lean Solution |
|---|---|---|
| **Character Animations** | Missing idle, run, and interaction animations. | 1. Implement 0-speed walk pose fallback for idle.<br>2. Optionally import CC0 standard humanoid idle clip for Mixamo rig. |
| **NPC Behavior** | Village NPC (`Scene_1_map/Cube`) has no Animator or interaction component. | Add Animator sharing Sadhu avatar, attach `NPCInteractable` script, configure dialogue trigger. |
| **Medicinal Herbs** | No dedicated plant models for harvesting. | Create clean, distinctive low-poly herb prefabs using Unity primitives with emissive botanical URP materials (Tulsi = emerald green, Ashwagandha = golden amber). |
| **Crafting Mechanism** | No crafting/brewing logic or workstation UI. | Implement simple recipe check (`Water + Herbs = Sanjeevani Kadha`) via an interactive trigger at the village apothecary house. |
| **Audio & SFX** | No audio sources, footstep sounds, or ambient audio. | Implement a lightweight `AudioManager` with audio cues for collection, quest progress, and completion. |
| **UI / HUD** | No HUD, interaction prompt, or quest guide. | Create a modular UGUI Canvas displaying Objective text, prompt banner ("Press [E] to Interact"), and herb inventory counter. |

---

## 3. Scope Control & Anti-Patterns to Avoid
1. **No External Asset Store Bloat:** Do not download heavy third-party controller packages or massive nature packs that could break Unity 6 URP compatibility.
2. **No Scene Destruction:** Do not modify or unpack `Assets/Scene_1_map.blend`. Keep all additions as modular game objects and prefabs.
3. **No Over-Engineered Architecture:** Avoid bloated state machines, complex inventory grids, or multi-branch dialogue trees. The 6-mission story must remain linear, robust, and completed within the 15–20 hour budget.
