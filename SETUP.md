# Midnight Return — Unity HDRP Setup Guide

## 1. Create Unity Project
1. Open Unity Hub → New Project
2. Template: **3D (HDRP)** — Unity 2023.2 LTS or 6.x
3. Name: `MidnightReturn`
4. Clone this repo into the project folder OR copy `Assets/` and `Packages/` into it

## 2. Package Verification
Open **Window → Package Manager**, confirm these are installed:
- High Definition RP (16.x)
- Cinemachine (3.x)
- Input System (1.8+)
- Visual Effect Graph (16.x)
- Animation Rigging (1.3+)
- TextMeshPro (3.x)
- Timeline (1.8+)

If `manifest.json` was copied correctly, all packages auto-install.

## 3. HDRP Quality Configuration
1. Edit → Project Settings → HDRP Default Settings
2. Import `Assets/Settings/HDRPSettings.md` as a reference — manually configure as listed
3. Create a **Global Volume** in your scene with the post-processing overrides listed

## 4. Input System
1. Edit → Project Settings → Player → Active Input Handling → **Both** (or New Input System Only)
2. Import `Assets/InputActions/PlayerActions.inputactions`
3. Assign to `PlayerInput` component on the Player prefab

## 5. Scene Hierarchy Setup

```
Scene Root
├── [Managers]
│   ├── GameManager          → GameManager.cs (DontDestroyOnLoad)
│   ├── VFXManager           → VFXManager.cs  (assign all VFX assets)
│   ├── AudioManager         → AudioManager.cs
│   └── CameraRig
│       ├── Main Camera      → Camera + HDAdditionalCameraData
│       └── Cinemachine Brain → CinemachineCamera + CameraSystem.cs
│
├── [Lighting]
│   ├── Directional Light    → Key light (warm orange #ff8833)
│   ├── Global Volume        → HDRP post-processing profile
│   ├── Reflection Probe     → Baked + realtime
│   └── Fog Volume           → HDRP Local Volumetric Fog
│
├── [Player]
│   ├── PlayerRoot           → PlayerController.cs, PlayerMovement.cs,
│   │                          PlayerInputHandler.cs, PlayerCombat.cs
│   │                          CharacterController (radius 0.3, height 1.8)
│   ├── Mesh                 → SkinnedMeshRenderer, Animator
│   │   └── (Import 3D character model with animations)
│   ├── WeaponBone           → Child of hand bone, hitbox origin for PlayerCombat
│   └── PlayerCamera Target  → Cinemachine Follow target
│
├── [Level]
│   ├── Ground               → Static, layer: Ground
│   ├── Platforms            → Static, layer: Platform
│   └── Room_EntranceHall    → Contains enemies, props, triggers
│       ├── Enemies/
│       │   ├── Zombie       → PatrolEnemy.cs + EnemyData SO ref
│       │   ├── Skeleton     → PatrolEnemy.cs + EnemyData SO ref
│       │   └── AxeKnight    → PatrolEnemy.cs + EnemyData SO ref
│       └── Triggers/
│           └── RoomBounds   → BoxCollider (trigger), assign roomId
│
└── [UI]
    └── UIDocument           → HUDController.cs, assign UXML asset
```

## 6. Character Controller Settings
```
CharacterController:
  Slope Limit: 45
  Step Offset:  0.3
  Skin Width:   0.08
  Min Move Distance: 0.001
  Center: (0, 0.9, 0)
  Radius: 0.3
  Height: 1.8
```

## 7. Animator Controller Setup
Create `PlayerAnimator.controller` with these states:

```
Any State → Death (trigger: Die)
Idle    → Run     (bool: IsMoving)
Idle    → Jump    (trigger: Jump)
Idle    → Dash    (trigger: Dash)
Idle    → Attack1 (trigger: Attack)
Run     → Idle    (bool: !IsMoving)
Run     → Jump    (trigger: Jump)
Run     → Dash    (trigger: Dash)
Jump    → Fall    (float: VelocityY < -0.5)
Fall    → Idle    (bool: IsGrounded)
Attack1 → Attack2 (trigger: Attack, when attackDone)
Attack2 → Attack3 (trigger: Attack, when attackDone)
Attack3 → Idle    (auto on complete)
Dash    → Idle    (auto on complete)
```

Animation events on Attack1/2/3 animations:
- At hit frame: call `OnAttackHitFrame()` → sets `AttackDone = true` on Animator

## 8. VFX Graph Assets
Follow `Assets/VFX/VFXGraphSetup.md` to build each VFX Graph.
Assign completed VFX assets to `VFXManager` in Inspector.

## 9. Shader Graph Materials

### DissolveEnemy.mat
1. Create new Shader Graph: HDRP/Lit Graph
2. Add Custom Function node → link `Assets/Shaders/HLSL/DissolveEffect.hlsl`
3. Wire: UV → DissolveEffect_float
4. `_DissolveAmount` float property (range 0–1, exposed)
5. Wire Alpha out → Alpha clip threshold
6. Wire EmissiveColor → Emission input (multiply by 10 for HDR)
7. Assign as material to all enemy renderers

### WeaponTrail.mat
1. Shader Graph: HDRP/Unlit Graph
2. Custom Function: `WeaponTrailFade_float`
3. Expose: `TrailColor` (Color, HDR), `Intensity` (float)
4. Wire Alpha out → Opacity
5. Blend Mode: Additive (set in Graph Settings)

### GothicLit.mat (base character material)
1. HDRP/Lit Graph
2. Add `GothicRimLight_float` custom function
3. Expose: `RimColor` (Color), `RimPower` (float, default 4), `RimIntensity` (float, default 3)
4. Add `EmissivePulse_float` for glowing eye/sigil details
5. Use `CapeWindDeformation_float` in vertex stage for cloth animation

## 10. Cinemachine 3.x Settings
```
CinemachineCamera:
  Lens: Orthographic, Size = 6
  Target: Player → PlayerCameraTarget child
  
PositionComposer (Body):
  TrackedObjectOffset: (0, 0, 0) — animated by CameraSystem.cs
  Lookahead Time: 0.0 (we handle lookahead manually)
  
RotationComposer (Aim):
  Disabled for 2.5D

CinemachineImpulseSource:
  Default Velocity: (1, 0, 0)
  ImpulseDefinition: Time = 0.2, Curve = (shake envelope)
```

## 11. Layer Collision Matrix
Physics → Layer Collision Matrix:
- Player vs Enemy: ✅
- Player vs Ground: ✅
- Player vs Platform: ✅
- Enemy vs Ground: ✅
- Projectile vs Player: ✅
- Projectile vs Ground: ✅
- Projectile vs Enemy: ❌ (friendly fire off)
- Hitbox vs Enemy: ✅ (player attack hitbox)
- VFX vs everything: ❌

## Controls (Default)
| Action  | Keyboard     | Gamepad        |
|---------|-------------|----------------|
| Move    | A/D          | Left Stick      |
| Jump    | Space / W    | A (South)       |
| Dash    | Left Shift   | B (East)        |
| Attack  | J            | X (West) / RT   |
| Spell   | K            | Y (North) / LT  |
| Interact| E            | Y (North)       |
| Pause   | Escape       | Start           |
