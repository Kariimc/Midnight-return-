using System.Collections;
using UnityEngine;
using Cinemachine;
using MidnightReturn.AI;
using MidnightReturn.Data;
using MidnightReturn.Systems;
using MidnightReturn.Utils;

namespace MidnightReturn.Enemies.Boss
{
    // ══════════════════════════════════════════════════════════════════
    //  BossController
    //  Extends EnemyBase directly. Owns a macro FSM (BossPhase enum)
    //  and rebuilds a micro BehaviorTree per phase from BossPhaseDataSO
    //  assets. The BT drives movement and attack selection each
    //  FixedUpdate while the macro FSM controls HP threshold transitions,
    //  intro sequence, and death.
    // ══════════════════════════════════════════════════════════════════
    public sealed class BossController : EnemyBase
    {
        // ── Inspector ─────────────────────────────────────────────────
        [Header("Boss Identity")]
        [SerializeField] string _bossId = "Death";

        [Header("Phase Data (assign in order: phase 0, 1, 2)")]
        [SerializeField] BossPhaseDataSO[] _phases;       // 3 entries expected

        [Header("Arena")]
        [SerializeField] float _arenaLeftX  = -8f;
        [SerializeField] float _arenaRightX =  8f;

        [Header("Attack Prefabs")]
        [SerializeField] GameObject _projectilePrefab;
        [SerializeField] GameObject _minionPrefab;

        [Header("Cinemachine")]
        [SerializeField] CinemachineImpulseSource _impulse;

        // ── Runtime state ─────────────────────────────────────────────
        enum BossPhase { Idle, Intro, Phase0, Phase1, Phase2, Dying, Dead }

        BossPhase      _phase      = BossPhase.Idle;
        BTBlackboard   _bb;
        BTNode         _btRoot;
        BossPhaseDataSO _activeData;
        float[]        _attackLastUsed;   // per-attack-slot last-fired time
        int            _attackPatternIdx  = -1;

        // Per-attack execution state
        bool  _attacking;
        float _telegraphTimer;

        // Minion tracking (cached array to avoid GC)
        readonly Collider[] _overlapBuf = new Collider[8];

        // ── Unity lifecycle ───────────────────────────────────────────
        protected override void Awake()
        {
            base.Awake();
            _bb = new BTBlackboard();
        }

        protected override void Start()
        {
            base.Start();
            // Boss waits for player trigger — don't start AI yet
        }

        // Called by trigger zone when player enters the boss room
        public void TriggerIntro()
        {
            if (_phase != BossPhase.Idle) return;
            StartCoroutine(IntroSequence());
        }

        // ── Core update loop ──────────────────────────────────────────
        protected override void OnUpdate(float dt)
        {
            if (_phase is BossPhase.Idle or BossPhase.Intro or BossPhase.Dying or BossPhase.Dead)
                return;
            if (_btRoot == null || _player == null) return;

            // HP threshold check → phase promotion
            CheckPhaseTransition();

            // Write blackboard
            _bb.Set(BBKey.DeltaTime,    dt);
            _bb.Set(BBKey.PlayerPos,    _player.position);
            _bb.Set(BBKey.SelfPos,      transform.position);
            _bb.Set(BBKey.DistToPlayer, DistToPlayer);
            _bb.Set(BBKey.HpNormalized, Data != null ? (float)Hp / Data.MaxHp : 1f);
            _bb.Set(BBKey.IsGrounded,   _cc.isGrounded);

            _btRoot.Execute(_bb);
        }

        // ── Phase management ──────────────────────────────────────────
        void CheckPhaseTransition()
        {
            if (_phases == null) return;
            float hpNorm = (float)Hp / Mathf.Max(1, Data.MaxHp);

            int targetPhaseIdx = 0;
            // _phases[0] = 100%→66%, [1] = 66%→33%, [2] = 33%→0%
            for (int i = _phases.Length - 1; i >= 0; i--)
            {
                if (hpNorm <= _phases[i].HPThreshold) { targetPhaseIdx = i; break; }
            }

            BossPhase targetMacro = (BossPhase)((int)BossPhase.Phase0 + targetPhaseIdx);
            if (_phase != targetMacro)
                StartCoroutine(TransitionToPhase(targetPhaseIdx));
        }

        void BuildBT(BossPhaseDataSO data)
        {
            _activeData    = data;
            _attackLastUsed = new float[data.AttackPatterns?.Length ?? 0];
            _btRoot = BuildBossTree();
        }

