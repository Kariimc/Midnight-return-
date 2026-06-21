# Midnight Return — Art Direction Bible (AAA Gothic 2.5D)

Research-driven spec distilled from Castlevania: Symphony of the Night, Bloodstained,
Blasphemous, Hollow Knight, Ender Lilies, and Nine Sols. This is the contract for every
character/enemy/texture asset — web demo (`vertical-slice-demo.html`) and Unity HDRP alike.

## 1. The AAA fix-stack (priority order)
Why "programmer-art" reads cheap, and the concrete fix for each — apply in this order:

1. **Silhouette first.** Black-shape test: fill the design solid black; if it isn't readable
   and distinct, no shading saves it. Asymmetric, dynamic line-of-action — never symmetric boxes.
2. **Value structure.** Squint test: clear dark/mid/light, no muddy midtones. 4–5 value steps/material max.
3. **Hue-shifted ramps.** Never lighten-with-white / darken-with-black. Shadows rotate cool
   (blue/violet) + desaturate; highlights rotate warm (orange/yellow) + saturate. ~15–40° hue/step.
4. **Single key light** (upper-left), held consistently. Kills pillow shading. Add a **rim/back light**
   to separate the sprite from dark backgrounds.
5. **Selout** (selective outline): break/lighten the outline where light hits — no flat black keyline.
6. **AO + specular.** Darken crevices/contact (cooler than base shadow). Single bright pixel on
   eyes/gems/metal; metal tints its specular to the metal's own hue.
7. **Detail hierarchy.** Render face + weapon sharpest; armor/body simpler. Don't detail everything.
8. **Grime/edge breakup** kills flatness: scratches at corners, asymmetry, noise — "authored" not "asset pack."

## 2. Gothic palette (vampire-16 derived, hue-shifted) — `[deepShadow, shadow, mid, light]`
```
cloak   #0a0413 #1c0c38 #34215e #5b3ba0   midnight blue-purple cloth
crimson #2e0610 #62101f #b41f2c #ff464d   blood / cape / collar
gold    #4a2208 #9d551c #e0982f #ffe27b   trim / buckle / candle
bone    #574a38 #8a7c62 #c4b596 #f2ead2   bleached bone / skull
steel   #0d1019 #262d3e #566078 #b4c0dc   blade / silver hair
flesh   #5e3326 #9a5a40 #cc8d6a #eec6a4   pale aristocratic skin
necro   #16240e #33491c #62842f #aad05a   rotting zombie flesh
bronze  #2c1606 #6e3e12 #bb7d27 #f2c65e   clockwork metal
verd    #0c2a24 #1d5048 #3d867a #74c6b1   verdigris / oxidation in recesses
rust    #241016 #4a2730 #8a3f29 #d06a34   corroded iron
voidc   #040208 #0c0818 #181128 #2c1f47   shade/wraith void body
ghost   #0a2230 #1d4a64 #4f86b8 #bfe2ff   spectral cyan
```
Scene ratio for gothic mood: ~70% near-black, ~20% deep accent (plum/burgundy), ~10% bone/silver for legibility.
**One reserved emissive** for undead eye-glow (orange-red) so it pops against the desaturated world.

## 3. Animation — the "alive-stack" (nothing is ever fully still)
- **Idle breath:** ~1px torso/shoulder bob, 2 frames, 4–6 FPS. On a *different, longer* cycle than sway.
- **Secondary motion (cape/hair):** same line-of-action as body but lagged 1–2 frames (overlapping action).
  Tips move faster + lag more than the anchored root. On stop/turn → drag, then settle (follow-through).
- **Squash & stretch (preserve volume):** stretch tall+thin at peak jump/fall velocity; squash wide+flat on landing.
- **Attack = startup → active → recovery.** Anticipation = pull weapon *back*, longest hold (telegraph).
  Snap to impact pose (1-frame smear/stretch along motion), then ease out. Pairs with hitstop on impact frame.
- **FPS:** idle 4–6, walk/run 8–12, attack 10–15. Timing (holds) beats raw frame count (Skullgirls: a punch in ~6 frames).

## 4. Enemy design language (silhouette variety = each reads distinctly)
- **Skeleton** — bleached bone; partial/mismatched armor (helm + shield + class weapon) over exposed bone so the
  frame still reads; glowing eye-socket fire; throws own ribs. Blood-stained crimson bone for elites (palette swap).
- **Zombie** — necrotic green skin + purple/blue/black veins; hanging skin strips, decay shown in armor gaps;
  tattered blood-stained cloth; slow shamble + single melee tell (contrast skeleton's speed); survives dismemberment.
- **Shade** — void-black opaque body (HK Shade) wrapped in a soft blue aura (SOTN); two white pinprick eyes are the
  anchor; floats (no ground contact), lower body dissolves to wisps; rises ambient motes.
- **Wraith** — pointed **capirote hood** silhouette (Blasphemous); tattered cloak hem dissolving to mist; void face
  with hostile glowing red eyes; pale skeletal claw hands (the only hard/opaque element vs the soft void cloth).
- **Gear Golem** — aged brass/bronze + **verdigris in recesses, polished worn edges**; asymmetric exposed chest
  gear-core with a glowing amber core (destructible weak-point read); red eye glow beneath a helm visor; steam vents.
- **Vampire Bat** — dark near-silhouette body; **backlit translucent wing membrane with reddish-brown veins**
  (FakeSSS lit from behind by torchlight); wing flap + sine bob; color-code behavior (cling-dive vs sine-flyer).

## 5. SOTN reference facts (verified)
- Screen 256×240; sprites = hand-drawn 4-bit/16-color CLUT (15-bit 5/5/5 color), swappable cloak palettes.
- Alucard = heroic/bishōnen proportions (NOT chibi); animated idle; high-frame-count flowing cape (famous for 1997).
- Bloodstained (modern successor) = Unity 3D models + an "illustration" cel shader reading as 2D on a 2.5D plane —
  the same hybrid strategy this project uses (procedural effects/lighting + authored character assets).

## 6. Where this lives in code
- Web demo: `web-prototype/public/vertical-slice-demo.html` → `PAL` ramps, `rampG`/`glowDot`/`softShadow` helpers,
  `drawPlayerSprite` + 6 enemy renderers, upgraded `mkStone`.
- Unity HDRP hooks: `GothicRimLight_float` (rim, §1.4), `FakeSSSGothic_float` (bat wing / ghost translucency, §4),
  `DissolveEffect.hlsl` + emissive MaterialPropertyBlock (shade/wraith edge-dissolve + eye-glow), `ZoneLightingSO`
  (seed per-zone key/fill from the palette above), `SpriteSheetDataSO.SpriteClip.Fps` (per-clip rates, §3).
