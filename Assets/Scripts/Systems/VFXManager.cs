using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using TMPro;
using MidnightReturn.Utils;

namespace MidnightReturn.Systems
{
    // Centralised VFX Manager — wraps Unity VFX Graph for all particle effects
    // All methods are null-safe (ok to call before scene loaded)
    public class VFXManager : MonoBehaviour
    {
        public static VFXManager Instance { get; private set; }

        // ── VFX Graph asset references (assign in Inspector) ──────────────────
        [Header("VFX Graph Assets")]
        [SerializeField] private VisualEffectAsset _hitSparkVFX;
        [SerializeField] private VisualEffectAsset _deathBurstVFX;
        [SerializeField] private VisualEffectAsset _healBurstVFX;
        [SerializeField] private VisualEffectAsset _levelUpBurstVFX;
        [SerializeField] private VisualEffectAsset _dashBurstVFX;
        [SerializeField] private VisualEffectAsset _landDustVFX;
        [SerializeField] private VisualEffectAsset _runDustVFX;
        [SerializeField] private VisualEffectAsset _wallScrapeVFX;
        [SerializeField] private VisualEffectAsset _weaponSwingVFX;
        [SerializeField] private VisualEffectAsset _bloodVFX;

        [Header("Prefabs")]
        [SerializeField] private GameObject  _damageNumberPrefab; // TextMeshPro world-space
        [SerializeField] private GameObject  _afterimagePrefab;   // Ghost mesh with dissolve mat
        [SerializeField] private GameObject  _screenFlashPrefab;  // Full-screen quad

        [Header("HDRP Post Processing")]
        [SerializeField] private Volume      _globalVolume;
        private ChromaticAberration _chromaticAberration;
        private Vignette            _vignette;
        private ColorAdjustments    _colorAdjustments;
        private Bloom               _bloom;

        // ── Pooled VFX instances ──────────────────────────────────────────────
        private readonly Queue<VisualEffect> _vfxPool = new();
        private VisualEffect _wallScrapeInstance;

        // ── HDRP Volume param save ────────────────────────────────────────────
        private float _defaultBloomIntensity;
        private float _defaultChromaticIntensity;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Cache HDRP post-processing overrides
            if (_globalVolume != null)
            {
                _globalVolume.profile.TryGet(out _chromaticAberration);
                _globalVolume.profile.TryGet(out _vignette);
                _globalVolume.profile.TryGet(out _colorAdjustments);
                _globalVolume.profile.TryGet(out _bloom);
                if (_bloom != null) _defaultBloomIntensity = _bloom.intensity.value;
                if (_chromaticAberration != null) _defaultChromaticIntensity = _chromaticAberration.intensity.value;
            }

            EventBus.Subscribe<ScreenFlashEvent>(OnScreenFlash);
            EventBus.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<ScreenFlashEvent>(OnScreenFlash);
            EventBus.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
        }

        // ── Spawn helpers ─────────────────────────────────────────────────────
        public void SpawnHitSpark(Vector3 position, bool isCrit, Color trailColor = default)
        {
            var vfx = GetPooledVFX(_hitSparkVFX, position);
            if (vfx == null) return;
            vfx.SetVector4("Color",  isCrit ? new Vector4(1f, 0.9f, 0f, 1f) : new Vector4(trailColor.r, trailColor.g, trailColor.b, 1f));
            vfx.SetFloat("Scale",    isCrit ? 1.8f : 1.0f);
            vfx.SetInt("ParticleCount", isCrit ? 24 : 12);
            vfx.SendEvent("OnPlay");
            StartCoroutine(ReturnToPool(vfx, 1.5f));
        }

        public void PlayDeathBurst(VisualEffectAsset asset, Vector3 position, Color color)
        {
            var vfx = GetPooledVFX(asset ?? _deathBurstVFX, position);
            if (vfx == null) return;
            vfx.SetVector4("Color", new Vector4(color.r, color.g, color.b, 1f));
            vfx.SetInt("ParticleCount", 80);
            vfx.SendEvent("OnPlay");

            // HDRP bloom spike on death
            StartCoroutine(BloomSpike(2.5f, 0.3f));
            StartCoroutine(ReturnToPool(vfx, 3f));
        }

