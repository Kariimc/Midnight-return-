# Player Sprite Sheets — Alucard

Generated sprite sheets for the player character, wired into the sprite-in-3D
pipeline (`SpriteSheetDataSO` → `SpriteAnimator` → `PlayerSpriteController`).

## ⚠️ Drop the 4 PNGs here (binaries not in the repo)

The generated PNGs live on the Higgsfield CDN, which the build environment's
network policy blocks (`host_not_allowed`), so they could not be committed
automatically. Download them from Higgsfield and place them here with these
**exact filenames** — all the code wiring is keyed to them:

| Filename | Sheet | Rows | Higgsfield job id |
|---|---|---|---|
| `Alucard_Core_v1.png`     | Core, original        | 16 | `a4daf116-d7ca-4dda-873e-4d6a00898ba8` |
| `Alucard_Core_v2.png`     | Core, anime-samurai   | 16 | `b8c76495-7447-44bb-985c-50ed7a69d31c` |
| `Alucard_Advanced_v1.png` | Advanced, original    | 10 | `81e613f2-b46b-4d4c-95fb-a4c5b68a5c73` |
| `Alucard_Advanced_v2.png` | Advanced, anime-samurai | 10 | `0a09ae4b-3802-40f7-8543-4636958d5eca` |

Import settings are applied automatically by
`Assets/Scripts/Editor/PlayerSpriteImportSettings.cs` (Default texture type,
Point filter, alpha-is-transparency, no mipmaps, uncompressed, 8192 max) — no
manual Inspector work needed.

## Wiring (one-time)

1. Add a `PlayerSpriteBootstrap` component to a GameObject in the scene.
2. Assign the 4 PNGs to its `Core V1 / Core V2 / Advanced V1 / Advanced V2` slots.
3. Leave `Player` empty to auto-find by the `Player` tag.
4. Press Play. It builds the sprite quad on the player and injects the active sheet.

**Play-mode hotkeys:** `Tab` cycles the 4 sheets · `P` toggles preview mode
(cycles through *every* clip of the active sheet — the in-engine A/B comparison).

`Core` sheets drive the real `PlayerSpriteController` from FSM state. `Advanced`
sheets (and preview mode) cycle every clip so you can eyeball all rows.

## Atlas grid → clip map

Defined in code in `PlayerSpriteContent.cs`. Frames are row-major:
`absFrame = rowIndex * Columns + col`.

**Core** — `Columns = 10`, `Rows = 16`:

| Row | Clip(s) | Frames |
|--|--|--|
| 1 | `Idle` | 8 |
| 2 | `Walk` | 10 |
| 3 | `Run` | 8 |
| 4 | `Crouch` | 4 |
| 5 | `Jump` | 6 |
| 6 | `JumpApex` | 4 |
| 7 | `Fall` | 6 |
| 8 | `Dash` | 6 |
| 9 | `WallSlide` | 6 |
| 10–12 | `Attack1` / `Attack2` / `Attack3` | 6 / 6 / 8 |
| 13 | `AirAttack` | 6 |
| 14 | `Hurt` | 4 |
| 15 | `Death` | 8 |
| 16 | `Climb` | 8 |

**Advanced** — `Columns = 8`, `Rows = 10`:
`TurnAround`(6) · `BackDash`(8) · `SubWeapon`(8) · `SpellCast`(8) ·
`DragonKick`(8) · `DoubleJump`(8) · `AirDash`(6) · `Landing`(5) ·
`WallJump`(6) · `KneelSave`(6).

## ⚠️ Grid-tuning caveat

These are AI-generated sheets — the sprites are **not** guaranteed to sit on a
perfectly uniform pixel grid. The `Columns`/`Rows`/`StartFrame` values are a
structured starting point derived from the generation prompt, not a
pixel-measured slice. If frames look misaligned in-engine, tune `CORE_COLUMNS` /
`ADV_COLUMNS` (or per-clip `StartFrame`) in `PlayerSpriteContent.cs`, or re-slice
each sheet into a clean uniform atlas and keep the same clip names.

## Driving the Advanced moves from gameplay

The single `SpriteAnimator` samples one texture, so Advanced clips
(`SubWeapon`, `DragonKick`, `TurnAround`, …) can't render off the Core atlas.
To drive them from their FSM states, add a second quad+animator fed the Advanced
config and switch the active animator per state — or merge both sheets into one
atlas and extend the clip map. Until then those states fall back to `Idle`.
