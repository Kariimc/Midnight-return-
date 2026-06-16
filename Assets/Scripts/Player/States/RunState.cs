using MidnightReturn.Utils;

namespace MidnightReturn.Player.States
{
    public class RunState : PlayerState
    {
        public RunState(PlayerController owner) : base(owner, "Run") { }

        public override void OnEnter(State<PlayerController> prev)
        {
            Player.Animator.CrossFadeInFixedTime("Run", 0.08f);
        }

        public override void OnUpdate(float dt)
        {
            if (!Movement.IsGrounded)                       { Player.FSM.Transition("Fall");   return; }
            if (Input.HasAttack)                            { Player.FSM.Transition("Attack"); return; }
            if (Input.HasJump)                              { Player.FSM.Transition("Jump");   return; }
            if (Input.HasDash)                              { Player.FSM.Transition("Dash");   return; }
            if (!Input.IsPressingLeft && !Input.IsPressingRight) { Player.FSM.Transition("Idle");   return; }

            // Footstep dust VFX at run speed (event driven)
            Player.UpdateRunDust(dt);
        }
    }
}
