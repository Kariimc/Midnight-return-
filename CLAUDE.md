# Midnight Return — Project Instructions

## Role
You are the Principal Game Systems Architect and Lead Engineer on this project. Act as a technical co-founder: autonomous, opinionated, performance-focused.

## Project
AAA 2.5D Metroidvania — spiritual successor to Castlevania: Symphony of the Night.
**Engine:** Unity 2023.2 LTS + HDRP 16.x
**Stack:** C#, HDRP, VFX Graph, Cinemachine 3.x, New Input System, UI Toolkit, Animation Rigging, ShaderGraph

## Communication Rules
- Zero fluff. No pleasantries. Punchy and direct.
- ELI5 for technical choices: simple analogies, plain language, concise.
- **Break-or-Pivot Protocol:** Never ask permission on routine implementation. Only interrupt if a choice will break core architecture, cause irreversible data loss, or require a massive creative pivot.

## Autonomous Operation
- Leverage all available Unity packages, HDRP features, and open-source tools aggressively.
- Use boilerplate and proven patterns (component systems, state machines, ScriptableObjects, object pooling).
- Write → dry-run verify → fix → loop. Do not present code until it is stable within our architecture.

## Technical Mindset
- Strict separation: data (ScriptableObjects) | physics (CharacterController) | rendering (HDRP pipeline).
- Performance: no-alloc hot paths (MaterialPropertyBlock, Physics.OverlapBoxNonAlloc, value-type EventBus events).
- Modularity: every system decoupled via the typed EventBus.
- 2.5D constraint: all movement on XY plane, Z locked to 0, full HDRP 3D world.

## Current Architecture

### Directory Layout
```
Assets/
├── Scripts/
│   ├── Core/          GameManager.cs (singleton, phase FSM, save/load)
│   ├── Player/        PlayerController, PlayerMovement, PlayerInputHandler, PlayerCombat
│   │   └── States/    Idle, Run, Jump, Fall, Dash, WallSlide, Attack (3-hit combo)
│   ├── Enemies/       EnemyBase (abstract), Projectile
│   │   └── Types/     PatrolEnemy, FlyingEnemy, RangedEnemy
│   ├── Systems/       VFXManager, CameraSystem, AudioManager
│   ├── Data/
│   │   └── ScriptableObjects/  EnemyDataSO, WeaponDataSO, ArmorDataSO
│   ├── UI/            HUDController (UI Toolkit)
│   └── Utils/         StateMachine<T>, EventBus, ObjectPool<T>, MathUtils
├── Shaders/HLSL/      DissolveEffect.hlsl, GothicLitExtension.hlsl
├── VFX/               VFXGraphSetup.md (node-by-node guide for 5 VFX Graph assets)
├── Settings/          HDRPSettings.md (full HDRP quality config)
└── InputActions/      PlayerActions.inputactions (keyboard + gamepad)
Packages/manifest.json
SETUP.md               Full Unity wiring guide (scene hierarchy, animator, Cinemachine, layers)
web-prototype/         Original Phaser 3 + TypeScript prototype (reference only)
```

### Key Systems

**EventBus** — zero-heap-alloc typed events via `Action<T>` on value-type structs.
Event catalogue: `PlayerDamagedEvent`, `PlayerDiedEvent`, `PlayerLeveledUpEvent`, `EnemyDiedEvent`, `ItemPickedUpEvent`, `RoomEnteredEvent`, `BossStartedEvent`, `BossDefeatedEvent`, `PlayerAttackEvent`, `ScreenFlashEvent`, `CameraShakeEvent`

**StateMachine<T>** — generic, lock-able, enter/update/fixedUpdate/exit hooks.

**PlayerMovement** — CharacterController 2.5D, 8-frame input buffer, coyote time (0.1s), variable-height jump (cut on release), wall-jump/cling, air dash (unlockable), wall slide.

**PlayerCombat** — `Physics.OverlapBoxNonAlloc` hitbox, `Time.timeScale=0` hitstop on hit, weapon SO swap, crit system.

**VFXManager** — pooled `VisualEffect` instances for VFX Graph assets. HDRP post-processing driven per-event (Bloom spike, Vignette pulse, Chromatic aberration, Screen flash).

