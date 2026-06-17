using MidnightReturn.Utils;

namespace MidnightReturn.Player.States
{
    // Slow-tilt walk — active when horizontal axis is below the run threshold.
    // Primarily meaningful on gamepad (analog stick at partial travel).
    // On keyboard MoveAxis.x is always ±1, so this state is bypassed in practice.
    public class WalkState : PlayerState
    {
        private const float RUN_THRESHOLD = 0.6f;

        public WalkState(PlayerController owner) : base(owner, "Walk") { }

        public override void OnEnter(State<PlayerController> prev)
        {
            Player.Animator.CrossFadeInFixedTime("Walk", 0.1f);
        }

        public override void OnUpdate(float dt)
        {
            if (!Movement.IsGrounded)                              { Player.FSM.Transition("Fall");    return; }
            if (Input.HasAttack)                                   { Player.FSM.Transition("Attack");  return; }
            if (Input.HasJump)                                     { Player.FSM.Transition("Jump");    return; }
            if (Input.HasDash)                                     { Player.FSM.Transition("Dash");    return; }
            if (Input.HasSpell)                                    { Player.FSM.Transition("SubWeapon"); return; }
            if (Input.IsPressingDown)                              { Player.FSM.Transition("Crouch");  return; }
            if (!Input.IsPressingLeft && !Input.IsPressingRight)   { Player.FSM.Transition("Idle");    return; }

            // Promote to Run once the stick is pushed far enough.
            if (UnityEngine.Mathf.Abs(Input.MoveAxis.x) >= RUN_THRESHOLD)
                { Player.FSM.Transition("Run"); return; }
        }
    }
}
