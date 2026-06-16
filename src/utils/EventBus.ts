type Handler<T = unknown> = (payload: T) => void;

class EventBus {
  private listeners = new Map<string, Set<Handler<any>>>();

  on<T>(event: string, handler: Handler<T>): () => void {
    if (!this.listeners.has(event)) this.listeners.set(event, new Set());
    this.listeners.get(event)!.add(handler as Handler<unknown>);
    return () => this.off(event, handler);
  }

  once<T>(event: string, handler: Handler<T>): void {
    const wrapper = (payload: T) => {
      handler(payload);
      this.off(event, wrapper);
    };
    this.on(event, wrapper);
  }

  off<T>(event: string, handler: Handler<T>): void {
    this.listeners.get(event)?.delete(handler as Handler<unknown>);
  }

  emit<T>(event: string, payload?: T): void {
    this.listeners.get(event)?.forEach(h => h(payload));
  }

  clear(event?: string): void {
    if (event) this.listeners.delete(event);
    else this.listeners.clear();
  }
}

export const bus = new EventBus();

// Typed event catalogue — extend as needed
export const EVENTS = {
  PLAYER_DAMAGED:    'player:damaged',
  PLAYER_DIED:       'player:died',
  PLAYER_LEVELED_UP: 'player:leveledUp',
  ENEMY_DIED:        'enemy:died',
  ITEM_PICKED_UP:    'item:pickedUp',
  ROOM_ENTERED:      'room:entered',
  BOSS_STARTED:      'boss:started',
  BOSS_DEFEATED:     'boss:defeated',
  UI_TOGGLE_MAP:     'ui:toggleMap',
  UI_TOGGLE_MENU:    'ui:toggleMenu',
} as const;
