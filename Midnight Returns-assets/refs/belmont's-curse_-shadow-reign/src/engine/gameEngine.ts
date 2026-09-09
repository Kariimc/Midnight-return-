/**
 * Belmont's Curse: Shadow Reign - Core 60+ FPS Metroidvania Game Engine
 * Manages player mechanics, combat combo state machine, enemy AI, boss phases,
 * dynamic particles, lighting, camera tracking, floating text, and map state.
 */

import {
  CapePoint,
  DynamicLight,
  EnemyInstance,
  EnemyType,
  EquipmentItem,
  EquipmentSlotType,
  FloatingText,
  HairParticle,
  Hitbox,
  MapRoom,
  MapZone,
  PlayerAbility,
  PlayerStats,
  WeaponType
} from '../types/game';
import { audio } from '../utils/audio';
import { INITIAL_ITEMS, WORLD_ZONES } from './mapData';
import { ParticleEngine } from './particles';
import { PhysicsEngine } from './physics';

export class GameEngine {
  public zones: MapZone[] = WORLD_ZONES;
  public currentZoneId: string = 'zone_courtyard';
  public currentRoomId: string = 'room_courtyard_1';

  // Player State
  public playerStats: PlayerStats = {
    level: 1,
    xp: 0,
    nextLevelXp: 100,
    hp: 120,
    maxHp: 120,
    mp: 60,
    maxMp: 60,
    str: 15,
    con: 12,
    int: 10,
    lck: 8,
    gold: 50,
    soulShards: 20,
    darkOre: 5
  };

  public equipped: Record<EquipmentSlotType, EquipmentItem | null> = {
    weapon: INITIAL_ITEMS.vampire_killer,
    helmet: null,
    armor: null,
    cape: INITIAL_ITEMS.blood_cloak,
    accessory: null,
    subweapon: null
  };

  public inventory: EquipmentItem[] = [
    INITIAL_ITEMS.vampire_killer,
    INITIAL_ITEMS.blood_cloak
  ];

  // Gamepad State & Menu Request Callbacks
  public gamepadConnected: boolean = false;
  public gamepadName: string = '';
  private prevGamepadButtons: boolean[] = [];
  public onToggleMenuRequest?: (tab?: 'equipment' | 'relics' | 'map' | 'forge' | 'settings') => void;

  public abilities: Record<string, boolean> = {
    double_jump: true,
    dash_slide: true,
    parry_counter: true,
    holy_flame: true,
    soul_steal: false,
    gravity_boots: false
  };

  // Player Physics & Position
  public playerPos = { x: 200, y: 640 };
  public playerVel = { x: 0, y: 0 };
  public playerWidth = 28;
  public playerHeight = 56;
  public facing: 'left' | 'right' = 'right';
  public animState: 'idle' | 'run' | 'jump' | 'fall' | 'slide' | 'attack' | 'dash' | 'hit' = 'idle';
  public animFrame: number = 0;
  public isGrounded: boolean = false;
  public isWallSliding: boolean = false;
  public jumpCount: number = 0;
  public maxJumps: number = 2;

  // Combat Timers
  public attackCooldown: number = 0;
  public attackComboStep: number = 0;
  public isAttacking: boolean = false;
  public attackProgress: number = 0; // 0 to 1
  public dashCooldown: number = 0;
  public isDashing: boolean = false;
  public dashTimer: number = 0;
  public invulnerableFrames: number = 0;
  public parryWindowFrames: number = 0;

  //verlet physics chains
  public capePoints: CapePoint[] = Array.from({ length: 6 }, () => ({ x: 200, y: 640, oldX: 200, oldY: 640 }));
  public hairPoints: HairParticle[] = Array.from({ length: 4 }, () => ({ x: 200, y: 640, oldX: 200, oldY: 640 }));

  // Room Instances
  public activeEnemies: EnemyInstance[] = [];
  public floatingTexts: FloatingText[] = [];
  public particleEngine: ParticleEngine = new ParticleEngine();

  // Camera & FX
  public camera = { x: 0, y: 0 };
  public cameraShake = 0;
  public screenWidth = 1280;
  public screenHeight = 720;

  // Input Map
  public keys: Record<string, boolean> = {};

  // Saved Data Tracking
  public openedChests: Set<string> = new Set();
  public defeatedBosses: Set<string> = new Set();

  constructor() {
    this.loadRoom(this.currentRoomId);
  }

  // Calculate combined attributes including equipment stats
  public getComputedStats() {
    let bonusAtk = 0;
    let bonusDef = 0;
    let bonusCrit = 0;

    Object.values(this.equipped).forEach((item) => {
      if (item && item.stats) {
        bonusAtk += item.stats.attack || 0;
        bonusDef += item.stats.defense || 0;
        bonusCrit += item.stats.critChance || 0;
      }
    });

    const totalAtk = Math.floor(this.playerStats.str * 1.5 + bonusAtk);
    const totalDef = Math.floor(this.playerStats.con * 1.2 + bonusDef);
    const totalCrit = Math.min(0.75, 0.05 + this.playerStats.lck * 0.01 + bonusCrit);

    return { totalAtk, totalDef, totalCrit };
  }

