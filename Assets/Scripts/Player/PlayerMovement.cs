using UnityEngine;
using MidnightReturn.Utils;

namespace MidnightReturn.Player
{
    // 2.5D movement — physics on XY plane, Z locked to 0
    // Attach to the same GameObject as CharacterController
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        // ── Tuning ────────────────────────────────────────────────────────────
        [Header("Ground Movement")]
        [SerializeField] float RunSpeed       = 7.5f;
        [SerializeField] float AirSpeed       = 6.5f;
        [SerializeField] float AccelGround    = 60f;
        [SerializeField] float AccelAir       = 30f;
        [SerializeField] float FrictionGround = 75f;
        [SerializeField] float FrictionAir    = 12f;

        [Header("Jump")]
        [SerializeField] float JumpVelocity   = -17f; // negative = up in world-space (we flip gravity)
        [SerializeField] float JumpCutFactor  = 0.4f;
        [SerializeField] float FallGravity    = 48f;
        [SerializeField] float JumpGravity    = 30f;
        [SerializeField] float MaxFallSpeed   = 24f;
        [SerializeField] float CoyoteTime     = 0.10f;

        [Header("Dash")]
        [SerializeField] float DashSpeed      = 18f;
        [SerializeField] float DashDuration   = 0.18f;
        [SerializeField] float DashCooldown   = 0.50f;

        [Header("Wall")]
        [SerializeField] float WallJumpVX     = 9f;
        [SerializeField] float WallJumpVY     = 16f;
        [SerializeField] float WallSlideSpeed = 2.5f;

        [Header("Abilities (unlockable)")]
        public bool CanDoubleJump  = false;
        public bool CanWallJump    = true;
        public bool CanAirDash     = false;
        public bool CanWallCling   = true;

        // ── Public state ──────────────────────────────────────────────────────
        public Vector2 Velocity      { get; private set; }
        public int     FacingDir     { get; private set; } = 1;
        public bool    IsGrounded    { get; private set; }
        public bool    IsTouchingWall{ get; private set; }
        public int     WallDir       { get; private set; }
        public bool    IsDashing     { get; private set; }
        public bool    IsWallSliding { get; private set; }

        // ── Private state ──────────────────────────────────────────────────────
        private CharacterController _cc;
        private float  _coyoteTimer;
        private int    _jumpsUsed;
        private bool   _jumpHeld;
        private float  _jumpTimer;
        private float  _dashTimer;
        private float  _dashCooldownTimer;
        private int    _dashDir;
        private float  _wallJumpLock;
        private float  _gravity;

        private void Awake() => _cc = GetComponent<CharacterController>();

        public void Tick(float dt, PlayerInputHandler input)
        {
            SyncGroundState();
            UpdateTimers(dt);

            if (IsDashing) { TickDash(dt); return; }

            TickWall(dt, input);
            TickJump(dt, input);
            TickDashRequest(dt, input);
            TickHorizontal(dt, input);
            TickGravity(dt, input);

            // Move
            var motion = new Vector3(Velocity.x, Velocity.y, 0f) * dt;
            _cc.Move(motion);
            // Lock Z
            var pos = transform.position;
            transform.position = new Vector3(pos.x, pos.y, 0f);

            // Flip
            if (input.IsPressingRight) FacingDir =  1;
            if (input.IsPressingLeft)  FacingDir = -1;
        }

        private void SyncGroundState()
        {
            IsGrounded     = _cc.isGrounded;
            IsTouchingWall = (_cc.collisionFlags & CollisionFlags.Sides) != 0;

            if (IsGrounded)
            {
                _jumpsUsed  = 0;
                _coyoteTimer = CoyoteTime;
                var v = Velocity;
                v.y = Mathf.Max(v.y, -0.5f);
                Velocity = v;
            }
        }

        private void UpdateTimers(float dt)
        {
            _coyoteTimer       = Mathf.Max(0f, _coyoteTimer - dt);
            _dashCooldownTimer = Mathf.Max(0f, _dashCooldownTimer - dt);
            if (_wallJumpLock > 0f) _wallJumpLock -= dt;
        }

