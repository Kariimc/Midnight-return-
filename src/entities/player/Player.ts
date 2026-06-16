import Phaser from 'phaser';
import { StateMachine } from '../../utils/StateMachine';
import { PlayerInput } from './PlayerInput';
import { PlayerMovement } from './PlayerMovement';
import { IdleState }   from './states/PlayerStateIdle';
import { RunState }    from './states/PlayerStateRun';
import { JumpState }   from './states/PlayerStateJump';
import { FallState }   from './states/PlayerStateFall';
import { DashState }   from './states/PlayerStateDash';
import { AttackState } from './states/PlayerStateAttack';
import { GameState }   from '../../core/GameState';
import { bus, EVENTS } from '../../utils/EventBus';
import { clamp }       from '../../utils/MathUtils';

export class Player {
  sprite: Phaser.Physics.Arcade.Sprite;
  input:    PlayerInput;
  movement: PlayerMovement;
  fsm:      StateMachine;

  private invincibleTimer = 0;
  private readonly INVINCIBLE_DURATION = 1.0;

  constructor(
    public scene: Phaser.Scene,
    x: number, y: number,
  ) {
    this.sprite = scene.physics.add.sprite(x, y, 'player');
    this.sprite.setCollideWorldBounds(true);
    (this.sprite.body as Phaser.Physics.Arcade.Body).setSize(28, 52);
    this.sprite.setDepth(10);

    this.input    = new PlayerInput(scene);
    this.movement = new PlayerMovement({
      canDoubleJump: false, // unlockable
      canWallJump:   true,
      canAirDash:    false, // unlockable
      canWallCling:  true,
    });

    this.fsm = new StateMachine()
      .add(new IdleState(this))
      .add(new RunState(this))
      .add(new JumpState(this))
      .add(new FallState(this))
      .add(new DashState(this))
      .add(new AttackState(this));

    this.fsm.transition('idle');
  }

  update(dt: number): void {
    this.input.update();
    const snap = this.input.snapshot();

    // Drive movement regardless of state
    this.movement.update(
      dt,
      {
        left:     snap.left,
        right:    snap.right,
        jump:     snap.jump,
        dash:     snap.dash,
        jumpHeld: this.scene.input.keyboard!
          .checkDown(this.scene.input.keyboard!.addKey(Phaser.Input.Keyboard.KeyCodes.SPACE)),
      },
      this.sprite.body as Phaser.Physics.Arcade.Body,
    );

    // Update FSM
    this.fsm.update(dt);

    // Flip sprite to face direction
    this.sprite.setFlipX(this.movement.facingDir === -1);

    // Invincibility flash
    if (this.invincibleTimer > 0) {
      this.invincibleTimer -= dt;
      this.sprite.setAlpha(Math.floor(this.invincibleTimer * 10) % 2 === 0 ? 1 : 0.3);
    } else {
      this.sprite.setAlpha(1);
    }
  }

  takeDamage(amount: number): void {
    if (this.invincibleTimer > 0) return;
    const stats = GameState.save.stats;
    const dmg   = Math.max(1, amount - Math.floor(stats.def * 0.3));
    stats.hp    = Math.max(0, stats.hp - dmg);
    this.invincibleTimer = this.INVINCIBLE_DURATION;

    bus.emit(EVENTS.PLAYER_DAMAGED, { dmg, hp: stats.hp });

    if (stats.hp <= 0) {
      bus.emit(EVENTS.PLAYER_DIED, {});
    }
  }

  heal(amount: number): void {
    const stats = GameState.save.stats;
    stats.hp    = clamp(stats.hp + amount, 0, stats.maxHp);
  }

  get x(): number { return this.sprite.x; }
  get y(): number { return this.sprite.y; }
  get body(): Phaser.Physics.Arcade.Body {
    return this.sprite.body as Phaser.Physics.Arcade.Body;
  }
}