  private getEnemyTypeDefinition(id: string): EnemyType {
    switch (id) {
      case 'skeleton':
        return { id, name: 'Crypt Skeleton', maxHp: 50, damage: 14, speed: 1.6, xpReward: 30, goldReward: 15, shardsReward: 4, width: 32, height: 52, color: '#e2e8f0', behavior: 'chaser', spriteType: 'skeleton', description: 'Reanimated bones wielding rusted iron.' };
      case 'bat':
        return { id, name: 'Vampire Bat', maxHp: 28, damage: 10, speed: 3.2, xpReward: 20, goldReward: 10, shardsReward: 2, width: 24, height: 24, color: '#475569', behavior: 'flyer', spriteType: 'bat', description: 'Aggressive flying bloodsucker.' };
      case 'specter':
        return { id, name: 'Phantom Specter', maxHp: 65, damage: 20, speed: 2.2, xpReward: 45, goldReward: 22, shardsReward: 6, width: 32, height: 52, color: '#38bdf8', behavior: 'chaser', spriteType: 'specter', description: 'Wraith floating through walls.' };
      case 'gargoyle':
        return { id, name: 'Stone Gargoyle', maxHp: 80, damage: 22, speed: 2.0, xpReward: 55, goldReward: 25, shardsReward: 8, width: 36, height: 48, color: '#64748b', behavior: 'chaser', spriteType: 'gargoyle', description: 'Carved guardian brought to life.' };
      case 'blood_leecher':
        return { id, name: 'Blood Leecher', maxHp: 75, damage: 24, speed: 1.8, xpReward: 50, goldReward: 28, shardsReward: 7, width: 30, height: 30, color: '#ef4444', behavior: 'chaser', spriteType: 'blood_leecher', description: 'Horrific creature born of acid blood.' };
      case 'arcane_tome':
        return { id, name: 'Living Grimoire', maxHp: 60, damage: 18, speed: 2.8, xpReward: 40, goldReward: 20, shardsReward: 5, width: 28, height: 28, color: '#a855f7', behavior: 'flyer', spriteType: 'arcane_tome', description: 'Animated spellbook shooting magic bolts.' };
      case 'clockwork_drone':
        return { id, name: 'Clockwork Automaton', maxHp: 90, damage: 26, speed: 2.1, xpReward: 65, goldReward: 35, shardsReward: 10, width: 34, height: 48, color: '#f59e0b', behavior: 'chaser', spriteType: 'clockwork_drone', description: 'Brass machine driven by gears.' };
      case 'frost_wraith':
        return { id, name: 'Frost Wraith', maxHp: 100, damage: 28, speed: 2.4, xpReward: 75, goldReward: 40, shardsReward: 12, width: 32, height: 52, color: '#38bdf8', behavior: 'chaser', spriteType: 'frost_wraith', description: 'Chilling spirit of the glacial abyss.' };
      case 'bog_witch':
        return { id, name: 'Toxic Bog Witch', maxHp: 110, damage: 30, speed: 1.9, xpReward: 85, goldReward: 45, shardsReward: 14, width: 32, height: 56, color: '#22c55e', behavior: 'chaser', spriteType: 'bog_witch', description: 'Caster of corrosive poison clouds.' };
      case 'void_fiend':
        return { id, name: 'Abyssal Void Fiend', maxHp: 140, damage: 36, speed: 2.6, xpReward: 110, goldReward: 60, shardsReward: 20, width: 36, height: 56, color: '#8b5cf6', behavior: 'chaser', spriteType: 'void_fiend', description: 'Nightmare spawn of the singularity.' };
        
      // Sub-Bosses
      case 'boss_gargoyle':
        return { id, name: 'Vanguard Gargoyle', maxHp: 450, damage: 25, speed: 2.4, xpReward: 400, goldReward: 200, shardsReward: 35, width: 56, height: 72, color: '#f59e0b', behavior: 'boss', spriteType: 'boss_gargoyle', description: 'Mighty stone lord.', isSubBoss: true };
      case 'boss_phantasm':
        return { id, name: 'Grave Warden Phantasm', maxHp: 550, damage: 28, speed: 2.5, xpReward: 500, goldReward: 250, shardsReward: 40, width: 56, height: 72, color: '#ef4444', behavior: 'boss', spriteType: 'boss_phantasm', description: 'Crypt phantom.', isSubBoss: true };
      case 'boss_archmage':
        return { id, name: 'Arch-Mage Solon', maxHp: 650, damage: 32, speed: 2.6, xpReward: 600, goldReward: 300, shardsReward: 45, width: 56, height: 72, color: '#c084fc', behavior: 'boss', spriteType: 'boss_archmage', description: 'Master of forbidden arcana.', isSubBoss: true };
      case 'boss_behemoth':
        return { id, name: 'Clockwork Behemoth', maxHp: 800, damage: 36, speed: 2.2, xpReward: 750, goldReward: 400, shardsReward: 55, width: 64, height: 80, color: '#f59e0b', behavior: 'boss', spriteType: 'boss_behemoth', description: 'Giant brass titan.', isSubBoss: true };
      case 'boss_wyrm':
        return { id, name: 'Glacial Wyrm Matriarch', maxHp: 950, damage: 40, speed: 2.7, xpReward: 900, goldReward: 500, shardsReward: 65, width: 68, height: 84, color: '#38bdf8', behavior: 'boss', spriteType: 'boss_wyrm', description: 'Frozen dragon of the peaks.', isSubBoss: true };
      case 'boss_hydra':
        return { id, name: 'Hydra Leviathan', maxHp: 1100, damage: 45, speed: 2.5, xpReward: 1100, goldReward: 600, shardsReward: 80, width: 70, height: 88, color: '#22c55e', behavior: 'boss', spriteType: 'boss_hydra', description: 'Multi-headed serpent.', isSubBoss: true };
      case 'boss_harbinger':
        return { id, name: 'Void Harbinger', maxHp: 1300, damage: 50, speed: 2.8, xpReward: 1400, goldReward: 750, shardsReward: 100, width: 72, height: 90, color: '#8b5cf6', behavior: 'boss', spriteType: 'boss_harbinger', description: 'Herald of the abyss.', isSubBoss: true };

      // Main Bosses
      case 'boss_malakor':
        return { id, name: 'Blood Warden Malakor', maxHp: 700, damage: 30, speed: 2.3, xpReward: 650, goldReward: 350, shardsReward: 50, width: 60, height: 80, color: '#ef4444', behavior: 'boss', spriteType: 'boss_malakor', description: 'Demon warden.' };
      case 'boss_executioner':
        return { id, name: 'Abyssal Executioner', maxHp: 850, damage: 35, speed: 2.4, xpReward: 800, goldReward: 450, shardsReward: 60, width: 64, height: 82, color: '#991b1b', behavior: 'boss', spriteType: 'boss_executioner', description: 'Grim reaper executioner.' };
      case 'boss_vespera':
        return { id, name: 'Arch-Phantom Vespera', maxHp: 1000, damage: 40, speed: 2.6, xpReward: 1000, goldReward: 550, shardsReward: 75, width: 64, height: 82, color: '#a855f7', behavior: 'boss', spriteType: 'boss_vespera', description: 'Sovereign of specters.' };
      case 'boss_ignis':
        return { id, name: 'Gearmaster Ignis', maxHp: 1200, damage: 45, speed: 2.5, xpReward: 1250, goldReward: 700, shardsReward: 90, width: 68, height: 84, color: '#f59e0b', behavior: 'boss', spriteType: 'boss_ignis', description: 'Overlord of the Clockwork Keep.' };
      case 'boss_empress':
        return { id, name: 'Rime Empress Lyra', maxHp: 1400, damage: 50, speed: 2.8, xpReward: 1500, goldReward: 850, shardsReward: 110, width: 70, height: 86, color: '#06b6d4', behavior: 'boss', spriteType: 'boss_empress', description: 'Monarch of the Glacial Abyss.' };
      case 'boss_cinder':
        return { id, name: 'Poison Queen Cinder', maxHp: 1600, damage: 55, speed: 2.7, xpReward: 1800, goldReward: 1000, shardsReward: 130, width: 72, height: 88, color: '#22c55e', behavior: 'boss', spriteType: 'boss_cinder', description: 'Ruler of the Sunken Crypts.' };
      case 'boss_belmont':
      default:
        return { id, name: 'Lord Belmont the Cursed', maxHp: 2200, damage: 65, speed: 3.0, xpReward: 3000, goldReward: 2000, shardsReward: 250, width: 76, height: 96, color: '#dc2626', behavior: 'boss', spriteType: 'boss_belmont', description: 'The Fallen Lord of Darkness.' };
    }
  }

