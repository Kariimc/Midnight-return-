using UnityEngine;
using MidnightReturn.Utils;
using MidnightReturn.Systems;
using MidnightReturn.VerticalSlice;

namespace MidnightReturn.Player.States
{
    // Soul Tether — Up+Spell while airborne latches a spectral chain onto the nearest
    // TetherAnchor above the player and yanks them toward it.  Invincible during pull.
    // Falls back to SubWeapon if no anchor is in range.
    public class SoulTetherState : PlayerState
    {
        private const float YANK_SPEED   = 26f;  // units/s pull speed
        private const float CHAIN_RANGE  = 14f;  // OverlapSphere radius for anchor search
        private const float MIN_DURATION = 0.12f; // minimum travel time before arrival test
        private const float MAX_DURATION = 0.5f;  // hard timeout

        private static readonly Collider[] _anchorBuffer = new Collider[8];

        private Transform _anchor;
        private float     _timer;

        public SoulTetherState(PlayerController owner) : base(owner, "SoulTether") { }

        public override void OnEnter(State<PlayerController> prev)
        {
            Input.ConsumeSpell();
            _anchor = FindNearestAnchor();
            _timer  = 0f;

            if (_anchor == null)
            {
                // No anchor — hand off to SubWeapon as normal spell cast
                Player.FSM.Transition("SubWeapon");
                return;
            }

            Player.Animator.CrossFadeInFixedTime("Jump", 0.05f);
            Player.SetInvincible(true);

            EventBus.Emit(new ScreenFlashEvent
            {
                Color    = new Color(0.4f, 0.1f, 1f, 0.35f),
                Duration = 0.2f,
            });
            EventBus.Emit(new SoulTetherLatchedEvent { AnchorPos = _anchor.position });
            VFXManager.Instance?.SpawnDashBurst(Player.transform.position, Movement.FacingDir);
        }

        public override void OnUpdate(float dt)
        {
            if (_anchor == null) { Player.FSM.Transition("Fall"); return; }

            _timer += dt;

            Vector3 toAnchor = _anchor.position - Player.transform.position;
            float   dist     = toAnchor.magnitude;

            bool arrived = dist < 0.6f || (_timer > MIN_DURATION && dist < 1.8f);
            bool timedOut = _timer > MAX_DURATION;

            if (arrived || timedOut)
            {
                Player.FSM.Transition(Movement.IsGrounded ? "Idle" : "Fall");
                return;
            }

            // Override velocity — pull player toward anchor each frame
            Vector2 dir = new Vector2(toAnchor.x, toAnchor.y).normalized;
            Movement.SetDiveVelocity(dir.x * YANK_SPEED, dir.y * YANK_SPEED);
        }

        public override void OnExit(State<PlayerController> next)
        {
            Player.SetInvincible(false);
            _anchor = null;
        }

        private Transform FindNearestAnchor()
        {
            int count = Physics.OverlapSphereNonAlloc(
                Player.transform.position, CHAIN_RANGE,
                _anchorBuffer, ~0);

            Transform best    = null;
            float     bestDist = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                if (_anchorBuffer[i] == null) continue;
                if (!_anchorBuffer[i].TryGetComponent<TetherAnchor>(out _)) continue;
                // Prefer anchors that are above the player
                if (_anchorBuffer[i].transform.position.y < Player.transform.position.y - 0.3f) continue;
                float d = Vector3.Distance(Player.transform.position, _anchorBuffer[i].transform.position);
                if (d < bestDist) { bestDist = d; best = _anchorBuffer[i].transform; }
            }
            return best;
        }
    }
}
