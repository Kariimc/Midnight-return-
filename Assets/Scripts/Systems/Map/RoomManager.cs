using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using MidnightReturn.Core;
using MidnightReturn.Utils;
using MidnightReturn.Player;

namespace MidnightReturn.Map
{
    // ══════════════════════════════════════════════════════════════════
    //  RoomManager  — singleton that owns room loading/unloading,
    //  player repositioning, and transition fade.
    //
    //  Scene setup:
    //    "Persistent" scene → GameManager, RoomManager, Player, HUD, Camera
    //    "Room_*" scenes   → geometry, enemies, triggers (additive)
    //
    //  Flow:  DoorTrigger → RequestTransition → fade → unload old →
    //         load new (async) → reposition player → fade → notify
    // ══════════════════════════════════════════════════════════════════
    public sealed class RoomManager : MonoBehaviour
    {
        public static RoomManager Instance { get; private set; }

        [Header("Graph")]
        [SerializeField] RoomGraph _graph;

        [Header("Transition")]
        [SerializeField] float _fadeDuration   = 0.35f;
        [SerializeField] CanvasGroup _fadePanel;          // full-screen black CanvasGroup

        public string      CurrentRoomId { get; private set; }
        public RoomDataSO  CurrentRoom   => _graph?.GetRoom(CurrentRoomId);
        public bool        IsTransitioning { get; private set; }

        Transform   _playerTransform;
        Coroutine   _transitionRoutine;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _graph?.Init();
        }

        void Start()
        {
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO) _playerTransform = playerGO.transform;

            // Boot into the saved or start room
            string startId = GameManager.Instance?.Save?.CurrentRoom
                          ?? _graph?.StartRoomId
                          ?? "entrance_hall_01";
            StartCoroutine(LoadRoomImmediate(startId));
        }

        // Called by DoorTrigger
        public void RequestTransition(string targetRoomId, DoorDirection fromDirection)
        {
            if (IsTransitioning) return;

            var save = GameManager.Instance?.Save;
            var room = CurrentRoom;
            if (room == null) return;

            // Find the connection in the current room that leads to target
            RoomConnection? conn = null;
            foreach (var c in room.Connections)
                if (c.TargetRoomId == targetRoomId) { conn = c; break; }
            if (conn == null) return;

            // Lock check
            if (save != null && !_graph.CanPass(conn.Value, save))
            {
                EventBus.Emit(new RoomTransitionBlockedEvent
                {
                    RequiredItemId = conn.Value.UnlockItemId,
                    DoorType       = conn.Value.DoorType,
                });
                return;
            }

            _transitionRoutine = StartCoroutine(TransitionRoutine(targetRoomId, fromDirection));
        }

        IEnumerator TransitionRoutine(string targetId, DoorDirection fromDir)
        {
            IsTransitioning = true;

            // Disable player input during transition
            var input = _playerTransform?.GetComponent<PlayerInputHandler>();
            if (input) input.enabled = false;

            EventBus.Emit(new RoomTransitionStartedEvent { FromRoomId = CurrentRoomId, ToRoomId = targetId });

            // Fade out
            yield return FadeTo(1f);

            // Unload current room
            if (!string.IsNullOrEmpty(CurrentRoomId))
            {
                var unload = SceneManager.UnloadSceneAsync(
                    _graph.GetRoom(CurrentRoomId)?.SceneName ?? CurrentRoomId);
                yield return unload;
            }

            // Load new room
            var targetData = _graph.GetRoom(targetId);
            if (targetData == null) { Debug.LogError($"RoomManager: unknown room '{targetId}'"); yield break; }

            var load = SceneManager.LoadSceneAsync(targetData.SceneName, LoadSceneMode.Additive);
            load.allowSceneActivation = true;
            yield return load;

            // Reposition player to the spawn point that matches our arrival direction
            var arrival = RoomDataSO.Opposite(fromDir);
            var spawnConn = targetData.GetConnection(arrival);
            if (spawnConn.HasValue && _playerTransform != null)
            {
                _playerTransform.position = spawnConn.Value.SpawnWorldPos;
                _playerTransform.position = new Vector3(
                    _playerTransform.position.x, _playerTransform.position.y, 0f);
            }

            // Update state
            CurrentRoomId = targetId;
            if (GameManager.Instance != null)
                GameManager.Instance.MarkRoomVisited(targetId);

            // Apply ambient settings (lighting, music)
            ApplyRoomAmbience(targetData);

            // Fade in
            yield return FadeTo(0f);

            if (input) input.enabled = true;
            IsTransitioning = false;

            EventBus.Emit(new RoomTransitionCompleteEvent
            {
                RoomId      = CurrentRoomId,
                DisplayName = targetData.DisplayName,
                Zone        = targetData.Zone,
            });
        }

        // Used at game start — no fade, no unload
        IEnumerator LoadRoomImmediate(string roomId)
        {
            IsTransitioning = true;
            if (_fadePanel) _fadePanel.alpha = 1f;

            var data = _graph?.GetRoom(roomId);
            if (data == null) { IsTransitioning = false; yield break; }

            var load = SceneManager.LoadSceneAsync(data.SceneName, LoadSceneMode.Additive);
            yield return load;

            CurrentRoomId = roomId;
            GameManager.Instance?.MarkRoomVisited(roomId);
            ApplyRoomAmbience(data);

            yield return FadeTo(0f);
            IsTransitioning = false;

            EventBus.Emit(new RoomTransitionCompleteEvent
            {
                RoomId      = CurrentRoomId,
                DisplayName = data.DisplayName,
                Zone        = data.Zone,
            });
        }

        void ApplyRoomAmbience(RoomDataSO data)
        {
            if (data.MusicTrack != null)
                AudioManager.Instance?.CrossFade(data.MusicTrack);
            RenderSettings.ambientLight = data.AmbientLightColor;
        }

        IEnumerator FadeTo(float target)
        {
            if (!_fadePanel) yield break;
            float start = _fadePanel.alpha;
            float t     = 0f;
            while (t < _fadeDuration)
            {
                t              += Time.unscaledDeltaTime;
                _fadePanel.alpha = Mathf.Lerp(start, target, t / _fadeDuration);
                yield return null;
            }
            _fadePanel.alpha = target;
        }
    }
}
