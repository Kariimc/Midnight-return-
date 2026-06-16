using MidnightReturn.Utils;

namespace MidnightReturn.Player.States
{
    public class IdleState : PlayerState
    {
        public IdleState(PlayerController owner) : base(owner, "Idle") { }

        public override void OnEnter(State<PlayerController> prev)
        {
            Player.Animator.CrossFadeInFixedTime("Idle", 0.1f);
        }

        public override void OnUpdate(float dt)
        {
            if (!Movement.IsGrounded)         { Player.FSM.Transition("Fall");   return; }
            if (Input.HasAttack)              { Player.FSM.Transition("Attack"); return; }
            if (Input.HasJump)                { Player.FSM.Transition("Jump");   return; }
            if (Input.HasDash)                { Player.FSM.Transition("Dash");   return; }
            if (Input.IsPressingLeft ||
                Input.IsPressingRight)        { Player.FSM.Transition("Run");    return; }
        }
    }
}
