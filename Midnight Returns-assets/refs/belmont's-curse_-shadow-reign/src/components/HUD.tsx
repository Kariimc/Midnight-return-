/**
 * Belmont's Curse: Shadow Reign - Dark Gothic HUD & Status Bars
 * Renders ornate health, mana, XP, active weapon, soul shards, and boss health bar.
 */

import React, { useState, useEffect } from 'react';
import { GameEngine } from '../engine/gameEngine';
import { Shield, Zap, Crown, Sword, Sparkles, Map, Hammer, Settings as SettingsIcon, Music, Gamepad2 } from 'lucide-react';
import { audio } from '../utils/audio';

interface HUDProps {
  engine: GameEngine;
  onOpenInventory: () => void;
  onOpenSkills: () => void;
  onOpenMap: () => void;
  onOpenForge: () => void;
  onOpenSettings: () => void;
}

export const HUD: React.FC<HUDProps> = ({
  engine,
  onOpenInventory,
  onOpenSkills,
  onOpenMap,
  onOpenForge,
  onOpenSettings
}) => {
  const { hp, maxHp, mp, maxMp, xp, nextLevelXp, level, soulShards, darkOre } = engine.playerStats;
  const { totalAtk, totalDef } = engine.getComputedStats();
  const currentWeapon = engine.equipped.weapon;

  const [trackInfo, setTrackInfo] = useState<{ title: string; composer: string } | null>(() => audio.getTrackInfo());

  useEffect(() => {
    const handleTrackChange = (info: { title: string; composer: string } | null) => {
      setTrackInfo(info);
    };
    audio.addTrackChangeListener(handleTrackChange);
    return () => {
      audio.removeTrackChangeListener(handleTrackChange);
    };
  }, []);

  const [ghostHp, setGhostHp] = useState(hp);

  useEffect(() => {
    if (hp < ghostHp) {
      const timer = setTimeout(() => {
        setGhostHp((prev) => Math.max(hp, prev - 2));
      }, 300);
      return () => clearTimeout(timer);
    } else if (hp > ghostHp) {
      setGhostHp(hp);
    }
  }, [hp, ghostHp]);

  // Find active boss in current room
  const activeBoss = engine.activeEnemies.find((e) => e.type.behavior === 'boss');

  return (
    <div className="absolute inset-0 pointer-events-none flex flex-col justify-between p-4 z-20 select-none">
      {/* Top Center: Orchestral BGM Now Playing Banner */}
      {trackInfo && (
        <div className="absolute top-4 left-1/2 -translate-x-1/2 bg-zinc-950/90 border border-amber-600/70 rounded-full px-4 py-1.5 backdrop-blur-xl shadow-2xl flex items-center gap-2 text-[11px] text-amber-200 pointer-events-auto transition-all animate-fade-in z-30">
          <Music className="w-3.5 h-3.5 text-amber-400 animate-pulse" />
          <span className="font-bold font-serif text-amber-300">{trackInfo.title}</span>
          <span className="text-zinc-400 text-[10px] hidden sm:inline">({trackInfo.composer})</span>
        </div>
      )}
      {/* Top Left: Player Status Frame */}
      <div className="flex items-start gap-4">
        <div className="relative gothic-card rounded-xl p-3 backdrop-blur-xl pointer-events-auto flex items-center gap-3">
          {/* Level Gem Emblem */}
          <div className="relative w-14 h-14 rounded-full bg-gradient-to-br from-amber-950 via-zinc-900 to-amber-900 border-2 border-amber-500/80 flex flex-col items-center justify-center shadow-[0_0_15px_rgba(245,158,11,0.3)]">
            <span className="text-[9px] text-amber-400 font-bold tracking-widest uppercase font-cinzel">LVL</span>
            <span className="text-xl font-black text-amber-100 font-cinzel leading-none">{level}</span>
          </div>

          {/* Bar Gauges */}
          <div className="flex flex-col gap-1.5 w-52 sm:w-64">
            {/* Health Bar */}
            <div className="flex flex-col">
              <div className="flex justify-between items-center text-[11px] font-bold text-red-200 mb-0.5">
                <span className="flex items-center gap-1 font-cinzel text-red-400"><Shield className="w-3.5 h-3.5" /> HP</span>
                <span className="font-mono text-[10px] text-red-200">{hp} / {maxHp}</span>
              </div>
              <div className="w-full h-3.5 bg-zinc-950 rounded border border-red-900/80 overflow-hidden relative shadow-inner">
                {/* Ghost damage bar */}
                <div
                  className="absolute top-0 left-0 h-full bg-amber-600/80 transition-all duration-300"
                  style={{ width: `${Math.max(0, Math.min(100, (ghostHp / maxHp) * 100))}%` }}
                />
                {/* Main health bar */}
                <div
                  className="relative z-10 h-full bg-gradient-to-r from-red-900 via-red-600 to-amber-500 transition-all duration-150 shadow-[0_0_10px_rgba(239,68,68,0.5)]"
                  style={{ width: `${Math.max(0, Math.min(100, (hp / maxHp) * 100))}%` }}
                />
              </div>
            </div>

            {/* Mana Bar */}
            <div className="flex flex-col">
              <div className="flex justify-between items-center text-[11px] font-bold text-sky-200 mb-0.5">
                <span className="flex items-center gap-1 font-cinzel text-sky-400"><Zap className="w-3.5 h-3.5" /> MP</span>
                <span className="font-mono text-[10px] text-sky-200">{mp} / {maxMp}</span>
              </div>
              <div className="w-full h-3 bg-zinc-950 rounded border border-sky-900/80 overflow-hidden relative shadow-inner">
                <div
                  className="h-full bg-gradient-to-r from-blue-800 via-sky-500 to-indigo-400 transition-all duration-150 shadow-[0_0_10px_rgba(56,189,248,0.5)]"
                  style={{ width: `${Math.max(0, Math.min(100, (mp / maxMp) * 100))}%` }}
                />
              </div>
            </div>

            {/* Experience Bar */}
            <div className="w-full h-1.5 bg-zinc-950 rounded-full border border-amber-900/60 overflow-hidden mt-0.5">
              <div
                className="h-full bg-gradient-to-r from-amber-500 to-yellow-300 transition-all duration-300 shadow-[0_0_8px_rgba(251,191,36,0.6)]"
                style={{ width: `${Math.max(0, Math.min(100, (xp / nextLevelXp) * 100))}%` }}
              />
            </div>
          </div>
        </div>

        {/* Currencies & Equipped Stats */}
        <div className="hidden sm:flex flex-col gap-1.5 gothic-card rounded-lg p-3 text-xs text-amber-200 font-cinzel">
          <div className="flex items-center gap-2">
            <Sparkles className="w-3.5 h-3.5 text-sky-400" />
            <span>Shards: <strong className="text-white font-mono">{soulShards}</strong></span>
          </div>
          <div className="flex items-center gap-2">
            <Hammer className="w-3.5 h-3.5 text-amber-500" />
            <span>Dark Ore: <strong className="text-white font-mono">{darkOre}</strong></span>
          </div>
          <div className="flex items-center gap-2 text-amber-400 font-bold border-t border-amber-900/40 pt-1">
            <Sword className="w-3.5 h-3.5" />
            <span>ATK: {totalAtk} | DEF: {totalDef}</span>
          </div>
        </div>
      </div>

      {/* Top Right: Navigation Buttons */}
      <div className="absolute top-4 right-4 pointer-events-auto flex items-center gap-2 font-cinzel">
        {engine.gamepadConnected && (
          <div className="bg-emerald-950/90 border border-emerald-500/80 text-emerald-300 px-3 py-2 rounded-lg flex items-center gap-1.5 text-[11px] font-bold shadow-xl animate-pulse">
            <Gamepad2 className="w-4 h-4 text-emerald-400" />
            <span className="hidden xl:inline">{engine.gamepadName.split(' ')[0]} Active</span>
          </div>
        )}

        <button
          onClick={onOpenInventory}
          className="bg-zinc-950/90 border border-amber-600/70 hover:border-amber-400 text-amber-200 px-3.5 py-2 rounded-lg flex items-center gap-2 text-xs font-bold shadow-xl hover:bg-amber-950/40 transition active:scale-95"
        >
          <Sword className="w-4 h-4 text-amber-400" />
          <span className="hidden md:inline">Gear (I)</span>
        </button>

        <button
          onClick={onOpenSkills}
          className="bg-zinc-950/90 border border-amber-600/70 hover:border-amber-400 text-amber-200 px-3.5 py-2 rounded-lg flex items-center gap-2 text-xs font-bold shadow-xl hover:bg-amber-950/40 transition active:scale-95"
        >
          <Sparkles className="w-4 h-4 text-sky-400" />
          <span className="hidden md:inline">Skills (K)</span>
        </button>

        <button
          onClick={onOpenMap}
          className="bg-zinc-950/90 border border-amber-600/70 hover:border-amber-400 text-amber-200 px-3.5 py-2 rounded-lg flex items-center gap-2 text-xs font-bold shadow-xl hover:bg-amber-950/40 transition active:scale-95"
        >
          <Map className="w-4 h-4 text-emerald-400" />
          <span className="hidden md:inline">Map (M)</span>
        </button>

        <button
          onClick={onOpenForge}
          className="bg-zinc-950/90 border border-amber-600/70 hover:border-amber-400 text-amber-200 px-3.5 py-2 rounded-lg flex items-center gap-2 text-xs font-bold shadow-xl hover:bg-amber-950/40 transition active:scale-95"
        >
          <Hammer className="w-4 h-4 text-orange-400" />
          <span className="hidden md:inline">Forge</span>
        </button>

        <button
          onClick={onOpenSettings}
          className="bg-zinc-950/90 border border-amber-600/70 hover:border-amber-400 text-amber-200 p-2 rounded-lg shadow-xl hover:bg-amber-950/40 transition active:scale-95"
        >
          <SettingsIcon className="w-4 h-4 text-zinc-300" />
        </button>
      </div>

      {/* Bottom Center: BOSS HEALTH BAR (If in active boss fight) */}
      {activeBoss && (
        <div className="w-full max-w-2xl mx-auto gothic-card rounded-xl p-3.5 shadow-2xl backdrop-blur-xl flex flex-col gap-2 mb-8 animate-fade-in pointer-events-auto border-red-800">
          <div className="flex justify-between items-center text-red-200 text-sm font-black tracking-widest uppercase font-cinzel">
            <span className="flex items-center gap-2">
              <Crown className="w-5 h-5 text-amber-400 animate-bounce" />
              {activeBoss.type.name}
            </span>
            <span className="text-xs text-red-400 font-mono">
              {Math.max(0, activeBoss.hp)} / {activeBoss.maxHp}
            </span>
          </div>
          <div className="w-full h-4 bg-zinc-950 rounded border border-red-900 overflow-hidden relative shadow-inner">
            <div
              className="h-full bg-gradient-to-r from-red-950 via-red-600 to-amber-500 transition-all duration-150 shadow-[0_0_15px_rgba(220,38,38,0.8)]"
              style={{ width: `${Math.max(0, Math.min(100, (activeBoss.hp / activeBoss.maxHp) * 100))}%` }}
            />
          </div>
        </div>
      )}

      {/* Active Weapon Indicator (Bottom Left) */}
      <div className="gothic-card rounded-lg p-2.5 flex items-center gap-3 text-xs text-amber-200 w-fit pointer-events-auto backdrop-blur-md font-cinzel">
        <div className="w-9 h-9 rounded-md bg-zinc-950 border border-amber-600/60 flex items-center justify-center shadow-inner">
          <Sword className="w-4 h-4 text-amber-400" />
        </div>
        <div className="flex flex-col">
          <span className="font-extrabold text-amber-100 text-xs">{currentWeapon?.name || 'Unarmed'}</span>
          <span className="text-[10px] text-amber-400 font-mono">ATK +{currentWeapon?.stats.attack || 0}</span>
        </div>
      </div>
    </div>
  );
};

