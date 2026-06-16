using System.Collections;
using UnityEngine;
using MidnightReturn.Data;
using MidnightReturn.Player;
using MidnightReturn.Utils;

namespace MidnightReturn.Systems.Inventory
{
    // Manages 4 equipped spell slots, MP consumption, per-slot cooldowns, and casting.
    // Attaches to the same GameObject as PlayerController.
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(PlayerInputHandler))]
    public sealed class PlayerSpellSystem : MonoBehaviour
    {
        public const int SLOT_COUNT = 4;

        [SerializeField] Transform _castOrigin; // assign to player's hand/chest bone

        public int ActiveSlot { get; private set; } = 0;

        readonly float[] _cooldown = new float[SLOT_COUNT];

        PlayerController   _player;
        PlayerInputHandler _input;

        void Awake()
        {
            _player = GetComponent<PlayerController>();
            _input  = GetComponent<PlayerInputHandler>();
        }

        void Update()
        {
            for (int i = 0; i < SLOT_COUNT; i++)
                if (_cooldown[i] > 0f) _cooldown[i] -= Time.deltaTime;

            if (_input.HasSpell)
            {
                _input.ConsumeSpell();
                TryCast(ActiveSlot);
            }
        }

        public void CycleSlot(int delta)
        {
            ActiveSlot = (ActiveSlot + delta + SLOT_COUNT) % SLOT_COUNT;
        }

        public bool CanCast(int slot)
        {
            if ((uint)slot >= SLOT_COUNT) return false;
            if (_cooldown[slot] > 0f) return false;
            var gm = Core.GameManager.Instance;
            if (gm == null) return false;
            var so = InventorySystem.Instance?.GetSpell(gm.Save.SpellSlots[slot]);
            return so != null && _player.Stats.Mp >= so.MpCost;
        }

        public bool TryCast(int slot)
        {
            if (!CanCast(slot)) return false;
            var gm = Core.GameManager.Instance;
            var so = InventorySystem.Instance.GetSpell(gm.Save.SpellSlots[slot]);

            _player.Stats.Mp = Mathf.Max(0, _player.Stats.Mp - so.MpCost);
            _cooldown[slot]  = so.Cooldown;

            Vector3 origin = _castOrigin != null ? _castOrigin.position : transform.position;
            AudioManager.Instance?.Play(so.CastSound);

            if (so.CastVFX != null)
                VFXManager.Instance?.PlayEnemyHitVFX(so.CastVFX, origin);

            switch (so.Type)
            {
                case SpellType.Projectile: StartCoroutine(FireProjectile(so, origin)); break;
                case SpellType.AreaBurst:  StartCoroutine(AreaBurst(so, origin)); break;
                case SpellType.Buff:       StartCoroutine(ApplyBuff(so)); break;
                case SpellType.Drain:      StartCoroutine(Drain(so, origin)); break;
            }

            EventBus.Emit(new SpellCastEvent { SpellId = gm.Save.SpellSlots[slot], Slot = slot, Origin = origin });
            return true;
        }

        // Cooldown fraction 0–1 (for UI progress ring)
        public float CooldownFraction(int slot)
        {
            if ((uint)slot >= SLOT_COUNT || _cooldown[slot] <= 0f) return 0f;
            var gm = Core.GameManager.Instance;
            var so = InventorySystem.Instance?.GetSpell(gm?.Save.SpellSlots[slot]);
            return so != null ? _cooldown[slot] / so.Cooldown : 0f;
        }

        // ── Spell effects ─────────────────────────────────────────────────────
        IEnumerator FireProjectile(SpellDataSO so, Vector3 origin)
        {
            int dir = _player.Movement.FacingDir;
            if (so.ProjectilePrefab != null)
            {
                var go = Instantiate(so.ProjectilePrefab, origin, Quaternion.identity);
                if (go.TryGetComponent<Rigidbody>(out var rb))
                    rb.velocity = new Vector3(dir * so.ProjectileSpeed, 0f, 0f);
                Destroy(go, so.Range / Mathf.Max(1f, so.ProjectileSpeed));
            }
            else
            {
                // Hitscan fallback
                HitLine(origin, origin + new Vector3(dir * so.Range, 0f, 0f), so);
                if (so.ImpactVFX != null)
                    VFXManager.Instance?.PlayEnemyHitVFX(so.ImpactVFX,
                        origin + new Vector3(dir * so.Range, 0f, 0f));
            }
            yield break;
        }

        IEnumerator AreaBurst(SpellDataSO so, Vector3 origin)
        {
            if (so.ImpactVFX != null)
                VFXManager.Instance?.PlayEnemyHitVFX(so.ImpactVFX, origin);
            EventBus.Emit(new ScreenFlashEvent { Color = new Color(0.3f, 0.1f, 0.5f, 0.2f), Duration = 0.2f });

            var cols = Physics.OverlapSphere(origin, so.AoeRadius, LayerMask.GetMask("Enemy"));
            foreach (var col in cols)
                if (col.TryGetComponent<Enemies.EnemyBase>(out var e))
                    e.TakeDamage(so.BaseDamage + _player.Stats.Int, (DamageType)(int)so.Element, false);
            yield break;
        }

        IEnumerator ApplyBuff(SpellDataSO so)
        {
            _player.Stats.Atk += so.StatBonus;
            yield return new WaitForSeconds(so.BuffDuration);
            _player.Stats.Atk -= so.StatBonus;
        }

        IEnumerator Drain(SpellDataSO so, Vector3 origin)
        {
            int dir   = _player.Movement.FacingDir;
            var half  = new Vector3(so.Range * 0.5f, 0.6f, 0.5f);
            var center= origin + new Vector3(dir * so.Range * 0.5f, 0f, 0f);
            var cols  = Physics.OverlapBox(center, half, Quaternion.identity, LayerMask.GetMask("Enemy"));
            int healed = 0;
            foreach (var col in cols)
                if (col.TryGetComponent<Enemies.EnemyBase>(out var e))
                    healed += Mathf.FloorToInt(e.TakeDamage(so.BaseDamage + _player.Stats.Int, (DamageType)(int)so.Element, false) * 0.3f);
            _player.Stats.Hp = Mathf.Min(_player.Stats.MaxHp, _player.Stats.Hp + healed);
            yield break;
        }

        void HitLine(Vector3 from, Vector3 to, SpellDataSO so)
        {
            var dir  = (to - from).normalized;
            var hits = Physics.RaycastAll(new Ray(from, dir), Vector3.Distance(from, to), LayerMask.GetMask("Enemy"));
            foreach (var h in hits)
                if (h.collider.TryGetComponent<Enemies.EnemyBase>(out var e))
                    e.TakeDamage(so.BaseDamage + _player.Stats.Int, (DamageType)(int)so.Element, false);
        }
    }
}
