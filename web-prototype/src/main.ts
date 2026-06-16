import Phaser from 'phaser';
import { BootScene }    from './scenes/BootScene';
import { PreloadScene } from './scenes/PreloadScene';
import { MenuScene }    from './scenes/MenuScene';
import { GameScene }    from './scenes/GameScene';
import { HUDScene }     from './scenes/HUDScene';

const config: Phaser.Types.Core.GameConfig = {
  type: Phaser.AUTO,   // WebGL → Canvas fallback
  width: 960,
  height: 540,
  backgroundColor: '#000000',
  parent: 'game-container',
  pixelArt: false,
  antialias: true,
  scene: [BootScene, PreloadScene, MenuScene, GameScene, HUDScene],
  physics: {
    default: 'arcade',
    arcade: {
      gravity: { x: 0, y: 0 }, // per-entity gravity via setGravityY
      debug:   false,
    },
  },
  scale: {
    mode:            Phaser.Scale.FIT,
    autoCenter:      Phaser.Scale.CENTER_BOTH,
    width:           960,
    height:          540,
  },
  render: {
    antialias:         true,
    antialiasGL:       true,
    roundPixels:       false,
    powerPreference:   'high-performance',
  },
};

new Phaser.Game(config);
