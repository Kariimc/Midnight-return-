import { StatBlock, createBaseStats } from './PlayerStats';
import { bus, EVENTS } from '../utils/EventBus';

export type GamePhase = 'boot' | 'menu' | 'playing' | 'paused' | 'dead' | 'cutscene';

export interface EquipSlots {
  rightHand: string | null;
  leftHand:  string | null;
  helmet:    string | null;
  armor:     string | null;
  cloak:     string | null;
  boots:     string | null;
  accessory1: string | null;
  accessory2: string | null;
}

export interface SaveData {
  slot: number;
  playerName: string;
  stats: StatBlock;
  equip: EquipSlots;
  inventory: Record<string, number>; // itemId → quantity
  mapExplored: Record<string, boolean>; // roomId → visited
  defeatedBosses: string[];
  playtime: number; // seconds
  savedAt: number;  // timestamp
}

class GameStateManager {
  phase: GamePhase = 'boot';
  save: SaveData = this.defaultSave();

  // Runtime-only (not persisted)
  isPaused = false;
  isInBossFight = false;
  currentRoomId = 'entrance_hall';

  private defaultSave(): SaveData {
    return {
      slot: 0,
      playerName: 'Alucard',
      stats: createBaseStats(),
      equip: {
        rightHand: null, leftHand: null,
        helmet: null, armor: null, cloak: null, boots: null,
        accessory1: null, accessory2: null,
      },
      inventory: {},
      mapExplored: {},
      defeatedBosses: [],
      playtime: 0,
      savedAt: Date.now(),
    };
  }

  setPhase(p: GamePhase): void {
    this.phase = p;
  }

  addToInventory(itemId: string, qty = 1): void {
    this.save.inventory[itemId] = (this.save.inventory[itemId] ?? 0) + qty;
    bus.emit(EVENTS.ITEM_PICKED_UP, { itemId, qty });
  }

  markRoomVisited(roomId: string): void {
    this.save.mapExplored[roomId] = true;
    bus.emit(EVENTS.ROOM_ENTERED, { roomId });
  }

  defeatBoss(bossId: string): void {
    if (!this.save.defeatedBosses.includes(bossId)) {
      this.save.defeatedBosses.push(bossId);
      bus.emit(EVENTS.BOSS_DEFEATED, { bossId });
    }
  }

  serialize(): string { return JSON.stringify(this.save); }
  deserialize(json: string): void { this.save = JSON.parse(json) as SaveData; }

  toLocalStorage(slot: number): void {
    localStorage.setItem(`midnight_save_${slot}`, this.serialize());
  }

  fromLocalStorage(slot: number): boolean {
    const raw = localStorage.getItem(`midnight_save_${slot}`);
    if (!raw) return false;
    this.deserialize(raw);
    return true;
  }
}

export const GameState = new GameStateManager();
