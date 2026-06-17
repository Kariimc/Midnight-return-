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
            if (Movement.IsGrounded)      { Player.FSM.Transition("Idle");       return; }
            if (Input.HasDash)             { Player.FSM.Transition("Dash");       return; }
            if (Input.HasSpell)            { Player.FSM.Transition("SubWeapon");  return; }
            if (Movement.CanClimb && (Input.IsPressingUp || Input.IsPressingDown))
                { Player.FSM.Transition("Climb"); return; }
            if (Movement.Velocity.y < -1f) { Player.FSM.Transition("Fall");       return; }

            // Down+Attack → DragonKick dive
            if (Input.HasAttack && Input.IsPressingDown)
                { Player.FSM.Transition("DragonKick"); return; }

            // Regular aerial attack
            if (Input.HasAttack)
                { Player.FSM.Transition("AirAttack"); return; }
        }
    }
}