  // Load Room state & spawn enemies/props
  public loadRoom(roomId: string) {
    this.currentRoomId = roomId;
    const room = this.getCurrentRoom();
    if (!room) return;

    room.explored = true;

    // Spawn Room Enemies
    this.activeEnemies = room.enemies.map((e, idx) => {
      const typeDef = this.getEnemyTypeDefinition(e.enemyTypeId);
      return {
        id: `${e.enemyTypeId}_${idx}_${Date.now()}`,
        type: typeDef,
        x: e.x,
        y: e.y,
        vx: 0,
        vy: 0,
        hp: typeDef.maxHp,
        maxHp: typeDef.maxHp,
        facing: 'left',
        state: 'patrol',
        attackCooldown: 0,
        aiTimer: 0,
        invulnerableFrames: 0
      };
    });

    // Spawn Boss if Trigger exists and not defeated
    if (room.bossTrigger && !this.defeatedBosses.has(room.bossTrigger.bossTypeId)) {
      const bType = room.bossTrigger.bossTypeId;
      const typeDef = this.getEnemyTypeDefinition(bType);

      this.activeEnemies.push({
        id: `boss_${bType}`,
        type: typeDef,
        x: room.bossTrigger.x,
        y: room.bossTrigger.y,
        vx: 0,
        vy: 0,
        hp: typeDef.maxHp,
        maxHp: typeDef.maxHp,
        facing: 'left',
        state: 'idle',
        attackCooldown: 0,
        aiTimer: 0,
        invulnerableFrames: 0,
        phase: 1
      });

      audio.startMusicTrack(bType === 'boss_belmont' ? 'final_boss' : 'boss');
    } else {
      audio.startMusicTrack(this.getCurrentZone()?.backgroundMusic || 'courtyard');
    }

    this.particleEngine.clear();
  }

