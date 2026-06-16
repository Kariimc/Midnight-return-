export type WeaponType = 'sword' | 'whip' | 'axe' | 'spear' | 'scythe' | 'fist' | 'wand';
export type DamageType = 'physical' | 'fire' | 'ice' | 'lightning' | 'dark' | 'holy';

export interface WeaponDef {
  id:       string;
  name:     string;
  type:     WeaponType;
  spriteKey: string;
  atk:      number;
  range:    number;        // px
  hitboxW:  number;
  hitboxH:  number;
  damageType: DamageType;
  comboHits:  number;
  swingSpeed: number;      // anim fps
  trailColor: number;      // hex for VFX trail
  trailWidth: number;      // px
  specialMove?: string;    // id of special ability
  critMod:    number;      // additive crit % bonus
  description: string;
}

export const WEAPON_ROSTER: WeaponDef[] = [
  {
    id: 'short_sword',
    name: 'Short Sword',
    type: 'sword',
    spriteKey: 'item_short_sword',
    atk: 12, range: 45, hitboxW: 45, hitboxH: 20,
    damageType: 'physical', comboHits: 2, swingSpeed: 18,
    trailColor: 0xccccff, trailWidth: 3,
    critMod: 0,
    description: 'Standard issue. Fast, reliable, no frills.',
  },
  {
    id: 'vampire_killer',
    name: "Vampire Killer",
    type: 'whip',
    spriteKey: 'item_whip',
    atk: 18, range: 90, hitboxW: 90, hitboxH: 14,
    damageType: 'holy', comboHits: 1, swingSpeed: 12,
    trailColor: 0xffee00, trailWidth: 5,
    specialMove: 'whip_cross',
    critMod: 5,
    description: 'Legendary holy whip. Extended range, halved against undead immune.',
  },
  {
    id: 'claymore',
    name: 'Claymore',
    type: 'sword',
    spriteKey: 'item_claymore',
    atk: 32, range: 58, hitboxW: 58, hitboxH: 26,
    damageType: 'physical', comboHits: 2, swingSpeed: 9,
    trailColor: 0x4488ff, trailWidth: 6,
    critMod: 8,
    description: 'Massive two-handed blade. Slow but devastating cleave.',
  },
  {
    id: 'battle_axe',
    name: 'Battle Axe',
    type: 'axe',
    spriteKey: 'item_battle_axe',
    atk: 28, range: 50, hitboxW: 50, hitboxH: 40,
    damageType: 'physical', comboHits: 1, swingSpeed: 8,
    trailColor: 0xff6600, trailWidth: 6,
    specialMove: 'axe_throw',
    critMod: 12,
    description: 'Heavy cleaver. Can be thrown in an arc as a sub-weapon.',
  },
  {
    id: 'shadow_blade',
    name: 'Shadow Blade',
    type: 'sword',
    spriteKey: 'item_shadow_blade',
    atk: 38, range: 52, hitboxW: 52, hitboxH: 22,
    damageType: 'dark', comboHits: 3, swingSpeed: 20,
    trailColor: 0x6600aa, trailWidth: 8,
    specialMove: 'dark_slash',
    critMod: 15,
    description: 'Cursed dark blade. 3-hit combo releases shadow shockwave on final hit.',
  },
  {
    id: 'holy_lance',
    name: 'Holy Lance',
    type: 'spear',
    spriteKey: 'item_holy_lance',
    atk: 30, range: 72, hitboxW: 72, hitboxH: 16,
    damageType: 'holy', comboHits: 2, swingSpeed: 14,
    trailColor: 0xffffff, trailWidth: 5,
    specialMove: 'lance_charge',
    critMod: 5,
    description: 'Blessed lance. Thrusting attack pierces multiple enemies.',
  },
  {
    id: 'death_scythe',
    name: 'Death\'s Scythe',
    type: 'scythe',
    spriteKey: 'item_death_scythe',
    atk: 48, range: 65, hitboxW: 65, hitboxH: 30,
    damageType: 'dark', comboHits: 2, swingSpeed: 11,
    trailColor: 0x00ff44, trailWidth: 10,
    specialMove: 'soul_reap',
    critMod: 20,
    description: 'Soul reaping scythe. Drains HP equal to 30% of damage dealt.',
  },
  {
    id: 'flame_sword',
    name: 'Flame Sword',
    type: 'sword',
    spriteKey: 'item_flame_sword',
    atk: 35, range: 55, hitboxW: 55, hitboxH: 24,
    damageType: 'fire', comboHits: 2, swingSpeed: 16,
    trailColor: 0xff3300, trailWidth: 9,
    specialMove: 'fire_wave',
    critMod: 6,
    description: 'Ignites enemies on hit. Launches fire wave horizontally on special.',
  },
];

export const WEAPON_MAP: Record<string, WeaponDef> =
  Object.fromEntries(WEAPON_ROSTER.map(w => [w.id, w]));
