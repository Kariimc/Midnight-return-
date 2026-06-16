// Faithful SotN RPG stat system with growth tables
export interface StatBlock {
  level: number;
  exp: number;
  expToNext: number;
  // Primary
  str: number; // physical attack multiplier
  con: number; // HP growth + physical defense
  int: number; // MP growth + spell power
  lck: number; // crit rate, drop rate, damage variance
  // Derived
  hp: number;
  maxHp: number;
  mp: number;
  maxMp: number;
  atk: number; // weapon + str bonus
  def: number; // armor + con bonus
  // Resistances (0–100%)
  fireRes: number;
  iceRes: number;
  lightningRes: number;
  darkRes: number;
  holyRes: number;
  poisonRes: number;
}

export function createBaseStats(): StatBlock {
  return {
    level: 1, exp: 0, expToNext: 100,
    str: 10, con: 10, int: 10, lck: 10,
    hp: 120, maxHp: 120, mp: 50, maxMp: 50,
    atk: 10, def: 5,
    fireRes: 0, iceRes: 0, lightningRes: 0,
    darkRes: 0, holyRes: 0, poisonRes: 0,
  };
}

export function expThreshold(level: number): number {
  // Exponential growth curve matching SotN feel
  return Math.floor(100 * Math.pow(1.45, level - 1));
}

export function levelUp(stats: StatBlock): StatBlock {
  const next = { ...stats };
  next.level++;
  next.expToNext = expThreshold(next.level);
  // Stat growth (random variance like SotN)
  next.maxHp  += Math.floor(5  + next.con * 0.4 + Math.random() * 4);
  next.maxMp  += Math.floor(2  + next.int * 0.25 + Math.random() * 3);
  next.atk    += Math.floor(0.3 + next.str * 0.05);
  next.def    += Math.floor(0.2 + next.con * 0.04);
  next.str    += 1;
  next.con    += 1;
  next.int    += 1;
  next.lck    += Math.random() > 0.7 ? 1 : 0;
  // Full heal on level up (SotN tradition)
  next.hp = next.maxHp;
  next.mp = next.maxMp;
  return next;
}

export function addExp(stats: StatBlock, amount: number): { stats: StatBlock; leveled: boolean } {
  let s = { ...stats, exp: stats.exp + amount };
  let leveled = false;
  while (s.exp >= s.expToNext) {
    s.exp -= s.expToNext;
    s = levelUp(s);
    leveled = true;
  }
  return { stats: s, leveled };
}

export function calcDamage(
  atkStat: number,
  defStat: number,
  lck: number,
  variance = 0.1,
): number {
  const base = Math.max(1, atkStat - Math.floor(defStat * 0.5));
  const critChance = clamp(lck * 0.5, 0, 25) / 100;
  const isCrit = Math.random() < critChance;
  const v = 1 + (Math.random() * 2 - 1) * variance;
  return Math.floor(base * v * (isCrit ? 2 : 1));
}

function clamp(v: number, min: number, max: number): number {
  return Math.max(min, Math.min(max, v));
}
