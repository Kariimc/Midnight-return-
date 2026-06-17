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
                VFXManager.Instance?.SpawnLandDust(Player.transform.position);
                EventBus.Emit(new CameraShakeEvent { Intensity = 0.06f, Duration = 0.12f });
                Player.FSM.Transition(Input.IsPressingLeft || Input.IsPressingRight ? "Run" : "Idle");
                return;
            }

            if (Movement.CanClimb && (Input.IsPressingUp || Input.IsPressingDown))
                { Player.FSM.Transition("Climb"); return; }
            if (Input.HasDash)   { Player.FSM.Transition("Dash");      return; }
            if (Input.HasSpell)  { Player.FSM.Transition("SubWeapon"); return; }
            if (Input.HasJump)   { Player.FSM.Transition("Jump");      return; }

            // Down+Attack → DragonKick dive-kick
            if (Input.HasAttack && Input.IsPressingDown)
                { Player.FSM.Transition("DragonKick"); return; }

            // Regular aerial attack
            if (Input.HasAttack)
                { Player.FSM.Transition("AirAttack"); return; }
        }
    }
}
