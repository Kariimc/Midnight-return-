using UnityEngine;
using MidnightReturn.Map;

namespace MidnightReturn.Data
{
    // ══════════════════════════════════════════════════════════════════════════
    //  ZoneAtmosphereProfile — FEAT-01 "Zone Cinema System" data contract.
    //
    //  Sits ALONGSIDE ZoneLightingSO (which owns HDRP light/post mood). This SO
    //  owns the *staged scenery*: the six depth-layered parallax bands, the
    //  living background elements (drips, steam, swaying banners), and the
    //  ambient particle bed. ZoneCinemaDirector resolves one per zone on room
    //  entry and hands it to ProceduralZoneBackground + VFXManager.
    //
    //  Decoupling proof: nothing here references gameplay. A zone "looks" a way
    //  purely by swapping this asset — no scene rewiring.
    // ══════════════════════════════════════════════════════════════════════════
    [CreateAssetMenu(fileName = "ZoneAtmos_", menuName = "MidnightReturn/Zone Atmosphere Profile")]
    public sealed class ZoneAtmosphereProfile : ScriptableObject
    {
        [Header("Identity")]
        public ZoneType Zone;
        public string   DisplayName;

        // ── Six parallax bands (back → front) ──────────────────────────────────
        [System.Serializable]
        public struct ParallaxBand
        {
            [Tooltip("0 = locked to world, 1 = camera-locked (infinitely distant).")]
            [Range(0f, 1f)] public float Factor;
            [Tooltip("Auto-scroll drift in world units/sec (clouds, fog bands).")]
            public float ScrollX;
            [Tooltip("Tint multiplied over the band sprite/mesh.")]
            public Color Tint;
            [Tooltip("Distance haze: 0 sharp, 1 fully washed toward FogColor.")]
            [Range(0f, 1f)] public float Haze;
            [Tooltip("Z depth in world units (sorting + perspective).")]
            public float ZDepth;
        }

        [Header("Parallax Bands (6 recommended: sky → near arch)")]
        public ParallaxBand[] Bands = new ParallaxBand[6];

        // ── Atmosphere wash ────────────────────────────────────────────────────
        [Header("Atmosphere Wash")]
        public Color FogColor      = new Color(0.08f, 0.05f, 0.14f);
        [Range(0f, 1f)] public float FogDensity = 0.3f;
        [Tooltip("Rising mist tint drawn over the lower screen band.")]
        public Color MistColor     = new Color(0.12f, 0.06f, 0.22f, 0.18f);

        // ── Living background elements ─────────────────────────────────────────
        [Header("Living Elements")]
        [Tooltip("Ceiling water drips (Catacombs). 0 = off.")]
        [Range(0f, 1f)] public float DripRate     = 0f;
        [Tooltip("Wall steam vents (Clocktower). Frames between puffs; 0 = off.")]
        public int      SteamInterval = 0;
        [Tooltip("Hanging banner sway amplitude in world units. 0 = no banners.")]
        public float    BannerSway    = 0f;

        // ── Ambient particle bed ───────────────────────────────────────────────
        [Header("Ambient Particle Bed")]
        public Color AmbientMoteColor = new Color(0.72f, 0.63f, 0.88f);
        [Range(0, 12)] public int AmbientMoteCount = 4;

        // ── Contextual post hooks (read by ScreenJuiceManager) ─────────────────
        [Header("Contextual Post")]
        [Tooltip("Max chromatic aberration kicked on player damage in this zone.")]
        [Range(0f, 1f)] public float DamageChromaticMax = 0.6f;
        [Tooltip("Color grade applied as a soft multiply over the whole frame.")]
        public Color ColorGrade = new Color(0.16f, 0.08f, 0.24f, 0.07f);

        [Header("Transition")]
        public float TransitionTime = 0.5f;

        // HDRP fog uses meanFreePath (distance); higher density = shorter path.
        public float MeanFreePath => Mathf.Lerp(200f, 12f, FogDensity);
    }
}
