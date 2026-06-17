using UnityEngine;
using MidnightReturn.Systems;
using MidnightReturn.Utils;

namespace MidnightReturn.Player
{
    // Foley layer — drives footsteps and movement SFX from FSM state + grounding.
    // Reads the PlayerController FSM each frame; emits no events, just plays sound.
    // Footstep cadence mirrors the run-dust interval for visual/audio sync.
    [RequireComponent(typeof(PlayerController))]
    public class PlayerAudio : MonoBehaviour
    {
        [Header("Footsteps")]
        [SerializeField] private AudioClip[] _footsteps;       // round-robin / random
        [SerializeField] private float _stepInterval = 0.30f;
        [SerializeField, Range(0f, 1f)] private float _stepVolume = 0.5f;

        [Header("Movement")]
        [SerializeField] private AudioClip _jump;
        [SerializeField] private AudioClip _land;
        [SerializeField] private AudioClip _dash;
        [SerializeField] private AudioClip _wallSlideLoop;     // looped while clinging
        [SerializeField] private AudioClip _hurt;

        private PlayerController _player;
        private AudioSource      _slideSource;                  // dedicated loop source

        private float  _stepTimer;
        private bool   _wasGrounded = true;
        private string _lastState = "";

        private void Awake()
        {
            _player = GetComponent<PlayerController>();

            // Dedicated 3D source for the wall-slide scrape loop
            var go = new GameObject("WallSlideLoop");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            _slideSource = go.AddComponent<AudioSource>();
            _slideSource.loop = true;
            _slideSource.playOnAwake = false;
            _slideSource.spatialBlend = 0.6f;
            _slideSource.clip = _wallSlideLoop;
        }

        private void OnEnable()  => EventBus.Subscribe<PlayerDamagedEvent>(OnDamaged);
        private void OnDisable() => EventBus.Unsubscribe<PlayerDamagedEvent>(OnDamaged);

        private void Update()
        {
            if (_player.FSM == null) return;
            string state    = _player.FSM.CurrentState;
            bool   grounded = _player.Movement.IsGrounded;

            // ── Landing ───────────────────────────────────────────────────────
            if (grounded && !_wasGrounded) PlayAt(_land, 0.6f);
            _wasGrounded = grounded;

            // ── State-entry one-shots ─────────────────────────────────────────
            if (state != _lastState)
            {
                switch (state)
                {
                    case "Jump": PlayAt(_jump, 0.6f); break;
                    case "Dash": PlayAt(_dash, 0.7f); break;
                }
                HandleWallSlide(state);
                _lastState = state;
            }

            // ── Footstep cadence ──────────────────────────────────────────────
            if (state == "Run" && grounded)
            {
                _stepTimer += Time.deltaTime;
                if (_stepTimer >= _stepInterval)
                {
                    _stepTimer = 0f;
                    PlayFootstep();
                }
            }
            else
            {
                _stepTimer = _stepInterval; // next run step fires immediately
            }
        }

        private void HandleWallSlide(string state)
        {
            if (state == "WallSlide")
            {
                if (_wallSlideLoop != null && !_slideSource.isPlaying) _slideSource.Play();
            }
            else if (_slideSource.isPlaying)
            {
                _slideSource.Stop();
            }
        }

        private void PlayFootstep()
        {
            if (_footsteps == null || _footsteps.Length == 0) return;
            var clip = _footsteps[Random.Range(0, _footsteps.Length)];
            AudioManager.Instance?.PlaySFX(clip, transform.position, _stepVolume);
        }

        private void PlayAt(AudioClip clip, float vol)
        {
            if (clip == null) return;
            AudioManager.Instance?.PlaySFX(clip, transform.position, vol);
        }

        private void OnDamaged(PlayerDamagedEvent e)
        {
            if (e.Damage > 0) PlayAt(_hurt, 0.8f);
        }
    }
}
