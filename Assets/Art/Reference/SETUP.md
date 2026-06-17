# Sprite Sheet Setup Guide

## Step 1 — Drop the PNG here

Place the sprite sheet PNG in this directory:

```
Assets/Art/Reference/SotN_SpriteSheet.png
```

Name it exactly `SotN_SpriteSheet.png` (or update the SpriteSheetDataSO reference below).

## Step 2 — Unity Import Settings

Select the PNG in the Project window and set these in the Inspector:

| Setting | Value |
|---------|-------|
| Texture Type | **Default** (not Sprite, since we sample it as an atlas via UV offset) |
| Non-Power of 2 | **None** |
| Alpha Is Transparency | ✅ |
| Filter Mode | **Point (no filter)** — preserves crisp pixel art |
| Compression | **None** or RGBA 32 bit |
| Generate Mip Maps | ❌ off |
| Read/Write Enabled | ❌ off (not needed at runtime) |

Click **Apply**.

## Step 3 — Create a SpriteSheetDataSO

Right-click in Project → **Create → MidnightReturn → Sprite Sheet Data**

Configure:
- **Sheet** → drag in `SotN_SpriteSheet.png`
- **Columns** → number of frames across the sheet
- **Rows** → number of rows down the sheet

Then add one entry per animation in **Clips**:

| Name | StartFrame | FrameCount | Fps | Loop |
|------|-----------|------------|-----|------|
| Idle | 0 | 4 | 8 | ✅ |
| Walk | 4 | 6 | 10 | ✅ |
| Run | 10 | 8 | 12 | ✅ |
| Jump | 18 | 3 | 8 | ❌ |
| Fall | 21 | 2 | 8 | ✅ |
| Dash | 23 | 4 | 16 | ✅ |
| WallSlide | 27 | 2 | 8 | ✅ |
| Attack1 | 29 | 4 | 14 | ❌ |
| Attack2 | 33 | 4 | 14 | ❌ |
| Attack3 | 37 | 5 | 14 | ❌ |
| AirAttack | 42 | 4 | 14 | ❌ |
| Death | 46 | 6 | 8 | ❌ |

> **Frame indices above are placeholders.** Open the PNG and count the actual frames
> per row starting from 0. Update these values to match the real layout.

## Step 4 — Wire up the Player Sprite Quad

In the Player prefab hierarchy, add a child GameObject:

```
Player (PlayerController, PlayerMovement, PlayerInputHandler, PlayerCombat, Animator)
└── SpriteQuad
    ├── MeshFilter   (Quad mesh)
    ├── MeshRenderer (HDRP/Lit material — see below)
    ├── SpriteAnimator
    ├── SpriteBillboard
    └── PlayerSpriteController (drag Player into _controller field)
```

### Material Setup (HDRP/Lit)

1. Create a new Material → shader **HDRP/Lit**
2. Set **Surface Type** = Transparent, **Alpha Clipping** = ✅
3. Set **Base Color Map** = your `SotN_SpriteSheet.png`
4. In `SpriteAnimator`, set **Sheet** = the SpriteSheetDataSO you created

The `SpriteAnimator` drives `_BaseColorMap_ST` tiling+offset every frame via
`MaterialPropertyBlock` — zero GC, fully HDRP-compatible.

## Step 5 — Wire Enemy Sprite Quads

Same pattern under each enemy prefab:

```
PatrolEnemy (EnemyBase, CharacterController …)
└── SpriteQuad
    ├── MeshFilter + MeshRenderer (HDRP/Lit)
    ├── SpriteAnimator  (assign EnemySpriteSheetDataSO)
    ├── SpriteBillboard
    └── EnemySpriteController  (Type = Patrol, _enemy = parent EnemyBase)
```

Enemy sprite clip names expected by `EnemySpriteController`:

| Type | Clips needed |
|------|-------------|
| Patrol | `Idle`, `Walk`, `Attack`, `Death` |
| Flying | `Fly`, `Chase`, `Dive`, `Death` |
| Ranged | `Idle`, `Walk`, `Attack`, `Death` |

## Step 6 — HTML WebGL Demo

The `web-prototype/public/game-visual-demo.html` demo renders the sprite sheet
in Three.js. Drop `SotN_SpriteSheet.png` into `web-prototype/public/` and update
the `SPRITE_SHEET_URL` constant at the top of the file.

The demo sprite shader applies analytic torch attenuation and the correct UV
region for whichever animation frame is active.