  public getCurrentZone(): MapZone | undefined {
    return this.zones.find((z) => z.id === this.currentZoneId);
  }

  public getCurrentRoom(): MapRoom | undefined {
    const zone = this.getCurrentZone();
    return zone?.rooms.find((r) => r.id === this.currentRoomId);
  }

  // --- CORE FRAME TICK (60 FPS) ---
  public update(dt: number) {
    const room = this.getCurrentRoom();
    if (!room) return;

    this.animFrame++;

    // 0. GAMEPAD CONTROLLER POLLING
    this.pollGamepadInput();

    // 1. INPUT PROCESSING & MOVEMENT PHYSICS
    this.handlePlayerInput();

    // Gravity & Velocity
    if (!this.isGrounded) {
      this.playerVel.y += 0.55; // Gravity
      if (this.playerVel.y > 14) this.playerVel.y = 14;
    }

    // Platform Collisions
    const playerHitbox: Hitbox = {
      x: this.playerPos.x,
      y: this.playerPos.y,
      width: this.playerWidth,
      height: this.playerHeight
    };

    const res = PhysicsEngine.resolvePlatformCollisions(
      playerHitbox,
      this.playerVel.x,
      this.playerVel.y,
      room.platforms
    );

    this.playerPos.x = res.x;
    this.playerPos.y = res.y;
    this.playerVel.x = res.vx;
    this.playerVel.y = res.vy;
    this.isGrounded = res.isGrounded;

    if (this.isGrounded) {
      this.jumpCount = 0;
      this.isWallSliding = false;
    } else if (res.isWallTouching && this.playerVel.y > 0) {
      this.isWallSliding = true;
      this.playerVel.y = Math.min(this.playerVel.y, 2.5); // Wall slide damping
    }

    // Dynamic Animation State Selection
    if (this.invulnerableFrames > 0) this.invulnerableFrames--;
    if (this.attackCooldown > 0) this.attackCooldown--;
    if (this.dashCooldown > 0) this.dashCooldown--;

    if (this.isDashing) {
      this.dashTimer--;
      this.particleEngine.spawnGhostTrail(
        this.playerPos.x,
        this.playerPos.y,
        this.playerWidth,
        this.playerHeight,
        '#38bdf8'
      );
      if (this.dashTimer <= 0) {
        this.isDashing = false;
      }
    }

    if (this.isAttacking) {
      this.attackProgress = Math.min(1, this.attackProgress + 0.12);
      if (this.attackProgress >= 1) {
        this.isAttacking = false;
      }
    }

    if (this.isDashing) this.animState = 'dash';
    else if (this.isAttacking) this.animState = 'attack';
    else if (!this.isGrounded) this.animState = this.playerVel.y < 0 ? 'jump' : 'fall';
    else if (Math.abs(this.playerVel.x) > 0.5) this.animState = 'run';
    else this.animState = 'idle';

    // Update Cape & Hair Verlet Chains
    PhysicsEngine.updateCapePhysics(
      this.capePoints,
      this.playerPos.x + this.playerWidth / 2,
      this.playerPos.y + this.playerHeight,
      this.playerVel.x,
      this.playerVel.y,
      this.facing
    );

    PhysicsEngine.updateHairPhysics(
      this.hairPoints,
      this.playerPos.x + this.playerWidth / 2,
      this.playerPos.y + this.playerHeight,
      this.playerVel.x,
      this.facing
    );

    // 2. DOOR / ROOM TRANSITION CHECK
    this.checkDoorTransitions(room);

    // 2.5. ENVIRONMENTAL HAZARDS CHECK
    if (room.hazards) {
      room.hazards.forEach((hazard) => {
        if (PhysicsEngine.checkAABB(playerHitbox, hazard)) {
          if (hazard.type === 'acid_blood' || hazard.type === 'spikes' || hazard.type === 'crushing_gear' || hazard.type === 'falling_stalactite') {
            if (this.invulnerableFrames <= 0) {
              const dmg = hazard.damage || 15;
              this.playerStats.hp -= dmg;
              this.invulnerableFrames = 25;
              this.playerVel.y = -8;
              this.addFloatingText(`-${dmg} HAZARD!`, this.playerPos.x, this.playerPos.y, '#ef4444', 1.3);
              audio.playHazardHit();
            }
          } else if (hazard.type === 'steam_jet') {
            this.playerVel.y = -14; // Steam jet thermal lift
            if (this.invulnerableFrames <= 0) {
              this.playerStats.hp -= 5;
              this.invulnerableFrames = 15;
              this.addFloatingText('-5 STEAM!', this.playerPos.x, this.playerPos.y, '#f59e0b', 1.0);
              audio.playHazardHit();
            }
          } else if (hazard.type === 'mana_drain') {
            if (this.playerStats.mp > 0) {
              this.playerStats.mp = Math.max(0, this.playerStats.mp - 0.2);
            }
          } else if (hazard.type === 'void_well') {
            const centerX = hazard.x + hazard.width / 2;
            const pullDir = centerX > this.playerPos.x ? 1 : -1;
            this.playerVel.x += pullDir * 0.8;
          }
        }
      });
    }

    // 3. INTERACTION WITH CHESTS / ALTARS
    this.checkInteractions(room);

    // 4. UPDATE ENEMIES & AI
    this.updateEnemies(room);

    // 5. UPDATE FLOATING TEXT
    for (let i = this.floatingTexts.length - 1; i >= 0; i--) {
      const ft = this.floatingTexts[i];
      ft.life += dt;
      ft.y += ft.vy;
      if (ft.life >= ft.maxLife) this.floatingTexts.splice(i, 1);
    }

    // 6. CAMERA SMOOTH TRACKING (LERP)
    const targetCamX = this.playerPos.x - this.screenWidth / 2;
    const targetCamY = this.playerPos.y - this.screenHeight / 2;

    this.camera.x += (targetCamX - this.camera.x) * 0.1;
    this.camera.y += (targetCamY - this.camera.y) * 0.1;

    // Clamp camera within room bounds
    this.camera.x = Math.max(0, Math.min(room.width - this.screenWidth, this.camera.x));
    this.camera.y = Math.max(0, Math.min(room.height - this.screenHeight, this.camera.y));

    if (this.cameraShake > 0) {
      this.camera.x += (Math.random() - 0.5) * this.cameraShake;
      this.camera.y += (Math.random() - 0.5) * this.cameraShake;
      this.cameraShake *= 0.85;
      if (this.cameraShake < 0.5) this.cameraShake = 0;
    }
  }