**CameraSystem** — Cinemachine 3.x, orthographic size 6, `CinemachineImpulseSource` shake, smooth horizontal lookahead (FacingDir × 1.8u).

**EnemyBase** — abstract, HDRP dissolve coroutine (`MaterialPropertyBlock._DissolveAmount` 0→1), hit flash via emissive property block.

**HUDController** — UI Toolkit, queries UXML elements, reactive to EventBus events, danger color ramp on HP bar.

### HDRP AAA Features Implemented
| Feature | File |
|---|---|
| Enemy dissolve death | `DissolveEffect.hlsl` → ShaderGraph |
| Weapon trail | VFX Graph particle strip + `WeaponTrailFade_float` |
| Bloom/vignette/CA spikes | `VFXManager.cs` (HDRP Volume override) |
| Afterimage dash trail | `PlayerController.cs` coroutine + MaterialPropertyBlock |
| Hitstop freeze frames | `AttackState.cs` Time.timeScale |
| Camera shake | `CinemachineImpulseSource.GenerateImpulse()` |
| Rim lighting | `GothicRimLight_float` ShaderGraph custom function |
| Cape cloth vertex deform | `CapeWindDeformation_float` |
| Fake SSS for creatures | `FakeSSSGothic_float` |
| Screen distortion | `ScreenDistortion_float` |

### Asset / Animation Reference
`Assets/Art/AssetReference.md` — SotN sprite sheets catalogued; maps player animation states → Animator
clip-name contract + StateMachine (flags gaps: Walk, AirAttack, SubWeapon, Crouch, TurnAround, DragonKick,
Climb), weapon/armor tier priority, enemy roster → zone/AI.

### Sprite-in-3D Pipeline
SotN sprites run as atlas-sampled quads in HDRP 3D space. Drop PNG at `Assets/Art/Reference/SotN_SpriteSheet.png`.
- `SpriteSheetDataSO.cs` — atlas layout SO (Columns, Rows, named SpriteClip[] with StartFrame/FrameCount/Fps/Loop)
- `SpriteAnimator.cs` — drives `_BaseColorMap_ST` UV tiling+offset via MaterialPropertyBlock, zero-GC
- `SpriteBillboard.cs` — LockY camera-facing quad for 2.5D
- `PlayerSpriteController.cs` — FSM state → SpriteAnimator.Play() (maps "Idle"/"Run"/"Jump"/"Fall"/"Dash"/"WallSlide"/"Attack" → clip names)
- `EnemySpriteController.cs` — EnemyBase state flags → clip names (Patrol/Flying/Ranged types)
- `EnemyBase.cs` — added public IsDead, IsMoving, IsAttacking, FacingDir, IsChasing, IsDiving
- `PlayerCombat.cs` — added public ComboIndex property
- `Assets/Art/Reference/SETUP.md` — PNG import settings, SpriteSheetDataSO config, prefab wiring guide
- `web-prototype/public/game-visual-demo.html` — SPRITE_SHEET_URL const + SPRITE_FRAG atlas sampler with analytic torch lighting; keyboard shortcuts i/r/j/f/d/1/2/3 cycle states, arrow keys flip facing

### Animation Gap States (14/15 complete — Climb deferred to Phase 7)
New states added to PlayerController FSM (all registered in BuildFSM):
- `WalkState` — slow-tilt walk, demoted from Run when MoveAxis.x < 0.6 (gamepad partial input)
- `TurnAroundState` — 0.08s cosmetic pivot on direction flip from Idle or Run
- `AirAttackState` — single aerial hit (no combo); Jump/Fall + Attack without Down; brief Y-hold; chains to DragonKick
- `SubWeaponState` — Spell input from any state; plays "SubWeapon", fires PlayerSpellSystem.TryCast(0) at throw frame
- `CrouchState` — Down while grounded; CC height ×0.55; crouch-attack via Attack1; Jump/Dash escapes
- `DragonKickState` — Down+Attack while airborne; dive 26u/s; invincible; AOE hitbox + heavy shake + flash on land
Supporting changes:
- `PlayerMovement.cs` — SetCrouch() (CC height/center resize), SetDiveVelocity() for DragonKick
- `PlayerController.cs` — SpellSystem nullable property, all 6 states registered
- `IdleState/RunState` — TurnAround on dir flip, SubWeapon/Crouch routing
- `JumpState/FallState` — AirAttack (Attack), DragonKick (Down+Attack), SubWeapon routing

