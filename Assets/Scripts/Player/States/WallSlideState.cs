using MidnightReturn.Utils;
using MidnightReturn.Systems;

namespace MidnightReturn.Player.States
{
    public class WallSlideState : PlayerState
    {
        public WallSlideState(PlayerController owner) : base(owner, "WallSlide") { }

        public override void OnEnter(State<PlayerController> prev)
        {
            Player.Animator.CrossFadeInFixedTime("WallSlide", 0.08f);
            VFXManager.Instance?.PlayWallScrape(Player.transform.position);
        }

        public override void OnUpdate(float dt)
        {
            if (Movement.IsGrounded)        { Player.FSM.Transition("Idle"); return; }
            if (!Movement.IsWallSliding)    { Player.FSM.Transition("Fall"); return; }
            if (Input.HasJump)              { Player.FSM.Transition("Jump"); return; }
            if (Input.HasDash)              { Player.FSM.Transition("Dash"); return; }
        }

        public override void OnExit(State<PlayerController> next)
        {
            VFXManager.Instance?.StopWallScrape();
        }
    }
}
