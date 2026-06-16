using MidnightReturn.Utils;
using MidnightReturn.Systems;

namespace MidnightReturn.Player.States
{
    public class DashState : PlayerState
    {
        public DashState(PlayerController owner) : base(owner, "Dash") { }

        public override void OnEnter(State<PlayerController> prev)
        {
            Player.Animator.CrossFadeInFixedTime("Dash", 0.04f);
            Input.ConsumeDash();

            // Trigger VFX Graph dash burst + afterimage trail
            VFXManager.Instance?.PlayDashBurst(Player.transform.position, Movement.FacingDir);
            Player.StartAfterimageTrail();

            // Invincibility during dash
            Player.SetInvincible(true);
        }

        public override void OnUpdate(float dt)
        {
            if (!Movement.IsDashing)
            {
                Player.StopAfterimageTrail();
                Player.SetInvincible(false);
                Player.FSM.Transition(Movement.IsGrounded
                    ? (Input.IsPressingLeft || Input.IsPressingRight ? "Run" : "Idle")
                    : "Fall");
            }
        }

        public override void OnExit(State<PlayerController> next)
        {
            Player.StopAfterimageTrail();
            Player.SetInvincible(false);
        }
    }
}
