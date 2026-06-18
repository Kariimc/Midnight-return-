using UnityEngine;
using MidnightReturn.Utils;
using MidnightReturn.Systems;

namespace MidnightReturn.Player.States
{
    // Wraith Step — automatically triggered when the player dashes into a PhaseableWall.
    // Disables CharacterController collision and suppresses the normal movement tick for
    // PHASE_DURATION seconds, letting the player glide through 1-tile-wide geometry.
    // Ghost afterimage trail + purple screen flash confirm the phase.
    public class WraithStepState : PlayerState
    {
        private const float PHASE_SPEED    = 13f;
        private const float PHASE_DURATION = 0.32f;

        private float _timer;
        private int   _dir;
        private CharacterController _cc;

        public WraithStepState(PlayerController owner) : base(owner, "WraithStep") { }

        public override void OnEnter(State<PlayerController> prev)
        {
            _timer = 0f;
            _dir   = Movement.FacingDir;
            _cc    = Player.GetComponent<CharacterController>();

            Player.Animator.CrossFadeInFixedTime("Dash", 0.04f);
            Player.StartAfterimageTrail();
            Player.SetInvincible(true);

            // Disable CC collision so we pass through the PhaseableWall collider
            _cc.detectCollisions = false;
            Movement.IsSuppressed = true;

            EventBus.Emit(new ScreenFlashEvent
            {
                Color    = new Color(0.55f, 0.1f, 1f, 0.45f),
                Duration = 0.22f,
            });
            EventBus.Emit(new WraithStepActivatedEvent
            {
                WallPos = Player.transform.position + Vector3.right * _dir
            });
            EventBus.Emit(new CameraShakeEvent { Intensity = 0.06f, Duration = 0.1f });
        }

        public override void OnFixedUpdate(float dt)
        {
            _timer += dt;

            // Drive the player through the wall manually while CC collision is off
            var motion = new Vector3(_dir * PHASE_SPEED * dt, 0f, 0f);
            _cc.Move(motion);

            // Z lock
            var p = Player.transform.position;
            Player.transform.position = new Vector3(p.x, p.y, 0f);

            if (_timer >= PHASE_DURATION)
                Player.FSM.Transition(Movement.IsGrounded ? "Idle" : "Fall");
        }

        public override void OnExit(State<PlayerController> next)
        {
            _cc.detectCollisions = true;
            Movement.IsSuppressed = false;
            Player.StopAfterimageTrail();
            Player.SetInvincible(false);
        }
    }
}
