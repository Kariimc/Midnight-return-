/**
 * Belmont's Curse: Shadow Reign - Core Game Types
 */

export type EquipmentSlotType = 'weapon' | 'helmet' | 'armor' | 'cape' | 'accessory' | 'subweapon';

export type WeaponType = 'whip' | 'greatsword' | 'rapier' | 'scythe' | 'dagger';

export interface ItemStats {
  attack?: number;
  defense?: number;
  critChance?: number; // 0 to 1
  speedBonus?: number;
  manaCostReduction?: number;
  elementalDamage?: {
    type: 'fire' | 'ice' | 'holy' | 'shadow' | 'lightning' | 'poison';
    amount: number;
  };
}

export interface EquipmentItem {
  id: string;
  name: string;
  description: string;
  slot: EquipmentSlotType;
  weaponType?: WeaponType;
  rarity: 'common' | 'rare' | 'epic' | 'legendary' | 'cursed';
  stats: ItemStats;
  level: number;
  maxLevel: number;
  upgradeCost: { shards: number; ore: number };
  color: string;
  trailColor?: string;
  visualStyle: {
    bladeColor?: string;
    handleColor?: string;
    armorColor?: string;
    capeColor?: string;
    glowColor?: string;
  };
  iconName: string;
}

export interface PlayerStats {
  level: number;
  xp: number;
  nextLevelXp: number;
  hp: number;
  maxHp: number;
  mp: number;
  maxMp: number;
  str: number; // Strength -> Melee damage
  con: number; // Constitution -> Max HP & Defense
  int: number; // Intelligence -> Max MP & Spell damage
  lck: number; // Luck -> Crit chance & Item drops
  gold: number;
  soulShards: number;
  darkOre: number;
}

export interface PlayerAbility {
  id: string;
  name: string;
  description: string;
  unlocked: boolean;
  type: 'movement' | 'combat' | 'magic' | 'passive';
  icon: string;
  cost: number;
  requires?: string[];
}

export interface Vector2D {
  x: number;
  y: number;
}

export interface Hitbox {
  x: number;
  y: number;
  width: number;
  height: number;
  type?: 'standard' | 'slippery_ice' | 'spikes' | 'acid_blood' | 'steam_jet' | 'crushing_gear' | 'falling_stalactite' | 'mana_drain' | 'void_well';
  damage?: number;
  active?: boolean;
}

export interface HairParticle {
  x: number;
  y: number;
  oldX: number;
  oldY: number;
}

export interface CapePoint {
  x: number;
  y: number;
  oldX: number;
  oldY: number;
  pinned?: boolean;
}

export interface DynamicParticle {
  x: number;
  y: number;
  vx: number;
  vy: number;
  life: number;
  maxLife: number;
  size: number;
  color: string;
  glow?: boolean;
  shape?: 'circle' | 'spark' | 'smoke' | 'blood' | 'rune' | 'ring' | 'blade_ghost' | 'snowflake' | 'spore' | 'lightning';
  alpha: number;
  rotation?: number;
  vRot?: number;
}

export interface DynamicLight {
  x: number;
  y: number;
  radius: number;
  color: string;
  intensity: number;
  flicker?: boolean;
}

export type EnemySpriteType =
  | 'skeleton'
  | 'bat'
  | 'specter'
  | 'gargoyle'
  | 'blood_leecher'
  | 'arcane_tome'
  | 'clockwork_drone'
  | 'frost_wraith'
  | 'bog_witch'
  | 'void_fiend'
  | 'boss_gargoyle'
  | 'boss_phantasm'
  | 'boss_archmage'
  | 'boss_behemoth'
  | 'boss_wyrm'
  | 'boss_hydra'
  | 'boss_harbinger'
  | 'boss_malakor'
  | 'boss_executioner'
  | 'boss_vespera'
  | 'boss_ignis'
  | 'boss_empress'
  | 'boss_cinder'
  | 'boss_belmont';

export interface EnemyType {
  id: string;
  name: string;
  maxHp: number;
  damage: number;
  speed: number;
  xpReward: number;
  goldReward: number;
  shardsReward: number;
  width: number;
  height: number;
  color: string;
  behavior: 'patrol' | 'flyer' | 'chaser' | 'caster' | 'boss';
  spriteType: EnemySpriteType;
  description: string;
  isSubBoss?: boolean;
}

export interface EnemyInstance {
  id: string;
  type: EnemyType;
  x: number;
  y: number;
  vx: number;
  vy: number;
  hp: number;
  maxHp: number;
  facing: 'left' | 'right';
  state: 'idle' | 'patrol' | 'chase' | 'attack' | 'stagger' | 'dead';
  attackCooldown: number;
  aiTimer: number;
  invulnerableFrames: number;
  phase?: number;
}

export interface FloatingText {
  id: string;
  text: string;
  x: number;
  y: number;
  color: string;
  scale: number;
  life: number;
  maxLife: number;
  vy: number;
}

export interface MapRoom {
  id: string;
  zoneId: string;
  gridX: number;
  gridY: number;
  width: number; // In game units (e.g. 1600)
  height: number; // In game units (e.g. 800)
  platforms: Hitbox[];
  slopes?: { x1: number; y1: number; x2: number; y2: number }[];
  hazards?: Hitbox[];
  doors: {
    direction: 'left' | 'right' | 'top' | 'bottom';
    targetRoomId: string;
    targetDoorIndex: number;
    locked?: boolean;
    keyRequired?: string;
  }[];
  enemies: { enemyTypeId: string; x: number; y: number }[];
  chests?: { id: string; x: number; y: number; item: EquipmentItem | { shards: number; ore: number }; opened: boolean }[];
  breakableWalls?: { id: string; x: number; y: number; width: number; height: number; broken: boolean; revealsSecretRoomId?: string }[];
  saveStatue?: { x: number; y: number };
  teleporter?: { x: number; y: number };
  forgeAltar?: { x: number; y: number };
  bossTrigger?: { bossTypeId: string; x: number; y: number; defeated: boolean };
  backgroundProps?: { x: number; y: number; type: 'pillar' | 'stained_glass' | 'statue' | 'torch' | 'arch' | 'chains' | 'gears' | 'ice_crystals' | 'fossil' | 'void_portal' }[];
  explored?: boolean;
  isSecret?: boolean;
}

export type BackgroundMusicTrack =
  | 'courtyard'
  | 'catacombs'
  | 'library'
  | 'clockwork'
  | 'frozen'
  | 'sunken'
  | 'void'
  | 'boss'
  | 'final_boss';

export interface MapZone {
  id: string;
  name: string;
  subtitle: string;
  themeColor: string;
  ambientLight: string; // RGBA overlay
  backgroundMusic: BackgroundMusicTrack;
  rooms: MapRoom[];
}

export interface GameSaveState {
  version: number;
  playerStats: PlayerStats;
  equipped: Record<EquipmentSlotType, EquipmentItem | null>;
  inventory: EquipmentItem[];
  abilities: Record<string, boolean>;
  discoveredRooms: string[];
  openedChests: string[];
  brokenWalls: string[];
  defeatedBosses: string[];
  currentRoomId: string;
  playTimeSeconds: number;
}