### Enemy Roster (17 enemies, data in `web-prototype/src/data/enemies.ts`)
Zones: Entrance Hall → Catacombs → Cursed Library → Clocktower → Throne Room
AI types: Patrol, Chase, PatrolJump, Ranged, Flying, Boss

### Weapons (8): Short Sword, Vampire Killer (whip), Claymore, Battle Axe, Shadow Blade, Holy Lance, Death's Scythe, Flame Sword
### Armor (12 pieces): Helmets, Body, Cloaks, Boots, Accessories — all with stat modifiers and passive abilities

### RPG System (SotN-faithful)
Stats: STR, CON, INT, LCK → derive ATK, DEF, MaxHP, MaxMP
Damage: `CalcDamage(attacker, defStat)` → base - def*0.5, ±10% variance, LCK-based crit
EXP curve: `100 × 1.45^(level-1)` exponential threshold
Level-up: random stat growth with variance, full HP/MP restore

## Visual & Interactive Artifacts
- For all UI layouts, HUD designs, or menu systems: generate self-contained interactive HTML/JS/CSS files in `web-prototype/public/` that open directly in a browser.
- For level layouts, camera zones, hitbox structures: use Mermaid.js diagrams or clean ASCII grids.
- For architecture decisions: provide a brief breakdown before shipping large code blocks.

## Phase Roadmap
- [x] Phase 1: Core framework, movement system, state machine, RPG stats, VFX pipeline
- [x] Phase 2: Enemy roster data, weapon/armor data, initial game scene
- [x] Engine conversion: Phaser 3 → Unity HDRP C# (complete)
- [x] Phase 3: Enemy AI behavior trees, boss multi-phase scripting
  - BehaviorTree.cs — Sequence/Selector/Parallel composites, Cooldown/Inverter/RepeatUntilFail decorators, Condition/Action lambdas, BTBlackboard
  - EnemyBTNodes.cs — BuildPatrolTree, BuildRangedTree, BuildFlyingTree factories
  - EnemyAIController.cs — additive EnemyBase subclass, BT-driven, coexists with existing FSM enemies
  - BossController.cs — 6-phase FSM (Idle→Intro→Phase0-2→Dying→Dead), 6 attack patterns, per-phase BT rebuild
  - BossPhaseDataSO.cs — weighted attack picker, HP threshold, aggression multiplier, transition VFX
- [x] Phase 4: Metroidvania map system — room graph, transitions, save statues
  - RoomDataSO.cs — room identity, zone, map grid position, door connections, ambience
  - RoomGraph.cs — ScriptableObject graph, Init() dictionary, CanPass() lock check
  - RoomManager.cs — singleton, additive async load/unload, fade transitions, ambience apply
  - DoorTrigger.cs — collider-based room exit, lock/unlock visual, EventBus blocked event
  - SaveStatue.cs — interact radius, HP+½MP restore, HDRP light spike, idle glow pulse
  - MinimapController.cs — UI Toolkit procedural grid, fog-of-war, zone tints, room icons
  - PlayerController.cs — RestoreAtStatue() added
  - PlayerInputHandler.cs — ConsumeInteract() buffered interact input
  - EventBus.cs — RoomTransition{Started,Complete,Blocked}Event, SaveStatueActivatedEvent
  - GameManager.cs — SaveData: LastSaveStatueId, LastSavePosition, UnlockedDoors
- [x] Phase 5: Inventory UI, equipment screen, spell system
  - SpellDataSO.cs — SpellType/SpellElement enums, MP cost, cooldown, damage, cast/impact VFX, projectile prefab
  - ItemDataSO.cs — ItemCategory enum, HP/MP restore, permanent stat mods, stack size
  - InventorySystem.cs — singleton catalog, equip/unequip for all slot types, stat recalculation, UseItem()
  - PlayerSpellSystem.cs — 4 spell slots, TryCast() with MP check + cooldown, Projectile/AreaBurst/Buff/Drain effects
  - InventoryUI.cs — runtime UI Toolkit panel: equipment doll (8 slots) + item grid + stats + spell slots, Tab toggle
  - AudioManager.cs — public CrossFade() and PlaySFX() wrappers added (Phase 4 bug fixes)
  - EventBus.cs — EquipmentChangedEvent, SpellEquippedEvent, SpellCastEvent, ItemUsedEvent
  - GameManager.cs — SaveData.SpellSlots[4] added
  - PlayerController.cs — RestoreMp() added
  - web-prototype/public/inventory-demo.html — interactive gothic inventory demo
