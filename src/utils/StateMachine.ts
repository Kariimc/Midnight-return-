export interface IState {
  name: string;
  onEnter?(prevState?: IState): void;
  onUpdate?(dt: number): void;
  onExit?(nextState?: IState): void;
}

export class StateMachine {
  private states = new Map<string, IState>();
  private current: IState | null = null;
  private locked = false;

  add(state: IState): this {
    this.states.set(state.name, state);
    return this;
  }

  transition(name: string): boolean {
    if (this.locked) return false;
    const next = this.states.get(name);
    if (!next || next === this.current) return false;

    const prev = this.current;
    this.current?.onExit?.(next);
    this.current = next;
    this.current.onEnter?.(prev ?? undefined);
    return true;
  }

  update(dt: number): void {
    this.current?.onUpdate?.(dt);
  }

  get state(): string {
    return this.current?.name ?? 'none';
  }

  is(name: string): boolean {
    return this.current?.name === name;
  }

  lock(): void  { this.locked = true; }
  unlock(): void { this.locked = false; }
}
