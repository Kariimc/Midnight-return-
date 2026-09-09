/**
 * Belmont's Curse: Shadow Reign - Multi-Layer Dynamic Particle & Visual FX Engine
 * Sparks, blood mist, shadow ghost trails, spell vortexes, rain, embers, and shockwaves.
 */

import { DynamicParticle } from '../types/game';

export class ParticleEngine {
  private particles: DynamicParticle[] = [];

  public updateAndDraw(ctx: CanvasRenderingContext2D, dt: number) {
    for (let i = this.particles.length - 1; i >= 0; i--) {
      const p = this.particles[i];

      p.life += dt;
      if (p.life >= p.maxLife) {
        this.particles.splice(i, 1);
        continue;
      }

      p.x += p.vx;
      p.y += p.vy;

      if (p.vRot) {
        p.rotation = (p.rotation || 0) + p.vRot;
      }

      const progress = p.life / p.maxLife;
      const alpha = p.alpha * (1 - progress);

      ctx.save();
      ctx.globalAlpha = Math.max(0, alpha);

      if (p.glow) {
        ctx.shadowColor = p.color;
        ctx.shadowBlur = 12;
      }

      ctx.fillStyle = p.color;
      ctx.strokeStyle = p.color;

      if (p.shape === 'spark') {
        ctx.lineWidth = 2;
        ctx.beginPath();
        ctx.moveTo(p.x, p.y);
        ctx.lineTo(p.x - p.vx * 3, p.y - p.vy * 3);
        ctx.stroke();
      } else if (p.shape === 'blade_ghost') {
        // Shadow trail afterimage
        ctx.fillStyle = p.color;
        ctx.fillRect(p.x - p.size / 2, p.y - p.size, p.size, p.size * 2);
      } else if (p.shape === 'ring') {
        ctx.lineWidth = 3;
        ctx.beginPath();
        ctx.arc(p.x, p.y, p.size * (1 + progress * 2), 0, Math.PI * 2);
        ctx.stroke();
      } else if (p.shape === 'blood') {
        ctx.beginPath();
        ctx.arc(p.x, p.y, p.size * (1 - progress * 0.5), 0, Math.PI * 2);
        ctx.fill();
      } else { // Circle / Ember / Smoke
        ctx.beginPath();
        ctx.arc(p.x, p.y, p.size * (1 - progress * 0.3), 0, Math.PI * 2);
        ctx.fill();
      }

      ctx.restore();
    }
  }

  // Spawn Sword Impact Sparks
  public spawnSparks(x: number, y: number, count: number = 12, color: string = '#fde047') {
    for (let i = 0; i < count; i++) {
      const angle = Math.random() * Math.PI * 2;
      const speed = 2 + Math.random() * 6;
      this.particles.push({
        x,
        y,
        vx: Math.cos(angle) * speed,
        vy: Math.sin(angle) * speed,
        life: 0,
        maxLife: 0.2 + Math.random() * 0.25,
        size: 3,
        color,
        glow: true,
        shape: 'spark',
        alpha: 1
      });
    }
  }

  // Spawn Crimson Blood Mist
  public spawnBloodMist(x: number, y: number, count: number = 10) {
    for (let i = 0; i < count; i++) {
      this.particles.push({
        x: x + (Math.random() - 0.5) * 10,
        y: y + (Math.random() - 0.5) * 10,
        vx: (Math.random() - 0.5) * 3,
        vy: -1 - Math.random() * 2,
        life: 0,
        maxLife: 0.4 + Math.random() * 0.3,
        size: 2 + Math.random() * 4,
        color: Math.random() > 0.5 ? '#dc2626' : '#991b1b',
        shape: 'blood',
        alpha: 0.9
      });
    }
  }

  // Spawn Dash Ghost Afterimage
  public spawnGhostTrail(x: number, y: number, width: number, height: number, color: string) {
    this.particles.push({
      x,
      y,
      vx: 0,
      vy: 0,
      life: 0,
      maxLife: 0.25,
      size: width,
      color,
      shape: 'blade_ghost',
      alpha: 0.6,
      glow: true
    });
  }

  // Spawn Spell Vortex Blast
  public spawnMagicRing(x: number, y: number, color: string = '#38bdf8') {
    this.particles.push({
      x,
      y,
      vx: 0,
      vy: 0,
      life: 0,
      maxLife: 0.35,
      size: 15,
      color,
      shape: 'ring',
      alpha: 1,
      glow: true
    });
  }

  // Clear all particles on room change
  public clear() {
    this.particles = [];
  }
}
