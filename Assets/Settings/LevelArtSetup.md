# Level Art Pipeline — Phase 7 Setup

Three subsystems: **tile system**, **parallax**, **per-zone HDRP lighting**, plus
the **Climb** mechanic. All data-driven via ScriptableObjects, all decoupled.

---

## 1. Tile System

### TileSetSO — the palette
`Assets → Create → MidnightReturn → Tile Set`

| Field | Meaning |
|-------|---------|
| `Atlas` | tile sheet texture (point-filtered for pixel art) |
| `Material` | HDRP/Lit using the atlas as Base Color Map |
| `Columns` / `Rows` | atlas grid |
| `TileWorldSize` | world units per tile (1 = one Unity unit) |
| `SolidTileIndices` | atlas indices that generate colliders |

Tiles are addressed by **atlas index** (row-major, 0-based, top-left origin).

### TilemapLayerSO — the authored grid
`Assets → Create → MidnightReturn → Tilemap Layer`

- `Width` × `Height`, then `Tiles[]` (row-major, top row first, `-1` = empty).
- `ZDepth` — world Z for this layer. **Gameplay collision layer must be Z=0.**
- `BuildColliders` — merged box colliders for solid tiles.

Stack multiple layers (background fill / midground detail / foreground occluders)
at different `ZDepth` to compose a room.

### TileChunkBuilder — the baker
Put on a GameObject with MeshFilter + MeshRenderer. Assign a TileSetSO + a
TilemapLayerSO. `Build()` (also a context-menu item) bakes the whole layer into
**one combined mesh = one draw call**, no per-tile GameObjects. Solid tiles get
merged horizontal-run box colliders.

```
Room_Geometry
├── TileLayer_BG    (TileChunkBuilder, ZDepth +4, no colliders)
├── TileLayer_Main  (TileChunkBuilder, ZDepth  0, colliders ✓)  ← gameplay plane
└── TileLayer_FG    (TileChunkBuilder, ZDepth -2, no colliders)
```

---

## 2. Parallax

`ParallaxLayer` on each background plane:

| Field | Effect |
|-------|--------|
| `FactorX` / `FactorY` | 0 = locked to world, 1 = locked to camera (infinitely far) |
| `ScrollX` / `ScrollY` | constant auto-drift (fog/cloud bands) |
| `TileSpan` | seamless horizontal wrap width (0 = none) |

Self-driving — reads `Camera.main` travel from spawn, no controller needed.
Suggested depth ramp: distant spires 0.9, far towers 0.75, mid 0.5, near pillars
0.2, gameplay tiles 0.0.

---

## 3. Per-Zone HDRP Lighting

### ZoneLightingSO — the mood
`Assets → Create → MidnightReturn → Zone Lighting`, one per `ZoneType`.

Controls ambient color, key/fill light color+intensity+angle, fog
(color + density → HDRP `meanFreePath`), and post — bloom, vignette, color
grade, fixed exposure. Plus `TransitionTime`.

### ZoneLightingController — the applier
Put on the scene's HDRP **Volume** GameObject. Assign:
- `_zones` — the ZoneLightingSO array (one per zone)
- `_keyLight` — the directional light (HDRP intensity via HDAdditionalLightData)
- `_fillLight` — optional secondary

On `RoomTransitionCompleteEvent` it resolves the zone and **lerps** every light +
Volume override toward the profile over `TransitionTime`. Missing overrides are
skipped, so it works against a minimal or fully-authored Volume profile. Call
`ApplyImmediate(zone)` on boot for the first room.

> Same event-driven pattern as MusicDirector — zones drive sight AND sound from
> the single `RoomTransitionCompleteEvent`.

---

## 4. Climb (ladders / chains)

### ClimbableVolume — the trigger
Put on a ladder/chain object with a trigger `BoxCollider` sized to the climbable
span. While the player overlaps it, `PlayerMovement.CanClimb` is true and the
ladder centerline + vertical bounds feed snapping/clamping.

### Flow
- **Mount:** press Up on a ladder (Idle/Run/Jump/Fall), or Down when standing on
  its top.
- **Climb:** `MoveAxis.y` drives vertical speed, gravity off, X snaps to the
  ladder; the `Climb` anim freezes when not moving.
- **Dismount:** leave the volume, jump off, climb over the top, or reach the
  bottom on the ground.

PlayerMovement adds `SetClimbable()`, `SetClimbing()`, `TickClimb()`, and the
`CanClimb / IsClimbing / AtLadderTop / AtLadderBottom` flags. `ClimbState` is
registered in `PlayerController.BuildFSM()`. Requires a `Climb` animator clip.

---

## 5. Browser Reference

`web-prototype/public/level-art-demo.html` — Three.js demo: 5 parallax depths,
shader-tiled ground + wall, torch bloom, and live per-zone lighting (fog, key
light, ambient, bloom) you can switch and pan through. Keys: `◂ ▸` pan, `Space`
auto-pan, `1–5` zones.