        BTNode BuildBossTree()
        {
            // Per-phase BT: try to attack if cooldown ready, otherwise circle-strafe
            return new Selector("BossRoot",
                // Attack branch: pick and execute an attack
                new Sequence("AttackBranch",
                    new Condition("ReadyToAttack", bb => !_attacking),
                    new Action("PickAttack", bb =>
                    {
                        int idx = _activeData.PickAttack(_attackLastUsed, Time.time);
                        if (idx < 0) return BTStatus.Failure;
                        _attackPatternIdx = idx;
                        return BTStatus.Success;
                    }),
                    new Action("ExecuteAttack", bb =>
                    {
                        StartCoroutine(ExecuteAttackCoroutine(_attackPatternIdx));
                        return BTStatus.Success;
                    })
                ),
                // Strafe / reposition while attack on cooldown
                new Action("Strafe", bb =>
                {
                    float dt      = bb.Get(BBKey.DeltaTime, Time.fixedDeltaTime);
                    Vector3 pPos  = bb.Get<Vector3>(BBKey.PlayerPos);
                    float dist    = bb.Get<float>(BBKey.DistToPlayer);
                    float speed   = _activeData?.MoveSpeed ?? 3f;
                    float aggr    = _activeData?.AggressionMultiplier ?? 1f;

                    // Maintain 3-5u preferred distance, side-step horizontally
                    float preferred = 3.5f;
                    float dx = transform.position.x - pPos.x;
                    Vector3 target;
                    if (dist < preferred - 0.5f)
                    {
                        // Too close — back off
                        target = transform.position + new Vector3(Mathf.Sign(dx), 0f, 0f);
                    }
                    else if (dist > preferred + 1f)
                    {
                        // Too far — approach
                        target = pPos + new Vector3(Mathf.Sign(dx) * preferred, 0f, 0f);
                    }
                    else
                    {
                        // Strafe: oscillate side to side
                        float side = Mathf.Sin(Time.time * 1.4f * aggr);
                        target = transform.position + new Vector3(side * speed * dt, 0f, 0f);
                    }

                    // Clamp to arena bounds
                    target.x = Mathf.Clamp(target.x, _arenaLeftX, _arenaRightX);
                    MoveToward(target, speed * aggr, dt);

                    // Face player
                    transform.localScale = new Vector3(
                        pPos.x >= transform.position.x ? 1f : -1f, 1f, 1f);
                    return BTStatus.Running;
                })
            );
        }

        // ── Attack coroutines ─────────────────────────────────────────
        IEnumerator ExecuteAttackCoroutine(int idx)
        {
            if (_attacking) yield break;
            _attacking = true;

            var entry = _activeData.AttackPatterns[idx];
            _anim?.SetBool("IsMoving", false);

            // Telegraph
            if (entry.TelegraphDuration > 0f)
            {
                _anim?.SetTrigger("TelegraphAttack");
                VFXManager.Instance?.PlayTelegraphWarning(transform.position, entry.TelegraphDuration);
                yield return new WaitForSeconds(entry.TelegraphDuration);
            }

            // Execute
            switch (entry.Type)
            {
                case BossAttackType.ScytheSweep:   yield return DoScytheSweep(entry);   break;
                case BossAttackType.TeleportSlam:  yield return DoTeleportSlam(entry);  break;
                case BossAttackType.DashSlash:     yield return DoDashSlash(entry);     break;
                case BossAttackType.Barrage:       yield return DoBarrage(entry);       break;
                case BossAttackType.SummonMinions: yield return DoSummonMinions(entry); break;
                case BossAttackType.SpinAttack:    yield return DoSpinAttack(entry);    break;
            }

            _attackLastUsed[idx] = Time.time;
            _attacking = false;
        }

        IEnumerator DoScytheSweep(AttackEntry e)
        {
            _anim?.SetTrigger("Attack");
            // Wide horizontal hitbox — 3.5u reach
            yield return new WaitForSeconds(0.15f);
            int hit = Physics.OverlapBoxNonAlloc(
                transform.position + transform.right * 1.8f,
                new Vector3(1.8f, 1f, 0.5f),
                _overlapBuf, Quaternion.identity,
                LayerMask.GetMask("Player"));
            ApplyHits(hit, e.Damage);
            VFXManager.Instance?.PlayWeaponSwing(transform.position, transform.localScale.x > 0 ? 1 : -1);
            _impulse?.GenerateImpulse(new Vector3(0.15f, 0.06f, 0f));
            yield return new WaitForSeconds(0.3f);
        }

