# Sprite Sheet Reference — Asset & Animation Catalogue

> **Direction:** Reference only. Art ships as **2.5D HDRP / WebGL** (meshes + shaders + particles).
> SotN sprite sheets drive the **roster**, **tier priority**, **proportions/silhouette**, and the
> **animation-state list** below. We do **not** ship pixel sprites — we rebuild these forms in 3D.

---

## 1. Player Animation States

Source sheet (col. 3) → our `Animator` clip name → `StateMachine<PlayerState>` state.
`AttackState` already drives a 3-hit combo via `_comboAnims[]`.

| # | SotN Sheet State        | Animator Clip      | StateMachine State   | Status | Notes |
|---|-------------------------|--------------------|-----------------------|--------|-------|
| 1 | Idle                    | `Idle`             | `IdleState`           | ✅ done | breathing bob |
| 2 | Walking                 | `Walk`             | `WalkState`           | ✅ done | demote from Run when axis < 0.6; gamepad meaningful |
| 3 | Running                 | `Run`              | `RunState`            | ✅ done | |
| 4 | Jumping                 | `Jump`             | `JumpState`           | ✅ done | variable-height already wired |
| 5 | Falling                 | `Fall`             | `FallState`           | ✅ done | |
| 6 | Dashing                 | `Dash`             | `DashState`           | ✅ done | afterimage MaterialPropertyBlock |
| 7 | Wall slide / cling      | `WallSlide`        | `WallSlideState`      | ✅ done | |
| 8 | Attacking with weapon   | `Attack1/2/3`      | `AttackState`         | ✅ done | 3-hit combo |
| 9 | Attacking (air)         | `AirAttack`        | `AirAttackState`      | ✅ done | Jump/Fall + Attack (no Down); brief Y-hold; single hit |
|10 | Sub-weapon throw        | `SubWeapon`        | `SubWeaponState`      | ✅ done | Spell input from any grounded/aerial state; fires TryCast(0) at throw frame |
|11 | Crouching               | `Crouch`           | `CrouchState`         | ✅ done | CC height ×0.55, center adjusted; crouch-attack via Attack1 |
|12 | Turning around          | `TurnAround`       | `TurnAroundState`     | ✅ done | 0.08s cosmetic pivot; fires from Idle/Run on direction flip |
|13 | Dragon Kick (down+atk)  | `DragonKick`       | `DragonKickState`     | ✅ done | Down+Attack airborne; dive velocity 26u/s; AOE burst on land |
|14 | Climbing (ladder/chain) | `Climb`            | —                     | ⬜ gap  | needs ClimbState + climbable trigger volumes (Phase 7 or later) |
|15 | Death                   | `Death`            | (locked FSM)          | ✅ done | dissolve via VFXManager |

**Canonical clip-name contract** — the Animator Controller must expose exactly these state names
(case-sensitive), since `CrossFadeInFixedTime("<name>")` is called by string:
`Idle, Walk, Run, Jump, Fall, Dash, WallSlide, Attack1, Attack2, Attack3, AirAttack,
SubWeapon, Crouch, TurnAround, DragonKick, Climb, Death`

**Only remaining gap:** Climb — needs a climbable-volume trigger system and vertical movement
override in PlayerMovement. Deferred to Phase 7 (level art pipeline).

---

## 2. Weapon Tier Priority (S+ → F)

Our 8 weapons (`weapons.ts` / `WeaponDataSO`) ranked for **build/polish priority** —
which get the richest VFX, trail shaders, and special-move feel first.

| Tier | Weapon          | Type   | Why this tier |
|------|-----------------|--------|---------------|
| S+   | Vampire Killer  | whip   | signature weapon — holy element, longest reach, hero piece. Full VFX pass. |
| S+   | Death's Scythe  | scythe | iconic wide arc + dark element; great for the afterimage trail shader |
| A+   | Flame Sword     | sword  | elemental fire trail — showcases additive ember VFX |
| A+   | Holy Lance      | spear  | thrust + pierce; anti-undead bonus damage |
| A    | Shadow Blade    | sword  | dark, lifesteal special — pairs with Soul Steal spell |
| A    | Claymore        | sword  | heavy 2-hand, slow big-hit; strong hitstop feel |
| B    | Battle Axe      | axe    | arc swing, knockback |
| B    | Short Sword     | sword  | starter — fast combo, baseline feel reference |

