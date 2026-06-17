# Midnight Return — Tile Texture Pack Setup

All tiles use HDRP/Lit with full PBR channels.  
`HDRPTileMaterial.cs` wires BaseColorMap + NormalMap + MaskMap from Resources.

---

## Recommended Free Packs (CC0)

### Polyhaven (polyhaven.com/textures)
| Asset name | Use | Resolution |
|---|---|---|
| `cobblestone_floor_08` | Main floor tiles (FLOOR index 0) | 2K |
| `stone_bricks_02` | Wall tiles + ceiling (WALL index 1) | 2K |
| `old_brick_floor_001` | Background brick fill (BRICK index 8) | 2K |
| `rusty_metal_02` | Ladder rails + chains | 1K |

Download each at **2K PNG**, selecting these maps:
- `*_diff_2k.png` → BaseColor
- `*_nor_gl_2k.png` → Normal (GL convention, flips green for HDRP)
- `*_rough_2k.png` and `*_ao_2k.png` → combine into MaskMap (see below)

### AmbientCG (ambientcg.com)
`StoneBricksFace001`, `Cobblestone020`, `Metal032` — same channel naming convention.

---

## File Layout

```
Assets/
└── Art/
    └── Textures/
        ├── Stone/
        │   ├── CobblestoneFloor_BaseColor.png
        │   ├── CobblestoneFloor_Normal.png
        │   └── CobblestoneFloor_MaskMap.png     ← packed (see below)
        ├── Brick/
        │   ├── StoneBricks_BaseColor.png
        │   ├── StoneBricks_Normal.png
        │   └── StoneBricks_MaskMap.png
        └── Metal/
            ├── RustyMetal_BaseColor.png
            ├── RustyMetal_Normal.png
            └── RustyMetal_MaskMap.png
```

---

## MaskMap Packing (HDRP)

HDRP's MaskMap packs four channels: **R=Metallic G=AO B=DetailMask A=Smoothness**

Roughness → Smoothness: `Smoothness = 1 - Roughness`

Quick pack in Photoshop / Krita:
1. Open AO map → copy to Green channel
2. Open Roughness map → invert → copy to Alpha channel
3. Red channel = 0 (non-metallic stone) or filled white (metal)
4. Blue channel = 255 (full detail mask)
5. Export as RGBA PNG → `*_MaskMap.png`

Or use the free **HDRP Mask Map Packer** Unity asset (Unity Package Manager search).

---

## Unity Import Settings

**BaseColor maps:**
- sRGB: ✓ (checked)
- Alpha Source: From Gray Scale (if AO-packed in alpha, otherwise None)
- Max Size: 2048

**Normal maps:**
- Texture Type: **Normal Map**
- Flip Green Channel: ✓ (Polyhaven uses OpenGL convention)
- Max Size: 2048

**MaskMap:**
- sRGB: ✗ (unchecked — linear data)
- Alpha Source: Input Texture Alpha
- Max Size: 2048

---

## Wiring Into the Bootstrap

Once textures are placed in `Assets/Art/Textures/`, add them as a sub-folder in `Resources/Textures/` so `Resources.Load<Texture2D>()` can find them, then `VerticalSliceBootstrap` picks them up automatically via `TryLoadPBRMaterial()`.

Or assign the textures directly in the Inspector on the `VerticalSliceBootstrap` serialized fields if you prefer not to use Resources.

---

## Tiling Scale

Stone floor tiles tile at **1:1 world unit** (one tile = one texture repeat).  
For wall tiles, a 2×2 tiling looks better — set `_BaseColorMap_ST` to `(2,2,0,0)` in the material.
