// HDRP Lit Shader Extension — Gothic atmosphere lighting
// Use in ShaderGraph as Custom Lighting function node

#ifndef GOTHIC_LIT_EXTENSION_INCLUDED
#define GOTHIC_LIT_EXTENSION_INCLUDED

// Rim light: strong rim glow for silhouette contrast (back-lit torch look)
void GothicRimLight_float(
    float3  NormalWS,        // world-space normal
    float3  ViewDirWS,       // world-space view direction
    float3  RimColor,        // e.g. deep orange/purple
    float   RimPower,        // 3–8 typical
    float   RimIntensity,    // HDR value, e.g. 2–8
    out float3 RimContrib
)
{
    float rim    = 1.0 - saturate(dot(NormalWS, ViewDirWS));
    rim          = pow(rim, RimPower);
    RimContrib   = RimColor * rim * RimIntensity;
}

// Stylised subsurface scattering approximation for creature flesh/wings
void FakeSSSGothic_float(
    float3 LightDirWS,
    float3 NormalWS,
    float3 SSSColor,         // e.g. blood red for vampires
    float  SSSThickness,     // 0.1–0.5
    out float3 SSSContrib
)
{
    float backFace = saturate(dot(-LightDirWS, NormalWS));
    SSSContrib = SSSColor * backFace * SSSThickness;
}

// Emissive pulse — for glowing eyes, cursed sigils, boss health thresholds
void EmissivePulse_float(
    float3 BaseEmissive,
    float  Time,
    float  PulseFrequency,   // e.g. 1.5
    float  PulseAmplitude,   // e.g. 0.5
    out float3 PulsedEmissive
)
{
    float pulse   = 1.0 + sin(Time * PulseFrequency * 6.283) * PulseAmplitude;
    PulsedEmissive = BaseEmissive * pulse;
}

// Vertex wind for capes/cloth
void CapeWindDeformation_float(
    float3 PositionOS,       // object-space position
    float  WindStrength,
    float  WindFrequency,
    float  Time,
    float  ClothMask,        // 0 = pinned (shoulders), 1 = free (hem)
    out float3 DeformedPositionOS
)
{
    float wave = sin(PositionOS.y * WindFrequency + Time * 3.0) * WindStrength * ClothMask;
    DeformedPositionOS = PositionOS + float3(wave, 0.0, wave * 0.3);
}

#endif
