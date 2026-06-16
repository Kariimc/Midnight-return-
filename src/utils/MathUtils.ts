export const lerp = (a: number, b: number, t: number): number => a + (b - a) * t;
export const clamp = (v: number, min: number, max: number): number => Math.max(min, Math.min(max, v));
export const sign = (v: number): number => v < 0 ? -1 : v > 0 ? 1 : 0;
export const approach = (cur: number, target: number, delta: number): number => {
  if (cur < target) return Math.min(cur + delta, target);
  return Math.max(cur - delta, target);
};
export const randRange = (min: number, max: number): number => Math.random() * (max - min) + min;
export const randInt = (min: number, max: number): number => Math.floor(randRange(min, max + 1));
export const degToRad = (d: number): number => d * (Math.PI / 180);
export const radToDeg = (r: number): number => r * (180 / Math.PI);

export class Vec2 {
  constructor(public x = 0, public y = 0) {}
  add(v: Vec2): Vec2 { return new Vec2(this.x + v.x, this.y + v.y); }
  sub(v: Vec2): Vec2 { return new Vec2(this.x - v.x, this.y - v.y); }
  scale(s: number): Vec2 { return new Vec2(this.x * s, this.y * s); }
  mag(): number { return Math.hypot(this.x, this.y); }
  norm(): Vec2 { const m = this.mag(); return m === 0 ? new Vec2() : this.scale(1 / m); }
  dot(v: Vec2): number { return this.x * v.x + this.y * v.y; }
  static zero(): Vec2 { return new Vec2(0, 0); }
}
