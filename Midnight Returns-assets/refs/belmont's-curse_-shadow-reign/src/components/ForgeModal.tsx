/**
 * Belmont's Curse: Shadow Reign - Forge & Altar Upgrade Modal
 * Upgrade weapons & equipment (+1 to +10) spending Soul Shards & Dark Ore.
 */

import React, { useState } from 'react';
import { GameEngine } from '../engine/gameEngine';
import { EquipmentItem } from '../types/game';
import { Hammer, X, Sparkles, Sword } from 'lucide-react';
import { audio } from '../utils/audio';

interface ForgeModalProps {
  engine: GameEngine;
  onClose: () => void;
}

export const ForgeModal: React.FC<ForgeModalProps> = ({ engine, onClose }) => {
  const [selectedWeapon, setSelectedWeapon] = useState<EquipmentItem | null>(
    engine.equipped.weapon
  );

  const { soulShards, darkOre } = engine.playerStats;

  const handleUpgrade = (item: EquipmentItem) => {
    if (item.level >= item.maxLevel) return;
    const cost = item.upgradeCost;

    if (soulShards < cost.shards || darkOre < cost.ore) return;

    // Deduct resources
    engine.playerStats.soulShards -= cost.shards;
    engine.playerStats.darkOre -= cost.ore;

    // Level up item
    item.level += 1;
    item.stats.attack = Math.floor((item.stats.attack || 10) * 1.2);
    item.stats.critChance = Math.min(0.5, (item.stats.critChance || 0.1) + 0.02);

    // Increase next upgrade cost
    item.upgradeCost.shards = Math.floor(cost.shards * 1.4);
    item.upgradeCost.ore = Math.floor(cost.ore * 1.3);

    audio.playParry();
    engine.particleEngine.spawnSparks(
      engine.playerPos.x + engine.playerWidth / 2,
      engine.playerPos.y + engine.playerHeight / 2,
      25,
      '#f59e0b'
    );
    engine.addFloatingText(`${item.name} UPGRADED TO +${item.level}!`, engine.playerPos.x, engine.playerPos.y - 30, '#f59e0b', 1.4);
  };

  const weaponsList = engine.inventory.filter((item) => item.slot === 'weapon');

  return (
    <div className="fixed inset-0 z-50 bg-black/80 backdrop-blur-md flex items-center justify-center p-4 select-none">
      <div className="bg-zinc-950 border-2 border-amber-800/80 rounded-xl w-full max-w-3xl max-h-[90vh] flex flex-col overflow-hidden shadow-2xl text-amber-100">
        {/* Header */}
        <div className="flex justify-between items-center p-4 border-b border-amber-900/50 bg-zinc-900/90">
          <div className="flex items-center gap-2">
            <Hammer className="w-5 h-5 text-orange-400" />
            <h2 className="text-lg font-bold uppercase tracking-wider text-amber-200">Shadow Forge & Altar</h2>
          </div>

          <div className="flex items-center gap-4">
            <span className="text-xs text-amber-300 font-semibold bg-zinc-900 border border-amber-800/50 px-3 py-1 rounded">
              Shards: <strong className="text-sky-400">{soulShards}</strong> | Ore: <strong className="text-orange-400">{darkOre}</strong>
            </span>
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
        </div>

        {/* Forge Content */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 p-6 overflow-y-auto">
          {/* Left: Weapon Selection */}
          <div className="flex flex-col gap-3">
            <h3 className="text-xs font-bold text-amber-400 uppercase tracking-wider">Select Weapon to Enhance</h3>
            <div className="flex flex-col gap-2 max-h-72 overflow-y-auto pr-1">
              {weaponsList.map((w) => {
                const isSelected = selectedWeapon?.id === w.id;

                return (
                  <button
                    key={w.id}
                    onClick={() => {
                      setSelectedWeapon(w);
                      audio.playUiClick();
                    }}
                    className={`p-3 rounded-lg border text-left flex items-center justify-between transition ${
                      isSelected
                        ? 'bg-amber-950/60 border-amber-400'
                        : 'bg-zinc-900/60 border-amber-900/30 hover:border-amber-700'
                    }`}
                  >
                    <div className="flex flex-col">
                      <span className="text-xs font-bold text-white flex items-center gap-1">
                        {w.name} <span className="text-amber-400 font-mono">+{w.level}</span>
                      </span>
                      <span className="text-[10px] text-zinc-400">ATK: {w.stats.attack}</span>
                    </div>

                    <Sword className="w-4 h-4 text-amber-400" />
                  </button>
                );
              })}
            </div>
          </div>

          {/* Right: Weapon Upgrade Bench */}
          {selectedWeapon ? (
            <div className="bg-zinc-900/80 border border-amber-900/40 rounded-lg p-5 flex flex-col justify-between gap-4">
              <div className="flex flex-col gap-3">
                <div className="flex justify-between items-start border-b border-zinc-800 pb-3">
                  <div className="flex flex-col">
                    <h4 className="font-extrabold text-base text-amber-200">{selectedWeapon.name}</h4>
                    <span className="text-xs text-amber-400 font-mono">Enhancement Level: +{selectedWeapon.level} / {selectedWeapon.maxLevel}</span>
                  </div>
                  <Sparkles className="w-6 h-6 text-amber-400 animate-pulse" />
                </div>

                <div className="flex flex-col gap-2 text-xs">
                  <div className="flex justify-between">
                    <span className="text-zinc-400">Current Attack:</span>
                    <strong className="text-white font-mono">{selectedWeapon.stats.attack}</strong>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-zinc-400">Next Level Attack:</span>
                    <strong className="text-emerald-400 font-mono">
                      {Math.floor((selectedWeapon.stats.attack || 10) * 1.2)}
                    </strong>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-zinc-400">Critical Strike Rate:</span>
                    <strong className="text-amber-300 font-mono">
                      {((selectedWeapon.stats.critChance || 0.1) * 100).toFixed(0)}%
                    </strong>
                  </div>
                </div>
              </div>

              {/* Upgrade Requirements */}
              <div className="bg-zinc-950/60 border border-amber-900/40 rounded p-3 flex flex-col gap-2">
                <span className="text-[10px] text-amber-400 font-bold uppercase tracking-wider">Required Materials</span>
                <div className="flex justify-between text-xs font-mono">
                  <span>Soul Shards: {selectedWeapon.upgradeCost.shards}</span>
                  <span>Dark Ore: {selectedWeapon.upgradeCost.ore}</span>
                </div>

                <button
                  disabled={
                    selectedWeapon.level >= selectedWeapon.maxLevel ||
                    soulShards < selectedWeapon.upgradeCost.shards ||
                    darkOre < selectedWeapon.upgradeCost.ore
                  }
                  onClick={() => handleUpgrade(selectedWeapon)}
                  className={`w-full py-3 rounded-lg font-bold uppercase tracking-wider text-xs flex items-center justify-center gap-2 mt-2 transition ${
                    selectedWeapon.level >= selectedWeapon.maxLevel
                      ? 'bg-zinc-800 text-zinc-500 cursor-not-allowed'
                      : soulShards >= selectedWeapon.upgradeCost.shards && darkOre >= selectedWeapon.upgradeCost.ore
                      ? 'bg-gradient-to-r from-amber-600 to-amber-500 hover:from-amber-500 hover:to-amber-400 text-zinc-950 shadow-lg'
                      : 'bg-zinc-800 text-zinc-500 cursor-not-allowed'
                  }`}
                >
                  <Hammer className="w-4 h-4" />
                  {selectedWeapon.level >= selectedWeapon.maxLevel ? 'MAX LEVEL REACHED' : 'ENHANCE WEAPON'}
                </button>
              </div>
            </div>
          ) : (
            <div className="flex items-center justify-center p-8 text-xs text-zinc-500">
              Select a weapon on the left to begin forging.
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
