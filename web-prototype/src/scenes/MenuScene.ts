import Phaser from 'phaser';
import { GameState } from '../core/GameState';
import { bus, EVENTS } from '../utils/EventBus';

export class MenuScene extends Phaser.Scene {
  constructor() { super({ key: 'MenuScene' }); }

  create(): void {
    const { width, height } = this.scale;

    // Background gradient
    const bg = this.add.graphics();
    bg.fillGradientStyle(0x000000, 0x000000, 0x110022, 0x110022, 1);
    bg.fillRect(0, 0, width, height);

    // Animated particles in background
    this.add.particles(0, 0, 'particle_dot', {
      x:        { min: 0, max: width },
      y:        { min: 0, max: height },
      speedX:   { min: -10, max: 10 },
      speedY:   { min: -20, max: -5 },
      scale:    { start: 0.3, end: 0 },
      lifespan: { min: 2000, max: 5000 },
      tint:     [0x8800ff, 0x4400aa, 0xcc44ff],
      frequency: 80,
      alpha:    { start: 0.6, end: 0 },
    });

    // Title
    this.add.text(width / 2, height * 0.22, 'MIDNIGHT RETURN', {
      fontFamily: '"Palatino Linotype", Palatino, serif',
      fontSize:   '56px',
      color:      '#ddbbff',
      stroke:     '#440066',
      strokeThickness: 8,
      shadow:     { color: '#8800ff', blur: 20, fill: true },
    }).setOrigin(0.5);

    this.add.text(width / 2, height * 0.34, 'A CASTLE AWAITS', {
      fontFamily: 'serif',
      fontSize:   '18px',
      color:      '#996699',
      letterSpacing: 6,
    }).setOrigin(0.5);

    // Menu options
    const options = [
      { label: 'NEW GAME',      action: () => this.startNewGame() },
      { label: 'CONTINUE',      action: () => this.loadGame() },
      { label: 'OPTIONS',       action: () => {} },
    ];

    options.forEach((opt, i) => {
      const y = height * 0.54 + i * 54;
      const btn = this.add.text(width / 2, y, opt.label, {
        fontFamily: 'serif',
        fontSize:   '26px',
        color:      '#ccaaee',
        stroke:     '#000000',
        strokeThickness: 3,
      }).setOrigin(0.5).setInteractive({ useHandCursor: true });

      btn.on('pointerover', () => {
        btn.setColor('#ffffff');
        btn.setFontSize(28);
        this.add.particles(btn.x - 120, btn.y, 'particle_dot', {
          speed: 20, lifespan: 300, tint: 0xcc88ff,
          quantity: 1, scale: { start: 0.4, end: 0 },
        });
      });
      btn.on('pointerout',  () => { btn.setColor('#ccaaee'); btn.setFontSize(26); });
      btn.on('pointerdown', () => opt.action());
    });

    // Version / controls hint
    this.add.text(width / 2, height - 30,
      'WASD / Move  |  SPACE / Jump  |  SHIFT / Dash  |  J / Attack  |  K / Spell',
      { fontFamily: 'monospace', fontSize: '12px', color: '#554466' }
    ).setOrigin(0.5);
  }

  private startNewGame(): void {
    GameState.setPhase('playing');
    this.scene.start('GameScene');
    this.scene.start('HUDScene');
    this.scene.bringToTop('HUDScene');
  }

  private loadGame(): void {
    if (!GameState.fromLocalStorage(0)) {
      this.startNewGame();
      return;
    }
    this.startNewGame();
  }
}