- [x] Phase 6: Full audio pipeline, music zones, dynamic mix
  - MusicZoneSO.cs — per-zone stem contract: exploration/combat/boss tracks, ambient bed, reverb preset + wet, mixer snapshot, mix ceilings
  - AudioManager.cs — rewritten layered engine: ping-pong base music crossfade, adaptive combat-layer source, ambient bed, positional 3D SFX pool, AudioMixer bus volumes (linear→dB), ducking, reverb filter control, FadeOutAll (fixes prior CS0111 CrossFade collision)
  - MusicDirector.cs — EventBus-driven dynamic-mix brain: zone resolve on room enter, decaying combat-intensity meter (attack/hit/kill bumps) → adaptive layering, boss override + return, fade on death
  - PlayerAudio.cs — foley: run-cadence footsteps, jump/land/dash one-shots, looping wall-slide scrape, hurt grunt
  - AttackState.cs — emits PlayerAttackEvent to feed the intensity meter
  - EventBus.cs — MusicZoneChangedEvent, CombatIntensityEvent, MusicStateChangedEvent
  - Assets/Settings/AudioMixerSetup.md — mixer groups, exposed params, snapshots, reverb, stem authoring rules
  - web-prototype/public/audio-mixer-demo.html — live Web Audio mirror: synthesized per-zone stems, intensity meter, adaptive combat layer, boss override, bus faders (keys 1–5 zones, A/H/K events, B boss)
- [x] Phase 7: Level art pipeline — tile system, parallax, HDRP lighting per zone, Climb
  - TileSetSO.cs — atlas palette: columns/rows, tile world size, sparse solid-index set, GetUV() with seam padding
  - TilemapLayerSO.cs — authored grid (row-major, -1 empty), ZDepth per layer, BuildColliders flag
  - TileChunkBuilder.cs — bakes a layer into ONE combined mesh (1 draw call), merged horizontal-run box colliders, UInt32 index for >65k verts
  - ParallaxLayer.cs — self-driving fractional camera-travel parallax, auto-scroll, seamless horizontal wrap
  - ZoneLightingSO.cs — per-zone HDRP mood: ambient, key/fill light color+intensity+angle, fog (density→meanFreePath), bloom/vignette/color-grade/exposure
  - ZoneLightingController.cs — on RoomTransitionCompleteEvent lerps lights + HDRP Volume overrides (Fog/Bloom/Vignette/ColorAdjustments/Exposure via TryGet); ApplyImmediate() for boot
  - ClimbableVolume.cs — trigger feeding PlayerMovement.SetClimbable (centerline + vertical bounds)
  - ClimbState.cs — ladder climb; gravity off, MoveAxis.y vertical, X snap, anim freeze when idle; jump-off / over-top / bottom dismount
  - PlayerMovement.cs — SetClimbable/SetClimbing/TickClimb + CanClimb/IsClimbing/AtLadderTop/AtLadderBottom; climb branch in Tick
  - PlayerController.cs — ClimbState registered; Idle/Run/Jump/Fall route into Climb (Up to mount; Down at top)
  - Assets/Settings/LevelArtSetup.md — tile/parallax/lighting/climb wiring guide
  - web-prototype/public/level-art-demo.html — Three.js: 5 parallax depths, shader-tiled ground+wall, torch bloom, live per-zone lighting (keys ◂▸ pan, Space auto, 1–5 zones)
  - All 15 player animation states now complete (Climb was the last gap)
