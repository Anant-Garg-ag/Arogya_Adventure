# Scene Hierarchy Audit – Arogya Adventure

**Scene:** `Assets/Scenes/SampleScene.unity`  
**Total GameObjects:** 297  
**Total Root GameObjects:** 4  

---

## 1. Top-Level Scene Structure

```
SampleScene (Active Scene)
├── Directional Light             [Root - Lighting]
├── Global Volume                 [Root - Post-Processing / Sky]
├── Scene_1_map                   [Root - Environment & NPCs (212 GameObjects)]
└── Player                        [Root - Player Character & Camera (83 GameObjects)]
```

---

## 2. Root Object Breakdowns

### 2.1 Directional Light
* **Components:** `Transform`, `Light`, `UniversalAdditionalLightData`
* **Transform:** Position `(0, 3, 0)`, Rotation `(50, -30, 0)`
* **Role:** Primary sunlight illumination for URP Lit materials with soft shadow casting.

### 2.2 Global Volume
* **Components:** `Transform`, `Volume`
* **Transform:** Position `(0, 0, 0)`, Rotation `(0, 0, 0)`
* **Profile:** References `Assets/Scenes/SampleScene/Global Volume Profile.asset`
* **Role:** Scene-wide tone mapping, exposure, and atmospheric color grading.

---

### 2.3 Scene_1_map (Environment Hierarchy)
Instantiated from `Assets/Scene_1_map.blend`. Contains 212 GameObjects structured into three functional groups:

#### A. Terrain Base & Active Colliders
* **`Scene_1_map/Ground`**
  * Components: `Transform`, `MeshFilter`, `MeshRenderer`, `BoxCollider`
  * Position: `(128.55, -44.17, -72.73)`, Scale: `(455.42, 455.42, 455.42)`
  * *Note: This is the ONLY environmental collider currently present in the entire scene.*

#### B. Architectural, Terrain & Nature Landmark Meshes (133 Objects)
* **Bridges:** `Bridge` (Entrance), `Bridge.001` (River Crossing), `Bridge.002` (High Chasm)
* **Village Buildings:**
  * `House_1`, `House_1.001`, `House_1.002`, `House_1.003`
  * `House_2`, `House_2.001`, `House_2.002`, `House_2.003`
  * `House_3`, `House_3.001`, `House_3.002`, `House_3.003`
* **Water & Nature:** `Lake`, `River`, `WaterFall`, `Path_to_village`, `Well`
* **Foliage & Props:** `Tree2`, `Tree3`, `Tree4`, `Tree5`, `Wall`, `fence`, `door.001`, `Bundle_of_rocks`, `block.001`
* **Modular Geometry Chunks:** Objects named numerically (`32.001`–`32.005`, `49`–`49.001`, `53`–`53.006`, `59`–`59.003`, `60`–`60.001`, `62`–`64.001`, `67`, `68`, `70`–`70.005`, `74`–`79`, `85`, `114`, `121`–`121.002`) representing rock faces, building extensions, roofs, and terrain contours.

#### C. Embedded Village NPC Hierarchy (26 Objects)
* **`Scene_1_map/Cube`**: SkinnedMeshRenderer using `Material.012` with `Villager_1.png` texture.
* **`Scene_1_map/Armature`**: Humanoid skeleton rooted at `mixamorig:Hips`:
  * Spine Chain: `mixamorig:Spine` -> `Spine1` -> `Spine2` -> `Neck` -> `Head`
  * Upper Limbs: Left/Right Shoulders, Arms, ForeArms
  * Lower Limbs: Left/Right UpLegs, Legs, Feet, Toes
  * *Note: Currently unassigned Animator; stands in static bind pose at `(316.51, -43.71, -84.17)`.*

---

### 2.4 Player Hierarchy (83 GameObjects)
Root GameObject: `Player` at `(328.812, -34.520, -62.918)`.

```
Player (Components: Transform, Rigidbody, BoxCollider [Disabled], CharacterController, PlayerMovement, Animator)
├── Armature.001                  [Humanoid Bone Hierarchy - 78 bones]
│   └── mixamorig:Hips
│       ├── mixamorig:Spine -> Spine1 -> Spine2 -> Neck -> Head
│       │   ├── LeftShoulder -> LeftArm -> LeftForeArm -> LeftHand (with 15 finger bones)
│       │   └── RightShoulder -> RightArm -> RightForeArm -> RightHand (with 15 finger bones)
│       ├── mixamorig:LeftUpLeg -> LeftLeg -> LeftFoot -> LeftToeBase -> LeftToe_End
│       └── mixamorig:RightUpLeg -> RightLeg -> RightFoot -> RightToeBase -> RightToe_End
├── Cube.001                      [SkinnedMeshRenderer - Upper Body/Torso]
├── Cube.002                      [SkinnedMeshRenderer - Limbs]
│   └── Dhoti_01.001              [SkinnedMeshRenderer - Cloth / Dhoti]
└── Main Camera                   [Camera, AudioListener, UniversalAdditionalCameraData, MouseLook]
```

#### Key Hierarchy Findings on Player:
1. **Mesh Offset:** The visual children (`Armature.001`, `Cube.001`, `Cube.002`, `Dhoti_01.001`) have a local offset of roughly `(12.8, 0.3, -7.3)` relative to the `Player` root.
2. **Camera Placement:** `Main Camera` is childed directly to `Player` with local position `(12.42, 4.96, -9.41)`, local rotation `(0, 278.34, 0)`, and negative scale `(-1.214, 1, 1)`.
3. **Rigid Child Structure:** Mouse look rotates the entire player GameObject yaw, while pitch tilts the camera.
