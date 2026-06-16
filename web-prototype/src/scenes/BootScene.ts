import Phaser from 'phaser';

export class BootScene extends Phaser.Scene {
  constructor() { super({ key: 'BootScene' }); }

  preload(): void {
    // Minimal assets for the loading bar
    this.load.image('loading_bg', 'assets/ui/loading_bg.png');
    this.load.image('loading_bar', 'assets/ui/loading_bar.png');
  }

  create(): void {
    this.scene.start('PreloadScene');
  }
}
