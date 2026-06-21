using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using MidnightReturn.Utils;

namespace MidnightReturn.Systems
{
    // ══════════════════════════════════════════════════════════════════════════
    //  ScreenJuiceManager — Metroid-Dread-style "impact juice" brain.
    //
    //  EventBus-driven, fully decoupled. Owns the *contextual* post spikes that
    //  fire only on moments (not always-on, per the Dread tech analysis):
    //    • PlayerDamagedEvent → chromatic-aberration spike (zone-keyed ceiling)
    //    • EnemyDiedEvent     → short bloom pop
    //    • BossDefeatedEvent  → 2s cinematic vignette + slow colour drain
    //
    //  All overrides are restored to their captured baseline so the manager
    //  never fights ZoneLightingController for ownership of the Volume.
    // ══════════════════════════════════════════════════════════════════════════
    public sealed class ScreenJuiceManager : MonoBehaviour
    {
        public static ScreenJuiceManager Instance { get; private set; }

        [Header("HDRP Post Volume")]
        [SerializeField] private Volume _volume;

        [Header("Damage Chromatic Aberration")]
        [Tooltip("Fallback CA ceiling if no zone profile is active.")]
        [Range(0f, 1f)] [SerializeField] private float _damageChromaticMax = 0.6f;
        [SerializeField] private float _chromaticDecay = 0.25f;

        private ChromaticAberration _ca;
        private Bloom               _bloom;
        private Vignette            _vignette;

        private float _caBaseline, _bloomBaseline, _vigBaseline;
        private Coroutine _caCo, _bloomCo, _bossCo;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (_volume != null && _volume.profile != null)
            {
                _volume.profile.TryGet(out _ca);
                _volume.profile.TryGet(out _bloom);
                _volume.profile.TryGet(out _vignette);
                if (_ca != null)       _caBaseline    = _ca.intensity.value;
                if (_bloom != null)    _bloomBaseline = _bloom.intensity.value;
                if (_vignette != null) _vigBaseline   = _vignette.intensity.value;
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<PlayerDamagedEvent>(OnDamaged);
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Subscribe<BossDefeatedEvent>(OnBossDefeated);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerDamagedEvent>(OnDamaged);
            EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Unsubscribe<BossDefeatedEvent>(OnBossDefeated);
        }

        // ── Damage → chromatic spike ──────────────────────────────────────────
        private void OnDamaged(PlayerDamagedEvent e)
        {
            if (e.Damage <= 0 || _ca == null) return;

            float ceiling = _damageChromaticMax;
            var cinema = Level.ZoneCinemaDirector.Instance;
            if (cinema != null && cinema.Current != null)
                ceiling = cinema.Current.DamageChromaticMax;

            if (_caCo != null) StopCoroutine(_caCo);
            _caCo = StartCoroutine(Decay(_ca.intensity, ceiling, _caBaseline, _chromaticDecay));
        }

        // ── Enemy death → bloom pop ───────────────────────────────────────────
        private void OnEnemyDied(EnemyDiedEvent e)
        {
            if (_bloom == null) return;
            if (_bloomCo != null) StopCoroutine(_bloomCo);
            _bloomCo = StartCoroutine(Decay(_bloom.intensity, _bloomBaseline + 1.2f, _bloomBaseline, 0.35f));
        }

        // ── Boss defeat → cinematic vignette ──────────────────────────────────
        private void OnBossDefeated(BossDefeatedEvent e)
        {
            if (_bossCo != null) StopCoroutine(_bossCo);
            _bossCo = StartCoroutine(BossCinematic());
        }

        private IEnumerator BossCinematic()
        {
            const float DURATION = 2.0f;
            float t = 0f;
            while (t < DURATION)
            {
                t += Time.unscaledDeltaTime;
                float k = t / DURATION;
                // Vignette pulses up then releases; bloom blooms then settles.
                if (_vignette != null)
                    _vignette.intensity.value = Mathf.Lerp(_vigBaseline + 0.45f, _vigBaseline, k);
                if (_bloom != null)
                    _bloom.intensity.value = Mathf.Lerp(_bloomBaseline + 2.5f, _bloomBaseline, Mathf.SmoothStep(0f, 1f, k));
                yield return null;
            }
            if (_vignette != null) _vignette.intensity.value = _vigBaseline;
            if (_bloom != null)    _bloom.intensity.value    = _bloomBaseline;
        }

        // Spike a ClampedFloatParameter to `peak`, then ease back to `baseline`.
        private IEnumerator Decay(ClampedFloatParameter param, float peak, float baseline, float duration)
        {
            param.value = peak;
            float t = 0f;
            duration = Mathf.Max(0.0001f, duration);
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                param.value = Mathf.Lerp(peak, baseline, t / duration);
                yield return null;
            }
            param.value = baseline;
        }
    }
}
