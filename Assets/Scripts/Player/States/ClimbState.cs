using UnityEngine;
using MidnightReturn.Utils;

namespace MidnightReturn.Player.States
{
    // Ladder / chain climbing. Gravity off, vertical control from MoveAxis.y.
    // Enter from grounded states (press Up at a ladder) or airborne (grab on).
    // Exit: leave the volume, reach the ground at the bottom, jump off, or
    // dismount over the top.
    public class ClimbState : PlayerState
    {
        public ClimbState(PlayerController owner) : base(owner, "Climb") { }

        public override void OnEnter(State<PlayerController> prev)
        {
            Movement.SetClimbing(true);
            Player.Animator.CrossFadeInFixedTime("Climb", 0.08f);
        }

        public override void OnUpdate(float dt)
        {
            // Left the ladder volume
            if (!Movement.CanClimb)
            {
                Player.FSM.Transition(Movement.IsGrounded ? "Idle" : "Fall");
                return;
            }

            // Jump off the ladder
            if (Input.HasJump)
            {
                Movement.SetClimbing(false);
                Player.FSM.Transition("Jump");
                return;
            }

            // Dismount over the top onto a platform
            if (Movement.AtLadderTop && Input.IsPressingUp)
            {
                Movement.SetClimbing(false);
                Player.FSM.Transition("Idle");
                return;
            }

            // Reached the bottom and standing on ground
            if (Movement.AtLadderBottom && Movement.IsGrounded && Input.IsPressingDown)
            {
                Movement.SetClimbing(false);
                Player.FSM.Transition("Idle");
                return;
            }

            // Freeze the climb anim when not moving vertically (cosmetic)
            float climbInput = Mathf.Abs(Input.MoveAxis.y);
            Player.Animator.speed = climbInput > 0.1f ? 1f : 0f;
        }

        public override void OnExit(State<PlayerController> next)
        {
            Player.Animator.speed = 1f;
            Movement.SetClimbing(false);
        }
    }
}
