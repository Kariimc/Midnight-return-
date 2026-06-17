using UnityEngine;

namespace MidnightReturn.Systems.Rendering
{
    // Keeps the sprite quad facing the camera while preserving 2.5D Y-up orientation.
    // Lock Y rotation so the sprite only pivots on Y-axis to face cam — maintains
    // the 2.5D illusion without tilting up/down.
    public class SpriteBillboard : MonoBehaviour
    {
        [SerializeField] private bool _lockY = true; // keep upright for 2.5D

        private Transform _cam;

        private void Start()
        {
            _cam = Camera.main?.transform;
        }

        private void LateUpdate()
        {
            if (_cam == null) { _cam = Camera.main?.transform; return; }

            Vector3 dir = transform.position - _cam.position;

            if (_lockY)
            {
                dir.y = 0f;
                if (dir.sqrMagnitude < 0.001f) return;
                transform.rotation = Quaternion.LookRotation(dir);
            }
            else
            {
                if (dir.sqrMagnitude < 0.001f) return;
                transform.rotation = Quaternion.LookRotation(dir);
            }
        }
    }
}
