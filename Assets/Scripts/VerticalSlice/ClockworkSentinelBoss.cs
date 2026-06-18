using System.Collections;
using UnityEngine;
using MidnightReturn.Data;
using MidnightReturn.Player;
using MidnightReturn.Utils;

namespace MidnightReturn.VerticalSlice
{
    // ══════════════════════════════════════════════════════════════════════════
    //  ClockworkSentinelBoss — 2-phase boss at the apex of the Clocktower.
    //
    //  Phase 1  (100%→40% HP): ground combat on the top platform
    //    • GearSlam   — rush + wide OverlapBox stomp AOE
    //    • PendulumThrow — three arc hitboxes flung at the player
    //
    //  Phase 2  (≤40% HP): becomes aerial
    //    • AerialStrike — snap high above + lightning-fast dive + AOE
    //    • GearBarrage  — 3 rapid fan swings across player position
    //
    //  Defeat reward: PlayerMovement.CanHourglass = true (Death's Hourglass)
    // ══════════════════════════════════════════════════════════════════════════
    public sealed class ClockworkSentinelBoss : Enemies.EnemyBase
    {
        private const float PHASE2_HP_RATIO = 0.40f;
        private const float PHASE1_ATK_CD   = 2.4f;
        private const float PHASE2_ATK_CD   = 1.7f;
        private const float GROUND_GRAVITY  = -22f;

        private enum Phase { Intro, Ground, Transforming, Aerial, Done }

        private Phase _phase = Phase.Intro;
        private float _atkCd;
        private int   _atkIndex;
        private float _vy;

        private static readonly Collider[] _hitBuffer = new Collider[8];

        // ── EnemyBase hooks ───────────────────────────────────────────────────
        protected override void Start()
        {
            base.Start();
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
            base.Die();
            EventBus.Emit(new BossDefeatedEvent { BossId = "clockwork_sentinel" });
            StartCoroutine(GrantHourglass());
        }

        // ── Intro ─────────────────────────────────────────────────────────────
        private IEnumerator Intro()
        {
            EventBus.Emit(new BossStartedEvent { BossId = "clockwork_sentinel" });
            EventBus.Emit(new ScreenFlashEvent { Color = new Color(1f, 0.6f, 0.1f), Duration = 1.0f });
            EventBus.Emit(new CameraShakeEvent { Intensity = 0.5f, Duration = 1.0f });
            _anim?.CrossFadeInFixedTime("Idle", 0.1f);
            yield return new WaitForSeconds(1.5f);
            _phase = Phase.Ground;
            _atkCd = 1.2f;
        }

        // ── Phase 1 — ground ─────────────────────────────────────────────────
        private void GroundTick(float dt)
        {
            if ((float)Hp / Data.MaxHp <= PHASE2_HP_RATIO)
            {
                _phase = Phase.Transforming;
                StartCoroutine(PhaseTransition());
                return;
            }

            _vy = _cc.isGrounded
                ? -2f
                : Mathf.Max(_vy + GROUND_GRAVITY * dt, -28f);

            float dx     = PlayerPos.x - transform.position.x;
            float sign   = Mathf.Sign(dx);
            float approach = Mathf.Abs(dx) > Data.AttackRange * 2.6f
                ? sign * Data.MoveSpeed * dt : 0f;

            SetFacing(dx);
            if (_cc.enabled)
            {
                _cc.Move(new Vector3(approach, _vy * dt, 0f));
                transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
            }

            _atkCd -= dt;
            if (_atkCd <= 0f)
            {
                _atkCd = PHASE1_ATK_CD;
                StartCoroutine(_atkIndex++ % 2 == 0 ? GearSlam() : PendulumThrow());
            }
        }

        // ── Phase 2 — aerial ─────────────────────────────────────────────────
        private void AerialTick(float dt)
        {
            float targetX = PlayerPos.x - FacingDir * 2f;
            float targetY = PlayerPos.y + 3.5f;
            transform.position = Vector3.Lerp(
                transform.position,
                new Vector3(targetX, targetY, 0f),
                dt * 1.9f);
            SetFacing(PlayerPos.x - transform.position.x);

            _atkCd -= dt;
            if (_atkCd <= 0f)
            {
                _atkCd = PHASE2_ATK_CD;
                StartCoroutine(_atkIndex++ % 2 == 0 ? AerialStrike() : GearBarrage());
            }
        }

        // ── Phase transition ──────────────────────────────────────────────────
        private IEnumerator PhaseTransition()
        {
            EventBus.Emit(new ScreenFlashEvent { Color = new Color(1f, 0.7f, 0.15f), Duration = 1.0f });
            EventBus.Emit(new CameraShakeEvent { Intensity = 0.65f, Duration = 1.0f });
            yield return new WaitForSeconds(1.5f);
            if (_cc != null) _cc.enabled = false;
            _atkIndex = 0;
            _atkCd    = 0.8f;
            _phase    = Phase.Aerial;
        }