Trail color / element guidance lives in `WeaponDataSO.TrailColor` + `DamageType`.

---

## 3. Armor Tier Priority

12 pieces (`armor.ts` / `ArmorDataSO`) by slot. Tier = visual/passive impact priority.

| Slot      | Pieces (low→high)                              | Top-tier showcase |
|-----------|------------------------------------------------|-------------------|
| Helmet    | Iron Helm → Circlet of Wisdom                  | Circlet (INT/MP glow) |
| Body      | Leather → Knight → Dark Robe → Dragon Scale Mail | Dragon Scale (rim-light + scale spec) |
| Cloak     | Traveler's Cloak → Cloak of Shadows            | Shadow Cloak (cape cloth shader + MP) |
| Boots     | Iron Boots → Swift Boots                        | Swift (dash afterimage tint) |
| Accessory | Ring of Varda, Bloodstone                       | Bloodstone (LCK/crit) |

`OverrideMaterial` on `ArmorDataSO` swaps the character mesh material when equipped —
Dragon Scale Mail + Cloak of Shadows are the two that most change the silhouette.

---

## 4. Enemy Roster → Zone & AI Mapping

17 enemies (`enemies.ts`) → our AI pipeline. `behavior` maps to either the legacy FSM
types **or** the new `EnemyAIController` behaviour-tree factories (Phase 3).

| Enemy          | behavior      | BT Factory / FSM         | Zone (suggested)  |
|----------------|---------------|--------------------------|-------------------|
| Zombie         | patrol        | `BuildPatrolTree`        | Entrance Hall     |
| Skeleton       | patrol        | `BuildPatrolTree`        | Entrance Hall     |
| Axe Knight     | patrol_jump   | PatrolJump FSM           | Entrance Hall     |
| Medusa Head    | flying        | `BuildFlyingTree`        | Catacombs         |
| Ghost          | flying        | `BuildFlyingTree`        | Catacombs         |
| Bone Archer    | ranged        | `BuildRangedTree`        | Catacombs         |
| Warg           | chase         | Chase FSM                | Catacombs         |
| Harpy          | flying        | `BuildFlyingTree`        | Cursed Library    |
| Merman         | patrol        | `BuildPatrolTree`        | Cursed Library    |
| Sorcerer       | ranged        | `BuildRangedTree`        | Cursed Library    |
| Gargoyle       | flying        | `BuildFlyingTree`        | Clocktower        |
| Blood Skeleton | patrol        | `BuildPatrolTree`        | Clocktower        |
| Scythe Knight  | chase         | Chase FSM                | Clocktower        |
| Gear Golem     | patrol_jump   | PatrolJump FSM           | Clocktower        |
| Vampire Bat    | flying        | `BuildFlyingTree`        | Throne Room       |
| Dark Knight    | chase         | Chase FSM                | Throne Room       |
| Succubus       | flying        | `BuildFlyingTree`        | Throne Room       |

**Difficulty ramp** mirrors the sheet's tier ordering — early zones = patrol/jump basics,
late zones = chase + flying mixes that pressure the player's air-dash and spell timing.

---

## 5. Props / Environment (sheet col. 4)

Reference for set-dressing meshes — chests, candelabra, banners, NPCs, decor.

| Prop          | System hook |
|---------------|-------------|
| Chest         | item pickup → `GameManager.AddToInventory` |
| Candelabra    | torch PointLight + ember VFX (already in game-visual-demo) |
| Banner/decor  | parallax / set-dressing (Phase 7 tile pipeline) |
| Save NPC/idol | `SaveStatue` (Phase 4) |
| Wandering NPC | future dialogue system |

---

## 6. Proportions / Silhouette

Hero read from the player sheet: **tall, slim, high-contrast silhouette, single glowing red eye,
flowing cape.** Already encoded in `game-visual-demo.html` (box-geometry body + GPU cloth cape +
`MeshBasicMaterial` red eye caught by bloom). Keep enemy silhouettes **readable at a glance** —
distinct profile per AI type (grounded = bulky, flying = winged spread, ranged = staff/bow tell).
