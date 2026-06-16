import Phaser from 'phaser';
import { GameState } from '../core/GameState';
import { bus, EVENTS } from '../utils/EventBus';

export class HUDScene extends Phaser.Scene {
  private hpBar!:    Phaser.GameObjects.Rectangle;
  private hpBarBg!:  Phaser.GameObjects.Rectangle;
  private mpBar!:    Phaser.GameObjects.Rectangle;
  private mpBarBg!:  Phaser.GameObjects.Rectangle;
  private hpText!:   Phaser.GameObjects.Text;
  private mpText!:   Phaser.GameObjects.Text;
  private lvlText!:  Phaser.GameObjects.Text;
  private expBar!:   Phaser.GameObjects.Rectangle;
  private levelUpText!: Phaser.GameObjects.Text;
  private readonly BAR_W = 200;

  constructor() { super({ key: 'HUDScene' }); }

  create(): void {
    this.buildHUD();

    bus.on(EVENTS.PLAYER_DAMAGED,    this.onDamaged);
    bus.on(EVENTS.PLAYER_LEVELED_UP, this.onLevelUp);
    bus.on(EVENTS.ENEMY_DIED,        this.onExpGained);
  }

  private buildHUD(): void {
    const PAD = 16;

    // ── HP Bar ─────────────────────────────────────────────────────────────
    this.add.text(PAD, PAD, 'HP', { fontFamily: 'serif', fontSize: '13px', color: '#cc4444' });
    this.hpBarBg = this.add.rectangle(PAD + 22, PAD + 6, this.BAR_W, 14, 0x330000).setOrigin(0, 0.5);
    this.hpBar   = this.add.rectangle(PAD + 22, PAD + 6, this.BAR_W, 14, 0xcc2222).setOrigin(0, 0.5);

    this.hpText  = this.add.text(PAD + 22 + this.BAR_W + 6, PAD - 1, '', {
      fontFamily: 'monospace', fontSize: '12px', color: '#ff6666',
    });

    // ── MP Bar ─────────────────────────────────────────────────────────────
    this.add.text(PAD, PAD + 22, 'MP', { fontFamily: 'serif', fontSize: '13px', color: '#4488cc' });
    this.mpBarBg = this.add.rectangle(PAD + 22, PAD + 28, this.BAR_W, 10, 0x001133).setOrigin(0, 0.5);
    this.mpBar   = this.add.rectangle(PAD + 22, PAD + 28, this.BAR_W, 10, 0x2266cc).setOrigin(0, 0.5);

    this.mpText  = this.add.text(PAD + 22 + this.BAR_W + 6, PAD + 20, '', {
      fontFamily: 'monospace', fontSize: '12px', color: '#66aaff',
    });

    // ── Level / EXP ────────────────────────────────────────────────────────
    this.lvlText = this.add.text(PAD, PAD + 44, 'LVL 1', {
      fontFamily: 'serif', fontSize: '13px', color: '#ccaaff',
    });

    this.expBar = this.add.rectangle(PAD + 44, PAD + 50, 0, 4, 0xaa66ff).setOrigin(0, 0.5);
    this.add.rectangle(PAD + 44, PAD + 50, this.BAR_W, 4, 0x220044).setOrigin(0, 0.5);

    // ── Level Up flash ─────────────────────────────────────────────────────
    const { width, height } = this.scale;
    this.levelUpText = this.add.text(width / 2, height / 2 - 60, 'LEVEL UP!', {
      fontFamily: '"Palatino Linotype", serif',
      fontSize:   '40px',
      color:      '#ffdd00',
      stroke:     '#884400',
      strokeThickness: 6,
    }).setOrigin(0.5).setAlpha(0);

    // Initial refresh
    this.refreshBars();
  }

  private refreshBars(): void {
    const { hp, maxHp, mp, maxMp, level, exp, expToNext } = GameState.save.stats;
    const hpRatio = maxHp > 0 ? hp / maxHp : 0;
    const mpRatio = maxMp > 0 ? mp / maxMp : 0;
    const expRatio = expToNext > 0 ? exp / expToNext : 0;

    this.hpBar.width  = Math.max(0, this.BAR_W * hpRatio);
    this.mpBar.width  = Math.max(0, this.BAR_W * mpRatio);
    this.expBar.width = Math.max(0, this.BAR_W * expRatio);

    this.hpText.setText(`${hp}/${maxHp}`);
    this.mpText.setText(`${mp}/${maxMp}`);
    this.lvlText.setText(`LVL ${level}`);

    // Danger flash
    if (hpRatio < 0.25) {
      this.hpBar.setFillStyle(0xff0000);
    } else if (hpRatio < 0.5) {
      this.hpBar.setFillStyle(0xff6600);
    } else {
      this.hpBar.setFillStyle(0xcc2222);
    }
  }

  private onDamaged = () => {
    this.refreshBars();
    // Shake HP bar
    this.tweens.add({
      targets: [this.hpBar, this.hpBarBg, this.hpText],
      x: '+=4', yoyo: true, repeat: 3, duration: 40,
    });
  };

  private onExpGained = () => { this.refreshBars(); };

  private onLevelUp = (data: { level: number }) => {
    this.refreshBars();
    this.levelUpText.setText(`LEVEL UP!  LVL ${data.level}`).setAlpha(1);
    this.tweens.add({
      targets:  this.levelUpText,
      y:        '-=40',
      alpha:    0,
      duration: 2000,
      ease:     'Power2',
    });
  };

  shutdown(): void {
    bus.off(EVENTS.PLAYER_DAMAGED,    this.onDamaged);
    bus.off(EVENTS.PLAYER_LEVELED_UP, this.onLevelUp);
    bus.off(EVENTS.ENEMY_DIED,        this.onExpGained);
  }
}
