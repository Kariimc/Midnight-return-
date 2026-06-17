using System.Collections;
using UnityEngine;
using MidnightReturn.Data;
using MidnightReturn.Player;
using MidnightReturn.Utils;

namespace MidnightReturn.VerticalSlice
{
    // ══════════════════════════════════════════════════════════════════════════
    //  DeathBoss — 2-phase boss fight at the end of the Catacombs.
    //
    //  Phase 1  (100%→30% HP): ground combat
    //    • ScytheSweep — rush + wide OverlapBox swing
    //    • TeleportSlash — instant reposition behind player + swing
    //
    //  Phase 2  (≤30% HP): after 1.5s pause + VFX, becomes aerial
    //    • AerialDive — snap above player → dive down, AOE on landing
    //    • ScytheBarrage — 3 rapid swings across player position
    //
    //  Defeat reward: PlayerMovement.CanAirDash = true
    // ══════════════════════════════════════════════════════════════════════════
    public sealed class DeathBoss : Enemies.EnemyBase
    {
        private const float PHASE2_HP_RATIO  = 0.30f;
        private const float PHASE1_ATK_CD    = 2.3f;
        private const float PHASE2_ATK_CD    = 1.7f;
        private const float GROUND_GRAVITY   = -22f;

        private enum Phase { Intro, Ground, Transforming, Aerial, Done }

        private Phase  _phase = Phase.Intro;
        private float  _atkCd;
        private int    _atkIndex;
        private float  _vy;

        // Shared hitbox buffer — static to avoid per-frame allocation
        private static readonly Collider[] _hitBuffer = new Collider[8];

        // ── EnemyBase hooks ─────────────────────────────────────────────────
        protected override void Start()
        {
            base.Start();    // caches _player
            StartCoroutine(Intro());
        }

        protected override void OnUpdate(float dt)
        {
            switch (_phase)
            {
                case Phase.Ground: GroundTick(dt); break;
                case Phase.Aerial: AerialTick(dt); break;
            }
        }

        protected override void Die()
        {
            _phase = Phase.Done;
            if (_cc != null) _cc.enabled = false;
            base.Die();  // dissolve coroutine + EnemyDiedEvent
            EventBus.Emit(new BossDefeatedEvent { BossId = "death" });
            StartCoroutine(GrantAirDash());
        }

        // ── Intro ─────────────────────────────────────────────────────────
        private IEnumerator Intro()
        {
            EventBus.Emit(new BossStartedEvent { BossId = "death" });
            EventBus.Emit(new ScreenFlashEvent { Color = new Color(0.4f, 0f, 0.6f), Duration = 0.9f });
            EventBus.Emit(new CameraShakeEvent { Intensity = 0.4f, Duration = 0.9f });
            yield return new WaitForSeconds(1.5f);
            _phase = Phase.Ground;
            _atkCd = 1.2f;
        }

        // ── Phase 1 — ground ────────────────────────────────────────────────
        private void GroundTick(float dt)
        {
            if ((float)Hp / Data.MaxHp <= PHASE2_HP_RATIO)
            {
                _phase = Phase.Transforming;
                StartCoroutine(PhaseTransition());
                return;
            }

            // Gravity
            _vy = _cc.isGrounded
                ? -2f
                : Mathf.Max(_vy + GROUND_GRAVITY * dt, -28f);

            // Approach player, stop just outside attack range
            float dx     = PlayerPos.x - transform.position.x;
            float sign   = Mathf.Sign(dx);
            float approach = Mathf.Abs(dx) > Data.AttackRange * 2.2f
                ? sign * Data.MoveSpeed * dt : 0f;

            SetFacing(dx);
            _cc.Move(new Vector3(approach, _vy * dt, 0f));

            _atkCd -= dt;
            if (_atkCd <= 0f)
            {
                _atkCd = PHASE1_ATK_CD;
                StartCoroutine(_atkIndex++ % 2 == 0 ? ScytheSweep() : TeleportSlash());
            }
        }

        // ── Phase 2 — aerial ────────────────────────────────────────────────
        private void AerialTick(float dt)
        {
            // Float toward player + offset above
            float targetX = PlayerPos.x - FacingDir * 2.5f;
            float targetY = PlayerPos.y + 2.8f;
            transform.position = Vector3.Lerp(
                transform.position,
                new Vector3(targetX, targetY, 0f),
                dt * 2.0f);
            SetFacing(PlayerPos.x - transform.position.x);

            _atkCd -= dt;
            if (_atkCd <= 0f)
            {
                _atkCd = PHASE2_ATK_CD;
                StartCoroutine(_atkIndex++ % 2 == 0 ? AerialDive() : ScytheBarrage());
            }
        }

        // ── Phase transition ─────────────────────────────────────────────────
        private IEnumerator PhaseTransition()
        {
            EventBus.Emit(new ScreenFlashEvent { Color = new Color(0.7f, 0f, 1f),  Duration = 0.9f });
            EventBus.Emit(new CameraShakeEvent { Intensity = 0.6f, Duration = 0.9f });
            yield return new WaitForSeconds(1.5f);
            _cc.enabled = false;     // gravity off — movement via transform
            _atkIndex = 0;
            _atkCd = 0.9f;
            _phase = Phase.Aerial;
        }