  // Player Input
  private handlePlayerInput() {
    const runSpeed = 5.2;

    if (this.isDashing) return;

    if (this.keys['ArrowLeft'] || this.keys['a'] || this.keys['A']) {
      this.playerVel.x = -runSpeed;
      this.facing = 'left';
    } else if (this.keys['ArrowRight'] || this.keys['d'] || this.keys['D']) {
      this.playerVel.x = runSpeed;
      this.facing = 'right';
    } else {
      this.playerVel.x *= 0.7; // Friction
    }

    // Jump
    if ((this.keys[' '] || this.keys['w'] || this.keys['W']) && !this.keys['_jumpLock']) {
      this.keys['_jumpLock'] = true;
      if (this.isGrounded) {
        this.playerVel.y = -12.5;
        this.jumpCount = 1;
        this.isGrounded = false;
        audio.playDash();
      } else if (this.abilities.double_jump && this.jumpCount < this.maxJumps) {
        this.playerVel.y = -11.5;
        this.jumpCount++;
        this.particleEngine.spawnMagicRing(
          this.playerPos.x + this.playerWidth / 2,
          this.playerPos.y + this.playerHeight,
          '#38bdf8'
        );
        audio.playDash();
      }
    }
    if (!this.keys[' '] && !this.keys['w'] && !this.keys['W']) {
      this.keys['_jumpLock'] = false;
    }

    // Dash / Flash Shift
    if ((this.keys['Shift'] || this.keys['k'] || this.keys['K']) && this.dashCooldown <= 0 && this.abilities.dash_slide) {
      this.isDashing = true;
      this.dashTimer = 12;
      this.dashCooldown = 35;
      this.invulnerableFrames = 15;
      this.playerVel.x = (this.facing === 'right' ? 1 : -1) * 14;
      this.playerVel.y = 0;
      audio.playDash();
    }

    // Attack
    if ((this.keys['j'] || this.keys['J'] || this.keys['Enter']) && !this.isAttacking && this.attackCooldown <= 0) {
      this.performAttack();
    }

    // Spell Cast
    if ((this.keys['u'] || this.keys['U'] || this.keys['e'] || this.keys['E']) && this.playerStats.mp >= 15) {
      this.castSpell('holy_flame');
    }
  }

