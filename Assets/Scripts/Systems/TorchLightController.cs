using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace MidnightReturn.Systems
{
    // ══════════════════════════════════════════════════════════════════════════
    //  TorchLightController — animates an HDRP point light to simulate organic
    //  flicker, crystal pulse, or slow heartbeat.
    //
    //  Drop on any GameObject with a Light component. ZoneLightingController can
    //  call SetZoneColor() to rekey the color palette per zone without replacing
    //  the component.
    //
    //  Flicker quality: two Perlin layers at different frequencies are blended
    //  so the pattern never sounds mechanical (no obvious looping).
    // ══════════════════════════════════════════════════════════════════════════
    [RequireComponent(typeof(Light))]
    public sealed class TorchLightController : MonoBehaviour
    {
        public enum FlickerMode { Torch, Crystal, Pulse, None }

        // Converts HDRP lux intensities to a sane Builtin/URP Light.intensity range.
        private const float NON_HDRP_SCALE = 0.0025f;

        [SerializeField] private FlickerMode _mode          = FlickerMode.Torch;
        [SerializeField] private float       _baseIntensity = 800f;   // HDRP lux
        [SerializeField] private float       _flickerAmount = 0.24f;  // ± fraction of base
        [SerializeField] private float       _flickerSpeed  = 7.6f;   // noise sample rate
        [SerializeField] private Color       _baseColor     = new Color(1.00f, 0.62f, 0.22f);
        [SerializeField] private Color       _hotColor      = new Color(1.00f, 0.36f, 0.08f);
        [SerializeField] private float       _range         = 6f;     // world units

        private Light                  _light;
        private HDAdditionalLightData  _hdLight;
        private float                  _phaseA;
        private float                  _phaseB;

        private void Awake()
        {
            _light   = GetComponent<Light>();
            _hdLight = _light.GetComponent<HDAdditionalLightData>();

            if (_hdLight != null)
            {
                _hdLight.SetIntensity(_baseIntensity, LightUnit.Lux);
                _hdLight.range = _range;
                // Real-time shadow maps — this is what casts dynamic shadows from
                // torches onto tiles and characters (the core 2.5D AAA jump).
                _hdLight.EnableShadows(true);
            }
            else
            {
                // Non-HDRP fallback (e.g. URP/Builtin preview): use raw Light units.
                _light.intensity = Mathf.Max(0f, _baseIntensity * NON_HDRP_SCALE);
                _light.range     = _range;
                _light.shadows   = LightShadows.Soft;
            }

            _light.color = _baseColor;
        }

        private void Update()
        {
            _phaseA += Time.deltaTime * _flickerSpeed;
            _phaseB += Time.deltaTime * (_flickerSpeed * 0.41f); // incommensurate rate → no loop

            float n1 = Mathf.PerlinNoise(_phaseA, 0.5f) * 2f - 1f;
            float n2 = Mathf.PerlinNoise(_phaseB, 1.7f) * 2f - 1f;
            float noise = n1 * 0.72f + n2 * 0.28f;            // weighted blend

            float newIntensity;
            Color newColor;

            switch (_mode)
            {
                case FlickerMode.Torch:
                    newIntensity = _baseIntensity * (1f + noise * _flickerAmount);
                    newColor     = Color.Lerp(_baseColor, _hotColor, (noise + 1f) * 0.5f * 0.28f);
                    break;

                case FlickerMode.Crystal:
                    // Smooth pulse with subtle shimmer — save statues / pickup orbs.
                    float crystalK = 0.88f + 0.12f * Mathf.Abs(Mathf.Sin(_phaseA * 0.7f));
                    newIntensity   = _baseIntensity * crystalK;
                    newColor       = Color.Lerp(_baseColor, Color.white, 0.18f + Mathf.Abs(noise) * 0.09f);
                    break;

                case FlickerMode.Pulse:
                    // Slow heartbeat — boss arenas, activated statues.
                    newIntensity = _baseIntensity * (0.74f + 0.26f * Mathf.Sin(_phaseA * 0.38f));
                    newColor     = _baseColor;
                    break;

                default: // None
                    newIntensity = _baseIntensity;
                    newColor     = _baseColor;
                    break;
            }

            if (_hdLight != null)
                _hdLight.SetIntensity(Mathf.Max(0f, newIntensity), LightUnit.Lux);
            else
                _light.intensity = Mathf.Max(0f, newIntensity * NON_HDRP_SCALE); // lux → Builtin range

            _light.color = newColor;
        }

        // ── Public API ─────────────────────────────────────────────────────────

        public void SetMode(FlickerMode mode) => _mode = mode;

        // ZoneLightingController calls this to rekey the palette on zone transitions.
        public void SetZoneColor(Color primary, Color hot)
        {
            _baseColor = primary;
            _hotColor  = hot;
        }

        public void SetBaseIntensity(float lux)
        {
            _baseIntensity = lux;
            if (_hdLight != null) _hdLight.SetIntensity(lux, LightUnit.Lux);
            else                  _light.intensity = lux * NON_HDRP_SCALE;
        }

        // Called by VerticalSliceBootstrap to configure a freshly-spawned light GO.
        public static TorchLightController Configure(
            GameObject go,
            FlickerMode mode,
            Color primary,
            Color hot,
            float intensityLux,
            float range)
        {
            var ctrl = go.AddComponent<TorchLightController>();
            ctrl._mode          = mode;
            ctrl._baseColor     = primary;
            ctrl._hotColor      = hot;
            ctrl._baseIntensity = intensityLux;
            ctrl._range         = range;
            return ctrl;
        }
    }
}
