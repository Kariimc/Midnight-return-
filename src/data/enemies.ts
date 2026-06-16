// Full initial enemy roster — inspired by SotN bestiary
// Each entry drives AI behavior, VFX, hitboxes, and stat scaling

export type AIBehavior =
  | 'patrol'       // walk left/right, turn on wall
  | 'chase'        // run toward player on detect
  | 'patrol_jump'  // patrol + jumps toward player
  | 'ranged'       // stay at distance, fire projectiles
  | 'flying'       // aerial patrol, dive-bomb attack
  | 'boss';        // complex multi-phase scripted

export interface DropEntry {
  itemId: string;
  chance: number; // 0–100
}

export interface EnemyDef {
  id:        string;
  name:      string;
  spriteKey: string;
  hp:        number;
  atk:       number;
  def:       number;
  exp:       number;
  hitboxW:   number;
  hitboxH:   number;
  behavior:  AIBehavior;
  moveSpeed: number;
  drops:     DropEntry[];
  resistances?: Record<string, number>; // damageType → % reduction
  deathColor?:    number; // hex
  deathParticles?: number;
  description: string;
}

export const ENEMY_ROSTER: EnemyDef[] = [
  // ── Zone 1: Entrance Hall ────────────────────────────────────────────────
  {
    id: 'zombie',
    name: 'Zombie',
    spriteKey: 'enemy_zombie',
    hp: 30, atk: 8, def: 2, exp: 15,
    hitboxW: 28, hitboxH: 52,
    behavior: 'patrol',
    moveSpeed: 50,
    drops: [{ itemId: 'zap_trap', chance: 5 }, { itemId: 'heart_small', chance: 20 }],
    deathColor: 0x44ff22,
    description: 'Shambling undead. Slow but armored by rot.',
  },
  {
    id: 'skeleton',
    name: 'Skeleton',
    spriteKey: 'enemy_skeleton',
    hp: 20, atk: 12, def: 0, exp: 20,
    hitboxW: 24, hitboxH: 52,
    behavior: 'patrol',
    moveSpeed: 80,
    drops: [{ itemId: 'bone', chance: 30 }, { itemId: 'heart_small', chance: 15 }],
    resistances: { holy: -50 }, // takes double from holy
    deathColor: 0xccccaa,
    description: 'Reanimates from dark energy. Weak to holy.',
  },
  {
    id: 'axe_knight',
    name: 'Axe Knight',
    spriteKey: 'enemy_axe_knight',
    hp: 80, atk: 22, def: 14, exp: 60,
    hitboxW: 36, hitboxH: 58,
    behavior: 'patrol_jump',
    moveSpeed: 65,
    drops: [{ itemId: 'axe', chance: 8 }, { itemId: 'knight_armor', chance: 3 }],
    resistances: { fire: 20 },
    deathColor: 0xff6600,
    deathParticles: 60,
    description: 'Heavily armored. Hurls axes in a high arc.',
  },
  {
    id: 'medusa_head',
    name: 'Medusa Head',
    spriteKey: 'enemy_medusa_head',
    hp: 15, atk: 18, def: 0, exp: 30,
    hitboxW: 22, hitboxH: 22,
    behavior: 'flying',
    moveSpeed: 120,
    drops: [{ itemId: 'stone_mask', chance: 2 }],
    resistances: { dark: 50 },
    deathColor: 0x00ff88,
    description: 'Flying serpent head. Inflicts petrify on contact.',
  },
  {
    id: 'ghost',
    name: 'Ghost',
    spriteKey: 'enemy_ghost',
    hp: 25, atk: 14, def: 0, exp: 25,
    hitboxW: 30, hitboxH: 40,
    behavior: 'flying',
    moveSpeed: 90,
    drops: [{ itemId: 'ghost_card', chance: 4 }],
    resistances: { physical: 50, holy: -100 },
    deathColor: 0xaaddff,
    description: 'Phase-shifting. Immune to most physical attacks.',
  },
  // ── Zone 2: Catacombs ───────────────────────────────────────────────────
  {
    id: 'bone_archer',
    name: 'Bone Archer',
    spriteKey: 'enemy_bone_archer',
    hp: 35, atk: 20, def: 4, exp: 40,
    hitboxW: 24, hitboxH: 52,
    behavior: 'ranged',
    moveSpeed: 40,
    drops: [{ itemId: 'arrow', chance: 40 }, { itemId: 'archer_bow', chance: 5 }],
    resistances: { holy: -50 },
    deathColor: 0xccccaa,
    deathParticles: 50,
    description: 'Skeleton archer. Fires volleys from afar.',
  },
  {
    id: 'warg',
    name: 'Warg',
    spriteKey: 'enemy_warg',
    hp: 55, atk: 25, def: 6, exp: 50,
    hitboxW: 48, hitboxH: 36,
    behavior: 'chase',
    moveSpeed: 170,
    drops: [{ itemId: 'beast_fang', chance: 25 }, { itemId: 'wolf_pelt', chance: 10 }],
    deathColor: 0x884400,
    description: 'Demon wolf. Lunges across the screen in a single bound.',
  },
  {
    id: 'harpy',
    name: 'Harpy',
    spriteKey: 'enemy_harpy',
    hp: 45, atk: 28, def: 2, exp: 55,
    hitboxW: 40, hitboxH: 34,
    behavior: 'flying',
    moveSpeed: 145,
    drops: [{ itemId: 'feather', chance: 30 }, { itemId: 'talons', chance: 8 }],
    deathColor: 0xffaa00,
    description: 'Winged fury. Swoops from above, inflicts bleeding.',
  },
  {
    id: 'merman',
    name: 'Merman',
    spriteKey: 'enemy_merman',
    hp: 60, atk: 18, def: 10, exp: 45,
    hitboxW: 30, hitboxH: 50,
    behavior: 'patrol',
    moveSpeed: 55,
    drops: [{ itemId: 'aquamarine', chance: 8 }, { itemId: 'trident', chance: 3 }],
    resistances: { ice: 80, fire: -30 },
    deathColor: 0x0088ff,
    description: 'Emerges from floor-submerged waterways. Immune to ice.',
  },
  // ── Zone 3: Cursed Library ───────────────────────────────────────────────
  {
    id: 'sorcerer',
    name: 'Sorcerer',
    spriteKey: 'enemy_sorcerer',
    hp: 70, atk: 35, def: 3, exp: 80,
    hitboxW: 26, hitboxH: 56,
    behavior: 'ranged',
    moveSpeed: 30,
    drops: [{ itemId: 'spellbook', chance: 6 }, { itemId: 'magic_tome', chance: 2 }],
    resistances: { dark: 60, fire: 20 },
    deathColor: 0x9900ff,
    deathParticles: 70,
    description: 'Casts three-way ice bolts and teleports short range.',
  },
  {
    id: 'gargoyle',
    name: 'Gargoyle',
    spriteKey: 'enemy_gargoyle',
    hp: 90, atk: 30, def: 18, exp: 90,
    hitboxW: 40, hitboxH: 44,
    behavior: 'flying',
    moveSpeed: 100,
    drops: [{ itemId: 'stone_heart', chance: 4 }, { itemId: 'granite_mail', chance: 2 }],
    resistances: { lightning: -50, physical: 20 },
    deathColor: 0x888888,
    deathParticles: 80,
    description: 'Stone sentinel. Activates when player passes. Weak to lightning.',
  },
  {
    id: 'blood_skeleton',
    name: 'Blood Skeleton',
    spriteKey: 'enemy_blood_skeleton',
    hp: 200, atk: 18, def: 0, exp: 5,
    hitboxW: 24, hitboxH: 52,
    behavior: 'patrol',
    moveSpeed: 85,
    drops: [], // indestructible — reforms at 1hp
    resistances: { physical: 90 },
    deathColor: 0xdd0000,
    description: 'Reforms from blood unless hit with holy. Cannot be killed normally.',
  },
  {
    id: 'death_scythe',
    name: 'Scythe Knight',
    spriteKey: 'enemy_scythe_knight',
    hp: 120, atk: 40, def: 20, exp: 110,
    hitboxW: 34, hitboxH: 58,
    behavior: 'chase',
    moveSpeed: 95,
    drops: [{ itemId: 'scythe', chance: 5 }, { itemId: 'dark_robe', chance: 4 }],
    resistances: { dark: 80 },
    deathColor: 0x330066,
    deathParticles: 100,
    description: 'Phantom knight. Drains HP on hit. Cursed blade negates healing.',
  },
  // ── Zone 4: Clocktower ───────────────────────────────────────────────────
  {
    id: 'gear_golem',
    name: 'Gear Golem',
    spriteKey: 'enemy_gear_golem',
    hp: 200, atk: 45, def: 30, exp: 150,
    hitboxW: 56, hitboxH: 64,
    behavior: 'patrol_jump',
    moveSpeed: 45,
    drops: [{ itemId: 'clockwork_gear', chance: 15 }, { itemId: 'iron_ore', chance: 30 }],
    resistances: { physical: 30, lightning: -80, fire: -20 },
    deathColor: 0xff8800,
    deathParticles: 120,
    description: 'Mechanical guardian. Weakpoint: exposed core exposed after slam.',
  },
  {
    id: 'vampire_bat',
    name: 'Vampire Bat',
    spriteKey: 'enemy_vampire_bat',
    hp: 18, atk: 12, def: 0, exp: 12,
    hitboxW: 20, hitboxH: 16,
    behavior: 'flying',
    moveSpeed: 160,
    drops: [{ itemId: 'bat_wing', chance: 25 }],
    deathColor: 0x660000,
    description: 'Swarms in clusters. Inflicts HP drain.',
  },
  // ── Zone 5: Throne Room / Boss Antechamber ───────────────────────────────
  {
    id: 'dark_knight',
    name: 'Dark Knight',
    spriteKey: 'enemy_dark_knight',
    hp: 280, atk: 55, def: 35, exp: 200,
    hitboxW: 38, hitboxH: 60,
    behavior: 'chase',
    moveSpeed: 110,
    drops: [{ itemId: 'shadow_blade', chance: 3 }, { itemId: 'cursed_shield', chance: 2 }],
    resistances: { dark: 70, holy: -60 },
    deathColor: 0x110022,
    deathParticles: 150,
    description: 'Fallen paladin. Three-hit combo, shadow step dodge.',
  },
  {
    id: 'succubus',
    name: 'Succubus',
    spriteKey: 'enemy_succubus',
    hp: 180, atk: 48, def: 10, exp: 180,
    hitboxW: 30, hitboxH: 54,
    behavior: 'flying',
    moveSpeed: 130,
    drops: [{ itemId: 'charm_ring', chance: 5 }, { itemId: 'silk_dress', chance: 3 }],
    resistances: { dark: 60, holy: -80 },
    deathColor: 0xff0066,
    deathParticles: 90,
    description: 'Charm attack stuns player. Drains HP with kiss kiss embrace.',
  },
];

export const ENEMY_MAP: Record<string, EnemyDef> =
  Object.fromEntries(ENEMY_ROSTER.map(e => [e.id, e]));