  // Attack Execution
  private performAttack() {
    this.isAttacking = true;
    this.attackProgress = 0;
    this.attackCooldown = 18;

    const { totalAtk, totalCrit } = this.getComputedStats();
    const weaponType = this.equipped.weapon?.weaponType || 'greatsword';

    audio.playSlash(
      weaponType === 'whip' ? 'whip' : weaponType === 'scythe' ? 'scythe' : 'light'
    );

    // Calculate Weapon Swing Hitbox
    const attackReach = weaponType === 'whip' ? 90 : weaponType === 'scythe' ? 100 : 70;
    const hitX =
      this.facing === 'right'
        ? this.playerPos.x + this.playerWidth
        : this.playerPos.x - attackReach;

    const attackHitbox: Hitbox = {
      x: hitX,
      y: this.playerPos.y - 10,
      width: attackReach,
      height: this.playerHeight + 20
    };

    // Check hit against active enemies
    this.activeEnemies.forEach((e) => {
      const enemyHitbox: Hitbox = { x: e.x, y: e.y, width: e.type.width, height: e.type.height };
      if (PhysicsEngine.checkAABB(attackHitbox, enemyHitbox) && e.invulnerableFrames <= 0) {
        const isCrit = Math.random() < totalCrit;
        const damage = Math.floor(totalAtk * (isCrit ? 1.8 : 1.0));

        e.hp -= damage;
        e.invulnerableFrames = 12;
        e.vx = (this.facing === 'right' ? 1 : -1) * 6; // Knockback

        // FX
        this.cameraShake = isCrit ? 10 : 4;
        this.particleEngine.spawnSparks(
          e.x + e.type.width / 2,
          e.y + e.type.height / 2,
          isCrit ? 16 : 8,
          isCrit ? '#ef4444' : '#fde047'
        );
        this.particleEngine.spawnBloodMist(e.x + e.type.width / 2, e.y + e.type.height / 2);

        this.addFloatingText(
          `${damage}${isCrit ? ' CRIT!' : ''}`,
          e.x + e.type.width / 2,
          e.y,
          isCrit ? '#ef4444' : '#fef08a',
          isCrit ? 1.4 : 1.0
        );

        audio.playHit(isCrit, e.type.behavior === 'boss');

        if (e.hp <= 0) {
          this.onEnemyDefeated(e);
        }
      }
    });
  }

  // Cast Spell
  private castSpell(spellId: string) {
    if (this.playerStats.mp < 15) return;
    this.playerStats.mp -= 15;

    audio.playSpell('holy');
    this.particleEngine.spawnMagicRing(
      this.playerPos.x + this.playerWidth / 2,
      this.playerPos.y + this.playerHeight / 2,
      '#fef08a'
    );

    // Holy Flame Wave
    const spellReach = 200;
    const hitX = this.facing === 'right' ? this.playerPos.x : this.playerPos.x - spellReach;

    const spellHitbox: Hitbox = {
      x: hitX,
      y: this.playerPos.y - 20,
      width: spellReach + this.playerWidth,
      height: this.playerHeight + 40
    };

    this.activeEnemies.forEach((e) => {
      const enemyHitbox: Hitbox = { x: e.x, y: e.y, width: e.type.width, height: e.type.height };
      if (PhysicsEngine.checkAABB(spellHitbox, enemyHitbox)) {
        const damage = Math.floor(this.playerStats.int * 2.5);
        e.hp -= damage;
        this.particleEngine.spawnSparks(e.x + e.type.width / 2, e.y + e.type.height / 2, 12, '#fde047');
        this.addFloatingText(`${damage} HOLY!`, e.x, e.y, '#fef08a', 1.3);
        if (e.hp <= 0) this.onEnemyDefeated(e);
      }
    });
  }

