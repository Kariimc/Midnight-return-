import Phaser from 'phaser';
import type { Player } from '../entities/player/Player';

export class CameraSystem {
  private cam: Phaser.Cameras.Scene2D.Camera;
  private targetOffsetX = 0; // Lookahead in movement direction
  private bossShakeActive = false;

  constructor(scene: Phaser.Scene, worldW: number, worldH: number) {
    this.cam = scene.cameras.main;
    this.cam.setBounds(0, 0, worldW, worldH);
    // Slight zoom for that epic cinematic feel
    this.cam.setZoom(1.5);
  }

  follow(player: Player): void {
    this.cam.startFollow(player.sprite, true, 0.08, 0.1);
    this.cam.setFollowOffset(0, -30); // Slightly above center
  }

  update(_dt: number, player: Player): void {
    // Horizontal lookahead — shift camera in movement direction
    const targetX = player.movement.facingDir * 60;
    this.targetOffsetX += (targetX - this.targetOffsetX) * 0.05;
    this.cam.setFollowOffset(-this.targetOffsetX, -30);
  }

  shake(intensity = 0.006, duration = 300): void {
    this.cam.shake(duration, intensity);
  }

  bossShake(): void {
    if (this.bossShakeActive) return;
    this.bossShakeActive = true;
    this.cam.shake(600, 0.012);
    setTimeout(() => { this.bossShakeActive = false; }, 700);
  }

  flash(color = 0xffffff, duration = 200): void {
    this.cam.flash(duration, (color >> 16) & 0xff, (color >> 8) & 0xff, color & 0xff);
  }

  zoomTo(level: number, duration = 400): void {
    const scene = this.cam.scene;
    scene.tweens.add({
      targets: this.cam,
      zoom: level,
      duration,
      ease: 'Power2',
    });
  }
}
