using UnityEngine;
using MidnightReturn.Utils;
using MidnightReturn.Systems;
using MidnightReturn.VerticalSlice;

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

        private static readonly Collider[] _phaseBuffer = new Collider[4];

        public override void OnUpdate(float dt)
        {
            // Detect PhaseableWall ahead — auto-activate Wraith Step if unlocked
            if (Movement.CanWraithStep)
            {
                var checkPos = Player.transform.position
                    + Vector3.right * Movement.FacingDir * 1.2f;
                int n = Physics.OverlapBoxNonAlloc(
                    checkPos, new Vector3(0.25f, 0.45f, 0.5f),
                    _phaseBuffer, Quaternion.identity, ~0);
                for (int i = 0; i < n; i++)
                {
                    if (_phaseBuffer[i] != null &&
                        _phaseBuffer[i].GetComponent<PhaseableWall>() != null)
                    {
                        Player.StopAfterimageTrail();
                        Player.SetInvincible(false);
                        Player.FSM.Transition("WraithStep");
                        return;
                    }
                }
            }

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
