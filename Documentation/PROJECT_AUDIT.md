# Project Audit – Arogya Adventure

**Date:** November 19, 2026  
**Auditor:** Agentic AI Assistant (Unity MCP)  
**Project Path:** `E:\Arogya_Adventure_Real\Arogya_Adventure_Real`  
**Git Branch:** `ai-dev` (tracking `origin/ai-dev`, clean working tree)  

---

## 1. Executive Summary & Verification

### 1.1 Unity Version & Working Directory
* **Active Unity Editor Version:** `6000.6.2f1` (Unity 6.6)
* **`ProjectSettings/ProjectVersion.txt`:** `m_EditorVersion: 6000.6.2f1` (Revision: `770e33f6875c`)
* **Working Project Path:** `E:\Arogya_Adventure_Real\Arogya_Adventure_Real`  
  *Confirmed: This is the active and intended working Unity project directory.*

### 1.2 Unity MCP Connection Status
* **Status:** **Active & Operational**
* **Session Pin:** `fce029ef` matched to instance `10c99b5c-f13d-4a5f-87c9-060ea960fde7` (`unity:Arogya_Adventure_Real` on `LAPTOP-2IRPDD54`).
* **Authorization:** Currently valid. Scene queries (RQL), schema queries, and console inspection tools execute synchronously without rejection.

---

## 2. Project & Engine Configuration

| Setting / Package | Version / Asset | Audit Finding |
|---|---|---|
| **Render Pipeline** | Universal Render Pipeline (URP) `17.6.0` | Active quality asset is `Assets/Settings/Mobile_RPAsset.asset`. Shaders on environment materials use `Universal Render Pipeline/Lit`. No pink/magenta shader errors. |
| **Input System** | `com.unity.inputsystem` `1.20.0` | Configured with `Assets/InputSystem_Actions.inputactions`. However, existing scripts bypass this asset and poll raw keyboard/mouse directly. |
| **AI Navigation** | `com.unity.ai.navigation` `2.0.14` | Installed, but no NavMesh is baked or configured in the scene. |
| **Build Settings** | `ProjectSettings/EditorBuildSettings.asset` | Single active scene: `Assets/Scenes/SampleScene.unity`. |
| **Core Game Managers** | None | No `GameManager`, `UIManager`, `InventoryManager`, `DialogueManager`, or `QuestManager` exist. |

---

## 3. Player Subsystem Audit

### 3.1 Hierarchy & Transform Discrepancies
* **Root Object:** `Player` at world position `(328.812, -34.520, -62.918)`, rotation `(0, 0, 0)`, scale `(1, 1, 1)`.
* **Visual Mesh Offset:**
  * Child meshes (`Cube.001`, `Cube.002`, `Dhoti_01.001`) and rig `Armature.001` are offset locally by `~ (12.81, 0.31, -7.33)`.
  * The visual character is positioned nearly 15 units away from the GameObject's local origin.
* **Component-Level Compensations:**
  * Instead of centering the 3D model, the `CharacterController` was manually given a local center of `(12.6, 3.0, -7.3)` (Height: `6.19`, Radius: `1.24`).
  * The inactive `BoxCollider` was also offset to `(13.33, 2.80, -7.41)`.
* **Blender Export Scales:**
  * `Armature.001` local scale is `(0.01, 0.01, 0.01)`.
  * Visual meshes have arbitrary non-uniform scales: `Cube.001` is `(0.171, 0.205, 0.319)`.

### 3.2 Physics Conflict: Rigidbody vs CharacterController
* The `Player` GameObject simultaneously holds:
  1. `CharacterController` (`enabled = true`, driving displacement via `controller.Move()`)
  2. `Rigidbody` (`mass = 1`, `useGravity = true`, `isKinematic = false`, `collisionDetectionMode = Continuous`)
* **Conflict:** `CharacterController` is designed for kinematic displacement where gravity is integrated via code. Having a non-kinematic `Rigidbody` with active PhysX gravity causes competing gravity calculations, jitter, unpredictable ground checks, and velocity fighting.
* **Redundant Component:** `BoxCollider` is disabled (`enabled = false`) and serves no purpose.

### 3.3 Animator & Motion Analysis
* **Controller:** `Assets/PlayerWalk.controller` (assigned to Player Animator).
* **Avatar:** `Sadhu_rigged_walking_animationAvatar`.
* **Critical Flaw:**
  * The Animator Controller defines two states: `Idle` and `Walk`.
  * **Both states point to the exact same motion clip:** `{fileID: 4781951162920444690, guid: 790a0814fdc619d42a4edd70080cd219}` (`Assets/Sadhu_rigged_walking_animation.fbx`).
  * As a result, the player constantly performs a walking animation even when stationary.
  * No true idle animation clip, running animation clip, or interaction clip exists in project assets.

### 3.4 Camera Configuration & Negative Scale
* **Path:** `Player/Main Camera` (child of `Player`).
* **Local Transform:** Position `(12.42, 4.96, -9.41)`, Rotation `(0, 278.342, 0)`, **Scale `(-1.2142, 1, 1)`**.
* **Issues:**
  1. Negative X scale mirrors the camera projection matrix and can invert frustum culling.
  2. Directly parenting the camera to the player without a spring arm / sphere-cast means any rotation or movement causes the camera to clip through geometry.
  3. Mouse look logic in [MouseLook.cs](file:///e:/Arogya_Adventure_Real/Arogya_Adventure_Real/Assets/MouseLook.cs) hardcodes player rotation on startup (`Quaternion.Euler(0, -90, 0)`).

---

## 4. Environment & Map Audit

### 4.1 Map Architecture
* The entire level environment is instantiated from a single Blender model: `Assets/Scene_1_map.blend`.
* Scene root `Scene_1_map` contains 212 GameObjects:
  * 133 modular mesh objects (numbered pieces and landmark meshes).
  * 1 embedded character rig (`Armature` + `Cube` with `Villager_1` texture).
* **Major Landmarks Identified:**
  * `Path_to_village`: `(282.65, -43.77, -63.75)`
  * `Bridge`: `(294.61, -43.77, -60.75)`
  * `Well`: `(174.63, -44.02, -94.83)`
  * `House_1` / `House_2` / `House_3` clusters (village center around `(214, -36, -45)`)
  * `Lake`: `(128.74, -43.03, -255.28)`
  * `River`: `(336.34, -43.03, -305.68)`
  * `WaterFall`: `(341.45, -7.00, -395.20)`
  * High bridges: `Bridge.001` (`276, -43, -304`), `Bridge.002` (`92, -43, -243`)

### 4.2 Severe Collider Deficiency
* **Current State:** Across all 212 environment objects, **only one collider exists**: `Scene_1_map/Ground` (a scaled BoxCollider).
* **Impact:** All buildings, walls, bridges, rocks, fences, and cliff edges have **no colliders**. The player currently passes straight through all solid environmental geometry and will fall off bridges without collision boundaries.

---

## 5. Console & Diagnostic Summary
* **Errors:** 1 transient MCP plugin authentication error from initial connection (resolved).
* **Warnings:** 3 token timeout warnings from startup (resolved).
* **Compilation:** Clean. 0 C# compiler errors.
