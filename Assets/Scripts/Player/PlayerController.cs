using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using MidnightReturn.Utils;
using MidnightReturn.Systems;
using MidnightReturn.Player.States;
using MidnightReturn.Core;

namespace MidnightReturn.Player
{
    // Root MonoBehaviour — orchestrates all player subsystems
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(PlayerCombat))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour
    {
        // ── Subsystem references (wired by Unity Inspector) ───────────────────
        [HideInInspector] public PlayerMovement    Movement;
        [HideInInspector] public PlayerInputHandler Input;
        [HideInInspector] public PlayerCombat       Combat;
        [HideInInspector] public Animator           Animator;

        public  StateMachine<PlayerController>     FSM { get; private set; }

        // ── HDRP-specific components ──────────────────────────────────────────
        [Header("HDRP VFX")]
        [SerializeField] private GameObject  _afterimagePrefab;
        [SerializeField] private float       _afterimageInterval = 0.04f;
        [SerializeField] private TrailRenderer _weaponTrail;

        [Header("Post Processing")]
        [SerializeField] private Volume _localPostProcessVolume; // for player-specific PP

        // ── RPG state ─────────────────────────────────────────────────────────
        public StatBlock Stats { get; private set; }

        private float _invincibleTimer;
        private const float INVINCIBLE_DURATION = 1.0f;
        private Coroutine _afterimageCoroutine;
        private float _runDustTimer;

        // ── Dust step interval ────────────────────────────────────────────────
        private const float RUN_DUST_INTERVAL = 0.22f;

        private void Awake()
        {
            Movement = GetComponent<PlayerMovement>();
            Input    = GetComponent<PlayerInputHandler>();
            Combat   = GetComponent<PlayerCombat>();
            Animator = GetComponent<Animator>();
            Stats    = new StatBlock();

            BuildFSM();
        }

        private void BuildFSM()
        {
            FSM = new StateMachine<PlayerController>();
            FSM.Add(new IdleState(this))
               .Add(new RunState(this))
               .Add(new JumpState(this))
               .Add(new FallState(this))
               .Add(new DashState(this))
               .Add(new WallSlideState(this))
               .Add(new AttackState(this));
            FSM.Transition("Idle");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<PlayerDamagedEvent>(OnDamaged);
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyKilled);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerDamagedEvent>(OnDamaged);
            EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyKilled);
        }

        private void Update()
        {
            if (GameManager.Instance.IsPaused) return;
            FSM.Update(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (GameManager.Instance.IsPaused) return;
            Movement.Tick(Time.fixedDeltaTime, Input);
            FSM.FixedUpdate(Time.fixedDeltaTime);

            // Flip character to face direction
            var scale = transform.localScale;
            scale.x   = Movement.FacingDir;
            transform.localScale = scale;

            // Tick invincibility flash
            if (_invincibleTimer > 0f)
            {
                _invincibleTimer -= Time.fixedDeltaTime;
                // Flash by toggling renderer alpha — done via material property block
                ToggleInvincibilityFlash();
            }
        }

        // ── Damage / Healing ──────────────────────────────────────────────────
        public void TakeDamage(int rawDamage, Data.DamageType type = Data.DamageType.Physical)
        {
            if (_invincibleTimer > 0f) return;

            float res = GetResistance(type);
            int dmg   = Mathf.Max(1, Mathf.FloorToInt(rawDamage * (1f - res / 100f) - Stats.Def * 0.3f));
            Stats.Hp  = Mathf.Max(0, Stats.Hp - dmg);
            _invincibleTimer = INVINCIBLE_DURATION;

            EventBus.Emit(new PlayerDamagedEvent { Damage = dmg, CurrentHp = Stats.Hp });
            VFXManager.Instance?.SpawnHitFlash(transform.position, Color.red);
            EventBus.Emit(new ScreenFlashEvent { Color = new Color(1f, 0f, 0f, 0.3f), Duration = 0.15f });

            if (Stats.Hp <= 0) Die();
        }

        public void Heal(int amount)
        {
            Stats.Hp = Mathf.Min(Stats.MaxHp, Stats.Hp + amount);
            VFXManager.Instance?.SpawnHealBurst(transform.position);
        }

        public void RestoreMp(int amount)
        {
            Stats.Mp = Mathf.Min(Stats.MaxMp, Stats.Mp + amount);
        }

        public void RestoreAtStatue()
        {
            Stats.Hp = Stats.MaxHp;
            Stats.Mp = Mathf.Min(Stats.MaxMp, Stats.Mp + Stats.MaxMp / 2);
            EventBus.Emit(new PlayerDamagedEvent { Damage = 0, CurrentHp = Stats.Hp });
        }

        private void Die()
        {
            FSM.Lock();
            Animator.CrossFadeInFixedTime("Death", 0.1f);
            VFXManager.Instance?.SpawnDeathDissolve(gameObject);
            EventBus.Emit(new PlayerDiedEvent());
        }

        // ── Event handlers ────────────────────────────────────────────────────
        private void OnDamaged(PlayerDamagedEvent e)
        {
            // Handled by UI — nothing needed here
        }

        private void OnEnemyKilled(EnemyDiedEvent e)
        {
            bool leveled = PlayerStatsSystem.AddExp(Stats, e.Exp);
            if (leveled)
            {
                EventBus.Emit(new PlayerLeveledUpEvent { Level = Stats.Level });
                VFXManager.Instance?.SpawnLevelUpBurst(transform.position);
                EventBus.Emit(new ScreenFlashEvent { Color = new Color(1f, 0.9f, 0f, 0.5f), Duration = 0.4f });
            }
        }

        // ── HDRP Afterimage Trail ─────────────────────────────────────────────
        public void StartAfterimageTrail()
        {
            if (_afterimagePrefab == null) return;
            _afterimageCoroutine = StartCoroutine(AfterimageLoop());
        }

        public void StopAfterimageTrail()
        {
            if (_afterimageCoroutine != null) StopCoroutine(_afterimageCoroutine);
        }

        private IEnumerator AfterimageLoop()
        {
            while (true)
            {
                // Instantiate afterimage with dissolve shader — fades over 0.3s
                var img = Instantiate(_afterimagePrefab, transform.position, transform.rotation);
                img.transform.localScale = transform.localScale;
                VFXManager.Instance?.FadeOutAfterimage(img, 0.3f);
                yield return new WaitForSeconds(_afterimageInterval);
            }
        }

        public void SetInvincible(bool value)
        {
            if (value) _invincibleTimer = INVINCIBLE_DURATION;
            else       _invincibleTimer = 0f;
        }

        public void UpdateRunDust(float dt)
        {
            _runDustTimer += dt;
            if (_runDustTimer >= RUN_DUST_INTERVAL)
            {
                _runDustTimer = 0f;
                VFXManager.Instance?.SpawnRunDust(
                    transform.position + Vector3.down * 0.9f,
                    Movement.FacingDir
                );
            }
        }

        private float _resistanceCache = 0f;
        private float GetResistance(Data.DamageType type)
        {
            return type switch
            {
                Data.DamageType.Fire      => Stats.FireRes,
                Data.DamageType.Ice       => Stats.IceRes,
                Data.DamageType.Lightning => Stats.LightningRes,
                Data.DamageType.Dark      => Stats.DarkRes,
                Data.DamageType.Holy      => Stats.HolyRes,
                Data.DamageType.Poison    => Stats.PoisonRes,
                _ => 0f,
            };
        }

        private MaterialPropertyBlock _mpb;
        private Renderer _renderer;
        private void ToggleInvincibilityFlash()
        {
            if (_renderer == null) _renderer = GetComponentInChildren<Renderer>();
            if (_mpb       == null) _mpb      = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(_mpb);
            float flash = Mathf.PingPong(_invincibleTimer * 10f, 1f) > 0.5f ? 1f : 0.3f;
            _mpb.SetFloat("_Alpha", flash);
            _renderer.SetPropertyBlock(_mpb);
        }
    }
}
