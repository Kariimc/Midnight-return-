import Phaser from 'phaser';
import { clamp, approach, lerp } from '../../utils/MathUtils';

// ─── Tuning constants ────────────────────────────────────────────────────────
const RUN_SPEED       = 220;  // px/s
const AIR_SPEED       = 190;
const ACCEL_GROUND    = 1800;
const ACCEL_AIR       = 900;
const FRICTION_GROUND = 2200;
const FRICTION_AIR    = 400;
const JUMP_VELOCITY   = -520;
const JUMP_CUT_FACTOR = 0.4;  // short-hop multiplier
const FALL_GRAVITY    = 1400;
const JUMP_GRAVITY    = 900;
const MAX_FALL_SPEED  = 700;
const COYOTE_TIME     = 0.1;   // seconds of grace after walking off ledge
const DASH_SPEED      = 550;
const DASH_DURATION   = 0.18;  // seconds
const DASH_COOLDOWN   = 0.5;
const WALLJUMP_VX     = 260;
const WALLJUMP_VY     = -480;
const WALL_SLIDE_SPEED = 80;

export interface MovementConfig {
  canDoubleJump: boolean;
  canWallJump:   boolean;
  canAirDash:    boolean;
  canWallCling:  boolean;
}

export class PlayerMovement {
  // Physics state
  vx = 0;
  vy = 0;
  facingDir: 1 | -1 = 1;
  isGrounded = false;
  isTouchingWall: 1 | -1 | 0 = 0;

  // Jump state
  private coyoteTimer = 0;
  private jumpsUsed    = 0;
  private jumpHeld     = false;
  private jumpTimer    = 0;

  // Dash state
  isDashing      = false;
  private dashTimer    = 0;
  private dashCooldown = 0;
  private dashDir: 1 | -1 = 1;

  // Wall state
  private wallClingTimer = 0;
  private wallJumpLock   = 0; // brief window locking movement after walljump

  constructor(public cfg: MovementConfig) {}

  // Called each physics frame from Player
  update(
    dt: number,
    input: { left: boolean; right: boolean; jump: boolean; dash: boolean; jumpHeld: boolean },
    body: Phaser.Physics.Arcade.Body,
  ): void {
    // Sync grounded / wall state from physics body
    this.isGrounded       = body.blocked.down;
    this.isTouchingWall   = body.blocked.right ? 1 : body.blocked.left ? -1 : 0;

    // Reset jump count when grounded
    if (this.isGrounded) {
      this.jumpsUsed    = 0;
      this.coyoteTimer  = COYOTE_TIME;
    } else {
      this.coyoteTimer = Math.max(0, this.coyoteTimer - dt);
    }

    // Dash cooldown
    if (this.dashCooldown > 0) this.dashCooldown -= dt;

    // ── Dash ────────────────────────────────────────────────────────────────
    const canDash = (this.isGrounded || this.cfg.canAirDash) && this.dashCooldown <= 0;
    if (input.dash && canDash && !this.isDashing) {
      this.isDashing   = true;
      this.dashTimer   = DASH_DURATION;
      this.dashCooldown = DASH_COOLDOWN;
      this.dashDir     = this.facingDir;
    }

    if (this.isDashing) {
      this.dashTimer -= dt;
      body.setVelocityX(DASH_SPEED * this.dashDir);
      body.setVelocityY(0);
      body.setAllowGravity(false);
      if (this.dashTimer <= 0) {
        this.isDashing = false;
        body.setAllowGravity(true);
      }
      return; // Skip normal movement while dashing
    }

    // ── Wall jump / wall cling ───────────────────────────────────────────────
    if (this.cfg.canWallCling && !this.isGrounded && this.isTouchingWall !== 0) {
      this.wallClingTimer += dt;
      if (body.velocity.y > WALL_SLIDE_SPEED) {
        body.setVelocityY(WALL_SLIDE_SPEED);
      }
    } else {
      this.wallClingTimer = 0;
    }

    if (this.wallJumpLock > 0) this.wallJumpLock -= dt;

    // ── Jump ────────────────────────────────────────────────────────────────
    const canCoyoteJump = this.coyoteTimer > 0 && this.jumpsUsed === 0;
    const canDoubleJump = this.cfg.canDoubleJump && this.jumpsUsed === 1 && !this.isGrounded;
    const canWallJump   = this.cfg.canWallJump && this.isTouchingWall !== 0 && !this.isGrounded;

    if (input.jump) {
      if (canWallJump) {
        this.vy = WALLJUMP_VY;
        this.vx = -this.isTouchingWall * WALLJUMP_VX;
        body.setVelocity(this.vx, this.vy);
        this.wallJumpLock = 0.15;
        this.jumpsUsed++;
      } else if (canCoyoteJump || this.isGrounded) {
        body.setVelocityY(JUMP_VELOCITY);
        this.coyoteTimer = 0;
        this.jumpsUsed++;
        this.jumpHeld  = true;
        this.jumpTimer = 0.2;
      } else if (canDoubleJump) {
        body.setVelocityY(JUMP_VELOCITY * 0.85);
        this.jumpsUsed++;
        this.jumpHeld  = true;
        this.jumpTimer = 0.15;
      }
    }

    // Variable jump height — cut velocity on release
    if (this.jumpHeld) {
      this.jumpTimer -= dt;
      if (!input.jumpHeld || this.jumpTimer <= 0) {
        this.jumpHeld = false;
        if (body.velocity.y < 0) {
          body.setVelocityY(body.velocity.y * JUMP_CUT_FACTOR);
        }
      }
    }

    // ── Horizontal movement ──────────────────────────────────────────────────
    if (this.wallJumpLock <= 0) {
      const maxSpeed = this.isGrounded ? RUN_SPEED : AIR_SPEED;
      const accel    = this.isGrounded ? ACCEL_GROUND : ACCEL_AIR;
      const friction = this.isGrounded ? FRICTION_GROUND : FRICTION_AIR;

      if (input.right) {
        this.facingDir = 1;
        const target = maxSpeed;
        body.setVelocityX(approach(body.velocity.x, target, accel * dt));
      } else if (input.left) {
        this.facingDir = -1;
        body.setVelocityX(approach(body.velocity.x, -maxSpeed, accel * dt));
      } else {
        // Friction
        const vx = body.velocity.x;
        body.setVelocityX(approach(vx, 0, friction * dt));
      }
    }

    // ── Gravity ──────────────────────────────────────────────────────────────
    const grav = body.velocity.y < 0 ? JUMP_GRAVITY : FALL_GRAVITY;
    body.setGravityY(grav);
    if (body.velocity.y > MAX_FALL_SPEED) body.setVelocityY(MAX_FALL_SPEED);
  }
}
