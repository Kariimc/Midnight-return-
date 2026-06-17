using UnityEngine;
using MidnightReturn.Utils;
using MidnightReturn.Systems;

namespace MidnightReturn.Player.States
{
    // Dragon Kick — aerial dive-kick special. Trigger: Down+Attack while airborne.
    // Slams the player downward; on landing, AOE burst + heavy screen shake + hitbox.
    // Grants invincibility during the dive (matches SotN's dive-kick feel).
    public class DragonKickState : PlayerState
    {
        private const float DIVE_SPEED    = 26f;
        private const float MAX_DURATION  = 1.2f;
        private const float LAND_SHAKE    = 0.18f;
        private const float LAND_SHAKE_DUR = 0.22f;

        private float _timer;
        private bool  _hasLanded;

        public DragonKickState(PlayerController owner) : base(owner, "DragonKick") { }

        public override void OnEnter(State<PlayerController> prev)
        {
            Input.ConsumeAttack();
            _timer     = 0f;
            _hasLanded = false;

            Player.Animator.CrossFadeInFixedTime("DragonKick", 0.04f);
            Player.SetInvincible(true);

            // Snap velocity downward — override horizontal slightly toward facing
            Movement.SetDiveVelocity(Movement.FacingDir * 3f, -DIVE_SPEED);

            VFXManager.Instance?.PlayDashBurst(Player.transform.position, Movement.FacingDir);

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

            if (_hasLanded || _timer >= MAX_DURATION)
            {
                Player.SetInvincible(false);
                Player.FSM.Transition("Idle");
                return;
            }

            if (Movement.IsGrounded)
            {
                _hasLanded = true;
                OnLand();
            }
        }

        public override void OnExit(State<PlayerController> next)
        {
            Player.SetInvincible(false);
        }

        private void OnLand()
        {
            // AOE hitbox at feet
            Combat.QueueAttackHitbox(0f, 2); // use combo index 2 for strongest hit

            // Heavy impact feedback
            VFXManager.Instance?.SpawnLandDust(Player.transform.position);
            VFXManager.Instance?.SpawnHitFlash(Player.transform.position, new Color(1f, 0.7f, 0.1f));
            EventBus.Emit(new CameraShakeEvent { Intensity = LAND_SHAKE, Duration = LAND_SHAKE_DUR });
            EventBus.Emit(new ScreenFlashEvent { Color = new Color(1f, 0.8f, 0.1f, 0.25f), Duration = 0.1f });

            Player.Animator.CrossFadeInFixedTime("Idle", 0.1f);
        }
    }
}
