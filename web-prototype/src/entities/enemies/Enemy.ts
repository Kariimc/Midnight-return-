import Phaser from 'phaser';
import { StateMachine } from '../../utils/StateMachine';
import { bus, EVENTS }  from '../../utils/EventBus';
import type { EnemyDef } from '../../data/enemies';

export abstract class Enemy {
  sprite: Phaser.Physics.Arcade.Sprite;
  fsm:    StateMachine;

  hp:   number;
  maxHp: number;
  isAlive = true;

  protected aggroRange  = 300;
  protected attackRange = 60;
  protected detectedPlayer = false;

  constructor(
    protected scene: Phaser.Scene,
    x: number, y: number,
    public readonly def: EnemyDef,
  ) {
    this.sprite = scene.physics.add.sprite(x, y, def.spriteKey);
    this.sprite.setDepth(8);
    this.hp     = def.hp;
    this.maxHp  = def.hp;
    this.fsm    = new StateMachine();
    (this.sprite.body as Phaser.Physics.Arcade.Body).setSize(def.hitboxW, def.hitboxH);
    this.setupStates();
    this.fsm.transition('idle');
  }

  protected abstract setupStates(): void;

  update(dt: number, playerX: number, playerY: number): void {
    if (!this.isAlive) return;
    const dist = Phaser.Math.Distance.Between(this.sprite.x, this.sprite.y, playerX, playerY);
    this.detectedPlayer = dist < this.aggroRange;
    this.fsm.update(dt);
  }

  takeDamage(amount: number, damageType = 'physical'): number {
    if (!this.isAlive) return 0;

    let dmg = amount;
    const res = this.def.resistances?.[damageType] ?? 0;
    dmg = Math.max(1, Math.floor(dmg * (1 - res / 100)));

    this.hp = Math.max(0, this.hp - dmg);

    // Damage number VFX
    this.scene.events.emit('damage_number', { x: this.sprite.x, y: this.sprite.y - 20, value: dmg });

    if (this.hp <= 0) this.die();
    return dmg;
  }

  protected die(): void {
    this.isAlive = false;
    // Death particle burst
    this.scene.events.emit('enemy_death_vfx', {
      x:        this.sprite.x,
      y:        this.sprite.y,
      color:    this.def.deathColor ?? 0xff4400,
      particles: this.def.deathParticles ?? 40,
    });
    bus.emit(EVENTS.ENEMY_DIED, { id: this.def.id, exp: this.def.exp, x: this.sprite.x, y: this.sprite.y });
    this.sprite.destroy();
  }

  // Hitbox for player attacks to overlap
  get hitbox(): Phaser.Geom.Rectangle {
    return this.sprite.getBounds();
  }
}
