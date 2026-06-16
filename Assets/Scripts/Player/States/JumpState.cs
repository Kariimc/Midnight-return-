using MidnightReturn.Utils;

namespace MidnightReturn.Player.States
{
    public class JumpState : PlayerState
    {
        public JumpState(PlayerController owner) : base(owner, "Jump") { }

        public override void OnEnter(State<PlayerController> prev)
        {
            Player.Animator.CrossFadeInFixedTime("Jump", 0.05f);
        }

        public override void OnUpdate(float dt)
        {
            if (Movement.IsGrounded)      { Player.FSM.Transition("Idle");   return; }
            if (Input.HasAttack)           { Player.FSM.Transition("Attack"); return; }
            if (Input.HasDash)             { Player.FSM.Transition("Dash");   return; }
            if (Movement.Velocity.y < -1f) { Player.FSM.Transition("Fall");   return; }
        }
    }
}
