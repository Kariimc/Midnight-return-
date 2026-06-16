import Phaser from 'phaser';

export class PreloadScene extends Phaser.Scene {
  constructor() { super({ key: 'PreloadScene' }); }

  preload(): void {
    const { width, height } = this.scale;

    // Loading bar UI
    const barBg   = this.add.rectangle(width / 2, height / 2, 400, 20, 0x222222);
    const barFill = this.add.rectangle(width / 2 - 198, height / 2, 0, 16, 0xcc8800).setOrigin(0, 0.5);
    const label   = this.add.text(width / 2, height / 2 + 30, 'LOADING...', {
      fontFamily: 'serif', fontSize: '16px', color: '#888888',
    }).setOrigin(0.5);

    this.load.on('progress', (v: number) => {
      barFill.width = 396 * v;
      label.setText(`LOADING... ${Math.floor(v * 100)}%`);
    });

    // ── Placeholder geometry sprites (no real assets yet) ─────────────────
    // These will be replaced by real spritesheet assets
    this.generatePlaceholderTextures();
  }

  private generatePlaceholderTextures(): void {
    // We procedurally generate placeholder textures so the game runs without real assets
    const g = this.make.graphics({ x: 0, y: 0 } as Phaser.Types.GameObjects.Graphics.Options);

    // Player
    g.fillStyle(0xcc8800).fillRect(0, 0, 32, 56);
    g.fillStyle(0xffcc88).fillRect(8, 2, 16, 16); // head
    g.fillStyle(0x880000).fillRect(0, 18, 32, 30); // cape
    g.generateTexture('player', 32, 56);

    // Generic enemy placeholder (gray rect)
    g.clear().fillStyle(0x666688).fillRect(0, 0, 28, 52);
    g.generateTexture('enemy_zombie',       28, 52);
    g.generateTexture('enemy_skeleton',     24, 52);
    g.generateTexture('enemy_axe_knight',   36, 58);
    g.generateTexture('enemy_medusa_head',  22, 22);
    g.generateTexture('enemy_ghost',        30, 40);
    g.generateTexture('enemy_bone_archer',  24, 52);
    g.generateTexture('enemy_warg',         48, 36);
    g.generateTexture('enemy_harpy',        40, 34);
    g.generateTexture('enemy_merman',       30, 50);
    g.generateTexture('enemy_sorcerer',     26, 56);
    g.generateTexture('enemy_gargoyle',     40, 44);
    g.generateTexture('enemy_blood_skeleton', 24, 52);
    g.generateTexture('enemy_scythe_knight', 34, 58);
    g.generateTexture('enemy_gear_golem',   56, 64);
    g.generateTexture('enemy_vampire_bat',  20, 16);
    g.generateTexture('enemy_dark_knight',  38, 60);
    g.generateTexture('enemy_succubus',     30, 54);

    // Particle dot
    g.clear().fillStyle(0xffffff).fillCircle(4, 4, 4);
    g.generateTexture('particle_dot', 8, 8);

    // Tiles
    g.clear().fillStyle(0x334455).fillRect(0, 0, 32, 32)
      .lineStyle(1, 0x445566).strokeRect(0, 0, 32, 32);
    g.generateTexture('tile_stone', 32, 32);

    g.clear().fillStyle(0x221133).fillRect(0, 0, 32, 32);
    g.generateTexture('tile_bg', 32, 32);

    // Platform
    g.clear().fillStyle(0x556677).fillRect(0, 0, 32, 16);
    g.generateTexture('tile_platform', 32, 16);

    // Item icons (tiny colored squares)
    const itemColors: Record<string, number> = {
      item_short_sword: 0xaaaaff, item_whip: 0xffee00, item_claymore: 0x4488ff,
      item_battle_axe: 0xff6600, item_shadow_blade: 0x6600aa, item_holy_lance: 0xffffff,
      item_death_scythe: 0x00ff44, item_flame_sword: 0xff3300,
    };
    for (const [key, color] of Object.entries(itemColors)) {
      g.clear().fillStyle(color).fillRect(0, 0, 24, 24);
      g.generateTexture(key, 24, 24);
    }

    g.destroy();
  }

  create(): void {
    // Add placeholder animations
    this.createAnimations();
    this.scene.start('MenuScene');
  }

  private createAnimations(): void {
    // Player animations (single-frame placeholders until real spritesheets arrive)
    const anims: Array<{ key: string; frame: string; frameRate: number; repeat: number }> = [
      { key: 'player_idle',     frame: 'player', frameRate: 8,  repeat: -1 },
      { key: 'player_run',      frame: 'player', frameRate: 12, repeat: -1 },
      { key: 'player_jump',     frame: 'player', frameRate: 8,  repeat: 0  },
      { key: 'player_fall',     frame: 'player', frameRate: 8,  repeat: 0  },
      { key: 'player_dash',     frame: 'player', frameRate: 12, repeat: 0  },
      { key: 'player_attack_1', frame: 'player', frameRate: 16, repeat: 0  },
      { key: 'player_attack_2', frame: 'player', frameRate: 16, repeat: 0  },
      { key: 'player_attack_3', frame: 'player', frameRate: 16, repeat: 0  },
    ];

    for (const a of anims) {
      if (!this.anims.exists(a.key)) {
        this.anims.create({
          key:      a.key,
          frames:   [{ key: a.frame }],
          frameRate: a.frameRate,
          repeat:   a.repeat,
        });
      }
    }
  }
}
