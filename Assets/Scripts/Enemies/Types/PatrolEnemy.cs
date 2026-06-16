using UnityEngine;
using MidnightReturn.Utils;

namespace MidnightReturn.Enemies.Types
{
    // Covers: Zombie, Skeleton, Axe Knight, Merman, Dark Knight
    public class PatrolEnemy : EnemyBase
    {
        [Header("Patrol")]
        [SerializeField] private float _patrolRange   = 4f;
        [SerializeField] private float _pauseAtEdge   = 0.8f;

        private Vector3 _origin;
        private int     _patrolDir = 1;
        private float   _pauseTimer;

        private StateMachine<PatrolEnemy> _fsm;

        protected override void Awake()
        {
            base.Awake();
            _origin = transform.position;
            BuildFSM();
        }

        private void BuildFSM()
        {
            _fsm = new StateMachine<PatrolEnemy>();
            // Simple patrol ↔ chase ↔ attack cycle managed in OnUpdate
        }

        protected override void OnUpdate(float dt)
        {
            if (CanSeePlayer)
            {
                ChasePlayer(dt);
                if (InAttackRange) TryAttack();
            }
            else
            {
                Patrol(dt);
            }

            // Face direction
            float scaleX = (_patrolDir > 0) ? 1f : -1f;
            if (CanSeePlayer)
                scaleX = (PlayerPos.x > transform.position.x) ? 1f : -1f;
            var scale = transform.localScale;
            scale.x   = scaleX;
            transform.localScale = scale;
        }

        private void Patrol(float dt)
        {
            if (_pauseTimer > 0f) { _pauseTimer -= dt; return; }

            MoveToward(transform.position + Vector3.right * _patrolDir, Data.MoveSpeed, dt);

            float dist = Mathf.Abs(transform.position.x - _origin.x);
            if (dist >= _patrolRange)
            {
                _patrolDir  = -_patrolDir;
                _pauseTimer = _pauseAtEdge;
                _anim?.CrossFadeInFixedTime("Idle", 0.1f);
            }
            else
            {
                _anim?.CrossFadeInFixedTime("Walk", 0.1f);
            }
        }

        private void ChasePlayer(float dt)
        {
            _anim?.CrossFadeInFixedTime("Run", 0.1f);
            MoveToward(PlayerPos, Data.MoveSpeed * 1.4f, dt);
        }

        private void TryAttack()
        {
            if (_attackCooldown > 0f) return;
            _attackCooldown = Data.AttackCooldown;
            _anim?.CrossFadeInFixedTime("Attack", 0.08f);
            // Hit check after windup
            StartCoroutine(AttackHit());
        }

        private System.Collections.IEnumerator AttackHit()
        {
            yield return new UnityEngine.WaitForSeconds(Data.AttackWindup);
            if (!IsAlive) yield break;
            if (InAttackRange)
            {
                var player = _player?.GetComponent<Player.PlayerController>();
                player?.TakeDamage(Data.Attack, Data.DamageType.Physical);
            }
        }
    }
}
