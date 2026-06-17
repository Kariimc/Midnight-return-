using UnityEngine;
using MidnightReturn.Systems.Rendering;

namespace MidnightReturn.Enemies
{
    // Drives SpriteAnimator from EnemyBase state flags.
    // Add to the sprite quad child of any enemy prefab.
    // Works for Patrol, Flying, and Ranged enemy types — each has its own clip set.
    [RequireComponent(typeof(SpriteAnimator))]
    public class EnemySpriteController : MonoBehaviour
    {
        public enum EnemySpriteType { Patrol, Flying, Ranged }

        [SerializeField] private EnemySpriteType _type;
        [SerializeField] private EnemyBase       _enemy;

        private SpriteAnimator _anim;
        private string         _lastClip;
        private Vector3        _baseScale;

        private void Awake()
        {
            _anim      = GetComponent<SpriteAnimator>();
            _baseScale = transform.localScale;

            if (_enemy == null)
                _enemy = GetComponentInParent<EnemyBase>();
        }

        private void LateUpdate()
        {
            if (_enemy == null) return;

            string clip = ResolveClip();
            if (clip != _lastClip)
            {
                _anim.Play(clip);
                _lastClip = clip;
            }

            // Flip to face movement direction
            float dir = _enemy.FacingDir;
            var s     = _baseScale;
            s.x       = Mathf.Abs(s.x) * dir;
            transform.localScale = s;
        }

        private string ResolveClip()
        {
            if (_enemy.IsDead)    return "Death";
            if (_enemy.IsAttacking) return "Attack";

            return _type switch
            {
                EnemySpriteType.Patrol  => ResolvePatrol(),
                EnemySpriteType.Flying  => ResolveFlying(),
                EnemySpriteType.Ranged  => ResolveRanged(),
                _                       => "Idle",
            };
        }

        private string ResolvePatrol()
        {
            return _enemy.IsMoving ? "Walk" : "Idle";
        }

        private string ResolveFlying()
        {
            return _enemy.IsChasing ? "Chase" : (_enemy.IsDiving ? "Dive" : "Fly");
        }

        private string ResolveRanged()
        {
            return _enemy.IsMoving ? "Walk" : "Idle";
        }
    }
}
