# HDRP Quality Settings — Midnight Return

## Project Setup (Edit → Project Settings → HDRP Default Settings)

### Rendering
- Lit Shader Mode: Both (deferred + forward for transparency)
- Color Buffer Format: R11G11B10 (HDR, no alpha needed for opaque)
- Depth Buffer Format: Depth 32 Bits
- Motion Vectors: Enabled (for TAA + motion blur)
- Decals: Enabled (blood splatter on walls/floors)
- Opaque Object Motion Vector: Enabled

### Lighting
- Screen Space Ambient Occlusion: Quality = High
- Screen Space Global Illumination: Quality = High  
- Screen Space Reflections: Quality = Medium
- Volumetric Lighting: Quality = Ultra (essential for the torch/candlelight atmosphere)
- Volumetric Clouds: Disabled (interior game, not needed)
- Physically Based Sky: Disabled (use HDRI sky probe instead)

### Anti-Aliasing
- Default: TAA (Temporal Anti-Aliasing) — smoothest for 2.5D
- Fallback: SMAA

### Post Processing (Global Volume Profile)
Bloom:
  - Threshold: 0.9 (only very bright HDR values bloom — torches, glowing eyes)
  - Intensity: 0.6 (default) → spikes during hits/level-up (driven via code)
  - Scatter: 0.7
  - Quality: High (bicubic upsampling)

Color Adjustments:
  - Color Filter: White (driven by screen flash code)
  - Post Exposure: 0.0
  - Contrast: 15 (adds gothic dramatic contrast)
  - Saturation: -10 (slightly desaturated base — more saturated VFX pop harder)
  - Hue Shift: 0

Vignette:
  - Mode: Procedural
  - Intensity: 0.2 (base) → spikes to 0.7 on damage hit
  - Smoothness: 0.5
  - Color: (0, 0, 0)

Chromatic Aberration:
  - Intensity: 0.05 (base) → spikes on damage and boss abilities
  - Quality: High

Depth of Field:
  - Mode: Off during gameplay (breaks 2.5D readability)
  - Enable only during cutscenes/boss intros

Motion Blur:
  - Mode: Camera And Objects
  - Quality: Medium
  - Clamp: 0.05 (subtle — too much breaks Metroidvania precision)

Film Grain:
  - Intensity: 0.08 (subtle, adds film-like texture to dark areas)
  - Response: 0.8

### Shadows
- Max Shadow Distance: 150
- Cascade Count: 4
- Resolution: 4096 (main directional light)
- Point/Spot resolution: 1024
- Shadow Filtering: PCF 9-tap

### Camera Setup (Cinemachine Brain)
- Mode: Orthographic, Size = 6 (16:9 → 10.67 × 6 units visible)
- Blend Definition: Linear, 0.5s
- World Up Override: +Y
- Update Method: SmartFixedUpdate

### Lighting Setup Per Zone
Each zone uses a separate Lighting Scene (additive loading):

| Zone | Key Light Color | Ambient | Fog | Bloom |
|------|----------------|---------|-----|-------|
| Entrance Hall | Warm orange (torch) #ff8833 | Deep purple | Thin volumetric | 0.6 |
| Catacombs | Cold blue-green #33aaaa | Near-black | Dense fog | 0.4 |
| Cursed Library | Amber candlelight #ffcc55 | Dark sepia | Book-dust motes | 0.8 |
| Clocktower | Cool silver moonlight #aabbcc | Steely grey | Minimal | 0.5 |
| Throne Room | Red-violet #cc2266 | Deep crimson | Thick purple | 1.2 |

### HDRP Decal Layer
- Blood splatter: Decal Projector on wall/floor geometry post-hit
- Scorch marks: Boss AOE residue
- Shadow blobs: Under every enemy (fake fake AO)

### Layers
```
0:  Default
3:  Player
6:  Enemy
7:  Projectile
8:  Ground
9:  Platform
10: Trigger
11: Hitbox (no render, physics only)
12: VFX (no collision)
13: UI3D (world-space UI elements)
```

### Render Pipeline Asset
Use `HDRenderPipelineAsset_Midnight` with:
- Lit Shader Mode: Both
- Support Distortion: Yes (for hit ripple effect)
- Support Subsurface Scattering: Yes (for enemy flesh shaders)
- Support Decals: Yes
- Support High Quality Line Rendering: Yes (for weapon trails)
