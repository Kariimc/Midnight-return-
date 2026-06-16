import Phaser from 'phaser';
import { Player }        from '../entities/player/Player';
import { VFXSystem }     from '../systems/VFXSystem';
import { CameraSystem }  from '../systems/CameraSystem';
import { GameState }     from '../core/GameState';
import { bus, EVENTS }   from '../utils/EventBus';
import { addExp }        from '../core/PlayerStats';
import { ENEMY_MAP }     from '../data/enemies';

const WORLD_WIDTH  = 4800;
const WORLD_HEIGHT = 2400;

interface AttackHitbox {
  rect:   Phaser.GameObjects.Rectangle;
  damage: number;
  dir:    number;
  life:   number;
}

export class GameScene extends Phaser.Scene {
  player!:     Player;
  vfx!:        VFXSystem;
  camera!:     CameraSystem;

  private ground!:     Phaser.Physics.Arcade.StaticGroup;
  private platforms!:  Phaser.Physics.Arcade.StaticGroup;
  private enemies:     Phaser.Physics.Arcade.Sprite[] = [];
  private hitboxes:    AttackHitbox[] = [];
  private isPaused = false;

  constructor() { super({ key: 'GameScene' }); }

  create(): void {
    this.physics.world.setBounds(0, 0, WORLD_WIDTH, WORLD_HEIGHT);
    this.physics.world.gravity.y = 0; // We control gravity per-entity

    // ── Background ─────────────────────────────────────────────────────────
    this.buildBackground();

    // ── Tilemaps / Collision Geometry ──────────────────────────────────────
    this.buildLevel();

    // ── Player ────────────────────────────────────────────────────────────
    this.player = new Player(this, 200, 800);
    this.physics.add.collider(this.player.sprite, this.ground);
    this.physics.add.collider(this.player.sprite, this.platforms);

    // ── VFX & Camera ──────────────────────────────────────────────────────
    this.vfx    = new VFXSystem(this);
    this.camera = new CameraSystem(this, WORLD_WIDTH, WORLD_HEIGHT);
    this.camera.follow(this.player);

    // ── Enemy spawns ───────────────────────────────────────────────────────
    this.spawnInitialEnemies();

    // ── Event wiring ──────────────────────────────────────────────────────
    this.events.on('player_attack', this.onPlayerAttack, this);
    bus.on(EVENTS.ENEMY_DIED, this.onEnemyDied);
    bus.on(EVENTS.PLAYER_DIED, this.onPlayerDied);

    // Pause
    this.input.keyboard!.on('keydown-ESC', () => this.togglePause());

    GameState.markRoomVisited('entrance_hall');
  }

  update(_time: number, delta: number): void {
    if (this.isPaused) return;
    const dt = delta / 1000;

    this.player.update(dt);
    this.camera.update(dt, this.player);
    this.vfx.update(dt);

    // Update enemy hitbox collisions
    this.updateHitboxes(dt);

    // Update enemies
    // (Enemies would call their own update here)
  }

  // ── Level Builder ─────────────────────────────────────────────────────────
  private buildBackground(): void {
    // Parallax layers (tiled rects simulating gothic castle background)
    const layers = [
      { color: 0x050510, depth: 0,   scroll: 0.1 },
      { color: 0x0a0820, depth: 1,   scroll: 0.2 },
      { color: 0x111030, depth: 2,   scroll: 0.35 },
    ];
    layers.forEach(l => {
      const bg = this.add.tileSprite(0, 0, WORLD_WIDTH * 2, WORLD_HEIGHT, 'tile_bg')
        .setOrigin(0, 0)
        .setTint(l.color)
        .setDepth(l.depth)
        .setScrollFactor(l.scroll);
    });
  }

  private buildLevel(): void {
    this.ground    = this.physics.add.staticGroup();
    this.platforms = this.physics.add.staticGroup();

    // Ground floor
    for (let x = 0; x < WORLD_WIDTH; x += 32) {
      const tile = this.ground.create(x + 16, WORLD_HEIGHT - 16, 'tile_stone') as Phaser.Physics.Arcade.Sprite;
      tile.setDepth(5);
    }

    // Platform layout — hand-crafted SotN-style interconnected rooms
    const platformDefs: Array<{ x: number; y: number; w: number }> = [
      // Room 1 - Entrance Hall
      { x: 96,   y: 900,  w: 5  },
      { x: 320,  y: 780,  w: 4  },
      { x: 520,  y: 680,  w: 3  },
      { x: 700,  y: 820,  w: 6  },
      // Room 2 - Upper Gallery
      { x: 900,  y: 600,  w: 8  },
      { x: 1100, y: 480,  w: 5  },
      { x: 1350, y: 700,  w: 4  },
      // Room 3 - Mid Castle
      { x: 1600, y: 850,  w: 6  },
      { x: 1800, y: 680,  w: 5  },
      { x: 2000, y: 550,  w: 4  },
      // Room 4 - Clocktower base
      { x: 2300, y: 900,  w: 8  },
      { x: 2500, y: 750,  w: 4  },
      { x: 2600, y: 600,  w: 4  },
      { x: 2700, y: 450,  w: 4  },
      // Room 5 - Throne approach
      { x: 3000, y: 800,  w: 10 },
      { x: 3200, y: 650,  w: 6  },
      { x: 3400, y: 500,  w: 5  },
    ];

    platformDefs.forEach(p => {
      for (let i = 0; i < p.w; i++) {
        const tile = this.platforms.create(
          p.x + i * 32, p.y, 'tile_platform'
        ) as Phaser.Physics.Arcade.Sprite;
        tile.setDepth(5).refreshBody();
      }
    });

    // Walls / vertical geometry
    const wallDefs: Array<{ x: number; y: number; h: number }> = [
      { x: 600,  y: 680,  h: 10 },
      { x: 900,  y: 600,  h: 12 },
      { x: 1600, y: 550,  h: 10 },
      { x: 2300, y: 450,  h: 14 },
      { x: 3000, y: 500,  h: 10 },
    ];
    wallDefs.forEach(w => {
      for (let i = 0; i < w.h; i++) {
        this.ground.create(w.x, w.y + i * 32, 'tile_stone').setDepth(5).refreshBody();
      }
    });
  }

