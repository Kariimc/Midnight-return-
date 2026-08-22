/**
 * Belmont's Curse: Shadow Reign - Gear & Equipment Inventory Modal
 * SOTN-style Equipment slots with paper-doll character stats and real-time visual gear updates.
 */

import React, { useState } from 'react';
import { GameEngine } from '../engine/gameEngine';
import { EquipmentItem, EquipmentSlotType } from '../types/game';
import { Shield, Sword, X, Sparkles, Check } from 'lucide-react';
import { audio } from '../utils/audio';

interface InventoryModalProps {
  engine: GameEngine;
  onClose: () => void;
}

export const InventoryModal: React.FC<InventoryModalProps> = ({ engine, onClose }) => {
  const [selectedSlot, setSelectedSlot] = useState<EquipmentSlotType>('weapon');
  const [hoveredItem, setHoveredItem] = useState<EquipmentItem | null>(null);

  const { hp, maxHp, mp, maxMp, level, str, con, int, lck } = engine.playerStats;
  const { totalAtk, totalDef, totalCrit } = engine.getComputedStats();

  const slotLabels: Record<EquipmentSlotType, string> = {
    weapon: 'Main Weapon',
    helmet: 'Helmet / Crown',
    armor: 'Body Armor',
    cape: 'Cape / Mantle',
    accessory: 'Accessory',
    subweapon: 'Sub-Weapon'
  };

  const handleEquip = (item: EquipmentItem) => {
    engine.equipped[item.slot] = item;
    audio.playItemPick();
  };

  const filteredInventory = engine.inventory.filter((item) => item.slot === selectedSlot);

  return (
    <div className="fixed inset-0 z-50 bg-black/80 backdrop-blur-md flex items-center justify-center p-4 select-none">
      <div className="bg-zinc-950 border-2 border-amber-800/80 rounded-xl w-full max-w-4xl max-h-[90vh] flex flex-col overflow-hidden shadow-2xl text-amber-100">
        {/* Header */}
        <div className="flex justify-between items-center p-4 border-b border-amber-900/50 bg-zinc-900/90">
          <div className="flex items-center gap-2">
            <Sword className="w-5 h-5 text-amber-400" />
            <h2 className="text-lg font-bold uppercase tracking-wider text-amber-200">Equipment & Character Stats</h2>
          </div>
          <button
            onClick={() => {
              audio.playUiClick();
              onClose();
            }}
            className="p-1.5 rounded-md hover:bg-zinc-800 text-amber-400 transition"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Content Body */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 p-6 overflow-y-auto">
          {/* Column 1: Player Stats & Paper-Doll */}
          <div className="bg-zinc-900/80 border border-amber-900/40 rounded-lg p-4 flex flex-col gap-4">
            <div className="flex items-center gap-3 border-b border-zinc-800 pb-3">
              <div className="w-12 h-12 rounded-full bg-amber-950 border border-amber-500/50 flex flex-col items-center justify-center font-bold">
                <span className="text-[10px] text-amber-400">LVL</span>
                <span className="text-base text-amber-100">{level}</span>
              </div>
              <div className="flex flex-col">
                <span className="font-extrabold text-amber-100 text-sm">Vampire Hunter</span>
                <span className="text-xs text-amber-400/80">Shadow Clan Warrior</span>
              </div>
            </div>

            {/* Stats Breakdown */}
            <div className="flex flex-col gap-2 text-xs">
              <div className="flex justify-between border-b border-zinc-800/60 pb-1">
                <span className="text-zinc-400">Total Attack:</span>
                <strong className="text-amber-300 font-mono">{totalAtk}</strong>
              </div>
              <div className="flex justify-between border-b border-zinc-800/60 pb-1">
                <span className="text-zinc-400">Total Defense:</span>
                <strong className="text-blue-300 font-mono">{totalDef}</strong>
              </div>
              <div className="flex justify-between border-b border-zinc-800/60 pb-1">
                <span className="text-zinc-400">Crit Rate:</span>
                <strong className="text-red-300 font-mono">{(totalCrit * 100).toFixed(0)}%</strong>
              </div>
              <div className="flex justify-between border-b border-zinc-800/60 pb-1">
                <span className="text-zinc-400">Strength (STR):</span>
                <strong className="text-amber-100 font-mono">{str}</strong>
              </div>
              <div className="flex justify-between border-b border-zinc-800/60 pb-1">
                <span className="text-zinc-400">Constitution (CON):</span>
                <strong className="text-amber-100 font-mono">{con}</strong>
              </div>
              <div className="flex justify-between border-b border-zinc-800/60 pb-1">
                <span className="text-zinc-400">Intelligence (INT):</span>
                <strong className="text-amber-100 font-mono">{int}</strong>
              </div>
              <div className="flex justify-between border-b border-zinc-800/60 pb-1">
                <span className="text-zinc-400">Luck (LCK):</span>
                <strong className="text-amber-100 font-mono">{lck}</strong>
              </div>
            </div>
          </div>

          {/* Column 2: Equipment Slots Selector */}
          <div className="flex flex-col gap-3">
            <h3 className="text-xs font-bold text-amber-400 uppercase tracking-wider">Equipped Gear</h3>
            {(Object.keys(slotLabels) as EquipmentSlotType[]).map((slot) => {
              const equippedItem = engine.equipped[slot];
              const isSelected = selectedSlot === slot;

              return (
                <button
                  key={slot}
                  onClick={() => {
                    setSelectedSlot(slot);
                    audio.playUiClick();
                  }}
                  className={`p-3 rounded-lg border text-left flex items-center justify-between transition ${
                    isSelected
                      ? 'bg-amber-950/60 border-amber-400 shadow-[0_0_15px_rgba(245,158,11,0.2)]'
                      : 'bg-zinc-900/60 border-amber-900/30 hover:border-amber-700/60'
                  }`}
                >
                  <div className="flex flex-col">
                    <span className="text-[10px] text-amber-400 uppercase font-semibold">{slotLabels[slot]}</span>
                    <span className="text-xs font-bold text-amber-100">{equippedItem ? equippedItem.name : 'Empty Slot'}</span>
                  </div>

                  {equippedItem && (
                    <span className="text-[10px] bg-amber-900/60 border border-amber-600/40 text-amber-200 px-2 py-0.5 rounded font-mono">
                      +{equippedItem.stats.attack || equippedItem.stats.defense || 0}
                    </span>
                  )}
                </button>
              );
            })}
          </div>

          {/* Column 3: Inventory List for Selected Slot */}
          <div className="flex flex-col gap-3">
            <h3 className="text-xs font-bold text-amber-400 uppercase tracking-wider">Available {slotLabels[selectedSlot]}s</h3>

            <div className="flex flex-col gap-2 max-h-72 overflow-y-auto pr-1">
              {filteredInventory.length === 0 ? (
                <div className="p-4 bg-zinc-900/40 border border-zinc-800 rounded text-center text-xs text-zinc-500">
                  No gear collected for this slot yet.
                </div>
              ) : (
                filteredInventory.map((item) => {
                  const isCurrentlyEquipped = engine.equipped[item.slot]?.id === item.id;

                  return (
                    <div
                      key={item.id}
                      onMouseEnter={() => setHoveredItem(item)}
                      onClick={() => handleEquip(item)}
                      className={`p-3 rounded-lg border cursor-pointer flex items-center justify-between transition ${
                        isCurrentlyEquipped
                          ? 'bg-amber-900/40 border-amber-400'
                          : 'bg-zinc-900/80 border-zinc-800 hover:border-amber-600'
                      }`}
                    >
                      <div className="flex flex-col">
                        <span className="text-xs font-bold text-white flex items-center gap-1">
                          {item.name}
                          {isCurrentlyEquipped && <Check className="w-3.5 h-3.5 text-amber-400" />}
                        </span>
                        <span className="text-[10px] text-zinc-400">{item.description}</span>
                      </div>

                      <button className="text-[10px] bg-amber-500 text-zinc-950 font-bold px-2 py-1 rounded hover:bg-amber-400 transition">
                        {isCurrentlyEquipped ? 'Equipped' : 'Equip'}
                      </button>
                    </div>
                  );
                })
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