        // ── Phase 1 attacks ──────────────────────────────────────────────────
        private IEnumerator ScytheSweep()
        {
            yield return new WaitForSeconds(Data.AttackWindup);

            float dir = Mathf.Sign(PlayerPos.x - transform.position.x);
            SetFacing(dir);

            // Rush
            float t = 0f;
            while (t < 0.22f)
            {
                t += Time.deltaTime;
                _cc.Move(new Vector3(dir * 16f * Time.deltaTime, _vy * Time.deltaTime, 0f));
                yield return null;
            }
            // Wide swing hitbox
            Swing(transform.position + new Vector3(dir * 2f, 0.9f, 0f), new Vector3(2.4f, 1.5f, 0.7f));
        }

        private IEnumerator TeleportSlash()
        {
            yield return new WaitForSeconds(Data.AttackWindup * 0.5f);

            // Snap behind player (in the direction they face away)
            float behindX = PlayerPos.x - Mathf.Sign(PlayerPos.x - transform.position.x) * 1.6f;
            transform.position = new Vector3(behindX, PlayerPos.y, 0f);
            SetFacing(PlayerPos.x - transform.position.x);

            EventBus.Emit(new ScreenFlashEvent { Color = new Color(0.5f, 0f, 0.7f, 0.25f), Duration = 0.15f });
            yield return new WaitForSeconds(0.12f);
            Swing(transform.position + new Vector3(FacingDir * 1.8f, 0.9f, 0f), new Vector3(2.2f, 1.5f, 0.7f));
        }

        // ── Phase 2 attacks ──────────────────────────────────────────────────
        private IEnumerator AerialDive()
        {
            // Reposition above target
            var above = new Vector3(PlayerPos.x, PlayerPos.y + 5.5f, 0f);
            float t = 0f;
            var startPos = transform.position;
            while (t < 0.28f)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, above, t / 0.28f);
                yield return null;
            }

            // Dive
            t = 0f;
            var diveEnd = new Vector3(above.x, PlayerPos.y - 0.3f, 0f);
            while (t < 0.16f)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(above, diveEnd, t / 0.16f);
                yield return null;
            }

            // Landing AOE
            Swing(transform.position + new Vector3(0f, 0.8f, 0f), new Vector3(1.6f, 2f, 0.7f));
            EventBus.Emit(new CameraShakeEvent { Intensity = 0.5f, Duration = 0.35f });
        }

        private IEnumerator ScytheBarrage()
        {
            for (int i = 0; i < 3; i++)
            {
                Swing(transform.position + new Vector3(FacingDir * 1.8f * (i + 1), 0.4f, 0f),
                      new Vector3(2f, 1.8f, 0.7f));
                EventBus.Emit(new ScreenFlashEvent { Color = new Color(0.3f, 0f, 0.5f, 0.12f), Duration = 0.12f });
                yield return new WaitForSeconds(0.32f);
            }
        }

        // ── Shared swing (OverlapBox player hit) ─────────────────────────────
        private void Swing(Vector3 center, Vector3 halfExtents)
        {
            int n = Physics.OverlapBoxNonAlloc(center, halfExtents * 0.5f, _hitBuffer);
            for (int i = 0; i < n; i++)
            {
                if (!_hitBuffer[i].CompareTag("Player")) continue;
                var pc = _hitBuffer[i].GetComponent<PlayerController>();
                pc?.TakeDamage(Data.Attack);
                EventBus.Emit(new CameraShakeEvent { Intensity = 0.3f, Duration = 0.22f });
                break;
            }
        }

        // ── Air Dash reward ──────────────────────────────────────────────────
        private IEnumerator GrantAirDash()
        {
            yield return new WaitForSeconds(1.8f);
            var pm = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerMovement>();
            if (pm != null) pm.CanAirDash = true;
            EventBus.Emit(new ItemPickedUpEvent { ItemId = "ability_air_dash", Quantity = 1 });
        }

        // ── Helpers ──────────────────────────────────────────────────────────
        private void SetFacing(float dx)
        {
            if (Mathf.Abs(dx) < 0.05f) return;
            var s = transform.localScale;
            transform.localScale = new Vector3(Mathf.Abs(s.x) * (dx > 0 ? 1f : -1f), s.y, s.z);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  BossTrigger — one-shot trigger that fires OnPlayerEntered and self-destructs.
    //  Used to spawn Death when the player reaches the Catacombs exit zone.
    // ══════════════════════════════════════════════════════════════════════════
    [RequireComponent(typeof(Collider))]
    public sealed class BossTrigger : MonoBehaviour
    {
        public System.Action<GameObject> OnPlayerEntered;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            OnPlayerEntered?.Invoke(other.gameObject);
            Destroy(gameObject);
        }
    }
}
