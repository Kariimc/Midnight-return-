using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using MidnightReturn.Data;
using MidnightReturn.Map;
using MidnightReturn.Utils;

namespace MidnightReturn.Level
{
    // Applies a ZoneLightingSO mood on RoomTransitionCompleteEvent, lerping the
    // key/fill lights and the HDRP Volume overrides (fog, bloom, vignette, color
    // grade, exposure). Any override missing from the profile is skipped, so the
    // same controller works against a minimal or a fully-authored Volume.
    [RequireComponent(typeof(Volume))]
    public sealed class ZoneLightingController : MonoBehaviour
    {
        [Header("Zone Catalogue")]
        [SerializeField] private ZoneLightingSO[] _zones;

        [Header("Scene Lights")]
        [SerializeField] private Light _keyLight;
        [SerializeField] private Light _fillLight;

        private Volume _volume;

        // Cached HDRP overrides (may be null if absent from the profile)
        private Fog              _fog;
        private Bloom            _bloom;
        private Vignette         _vignette;
        private ColorAdjustments _color;
        private Exposure         _exposure;

        private readonly Dictionary<ZoneType, ZoneLightingSO> _byZone = new();
        private Coroutine _blend;
        private ZoneLightingSO _current;

        private void Awake()
        {
            _volume = GetComponent<Volume>();
            if (_volume.profile != null)
            {
                _volume.profile.TryGet(out _fog);
                _volume.profile.TryGet(out _bloom);
                _volume.profile.TryGet(out _vignette);
                _volume.profile.TryGet(out _color);
                _volume.profile.TryGet(out _exposure);
            }

            foreach (var z in _zones)
                if (z != null) _byZone[z.Zone] = z;
        }

        private void OnEnable()  => EventBus.Subscribe<RoomTransitionCompleteEvent>(OnRoomEntered);
        private void OnDisable() => EventBus.Unsubscribe<RoomTransitionCompleteEvent>(OnRoomEntered);

        private void OnRoomEntered(RoomTransitionCompleteEvent e)
        {
            if (!_byZone.TryGetValue(e.Zone, out var zone) || zone == null) return;
            if (zone == _current) return;
            _current = zone;

            if (_blend != null) StopCoroutine(_blend);
            _blend = StartCoroutine(Blend(zone, zone.TransitionTime));
        }

        // Apply instantly (e.g. on boot before the first transition completes).
        public void ApplyImmediate(ZoneType zoneType)
        {
            if (!_byZone.TryGetValue(zoneType, out var zone) || zone == null) return;
            _current = zone;
            if (_blend != null) StopCoroutine(_blend);
            _blend = StartCoroutine(Blend(zone, 0f));
        }

        private IEnumerator Blend(ZoneLightingSO z, float duration)
        {
            // Snapshot start values
            Color   ambFrom   = RenderSettings.ambientLight;
            Color   keyColF   = _keyLight ? _keyLight.color : Color.black;
            Quaternion keyRotF = _keyLight ? _keyLight.transform.rotation : Quaternion.identity;
            float   keyIntF   = KeyIntensity();
            Color   fillColF  = _fillLight ? _fillLight.color : Color.black;

            float fogPathF = _fog != null ? _fog.meanFreePath.value : 100f;
            Color fogColF  = _fog != null ? _fog.color.value : Color.black;
            float bloomF   = _bloom != null ? _bloom.intensity.value : 0f;
            Color bloomTF  = _bloom != null ? _bloom.tint.value : Color.white;
            float vigF     = _vignette != null ? _vignette.intensity.value : 0f;
            Color vigCF    = _vignette != null ? _vignette.color.value : Color.black;
            Color filterF  = _color != null ? _color.colorFilter.value : Color.white;
            float postExpF = _color != null ? _color.postExposure.value : 0f;
            float fixExpF  = _exposure != null ? _exposure.fixedExposure.value : 8f;

            Quaternion keyRotT = Quaternion.Euler(z.KeyLightEuler);

            float t = 0f;
            duration = Mathf.Max(0.0001f, duration);
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / duration);

                RenderSettings.ambientLight = Color.Lerp(ambFrom, z.AmbientColor, k);

                if (_keyLight)
                {
                    _keyLight.color = Color.Lerp(keyColF, z.KeyLightColor, k);
                    _keyLight.transform.rotation = Quaternion.Slerp(keyRotF, keyRotT, k);
                    SetKeyIntensity(Mathf.Lerp(keyIntF, z.KeyLightIntensity, k));
                }
                if (_fillLight) _fillLight.color = Color.Lerp(fillColF, z.FillColor, k);

                if (_fog != null)
                {
                    _fog.enabled.value      = z.FogEnabled;
                    _fog.meanFreePath.value = Mathf.Lerp(fogPathF, z.MeanFreePath, k);
                    _fog.color.value        = Color.Lerp(fogColF, z.FogColor, k);
                }
                if (_bloom != null)
                {
                    _bloom.intensity.value = Mathf.Lerp(bloomF, z.BloomIntensity, k);
                    _bloom.tint.value      = Color.Lerp(bloomTF, z.BloomTint, k);
                }
                if (_vignette != null)
                {
                    _vignette.intensity.value = Mathf.Lerp(vigF, z.VignetteIntensity, k);
                    _vignette.color.value     = Color.Lerp(vigCF, z.VignetteColor, k);
                }
                if (_color != null)
                {
                    _color.colorFilter.value  = Color.Lerp(filterF, z.ColorFilter, k);
                    _color.postExposure.value = Mathf.Lerp(postExpF, z.PostExposure, k);
                }
                if (_exposure != null && z.UseFixedExposure)
                {
                    _exposure.mode.value = ExposureMode.Fixed;
                    _exposure.fixedExposure.value = Mathf.Lerp(fixExpF, z.FixedExposure, k);
                }

                yield return null;
            }
            _blend = null;
        }

        // HDRP stores intensity on HDAdditionalLightData; fall back to Light.intensity.
        private float KeyIntensity()
        {
            if (_keyLight && _keyLight.TryGetComponent<HDAdditionalLightData>(out var hd))
                return hd.intensity;
            return _keyLight ? _keyLight.intensity : 0f;
        }

        private void SetKeyIntensity(float lux)
        {
            if (_keyLight && _keyLight.TryGetComponent<HDAdditionalLightData>(out var hd))
                hd.intensity = lux;
            else if (_keyLight)
                _keyLight.intensity = lux;
        }
    }
}
