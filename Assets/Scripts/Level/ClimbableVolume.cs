using UnityEngine;
using MidnightReturn.Player;

namespace MidnightReturn.Level
{
    // Trigger volume that marks a climbable ladder/chain. While the player overlaps
    // it, PlayerMovement.CanClimb is true and the ladder's centerline + vertical
    // bounds are supplied for snapping/clamping. Put a trigger BoxCollider on the
    // same object sized to the ladder.
    [RequireComponent(typeof(BoxCollider))]
    public class ClimbableVolume : MonoBehaviour
    {
        private BoxCollider _box;

        private void Awake()
        {
            _box = GetComponent<BoxCollider>();
            _box.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent<PlayerMovement>(out var move)) return;
            ApplyBounds(move, true);
        }

        private void OnTriggerStay(Collider other)
        {
            // Refresh bounds each frame in case the volume moves (moving chains).
            if (other.TryGetComponent<PlayerMovement>(out var move))
                ApplyBounds(move, true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<PlayerMovement>(out var move))
                move.SetClimbable(false, 0f, 0f, 0f);
        }

        private void ApplyBounds(PlayerMovement move, bool can)
        {
            Vector3 c   = transform.TransformPoint(_box.center);
            float   halfH = _box.size.y * 0.5f * transform.lossyScale.y;
            move.SetClimbable(can, c.x, c.y + halfH, c.y - halfH);
        }
    }
}