        public void SpawnDeathDissolve(GameObject target)
        {
            // Drive _DissolveAmount on all HDRP materials via shader property
            // The EnemyBase coroutine handles property block updates directly
        }

        public void PlayEnemyHitVFX(VisualEffectAsset asset, Vector3 position)
        {
            var vfx = GetPooledVFX(asset, position);
            if (vfx == null) return;
            vfx.SendEvent("OnPlay");
            StartCoroutine(ReturnToPool(vfx, 1f));
        }

        public void SpawnDamageNumber(Vector3 worldPos, int value, bool isCrit)
        {
            if (_damageNumberPrefab == null) return;
            var go   = Instantiate(_damageNumberPrefab, worldPos, Quaternion.identity);
            var tmp  = go.GetComponentInChildren<TextMeshPro>();
            if (tmp != null)
            {
                tmp.text     = isCrit ? $"<size=120%><b>{value}</b></size>" : value.ToString();
                tmp.color    = isCrit ? new Color(1f, 0.85f, 0f) : Color.white;
                tmp.fontSize = isCrit ? 5.5f : 4f;
            }
            StartCoroutine(FloatAndFade(go, isCrit ? 1.2f : 0.8f));
        }

        public void SpawnHealBurst(Vector3 position)
        {
            var vfx = GetPooledVFX(_healBurstVFX, position);
            if (vfx == null) return;
            vfx.SetVector4("Color", new Vector4(0.2f, 1f, 0.4f, 1f));
            vfx.SendEvent("OnPlay");
            StartCoroutine(ReturnToPool(vfx, 2f));
        }

        public void SpawnLevelUpBurst(Vector3 position)
        {
            var vfx = GetPooledVFX(_levelUpBurstVFX, position);
            if (vfx == null) return;
            vfx.SetInt("ParticleCount", 120);
            vfx.SendEvent("OnPlay");
            StartCoroutine(BloomSpike(4f, 0.6f));
            StartCoroutine(ReturnToPool(vfx, 4f));
        }

        public void PlayDashBurst(Vector3 position, int direction)
        {
            var vfx = GetPooledVFX(_dashBurstVFX, position);
            if (vfx == null) return;
            vfx.SetFloat("Direction", direction);
            vfx.SendEvent("OnPlay");
            StartCoroutine(ReturnToPool(vfx, 0.5f));
        }

        public void SpawnLandDust(Vector3 position)
        {
            var vfx = GetPooledVFX(_landDustVFX, position);
            if (vfx == null) return;
            vfx.SendEvent("OnPlay");
            StartCoroutine(ReturnToPool(vfx, 1f));
        }

        public void SpawnRunDust(Vector3 position, int direction)
        {
            var vfx = GetPooledVFX(_runDustVFX, position);
            if (vfx == null) return;
            vfx.SetFloat("Direction", -direction);
            vfx.SendEvent("OnPlay");
            StartCoroutine(ReturnToPool(vfx, 0.5f));
        }

        public void PlayWallScrape(Vector3 position)
        {
            if (_wallScrapeVFX == null) return;
            if (_wallScrapeInstance == null)
            {
                var go = new GameObject("WallScrapeVFX");
                _wallScrapeInstance = go.AddComponent<VisualEffect>();
                _wallScrapeInstance.visualEffectAsset = _wallScrapeVFX;
            }
            _wallScrapeInstance.transform.position = position;
            _wallScrapeInstance.gameObject.SetActive(true);
            _wallScrapeInstance.Play();
        }

        public void StopWallScrape()
        {
            _wallScrapeInstance?.Stop();
            if (_wallScrapeInstance != null)
                _wallScrapeInstance.gameObject.SetActive(false);
        }

        public void PlayWeaponSwing(Vector3 position, int direction, int comboIndex)
        {
            var vfx = GetPooledVFX(_weaponSwingVFX, position);
            if (vfx == null) return;
            vfx.SetFloat("Direction",  direction);
            vfx.SetInt("ComboIndex",   comboIndex);
            // Trail intensity ramps up with combo
            vfx.SetFloat("TrailScale", 1f + comboIndex * 0.3f);
            vfx.SendEvent("OnPlay");
            StartCoroutine(ReturnToPool(vfx, 0.4f));
        }

        public void SpawnHitFlash(Vector3 position, Color color)
        {
            // HDRP chromatic aberration spike
            StartCoroutine(ChromaticSpike(0.8f, 0.12f));
        }

