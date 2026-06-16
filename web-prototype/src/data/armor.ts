export interface ArmorDef {
  id:       string;
  name:     string;
  slot:     'helmet' | 'armor' | 'cloak' | 'boots' | 'accessory';
  spriteKey: string;
  def:      number;
  strMod:   number;
  conMod:   number;
  intMod:   number;
  lckMod:   number;
  hpMod:    number;
  mpMod:    number;
  resistances: Partial<Record<string, number>>;
  special?: string; // passive ability id
  description: string;
}

export const ARMOR_ROSTER: ArmorDef[] = [
  // ── Helmets ─────────────────────────────────────────────────────────────
  {
    id: 'iron_helm',
    name: 'Iron Helm',
    slot: 'helmet', spriteKey: 'item_iron_helm',
    def: 8, strMod: 0, conMod: 2, intMod: 0, lckMod: 0, hpMod: 20, mpMod: 0,
    resistances: {},
    description: 'Basic protection. Heavy but reliable.',
  },
  {
    id: 'circlet_of_wisdom',
    name: 'Circlet of Wisdom',
    slot: 'helmet', spriteKey: 'item_circlet',
    def: 3, strMod: 0, conMod: 0, intMod: 8, lckMod: 2, hpMod: 0, mpMod: 40,
    resistances: { dark: 15 },
    description: 'Amplifies spellcasting. Faint violet glow.',
  },
  // ── Body Armor ───────────────────────────────────────────────────────────
  {
    id: 'leather_armor',
    name: 'Leather Armor',
    slot: 'armor', spriteKey: 'item_leather_armor',
    def: 6, strMod: 0, conMod: 1, intMod: 0, lckMod: 0, hpMod: 15, mpMod: 0,
    resistances: {},
    description: 'Starter armor. Lightweight, decent coverage.',
  },
  {
    id: 'knight_armor',
    name: 'Knight Armor',
    slot: 'armor', spriteKey: 'item_knight_armor',
    def: 22, strMod: 2, conMod: 4, intMod: -2, lckMod: 0, hpMod: 60, mpMod: -20,
    resistances: { physical: 20, fire: 10 },
    description: 'Full plate. Slows movement slightly, excellent defense.',
  },
  {
    id: 'dark_robe',
    name: 'Dark Robe',
    slot: 'armor', spriteKey: 'item_dark_robe',
    def: 8, strMod: 0, conMod: 0, intMod: 12, lckMod: 4, hpMod: 0, mpMod: 80,
    resistances: { dark: 40, holy: -30 },
    special: 'mp_regen',
    description: 'Woven from void-silk. Regenerates 1 MP/sec. Weak to holy.',
  },
  {
    id: 'dragon_scale_mail',
    name: 'Dragon Scale Mail',
    slot: 'armor', spriteKey: 'item_dragon_mail',
    def: 34, strMod: 3, conMod: 5, intMod: 0, lckMod: 0, hpMod: 100, mpMod: 0,
    resistances: { fire: 60, ice: 20, physical: 15 },
    description: 'Forged from elder dragon scales. Legendary tier defense.',
  },
  // ── Cloaks ───────────────────────────────────────────────────────────────
  {
    id: 'traveler_cloak',
    name: 'Traveler\'s Cloak',
    slot: 'cloak', spriteKey: 'item_traveler_cloak',
    def: 2, strMod: 0, conMod: 0, intMod: 0, lckMod: 3, hpMod: 0, mpMod: 0,
    resistances: {},
    description: 'Wind-worn. Slightly increases item drop rates.',
  },
  {
    id: 'shadow_cloak',
    name: 'Cloak of Shadows',
    slot: 'cloak', spriteKey: 'item_shadow_cloak',
    def: 6, strMod: 0, conMod: 0, intMod: 4, lckMod: 8, hpMod: 0, mpMod: 30,
    resistances: { dark: 25 },
    special: 'dash_shadow',
    description: 'Leaves shadow afterimages on dash. Doubles luck stat.',
  },
  // ── Boots ────────────────────────────────────────────────────────────────
  {
    id: 'iron_boots',
    name: 'Iron Boots',
    slot: 'boots', spriteKey: 'item_iron_boots',
    def: 5, strMod: 1, conMod: 1, intMod: 0, lckMod: 0, hpMod: 10, mpMod: 0,
    resistances: {},
    description: 'Stomp attack available from above.',
  },
  {
    id: 'swift_boots',
    name: 'Swift Boots',
    slot: 'boots', spriteKey: 'item_swift_boots',
    def: 2, strMod: 0, conMod: 0, intMod: 0, lckMod: 5, hpMod: 0, mpMod: 0,
    resistances: {},
    special: 'movement_speed_up',
    description: '+20% movement speed. Lightweight alchemized leather.',
  },
  // ── Accessories ──────────────────────────────────────────────────────────
  {
    id: 'ring_of_varda',
    name: 'Ring of Varda',
    slot: 'accessory', spriteKey: 'item_ring_varda',
    def: 0, strMod: 5, conMod: 5, intMod: 5, lckMod: 5, hpMod: 50, mpMod: 30,
    resistances: {},
    description: 'Balanced legendary ring. Minor boost to all stats.',
  },
  {
    id: 'bloodstone',
    name: 'Bloodstone',
    slot: 'accessory', spriteKey: 'item_bloodstone',
    def: 0, strMod: 0, conMod: 0, intMod: 0, lckMod: 0, hpMod: 0, mpMod: 0,
    resistances: {},
    special: 'lifesteal_10',
    description: 'Converts 10% of damage dealt into HP.',
  },
];

export const ARMOR_MAP: Record<string, ArmorDef> =
  Object.fromEntries(ARMOR_ROSTER.map(a => [a.id, a]));
