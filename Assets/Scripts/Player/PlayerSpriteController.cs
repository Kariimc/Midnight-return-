using System.Collections.Generic;
using UnityEngine;
using MidnightReturn.Systems.Rendering;

namespace MidnightReturn.Player
{
    // Bridges the PlayerController FSM to SpriteAnimator clip names.
    // Owns TWO animators: Core quad (Idle/Run/Attack/…) and Advanced quad
    // (TurnAround/SubWeapon/DragonKick/…).  Each frame it decides which quad
    // is visible and routes the clip name to the correct animator.
    // Attach to the Core sprite quad child (which has MeshRenderer + SpriteAnimator
    // + SpriteBillboard). Wire the Advanced quad via ConfigureAdvanced().
    public class PlayerSpriteController : MonoBehaviour
    {
        [SerializeField] private PlayerController _controller;

        // Core quad — same GameObject as this component
        private SpriteAnimator _anim;
        private MeshRenderer   _coreRenderer;
        private Vector3        _baseScale;

        // Advanced quad — sibling GameObject, hidden unless an Advanced state is active
        private SpriteAnimator _advAnim;
        private MeshRenderer   _advRenderer;
        private Transform      _advTransform;
        private Vector3        _advBaseScale;

        private string _lastClip;

        // FSM state names that require the Advanced atlas
        private static readonly HashSet<string> AdvancedStates = new()
        {
            "TurnAround", "SubWeapon", "DragonKick"
        };

        private void Awake()
        {
            _anim         = GetComponent<SpriteAnimator>();
            _coreRenderer = GetComponent<MeshRenderer>();
            _baseScale    = transform.localScale;

            if (_controller == null)
                _controller = GetComponentInParent<PlayerController>();
        }

        // Called by PlayerSpriteBootstrap after the Advanced quad is created.
        public void ConfigureAdvanced(SpriteAnimator advAnim)
        {
            _advAnim      = advAnim;
            _advRenderer  = advAnim.GetComponent<MeshRenderer>();
            _advTransform = advAnim.transform;
            _advBaseScale = _advTransform.localScale;
            _advRenderer.enabled = false;
        }

        // Forces a clip re-evaluation on the next LateUpdate (needed after Configure()
        // resets the animator — otherwise the last _lastClip match prevents a re-play).
        public void ResetClip() => _lastClip = null;

        private void LateUpdate()
        {
            if (_controller == null) return;

            string state  = _controller.FSM.CurrentState;
            bool   useAdv = _advAnim != null && AdvancedStates.Contains(state);

            // Toggle which quad is visible
            _coreRenderer.enabled = !useAdv;
            if (_advRenderer != null) _advRenderer.enabled = useAdv;

            // Mirror facing direction on both quads
            float facingDir = _controller.Movement.FacingDir;
            FlipTransform(transform, _baseScale, facingDir);
            if (_advTransform != null)
                FlipTransform(_advTransform, _advBaseScale, facingDir);

            // Resolve and play on the active animator
            string clip = useAdv ? state : ResolveClip(state);
            if (clip != _lastClip)
            {
                (useAdv ? _advAnim : _anim).Play(clip);
                _lastClip = clip;
            }
        }

        private static void FlipTransform(Transform t, Vector3 baseScale, float facingDir)
        {
            var s = baseScale;
            s.x = Mathf.Abs(s.x) * facingDir;
            t.localScale = s;
        }

        private string ResolveClip(string state)
        {
            return state switch
            {
                "Idle"      => "Idle",
                "Run"       => "Run",
                "Walk"      => "Walk",
                "Crouch"    => "Crouch",
                "Jump"      => "Jump",
                "Fall"      => "Fall",
                "Dash"      => "Dash",
                "WallSlide" => "WallSlide",
                "Climb"     => "Climb",
                "AirAttack" => "AirAttack",
                "Attack"    => ResolveAttack(),
                _           => "Idle",
            };
        }

        private string ResolveAttack()
        {
            if (!_controller.Movement.IsGrounded) return "AirAttack";
            return _controller.Combat.ComboIndex switch
            {
                0 => "Attack1",
                1 => "Attack2",
                _ => "Attack3",
            };
        }
    }
}
