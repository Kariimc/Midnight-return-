import { IState } from '../../../utils/StateMachine';
import type { Player } from '../Player';

export class DashState implements IState {
  name = 'dash';
  constructor(private player: Player) {}

  onEnter(): void {
    this.player.sprite.play('player_dash', true);
    this.player.input.consumeDash();
    // Afterimage VFX
    this.player.scene.events.emit('player_dash_start', {
      x: this.player.sprite.x,
      y: this.player.sprite.y,
      dir: this.player.movement.facingDir,
    });
  }

  onUpdate(_dt: number): void {
    if (!this.player.movement.isDashing) {
      const snap = this.player.input.snapshot();
      this.player.fsm.transition(
        this.player.movement.isGrounded
          ? (snap.left || snap.right ? 'run' : 'idle')
          : 'fall'
      );
    }
  }

  onExit(): void {
    this.player.scene.events.emit('player_dash_end');
  }
}
