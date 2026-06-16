import Phaser from 'phaser';

// Sub-frame input buffer — queues inputs so frame-perfect moves always register
const BUFFER_FRAMES = 8;

export interface InputSnapshot {
  left:   boolean;
  right:  boolean;
  up:     boolean;
  down:   boolean;
  jump:   boolean;
  dash:   boolean;
  attack: boolean;
  spell:  boolean;
  interact: boolean;
}

export class PlayerInput {
  private keys!: {
    left:   Phaser.Input.Keyboard.Key;
    right:  Phaser.Input.Keyboard.Key;
    up:     Phaser.Input.Keyboard.Key;
    down:   Phaser.Input.Keyboard.Key;
    jump:   Phaser.Input.Keyboard.Key;
    dash:   Phaser.Input.Keyboard.Key;
    attack: Phaser.Input.Keyboard.Key;
    spell:  Phaser.Input.Keyboard.Key;
    interact: Phaser.Input.Keyboard.Key;
    jumpAlt: Phaser.Input.Keyboard.Key;
  };

  // Per-action input buffers (counts down each frame)
  jumpBuffer   = 0;
  dashBuffer   = 0;
  attackBuffer = 0;

  constructor(private scene: Phaser.Scene) {
    const kb = scene.input.keyboard!;
    this.keys = {
      left:     kb.addKey(Phaser.Input.Keyboard.KeyCodes.A),
      right:    kb.addKey(Phaser.Input.Keyboard.KeyCodes.D),
      up:       kb.addKey(Phaser.Input.Keyboard.KeyCodes.W),
      down:     kb.addKey(Phaser.Input.Keyboard.KeyCodes.S),
      jump:     kb.addKey(Phaser.Input.Keyboard.KeyCodes.SPACE),
      jumpAlt:  kb.addKey(Phaser.Input.Keyboard.KeyCodes.W),
      dash:     kb.addKey(Phaser.Input.Keyboard.KeyCodes.SHIFT),
      attack:   kb.addKey(Phaser.Input.Keyboard.KeyCodes.J),
      spell:    kb.addKey(Phaser.Input.Keyboard.KeyCodes.K),
      interact: kb.addKey(Phaser.Input.Keyboard.KeyCodes.E),
    };
  }

  update(): void {
    // Decrement buffers
    if (this.jumpBuffer   > 0) this.jumpBuffer--;
    if (this.dashBuffer   > 0) this.dashBuffer--;
    if (this.attackBuffer > 0) this.attackBuffer--;

    // Fill buffers on fresh press
    if (Phaser.Input.Keyboard.JustDown(this.keys.jump) ||
        Phaser.Input.Keyboard.JustDown(this.keys.jumpAlt)) {
      this.jumpBuffer = BUFFER_FRAMES;
    }
    if (Phaser.Input.Keyboard.JustDown(this.keys.dash))   this.dashBuffer   = BUFFER_FRAMES;
    if (Phaser.Input.Keyboard.JustDown(this.keys.attack)) this.attackBuffer = BUFFER_FRAMES;
  }

  snapshot(): InputSnapshot {
    return {
      left:     this.keys.left.isDown,
      right:    this.keys.right.isDown,
      up:       this.keys.up.isDown,
      down:     this.keys.down.isDown,
      jump:     this.jumpBuffer > 0,
      dash:     this.dashBuffer > 0,
      attack:   this.attackBuffer > 0,
      spell:    Phaser.Input.Keyboard.JustDown(this.keys.spell),
      interact: Phaser.Input.Keyboard.JustDown(this.keys.interact),
    };
  }

  consumeJump():   void { this.jumpBuffer   = 0; }
  consumeDash():   void { this.dashBuffer   = 0; }
  consumeAttack(): void { this.attackBuffer = 0; }

  isHoldingDown(): boolean { return this.keys.down.isDown; }
  isHoldingUp():   boolean { return this.keys.up.isDown; }
}