        IEnumerator DoTeleportSlam(AttackEntry e)
        {
            // Fade out
            VFXManager.Instance?.PlayDeathBurst(null, transform.position, Color.black * 0.5f);
            _renderer.enabled = false;
            yield return new WaitForSeconds(0.35f);

            // Reappear above player
            Vector3 dest = _player.position + Vector3.up * 4f;
            dest.z = 0f;
            transform.position = dest;
            _renderer.enabled  = true;
            _anim?.SetTrigger("Jump");

            // Fall with gravity until grounded
            float fallTimer = 0f;
            while (!_cc.isGrounded && fallTimer < 1.5f)
            {
                fallTimer += Time.fixedDeltaTime;
                float fall = Physics.gravity.y * fallTimer * fallTimer * 0.5f;
                _cc.Move(new Vector3(0f, fall * Time.fixedDeltaTime, 0f));
                yield return new WaitForFixedUpdate();
            }

            // Slam impact
            _anim?.SetTrigger("Land");
            _impulse?.GenerateImpulse(new Vector3(0f, -0.35f, 0f));
            EventBus.Emit(new CameraShakeEvent { Intensity = 0.35f, Duration = 0.4f });
            VFXManager.Instance?.SpawnLandDust(transform.position);

            int hit = Physics.OverlapBoxNonAlloc(
                transform.position, new Vector3(2f, 1f, 0.5f),
                _overlapBuf, Quaternion.identity, LayerMask.GetMask("Player"));
            ApplyHits(hit, e.Damage);
            yield return new WaitForSeconds(0.5f);
        }

