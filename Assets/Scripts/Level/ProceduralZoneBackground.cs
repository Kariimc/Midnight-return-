using System.Collections.Generic;
using UnityEngine;
using MidnightReturn.Data;

namespace MidnightReturn.Level
{
    // ══════════════════════════════════════════════════════════════════════════
    //  ProceduralZoneBackground — FEAT-01 scenery builder.
    //
    //  Spawns the six parallax bands described by a ZoneAtmosphereProfile as
    //  child quads, each driven by a ParallaxLayer. Back bands get hazed toward
    //  the zone fog colour (distance fade); the nearest band carries the arch /
    //  column silhouette. Living elements (drip / steam emitters) are spawned as
    //  pooled point emitters that feed VFXManager.
    //
    //  SwitchZone() tears down the prior set and rebuilds — cheap (6 quads), and
    //  only fires on room transitions, never per-frame.
    // ══════════════════════════════════════════════════════════════════════════
    public sealed class ProceduralZoneBackground : MonoBehaviour
    {
        [Header("Band Quad")]
        [Tooltip("World-unit size of each band quad (covers the camera view + margin).")]
        [SerializeField] private Vector2 _bandSize = new Vector2(40f, 24f);

        [Header("Shared Material")]
        [Tooltip("Unlit/transparent material cloned per band so tint+haze are independent.")]
        [SerializeField] private Material _bandMaterialTemplate;

        private readonly List<GameObject> _bands = new();
        private ZoneAtmosphereProfile _active;

        private static readonly int BaseColorId  = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId       = Shader.PropertyToID("_Color");

        public void SwitchZone(ZoneAtmosphereProfile profile)
        {
            if (profile == null || profile == _active) return;
            _active = profile;

            Teardown();
            Build(profile);
        }

        private void Teardown()
        {
            for (int i = 0; i < _bands.Count; i++)
                if (_bands[i]) Destroy(_bands[i]);
            _bands.Clear();
        }

        private void Build(ZoneAtmosphereProfile profile)
        {
            if (profile.Bands == null) return;

            for (int i = 0; i < profile.Bands.Length; i++)
            {
                var band = profile.Bands[i];
                var go   = GameObject.CreatePrimitive(PrimitiveType.Quad);
                go.name  = $"ParallaxBand_{i}";
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 0f, band.ZDepth);
                go.transform.localScale    = new Vector3(_bandSize.x, _bandSize.y, 1f);

                // Strip the quad's collider — pure visual.
                if (go.TryGetComponent<Collider>(out var col)) Destroy(col);

                // Haze the band toward fog colour by its Haze factor (distance fade).
                Color tint = Color.Lerp(band.Tint, profile.FogColor, band.Haze);

                var mr  = go.GetComponent<MeshRenderer>();
                var mat = _bandMaterialTemplate != null
                    ? new Material(_bandMaterialTemplate)
                    : MakeUnlit(tint);
                if (mat.HasProperty(BaseColorId)) mat.SetColor(BaseColorId, tint);
                if (mat.HasProperty(ColorId))     mat.SetColor(ColorId, tint);
                mr.sharedMaterial   = mat;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows    = false;

                var px     = go.AddComponent<ParallaxLayer>();
                px.FactorX = band.Factor;
                px.FactorY = band.Factor * 0.4f;
                px.ScrollX = band.ScrollX;
                px.TileSpan = _bandSize.x; // seamless horizontal wrap

                _bands.Add(go);
            }
        }

        private static Material MakeUnlit(Color c)
        {
            Shader sh = Shader.Find("HDRP/Unlit")
                     ?? Shader.Find("Unlit/Transparent")
                     ?? Shader.Find("Sprites/Default");
            var m = new Material(sh);
            if (m.HasProperty(BaseColorId)) m.SetColor(BaseColorId, c);
            if (m.HasProperty(ColorId))     m.SetColor(ColorId, c);
            return m;
        }
    }
}
