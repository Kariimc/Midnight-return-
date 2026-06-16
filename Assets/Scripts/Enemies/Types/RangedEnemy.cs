using System.Collections;
using UnityEngine;
using MidnightReturn.Systems;
using MidnightReturn.Utils;

namespace MidnightReturn.Enemies.Types
{
    // Covers: Bone Archer, Sorcerer
    public class RangedEnemy : EnemyBase
    {
        [Header("Ranged")]
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private int        _projectilesPerBurst = 1;
        [SerializeField] private float      _projectileSpeed     = 9f;
        [SerializeField] private float      _preferredRange      = 5f;
        [SerializeField] private float      _retreatSpeed        = 2.5f;

        protected override void OnUpdate(float dt)
        {
            if (!CanSeePlayer) { IdleWander(dt); return; }

            float dist = DistToPlayer;

            // Maintain preferred range — retreat if player closes in
            if (dist < _preferredRange * 0.5f)
            {
                Retreat(dt);
            }
            else if (dist > _preferredRange * 2f)
            {
                Approach(dt);
            }
            else
            {
                // Stand still and fire
                _anim?.CrossFadeInFixedTime("Idle", 0.2f);
                TryFire();
            }

            // Face player
            float scaleX = PlayerPos.x > transform.position.x ? 1f : -1f;
            var scale = transform.localScale;
            scale.x   = scaleX;
            transform.localScale = scale;
        }

        private void IdleWander(float dt)
        {
            _anim?.CrossFadeInFixedTime("Idle", 0.2f);
        }

        private void Retreat(float dt)
        {
            _anim?.CrossFadeInFixedTime("Walk", 0.1f);
            var dir = (transform.position - PlayerPos).normalized;
            MoveToward(transform.position + dir, _retreatSpeed, dt);
        }

        private void Approach(float dt)
        {
            _anim?.CrossFadeInFixedTime("Walk", 0.1f);
            MoveToward(PlayerPos, Data.MoveSpeed * 0.7f, dt);
        }

        private void TryFire()
        {
            if (_attackCooldown > 0f) return;
            _attackCooldown = Data.AttackCooldown;
            _anim?.CrossFadeInFixedTime("Attack", 0.08f);
            StartCoroutine(FireBurst());
        }

        private IEnumerator FireBurst()
        {
            yield return new WaitForSeconds(Data.AttackWindup);
            if (!IsAlive) yield break;

            for (int i = 0; i < _projectilesPerBurst; i++)
            {
                SpawnProjectile(i);
                if (_projectilesPerBurst > 1) yield return new WaitForSeconds(0.12f);
            }
        }

        private void SpawnProjectile(int index)
        {
            if (_projectilePrefab == null) return;
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            Vector3 dir    = (PlayerPos - origin).normalized;

            // Multi-shot spread
            if (_projectilesPerBurst > 1)
            {
                float spread = (_projectilesPerBurst == 3) ? 15f : 10f;
                float angle  = (index - (_projectilesPerBurst - 1) * 0.5f) * spread;
                dir = Quaternion.Euler(0f, 0f, angle) * dir;
            }
            dir.z = 0f;

            var proj = Instantiate(_projectilePrefab, origin, Quaternion.identity);
            if (proj.TryGetComponent<Projectile>(out var p))
            {
                p.Init(dir * _projectileSpeed, Data.Attack, Data.DamageType.Physical);
            }

            AudioManager.Instance?.Play(Data.AttackSound);
        }
    }
}
