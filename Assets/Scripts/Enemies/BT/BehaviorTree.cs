using System;
using System.Collections.Generic;

namespace MidnightReturn.AI
{
    public enum BTStatus { Success, Failure, Running }

    // ══════════════════════════════════════════════════════════════════
    //  BASE NODE
    // ══════════════════════════════════════════════════════════════════
    public abstract class BTNode
    {
        public readonly string Name;
        protected BTNode(string name) => Name = name;
        public abstract BTStatus Execute(BTBlackboard bb);
        public virtual void Reset() { }
    }

    // ══════════════════════════════════════════════════════════════════
    //  COMPOSITES
    // ══════════════════════════════════════════════════════════════════

    // Sequence — AND: fails on first Failure, succeeds when all succeed.
    // Resumes from the running child on the next tick.
    public sealed class Sequence : BTNode
    {
        private readonly BTNode[] _children;
        private int _idx;

        public Sequence(string name, params BTNode[] children) : base(name)
            => _children = children;

        public override BTStatus Execute(BTBlackboard bb)
        {
            while (_idx < _children.Length)
            {
                var s = _children[_idx].Execute(bb);
                if (s == BTStatus.Failure) { Reset(); return BTStatus.Failure; }
                if (s == BTStatus.Running)  return BTStatus.Running;
                _idx++;
            }
            Reset();
            return BTStatus.Success;
        }

        public override void Reset()
        {
            _idx = 0;
            foreach (var c in _children) c.Reset();
        }
    }

    // Selector — OR: succeeds on first Success, fails when all fail.
    public sealed class Selector : BTNode
    {
        private readonly BTNode[] _children;
        private int _idx;

        public Selector(string name, params BTNode[] children) : base(name)
            => _children = children;

        public override BTStatus Execute(BTBlackboard bb)
        {
            while (_idx < _children.Length)
            {
                var s = _children[_idx].Execute(bb);
                if (s == BTStatus.Success) { Reset(); return BTStatus.Success; }
                if (s == BTStatus.Running)  return BTStatus.Running;
                _idx++;
            }
            Reset();
            return BTStatus.Failure;
        }

        public override void Reset()
        {
            _idx = 0;
            foreach (var c in _children) c.Reset();
        }
    }

    // Parallel — runs ALL children every tick.
    // Returns Success if >= successThreshold children succeed.
    // Returns Failure if too many fail to ever meet threshold.
    public sealed class Parallel : BTNode
    {
        private readonly BTNode[] _children;
        private readonly int _threshold;

        public Parallel(string name, int successThreshold, params BTNode[] children) : base(name)
        {
            _children  = children;
            _threshold = successThreshold < 0 ? children.Length : successThreshold;
        }

        // Convenience: require all
        public Parallel(string name, params BTNode[] children)
            : this(name, children.Length, children) { }

        public override BTStatus Execute(BTBlackboard bb)
        {
            int ok = 0, fail = 0;
            foreach (var c in _children)
            {
                var s = c.Execute(bb);
                if (s == BTStatus.Success) ok++;
                else if (s == BTStatus.Failure) fail++;
            }
            if (ok  >= _threshold)                  return BTStatus.Success;
            if (fail > _children.Length - _threshold) return BTStatus.Failure;
            return BTStatus.Running;
        }
    }

    // ══════════════════════════════════════════════════════════════════
    //  DECORATORS
    // ══════════════════════════════════════════════════════════════════

    public sealed class Inverter : BTNode
    {
        private readonly BTNode _child;
        public Inverter(BTNode child) : base("Inverter") => _child = child;

        public override BTStatus Execute(BTBlackboard bb)
        {
            var s = _child.Execute(bb);
            return s switch
            {
                BTStatus.Success => BTStatus.Failure,
                BTStatus.Failure => BTStatus.Success,
                _                => BTStatus.Running,
            };
        }
        public override void Reset() => _child.Reset();
    }

    // Cooldown — blocks child until interval elapses (wall-clock via Blackboard time key).
    public sealed class Cooldown : BTNode
    {
        private readonly BTNode _child;
        private readonly float  _interval;
        private float _elapsed;

