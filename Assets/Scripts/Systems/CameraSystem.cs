using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using MidnightReturn.Utils;

namespace MidnightReturn.Systems
{
    // Cinemachine-driven 2.5D camera with lookahead, boss zoom, impulse shake
    [RequireComponent(typeof(CinemachineCamera))]
    public class CameraSystem : MonoBehaviour
    {
        public static CameraSystem Instance { get; private set; }

        [Header("Cinemachine")]
        [SerializeField] private CinemachineCamera    _vcam;
        [SerializeField] private CinemachineImpulseSource _impulse;
        [SerializeField] private CinemachinePositionComposer _composer;

        [Header("2.5D Settings")]
        [SerializeField] private float _defaultOrthographicSize = 6f;
        [SerializeField] private float _bossOrthographicSize    = 9f;
        [SerializeField] private float _zoomSpeed               = 2f;

        [Header("Lookahead")]
        [SerializeField] private float _lookaheadX      = 1.8f;
        [SerializeField] private float _lookaheadSmooth = 0.3f;

        private float  _currentLookahead;
        private Player.PlayerController _player;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (_vcam == null)    _vcam    = GetComponent<CinemachineCamera>();
            if (_impulse == null) _impulse = GetComponent<CinemachineImpulseSource>();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<CameraShakeEvent>(OnShake);
            EventBus.Subscribe<BossStartedEvent>(OnBossStart);
            EventBus.Subscribe<BossDefeatedEvent>(OnBossEnd);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CameraShakeEvent>(OnShake);
            EventBus.Unsubscribe<BossStartedEvent>(OnBossStart);
            EventBus.Unsubscribe<BossDefeatedEvent>(OnBossEnd);
        }

        private void Start()
        {
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                _player = playerGO.GetComponent<Player.PlayerController>();
                _vcam.Target.TrackingTarget = playerGO.transform;
            }
        }

        private void Update()
        {
            if (_player == null) return;
            UpdateLookahead();
        }

        private void UpdateLookahead()
        {
            // Shift camera ahead of player movement direction
            float targetLookahead = _player.Movement.FacingDir * _lookaheadX;
            _currentLookahead     = Mathf.Lerp(_currentLookahead, targetLookahead, Time.deltaTime / _lookaheadSmooth);

            if (_composer != null)
            {
                var offset            = _composer.TargetOffset;
                offset.x              = _currentLookahead;
                _composer.TargetOffset = offset;
            }
        }

        // ── Event handlers ────────────────────────────────────────────────────
        private void OnShake(CameraShakeEvent e)
        {
            _impulse?.GenerateImpulse(Vector3.right * e.Intensity);
        }

        private void OnBossStart(BossStartedEvent e)
        {
            StartCoroutine(ZoomTo(_bossOrthographicSize, 1.5f));
            // Letterbox bars
        }

        private void OnBossEnd(BossDefeatedEvent e)
        {
            StartCoroutine(ZoomTo(_defaultOrthographicSize, 2f));
        }

        // ── Manual control ────────────────────────────────────────────────────
        public void Shake(float intensity, float duration = 0.15f)
        {
            _impulse?.GenerateImpulseWithDuration(Vector3.right * intensity, duration);
        }

        public void ZoomToBoss() => StartCoroutine(ZoomTo(_bossOrthographicSize, 1.5f));
        public void ZoomToDefault() => StartCoroutine(ZoomTo(_defaultOrthographicSize, 2f));

        private IEnumerator ZoomTo(float targetSize, float duration)
        {
            if (_vcam == null || _vcam.Lens.OrthographicSize == targetSize) yield break;
            float start = _vcam.Lens.OrthographicSize;
            float t     = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                var lens = _vcam.Lens;
                lens.OrthographicSize = Mathf.Lerp(start, targetSize, t / duration);
                _vcam.Lens = lens;
                yield return null;
            }
            var finalLens = _vcam.Lens;
            finalLens.OrthographicSize = targetSize;
            _vcam.Lens = finalLens;
        }
    }
}