- [x] Vertical Slice: first playable room — all 7 phases through one code path
  - VerticalSliceContent.cs — code-authored content (no hand-written .asset): builds TilemapLayerSO (main+bg via rect/line grid helpers), TileSetSO, world-unit-tuned EnemyDataSO (Zombie/Skeleton); exposes room feature coords + SurfaceY/ColX world-space helpers
  - VerticalSliceBootstrap.cs — runtime room assembly: bakes tile meshes, spawns 2 PatrolEnemy on enemy layer, places SaveStatue + ClimbableVolume ladder, repositions Player, emits ONE RoomTransitionCompleteEvent (MusicDirector/ZoneLightingController self-activate — zero direct wiring = the decoupling proof)
  - EnemyBase.cs — Configure(EnemyDataSO) runtime injection (serialized Data can't be set externally)
  - TileChunkBuilder.cs — Configure(TileSetSO, TilemapLayerSO) runtime injection
  - Assets/VerticalSlice/SETUP.md — scene wiring + the pixel→world-unit EnemyDataSO tuning finding
  - web-prototype/public/vertical-slice-demo.html — fully playable canvas build of the room: patrol/chase enemies, pit jump, ladder climb, save statue, top-left HUD, attack + sub-weapon, level-ups (← → move, ↑↓ climb, Z/Space jump, J attack, K sub-weapon, E save)
- [x] AAA Texture Pipeline: PBR material infrastructure + visual overhaul
  - HDRPTileMaterial.cs — static factory: BuildStoneMaterial/BuildBrickMaterial/BuildMetalMaterial each wire BaseColorMap+NormalMap+MaskMap (HDRP R=Metallic G=AO B=DetailMask A=Smoothness) with keyword enabling; shader fallback chain HDRP/Lit→URP/Lit→Standard
  - VerticalSliceBootstrap.cs — expanded: separate stone (6 PBR slots) + brick Inspector headers; TileMaterial() PBR path via HDRPTileMaterial; BrickMaterial() for bg layer; MainLayer baked before BG layer with independent materials
  - Assets/Art/Textures/README.md — texture pack guide: Polyhaven/AmbientCG CC0 packs, MaskMap packing (R/G/B/A channels), Unity import settings, file layout, tiling scale recommendations
  - web-prototype/public/vertical-slice-demo.html — VISUAL OVERHAUL (387→714 lines): 8 procedural seeded stone tile variants (LCG noise+cracks), Gothic brick background pattern, animated torch sconces (triple-ellipse flame), dynamic lighting canvas (multiply composite, radial gradient punch-holes), Gothic column parallax, ornate iron ladder, particle system (hits/deaths/ambient dust), 11-layer draw pipeline, color grade overlay
- [x] Room 2 + transitions: kill-gated door, fade-to-black swap, Catacombs
  - CatacombsContent.cs — Room 2 layout factory (open eerie cavern, 44×16): three cascading platforms (HIGH row5/MID row8/LOW row11) over a vast central void, two stalactite columns, left+right floor landings only; SurfaceY/ColX helpers; placeholder BuildShade()/BuildWraith() EnemyDataSOs (roster TBD)
  - ProceduralRoomTransition.cs — kill-gated door trigger: RequireComponent(Collider), start locked (dark red mesh), Unlock() turns gold; OnTriggerEnter/Update fires RunTransition() coroutine which fades CanvasGroup overlay 0→1 (0.3s), invokes OnMidpoint callback (room swap), yields one frame, fades 1→0; UGUI overlay created once with DontDestroyOnLoad so it survives room destruction
  - VerticalSliceBootstrap.cs — full rewrite: BuildRoom1()/SwitchToRoom2() methods; _room1Objects List tracks all Room1 GOs for bulk Destroy; _room1EnemyIds HashSet + _room1EnemiesAlive counter fed by EnemyDiedEvent subscription; ExitDoor ProceduralRoomTransition.OnMidpoint wired to SwitchToRoom2; Room2 emits RoomTransitionCompleteEvent(Zone=Catacombs); both rooms share BuildTilemap/SpawnPatrol/BuildStatue/BuildLadder helpers
  - web-prototype/public/vertical-slice-demo.html — added Room 2: buildRoom2Grid()/resetRoom2Enemies(), kill-tracking checkRoomClear() unlocks door, CSS fade overlay (opacity transition 0.3s), startTransition() swaps grid+enemies+spawn, ladder position made room-specific (ladderX0Cur etc), statue interaction guarded to Room 1, exit door rendered at right wall with gold glow when unlocked, stalactite drip tips for Room 2, reset() rebuilds Room 1 grid
