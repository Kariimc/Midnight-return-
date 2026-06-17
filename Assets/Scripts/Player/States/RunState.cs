using UnityEngine;
using MidnightReturn.Utils;

namespace MidnightReturn.Player.States
{
    public class RunState : PlayerState
    {
        private const float WALK_THRESHOLD = 0.6f;
        private int _prevFacingDir;

        public RunState(PlayerController owner) : base(owner, "Run") { }

        public override void OnEnter(State<PlayerController> prev)
        {
            Player.Animator.CrossFadeInFixedTime("Run", 0.08f);
            _prevFacingDir = Movement.FacingDir;
        }

        public override void OnUpdate(float dt)
        {
            if (!Movement.IsGrounded)                              { Player.FSM.Transition("Fall");       return; }
            if (Input.HasAttack)                                   { Player.FSM.Transition("Attack");     return; }
            if (Input.HasSpell)                                    { Player.FSM.Transition("SubWeapon");  return; }
            if (Input.HasJump)                                     { Player.FSM.Transition("Jump");       return; }
            if (Input.HasDash)                                     { Player.FSM.Transition("Dash");       return; }
            if (Movement.CanClimb && Input.IsPressingUp)           { Player.FSM.Transition("Climb");      return; }
            if (Input.IsPressingDown)                              { Player.FSM.Transition("Crouch");     return; }
            if (!Input.IsPressingLeft && !Input.IsPressingRight)   { Player.FSM.Transition("Idle");       return; }

            // TurnAround when input direction reverses
            int desiredDir = Input.IsPressingRight ? 1 : -1;
            if (desiredDir != _prevFacingDir)
            {
                _prevFacingDir = desiredDir;
                Player.FSM.Transition("TurnAround");
                return;
            }

            // Demote to Walk on analog partial input (gamepad only meaningful)
            if (Mathf.Abs(Input.MoveAxis.x) < WALK_THRESHOLD)
            {
                Player.FSM.Transition("Walk");
                return;
            }

            Player.UpdateRunDust(dt);
        }

        public override void OnExit(State<PlayerController> next)
        {
            _prevFacingDir = Movement.FacingDir;
        }
    }
}
