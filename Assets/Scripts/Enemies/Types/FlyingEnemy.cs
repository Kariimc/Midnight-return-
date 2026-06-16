using System.Collections;
using UnityEngine;

namespace MidnightReturn.Enemies.Types
{
    // Covers: Medusa Head, Ghost, Harpy, Vampire Bat, Gargoyle, Succubus
    public class FlyingEnemy : EnemyBase
    {
        [Header("Flight")]
        [SerializeField] private float _hoverAmplitude = 0.4f;
        [SerializeField] private float _hoverFrequency = 1.5f;
        [SerializeField] private bool  _diveBombs       = false;
        [SerializeField] private float _diveSpeed       = 18f;

        private enum FlyState { Idle, Patrol, Chase, Dive, Recover }
        private FlyState   _flyState = FlyState.Patrol;
        private Vector3    _patrolTarget;
        private float      _hoverPhase;
        private float      _diveRecoverTimer;

        protected override void Awake()
        {
            base.Awake();
            // Flying enemies ignore gravity
            Physics.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Ground"), true);
            _patrolTarget = transform.position + Random.insideUnitSphere * 4f;
            _patrolTarget.z = 0f;
        }

        protected override void OnUpdate(float dt)
        {
            _hoverPhase += dt * _hoverFrequency;
            float hover  = Mathf.Sin(_hoverPhase) * _hoverAmplitude;

            switch (_flyState)
            {
                case FlyState.Patrol:  DoPatrol(dt, hover);  break;
                case FlyState.Chase:   DoChase(dt, hover);   break;
                case FlyState.Dive:    DoDive(dt);            break;
                case FlyState.Recover: DoRecover(dt);         break;
            }

            if (CanSeePlayer && _flyState == FlyState.Patrol)
                _flyState = _diveBombs ? FlyState.Dive : FlyState.Chase;
        }

        private void DoPatrol(float dt, float hover)
        {
            _anim?.CrossFadeInFixedTime("Fly", 0.15f);
            var target = _patrolTarget + Vector3.up * hover;
            transform.position = Vector3.MoveTowards(transform.position, target, Data.MoveSpeed * dt);
            transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
            if (Vector3.Distance(transform.position, _patrolTarget) < 0.3f)
            {
                _patrolTarget = transform.position + Random.insideUnitSphere * 5f;
                _patrolTarget.z = 0f;
            }
        }

        private void DoChase(float dt, float hover)
        {
            _anim?.CrossFadeInFixedTime("Chase", 0.1f);
            var target = PlayerPos + Vector3.up * 0.5f + Vector3.up * hover;
            transform.position = Vector3.MoveTowards(transform.position, target, Data.MoveSpeed * 1.3f * dt);
            transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
            if (InAttackRange) TryContactAttack();
        }

        private void DoDive(float dt)
        {
            // Rise above player first, then dive
            var abovePlayer = PlayerPos + Vector3.up * 4f;
            if (Vector3.Distance(transform.position, abovePlayer) > 0.5f)
            {
                transform.position = Vector3.MoveTowards(transform.position, abovePlayer, Data.MoveSpeed * dt);
                transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
            }
            else
            {
                // Dive!
                _anim?.CrossFadeInFixedTime("Dive", 0.05f);
                transform.position = Vector3.MoveTowards(transform.position, PlayerPos, _diveSpeed * dt);
                transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
                if (InAttackRange)
                {
                    TryContactAttack();
                    _flyState         = FlyState.Recover;
                    _diveRecoverTimer = 1.2f;
                }
            }
        }

        private void DoRecover(float dt)
        {
            _diveRecoverTimer -= dt;
            var recoverPos = transform.position + Vector3.up * 3f;
            transform.position = Vector3.MoveTowards(transform.position, recoverPos, Data.MoveSpeed * dt);
            transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
            if (_diveRecoverTimer <= 0f) _flyState = FlyState.Chase;
        }

        private void TryContactAttack()
        {
            if (_attackCooldown > 0f) return;
            _attackCooldown = Data.AttackCooldown;
            var player = _player?.GetComponent<Player.PlayerController>();
            player?.TakeDamage(Data.Attack, Data.DamageType.Physical);
        }
    }
}