        // ── Afterimage ────────────────────────────────────────────────────────
        public void FadeOutAfterimage(GameObject img, float duration)
        {
            StartCoroutine(FadeAfterimage(img, duration));
        }

        private IEnumerator FadeAfterimage(GameObject img, float duration)
        {
            if (img == null) yield break;
            var mpb      = new MaterialPropertyBlock();
            var renderer = img.GetComponentInChildren<Renderer>();
            float t = 0f;
            while (t < duration && img != null)
            {
                t += Time.deltaTime;
                renderer?.GetPropertyBlock(mpb);
                mpb.SetFloat("_Alpha", 1f - t / duration);
                renderer?.SetPropertyBlock(mpb);
                yield return null;
            }
            if (img != null) Destroy(img);
        }

        // ── HDRP Post-Processing Effects ──────────────────────────────────────
        private void OnScreenFlash(ScreenFlashEvent e)
        {
            StartCoroutine(ScreenFlashCoroutine(e.Color, e.Duration));
        }

        private IEnumerator ScreenFlashCoroutine(Color color, float duration)
        {
            if (_colorAdjustments == null) yield break;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float alpha = 1f - (t / duration);
                _colorAdjustments.colorFilter.Override(Color.Lerp(color, Color.white, 1f - alpha));
                yield return null;
            }
            _colorAdjustments.colorFilter.Override(Color.white);
        }

        private void OnPlayerDamaged(PlayerDamagedEvent e)
        {
            StartCoroutine(VignetteHit(0.7f, 0.3f));
            StartCoroutine(ChromaticSpike(1f, 0.2f));
        }

        private IEnumerator VignetteHit(float targetIntensity, float duration)
        {
            if (_vignette == null) yield break;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float v = Mathf.Lerp(targetIntensity, 0.2f, t / duration);
                _vignette.intensity.Override(v);
                yield return null;
            }
            _vignette.intensity.Override(0.2f);
        }

        private IEnumerator BloomSpike(float targetIntensity, float duration)
        {
            if (_bloom == null) yield break;
            _bloom.intensity.Override(targetIntensity);
            yield return new WaitForSeconds(duration * 0.3f);
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float v = Mathf.Lerp(targetIntensity, _defaultBloomIntensity, t / duration);
                _bloom.intensity.Override(v);
                yield return null;
            }
            _bloom.intensity.Override(_defaultBloomIntensity);
        }

        private IEnumerator ChromaticSpike(float target, float duration)
        {
            if (_chromaticAberration == null) yield break;
            _chromaticAberration.intensity.Override(target);
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float v = Mathf.Lerp(target, _defaultChromaticIntensity, t / duration);
                _chromaticAberration.intensity.Override(v);
                yield return null;
            }
            _chromaticAberration.intensity.Override(_defaultChromaticIntensity);
        }

        // ── VFX Pool ──────────────────────────────────────────────────────────
        private VisualEffect GetPooledVFX(VisualEffectAsset asset, Vector3 position)
        {
            if (asset == null) return null;
            VisualEffect vfx;
            if (_vfxPool.Count > 0)
            {
                vfx = _vfxPool.Dequeue();
            }
            else
            {
                var go = new GameObject($"VFX_{asset.name}");
                vfx    = go.AddComponent<VisualEffect>();
            }
            vfx.visualEffectAsset = asset;
            vfx.transform.position = position;
            vfx.gameObject.SetActive(true);
            return vfx;
        }

        private IEnumerator ReturnToPool(VisualEffect vfx, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (vfx == null) yield break;
            vfx.Stop();
            vfx.gameObject.SetActive(false);
            _vfxPool.Enqueue(vfx);
        }

        // ── Damage number float animation ─────────────────────────────────────
        private IEnumerator FloatAndFade(GameObject go, float duration)
        {
            float t   = 0f;
            var start = go.transform.position;
            while (t < duration && go != null)
            {
                t += Time.deltaTime;
                go.transform.position = start + Vector3.up * (t / duration) * 2f;
                var tmp = go.GetComponentInChildren<TextMeshPro>();
                if (tmp != null) tmp.alpha = 1f - (t / duration);
                yield return null;
            }
            if (go != null) Destroy(go);
        }
    }
}
