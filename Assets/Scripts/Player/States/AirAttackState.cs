using UnityEngine;
using MidnightReturn.Utils;
using MidnightReturn.Systems;

namespace MidnightReturn.Player.States
{
    // Single aerial strike — no combo. Triggered from Jump or Fall when
    // Attack is pressed without holding Down (Down+Attack → DragonKick).
    // Gives a brief upward velocity nudge to punctuate the swing.
    public class AirAttackState : PlayerState
    {
        private const float HIT_TIMING   = 0.28f;  // seconds into the clip
        private const float Y_BOOST      = 3.0f;   // small upward hold during swing
        private const float MAX_DURATION = 0.55f;

        private float _timer;
        private bool  _hitFired;

        public AirAttackState(PlayerController owner) : base(owner, "AirAttack") { }

        public override void OnEnter(State<PlayerController> prev)
        {
            Input.ConsumeAttack();
            _timer   = 0f;
            _hitFired = false;

            Player.Animator.CrossFadeInFixedTime("AirAttack", 0.04f);

            // Brief upward nudge so the animation reads cleanly while falling
            Movement.NudgeVelocityY(-Y_BOOST);

            VFXManager.Instance?.PlayWeaponSwing(Player.transform.position, Movement.FacingDir, 0);

            EventBus.Emit(new PlayerAttackEvent
            {
                Origin     = Player.transform.position,
                Direction  = (int)Movement.FacingDir,
                ComboIndex = 0,
            });
        }

        public override void OnUpdate(float dt)
        {
            _timer += dt;

            // Spawn hitbox at HIT_TIMING
            if (!_hitFired && _timer >= HIT_TIMING)
            {
                _hitFired = true;
                Combat.QueueAttackHitbox(0f, 0); // immediate
            }

            bool animDone = Player.Animator.GetBool("AttackDone") || _timer >= MAX_DURATION;

            // Can chain into DragonKick mid-air after the swing
            if (animDone && Input.HasAttack && Input.IsPressingDown && !Movement.IsGrounded)
            {
                Player.FSM.Transition("DragonKick");
                return;
            }

            if (animDone)
            {
                Player.Animator.SetBool("AttackDone", false);
                Player.FSM.Transition(Movement.IsGrounded ? "Idle" : "Fall");
            }
        }

        public override void OnExit(State<PlayerController> next)
        {
            Player.Animator.SetBool("AttackDone", false);
        }
    }
}
