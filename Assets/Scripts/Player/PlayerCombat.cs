using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidnightReturn.Data;
using MidnightReturn.Systems;
using MidnightReturn.Utils;

namespace MidnightReturn.Player
{
    // Manages attack hitboxes, weapon swaps, special moves
    public class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private LayerMask _enemyLayer;
        [SerializeField] private Transform _hitboxOrigin;
        [SerializeField] private WeaponDataSO _startingWeapon;

        public WeaponDataSO EquippedWeapon { get; private set; }

        private PlayerController  _player;
        private float             _pendingHitTime;
        private int               _pendingComboIdx;
        private bool              _hitboxQueued;
        private Coroutine         _hitboxCoroutine;

        private readonly Collider[] _hitResults = new Collider[16]; // no-alloc overlap check

        private void Awake()
        {
            _player = GetComponent<PlayerController>();
            EquippedWeapon = _startingWeapon;
        }

        public void QueueAttackHitbox(float normalizedDelay, int comboIndex)
        {
            if (_hitboxCoroutine != null) StopCoroutine(_hitboxCoroutine);
            _hitboxCoroutine = StartCoroutine(DelayedHitbox(normalizedDelay, comboIndex));
        }

        private IEnumerator DelayedHitbox(float delay, int combo)
        {
            // Wait until animation reaches hit frame
            if (delay > 0f) yield return new WaitForSeconds(delay * EquippedWeapon.SwingDuration);

            if (EquippedWeapon == null) yield break;

            Vector3 origin = _hitboxOrigin != null
                ? _hitboxOrigin.position
                : transform.position + Vector3.right * _player.Movement.FacingDir * 0.8f;

            Vector3 halfExtents = new(
                EquippedWeapon.HitboxSize.x * 0.5f,
                EquippedWeapon.HitboxSize.y * 0.5f,
                0.5f
            );

            int count = Physics.OverlapBoxNonAlloc(origin, halfExtents, _hitResults,
                Quaternion.identity, _enemyLayer);

            bool hitSomething = false;
            for (int i = 0; i < count; i++)
            {
                if (_hitResults[i].TryGetComponent<Enemies.EnemyBase>(out var enemy))
                {
                    var (dmg, isCrit) = PlayerStatsSystem.CalcDamage(_player.Stats, enemy.Defense);
                    dmg += EquippedWeapon.Attack;
                    int dealt = enemy.TakeDamage(dmg, EquippedWeapon.DamageType, isCrit);

                    // Hitstop + VFX per hit
                    VFXManager.Instance?.SpawnHitSpark(
                        _hitResults[i].bounds.center,
                        isCrit,
                        EquippedWeapon.TrailColor
                    );
                    hitSomething = true;
                }
            }

            // Different shake/sound for hit vs whiff
            if (hitSomething)
            {
                EventBus.Emit(new CameraShakeEvent { Intensity = 0.05f, Duration = 0.08f });
                AudioManager.Instance?.Play(EquippedWeapon.HitSound);
            }
            else
            {
                AudioManager.Instance?.Play(EquippedWeapon.SwingSound);
            }
        }

        public void EquipWeapon(WeaponDataSO weapon) => EquippedWeapon = weapon;
    }
}
