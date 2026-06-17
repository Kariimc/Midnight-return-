using System.Collections.Generic;
using UnityEngine;
using MidnightReturn.Data;
using MidnightReturn.Map;
using MidnightReturn.Utils;

namespace MidnightReturn.Systems.Audio
{
    // ══════════════════════════════════════════════════════════════════════════
    //  MusicDirector — the dynamic-mix brain.
    //
    //  Owns a catalogue of MusicZoneSO and reacts to gameplay events to decide
    //  WHAT plays and HOW LOUD the combat layer sits:
    //
    //    RoomTransitionComplete → resolve zone → crossfade exploration stem,
    //                             arm combat layer, swap ambient bed, set reverb.
    //    Combat events          → bump a decaying intensity meter → adaptive layer.
    //    BossStarted            → boss track replaces zone music, duck stinger.
    //    BossDefeated           → return to the zone exploration + combat layer.
    //    PlayerDied             → fade everything to silence.
    //
    //  Place on the persistent audio rig next to AudioManager.
    // ══════════════════════════════════════════════════════════════════════════
    public sealed class MusicDirector : MonoBehaviour
    {
        public static MusicDirector Instance { get; private set; }

        [Header("Zone Catalogue")]
        [SerializeField] private MusicZoneSO[] _zones;

        [Header("Combat Intensity Model")]
        [Tooltip("How fast intensity bleeds back toward calm (per second).")]
        [SerializeField] private float _decayRate = 0.35f;
        [SerializeField] private float _attackBump = 0.18f;
        [SerializeField] private float _hitBump    = 0.40f;
        [SerializeField] private float _killBump   = 0.30f;

        private readonly Dictionary<ZoneType, MusicZoneSO> _byZone = new();

        private MusicZoneSO _current;
        private float       _intensity;
        private bool        _bossActive;
        private string      _state = "Silent";

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            foreach (var z in _zones)
                if (z != null) _byZone[z.Zone] = z;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<RoomTransitionCompleteEvent>(OnRoomEntered);
            EventBus.Subscribe<BossStartedEvent>(OnBossStarted);
            EventBus.Subscribe<BossDefeatedEvent>(OnBossDefeated);
            EventBus.Subscribe<PlayerAttackEvent>(OnPlayerAttack);
            EventBus.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<RoomTransitionCompleteEvent>(OnRoomEntered);
            EventBus.Unsubscribe<BossStartedEvent>(OnBossStarted);
            EventBus.Unsubscribe<BossDefeatedEvent>(OnBossDefeated);
            EventBus.Unsubscribe<PlayerAttackEvent>(OnPlayerAttack);
            EventBus.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        private void Update()
        {
            // Intensity decays toward calm; boss fights bypass adaptive layering.
            if (!_bossActive)
            {
                _intensity = Mathf.MoveTowards(_intensity, 0f, _decayRate * Time.deltaTime);
                AudioManager.Instance?.SetCombatIntensity(_intensity);

                string desired = _intensity > 0.08f ? "Combat" : "Explore";
                if (_current != null && desired != _state) SetState(desired);
            }
        }

        // ── Zone music ────────────────────────────────────────────────────────
        private void OnRoomEntered(RoomTransitionCompleteEvent e)
        {
            if (!_byZone.TryGetValue(e.Zone, out var zone) || zone == null) return;
            if (zone == _current) return; // same zone, keep playing
            _current = zone;

            var am = AudioManager.Instance;
            if (am == null) return;

            if (!_bossActive)
            {
                am.PlayExploration(zone.ExplorationTrack, zone.CrossfadeTime);
                am.SetCombatLayer(zone.CombatLayer, zone.CombatLayerMax);
                SetState(_intensity > 0.08f ? "Combat" : "Explore");
            }

            am.PlayAmbient(zone.AmbientBed, zone.AmbientVolume, zone.CrossfadeTime);
            am.SetReverb(zone.Reverb, zone.ReverbWet);
            am.TransitionToSnapshot(zone.Snapshot, zone.CrossfadeTime);

            EventBus.Emit(new MusicZoneChangedEvent
            {
                Zone      = zone.Zone,
                TrackName = zone.ExplorationTrack != null ? zone.ExplorationTrack.name : zone.DisplayName,
            });
        }

        // ── Boss override ─────────────────────────────────────────────────────
        private void OnBossStarted(BossStartedEvent _)
        {
            _bossActive = true;
            var am = AudioManager.Instance;
            if (am == null || _current == null) return;

            am.DuckMusic(0.2f, 0.5f);          // brief drop for the boss sting
            am.PlayBoss(_current.BossTrack, 1.0f);
            SetState("Boss");
        }

        private void OnBossDefeated(BossDefeatedEvent _)
        {
            _bossActive = false;
            var am = AudioManager.Instance;
            if (am == null || _current == null) return;

            am.PlayExploration(_current.ExplorationTrack, _current.CrossfadeTime);
            am.SetCombatLayer(_current.CombatLayer, _current.CombatLayerMax);
            _intensity = 0f;
            SetState("Explore");
        }

        // ── Combat-intensity feed ─────────────────────────────────────────────
        private void OnPlayerAttack(PlayerAttackEvent _)  => Bump(_attackBump);
        private void OnPlayerDamaged(PlayerDamagedEvent e) { if (e.Damage > 0) Bump(_hitBump); }
        private void OnEnemyDied(EnemyDiedEvent _)         => Bump(_killBump);

        private void Bump(float amount)
        {
            if (_bossActive) return;
            _intensity = Mathf.Clamp01(_intensity + amount);
            EventBus.Emit(new CombatIntensityEvent { Intensity = _intensity });
        }

        private void OnPlayerDied(PlayerDiedEvent _)
        {
            AudioManager.Instance?.FadeOutAll(1.5f);
            _bossActive = false;
            _intensity  = 0f;
            SetState("Silent");
        }

        private void SetState(string state)
        {
            if (_state == state) return;
            _state = state;
            EventBus.Emit(new MusicStateChangedEvent { State = state });
        }

        // Exposed for debug / HUD readouts.
        public float CurrentIntensity => _intensity;
        public string CurrentState    => _state;
    }
}
