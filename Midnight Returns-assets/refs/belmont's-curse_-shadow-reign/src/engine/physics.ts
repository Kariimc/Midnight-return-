/**
 * Belmont's Curse: Shadow Reign - High-Skill Metroidvania Movement & Physics Engine
 * Handles AABB collision, slopes, wall slide/jump, dodge slide, and Cape/Hair Verlet physics.
 */

import { CapePoint, HairParticle, Hitbox } from '../types/game';

export class PhysicsEngine {
  // Verlet Cape Chain Physics Update
  public static updateCapePhysics(
    capePoints: CapePoint[],
    playerX: number,
    playerY: number,
    playerVx: number,
    playerVy: number,
    facing: 'left' | 'right'
  ) {
    const gravity = 0.4;
    const friction = 0.88;
    const dir = facing === 'right' ? 1 : -1;

    // Anchor first 2 points to player shoulders
    if (capePoints.length >= 2) {
      capePoints[0].x = playerX - 4 * dir;
      capePoints[0].y = playerY - 28;
      capePoints[1].x = playerX + 4 * dir;
      capePoints[1].y = playerY - 28;
    }

    // Update free points
    for (let i = 2; i < capePoints.length; i++) {
      const p = capePoints[i];
      const vx = (p.x - p.oldX) * friction - playerVx * 0.15;
      const vy = (p.y - p.oldY) * friction + gravity - playerVy * 0.1;

      p.oldX = p.x;
      p.oldY = p.y;
      p.x += vx;
      p.y += vy;
    }

    // Distance constraint enforcement (Chain link length = 8px)
    const segmentLen = 8;
    for (let iteration = 0; iteration < 4; iteration++) {
      for (let i = 0; i < capePoints.length - 1; i++) {
        const p1 = capePoints[i];
        const p2 = capePoints[i + 1];

        const dx = p2.x - p1.x;
        const dy = p2.y - p1.y;
        const dist = Math.hypot(dx, dy);

        if (dist > 0.1) {
          const delta = (dist - segmentLen) / dist;
          if (i > 0) {
            p1.x += dx * 0.5 * delta;
            p1.y += dy * 0.5 * delta;
          }
          p2.x -= dx * 0.5 * delta;
          p2.y -= dy * 0.5 * delta;
        }
      }
    }
  }

  // Verlet Hair Strand Update
  public static updateHairPhysics(
    hairPoints: HairParticle[],
    playerX: number,
    playerY: number,
    playerVx: number,
    facing: 'left' | 'right'
  ) {
    const dir = facing === 'right' ? 1 : -1;

    // Head anchor
    if (hairPoints.length > 0) {
      hairPoints[0].x = playerX - 3 * dir;
      hairPoints[0].y = playerY - 42;
    }

    for (let i = 1; i < hairPoints.length; i++) {
      const hp = hairPoints[i];
      const vx = (hp.x - hp.oldX) * 0.85 - playerVx * 0.1;
      const vy = (hp.y - hp.oldY) * 0.85 + 0.2;

      hp.oldX = hp.x;
      hp.oldY = hp.y;
      hp.x += vx;
      hp.y += vy;
    }

    // Constraint distance
    for (let i = 0; i < hairPoints.length - 1; i++) {
      const p1 = hairPoints[i];
      const p2 = hairPoints[i + 1];
      const dx = p2.x - p1.x;
      const dy = p2.y - p1.y;
      const dist = Math.hypot(dx, dy);

      if (dist > 0.1) {
        const delta = (dist - 5) / dist;
        p2.x -= dx * delta;
        p2.y -= dy * delta;
      }
    }
  }

  // Check AABB Collision between two hitboxes
  public static checkAABB(a: Hitbox, b: Hitbox): boolean {
    return (
      a.x < b.x + b.width &&
      a.x + a.width > b.x &&
      a.y < b.y + b.height &&
      a.y + a.height > b.y
    );
  }

  // Resolve platform collisions for player / enemies
  public static resolvePlatformCollisions(
    box: Hitbox,
    vx: number,
    vy: number,
    platforms: Hitbox[]
  ): { x: number; y: number; vx: number; vy: number; isGrounded: boolean; isWallTouching: 'left' | 'right' | null } {
    let newX = box.x + vx;
    let newY = box.y + vy;
    let newVx = vx;
    let newVy = vy;
    let isGrounded = false;
    let isWallTouching: 'left' | 'right' | null = null;

    // Check Horizontal Collisions
    const horizBox: Hitbox = { x: newX, y: box.y, width: box.width, height: box.height };
    for (const p of platforms) {
      if (this.checkAABB(horizBox, p)) {
        if (vx > 0) {
          newX = p.x - box.width;
          isWallTouching = 'right';
        } else if (vx < 0) {
          newX = p.x + p.width;
          isWallTouching = 'left';
        }
        newVx = 0;
        break;
      }
    }

    // Check Vertical Collisions
    const vertBox: Hitbox = { x: newX, y: newY, width: box.width, height: box.height };
    for (const p of platforms) {
      if (this.checkAABB(vertBox, p)) {
        if (vy > 0) {
          // Landing on platform
          newY = p.y - box.height;
          newVy = 0;
          isGrounded = true;
        } else if (vy < 0) {
          // Ceiling hit
          newY = p.y + p.height;
          newVy = 0;
        }
        break;
      }
    }

    return { x: newX, y: newY, vx: newVx, vy: newVy, isGrounded, isWallTouching };
  }
}
