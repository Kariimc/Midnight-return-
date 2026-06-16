import Phaser from 'phaser';
import { ObjectPool } from '../utils/ObjectPool';

// ─── Damage Number ───────────────────────────────────────────────────────────
interface DmgNumber { text: Phaser.GameObjects.Text; vx: number; vy: number; life: number; }

// ─── VFX System ──────────────────────────────────────────────────────────────
// Centralised system for particle bursts, screen flash, damage numbers, afterimages
export class VFXSystem {
  private emitters: Phaser.GameObjects.Particles.ParticleEmitter[] = [];
  private dmgNumbers: DmgNumber[] = [];
  private afterimages: Phaser.GameObjects.Image[] = [];
  private flashRect: Phaser.GameObjects.Rectangle;
  private scene: Phaser.Scene;

  constructor(scene: Phaser.Scene) {
    this.scene = scene;

    // Full-screen flash overlay
    const { width, height } = scene.scale;
    this.flashRect = scene.add.rectangle(width / 2, height / 2, width, height, 0xffffff, 0)
      .setScrollFactor(0)
      .setDepth(100);

    // Listen for game events
    scene.events.on('damage_number',    this.spawnDamageNumber, this);
    scene.events.on('enemy_death_vfx',  this.spawnDeathBurst,   this);
    scene.events.on('player_dash_start', this.spawnAfterimage,  this);
    scene.events.on('player_land',       this.spawnLandDust,    this);
    scene.events.on('screen_flash',      this.screenFlash,      this);
    scene.events.on('hit_spark',         this.spawnHitSpark,    this);
  }

  update(dt: number): void {
    // Animate floating damage numbers
    for (let i = this.dmgNumbers.length - 1; i >= 0; i--) {
      const d = this.dmgNumbers[i];
      d.life -= dt;
      d.text.x += d.vx * dt;
      d.text.y += d.vy * dt;
      d.vy += 200 * dt; // gravity
      d.text.setAlpha(Math.max(0, d.life / 0.8));
      if (d.life <= 0) { d.text.destroy(); this.dmgNumbers.splice(i, 1); }
    }

    // Fade afterimages
    for (let i = this.afterimages.length - 1; i >= 0; i--) {
      const img = this.afterimages[i];
      img.setAlpha(img.alpha - dt * 4);
      if (img.alpha <= 0) { img.destroy(); this.afterimages.splice(i, 1); }
    }
  }

  private spawnDamageNumber = (data: { x: number; y: number; value: number; crit?: boolean }) => {
    const isCrit = data.crit ?? false;
    const text   = this.scene.add.text(data.x, data.y, String(data.value), {
      fontFamily: 'serif',
      fontSize:   isCrit ? '28px' : '20px',
      color:      isCrit ? '#ffdd00' : '#ffffff',
      stroke:     '#000000',
      strokeThickness: 4,
    }).setDepth(50).setOrigin(0.5);

    this.dmgNumbers.push({
      text, vx: (Math.random() - 0.5) * 60, vy: -180, life: 0.8,
    });
  };

  private spawnDeathBurst = (data: { x: number; y: number; color: number; particles: number }) => {
    const emitter = this.scene.add.particles(data.x, data.y, 'particle_dot', {
      speed:        { min: 80, max: 300 },
      angle:        { min: 0, max: 360 },
      scale:        { start: 1.2, end: 0 },
      lifespan:     { min: 400, max: 900 },
      tint:         data.color,
      quantity:     data.particles,
      gravityY:     400,
      emitting:     false,
    });
    emitter.explode(data.particles, 0, 0);
    this.scene.time.delayedCall(1000, () => emitter.destroy());
  };

  private spawnAfterimage = (data: { x: number; y: number; dir: number }) => {
    // Would clone the player sprite frame — placeholder uses rectangle
    const img = this.scene.add.image(data.x, data.y, 'player')
      .setAlpha(0.5)
      .setTint(0x8844ff)
      .setFlipX(data.dir === -1)
      .setDepth(9);
    this.afterimages.push(img);
  };

  private spawnLandDust = (data: { x: number; y: number }) => {
    const emitter = this.scene.add.particles(data.x, data.y, 'particle_dot', {
      speed:    { min: 30, max: 100 },
      angle:    { min: 150, max: 210 },
      scale:    { start: 0.8, end: 0 },
      lifespan: 350,
      tint:     0xaaaaaa,
      quantity: 12,
      emitting: false,
    });
    emitter.explode(12, 0, 0);
    this.scene.time.delayedCall(500, () => emitter.destroy());
  };

  private spawnHitSpark = (data: { x: number; y: number; color?: number }) => {
    const color = data.color ?? 0xffff88;
    const emitter = this.scene.add.particles(data.x, data.y, 'particle_dot', {
      speed:    { min: 120, max: 280 },
      angle:    { min: 0, max: 360 },
      scale:    { start: 0.6, end: 0 },
      lifespan: 250,
      tint:     color,
      quantity: 8,
      emitting: false,
    });
    emitter.explode(8, 0, 0);
    this.scene.time.delayedCall(400, () => emitter.destroy());
  };

  screenFlash(color = 0xffffff, duration = 0.15): void {
    this.flashRect.setFillStyle(color, 0.6);
    this.scene.tweens.add({
      targets:  this.flashRect,
      fillAlpha: 0,
      duration: duration * 1000,
      ease:     'Power2',
    });
  }

  destroy(): void {
    this.scene.events.off('damage_number',    this.spawnDamageNumber, this);
    this.scene.events.off('enemy_death_vfx',  this.spawnDeathBurst,   this);
    this.scene.events.off('player_dash_start', this.spawnAfterimage,  this);
    this.scene.events.off('player_land',       this.spawnLandDust,    this);
    this.scene.events.off('screen_flash',      this.screenFlash,      this);
    this.scene.events.off('hit_spark',         this.spawnHitSpark,    this);
  }
}
