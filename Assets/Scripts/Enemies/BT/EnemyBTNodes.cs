using UnityEngine;
using MidnightReturn.AI;
using MidnightReturn.Enemies;
using MidnightReturn.Systems;

namespace MidnightReturn.AI.Nodes
{
    // ══════════════════════════════════════════════════════════════════
    //  FACTORY — builds per-enemy BT trees from cached references.
    //  All lambdas capture Transform / EnemyBase at build-time so
    //  Execute never calls GetComponent or allocates.
    // ══════════════════════════════════════════════════════════════════
    public static class EnemyBTFactory
    {
        // ── PATROL ENEMY ─────────────────────────────────────────────
        // Selector:
        //   Chase branch  → (IsAlerted OR CanSee) → Sequence(FacePlayer, MoveToPlayer, AttackIfInRange)
        //   Patrol branch → Sequence(PatrolToEdge, Pause)
        public static BTNode BuildPatrolTree(EnemyBase e, float patrolRadius, float alertRadius,
                                              float attackRange, float attackCooldown, float moveSpeed,
                                              System.Action onAttack)
        {
            Transform t = e.transform;
            float originX = t.position.x;

            return new Selector("PatrolRoot",

                // ── Chase branch ──
                new Sequence("ChaseBranch",
                    // Become alerted when player enters radius
                    new Action("CheckAlert", bb =>
                    {
                        bool alerted = bb.Get<bool>(BBKey.IsAlerted);
                        if (!alerted && bb.Get<float>(BBKey.DistToPlayer) < alertRadius)
                        {
                            bb.Set(BBKey.IsAlerted, true);
                            alerted = true;
                        }
                        return alerted ? BTStatus.Success : BTStatus.Failure;
                    }),
                    new Action("FacePlayer", bb =>
                    {
                        float dx = bb.Get<Vector3>(BBKey.PlayerPos).x - t.position.x;
                        t.localScale = new Vector3(dx >= 0f ? 1f : -1f, 1f, 1f);
                        return BTStatus.Success;
                    }),
                    new Action("ChasePlayer", bb =>
                    {
                        float dist = bb.Get<float>(BBKey.DistToPlayer);
                        if (dist < attackRange) return BTStatus.Success; // in range, done moving
                        Vector3 target = bb.Get<Vector3>(BBKey.PlayerPos);
                        e.MoveTowardPublic(target, moveSpeed, bb.Get(BBKey.DeltaTime, 0.02f));
                        return BTStatus.Running;
                    }),
                    // Attack with cooldown
                    new Cooldown(attackCooldown,
                        new Action("Attack", bb =>
                        {
                            if (bb.Get<float>(BBKey.DistToPlayer) > attackRange) return BTStatus.Failure;
                            onAttack?.Invoke();
                            return BTStatus.Success;
                        })
                    )
                ),

                // ── Patrol branch ──
                new Sequence("PatrolBranch",
                    new Action("Patrol", bb =>
                    {
                        float tx = bb.Get(BBKey.PatrolTarget, originX + patrolRadius);
                        float dt = bb.Get(BBKey.DeltaTime, 0.02f);
                        float cx = t.position.x;
                        int   dir = bb.Get(BBKey.PatrolDir, 1);

                        // Reached target → flip
                        if (Mathf.Abs(cx - tx) < 0.15f)
                        {
                            dir = -dir;
                            bb.Set(BBKey.PatrolDir, dir);
                            bb.Set(BBKey.PatrolTarget, originX + patrolRadius * dir);
                            return BTStatus.Success; // pause for one tick
                        }

                        t.localScale = new Vector3(dir >= 0 ? 1f : -1f, 1f, 1f);
                        e.MoveTowardPublic(new Vector3(tx, t.position.y, 0f), moveSpeed * .6f, dt);
                        return BTStatus.Running;
                    })
                )
            );
        }

