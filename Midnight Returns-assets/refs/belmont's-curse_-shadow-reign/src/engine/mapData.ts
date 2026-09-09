/**
 * Belmont's Curse: Shadow Reign - Metroidvania Interconnected Map World
 * 7 Massive Interconnected Biomes, Sub-Bosses, Main Bosses, Hazards, Secrets, Save Altars, Teleports, Forges.
 */

import { EquipmentItem, MapZone } from '../types/game';

export const INITIAL_ITEMS: Record<string, EquipmentItem> = {
  vampire_killer: {
    id: 'whip_vampire_killer',
    name: 'Vampire Slayer Whip',
    description: 'Ancient holy whip imbued with sacred fire. Swift long reach.',
    slot: 'weapon',
    weaponType: 'whip',
    rarity: 'rare',
    stats: { attack: 32, critChance: 0.15, elementalDamage: { type: 'holy', amount: 12 } },
    level: 1,
    maxLevel: 10,
    upgradeCost: { shards: 15, ore: 2 },
    color: '#e2e8f0',
    trailColor: '#fef08a',
    visualStyle: { bladeColor: '#fde047', glowColor: '#fef08a' },
    iconName: 'Zap'
  },
  alucard_sword: {
    id: 'sword_alucard',
    name: "Alucard's Shadow Blade",
    description: 'Forged in abyssal dark iron. Devastating critical strike output.',
    slot: 'weapon',
    weaponType: 'greatsword',
    rarity: 'epic',
    stats: { attack: 52, critChance: 0.28, speedBonus: 1.2 },
    level: 1,
    maxLevel: 10,
    upgradeCost: { shards: 25, ore: 5 },
    color: '#94a3b8',
    trailColor: '#38bdf8',
    visualStyle: { bladeColor: '#cbd5e1', glowColor: '#38bdf8', armorColor: '#1e293b' },
    iconName: 'Sword'
  },
  death_scythe: {
    id: 'scythe_death',
    name: "Grim Reaper's Harvest Scythe",
    description: 'Reaps souls to siphon maximum strength and dark energy.',
    slot: 'weapon',
    weaponType: 'scythe',
    rarity: 'legendary',
    stats: { attack: 75, critChance: 0.35, elementalDamage: { type: 'shadow', amount: 30 } },
    level: 1,
    maxLevel: 10,
    upgradeCost: { shards: 50, ore: 10 },
    color: '#a855f7',
    trailColor: '#a855f7',
    visualStyle: { bladeColor: '#c084fc', glowColor: '#a855f7' },
    iconName: 'Skull'
  },
  frostbite_dagger: {
    id: 'dagger_frost',
    name: 'Rime Crystal Stiletto',
    description: 'Freezing glass blade that pierces armor with glacial cold.',
    slot: 'weapon',
    weaponType: 'dagger',
    rarity: 'rare',
    stats: { attack: 40, speedBonus: 1.4, elementalDamage: { type: 'ice', amount: 18 } },
    level: 1,
    maxLevel: 10,
    upgradeCost: { shards: 20, ore: 4 },
    color: '#38bdf8',
    trailColor: '#7dd3fc',
    visualStyle: { bladeColor: '#38bdf8', glowColor: '#e0f2fe' },
    iconName: 'Sparkles'
  },
  venom_rapier: {
    id: 'rapier_venom',
    name: "Viper's Acid Needle",
    description: 'Infuses hits with deadly poison that corrodes enemy armor.',
    slot: 'weapon',
    weaponType: 'rapier',
    rarity: 'epic',
    stats: { attack: 58, speedBonus: 1.3, elementalDamage: { type: 'poison', amount: 22 } },
    level: 1,
    maxLevel: 10,
    upgradeCost: { shards: 30, ore: 6 },
    color: '#22c55e',
    trailColor: '#4ade80',
    visualStyle: { bladeColor: '#22c55e', glowColor: '#86efac' },
    iconName: 'Crosshair'
  },
  blood_cloak: {
    id: 'cape_blood',
    name: 'Blood Crimson Mantle',
    description: 'Flowing mantle woven with dark silk. Boosts defense and swiftness.',
    slot: 'cape',
    rarity: 'epic',
    stats: { defense: 22, speedBonus: 1.15 },
    level: 1,
    maxLevel: 10,
    upgradeCost: { shards: 20, ore: 3 },
    color: '#991b1b',
    visualStyle: { capeColor: '#991b1b' },
    iconName: 'Shield'
  },
  shadow_helm: {
    id: 'helm_shadow',
    name: 'Obsidian Horned Helm',
    description: 'Heavy horned helm forged in demonic embers.',
    slot: 'helmet',
    rarity: 'rare',
    stats: { defense: 18, manaCostReduction: 0.15 },
    level: 1,
    maxLevel: 10,
    upgradeCost: { shards: 12, ore: 2 },
    color: '#1f2937',
    visualStyle: { armorColor: '#111827' },
    iconName: 'Crown'
  },
  dragon_cuirass: {
    id: 'armor_dragon',
    name: 'Dragon Scale Plate',
    description: 'Forged from ancient dragon hide. Immense defensive power.',
    slot: 'armor',
    rarity: 'legendary',
    stats: { defense: 42, attack: 15 },
    level: 1,
    maxLevel: 10,
    upgradeCost: { shards: 40, ore: 8 },
    color: '#b45309',
    visualStyle: { armorColor: '#78350f', bladeColor: '#f59e0b' },
    iconName: 'ShieldAlert'
  }
};

