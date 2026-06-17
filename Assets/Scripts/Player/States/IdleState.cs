using MidnightReturn.Utils;

namespace MidnightReturn.Player.States
{
    public class IdleState : PlayerState
    {
        private int _prevFacingDir;

        public IdleState(PlayerController owner) : base(owner, "Idle") { }

        public override void OnEnter(State<PlayerController> prev)
        {
            Player.Animator.CrossFadeInFixedTime("Idle", 0.1f);
            _prevFacingDir = Movement.FacingDir;
        }

        public override void OnUpdate(float dt)
        {
            if (!Movement.IsGrounded)           { Player.FSM.Transition("Fall");       return; }
            if (Input.HasAttack)                { Player.FSM.Transition("Attack");     return; }
            if (Input.HasSpell)                 { Player.FSM.Transition("SubWeapon");  return; }
            if (Input.HasJump)                  { Player.FSM.Transition("Jump");       return; }
            if (Input.HasDash)                  { Player.FSM.Transition("Dash");       return; }

            // Mount a ladder: Up anywhere on it, or Down when standing on its top
            if (Movement.CanClimb && (Input.IsPressingUp ||
                (Input.IsPressingDown && Movement.AtLadderTop)))
                { Player.FSM.Transition("Climb"); return; }

            if (Input.IsPressingDown)           { Player.FSM.Transition("Crouch");     return; }

            if (Input.IsPressingLeft || Input.IsPressingRight)
            {
                // TurnAround if the pressed direction is opposite to current facing
                int desiredDir = Input.IsPressingRight ? 1 : -1;
                if (desiredDir != _prevFacingDir)
                {
                    Player.FSM.Transition("TurnAround");
                    return;
                }
                Player.FSM.Transition("Run");
            }
        }

        public override void OnExit(State<PlayerController> next)
        {
            _prevFacingDir = Movement.FacingDir;
        }
    }
}
