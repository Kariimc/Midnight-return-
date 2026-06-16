using MidnightReturn.Utils;
using MidnightReturn.Systems;

namespace MidnightReturn.Player.States
{
    public class FallState : PlayerState
    {
        public FallState(PlayerController owner) : base(owner, "Fall") { }

        public override void OnEnter(State<PlayerController> prev)
        {
            Player.Animator.CrossFadeInFixedTime("Fall", 0.1f);
        }

        public override void OnUpdate(float dt)
        {
            if (Movement.IsWallSliding)
            {
                Player.FSM.Transition("WallSlide");
                return;
            }

            if (Movement.IsGrounded)
            {
                // Land — trigger VFX and screen shake
                VFXManager.Instance?.SpawnLandDust(Player.transform.position);
                EventBus.Emit(new CameraShakeEvent { Intensity = 0.06f, Duration = 0.12f });
                Player.FSM.Transition(Input.IsPressingLeft || Input.IsPressingRight ? "Run" : "Idle");
                return;
            }

            if (Input.HasAttack) { Player.FSM.Transition("Attack"); return; }
            if (Input.HasJump)   { Player.FSM.Transition("Jump");   return; }
            if (Input.HasDash)   { Player.FSM.Transition("Dash");   return; }
        }
    }
}
