using System.Collections;
using UnityEngine;
using UnityEngine.VFX;
using MidnightReturn.Data;
using MidnightReturn.Systems;
using MidnightReturn.Utils;

namespace MidnightReturn.Enemies
{
    [RequireComponent(typeof(CharacterController))]
    public abstract class EnemyBase : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] protected EnemyDataSO Data;

        // Runtime
        public int Hp      { get; protected set; }
        public int Defense => Data != null ? Data.Defense : 0;
        public bool IsAlive => Hp > 0;

        protected Transform    _player;
        protected CharacterController _cc;
        protected Animator     _anim;
        protected VisualEffect _hitVFX;
        protected float        _attackCooldown;
        protected bool         _isDying;

        private MaterialPropertyBlock _mpb;
        private Renderer _renderer;

        // Cached
        protected Vector3 PlayerPos => _player != null ? _player.position : transform.position;
        protected float   DistToPlayer => Vector3.Distance(transform.position, PlayerPos);
        protected bool    CanSeePlayer => DistToPlayer < Data.DetectionRange;
        protected bool    InAttackRange => DistToPlayer < Data.AttackRange;

        protected virtual void Awake()
        {
            _cc       = GetComponent<CharacterController>();
            _anim     = GetComponent<Animator>();
            _renderer = GetComponentInChildren<Renderer>();
            _mpb      = new MaterialPropertyBlock();

            if (Data != null) Hp = Data.MaxHp;
        }

        protected virtual void Start()
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) _player = playerObj.transform;
        }

        private void Update()
        {
            if (_isDying) return;
            _attackCooldown = Mathf.Max(0f, _attackCooldown - Time.deltaTime);
            OnUpdate(Time.deltaTime);
        }

        protected abstract void OnUpdate(float dt);

        public int TakeDamage(int rawDamage, DamageType type = DamageType.Physical, bool isCrit = false)
        {
            if (_isDying) return 0;

            float res = Data?.GetResistance(type) ?? 0f;
            int dmg   = Mathf.Max(1, Mathf.FloorToInt(rawDamage * (1f - res / 100f)));
            Hp        = Mathf.Max(0, Hp - dmg);

            // Hit flash on material
            StartCoroutine(HitFlash());

            // Damage number
            VFXManager.Instance?.SpawnDamageNumber(
                transform.position + Vector3.up * 1.2f, dmg, isCrit
            );

            if (Data?.HitVFX != null)
                VFXManager.Instance?.PlayEnemyHitVFX(Data.HitVFX, transform.position);

            if (Hp <= 0) Die();
            return dmg;
        }

        protected virtual void Die()
        {
            if (_isDying) return;
            _isDying = true;

            _anim?.CrossFadeInFixedTime("Death", 0.1f);

            EventBus.Emit(new EnemyDiedEvent
            {
                EnemyId  = Data?.EnemyId ?? "unknown",
                Exp      = Data?.ExpReward ?? 10,
                Position = transform.position,
            });

            // HDRP Dissolve death
            StartCoroutine(DissolveAndDestroy());
        }

        private IEnumerator DissolveAndDestroy()
        {
            // Spawn death VFX burst
            if (Data?.DeathVFX != null)
                VFXManager.Instance?.PlayDeathBurst(Data.DeathVFX, transform.position, Data.DeathColor);

            // Drive dissolve shader _DissolveAmount from 0→1 over 1.2 seconds
            float t = 0f;
            const float DURATION = 1.2f;
            while (t < DURATION)
            {
                t += Time.deltaTime;
                float dissolve = t / DURATION;
                _renderer.GetPropertyBlock(_mpb);
                _mpb.SetFloat("_DissolveAmount", dissolve);
                _renderer.SetPropertyBlock(_mpb);
                yield return null;
            }

            Destroy(gameObject);
        }

        private IEnumerator HitFlash()
        {
            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetColor("_EmissiveColor", Color.white * 8f);
            _renderer.SetPropertyBlock(_mpb);
            yield return new WaitForSeconds(0.05f);
            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetColor("_EmissiveColor", Color.black);
            _renderer.SetPropertyBlock(_mpb);
        }

        // Shared movement helper — 2.5D plane locked
        protected void MoveToward(Vector3 target, float speed, float dt)
        {
            var dir  = (target - transform.position).normalized;
            dir.z    = 0f;
            float grav = _cc.isGrounded ? -0.5f : Physics.gravity.y * dt;
            var motion = new Vector3(dir.x * speed, grav, 0f) * dt;
            _cc.Move(motion);
            transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
        }
    }
}
