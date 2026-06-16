// HDRP Dissolve Death Effect
// Wire this into a ShaderGraph Custom Function node
// Inputs: UV (float2), DissolveAmount (float 0→1), NoiseTexture (Texture2D), EdgeColor (float3), EdgeWidth (float)
// Outputs: Alpha (float), EmissiveColor (float3)

#ifndef DISSOLVE_EFFECT_INCLUDED
#define DISSOLVE_EFFECT_INCLUDED

void DissolveEffect_float(
    float2     UV,
    float      DissolveAmount,     // 0 = fully visible, 1 = fully dissolved
    Texture2D  NoiseTexture,
    SamplerState NoiseSampler,
    float3     EdgeColor,          // e.g. orange/purple glow on edge
    float      EdgeWidth,          // e.g. 0.05–0.15
    out float  Alpha,
    out float3 EmissiveColor
)
{
    float noise    = SAMPLE_TEXTURE2D(NoiseTexture, NoiseSampler, UV).r;

    // Clip below dissolve threshold
    float threshold = noise - DissolveAmount;
    Alpha = step(0.0, threshold);   // binary clip

    // Glowing edge: bright band just above the dissolve line
    float edgeMask = saturate(threshold / max(EdgeWidth, 0.001));
    float edge     = 1.0 - edgeMask;                     // 1 at the edge, 0 away from it
    edge           = pow(edge, 3.0);                      // sharpen falloff
    EmissiveColor  = EdgeColor * edge * (1.0 + DissolveAmount * 5.0); // brighter as more dissolved
}

// ── Weapon Trail variant ────────────────────────────────────────────────────
void WeaponTrailFade_float(
    float2  UV,
    float3  TrailColor,
    float   Intensity,     // driven by swing speed / combo
    out float3 Color,
    out float  Alpha
)
{
    // UV.x = position along trail (0=tip, 1=handle), UV.y = width
    float falloff = pow(1.0 - UV.x, 2.5);      // sharper at handle
    float edge    = 1.0 - abs(UV.y * 2.0 - 1.0); // fade to edge
    Alpha  = falloff * edge * Intensity;
    Color  = TrailColor * (1.0 + falloff * 4.0 * Intensity); // HDR bloom on tip
}

// ── Screen Distortion (hit effect) ─────────────────────────────────────────
void ScreenDistortion_float(
    float2  ScreenUV,
    float   Strength,      // 0→0.05 typical
    float   Time,
    out float2 DistortedUV
)
{
    float2 wave = float2(
        sin(ScreenUV.y * 20.0 + Time * 8.0) * Strength,
        cos(ScreenUV.x * 20.0 + Time * 8.0) * Strength * 0.5
    );
    DistortedUV = ScreenUV + wave;
}

// ── Blood Splatter decal ───────────────────────────────────────────────────
void BloodSplatter_float(
    float2 UV,
    float  SplatterAmount, // 0→1
    Texture2D SplatterMask,
    SamplerState SplatterSampler,
    out float3 Color,
    out float  Alpha
)
{
    float mask = SAMPLE_TEXTURE2D(SplatterMask, SplatterSampler, UV).r;
    Alpha  = saturate(mask - (1.0 - SplatterAmount)) * SplatterAmount;
    Color  = float3(0.55, 0.01, 0.01);   // dark crimson
}

#endif