  // Enemy Slay Rewards
  private onEnemyDefeated(enemy: EnemyInstance) {
    this.playerStats.xp += enemy.type.xpReward;
    this.playerStats.gold += enemy.type.goldReward;
    this.playerStats.soulShards += enemy.type.shardsReward;

    audio.playItemPick();

    this.addFloatingText(
      `+${enemy.type.xpReward} XP`,
      enemy.x,
      enemy.y - 20,
      '#38bdf8',
      1.1
    );

    // Check Level Up
    if (this.playerStats.xp >= this.playerStats.nextLevelXp) {
      this.playerStats.level++;
      this.playerStats.xp -= this.playerStats.nextLevelXp;
      this.playerStats.nextLevelXp = Math.floor(this.playerStats.nextLevelXp * 1.5);

      this.playerStats.maxHp += 15;
      this.playerStats.hp = this.playerStats.maxHp;
      this.playerStats.maxMp += 10;
      this.playerStats.mp = this.playerStats.maxMp;
      this.playerStats.str += 3;
      this.playerStats.con += 2;

      this.addFloatingText('LEVEL UP!', this.playerPos.x, this.playerPos.y - 40, '#fde047', 1.6);
      this.particleEngine.spawnMagicRing(
        this.playerPos.x + this.playerWidth / 2,
        this.playerPos.y + this.playerHeight / 2,
        '#fde047'
      );
    }

    // Boss Defeated
    if (enemy.type.behavior === 'boss') {
      this.defeatedBosses.add(enemy.type.id);
      this.addFloatingText('BOSS SLAIN!', enemy.x, enemy.y - 40, '#ef4444', 2.0);
    }
  }

  // Update Enemy AI Logic
  private updateEnemies(room: MapRoom) {
    for (let i = this.activeEnemies.length - 1; i >= 0; i--) {
      const e = this.activeEnemies[i];

      if (e.invulnerableFrames > 0) e.invulnerableFrames--;

      // Simple AI Chase behavior
      const distToPlayer = Math.hypot(
        this.playerPos.x - e.x,
        this.playerPos.y - e.y
      );

      if (distToPlayer < 350) {
        e.facing = this.playerPos.x > e.x ? 'right' : 'left';
        const dirX = e.facing === 'right' ? 1 : -1;
        e.vx = dirX * e.type.speed;
      } else {
        e.vx *= 0.8;
      }

      e.x += e.vx;

      // Enemy Player Collision (Take Damage)
      const playerHitbox: Hitbox = {
        x: this.playerPos.x,
        y: this.playerPos.y,
        width: this.playerWidth,
        height: this.playerHeight
      };
      const enemyHitbox: Hitbox = { x: e.x, y: e.y, width: e.type.width, height: e.type.height };

      if (PhysicsEngine.checkAABB(playerHitbox, enemyHitbox) && this.invulnerableFrames <= 0 && !this.isDashing) {
        const { totalDef } = this.getComputedStats();
        const rawDamage = e.type.damage;
        const actualDamage = Math.max(1, rawDamage - Math.floor(totalDef * 0.3));

        this.playerStats.hp -= actualDamage;
        this.invulnerableFrames = 30;
        this.playerVel.x = (this.playerPos.x > e.x ? 1 : -1) * 8; // Knockback
        this.playerVel.y = -4;

        this.cameraShake = 8;
        this.addFloatingText(`-${actualDamage}`, this.playerPos.x, this.playerPos.y, '#ef4444', 1.2);
        audio.playHit(false, false);

        if (this.playerStats.hp <= 0) {
          // Respawn at save statue
          this.playerStats.hp = this.playerStats.maxHp;
          this.playerPos = { x: 150, y: 640 };
          this.addFloatingText('RESPAWNED AT SANCTUARY', this.playerPos.x, this.playerPos.y - 30, '#38bdf8', 1.2);
        }
      }

      if (e.hp <= 0) {
        this.activeEnemies.splice(i, 1);
      }
    }
  }

  // Room Doors / Transitions
  private checkDoorTransitions(room: MapRoom) {
    room.doors.forEach((door) => {
      let triggered = false;
      if (door.direction === 'right' && this.playerPos.x >= room.width - 40) triggered = true;
      if (door.direction === 'left' && this.playerPos.x <= 10) triggered = true;
      if (door.direction === 'top' && this.playerPos.y <= 10) triggered = true;

      if (triggered) {
        // Find target room
        this.zones.forEach((zone) => {
          const target = zone.rooms.find((r) => r.id === door.targetRoomId);
          if (target) {
            this.currentZoneId = zone.id;
            this.currentRoomId = target.id;

            // Reposition player
            if (door.direction === 'right') this.playerPos.x = 40;
            else if (door.direction === 'left') this.playerPos.x = target.width - 80;
            else if (door.direction === 'top') this.playerPos.y = target.height - 120;

            this.loadRoom(target.id);
          }
        });
      }
    });
  }

  // Chests & Statue Interactions
  private checkInteractions(room: MapRoom) {
    if (!room.chests) return;

    room.chests.forEach((chest) => {
      const dist = Math.hypot(
        this.playerPos.x - chest.x,
        this.playerPos.y - chest.y
      );

      if (dist < 40 && !chest.opened && (this.keys['e'] || this.keys['E'])) {
        chest.opened = true;
        this.openedChests.add(chest.id);

        if ('slot' in chest.item) {
          const eqItem = chest.item as EquipmentItem;
          this.inventory.push(eqItem);
          this.equipped[eqItem.slot] = eqItem; // Auto-equip new weapon/gear
          this.addFloatingText(`OBTAINED: ${eqItem.name}!`, chest.x, chest.y - 30, '#fde047', 1.3);
        }

        audio.playItemPick();
        this.particleEngine.spawnMagicRing(chest.x, chest.y, '#fde047');
      }
    });
  }

