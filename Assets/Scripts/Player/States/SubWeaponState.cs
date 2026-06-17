using UnityEngine;
using MidnightReturn.Utils;
using MidnightReturn.Systems;

namespace MidnightReturn.Player.States
{
    // Sub-weapon throw — routes to PlayerSpellSystem.TryCast(slot 0).
    // Plays "SubWeapon" anim, fires the projectile at the throw frame, then exits.
    // Can be triggered from Idle, Walk, Run, Jump, Fall.
    public class SubWeaponState : PlayerState
    {
        private const float THROW_TIMING  = 0.22f;  // seconds until projectile fires
        private const float MAX_DURATION  = 0.50f;

        private float _timer;
        private bool  _throwFired;

        public SubWeaponState(PlayerController owner) : base(owner, "SubWeapon") { }

        public override void OnEnter(State<PlayerController> prev)
        {
            Input.ConsumeSpell();
            _timer     = 0f;
            _throwFired = false;

            Player.Animator.CrossFadeInFixedTime("SubWeapon", 0.05f);

            // Brief MP cost check is handled inside TryCast — no need to gate here.
        }

        public override void OnUpdate(float dt)
        {
            _timer += dt;

            // Fire the projectile at the throw-frame timing
            if (!_throwFired && _timer >= THROW_TIMING)
            {
                _throwFired = true;
                Player.SpellSystem?.TryCast(0);

                VFXManager.Instance?.PlayWeaponSwing(Player.transform.position, Movement.FacingDir, 2);
            }

            bool animDone = Player.Animator.GetBool("AttackDone") || _timer >= MAX_DURATION;
            if (!animDone) return;

            Player.Animator.SetBool("AttackDone", false);

            if (!Movement.IsGrounded)           { Player.FSM.Transition("Fall");    return; }
            if (Input.IsPressingLeft ||
                Input.IsPressingRight)          { Player.FSM.Transition("Run");     return; }
            Player.FSM.Transition("Idle");
        }

        public override void OnExit(State<PlayerController> next)
        {
            Player.Animator.SetBool("AttackDone", false);
        }
    }
}