        public Cooldown(float interval, BTNode child) : base($"Cooldown({interval:F1}s)")
        { _interval = interval; _child = child; _elapsed = interval; }

        public override BTStatus Execute(BTBlackboard bb)
        {
            float dt = bb.Get(BBKey.DeltaTime, 0.02f);
            _elapsed += dt;
            if (_elapsed < _interval) return BTStatus.Failure;
            var s = _child.Execute(bb);
            if (s != BTStatus.Running) _elapsed = 0f;
            return s;
        }
        public override void Reset() { _elapsed = 0f; _child.Reset(); }
    }

    // RepeatUntilFail — re-runs child until it returns Failure (or maxReps hit).
    public sealed class RepeatUntilFail : BTNode
    {
        private readonly BTNode _child;
        private readonly int    _maxReps;
        private int _reps;

        public RepeatUntilFail(BTNode child, int maxReps = -1) : base("Repeat")
        { _child = child; _maxReps = maxReps; }

        public override BTStatus Execute(BTBlackboard bb)
        {
            if (_maxReps >= 0 && _reps >= _maxReps) { Reset(); return BTStatus.Success; }
            var s = _child.Execute(bb);
            if (s == BTStatus.Failure) { Reset(); return BTStatus.Failure; }
            _reps++;
            return BTStatus.Running;
        }
        public override void Reset() { _reps = 0; _child.Reset(); }
    }

    // ══════════════════════════════════════════════════════════════════
    //  LEAF SHORTCUTS
    // ══════════════════════════════════════════════════════════════════

    // Lambda condition — returns Success/Failure based on a predicate.
    public sealed class Condition : BTNode
    {
        private readonly Func<BTBlackboard, bool> _pred;
        public Condition(string name, Func<BTBlackboard, bool> pred) : base(name) => _pred = pred;
        public override BTStatus Execute(BTBlackboard bb)
            => _pred(bb) ? BTStatus.Success : BTStatus.Failure;
    }

    // Lambda action — full control over BTStatus return.
    public sealed class Action : BTNode
    {
        private readonly Func<BTBlackboard, BTStatus> _act;
        public Action(string name, Func<BTBlackboard, BTStatus> act) : base(name) => _act = act;
        public override BTStatus Execute(BTBlackboard bb) => _act(bb);
    }

    // ══════════════════════════════════════════════════════════════════
    //  BLACKBOARD
    // ══════════════════════════════════════════════════════════════════
    public sealed class BTBlackboard
    {
        private readonly Dictionary<string, object> _data = new(32);

        public void Set<T>(string key, T value) => _data[key] = value;

        public T Get<T>(string key, T fallback = default)
        {
            if (_data.TryGetValue(key, out var v) && v is T t) return t;
            return fallback;
        }

        public bool Has(string key) => _data.ContainsKey(key);
        public void Remove(string key) => _data.Remove(key);
    }

    // ══════════════════════════════════════════════════════════════════
    //  BLACKBOARD KEY CONSTANTS
    // ══════════════════════════════════════════════════════════════════
    public static class BBKey
    {
        // Written each FixedUpdate by EnemyAIController before Execute
        public const string DeltaTime       = "dt";
        public const string PlayerPos       = "player_pos";
        public const string SelfPos         = "self_pos";
        public const string DistToPlayer    = "dist_to_player";
        public const string HpNormalized    = "hp_norm";
        public const string IsGrounded      = "is_grounded";

        // Written by nodes
        public const string PatrolDir       = "patrol_dir";     // int: -1 or 1
        public const string PatrolTarget    = "patrol_target";  // float: world X
        public const string IsAlerted       = "is_alerted";     // bool
        public const string AttackCooldown  = "atk_cd";        // float: time remaining
        public const string IsTelegraphing  = "telegraphing";   // bool
        public const string TelegraphTimer  = "telegraph_t";    // float
        public const string CurrentPattern  = "pattern";        // BossAttackType enum (int)
        public const string PhaseIndex      = "phase_idx";      // int: 0/1/2
    }
}
