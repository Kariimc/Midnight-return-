import { IState } from '../../../utils/StateMachine';
import type { Player } from '../Player';

export class FallState implements IState {
  name = 'fall';
  constructor(private player: Player) {}

  onEnter(): void { this.player.sprite.play('player_fall', true); }

  onUpdate(_dt: number): void {
    const { input, movement } = this.player;
    const snap = input.snapshot();

    if (movement.isGrounded) {
      // Landing — spawn dust VFX
      this.player.scene.events.emit('player_land', {
        x: this.player.sprite.x,
        y: this.player.sprite.y + 20,
      });
      this.player.fsm.transition(snap.left || snap.right ? 'run' : 'idle');
      return;
    }
    if (snap.attack) { this.player.fsm.transition('attack'); return; }
    if (snap.jump)   { this.player.fsm.transition('jump');   return; }
    if (snap.dash)   { this.player.fsm.transition('dash');   return; }
    this.player.sprite.setFlipX(movement.facingDir === -1);
  }
}
