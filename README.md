# Midnight Return

A 2.5D Metroidvania built in Unity + HDRP — a spiritual successor to *Castlevania: Symphony of the Night*. Movement plays out on a flat XY plane (Z locked to 0) inside a full 3D HDRP world.

> **Status: source-only pre-production.** This repo contains C# gameplay code, shaders, input maps, and design docs — but it has **never been opened in Unity**. There are no `.meta` files, no `ProjectSettings/`, and no `Library/`. Unity generates those on first open. Nothing here is compiled or verified until you open it in the editor (see [Opening the project](#opening-the-project)).

## Stack

Verified from `Packages/manifest.json`:

| Package | Version |
|---|---|
| High Definition RP (HDRP) | 16.0.6 |
| Cinemachine | 3.1.1 |
| Input System | 1.8.2 |
| Visual Effect Graph | 16.0.6 |
| Animation Rigging | 1.3.0 |
| Timeline | 1.8.7 |
| UI Toolkit (`com.unity.ui`) | 2.0.0 |
| TextMeshPro | 3.0.9 |
| Addressables | 1.22.2 |
| Burst / Collections / Mathematics | 1.8.18 / 2.4.4 / 1.3.2 |

**Unity version:** the repo does not pin one (no `ProjectSettings/ProjectVersion.txt`). HDRP 16.0.6 ships with **Unity 2023.2**, so open with a 2023.2.x editor. `CLAUDE.md` and `SETUP.md` say "2023.2 LTS or 6.x" — note 2023.2 was a Tech Stream release. If you use Unity 6, expect Unity to prompt an API/package upgrade on first open.

Gameplay code is plain **C#** (~11.3k lines across 89 scripts). A separate `web-prototype/` folder holds an older **Phaser 3 + TypeScript** prototype (~2.3k lines) kept for reference only.

## Opening the project

### Unity game (the actual project)

1. Install **Unity 2023.2.x** (via Unity Hub).
2. Unity Hub → **Add** → select this repo folder → open it. Unity will generate `Library/`, `ProjectSettings/`, and `.meta` files, and auto-install the packages from `Packages/manifest.json` (this first import takes a while).
3. Follow **[`SETUP.md`](SETUP.md)** to wire up the scene. The gameplay scripts exist, but **no scenes, prefabs, materials, or ScriptableObject assets are committed** — `SETUP.md` is the step-by-step guide for building the scene hierarchy, animator, Cinemachine rig, layers, shaders, and VFX graphs by hand.

There is no ready-to-play scene in the repo yet; opening it gives you the code and packages, not a running build.

### Web prototype (reference only)

A standalone Phaser prototype under `web-prototype/`:

```bash
cd web-prototype
npm install
npm run dev        # webpack dev server
npm run build      # production bundle to dist/
npm run type-check # tsc --noEmit
```

This is independent of the Unity project and is not the current game.

## Project layout

```
Assets/
├── Scripts/            89 C# scripts, namespace root MidnightReturn.*
│   ├── Core/           GameManager (singleton, phase FSM)
│   ├── Player/         PlayerController, PlayerMovement, PlayerCombat, PlayerStats
│   │   └── States/     Idle, Run, Jump, Fall, Dash, WallSlide, Attack states
│   ├── Enemies/        EnemyBase (abstract), Projectile, AI controller
│   │   ├── Boss/       BossController + boss phase logic
│   │   ├── BT/         behaviour-tree nodes
│   │   └── Types/      concrete enemy variants
│   ├── Systems/        VFXManager, CameraSystem, AudioManager, rendering, map, inventory
│   ├── Data/
│   │   └── ScriptableObjects/  EnemyDataSO, WeaponDataSO, ArmorDataSO, ItemDataSO,
│   │                           BossPhaseDataSO, MusicZoneSO, ...
│   ├── Level/          level construction
│   ├── UI/             HUD (UI Toolkit)
│   ├── Utils/          StateMachine<T>, EventBus, ObjectPool<T>, MathUtils
│   └── VerticalSlice/  procedural bootstrap that assembles a playable slice from code
├── Shaders/HLSL/       DissolveEffect.hlsl, GothicLitExtension.hlsl
├── VFX/                VFXGraphSetup.md   (node-by-node VFX Graph build guide)
├── Settings/           HDRPSettings.md, AudioMixerSetup.md, LevelArtSetup.md
├── Art/                art direction + reference docs
├── InputActions/       PlayerActions.inputactions (keyboard + gamepad)
└── Tests/EditMode/     NUnit EditMode tests (added by this project — see below)
Packages/manifest.json  package/version pins
SETUP.md                full Unity wiring guide
web-prototype/          Phaser 3 + TypeScript prototype (reference only)
```

Design intent, architecture notes, and coding conventions live in [`CLAUDE.md`](CLAUDE.md).

## Architecture notes

- **Data / physics / rendering are kept separate:** tuning lives in ScriptableObjects, motion runs on `CharacterController`, visuals go through HDRP.
- **Typed `EventBus`** decouples systems via value-type struct events (no heap allocation on the hot path).
- **Generic `StateMachine<T>`** drives player and enemy behaviour.
- **No-alloc hot paths:** `MaterialPropertyBlock`, `Physics.OverlapBoxNonAlloc`, pooled objects.

## Tests

EditMode (NUnit) tests live in `Assets/Tests/EditMode/` and cover the deterministic, silently-breakable systems:

| Test file | Covers |
|---|---|
| `MathUtilsTests.cs` | `MathUtils.Approach/Lerp/Clamp/Sign/ExpThreshold` — the movement accel/friction primitive and the SotN exp curve |
| `PlayerStatsTests.cs` | `PlayerStatsSystem.ExpThreshold`, `AddExp` (leveling + exp carryover), `CalcDamage` (defense formula, min-damage floor, no-crit path) |
| `EnemyResistanceTests.cs` | `EnemyDataSO.GetResistance` — elemental resistance lookup + default |
| `BossAttackPickerTests.cs` | `BossPhaseDataSO.PickAttack` — cooldown filtering and empty-pattern handling |

To run them: open the project in Unity → **Window → General → Test Runner → EditMode → Run All**.

**These tests have not been executed** — Unity is not installed in the authoring environment, and the project has never been opened (see Status above). They are written against the real formulas in the code with exact expected values, but "green" is unconfirmed until someone runs them in Unity 2023.2. See [CI](#ci) for the automated path.

## CI

`.github/workflows/unity-test.yml` runs the EditMode suite on push/PR using [`game-ci/unity-test-runner`](https://game.ci/), with a `Library/` cache to speed up imports.

**It needs a `UNITY_LICENSE` secret to work.** game-ci must activate a Unity license before it can open the project; without the `UNITY_LICENSE` repository secret the job fails on **licensing**, not on the tests. The repo owner must add it (see the [game-ci activation guide](https://game.ci/docs/github/activation)). The workflow also pins `unityVersion: 2023.2.20f1` because the repo has no `ProjectVersion.txt` — adjust that to your installed 2023.2 patch if needed.
