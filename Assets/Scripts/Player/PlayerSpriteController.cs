using UnityEngine;
using MidnightReturn.Systems.Rendering;

namespace MidnightReturn.Player
{
    // Bridges the PlayerController FSM to SpriteAnimator clip names.
    // Attach to a child GameObject that has MeshRenderer + SpriteAnimator + SpriteBillboard.
    public class PlayerSpriteController : MonoBehaviour
    {
        [SerializeField] private PlayerController _controller;

        private SpriteAnimator _anim;
        private string         _lastClip;
        private Vector3        _baseScale;

        private void Awake()
        {
            _anim      = GetComponent<SpriteAnimator>();
            _baseScale = transform.localScale;

            if (_controller == null)
                _controller = GetComponentInParent<PlayerController>();
        }

        private void LateUpdate()
        {
            if (_controller == null) return;

            string clip = ResolveClip();
            if (clip != _lastClip)
            {
                _anim.Play(clip);
                _lastClip = clip;
            }

            // Flip sprite horizontally when facing left
            var s  = _baseScale;
            s.x    = Mathf.Abs(s.x) * _controller.Movement.FacingDir;
            transform.localScale = s;
        }

        private string ResolveClip()
        {
            // FSM state names match the string passed to the State<T> base constructor:
            // "Idle", "Run", "Jump", "Fall", "Dash", "WallSlide", "Attack"
            string state = _controller.FSM.CurrentState;

            return state switch
            {
                "Idle"      => "Idle",
                "Run"       => "Run",
                "Jump"      => "Jump",
                "Fall"      => "Fall",
                "Dash"      => "Dash",
                "WallSlide" => "WallSlide",
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