        // ── Phase 1 attacks ───────────────────────────────────────────────────
        private IEnumerator GearSlam()
        {
            _anim?.CrossFadeInFixedTime("Attack", 0.08f);
            yield return new WaitForSeconds(Data.AttackWindup);

            float dir = Mathf.Sign(PlayerPos.x - transform.position.x);
            SetFacing(dir);

            // Rush toward player
            float t = 0f;
            while (t < 0.20f)
            {
                t += Time.deltaTime;
                if (_cc != null && _cc.enabled)
                    _cc.Move(new Vector3(dir * 15f * Time.deltaTime, _vy * Time.deltaTime, 0f));
                yield return null;
            }

            // Ground shockwave
            Swing(transform.position + new Vector3(dir * 0.5f, 0.5f, 0f),
                  new Vector3(2.8f, 1.6f, 0.8f));
            EventBus.Emit(new CameraShakeEvent { Intensity = 0.4f, Duration = 0.3f });
            EventBus.Emit(new ScreenFlashEvent { Color = new Color(1f, 0.6f, 0.1f, 0.08f), Duration = 0.12f });
        }

        private IEnumerator PendulumThrow()
        {
            _anim?.CrossFadeInFixedTime("Attack", 0.08f);
            yield return new WaitForSeconds(Data.AttackWindup * 0.5f);

            float dir = Mathf.Sign(PlayerPos.x - transform.position.x);
            SetFacing(dir);

            // Three arc hitboxes — simulates gear projectiles in a sweeping arc
            for (int i = 0; i < 3; i++)
            {
                float xOff = dir * (1.5f + i * 1.8f);
                float yOff = 0.9f - i * 0.25f;
                Swing(transform.position + new Vector3(xOff, yOff, 0f),
                      new Vector3(1.1f, 1.1f, 0.8f));
                EventBus.Emit(new ScreenFlashEvent
                {
                    Color    = new Color(1f, 0.55f, 0.1f, 0.06f),
                    Duration = 0.07f,
                });
                yield return new WaitForSeconds(0.18f);
            }
        }

        // ── Phase 2 attacks ───────────────────────────────────────────────────
        private IEnumerator AerialStrike()
        {
            // Snap high above player
            var above    = new Vector3(PlayerPos.x, PlayerPos.y + 6f, 0f);
            float t      = 0f;
            var startPos = transform.position;
            while (t < 0.22f)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, above, t / 0.22f);
                yield return null;
            }

            // Flash telegraph
            EventBus.Emit(new ScreenFlashEvent { Color = new Color(1f, 0.7f, 0.1f, 0.18f), Duration = 0.12f });
            yield return new WaitForSeconds(0.1f);

            // Dive
            t = 0f;
            var diveEnd = new Vector3(above.x, PlayerPos.y + 0.1f, 0f);
            while (t < 0.14f)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(above, diveEnd, t / 0.14f);
                yield return null;
            }

            Swing(transform.position + new Vector3(0f, 0.8f, 0f), new Vector3(2.0f, 2.4f, 0.8f));
            EventBus.Emit(new CameraShakeEvent { Intensity = 0.55f, Duration = 0.35f });
        }

        private IEnumerator GearBarrage()
        {
            for (int i = 0; i < 3; i++)
            {
                float xOff = FacingDir * (1.6f + i * 1.3f);
                float yOff = 0.6f - i * 0.28f;
                Swing(transform.position + new Vector3(xOff, yOff, 0f),
                      new Vector3(1.9f, 1.7f, 0.8f));
                EventBus.Emit(new ScreenFlashEvent
                {
                    Color    = new Color(1f, 0.6f, 0.1f, 0.09f),
                    Duration = 0.09f,
                });
                yield return new WaitForSeconds(0.24f);
            }
        }

        // ── Shared helpers ────────────────────────────────────────────────────
        private void Swing(Vector3 center, Vector3 halfExtents)
        {
            int n = Physics.OverlapBoxNonAlloc(center, halfExtents * 0.5f, _hitBuffer);
            for (int i = 0; i < n; i++)
            {
                if (!_hitBuffer[i].CompareTag("Player")) continue;
                var pc = _hitBuffer[i].GetComponent<PlayerController>();
                pc?.TakeDamage(Data.Attack);
                EventBus.Emit(new CameraShakeEvent { Intensity = 0.3f, Duration = 0.2f });
                break;
            }
        }

        private void SetFacing(float dx)
        {
            if (Mathf.Abs(dx) < 0.05f) return;
            var s = transform.localScale;
            transform.localScale = new Vector3(Mathf.Abs(s.x) * (dx > 0 ? 1f : -1f), s.y, s.z);
        }

        // ── Death's Hourglass reward ──────────────────────────────────────────
        private IEnumerator GrantHourglass()
        {
            yield return new WaitForSeconds(1.8f);

            var pm = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerMovement>();
            if (pm != null) pm.CanHourglass = true;

            EventBus.Emit(new ItemPickedUpEvent { ItemId = "ability_hourglass", Quantity = 1 });

            AbilityAcquisitionPresenter.Show(
                "Death's Hourglass",
                "Rewind time 3 seconds — restores position and HP.\nCrouch + Sub-Weapon to activate.",
                new Color(0.85f, 0.65f, 0.1f));
        }
    }
}