        IEnumerator DoDashSlash(AttackEntry e)
        {
            int dir = _player.position.x > transform.position.x ? 1 : -1;
            transform.localScale = new Vector3(dir, 1f, 1f);
            _anim?.SetTrigger("Dash");

            float speed = (_activeData?.MoveSpeed ?? 4f) * 3.5f;
            float timer = 0f;
            const float DURATION = 0.28f;

            while (timer < DURATION)
            {
                timer += Time.fixedDeltaTime;
                _cc.Move(new Vector3(dir * speed * Time.fixedDeltaTime, 0f, 0f));
                transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

                // Continuous hitbox while dashing
                int hit = Physics.OverlapBoxNonAlloc(
                    transform.position, new Vector3(0.5f, 0.9f, 0.4f),
                    _overlapBuf, Quaternion.identity, LayerMask.GetMask("Player"));
                ApplyHits(hit, e.Damage);
                VFXManager.Instance?.PlayDashBurst(transform.position, dir);
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForSeconds(0.2f);
        }

        IEnumerator DoBarrage(AttackEntry e)
        {
            _anim?.SetTrigger("Attack");
            yield return new WaitForSeconds(0.1f);

            const int SHOTS = 5;
            const float SPREAD = 25f;
            for (int i = 0; i < SHOTS; i++)
            {
                float angle = -SPREAD * 2 + (SPREAD * i);
                var rot     = Quaternion.Euler(0f, 0f, angle);
                Vector3 dir = rot * ((_player.position - transform.position).normalized);
                dir.z = 0f;

                if (_projectilePrefab != null)
                {
                    var proj = Instantiate(_projectilePrefab, transform.position + dir * 0.6f, Quaternion.identity);
                    proj.GetComponent<Projectile>()?.Init(dir * 8f, e.Damage, DamageType.Arcane);
                }
                yield return new WaitForSeconds(0.1f);
            }
            yield return new WaitForSeconds(0.4f);
        }

        IEnumerator DoSummonMinions(AttackEntry e)
        {
            _anim?.SetTrigger("Summon");
            EventBus.Emit(new ScreenFlashEvent { Color = new Color(0.5f, 0f, 0.8f), Duration = 0.3f });
            yield return new WaitForSeconds(0.5f);

            if (_minionPrefab != null)
            {
                Vector3[] spawnPoints =
                {
                    transform.position + Vector3.left  * 3f,
                    transform.position + Vector3.right * 3f,
                };
                foreach (var sp in spawnPoints)
                {
                    var spawnPos = new Vector3(sp.x, sp.y, 0f);
                    var m = Instantiate(_minionPrefab, spawnPos, Quaternion.identity);
                    VFXManager.Instance?.PlayDeathBurst(null, spawnPos, new Color(0.4f, 0f, 0.6f));
                }
            }
            yield return new WaitForSeconds(0.6f);
        }

        IEnumerator DoSpinAttack(AttackEntry e)
        {
            // Phase 3 exclusive — 360° rotation with sweeping hitbox
            _anim?.SetTrigger("SpinAttack");
            const float SPIN_DURATION = 2f;
            float timer = 0f;
            float lastHitTime = -1f;

            while (timer < SPIN_DURATION)
            {
                timer += Time.fixedDeltaTime;
                // Spin hitbox 360° — check every 0.1s to avoid spam
                if (Time.time - lastHitTime > 0.1f)
                {
                    int hit = Physics.OverlapBoxNonAlloc(
                        transform.position, new Vector3(2.5f, 1f, 0.5f),
                        _overlapBuf, Quaternion.identity, LayerMask.GetMask("Player"));
                    ApplyHits(hit, Mathf.FloorToInt(e.Damage * 0.6f));
                    lastHitTime = Time.time;
                }
                VFXManager.Instance?.PlayWeaponSwing(transform.position, Mathf.Sin(timer * 6f) > 0 ? 1 : -1);
                yield return new WaitForFixedUpdate();
            }
            _impulse?.GenerateImpulse(new Vector3(0.25f, 0.1f, 0f));
            yield return new WaitForSeconds(0.5f);
        }

        // ── Intro / phase transition / death ─────────────────────────
        IEnumerator IntroSequence()
        {
            _phase = BossPhase.Intro;
            EventBus.Emit(new BossStartedEvent { BossId = _bossId });
            _anim?.SetTrigger("Intro");

            yield return new WaitForSeconds(2.5f); // cinematic pause

            // Begin phase 0
            yield return TransitionToPhase(0);
        }

        IEnumerator TransitionToPhase(int idx)
        {
            if (idx >= (_phases?.Length ?? 0)) yield break;

            var data = _phases[idx];
            _phase   = (BossPhase)((int)BossPhase.Phase0 + idx);

            // Screen flash + VFX
            EventBus.Emit(new ScreenFlashEvent { Color = data.TransitionFlashColor, Duration = 0.4f });
            if (data.TransitionVFX != null)
                VFXManager.Instance?.PlayDeathBurst(data.TransitionVFX, transform.position, data.TransitionFlashColor);

            _impulse?.GenerateImpulse(new Vector3(0.2f, 0.12f, 0f));

            if (!string.IsNullOrEmpty(data.PhaseIntroText))
                EventBus.Emit(new ScreenFlashEvent { Color = data.TransitionFlashColor, Duration = 0.1f });

            yield return new WaitForSeconds(0.3f);
            BuildBT(data);
        }

        protected override void Die()
        {
            if (_isDying) return;
            _isDying = true;
            _phase   = BossPhase.Dying;
            StartCoroutine(BossDeath());
        }

        IEnumerator BossDeath()
        {
            _attacking = false;
            _anim?.SetTrigger("Die");
            _impulse?.GenerateImpulse(new Vector3(0.4f, 0.3f, 0f));

            EventBus.Emit(new BossDefeatedEvent  { BossId = _bossId });
            EventBus.Emit(new EnemyDiedEvent     { EnemyId = _bossId, Exp = Data?.ExpReward ?? 500, Position = transform.position });
            EventBus.Emit(new ScreenFlashEvent   { Color = Color.white, Duration = 0.5f });

            // Drive dissolve slower for a dramatic death
            float t = 0f;
            const float DUR = 3f;
            while (t < DUR)
            {
                t += Time.deltaTime;
                float d = t / DUR;
                _mpb_pub.SetFloat("_DissolveAmount", d);
                _renderer.SetPropertyBlock(_mpb_pub);
                if (d > 0.3f) VFXManager.Instance?.PlayDeathBurst(Data?.DeathVFX, transform.position, Data?.DeathColor ?? Color.white);
                yield return null;
            }

            _phase = BossPhase.Dead;
            Destroy(gameObject);
        }

        // Local MPB for boss death sequence (EnemyBase._mpb is private)
        readonly MaterialPropertyBlock _mpb_pub = new MaterialPropertyBlock();

        // ── Shared hit application ────────────────────────────────────
        void ApplyHits(int hitCount, int damage)
        {
            if (hitCount == 0) return;
            for (int i = 0; i < hitCount; i++)
            {
                var pc = _overlapBuf[i].GetComponentInParent<Player.PlayerController>();
                pc?.TakeDamage(damage, DamageType.Physical);
            }
        }
    }
}
