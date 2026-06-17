using UnityEngine;
using MidnightReturn.Map;

namespace MidnightReturn.Data
{
    // Per-zone HDRP lighting & post mood. ZoneLightingController resolves one of
    // these on room entry and lerps the scene's key light, fog, and the HDRP
    // Volume overrides (bloom / vignette / color grade / exposure) toward it.
    [CreateAssetMenu(fileName = "ZoneLight_", menuName = "MidnightReturn/Zone Lighting")]
    public class ZoneLightingSO : ScriptableObject
    {
        [Header("Identity")]
        public ZoneType Zone;
        public string   DisplayName;

        [Header("Ambient")]
        public Color AmbientColor = new Color(0.05f, 0.04f, 0.08f);

        [Header("Key Light (directional)")]
        public Color   KeyLightColor     = new Color(0.55f, 0.55f, 0.75f);
        [Tooltip("HDRP lux. Catacombs ~2000, Throne Room ~6000.")]
        public float   KeyLightIntensity = 3000f;
        public Vector3 KeyLightEuler     = new Vector3(50f, -30f, 0f);

        [Header("Fill / Rim Tint")]
        public Color FillColor     = new Color(0.10f, 0.06f, 0.18f);
        public float FillIntensity = 800f;

        [Header("Fog")]
        public bool  FogEnabled = true;
        public Color FogColor   = new Color(0.05f, 0.03f, 0.10f);
        [Range(0f, 1f)] public float FogDensity = 0.35f; // mapped to HDRP meanFreePath

        [Header("Post — Bloom")]
        public float BloomIntensity = 0.6f;
        public Color BloomTint      = Color.white;

        [Header("Post — Vignette")]
        [Range(0f, 1f)] public float VignetteIntensity = 0.35f;
        public Color VignetteColor = Color.black;

        [Header("Post — Color Grade")]
        public Color ColorFilter   = Color.white;
        [Range(-3f, 3f)] public float PostExposure = 0f;

        [Header("Post — Exposure (fixed)")]
        public bool  UseFixedExposure = false;
        [Range(-5f, 12f)] public float FixedExposure = 8f;

        [Header("Transition")]
        public float TransitionTime = 2.0f;

        // HDRP fog uses meanFreePath (distance); higher density = shorter path.
        public float MeanFreePath => Mathf.Lerp(200f, 12f, FogDensity);
    }
}
