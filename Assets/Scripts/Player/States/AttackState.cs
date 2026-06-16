using UnityEngine;
using MidnightReturn.Utils;
using MidnightReturn.Systems;

namespace MidnightReturn.Player.States
{
    public class AttackState : PlayerState
    {
        private int   _combo         = 0;
        private float _comboTimer    = 0f;
        private bool  _animDone      = false;
        private const int   COMBO_MAX    = 3;
        private const float COMBO_WINDOW = 0.5f;

        private static readonly string[] _comboAnims = { "Attack1", "Attack2", "Attack3" };
        private static readonly float[]  _hitTiming  = { 0.25f, 0.22f, 0.35f }; // normalized time to spawn hitbox

        public AttackState(PlayerController owner) : base(owner, "Attack") { }

        public override void OnEnter(State<PlayerController> prev)
        {
            _animDone   = false;
            _comboTimer = COMBO_WINDOW;
            Input.ConsumeAttack();

            // Hitstop freeze: freeze world for 2 frames (AAA feel)
            if (_combo > 0) Time.timeScale = 0f;
            Player.StartCoroutine(ResumeTimeScale(0.033f));

            Player.Animator.CrossFadeInFixedTime(_comboAnims[_combo], 0.04f);

            // Register anim event listener
            Player.Animator.SetBool("AttackDone", false);

            // Spawn hitbox at correct timing via Combat system
            Combat.QueueAttackHitbox(_hitTiming[_combo], _combo);

            // VFX — weapon trail burst
            VFXManager.Instance?.PlayWeaponSwing(
                Player.transform.position,
                Movement.FacingDir,
                _combo
            );
        }

        public override void OnUpdate(float dt)
        {
            _comboTimer -= dt;
            bool animDone = Player.Animator.GetBool("AttackDone");

            // Chain combo
            if (Input.HasAttack && animDone && _combo < COMBO_MAX - 1)
            {
                _combo++;
                OnEnter(this);
                return;
            }

            if (animDone && (_comboTimer <= 0f || !Input.HasAttack))
            {
                _combo = 0;
                Player.FSM.Transition(Movement.IsGrounded ? "Idle" : "Fall");
            }
        }

        public override void OnExit(State<PlayerController> next)
        {
            _combo = 0;
            Player.Animator.SetBool("AttackDone", false);
        }

        private System.Collections.IEnumerator ResumeTimeScale(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            Time.timeScale = 1f;
        }
    }
}