  private spawnInitialEnemies(): void {
    const spawns: Array<{ id: string; x: number; y: number }> = [
      { id: 'zombie',      x: 350,  y: WORLD_HEIGHT - 60 },
      { id: 'skeleton',    x: 500,  y: WORLD_HEIGHT - 60 },
      { id: 'zombie',      x: 700,  y: WORLD_HEIGHT - 60 },
      { id: 'axe_knight',  x: 900,  y: WORLD_HEIGHT - 60 },
      { id: 'warg',        x: 1200, y: WORLD_HEIGHT - 60 },
      { id: 'harpy',       x: 1400, y: 600 },
      { id: 'bone_archer', x: 1600, y: 840 },
      { id: 'sorcerer',    x: 2000, y: 840 },
      { id: 'gargoyle',    x: 2400, y: 740 },
    ];

    spawns.forEach(s => {
      const def = ENEMY_MAP[s.id];
      if (!def) return;
      const sprite = this.physics.add.sprite(s.x, s.y, def.spriteKey);
      sprite.setDepth(8);
      sprite.body.setSize(def.hitboxW, def.hitboxH);
      sprite.setData('def', def);
      sprite.setData('hp',  def.hp);
      this.enemies.push(sprite);

      this.physics.add.collider(sprite, this.ground);
      this.physics.add.collider(sprite, this.platforms);
    });
  }

  // ── Attack hitbox from player state ──────────────────────────────────────
  private onPlayerAttack = (data: { x: number; y: number; dir: number; combo: number }) => {
    const stats  = GameState.save.stats;
    const damage = stats.atk + (data.combo * 3);

    const rect = this.add.rectangle(data.x, data.y, 60, 28, 0xffff00, 0.2).setDepth(15);
    this.hitboxes.push({ rect, damage, dir: data.dir, life: 0.08 });

    this.events.emit('hit_spark', { x: data.x, y: data.y });
    this.camera.shake(0.003, 80);
  };

  private updateHitboxes(dt: number): void {
    for (let hi = this.hitboxes.length - 1; hi >= 0; hi--) {
      const hb = this.hitboxes[hi];
      hb.life -= dt;

      // Check against enemies
      for (let ei = this.enemies.length - 1; ei >= 0; ei--) {
        const enemy = this.enemies[ei];
        if (!enemy.active) continue;

        const overlap = Phaser.Geom.Intersects.RectangleToRectangle(
          hb.rect.getBounds(), enemy.getBounds()
        );
        if (overlap) {
          const def  = enemy.getData('def');
          let hp     = enemy.getData('hp') as number;
          const dmg  = Math.max(1, hb.damage - Math.floor((def?.def ?? 0) * 0.3));
          hp -= dmg;
          enemy.setData('hp', hp);

          this.events.emit('damage_number', { x: enemy.x, y: enemy.y - 20, value: dmg });
          this.events.emit('hit_spark',     { x: enemy.x, y: enemy.y });

          if (hp <= 0) {
            this.events.emit('enemy_death_vfx', {
              x: enemy.x, y: enemy.y,
              color: def?.deathColor ?? 0xff4400,
              particles: def?.deathParticles ?? 40,
            });
            bus.emit(EVENTS.ENEMY_DIED, { id: def?.id, exp: def?.exp ?? 10 });
            enemy.destroy();
            this.enemies.splice(ei, 1);
          }
        }
      }

      if (hb.life <= 0) { hb.rect.destroy(); this.hitboxes.splice(hi, 1); }
    }
  }

  private onEnemyDied = (data: { id: string; exp: number }) => {
    const { stats, leveled } = addExp(GameState.save.stats, data.exp);
    GameState.save.stats = stats;
    if (leveled) {
      this.events.emit('screen_flash', 0xffdd00, 0.3);
      bus.emit(EVENTS.PLAYER_LEVELED_UP, { level: stats.level });
    }
  };

  private onPlayerDied = () => {
    this.camera.flash(0xff0000, 500);
    this.time.delayedCall(600, () => {
      this.scene.start('MenuScene');
    });
  };

  private togglePause(): void {
    this.isPaused = !this.isPaused;
    if (this.isPaused) {
      this.physics.pause();
      this.scene.pause();
    } else {
      this.physics.resume();
      this.scene.resume();
    }
  }

  shutdown(): void {
    bus.off(EVENTS.ENEMY_DIED, this.onEnemyDied);
    bus.off(EVENTS.PLAYER_DIED, this.onPlayerDied);
    this.vfx.destroy();
  }
}