        private void TickWall(float dt, PlayerInputHandler input)
        {
            IsWallSliding = false;
            if (!CanWallCling || IsGrounded || !IsTouchingWall) return;
            WallDir = input.IsPressingLeft ? -1 : 1;
            IsWallSliding = true;
            var v = Velocity;
            if (v.y < -WallSlideSpeed) v.y = -WallSlideSpeed;
            Velocity = v;
        }

        private void TickJump(float dt, PlayerInputHandler input)
        {
            bool canCoyote = _coyoteTimer > 0f && _jumpsUsed == 0;
            bool canDouble = CanDoubleJump && _jumpsUsed == 1 && !IsGrounded;
            bool canWall   = CanWallJump && IsTouchingWall && !IsGrounded;

            if (input.HasJump)
            {
                if (canWall)
                {
                    var v = Velocity;
                    v.y = WallJumpVY;
                    v.x = -WallDir * WallJumpVX;
                    Velocity = v;
                    _wallJumpLock = 0.15f;
                    _jumpsUsed++;
                    input.ConsumeJump();
                }
                else if (canCoyote || IsGrounded)
                {
                    var v = Velocity;
                    v.y = JumpVelocity;
                    Velocity = v;
                    _coyoteTimer = 0f;
                    _jumpsUsed++;
                    _jumpHeld  = true;
                    _jumpTimer = 0.2f;
                    input.ConsumeJump();
                }
                else if (canDouble)
                {
                    var v = Velocity;
                    v.y = JumpVelocity * 0.85f;
                    Velocity = v;
                    _jumpsUsed++;
                    _jumpHeld  = true;
                    _jumpTimer = 0.15f;
                    input.ConsumeJump();
                }
            }

            if (_jumpHeld)
            {
                _jumpTimer -= dt;
                if (!input.JumpHeld || _jumpTimer <= 0f)
                {
                    _jumpHeld = false;
                    var v = Velocity;
                    if (v.y > 0f) v.y *= JumpCutFactor;
                    Velocity = v;
                }
            }
        }

        private void TickDashRequest(float dt, PlayerInputHandler input)
        {
            bool canDash = (IsGrounded || CanAirDash) && _dashCooldownTimer <= 0f;
            if (!input.HasDash || !canDash) return;
            IsDashing          = true;
            _dashTimer         = DashDuration;
            _dashCooldownTimer = DashCooldown;
            _dashDir           = FacingDir;
            input.ConsumeDash();
        }

        private void TickDash(float dt)
        {
            _dashTimer -= dt;
            Velocity = new Vector2(_dashDir * DashSpeed, 0f);
            var motion = new Vector3(Velocity.x, 0f, 0f) * dt;
            _cc.Move(motion);
            if (_dashTimer <= 0f) IsDashing = false;
        }

        private void TickHorizontal(float dt, PlayerInputHandler input)
        {
            if (_wallJumpLock > 0f) return;
            float maxSpeed = IsGrounded ? RunSpeed : AirSpeed;
            float accel    = IsGrounded ? AccelGround : AccelAir;
            float friction = IsGrounded ? FrictionGround : FrictionAir;
            var v = Velocity;

            if (input.IsPressingRight)
                v.x = MathUtils.Approach(v.x,  maxSpeed, accel * dt);
            else if (input.IsPressingLeft)
                v.x = MathUtils.Approach(v.x, -maxSpeed, accel * dt);
            else
                v.x = MathUtils.Approach(v.x, 0f, friction * dt);

            Velocity = v;
        }

        private void TickGravity(float dt, PlayerInputHandler input)
        {
            var v = Velocity;
            float grav = (v.y > 0f) ? JumpGravity : FallGravity;
            v.y -= grav * dt;
            v.y  = Mathf.Max(v.y, -MaxFallSpeed);
            Velocity = v;
        }
    }
}
