using System.Collections;
using UnityEngine;
using UnityEngine.VFX;
using MidnightReturn.Player;
using MidnightReturn.Utils;

namespace MidnightReturn.Map
{
    // ══════════════════════════════════════════════════════════════════
    //  SaveStatue
    //  Player presses Interact (E / Y-button) within _interactRadius.
    //  Saves game, restores HP + half MP, pulses glow VFX.
    // ══════════════════════════════════════════════════════════════════
    public sealed class SaveStatue : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] string _statueId;   // unique per room

        [Header("Interaction")]
        [SerializeField] float _interactRadius = 1.8f;

        [Header("VFX / Audio")]
        [SerializeField] VisualEffect _idleGlow;     // assigned in prefab
        [SerializeField] VisualEffect _activateVFX;  // burst on save
        [SerializeField] AudioClip    _saveSound;
        [SerializeField] Light        _glowLight;    // HDRP point light

        static readonly int s_GlowColor    = Shader.PropertyToID("_EmissiveColor");
        static readonly int s_GlowIntensity = Shader.PropertyToID("_EmissiveIntensity");

        readonly MaterialPropertyBlock _mpb = new();
        Renderer      _renderer;
        bool          _activated;
        bool          _playerNear;
        Transform     _player;
        PlayerController _playerCtrl;

        void Awake()
        {
            _renderer = GetComponentInChildren<Renderer>();
        }

        void Start()
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go)
            {
                _player     = go.transform;
                _playerCtrl = go.GetComponent<PlayerController>();
            }

            // Idle pulsing glow
            StartCoroutine(IdleGlowPulse());
        }

        void Update()
        {
            if (_player == null) return;

            float dist = Vector2.Distance(
                new Vector2(transform.position.x, transform.position.y),
                new Vector2(_player.position.x,   _player.position.y));

            _playerNear = dist <= _interactRadius;

            // Listen for Interact input
            if (_playerNear && !_activated)
            {
                var input = _player.GetComponent<PlayerInputHandler>();
                if (input != null && input.ConsumeInteract())
                    StartCoroutine(ActivateSequence());
            }
        }

        IEnumerator ActivateSequence()
        {
            _activated = true;

            // Sound
            if (_saveSound)
                AudioManager.Instance?.PlaySFX(_saveSound, transform.position);

            // VFX burst
            if (_activateVFX) _activateVFX.Play();

            // Light spike
            if (_glowLight) StartCoroutine(LightSpike());

            // Restore HP + half MP
            _playerCtrl?.RestoreAtStatue();

            // Save
            var gm  = Core.GameManager.Instance;
            if (gm != null)
            {
                gm.Save.CurrentRoom       = RoomManager.Instance?.CurrentRoomId ?? "";
                gm.Save.LastSavePosition  = _player.position;
                gm.Save.LastSaveStatueId  = _statueId;
                gm.SaveGame(0);  // default slot; full slot UI in Phase 5
            }

            EventBus.Emit(new SaveStatueActivatedEvent { StatueId = _statueId });

            // HUD notification
            EventBus.Emit(new ScreenFlashEvent { Color = new Color(0.1f, 0.3f, 0.6f), Duration = 0.3f });

            yield return new WaitForSeconds(0.5f);

            // Statue stays lit after first save
            SetEmissive(new Color(0.2f, 0.4f, 1f) * 4f);
        }

        // Idle: slow blue-white pulse when unactivated; warm gold pulse when activated
        IEnumerator IdleGlowPulse()
        {
            while (true)
            {
                float t   = Mathf.PingPong(Time.time * 0.8f, 1f);
                Color col = _activated
                    ? Color.Lerp(new Color(0.8f, 0.6f, 0.1f), new Color(1f, 0.9f, 0.3f), t) * 3f
                    : Color.Lerp(new Color(0.05f, 0.1f, 0.3f), new Color(0.15f, 0.3f, 0.8f), t) * 2f;
                SetEmissive(col);
                if (_glowLight) _glowLight.color = col;
                yield return null;
            }
        }

        IEnumerator LightSpike()
        {
            float base_i = _glowLight.intensity;
            _glowLight.intensity = base_i * 8f;
            yield return new WaitForSeconds(0.12f);
            float t = 0f;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                _glowLight.intensity = Mathf.Lerp(base_i * 8f, base_i * 1.4f, t / 0.5f);
                yield return null;
            }
            _glowLight.intensity = base_i * 1.4f; // stays slightly brighter after activation
        }

        void SetEmissive(Color col)
        {
            if (!_renderer) return;
            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetColor(s_GlowColor, col);
            _renderer.SetPropertyBlock(_mpb);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.5f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, _interactRadius);
        }
    }
}
