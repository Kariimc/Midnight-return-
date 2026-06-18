using UnityEngine;
using MidnightReturn.Player;

namespace MidnightReturn.VerticalSlice
{
    // Tags a wall segment (BoxCollider, 1-tile deep) as passable via Wraith Step.
    // The wall remains physically solid against all other objects at all times —
    // only the WraithStepState explicitly disables CC detectCollisions to pass through.
    //
    // Renders a faint purple shimmer at runtime via a looping MaterialPropertyBlock
    // emissive pulse, so exploration-minded players can spot phaseable sections.
    [RequireComponent(typeof(BoxCollider))]
    public sealed class PhaseableWall : MonoBehaviour
    {
        [SerializeField] private Color _shimmerColor = new(0.45f, 0.1f, 1f, 0.35f);

        private MeshRenderer _renderer;
        private MaterialPropertyBlock _mpb;
        private float _pulse;

        private void Start()
        {
            _renderer = GetComponentInChildren<MeshRenderer>();
            _mpb      = new MaterialPropertyBlock();
        }

        private void Update()
        {
            if (_renderer == null) return;
            _pulse += Time.deltaTime * 1.8f;
            float t = (Mathf.Sin(_pulse) * 0.5f + 0.5f);

            _renderer.GetPropertyBlock(_mpb);
            var emissive = _shimmerColor * (0.3f + t * 0.7f);
            _mpb.SetColor("_EmissiveColor", emissive);
            _renderer.SetPropertyBlock(_mpb);
        }

        private void OnDrawGizmos()
        {
            var col = GetComponent<BoxCollider>();
            if (col == null) return;
            Gizmos.color = new Color(0.5f, 0.1f, 1f, 0.4f);
            Gizmos.DrawWireCube(transform.position + col.center, col.size);
        }
    }
}