  public addFloatingText(text: string, x: number, y: number, color: string, scale: number = 1.0) {
    this.floatingTexts.push({
      id: Math.random().toString(),
      text,
      x,
      y,
      color,
      scale,
      life: 0,
      maxLife: 0.8,
      vy: -1.2
    });
  }

  // Get Dynamic Lighting Sources for current frame
  public getLightSources(): DynamicLight[] {
    const lights: DynamicLight[] = [];

    // Player Light
    lights.push({
      x: this.playerPos.x + this.playerWidth / 2 - this.camera.x,
      y: this.playerPos.y + this.playerHeight / 2 - this.camera.y,
      radius: 180,
      color: '#fef08a',
      intensity: 0.85,
      flicker: true
    });

    // Room Props Light (Torches)
    const room = this.getCurrentRoom();
    if (room?.backgroundProps) {
      room.backgroundProps.forEach((prop) => {
        if (prop.type === 'torch') {
          lights.push({
            x: prop.x - this.camera.x,
            y: prop.y - this.camera.y,
            radius: 140,
            color: '#f97316',
            intensity: 0.9,
            flicker: true
          });
        }
      });
    }

    return lights;
  }

  // --- GAMEPAD CONTROLLER API POLLING ENGINE ---
  private pollGamepadInput() {
    if (typeof navigator === 'undefined' || !navigator.getGamepads) return;

    const gamepads = navigator.getGamepads();
    let activeGamepad: Gamepad | null = null;

    for (let i = 0; i < gamepads.length; i++) {
      const gp = gamepads[i];
      if (gp && gp.connected) {
        activeGamepad = gp;
        break;
      }
    }

    if (!activeGamepad) {
      if (this.gamepadConnected) {
        this.gamepadConnected = false;
        this.gamepadName = '';
      }
      return;
    }

    this.gamepadConnected = true;
    this.gamepadName = activeGamepad.id || 'Standard Gamepad Controller';

    const buttons = activeGamepad.buttons;
    const axes = activeGamepad.axes;

    // Analog Stick (Axis 0, 1) & D-Pad (Buttons 12, 13, 14, 15)
    const stickX = axes[0] || 0;
    const stickY = axes[1] || 0;
    const dpadUp = buttons[12]?.pressed || false;
    const dpadDown = buttons[13]?.pressed || false;
    const dpadLeft = buttons[14]?.pressed || false;
    const dpadRight = buttons[15]?.pressed || false;

    // Horizontal Direction Mapping
    if (stickX < -0.25 || dpadLeft) {
      this.keys['ArrowLeft'] = true;
      this.keys['ArrowRight'] = false;
    } else if (stickX > 0.25 || dpadRight) {
      this.keys['ArrowRight'] = true;
      this.keys['ArrowLeft'] = false;
    }

    // Jump (Button 0 - A/Cross or D-Pad Up or Stick Up)
    const jumpBtn = buttons[0]?.pressed || dpadUp || stickY < -0.6;
    if (jumpBtn) {
      this.keys[' '] = true;
    }

    // Attack (Button 2 - X/Square or Button 1 - B/Circle)
    const attackBtn = buttons[2]?.pressed || false;
    if (attackBtn) {
      this.keys['j'] = true;
    }

    // Spell / Subweapon (Button 3 - Y/Triangle or Button 4 - LB/L1)
    const spellBtn = buttons[3]?.pressed || buttons[4]?.pressed || false;
    if (spellBtn) {
      this.keys['u'] = true;
    }

    // Dash (Button 1 - B/Circle or Button 5 - RB/R1 or Button 7 - RT/R2)
    const dashBtn = buttons[1]?.pressed || buttons[5]?.pressed || buttons[7]?.pressed || false;
    if (dashBtn) {
      this.keys['Shift'] = true;
    }

    // Menu Edge Triggers: Start (Button 9) & Select (Button 8)
    const startPressed = buttons[9]?.pressed || false;
    const selectPressed = buttons[8]?.pressed || false;

    if (startPressed && !this.prevGamepadButtons[9]) {
      if (this.onToggleMenuRequest) {
        this.onToggleMenuRequest('equipment');
        audio.playUiClick();
      }
    }

    if (selectPressed && !this.prevGamepadButtons[8]) {
      if (this.onToggleMenuRequest) {
        this.onToggleMenuRequest('map');
        audio.playUiClick();
      }
    }

    // Store button states for edge detection
    for (let i = 0; i < buttons.length; i++) {
      this.prevGamepadButtons[i] = buttons[i]?.pressed || false;
    }
  }
}
