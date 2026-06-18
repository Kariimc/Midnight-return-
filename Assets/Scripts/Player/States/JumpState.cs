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
            // Down+Spell airborne + Hourglass unlocked → time rewind
            if (Input.HasSpell && Input.IsPressingDown && Movement.CanHourglass && Player.HourglassCooldown <= 0f)
                { Player.FSM.Transition("Hourglass"); return; }
            // Up+Spell while airborne → Soul Tether fires chain to nearest anchor
            if (Input.HasSpell && Input.IsPressingUp && Movement.CanSoulTether)
                { Player.FSM.Transition("SoulTether"); return; }
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
