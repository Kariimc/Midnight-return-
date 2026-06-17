using UnityEngine;
using MidnightReturn.Utils;

namespace MidnightReturn.Player.States
{
    // 3-frame cosmetic pivot — plays when the player reverses horizontal direction
    // while grounded. Very short; exits immediately if the player attacks or jumps.
    public class TurnAroundState : PlayerState
    {
        private const float DURATION = 0.08f; // ~5 frames at 60fps
        private float _timer;

        public TurnAroundState(PlayerController owner) : base(owner, "TurnAround") { }

        public override void OnEnter(State<PlayerController> prev)
        {
            _timer = DURATION;
            Player.Animator.CrossFadeInFixedTime("TurnAround", 0.03f);
        }

        public override void OnUpdate(float dt)
        {
            // Early exits — don't block combat or jumps
            if (!Movement.IsGrounded)  { Player.FSM.Transition("Fall");   return; }
            if (Input.HasAttack)       { Player.FSM.Transition("Attack"); return; }
            if (Input.HasJump)         { Player.FSM.Transition("Jump");   return; }
            if (Input.HasDash)         { Player.FSM.Transition("Dash");   return; }

            _timer -= dt;
            if (_timer <= 0f)
            {
                Player.FSM.Transition(
                    (Input.IsPressingLeft || Input.IsPressingRight) ? "Run" : "Idle");
            }
        }
    }
}