export const WORLD_ZONES: MapZone[] = [
  // 1. GOTHIC COURTYARD & BELL TOWER
  {
    id: 'zone_courtyard',
    name: 'Gothic Courtyard & Bell Tower',
    subtitle: 'Outer Castle Gates',
    themeColor: '#38bdf8',
    ambientLight: 'rgba(10, 15, 30, 0.65)',
    backgroundMusic: 'courtyard',
    rooms: [
      {
        id: 'room_courtyard_1',
        zoneId: 'zone_courtyard',
        gridX: 0,
        gridY: 0,
        width: 1600,
        height: 800,
        platforms: [
          { x: 0, y: 720, width: 1600, height: 80 },
          { x: 300, y: 560, width: 240, height: 30 },
          { x: 700, y: 440, width: 280, height: 30 },
          { x: 1100, y: 320, width: 240, height: 30 },
          { x: 0, y: 0, width: 40, height: 800 }
        ],
        doors: [
          { direction: 'right', targetRoomId: 'room_courtyard_2', targetDoorIndex: 0 }
        ],
        enemies: [
          { enemyTypeId: 'skeleton', x: 500, y: 660 },
          { enemyTypeId: 'bat', x: 800, y: 350 },
          { enemyTypeId: 'skeleton', x: 1200, y: 260 }
        ],
        chests: [
          { id: 'chest_vampire_killer', x: 1250, y: 270, item: INITIAL_ITEMS.vampire_killer, opened: false }
        ],
        saveStatue: { x: 150, y: 660 },
        backgroundProps: [
          { x: 200, y: 550, type: 'torch' },
          { x: 800, y: 200, type: 'stained_glass' },
          { x: 1400, y: 550, type: 'torch' }
        ],
        explored: true
      },
      {
        id: 'room_courtyard_2',
        zoneId: 'zone_courtyard',
        gridX: 1,
        gridY: 0,
        width: 1600,
        height: 800,
        platforms: [
          { x: 0, y: 720, width: 1600, height: 80 },
          { x: 200, y: 580, width: 200, height: 30 },
          { x: 500, y: 460, width: 220, height: 30 },
          { x: 850, y: 340, width: 220, height: 30 },
          { x: 1200, y: 500, width: 260, height: 30 }
        ],
        doors: [
          { direction: 'left', targetRoomId: 'room_courtyard_1', targetDoorIndex: 0 },
          { direction: 'right', targetRoomId: 'room_courtyard_subboss', targetDoorIndex: 0 },
          { direction: 'top', targetRoomId: 'room_library_1', targetDoorIndex: 0 }
        ],
        enemies: [
          { enemyTypeId: 'skeleton', x: 600, y: 400 },
          { enemyTypeId: 'gargoyle', x: 900, y: 250 },
          { enemyTypeId: 'bat', x: 1300, y: 400 }
        ],
        teleporter: { x: 1300, y: 440 },
        forgeAltar: { x: 300, y: 520 },
        backgroundProps: [
          { x: 300, y: 400, type: 'pillar' },
          { x: 900, y: 200, type: 'torch' }
        ],
        explored: false
      },
      {
        id: 'room_courtyard_subboss',
        zoneId: 'zone_courtyard',
        gridX: 2,
        gridY: 0,
        width: 1600,
        height: 800,
        platforms: [
          { x: 0, y: 720, width: 1600, height: 80 },
          { x: 0, y: 0, width: 40, height: 800 },
          { x: 1560, y: 0, width: 40, height: 800 }
        ],
        doors: [
          { direction: 'left', targetRoomId: 'room_courtyard_2', targetDoorIndex: 1 },
          { direction: 'right', targetRoomId: 'room_catacombs_1', targetDoorIndex: 0 }
        ],
        enemies: [],
        bossTrigger: {
          bossTypeId: 'boss_gargoyle',
          x: 1000,
          y: 600,
          defeated: false
        },
        saveStatue: { x: 150, y: 660 },
        backgroundProps: [
          { x: 800, y: 200, type: 'statue' }
        ],
        explored: false
      }
    ]
  },

  // 2. THE ABANDONED CATACOMBS & BLOOD VAULTS
  {
    id: 'zone_catacombs',
    name: 'The Abandoned Catacombs & Blood Vaults',
    subtitle: 'Underground Ossuary',
    themeColor: '#ef4444',
    ambientLight: 'rgba(25, 5, 10, 0.85)',
    backgroundMusic: 'catacombs',
    rooms: [
      {
        id: 'room_catacombs_1',
        zoneId: 'zone_catacombs',
        gridX: 3,
        gridY: 0,
        width: 1600,
        height: 800,
        platforms: [
          { x: 0, y: 720, width: 500, height: 80 },
          { x: 700, y: 720, width: 900, height: 80 },
          { x: 300, y: 550, width: 250, height: 30 },
          { x: 700, y: 420, width: 250, height: 30 },
          { x: 1100, y: 550, width: 250, height: 30 }
        ],
        hazards: [
          { x: 500, y: 740, width: 200, height: 40, type: 'acid_blood', damage: 8 },
          { x: 720, y: 400, width: 210, height: 20, type: 'spikes', damage: 15 }
        ],
        doors: [
          { direction: 'left', targetRoomId: 'room_courtyard_subboss', targetDoorIndex: 1 },
          { direction: 'right', targetRoomId: 'room_catacombs_subboss', targetDoorIndex: 0 }
        ],
        enemies: [
          { enemyTypeId: 'blood_leecher', x: 400, y: 490 },
          { enemyTypeId: 'specter', x: 800, y: 350 },
          { enemyTypeId: 'skeleton', x: 1200, y: 490 }
        ],
        chests: [
          { id: 'chest_blood_cloak', x: 800, y: 370, item: INITIAL_ITEMS.blood_cloak, opened: false }
        ],
        backgroundProps: [
          { x: 400, y: 300, type: 'chains' },
          { x: 1000, y: 300, type: 'torch' }
        ],
        explored: false
      },
      {
        id: 'room_catacombs_subboss',
        zoneId: 'zone_catacombs',
        gridX: 4,
        gridY: 0,
        width: 1600,
        height: 800,
        platforms: [
          { x: 0, y: 720, width: 1600, height: 80 },
          { x: 0, y: 0, width: 40, height: 800 },
          { x: 1560, y: 0, width: 40, height: 800 }
        ],
        doors: [
          { direction: 'left', targetRoomId: 'room_catacombs_1', targetDoorIndex: 1 },
          { direction: 'right', targetRoomId: 'room_catacombs_boss', targetDoorIndex: 0 }
        ],
        enemies: [],
        bossTrigger: {
          bossTypeId: 'boss_phantasm',
          x: 1000,
          y: 600,
          defeated: false
        },
        saveStatue: { x: 150, y: 660 },
        backgroundProps: [
          { x: 800, y: 200, type: 'arch' }
        ],
        explored: false
      },
      {
        id: 'room_catacombs_boss',
        zoneId: 'zone_catacombs',
        gridX: 5,
        gridY: 0,
        width: 1600,
        height: 800,
        platforms: [
          { x: 0, y: 720, width: 1600, height: 80 },
          { x: 0, y: 0, width: 40, height: 800 },
          { x: 1560, y: 0, width: 40, height: 800 }
        ],
        doors: [
          { direction: 'left', targetRoomId: 'room_catacombs_subboss', targetDoorIndex: 1 },
          { direction: 'right', targetRoomId: 'room_clockwork_1', targetDoorIndex: 0 }
        ],
        enemies: [],
        bossTrigger: {
          bossTypeId: 'boss_executioner',
          x: 1000,
          y: 600,
          defeated: false
        },
        backgroundProps: [
          { x: 400, y: 200, type: 'arch' },
          { x: 1200, y: 200, type: 'arch' }
        ],
        explored: false
      }
    ]
  },

  // 3. SHATTERED LIBRARY & ARCANE OBSERVATORY
  {
    id: 'zone_library',
    name: 'Shattered Library & Arcane Observatory',
    subtitle: 'Tower of Forgotten Lore',
    themeColor: '#a855f7',
    ambientLight: 'rgba(15, 10, 30, 0.75)',
    backgroundMusic: 'library',
    rooms: [
      {
        id: 'room_library_1',
        zoneId: 'zone_library',
        gridX: 1,
        gridY: -1,
        width: 1600,
        height: 800,
        platforms: [
          { x: 0, y: 720, width: 1600, height: 80 },
          { x: 250, y: 560, width: 220, height: 30 },
          { x: 600, y: 420, width: 220, height: 30 },
          { x: 1000, y: 300, width: 220, height: 30 },
          { x: 1300, y: 480, width: 220, height: 30 }
        ],
        hazards: [
          { x: 610, y: 390, width: 200, height: 30, type: 'mana_drain', damage: 5 }
        ],
        doors: [
          { direction: 'bottom', targetRoomId: 'room_courtyard_2', targetDoorIndex: 2 },
          { direction: 'right', targetRoomId: 'room_library_subboss', targetDoorIndex: 0 }
        ],
        enemies: [
          { enemyTypeId: 'arcane_tome', x: 300, y: 480 },
          { enemyTypeId: 'specter', x: 700, y: 350 },
          { enemyTypeId: 'bat', x: 1100, y: 200 }
        ],
        chests: [
          { id: 'chest_shadow_helm', x: 1050, y: 250, item: INITIAL_ITEMS.shadow_helm, opened: false }
        ],
        backgroundProps: [
          { x: 500, y: 200, type: 'stained_glass' },
          { x: 1100, y: 200, type: 'torch' }
        ],
        explored: false
      },
      {
        id: 'room_library_subboss',
        zoneId: 'zone_library',
        gridX: 2,
        gridY: -1,
        width: 1600,
        height: 800,
        platforms: [
          { x: 0, y: 720, width: 1600, height: 80 },
          { x: 0, y: 0, width: 40, height: 800 },
          { x: 1560, y: 0, width: 40, height: 800 }
        ],
        doors: [
          { direction: 'left', targetRoomId: 'room_library_1', targetDoorIndex: 1 },
          { direction: 'right', targetRoomId: 'room_library_boss', targetDoorIndex: 0 }
        ],
        enemies: [],
        bossTrigger: {
          bossTypeId: 'boss_archmage',
          x: 1000,
          y: 600,
          defeated: false
        },
        saveStatue: { x: 150, y: 660 },
        backgroundProps: [
          { x: 800, y: 150, type: 'stained_glass' }
        ],
        explored: false
      },
      {
        id: 'room_library_boss',
        zoneId: 'zone_library',
        gridX: 3,
        gridY: -1,
        width: 1600,
        height: 800,
        platforms: [
          { x: 0, y: 720, width: 1600, height: 80 },
          { x: 0, y: 0, width: 40, height: 800 },
          { x: 1560, y: 0, width: 40, height: 800 }
        ],
        doors: [
          { direction: 'left', targetRoomId: 'room_library_subboss', targetDoorIndex: 1 },
          { direction: 'right', targetRoomId: 'room_frozen_1', targetDoorIndex: 0 }
        ],
        enemies: [],
        bossTrigger: {
          bossTypeId: 'boss_vespera',
          x: 1000,
          y: 580,
          defeated: false
        },
        chests: [
          { id: 'chest_alucard_sword', x: 1400, y: 660, item: INITIAL_ITEMS.alucard_sword, opened: false }
        ],
        backgroundProps: [
          { x: 800, y: 150, type: 'stained_glass' }
        ],
        explored: false
      }
    ]
  },

  // 4. CLOCKWORK KEEP & GEAR MATRIX
  {
    id: 'zone_clockwork',
    name: 'Clockwork Keep & Gear Matrix',
    subtitle: 'The Lightning Spire',
    themeColor: '#f59e0b',
    ambientLight: 'rgba(25, 20, 10, 0.75)',
    backgroundMusic: 'clockwork',
    rooms: [
      {
        id: 'room_clockwork_1',
        zoneId: 'zone_clockwork',
        gridX: 6,
        gridY: 0,
        width: 1600,
        height: 800,
        platforms: [
          { x: 0, y: 720, width: 1600, height: 80 },
          { x: 200, y: 550, width: 300, height: 30 },
          { x: 650, y: 400, width: 300, height: 30 },
          { x: 1100, y: 280, width: 300, height: 30 }
        ],
        hazards: [
          { x: 660, y: 370, width: 280, height: 30, type: 'steam_jet', damage: 12 },
          { x: 1120, y: 250, width: 260, height: 30, type: 'crushing_gear', damage: 20 }
        ],
        doors: [
          { direction: 'left', targetRoomId: 'room_catacombs_boss', targetDoorIndex: 1 },
          { direction: 'right', targetRoomId: 'room_clockwork_subboss', targetDoorIndex: 0 }
        ],
        enemies: [
          { enemyTypeId: 'clockwork_drone', x: 350, y: 480 },
          { enemyTypeId: 'specter', x: 800, y: 320 },
          { enemyTypeId: 'bat', x: 1200, y: 180 }
        ],
        chests: [
          { id: 'chest_death_scythe', x: 1250, y: 220, item: INITIAL_ITEMS.death_scythe, opened: false }
        ],
        backgroundProps: [
          { x: 400, y: 200, type: 'gears' },
          { x: 1000, y: 200, type: 'gears' }
        ],
        explored: false
      },
      {
        id: 'room_clockwork_subboss',
        zoneId: 'zone_clockwork',
        gridX: 7,
        gridY: 0,
        width: 1600,
        height: 800,
        platforms: [
          { x: 0, y: 720, width: 1600, height: 80 },
          { x: 0, y: 0, width: 40, height: 800 },
          { x: 1560, y: 0, width: 40, height: 800 }
        ],
        doors: [
          { direction: 'left', targetRoomId: 'room_clockwork_1', targetDoorIndex: 1 },
          { direction: 'right', targetRoomId: 'room_clockwork_boss', targetDoorIndex: 0 }
        ],
        enemies: [],
        bossTrigger: {
          bossTypeId: 'boss_behemoth',
          x: 1000,
          y: 600,
          defeated: false
        },
        saveStatue: { x: 150, y: 660 },
        backgroundProps: [
          { x: 800, y: 200, type: 'gears' }
        ],
        explored: false
      },
      {
        id: 'room_clockwork_boss',
        zoneId: 'zone_clockwork',
        gridX: 8,
        gridY: 0,
        width: 1600,
        height: 800,
        platforms: [
          { x: 0, y: 720, width: 1600, height: 80 },
          { x: 0, y: 0, width: 40, height: 800 },
          { x: 1560, y: 0, width: 40, height: 800 }
        ],
        doors: [
          { direction: 'left', targetRoomId: 'room_clockwork_subboss', targetDoorIndex: 1 },
          { direction: 'right', targetRoomId: 'room_sunken_1', targetDoorIndex: 0 }
        ],
        enemies: [],
        bossTrigger: {
          bossTypeId: 'boss_ignis',
          x: 1000,
          y: 600,
          defeated: false
        },
        backgroundProps: [
          { x: 400, y: 200, type: 'gears' },
          { x: 1200, y: 200, type: 'gears' }
        ],
        explored: false
      }
    ]
  },

  // 5. FROZEN OBSIDIAN SPIRE & GLACIAL ABYSS
  {
    id: 'zone_frozen',
    name: 'Frozen Obsidian Spire & Glacial Abyss',
    subtitle: 'Rime Crystal Peaks',
    themeColor: '#06b6d4',
    ambientLight: 'rgba(5, 25, 40, 0.75)',
    backgroundMusic: 'frozen',
    rooms: [
      {
        id: 'room_frozen_1',
        zoneId: 'zone_frozen',
        gridX: 4,
        gridY: -1,
        width: 1600,
        height: 800,
        platforms: [
          { x: 0, y: 720, width: 1600, height: 80, type: 'slippery_ice' },
          { x: 250, y: 550, width: 280, height: 30, type: 'slippery_ice' },
          { x: 700, y: 400, width: 280, height: 30, type: 'slippery_ice' },
          { x: 1150, y: 280, width: 280, height: 30, type: 'slippery_ice' }
        ],
        hazards: [
          { x: 700, y: 350, width: 280, height: 30, type: 'falling_stalactite', damage: 16 }
        ],
        doors: [
          { direction: 'left', targetRoomId: 'room_library_boss', targetDoorIndex: 1 },
          { direction: 'right', targetRoomId: 'room_frozen_subboss', targetDoorIndex: 0 }
        ],
        enemies: [
          { enemyTypeId: 'frost_wraith', x: 350, y: 480 },
          { enemyTypeId: 'bat', x: 800, y: 320 },
          { enemyTypeId: 'frost_wraith', x: 1250, y: 200 }
        ],
        chests: [
          { id: 'chest_frostbite_dagger', x: 1250, y: 220, item: INITIAL_ITEMS.frostbite_dagger, opened: false }
        ],
        backgroundProps: [
          { x: 400, y: 200, type: 'ice_crystals' },
          { x: 1100, y: 200, type: 'ice_crystals' }
        ],
        explored: false
      },
      {
        id: 'room_frozen_subboss',
        zoneId: 'zone_frozen',
        gridX: 5,
        gridY: -1,
        width: 1600,
        height: 800,
        platforms: [
          { x: 0, y: 720, width: 1600, height: 80, type: 'slippery_ice' },
          { x: 0, y: 0, width: 40, height: 800 },
          { x: 1560, y: 0, width: 40, height: 800 }
        ],
        doors: [
          { direction: 'left', targetRoomId: 'room_frozen_1', targetDoorIndex: 1 },
          { direction: 'right', targetRoomId: 'room_frozen_boss', targetDoorIndex: 0 }
        ],
        enemies: [],
        bossTrigger: {
          bossTypeId: 'boss_wyrm',
          x: 1000,
          y: 600,
          defeated: false
        },
        saveStatue: { x: 150, y: 660 },
        backgroundProps: [
          { x: 800, y: 200, type: 'ice_crystals' }
        ],
        explored: false
      },
      {
        id: 'room_frozen_boss',
        zoneId: 'zone_frozen',
        gridX: 6,
        gridY: -1,
        width: 1600,
        height: 800,
        platforms: [
          { x: 0, y: 720, width: 1600, height: 80, type: 'slippery_ice' },
          { x: 0, y: 0, width: 40, height: 800 },
          { x: 1560, y: 0, width: 40, height: 800 }
        ],
        doors: [
          { direction: 'left', targetRoomId: 'room_frozen_subboss', targetDoorIndex: 1 },
          { direction: 'right', targetRoomId: 'room_void_1', targetDoorIndex: 0 }
        ],
        enemies: [],
        bossTrigger: {
          bossTypeId: 'boss_empress',
          x: 1000,
          y: 600,
          defeated: false
        },
        backgroundProps: [
          { x: 800, y: 150, type: 'ice_crystals' }
        ],
        explored: false
      }
    ]
  },

  // 6. SUNKEN CRYPTS & POISON LAGOON
  {
    id: 'zone_sunken',
    name: 'Sunken Crypts & Poison Lagoon',
    subtitle: 'Toxic Serpent Mire',
    themeColor: '#22c55e',
    ambientLight: 'rgba(5, 30, 15, 0.85)',
    backgroundMusic: 'sunken',
    rooms: [
      {
        id: 'room_sunken_1',
        zoneId: 'zone_sunken',
        gridX: 9,
        gridY: 0,
        width: 1600,
        height: 800,
        platforms: [
          { x: 0, y: 720, width: 400, height: 80 },
          { x: 800, y: 720, width: 800, height: 80 },
          { x: 300, y: 550, width: 220, height: 30 },
          { x: 650, y: 400, width: 220, height: 30 }
        ],
        hazards: [
          { x: 400, y: 740, width: 400, height: 40, type: 'acid_blood', damage: 10 }
        ],
        doors: [
          { direction: 'left', targetRoomId: 'room_clockwork_boss', targetDoorIndex: 1 },
          { direction: 'right', targetRoomId: 'room_sunken_subboss', targetDoorIndex: 0 }
        ],
        enemies: [
          { enemyTypeId: 'bog_witch', x: 350, y: 480 },
          { enemyTypeId: 'blood_leecher', x: 700, y: 330 }
        ],
        chests: [
          { id: 'chest_venom_rapier', x: 700, y: 350, item: INITIAL_ITEMS.venom_rapier, opened: false }
        ],
        backgroundProps: [
          { x: 400, y: 200, type: 'fossil' },
          { x: 1000, y: 200, type: 'fossil' }
        ],
        explored: false
      },
      {
        id: 'room_sunken_subboss',
        zoneId: 'zone_sunken',
        gridX: 10,
        gridY: 0,
        width: 1600,
        height: 800,
        platforms: [
          { x: 0, y: 720, width: 1600, height: 80 },
          { x: 0, y: 0, width: 40, height: 800 },
          { x: 1560, y: 0, width: 40, height: 800 }
        ],
        doors: [
          { direction: 'left', targetRoomId: 'room_sunken_1', targetDoorIndex: 1 },
          { direction: 'right', targetRoomId: 'room_sunken_boss', targetDoorIndex: 0 }
        ],
        enemies: [],
        bossTrigger: {
          bossTypeId: 'boss_hydra',
          x: 1000,
          y: 600,
          defeated: false
        },
        saveStatue: { x: 150, y: 660 },
        backgroundProps: [
          { x: 800, y: 200, type: 'fossil' }
        ],
        explored: false
      },
      {
        id: 'room_sunken_boss',
        zoneId: 'zone_sunken',
        gridX: 11,
        gridY: 0,
        width: 1600,
        height: 800,
        platforms: [
          { x: 0, y: 720, width: 1600, height: 80 },
          { x: 0, y: 0, width: 40, height: 800 },
          { x: 1560, y: 0, width: 40, height: 800 }
        ],
        doors: [
          { direction: 'left', targetRoomId: 'room_sunken_subboss', targetDoorIndex: 1 }
        ],
        enemies: [],
        bossTrigger: {
          bossTypeId: 'boss_cinder',
          x: 1000,
          y: 600,
          defeated: false
        },
        chests: [
          { id: 'chest_dragon_cuirass', x: 1400, y: 660, item: INITIAL_ITEMS.dragon_cuirass, opened: false }
        ],
        backgroundProps: [
          { x: 800, y: 150, type: 'fossil' }
        ],
        explored: false
      }
    ]
  },

  // 7. ABYSSAL VOID NEXUS & SHADOW THRONE
  {
    id: 'zone_void',
    name: 'Abyssal Void Nexus & Shadow Throne',
    subtitle: 'Seat of the Fallen Lord',
    themeColor: '#8b5cf6',
    ambientLight: 'rgba(20, 0, 35, 0.95)',
    backgroundMusic: 'void',
    rooms: [
      {
        id: 'room_void_1',
        zoneId: 'zone_void',
        gridX: 7,
        gridY: -1,
        width: 1800,
        height: 900,
        platforms: [
          { x: 0, y: 820, width: 1800, height: 80 },
          { x: 300, y: 620, width: 300, height: 30 },
          { x: 750, y: 460, width: 300, height: 30 },
          { x: 1200, y: 300, width: 300, height: 30 }
        ],
        hazards: [
          { x: 750, y: 410, width: 300, height: 50, type: 'void_well', damage: 14 }
        ],
        doors: [
          { direction: 'left', targetRoomId: 'room_frozen_boss', targetDoorIndex: 1 },
          { direction: 'right', targetRoomId: 'room_void_subboss', targetDoorIndex: 0 }
        ],
        enemies: [
          { enemyTypeId: 'void_fiend', x: 400, y: 550 },
          { enemyTypeId: 'specter', x: 850, y: 390 },
          { enemyTypeId: 'void_fiend', x: 1300, y: 230 }
        ],
        backgroundProps: [
          { x: 900, y: 200, type: 'void_portal' }
        ],
        explored: false
      },
      {
        id: 'room_void_subboss',
        zoneId: 'zone_void',
        gridX: 8,
        gridY: -1,
        width: 1800,
        height: 900,
        platforms: [
          { x: 0, y: 820, width: 1800, height: 80 },
          { x: 0, y: 0, width: 40, height: 900 },
          { x: 1760, y: 0, width: 40, height: 900 }
        ],
        doors: [
          { direction: 'left', targetRoomId: 'room_void_1', targetDoorIndex: 1 },
          { direction: 'right', targetRoomId: 'room_void_boss', targetDoorIndex: 0 }
        ],
        enemies: [],
        bossTrigger: {
          bossTypeId: 'boss_harbinger',
          x: 1200,
          y: 700,
          defeated: false
        },
        saveStatue: { x: 150, y: 760 },
        teleporter: { x: 250, y: 760 },
        backgroundProps: [
          { x: 900, y: 200, type: 'void_portal' }
        ],
        explored: false
      },
      {
        id: 'room_void_boss',
        zoneId: 'zone_void',
        gridX: 9,
        gridY: -1,
        width: 2000,
        height: 1000,
        platforms: [
          { x: 0, y: 920, width: 2000, height: 80 },
          { x: 0, y: 0, width: 40, height: 1000 },
          { x: 1960, y: 0, width: 40, height: 1000 },
          { x: 350, y: 720, width: 300, height: 30 },
          { x: 1350, y: 720, width: 300, height: 30 }
        ],
        doors: [
          { direction: 'left', targetRoomId: 'room_void_subboss', targetDoorIndex: 1 }
        ],
        enemies: [],
        bossTrigger: {
          bossTypeId: 'boss_belmont',
          x: 1300,
          y: 800,
          defeated: false
        },
        saveStatue: { x: 150, y: 860 },
        backgroundProps: [
          { x: 1000, y: 250, type: 'stained_glass' },
          { x: 400, y: 450, type: 'statue' },
          { x: 1600, y: 450, type: 'statue' }
        ],
        explored: false
      }
    ]
  }
];
