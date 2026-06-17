# Vertical Slice — First Playable Room

The first time all seven phases run through **one code path**. Instead of
hand-authored `.asset` files, the room is assembled procedurally at runtime so
it's diffable, version-controlled, and Unity-openable with zero manual wiring.

Room: **Entrance Hall — Threshold** (Zone `EntranceHall`)

```
  ┌──────────────────────────────────────────────┐  ← ceiling
  │                                               │
  │   ▭ zombie (ledge)                ▣ statue    │
  │  ════════                       ════════════  │  ← high platform + low ledge
  │                                        ║ ladder
  │  ☼ spawn      ▭ skeleton    ░░░░               │
  │ ══════════════════════════  pit  ════════════ │  ← floor (gap to jump)
  └──────────────────────────────────────────────┘
```

---

## What it exercises

| Phase | System | Proven by |
|-------|--------|-----------|
| 1 | Movement FSM, RPG stats | player runs/jumps/climbs/attacks |
| 2 | Enemy + weapon data | Zombie/Skeleton `EnemyDataSO` built in code |
| 3 | Enemy AI | `PatrolEnemy` patrol→chase→attack |
| 4 | Map / save | `SaveStatue` restore + `RoomTransitionCompleteEvent` |
| 6 | Dynamic audio | `MusicDirector` reacts to the room event (if present) |
| 7 | Tile bake + climb | `TileChunkBuilder` mesh+colliders, `ClimbableVolume` ladder |

The bootstrap **never references** MusicDirector or ZoneLightingController. It
emits one `RoomTransitionCompleteEvent`; those subscribers self-activate. That
decoupling *is* the test.

---

## Scene setup (Unity)

1. New scene. Keep your **Persistent** rig: `GameManager`, a **Player** prefab
   tagged `Player` (PlayerController + Movement + Input + Combat + Animator +
   CharacterController), a Camera, and the HUD `UIDocument`.
2. Create an empty GameObject `Bootstrap` at world origin → add
   **`VerticalSliceBootstrap`**.
3. Set **Enemy Layer** on the bootstrap to the same layer index your
   `PlayerCombat._enemyLayer` mask targets (e.g. `Enemy`). ⚠ If these don't
   match, your hits pass through enemies.
4. *(optional)* Assign a **Tile Atlas** (8×8) + **Tile Material** (HDRP/Lit with
   the atlas as Base Color Map). Unset → flat stone material is generated.
5. *(optional, for sight & sound)* Add `MusicDirector` (+`AudioManager`) and a
   `ZoneLightingController` on the HDRP Volume, each with an `EntranceHall`
   entry in its catalogue. They'll light up on the room event.
6. Press Play.

---

## Controls

`← →` move · `↑ ↓` climb · `Z`/`Space` jump · `J` attack · `K` sub-weapon ·
`E` save (near statue).

---

## Tuning note (real integration finding)

`EnemyDataSO` ships with **pixel-era defaults** from the Phaser prototype
(`MoveSpeed 80`, `DetectionRange 320`, `AttackRange 70`). In the 2.5D world
those are world-unit values now — `EnemyBase.DistToPlayer` is a world-space
`Vector3.Distance`. `VerticalSliceContent` re-tunes them to world units
(`MoveSpeed ~2–3`, `DetectionRange ~6–7`, `AttackRange ~1.3`). **Author future
`EnemyDataSO` assets in world units**, not pixels.

---

## Browser preview

`web-prototype/public/vertical-slice-demo.html` — the same room, fully
playable in a browser (canvas platformer): patrol/chase enemies, pit, ladder,
save statue, the top-left HUD, attack + sub-weapon, level-ups. Open it directly;
no build step.
