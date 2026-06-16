export class ObjectPool<T> {
  private pool: T[] = [];
  private active = new Set<T>();

  constructor(
    private readonly factory: () => T,
    private readonly reset: (obj: T) => void,
    preWarm = 0,
  ) {
    for (let i = 0; i < preWarm; i++) this.pool.push(factory());
  }

  get(): T {
    const obj = this.pool.pop() ?? this.factory();
    this.active.add(obj);
    return obj;
  }

  release(obj: T): void {
    if (!this.active.has(obj)) return;
    this.active.delete(obj);
    this.reset(obj);
    this.pool.push(obj);
  }

  releaseAll(): void {
    this.active.forEach(obj => {
      this.reset(obj);
      this.pool.push(obj);
    });
    this.active.clear();
  }

  get activeCount(): number { return this.active.size; }
  get pooledCount(): number { return this.pool.length; }
}
