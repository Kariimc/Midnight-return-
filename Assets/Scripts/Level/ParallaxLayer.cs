using UnityEngine;

namespace MidnightReturn.Level
{
    // Self-driving parallax layer. Shifts by a fraction of total camera travel:
    // 0 = locked to the world (gameplay plane), 1 = locked to the camera
    // (infinitely distant). Optional auto-scroll for drifting fog/cloud bands and
    // infinite horizontal wrapping for seamless backgrounds.
    public class ParallaxLayer : MonoBehaviour
    {
        [Header("Parallax Factor (0=world, 1=camera-locked)")]
        [Range(0f, 1f)] public float FactorX = 0.5f;
        [Range(0f, 1f)] public float FactorY = 0.2f;

        [Header("Auto-scroll (world units/sec)")]
        public float ScrollX = 0f;
        public float ScrollY = 0f;

        [Header("Infinite tiling")]
        [Tooltip("Width of one tile span; layer wraps when it drifts past half a span. 0 = no wrap.")]
        public float TileSpan = 0f;

        private Transform _cam;
        private Vector3   _startPos;     // layer position at spawn
        private Vector3   _camStart;     // camera position at spawn
        private float     _scrollAccum;
        private bool      _ready;

        private void Start() => TryBind();

        private void TryBind()
        {
            if (Camera.main == null) return;
            _cam      = Camera.main.transform;
            _startPos = transform.position;
            _camStart = _cam.position;
            _ready    = true;
        }

        private void LateUpdate()
        {
            if (!_ready) { TryBind(); return; }

            Vector3 cam = _cam.position;
            float travelX = cam.x - _camStart.x;
            float travelY = cam.y - _camStart.y;

            _scrollAccum += Time.deltaTime;

            float px = _startPos.x + travelX * FactorX + ScrollX * _scrollAccum;
            float py = _startPos.y + travelY * FactorY + ScrollY * _scrollAccum;

            // Seamless horizontal wrap relative to the camera.
            if (TileSpan > 0f)
            {
                float rel = px - cam.x;
                while (rel >  TileSpan * 0.5f) { px -= TileSpan; rel -= TileSpan; }
                while (rel < -TileSpan * 0.5f) { px += TileSpan; rel += TileSpan; }
            }

            transform.position = new Vector3(px, py, transform.position.z);
        }
    }
}
