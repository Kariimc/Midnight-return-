using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace MidnightReturn.Systems
{
    // ══════════════════════════════════════════════════════════════════════════
    //  AudioManager — Phase 6 layered audio engine.
    //
    //  Buses (conceptual; map to AudioMixer groups in production — see
    //  Assets/Settings/AudioMixerSetup.md):
    //    • Music  — two base sources (A/B) ping-pong crossfade + one combat-layer
    //               source faded by combat intensity (adaptive layering).
    //    • Ambient— one looping environmental bed.
    //    • SFX    — round-robin pool, 2D or positional 3D per call.
    //
    //  Dynamic mix: MusicDirector drives SetCombatIntensity()/PlayBoss()/etc.
    //  Everything degrades gracefully if the AudioMixer / reverb filter are unset.
    // ══════════════════════════════════════════════════════════════════════════
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Mixer (optional — production routing)")]
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private string _masterParam  = "MasterVol";
        [SerializeField] private string _musicParam   = "MusicVol";
        [SerializeField] private string _sfxParam     = "SfxVol";
        [SerializeField] private string _ambientParam = "AmbientVol";
        [SerializeField] private AudioReverbFilter _reverbFilter; // on SFX bus, optional

        [Header("SFX Pool")]
        [SerializeField] private int _sfxPoolSize = 16;

        // ── Music sources (built at runtime) ──────────────────────────────────
        private AudioSource _baseA, _baseB;     // exploration ping-pong
        private AudioSource _combatSrc;         // combat stem layer
        private AudioSource _ambientSrc;        // environmental bed
        private bool        _useA = true;       // which base source is live
        private AudioSource ActiveBase => _useA ? _baseA : _baseB;
        private AudioSource IdleBase   => _useA ? _baseB : _baseA;

        // ── Bus volumes (linear 0..1) ─────────────────────────────────────────
        private float _masterVol  = 1f;
        private float _musicVol    = 0.7f;
        private float _sfxVol      = 1f;
        private float _ambientVol  = 0.4f;
        private float _musicDuck   = 1f;        // transient duck multiplier

        // ── Adaptive layering ─────────────────────────────────────────────────
        private float _combatIntensity;          // target 0..1
        private float _combatLayerMax = 0.85f;
        private float _combatSmoothed;            // smoothed toward target

        private AudioSource[] _sfxPool;
        private int           _sfxPoolIdx;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            BuildMusicSources();
            BuildSfxPool();
        }

        private void BuildMusicSources()
        {
            _baseA     = MakeSource("Music_BaseA", loop: true);
            _baseB     = MakeSource("Music_BaseB", loop: true);
            _combatSrc = MakeSource("Music_Combat", loop: true);
            _ambientSrc = MakeSource("Ambient_Bed", loop: true);
            _combatSrc.volume = 0f;
        }

        private AudioSource MakeSource(string name, bool loop)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.loop = loop;
            src.playOnAwake = false;
            src.spatialBlend = 0f; // 2D
            return src;
        }

        private void BuildSfxPool()
        {
            _sfxPool = new AudioSource[_sfxPoolSize];
            for (int i = 0; i < _sfxPoolSize; i++)
            {
                var go = new GameObject($"SFX_{i}");
                go.transform.SetParent(transform);
                _sfxPool[i] = go.AddComponent<AudioSource>();
                _sfxPool[i].playOnAwake = false;
            }
        }

        private void Update()
        {
            // Smooth combat layer toward intensity target (adaptive layering)
            float target = _combatIntensity * _combatLayerMax;
            _combatSmoothed = Mathf.MoveTowards(_combatSmoothed, target, Time.unscaledDeltaTime * 1.6f);
            _combatSrc.volume = _combatSmoothed * _musicVol * _masterVol * _musicDuck;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  SFX
        // ══════════════════════════════════════════════════════════════════════
        public void Play(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return;
            var src = NextSfx();
            src.spatialBlend = 0f; // 2D
            src.clip   = clip;
            src.volume = volume * _sfxVol * _masterVol;
            src.pitch  = pitch + Random.Range(-0.03f, 0.03f);
            src.Play();
        }

        // Positional 3D one-shot.
        public void PlaySFX(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null) return;
            var src = NextSfx();
            src.transform.position = position;
            src.spatialBlend = 1f; // 3D
            src.minDistance  = 3f;
            src.maxDistance  = 28f;
            src.clip   = clip;
            src.volume = volume * _sfxVol * _masterVol;
            src.pitch  = 1f + Random.Range(-0.03f, 0.03f);
            src.Play();
        }

        private AudioSource NextSfx() => _sfxPool[_sfxPoolIdx++ % _sfxPoolSize];

        // ══════════════════════════════════════════════════════════════════════
        //  MUSIC — base / exploration
        // ══════════════════════════════════════════════════════════════════════
        // Legacy entry point (RoomManager calls this).
        public void CrossFade(AudioClip clip, float duration = 1.5f) => PlayExploration(clip, duration);

        // Legacy entry point — kept for older callers.
        public void PlayMusic(AudioClip clip, float fadeIn = 1.5f) => PlayExploration(clip, fadeIn);

        public void PlayExploration(AudioClip clip, float fade = 1.5f)
        {
            if (clip == null) return;
            if (ActiveBase.clip == clip && ActiveBase.isPlaying) return;
            StartCoroutine(CrossFadeBaseRoutine(clip, fade));
        }

        private IEnumerator CrossFadeBaseRoutine(AudioClip clip, float duration)
        {
            var from = ActiveBase;
            var to   = IdleBase;

            to.clip   = clip;
            to.volume = 0f;
            to.Play();

            float full = _musicVol * _masterVol * _musicDuck;
            float t = 0f;
            duration = Mathf.Max(0.01f, duration);
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = t / duration;
                to.volume   = Mathf.Lerp(0f, full, k);
                from.volume = Mathf.Lerp(full, 0f, k);
                yield return null;
            }
            to.volume = full;
            from.Stop();
            _useA = !_useA; // swap active
        }

        // ══════════════════════════════════════════════════════════════════════
        //  MUSIC — combat layer (adaptive)
        // ══════════════════════════════════════════════════════════════════════
        public void SetCombatLayer(AudioClip clip, float layerMax = 0.85f)
        {
            _combatLayerMax = layerMax;
            if (clip == null) { _combatSrc.Stop(); _combatSrc.clip = null; return; }
            if (_combatSrc.clip == clip && _combatSrc.isPlaying) return;
            _combatSrc.clip   = clip;
            _combatSrc.volume = 0f;
            _combatSrc.Play();
            _combatSmoothed = 0f;
        }

        // 0 = silent layer, 1 = full combat. MusicDirector drives this each frame.
        public void SetCombatIntensity(float intensity01)
            => _combatIntensity = Mathf.Clamp01(intensity01);

        // ══════════════════════════════════════════════════════════════════════
        //  MUSIC — boss override
        // ══════════════════════════════════════════════════════════════════════
        public void PlayBoss(AudioClip bossClip, float fade = 1f)
        {
            SetCombatIntensity(0f);
            _combatSrc.Stop();
            PlayExploration(bossClip, fade); // boss loop occupies the base channel
        }

        // ══════════════════════════════════════════════════════════════════════
        //  AMBIENT BED
        // ══════════════════════════════════════════════════════════════════════
        public void PlayAmbient(AudioClip clip, float volume = 0.4f, float fade = 1.5f)
        {
            _ambientVol = volume;
            if (clip == null) { StartCoroutine(FadeSourceOut(_ambientSrc, fade)); return; }
            if (_ambientSrc.clip == clip && _ambientSrc.isPlaying) return;
            StartCoroutine(SwapAmbient(clip, fade));
        }

        private IEnumerator SwapAmbient(AudioClip clip, float fade)
        {
            if (_ambientSrc.isPlaying) yield return FadeSourceOut(_ambientSrc, fade * 0.5f);
            _ambientSrc.clip   = clip;
            _ambientSrc.volume = 0f;
            _ambientSrc.Play();
            float full = _ambientVol * _masterVol;
            float t = 0f, half = Mathf.Max(0.01f, fade * 0.5f);
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                _ambientSrc.volume = Mathf.Lerp(0f, full, t / half);
                yield return null;
            }
            _ambientSrc.volume = full;
        }

        private IEnumerator FadeSourceOut(AudioSource src, float fade)
        {
            if (src == null || !src.isPlaying) yield break;
            float start = src.volume, t = 0f;
            fade = Mathf.Max(0.01f, fade);
            while (t < fade)
            {
                t += Time.unscaledDeltaTime;
                src.volume = Mathf.Lerp(start, 0f, t / fade);
                yield return null;
            }
            src.Stop();
        }

        public void FadeOutAll(float duration = 1.5f)
        {
            StartCoroutine(FadeSourceOut(_baseA, duration));
            StartCoroutine(FadeSourceOut(_baseB, duration));
            StartCoroutine(FadeSourceOut(_combatSrc, duration));
            StartCoroutine(FadeSourceOut(_ambientSrc, duration));
            SetCombatIntensity(0f);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  DUCKING — transient music dip (boss intro stinger, dialogue, etc.)
        // ══════════════════════════════════════════════════════════════════════
        public void DuckMusic(float factor = 0.35f, float duration = 0.6f)
            => StartCoroutine(DuckRoutine(Mathf.Clamp01(factor), duration));

        private IEnumerator DuckRoutine(float factor, float duration)
        {
            _musicDuck = factor;
            ApplyMusicVolumes();
            yield return new WaitForSecondsRealtime(duration);
            float t = 0f, recover = 0.4f;
            while (t < recover)
            {
                t += Time.unscaledDeltaTime;
                _musicDuck = Mathf.Lerp(factor, 1f, t / recover);
                ApplyMusicVolumes();
                yield return null;
            }
            _musicDuck = 1f;
            ApplyMusicVolumes();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  REVERB — zone character (drives an AudioReverbFilter if assigned)
        // ══════════════════════════════════════════════════════════════════════
        public void SetReverb(AudioReverbPreset preset, float wet = 0.5f)
        {
            if (_reverbFilter == null) return;
            _reverbFilter.reverbPreset = preset;
            // Wet/dry shaping when preset is User; otherwise preset governs.
            if (preset == AudioReverbPreset.User)
            {
                _reverbFilter.dryLevel = Mathf.Lerp(0f, -2000f, wet);
                _reverbFilter.room     = Mathf.Lerp(-2000f, 0f, wet);
            }
        }

        public void TransitionToSnapshot(AudioMixerSnapshot snapshot, float time = 1f)
        {
            if (snapshot != null) snapshot.TransitionTo(time);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  BUS VOLUMES
        // ══════════════════════════════════════════════════════════════════════
        public void SetMasterVolume(float v)  { _masterVol = Mathf.Clamp01(v); ApplyAll(); SetMixer(_masterParam, _masterVol); }
        public void SetMusicVolume(float v)   { _musicVol  = Mathf.Clamp01(v); ApplyMusicVolumes(); SetMixer(_musicParam, _musicVol); }
        public void SetSfxVolume(float v)     { _sfxVol    = Mathf.Clamp01(v); SetMixer(_sfxParam, _sfxVol); }
        public void SetAmbientVolume(float v) { _ambientVol = Mathf.Clamp01(v); _ambientSrc.volume = _ambientVol * _masterVol; SetMixer(_ambientParam, _ambientVol); }

        private void ApplyAll() { ApplyMusicVolumes(); if (_ambientSrc) _ambientSrc.volume = _ambientVol * _masterVol; }

        private void ApplyMusicVolumes()
        {
            float full = _musicVol * _masterVol * _musicDuck;
            if (ActiveBase != null && ActiveBase.isPlaying) ActiveBase.volume = full;
        }

        private void SetMixer(string param, float linear01)
        {
            if (_mixer == null || string.IsNullOrEmpty(param)) return;
            // Linear → dB; -80 dB floor at 0.
            float db = linear01 <= 0.0001f ? -80f : Mathf.Log10(linear01) * 20f;
            _mixer.SetFloat(param, db);
        }
    }
}
