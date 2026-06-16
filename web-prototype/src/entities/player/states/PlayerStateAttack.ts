import { IState } from '../../../utils/StateMachine';
import type { Player } from '../Player';

export class AttackState implements IState {
  name = 'attack';
  private combo = 0;
  private comboTimer = 0;
  private animDone = false;
  private readonly COMBO_WINDOW = 0.4;
  private readonly COMBO_MAX    = 3;

  constructor(private player: Player) {}

  onEnter(): void {
    this.animDone = false;
    this.comboTimer = this.COMBO_WINDOW;
    this.player.input.consumeAttack();

    const anim = `player_attack_${this.combo + 1}`;
    this.player.sprite.play(anim, true);

    this.player.sprite.once('animationcomplete', () => {
      this.animDone = true;
    });

    // Spawn hitbox via scene event
    this.player.scene.events.emit('player_attack', {
      x: this.player.sprite.x + this.player.movement.facingDir * 40,
      y: this.player.sprite.y,
      dir: this.player.movement.facingDir,
      combo: this.combo,
    });
  }

  onUpdate(dt: number): void {
    this.comboTimer -= dt;
    const snap = this.player.input.snapshot();

    if (snap.attack && this.animDone && this.combo < this.COMBO_MAX - 1) {
      this.combo++;
      this.onEnter();
      return;
    }

    if (this.animDone && (this.comboTimer <= 0 || !snap.attack)) {
      this.combo = 0;
      this.player.fsm.transition(
        this.player.movement.isGrounded ? 'idle' : 'fall'
      );
    }
  }

  onExit(): void {
    this.combo = 0;
  }
}
