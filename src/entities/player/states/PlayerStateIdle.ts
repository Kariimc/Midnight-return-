import { IState } from '../../../utils/StateMachine';
import type { Player } from '../Player';

export class IdleState implements IState {
  name = 'idle';
  constructor(private player: Player) {}

  onEnter(): void {
    this.player.sprite.play('player_idle', true);
  }

  onUpdate(_dt: number): void {
    const { input, movement } = this.player;
    const snap = input.snapshot();

    if (!movement.isGrounded) { this.player.fsm.transition('fall'); return; }
    if (snap.attack)          { this.player.fsm.transition('attack'); return; }
    if (snap.jump)            { this.player.fsm.transition('jump'); return; }
    if (snap.dash)            { this.player.fsm.transition('dash'); return; }
    if (snap.left || snap.right) { this.player.fsm.transition('run'); return; }
  }
}
