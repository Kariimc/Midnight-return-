import { IState } from '../../../utils/StateMachine';
import type { Player } from '../Player';

export class JumpState implements IState {
  name = 'jump';
  constructor(private player: Player) {}

  onEnter(): void {
    this.player.sprite.play('player_jump', true);
    this.player.input.consumeJump();
  }

  onUpdate(_dt: number): void {
    const { input, movement } = this.player;
    const snap = input.snapshot();

    if (movement.isGrounded)  { this.player.fsm.transition('idle'); return; }
    if (snap.attack)           { this.player.fsm.transition('attack'); return; }
    if (snap.dash)             { this.player.fsm.transition('dash'); return; }
    if ((this.player.sprite.body as Phaser.Physics.Arcade.Body).velocity.y > 50) {
      this.player.fsm.transition('fall');
    }

    this.player.sprite.setFlipX(movement.facingDir === -1);
  }
}
