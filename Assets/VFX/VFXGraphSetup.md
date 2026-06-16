# VFX Graph Assets — Setup Guide

All VFX Graph files go in Assets/VFX/. Create each as a new VFX Graph asset
(right-click → Create → Visual Effects → Visual Effect Graph).

---

## 1. HitSpark.vfx
**Purpose:** Weapon-on-enemy impact burst — most frequently seen VFX

Node setup:
```
[Spawn]  Constant Burst: Count = {Color:"ParticleCount" int}
[Initialize]
  - Set Lifetime: Random[0.15, 0.35]
  - Set Velocity: Spherical, Speed Random[3, 12]
  - Set Color (HDR): {Color:"Color" float4}
  - Set Scale: {Color:"Scale" float} × Random[0.3, 1.0]
[Update]
  - Drag: 8 (rapid deceleration)
  - Gravity: -5 (slight float)
  - Color over Lifetime: full color → transparent (last 40% of life)
[Output Particle Quad]
  - Shader: VFX/Particles/Add Blend (additive — no overdraw darkening)
  - Blend Mode: Additive
  - Sort Priority: 5
  - Orient: FacingCamera
```

Exposed Properties:
- `Color` (Vector4, HDR)
- `Scale` (float, default 1.0)
- `ParticleCount` (int, default 12)

---

## 2. DeathBurst.vfx
**Purpose:** Enemy death explosion — high particle count, dramatic

Node setup:
```
[Spawn]  Constant Burst: Count = {Color:"ParticleCount" int}
[Initialize]
  - Set Lifetime: Random[0.4, 1.2]
  - Set Velocity: Spherical, Speed Random[2, 18]
  - Set Color (HDR): {Color:"Color" float4} × 3 (HDR multiplier)
  - Set Size: Random[0.05, 0.3]
  - Set Angular Velocity: Random[-360, 360]
[Update]
  - Gravity: 12 (fall to ground)
  - Drag: 2
  - Color over Lifetime: white hot → color → transparent
  - Flipbook Player (if using sprite sheet): 8 frames
[Output Particle Quad]
  - Blend Mode: Additive
  - Sort Priority: 4
```

Additional Secondary System (smoke trails):
```
[Spawn] Rate: 20/second, Lifetime = 2s
[Initialize]
  - Velocity: Small random upward
  - Color: Grey, alpha 0.4
  - Size: 0.3–0.8
[Update]
  - Scale over Lifetime: 1 → 2.5
  - Color over Lifetime: grey → transparent
[Output Particle Lit Quad]
  - Lighting: Yes (smoke receives scene lighting)
```

---

## 3. DashBurst.vfx
**Purpose:** Player dash start — directional speed lines + energy ring

System A — Speed Lines:
```
[Spawn] Burst: 20 particles
[Initialize]
  - Velocity: Direction = {Color:"Direction" float} × -1 (opposite of dash)
  - Speed: Random[8, 20]
  - Lifetime: Random[0.1, 0.25]
  - Scale: (0.02w × 0.4h) — stretched lines
  - Color (HDR): #8844ff × 4
[Update] Drag: 15
[Output Particle Quad] Additive
```

System B — Energy Ring:
```
[Spawn] Burst: 1 particle
[Initialize]
  - Position: World origin
  - Lifetime: 0.3
  - Size: 0.5
[Update]
  - Scale over Lifetime: 0.5 → 4.0 (ring expands)
  - Alpha over Lifetime: 1 → 0
[Output Particle Mesh]
  - Mesh: Circle/Ring
  - Color (HDR): #cc88ff × 6
```

---

## 4. WeaponSwing.vfx
**Purpose:** Weapon trail arc — one system per combo hit, scale up with combo

```
[Spawn] Rate: 60/second while enabled, Duration: {Color:"SwingDuration" float}
[Initialize]
  - Strip: YES (particle strip for continuous trail)
  - Position: Attached to weapon bone via attached Transform
  - Lifetime: 0.12
  - Color: {Color:"TrailColor" float4} × {Color:"TrailScale" float}
[Update]
  - Color over Lifetime: full → transparent (last 70% of life)
[Output Particle Strip]
  - Blend Mode: Additive
  - Texture Mode: Stretch (UV stretches along strip length)
  - Texture: Assets/VFX/Textures/TrailGradient.png
```

---

## 5. LevelUpBurst.vfx
**Purpose:** Dramatic level-up celebration — full-character aura + ring

System A — Upward Sparks:
```
[Spawn] Burst: {Color:"ParticleCount" int} = 120
[Initialize]
  - Velocity: Cone upward, angle 30°, speed 5–20
  - Lifetime: 0.8–1.5
  - Color (HDR): gold (#ffdd00) × 8
  - Size: 0.04–0.15
[Update] Gravity: -3 (float upward)
[Output] Additive
```

System B — Expanding Ring:
```
[Spawn] Burst: 1, repeated 3×
[Initialize]
  - Size: 0.3
  - Lifetime: 0.6
  - Color: gold, alpha 1
[Update]
  - Scale × Lifetime: → 6
  - Alpha: → 0
[Output Particle Mesh] Ring mesh, Additive
```

System C — Column of Light:
```
[Spawn] Rate: continuous, 30/sec, Duration: 1.5s
[Initialize]
  - Position: Y = Random[0, 3]
  - Velocity: Up, slow
  - Color (HDR): white × 12
  - Size: (0.5w × 0.15h)
[Output] Additive
```

---

## Shared VFX Texture Assets (Assets/VFX/Textures/)
Create these in your art pipeline:
- `TrailGradient.png` — 256×32, white on left, transparent on right (horizontal gradient)
- `SoftSpark.png` — 64×64, white dot with soft falloff
- `DustPuff.png` — Spritesheet 4×4 frames, grey smoke puff
- `BloodSplash.png` — 128×128, red ink splatter for decals
- `DissolveNoise.png` — 512×512, Perlin/Voronoi noise, grayscale (used by dissolve shader)
- `StarGlyph.png` — 64×64, four-pointed star silhouette for hit sparks

---

## VFX Graph Property Convention
All VFX in this project use these standardised property names:
| Property     | Type    | Description |
|-------------|---------|-------------|
| Color        | Vector4 | RGBA, HDR values allowed |
| Scale        | float   | Uniform scale multiplier |
| ParticleCount| int     | Burst particle count |
| Direction    | float   | -1 or 1 for left/right |
| TrailColor   | Vector4 | Weapon trail tint |
| TrailScale   | float   | Trail intensity (1 + comboIndex × 0.3) |
| SwingDuration| float   | Time to emit trail |
