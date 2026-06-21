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
        public int          ComboIndex     { get; private set; }

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
            ComboIndex = comboIndex;
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
            bool critLanded   = false;
            int  facing       = _player.Movement.FacingDir;
            for (int i = 0; i < count; i++)
            {
                if (_hitResults[i].TryGetComponent<Enemies.EnemyBase>(out var enemy))
                {
                    var (dmg, isCrit) = PlayerStatsSystem.CalcDamage(_player.Stats, enemy.Defense);
                    dmg += EquippedWeapon.Attack;
                    int dealt = enemy.TakeDamage(dmg, EquippedWeapon.DamageType, isCrit);

                    // Directional impact spark flung in the attack direction, enemy-tinted.
                    Vector3 hitPos = _hitResults[i].bounds.center;
                    VFXManager.Instance?.SpawnHitSpark(hitPos, facing, EquippedWeapon.TrailColor, isCrit);
                    if (isCrit)
                    {
                        VFXManager.Instance?.SpawnCritBurst(hitPos);
                        critLanded = true;
                    }
                    if (dealt >= 20)
                        VFXManager.Instance?.SpawnBloodDrip(hitPos, facing);

                    hitSomething = true;
                }
            }

            // Hit/whiff differentiation. Hitstop is longer on crits (Dread-style weight).
            if (hitSomething)
            {
                Time.timeScale = 0f;
                StartCoroutine(ReleaseHitstop(critLanded ? 0.05f : 0.03f));
                EventBus.Emit(new CameraShakeEvent { Intensity = critLanded ? 0.12f : 0.05f, Duration = 0.1f });
                AudioManager.Instance?.Play(EquippedWeapon.HitSound);
            }
            else
            {
                AudioManager.Instance?.Play(EquippedWeapon.SwingSound);
            }
        }

        // Real-time hitstop release — uses unscaled time so it survives timeScale=0.
        private IEnumerator ReleaseHitstop(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            // Don't stomp a rewind/pause that legitimately froze time.
            if (Time.timeScale == 0f) Time.timeScale = 1f;
        }

        public void EquipWeapon(WeaponDataSO weapon) => EquippedWeapon = weapon;
    }
}
