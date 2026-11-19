# Implementation Plan – Arogya Adventure (6-Mission Game)

**Target Timeline:** 15–20 focused development hours (Maximum 4 days)  
**Constraints Enforced:**
* No extra external packages installed.
* Pure Unity 6 standard tools (`CharacterController`, UGUI, URP, new Input System).
* Modular, lightweight architecture without over-engineering.
* Zero modification of original assets until specific approved milestones.

---

## 1. Feature Priority Matrix

| Feature / Task | Priority | Dependencies | Est. Time | Day |
|---|---|---|---|---|
| **Player Controller & Physics Decoupling** | **Essential** | None | 1.5 hrs | Day 1 |
| **Third-Person Orbit / Spring-Arm Camera** | **Essential** | Player Controller | 1.0 hr | Day 1 |
| **Character Centering & Idle Motion Fix** | **Essential** | Player Controller | 1.0 hr | Day 1 |
| **Environment Colliders (Bridges, Walls, Ground)** | **Essential** | Scene Objects | 1.5 hrs | Day 1 |
| **Interaction & Prompt System (`IInteractable`)** | **Essential** | Player Controller | 1.5 hrs | Day 2 |
| **Dialogue & Mission State Manager** | **Essential** | Interaction System | 1.5 hrs | Day 2 |
| **Herbal Collection & Simple Inventory System** | **Essential** | Interaction System | 1.5 hrs | Day 2 |
| **HUD & Quest Tracker UI (UGUI)** | **Essential** | Mission Manager | 1.0 hr | Day 2 |
| **Mission 1: The Arrival & Elder Greeting** | **Important** | Dialogue & Colliders | 1.2 hrs | Day 3 |
| **Mission 2: Well of Purity (Restoration)** | **Important** | Mission 1, Interaction | 1.3 hrs | Day 3 |
| **Mission 3: Riverbank Herbal Foraging** | **Important** | Inventory, Spawners | 1.3 hrs | Day 3 |
| **Mission 4: The Sacred Waterfall Altar** | **Important** | Mission 3, Landmarks | 1.2 hrs | Day 3 |
| **Mission 5: Ayurvedic Medicine Brewing** | **Important** | Inventory, Crafting Table | 1.5 hrs | Day 4 |
| **Mission 6: Healing the Afflicted & Victory** | **Important** | Mission 5, NPC State | 1.5 hrs | Day 4 |
| **Audio Feedback & Visual Polish (Particles/Cues)** | **Optional** | Missions 1–6 | 1.0 hr | Day 4 |
| **Final Windows Build Verification** | **Essential** | Complete Game Flow | 1.0 hr | Day 4 |

**Total Estimated Hours:** **19.0 hours**

---

## 2. Daily Stage Roadmap

### Day 1: Movement, Camera & Environment Foundation (5.0 hrs)
* **Stage 1.1:** Decouple Player Physics. Remove conflicting `Rigidbody` (or set `isKinematic = true`), remove redundant `BoxCollider`, standardize `CharacterController` bounds.
* **Stage 1.2:** Normalize Camera. Reset negative scale to `(1, 1, 1)`. Implement smooth third-person follow with pitch/yaw clamping and distance sphere-cast to prevent wall clipping.
* **Stage 1.3:** Model Alignment & Animation States. Normalize child mesh offsets. Configure Animator Controller with clean idle/walk blending.
* **Stage 1.4:** Environment Collision Pass. Add collision boxes/mesh colliders to `Bridge`, `Bridge.001`, `Bridge.002`, village house perimeters, and boundary blockers to prevent player falling out of bounds.

### Day 2: Core Gameplay Systems & UI (5.5 hrs)
* **Stage 2.1:** Interaction Architecture. Implement lightweight raycast / trigger-based `Interactable` system with floating HUD key prompt ("Press E to Interact / Collect").
* **Stage 2.2:** Quest & Dialogue Framework. Create a clean `MissionManager` tracking 6 sequential mission states and simple non-intrusive dialogue overlay for NPC conversations.
* **Stage 2.3:** Herb Collection & Inventory. Create low-poly collectible herb prefabs (Tulsi, Ashwagandha) using URP materials with subtle glow, and a simple HUD inventory counter.

### Day 3: Missions 1 through 4 Implementation (5.0 hrs)
* **Stage 3.1: Mission 1 – The Arrival at Arogya Gram**
  * Player spawns on mountain trail, follows `Path_to_village`, crosses main `Bridge`, approaches Elder NPC (`Scene_1_map/Cube`) to initiate the story.
* **Stage 3.2: Mission 2 – Water of Purity**
  * Elder reveals the village well is dry/contaminated. Player navigates to `Well` (`174, -44, -94`), clears debris / collects fresh water vessel.
* **Stage 3.3: Mission 3 – Herbal Foraging**
  * Player explores the riparian shores (`Lake` and `River`) to gather 3 wild medicinal herbs.
* **Stage 3.4: Mission 4 – The Sacred Mountain Waterfall**
  * Journey across `Bridge.001` to the highland `WaterFall` shrine (`341, -7, -395`). Complete altar tribute to receive the herbal blessing.

### Day 4: Crafting, Healing Culmination & Windows Build (3.5 hrs)
* **Stage 4.1: Mission 5 – Ayurvedic Apothecary & Medicine Crafting**
  * Return to village apothecary (`House_1`/`House_2`). Interact with the brewing station to combine Pure Water + Gathered Herbs into Sanjeevani Kadha.
* **Stage 4.2: Mission 6 – Healing the Afflicted & Restoration**
  * Administer medicine to the ailing villager NPC. Trigger healing visual effect and transition to game completion screen.
* **Stage 4.3: Testing, Quality Audit & Windows Build Verification**
  * Verify full playthrough from initial spawn to victory screen. Run test build for Windows x86_64.