        // ── RANGED ENEMY ─────────────────────────────────────────────
        // Selector:
        //   TooClose → retreat
        //   InRange  → FacePlayer → Telegraph → Shoot
        //   OutRange → MoveToward preferred range
        public static BTNode BuildRangedTree(EnemyBase e, float preferredRange, float minRange,
                                              float alertRadius, float attackCooldown, float moveSpeed,
                                              System.Action onShoot)
        {
            Transform t = e.transform;

            return new Selector("RangedRoot",
                // Retreat if player too close
                new Sequence("Retreat",
                    new Condition("TooClose", bb => bb.Get<float>(BBKey.DistToPlayer) < minRange),
                    new Action("MoveBack", bb =>
                    {
                        float dx = t.position.x - bb.Get<Vector3>(BBKey.PlayerPos).x;
                        var away = t.position + new Vector3(Mathf.Sign(dx) * moveSpeed, 0f, 0f);
                        e.MoveTowardPublic(away, moveSpeed, bb.Get(BBKey.DeltaTime, 0.02f));
                        return BTStatus.Running;
                    })
                ),
                // Shoot if in preferred range
                new Sequence("ShootBranch",
                    new Condition("InRange", bb =>
                    {
                        float d = bb.Get<float>(BBKey.DistToPlayer);
                        return d >= minRange && d <= preferredRange * 1.3f;
                    }),
                    new Action("FacePlayer", bb =>
                    {
                        float dx = bb.Get<Vector3>(BBKey.PlayerPos).x - t.position.x;
                        t.localScale = new Vector3(dx >= 0f ? 1f : -1f, 1f, 1f);
                        return BTStatus.Success;
                    }),
                    new Cooldown(attackCooldown,
                        new Sequence("TelegraphAndShoot",
                            // Telegraph: glow for 0.4s before firing
                            new Action("Telegraph", bb =>
                            {
                                float timer = bb.Get(BBKey.TelegraphTimer, 0f);
                                timer += bb.Get(BBKey.DeltaTime, 0.02f);
                                bb.Set(BBKey.TelegraphTimer, timer);
                                if (timer < 0.4f) return BTStatus.Running;
                                bb.Remove(BBKey.TelegraphTimer);
                                return BTStatus.Success;
                            }),
                            new Action("Shoot", bb => { onShoot?.Invoke(); return BTStatus.Success; })
                        )
                    )
                ),
                // Move toward preferred range
                new Action("Approach", bb =>
                {
                    Vector3 target = bb.Get<Vector3>(BBKey.PlayerPos);
                    Vector3 dir    = (target - t.position).normalized;
                    Vector3 dest   = target - dir * preferredRange;
                    e.MoveTowardPublic(dest, moveSpeed * .8f, bb.Get(BBKey.DeltaTime, 0.02f));
                    return BTStatus.Running;
                })
            );
        }

        // ── FLYING ENEMY ─────────────────────────────────────────────
        // Hover with sin-wave bob, dive when alerted, recover
        public static BTNode BuildFlyingTree(EnemyBase e, float alertRadius, float diveRange,
                                              float moveSpeed, System.Action onDiveAttack)
        {
            Transform t = e.transform;
            float _bobPhase = 0f;
            const float BOB_AMP = 0.3f, BOB_FREQ = 2f;

            return new Selector("FlyRoot",
                // Dive attack when very close
                new Sequence("DiveBranch",
                    new Condition("Alerted",  bb => bb.Get<bool>(BBKey.IsAlerted)),
                    new Condition("DiveRange", bb => bb.Get<float>(BBKey.DistToPlayer) < diveRange),
                    new Cooldown(2.2f,
                        new Action("Dive", bb =>
                        {
                            Vector3 target = bb.Get<Vector3>(BBKey.PlayerPos);
                            e.MoveTowardPublic(target, moveSpeed * 2.2f, bb.Get(BBKey.DeltaTime, 0.02f));
                            if (bb.Get<float>(BBKey.DistToPlayer) < 0.8f)
                            {
                                onDiveAttack?.Invoke();
                                return BTStatus.Success;
                            }
                            return BTStatus.Running;
                        })
                    )
                ),
                // Alert + chase
                new Sequence("ChaseBranch",
                    new Action("CheckAlert", bb =>
                    {
                        if (!bb.Get<bool>(BBKey.IsAlerted) &&
                            bb.Get<float>(BBKey.DistToPlayer) < alertRadius)
                            bb.Set(BBKey.IsAlerted, true);
                        return bb.Get<bool>(BBKey.IsAlerted) ? BTStatus.Success : BTStatus.Failure;
                    }),
                    new Action("ChaseWithBob", bb =>
                    {
                        float dt = bb.Get(BBKey.DeltaTime, 0.02f);
                        _bobPhase += dt * BOB_FREQ;
                        Vector3 target = bb.Get<Vector3>(BBKey.PlayerPos)
                                       + Vector3.up * (2f + BOB_AMP * Mathf.Sin(_bobPhase));
                        e.MoveTowardPublic(target, moveSpeed, dt);
                        t.position = new Vector3(t.position.x, t.position.y, 0f);
                        return BTStatus.Running;
                    })
                ),
                // Idle hover
                new Action("IdleHover", bb =>
                {
                    float dt = bb.Get(BBKey.DeltaTime, 0.02f);
                    _bobPhase += dt * BOB_FREQ;
                    float bobY = BOB_AMP * Mathf.Sin(_bobPhase);
                    t.position += new Vector3(0f, bobY * dt, 0f);
                    t.position  = new Vector3(t.position.x, t.position.y, 0f);
                    return BTStatus.Running;
                })
            );
        }
    }
}
