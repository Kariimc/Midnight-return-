using UnityEngine;

namespace MidnightReturn.VerticalSlice
{
    // Marks a ceiling / structural point as a valid Soul Tether latch target.
    // Add to an invisible trigger sphere at any architectural anchor in the level
    // (stone bosses, ceiling hooks, chandelier chains, vault keystones, etc.).
    // Rendered as a faint glow sphere in scene view; invisible in game by default.
    public sealed class TetherAnchor : MonoBehaviour
    {
        [SerializeField] private float _glowRadius = 0.35f;

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.5f, 0.15f, 1f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, _glowRadius);
            Gizmos.color = new Color(0.5f, 0.15f, 1f, 0.12f);
            Gizmos.DrawSphere(transform.position, _glowRadius);
        }
    }
}
