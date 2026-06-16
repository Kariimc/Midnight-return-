using UnityEngine;
using MidnightReturn.AI;
using MidnightReturn.AI.Nodes;
using MidnightReturn.Data;

namespace MidnightReturn.Enemies
{
    // ══════════════════════════════════════════════════════════════════
    //  EnemyAIController
    //  A concrete EnemyBase subclass that drives behavior via a
    //  BehaviorTree, built once on Start from inspector-configured
    //  parameters. Coexists with PatrolEnemy/FlyingEnemy/RangedEnemy —
    //  those classes are untouched.
    // ══════════════════════════════════════════════════════════════════
    public enum EnemyAIType { Patrol, Ranged, Flying }

    public sealed class EnemyAIController : EnemyBase
    {
        [Header("AI Config")]
        [SerializeField] EnemyAIType _aiType = EnemyAIType.Patrol;
        [SerializeField] float _patrolRadius  = 4f;
        [SerializeField] float _alertRadius   = 7f;
        [SerializeField] float _attackRange   = 1.2f;
        [SerializeField] float _preferredRange= 6f;   // ranged only
        [SerializeField] float _minRange      = 2.5f;  // ranged only
        [SerializeField] float _moveSpeed     = 3.5f;
        [SerializeField] float _attackCooldown= 1.4f;

        BTBlackboard _bb;
        BTNode       _root;

        protected override void Start()
        {
            base.Start();
            _bb = new BTBlackboard();
            _root = BuildTree();
        }

        protected override void OnUpdate(float dt)
        {
            if (_root == null || _player == null) return;

            // Write fresh world-state to blackboard before Execute
            _bb.Set(BBKey.DeltaTime,    dt);
            _bb.Set(BBKey.PlayerPos,    _player.position);
            _bb.Set(BBKey.SelfPos,      transform.position);
            _bb.Set(BBKey.DistToPlayer, DistToPlayer);
            _bb.Set(BBKey.HpNormalized, Data != null ? (float)Hp / Data.MaxHp : 1f);
            _bb.Set(BBKey.IsGrounded,   _cc.isGrounded);

            _root.Execute(_bb);
        }

        // Public shim so BT lambdas can call MoveToward (which is protected)
        public void MoveTowardPublic(Vector3 target, float speed, float dt)
            => MoveToward(target, speed, dt);

        BTNode BuildTree() => _aiType switch
        {
            EnemyAIType.Patrol  => EnemyBTFactory.BuildPatrolTree(
                                        this, _patrolRadius, _alertRadius,
                                        _attackRange, _attackCooldown, _moveSpeed,
                                        OnAttack),
            EnemyAIType.Ranged  => EnemyBTFactory.BuildRangedTree(
                                        this, _preferredRange, _minRange,
                                        _alertRadius, _attackCooldown, _moveSpeed,
                                        OnShoot),
            EnemyAIType.Flying  => EnemyBTFactory.BuildFlyingTree(
                                        this, _alertRadius, _attackRange,
                                        _moveSpeed, OnAttack),
            _                   => EnemyBTFactory.BuildPatrolTree(
                                        this, _patrolRadius, _alertRadius,
                                        _attackRange, _attackCooldown, _moveSpeed,
                                        OnAttack),
        };

        // ── Attack callbacks (override in subclasses or extend here) ──
        void OnAttack()
        {
            _anim?.SetTrigger("Attack");
            // Hitbox is spawned via Animator event → PlayerCombat pattern
        }

        void OnShoot()
        {
            _anim?.SetTrigger("Attack");
            // Instantiate a Projectile from Data.ProjectilePrefab (set in EnemyDataSO)
            // Handled by AnimationEvent → SpawnProjectile()
        }
    }
}
