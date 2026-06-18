using UnityEngine;
using MidnightReturn.Utils;
using MidnightReturn.Systems;

namespace MidnightReturn.Player.States
{
    // Crouching — shrinks CharacterController to half height, plays "Crouch" anim.
    // Crouch-attack: Attack while crouching plays "Attack1" at shorter hitbox height.
    public class CrouchState : PlayerState
    {
        private bool _attacking;
        private float _attackTimer;
        private const float ATTACK_DURATION = 0.40f;
        private const float HIT_TIMING      = 0.18f;
        private bool _hitFired;

        public CrouchState(PlayerController owner) : base(owner, "Crouch") { }

        public override void OnEnter(State<PlayerController> prev)
        {
            Player.Animator.CrossFadeInFixedTime("Crouch", 0.08f);
            Movement.SetCrouch(true);
            _attacking = false;
        }

        public override void OnUpdate(float dt)
        {
            if (!Movement.IsGrounded) { ExitCrouch(); Player.FSM.Transition("Fall"); return; }

            if (_attacking)
            {
                _attackTimer += dt;
                if (!_hitFired && _attackTimer >= HIT_TIMING)
                {
                    _hitFired = true;
                    Combat.QueueAttackHitbox(0f, 0);
                    EventBus.Emit(new PlayerAttackEvent
                    {
                        Origin    = Player.transform.position,
                        Direction = (int)Movement.FacingDir,
                        ComboIndex = 0,
                    });
                }
                if (_attackTimer >= ATTACK_DURATION || Player.Animator.GetBool("AttackDone"))
                {
                    Player.Animator.SetBool("AttackDone", false);
                    _attacking = false;
                    Player.Animator.CrossFadeInFixedTime("Crouch", 0.06f);
                }
                return; // hold state while attacking
            }

            // Crouch-attack
            if (Input.HasAttack)
            {
                _attacking    = true;
                _attackTimer  = 0f;
                _hitFired     = false;
                Input.ConsumeAttack();
                Player.Animator.CrossFadeInFixedTime("Attack1", 0.04f);
                VFXManager.Instance?.PlayWeaponSwing(Player.transform.position, Movement.FacingDir, 0);
                return;
            }

            // Spell while crouching + Hourglass unlocked → time rewind
            if (Input.HasSpell && Movement.CanHourglass && Player.HourglassCooldown <= 0f)
                { ExitCrouch(); Player.FSM.Transition("Hourglass"); return; }

            if (Input.HasJump) { ExitCrouch(); Player.FSM.Transition("Jump"); return; }
            if (Input.HasDash) { ExitCrouch(); Player.FSM.Transition("Dash"); return; }

            // Release crouch
            if (!Input.IsPressingDown)
            {
                ExitCrouch();
                Player.FSM.Transition(
                    (Input.IsPressingLeft || Input.IsPressingRight) ? "Run" : "Idle");
            }
        }

        public override void OnExit(State<PlayerController> next) => ExitCrouch();

        private void ExitCrouch() => Movement.SetCrouch(false);
    }
}
