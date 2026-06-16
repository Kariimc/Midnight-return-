using UnityEngine;

namespace MidnightReturn.Map
{
    // Place on a BoxCollider (trigger) at each room exit.
    // Set TargetRoomId + Direction in Inspector.
    [RequireComponent(typeof(Collider))]
    public sealed class DoorTrigger : MonoBehaviour
    {
        [Header("Connection")]
        [SerializeField] string        _targetRoomId;
        [SerializeField] DoorDirection _directionFromThisRoom = DoorDirection.East;

        [Header("Visual")]
        [SerializeField] GameObject _lockedVisual;   // grate / portcullis mesh (optional)
        [SerializeField] GameObject _openVisual;     // open doorway mesh  (optional)

        bool _playerInside;

        void Start() => RefreshVisual();

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInside = true;
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInside = false;
        }

        void Update()
        {
            // Trigger on sustained contact (player walks through, not just touches)
            if (!_playerInside) return;
            if (RoomManager.Instance == null || RoomManager.Instance.IsTransitioning) return;
            RoomManager.Instance.RequestTransition(_targetRoomId, _directionFromThisRoom);
        }

        // Called by RoomManager or key pickup when a door is unlocked
        public void Unlock()
        {
            RefreshVisual();
        }

        void RefreshVisual()
        {
            // Determine lock state from save data
            var save = Core.GameManager.Instance?.Save;
            var room = RoomManager.Instance?.CurrentRoom;
            bool locked = false;

            if (room != null && save != null)
            {
                foreach (var c in room.Connections)
                {
                    if (c.TargetRoomId != _targetRoomId) continue;
                    locked = c.DoorType == DoorType.Locked
                          && !save.UnlockedDoors.Contains(c.UnlockItemId);
                    break;
                }
            }

            if (_lockedVisual) _lockedVisual.SetActive(locked);
            if (_openVisual)   _openVisual.SetActive(!locked);
        }
    }
}
