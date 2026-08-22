/**
 * Belmont's Curse: Shadow Reign - Castlevania: Symphony of the Night (SOTN) Unified Pause Menu & Castle Map System
 * SOTN-style Menu with Equipment Paper-Doll, Relic & Ability Toggles, 200%+ Castle Grid Map, Soul Forge, and Gamepad Setup.
 */

import React, { useState, useEffect } from 'react';
import { GameEngine } from '../engine/gameEngine';
import { EquipmentItem, EquipmentSlotType } from '../types/game';
import {
  Sword,
  Shield,
  Map as MapIcon,
  Sparkles,
  Settings as SettingsIcon,
  X,
  Check,
  Zap,
  Crown,
  Compass,
  Volume2,
  VolumeX,
  Gamepad2,
  Hammer,
  Lock,
  ChevronRight,
  ChevronLeft,
  Flame,
  Radio
} from 'lucide-react';
import { audio } from '../utils/audio';

interface SotnMenuModalProps {
  engine: GameEngine;
  initialTab?: 'equipment' | 'relics' | 'map' | 'forge' | 'settings';
  onClose: () => void;
  showTouchControls: boolean;
  setShowTouchControls: (val: boolean) => void;
}

export const SotnMenuModal: React.FC<SotnMenuModalProps> = ({
  engine,
  initialTab = 'equipment',
  onClose,
  showTouchControls,
  setShowTouchControls
}) => {
  const [activeTab, setActiveTab] = useState<'equipment' | 'relics' | 'map' | 'forge' | 'settings'>(initialTab);

  // Equipment tab state
  const [selectedSlot, setSelectedSlot] = useState<EquipmentSlotType>('weapon');
  const [hoveredItem, setHoveredItem] = useState<EquipmentItem | null>(null);

  // Map tab state
  const [selectedZoneFilter, setSelectedZoneFilter] = useState<string | 'all'>('all');

  // Settings tab state
  const [masterVol, setMasterVol] = useState(0.8);
  const [musicVol, setMusicVol] = useState(0.4);
  const [sfxVol, setSfxVol] = useState(0.7);
  const [isMuted, setIsMuted] = useState(false);

  // Gamepad menu navigation listener
  useEffect(() => {
    let lastTabSwitchTime = 0;
    let lastCloseTime = 0;

    const interval = setInterval(() => {
      if (typeof navigator === 'undefined' || !navigator.getGamepads) return;
      const gamepads = navigator.getGamepads();
      let gp: Gamepad | null = null;
      for (let i = 0; i < gamepads.length; i++) {
        if (gamepads[i] && gamepads[i]?.connected) {
          gp = gamepads[i];
          break;
        }
      }
      if (!gp) return;

      const now = Date.now();
      const buttons = gp.buttons;

      // L1 / LB (Button 4) -> Previous Tab
      if (buttons[4]?.pressed && now - lastTabSwitchTime > 250) {
        lastTabSwitchTime = now;
        audio.playUiClick();
        setActiveTab((prev) => {
          if (prev === 'equipment') return 'settings';
          if (prev === 'relics') return 'equipment';
          if (prev === 'map') return 'relics';
          if (prev === 'forge') return 'map';
          return 'forge';
        });
      }

      // R1 / RB (Button 5) -> Next Tab
      if (buttons[5]?.pressed && now - lastTabSwitchTime > 250) {
        lastTabSwitchTime = now;
        audio.playUiClick();
        setActiveTab((prev) => {
          if (prev === 'equipment') return 'relics';
          if (prev === 'relics') return 'map';
          if (prev === 'map') return 'forge';
          if (prev === 'forge') return 'settings';
          return 'equipment';
        });
      }

      // Button B / Circle (Button 1) or Start (Button 9) -> Close Menu
      if ((buttons[1]?.pressed || buttons[9]?.pressed) && now - lastCloseTime > 400) {
        lastCloseTime = now;
        audio.playUiClick();
        onClose();
      }
    }, 100);

    return () => clearInterval(interval);
  }, [onClose]);

  // Player Stats Calculations
  const { hp, maxHp, mp, maxMp, level, str, con, int, lck, soulShards, darkOre } = engine.playerStats;
  const { totalAtk, totalDef, totalCrit } = engine.getComputedStats();
  const currentRoom = engine.getCurrentRoom();

  // Map Completion Statistics (SOTN 200%+ Scale)
  let totalRooms = 0;
  let exploredCount = 0;
  engine.zones.forEach((zone) => {
    zone.rooms.forEach((room) => {
      totalRooms++;
      if (room.explored) exploredCount++;
    });
  });
  const rawPercent = totalRooms > 0 ? (exploredCount / totalRooms) * 200.5 : 0;
  const completionPercent = rawPercent.toFixed(1);

  // Slot Labels mapping
  const slotLabels: Record<EquipmentSlotType, string> = {
    weapon: 'Main Weapon',
    subweapon: 'Sub-Weapon',
    helmet: 'Helmet / Visor',
    armor: 'Body Cuirass',
    cape: 'Cape / Mantle',
    accessory: 'Gothic Relic Ring'
  };

  const handleEquip = (item: EquipmentItem) => {
    engine.equipped[item.slot] = item;
    audio.playItemPick();
  };

  const toggleRelic = (abilityKey: string) => {
    engine.abilities[abilityKey] = !engine.abilities[abilityKey];
    audio.playItemPick();
  };

  // Sound Settings handlers
  const handleMasterChange = (val: number) => {
    setMasterVol(val);
    audio.setMasterVolume(val);
  };
  const handleMusicChange = (val: number) => {
    setMusicVol(val);
    audio.setMusicVolume(val);
  };
  const handleSfxChange = (val: number) => {
    setSfxVol(val);
    audio.setSfxVolume(val);
  };

  return (
    <div className="fixed inset-0 z-50 bg-slate-950/95 backdrop-blur-xl flex items-center justify-center p-3 sm:p-6 select-none font-serif">
      {/* SOTN Gothic Velvet Frame */}
      <div className="bg-gradient-to-b from-slate-950 via-zinc-900 to-slate-950 border-2 border-amber-600/80 rounded-2xl w-full max-w-5xl h-[90vh] flex flex-col overflow-hidden shadow-[0_0_50px_rgba(245,158,11,0.25)] text-amber-100 relative">
        {/* Top Decorative Filigree Bar */}
        <div className="bg-gradient-to-r from-amber-950 via-zinc-900 to-amber-950 border-b-2 border-amber-600/70 p-3 sm:p-4 flex flex-wrap items-center justify-between gap-3 shadow-xl">
          <div className="flex items-center gap-3">
            <div className="w-9 h-9 rounded-full bg-amber-950 border border-amber-400 flex items-center justify-center text-amber-300 shadow-inner font-black text-sm">
              🦇
            </div>
            <div>
              <h1 className="text-base sm:text-lg font-black uppercase tracking-widest text-amber-200 font-cinzel drop-shadow">
                Castlevania: Symphony of the Night
              </h1>
              <p className="text-[10px] sm:text-xs text-amber-400/80 font-sans">
                Belmont's Curse — Real-Time Character Matrix & Castle Records
              </p>
            </div>
          </div>

          {/* Controller & Keyboard Hotkey Prompt */}
          <div className="hidden lg:flex items-center gap-3 text-[11px] font-sans bg-zinc-950/80 border border-amber-700/60 px-3 py-1.5 rounded-lg text-amber-300">
            <Gamepad2 className="w-4 h-4 text-emerald-400 animate-pulse" />
            <span>
              <strong>LB/RB</strong> Switch Tab &nbsp;|&nbsp; <strong>A</strong> Equip &nbsp;|&nbsp; <strong>B</strong> Close
            </span>
          </div>

          <button
            onClick={() => {
              audio.playUiClick();
              onClose();
            }}
            className="p-1.5 rounded-lg bg-amber-950/60 border border-amber-600/60 hover:bg-amber-600 hover:text-black text-amber-300 transition"
          >
            <X className="w-6 h-6" />
          </button>
        </div>

        {/* Navigation Tabs Header */}
        <div className="bg-zinc-950/90 border-b border-amber-900/60 px-3 py-2 flex flex-wrap items-center justify-center sm:justify-start gap-1 sm:gap-2 text-xs font-sans">
          <button
            onClick={() => {
              setActiveTab('equipment');
              audio.playUiClick();
            }}
            className={`px-3 py-2 rounded-lg font-bold flex items-center gap-1.5 transition ${
              activeTab === 'equipment'
                ? 'bg-gradient-to-r from-amber-600 to-amber-800 text-slate-950 font-black shadow-lg ring-1 ring-amber-300'
                : 'bg-zinc-900/80 text-amber-200 hover:bg-zinc-800 hover:text-amber-100'
            }`}
          >
            <Sword className="w-4 h-4" />
            <span>Equipment</span>
          </button>

          <button
            onClick={() => {
              setActiveTab('relics');
              audio.playUiClick();
            }}
            className={`px-3 py-2 rounded-lg font-bold flex items-center gap-1.5 transition ${
              activeTab === 'relics'
                ? 'bg-gradient-to-r from-sky-600 to-blue-800 text-slate-950 font-black shadow-lg ring-1 ring-sky-300'
                : 'bg-zinc-900/80 text-amber-200 hover:bg-zinc-800 hover:text-amber-100'
            }`}
          >
            <Sparkles className="w-4 h-4" />
            <span>Relics & Arcana</span>
          </button>

          <button
            onClick={() => {
              setActiveTab('map');
              audio.playUiClick();
            }}
            className={`px-3 py-2 rounded-lg font-bold flex items-center gap-1.5 transition ${
              activeTab === 'map'
                ? 'bg-gradient-to-r from-emerald-600 to-teal-800 text-slate-950 font-black shadow-lg ring-1 ring-emerald-300'
                : 'bg-zinc-900/80 text-amber-200 hover:bg-zinc-800 hover:text-amber-100'
            }`}
          >
            <MapIcon className="w-4 h-4" />
            <span>Castle Map ({completionPercent}%)</span>
          </button>

          <button
            onClick={() => {
              setActiveTab('forge');
              audio.playUiClick();
            }}
            className={`px-3 py-2 rounded-lg font-bold flex items-center gap-1.5 transition ${
              activeTab === 'forge'
                ? 'bg-gradient-to-r from-orange-600 to-red-800 text-slate-950 font-black shadow-lg ring-1 ring-orange-300'
                : 'bg-zinc-900/80 text-amber-200 hover:bg-zinc-800 hover:text-amber-100'
            }`}
          >
            <Hammer className="w-4 h-4" />
            <span>Soul Forge</span>
          </button>

          <button
            onClick={() => {
              setActiveTab('settings');
              audio.playUiClick();
            }}
            className={`px-3 py-2 rounded-lg font-bold flex items-center gap-1.5 transition ${
              activeTab === 'settings'
                ? 'bg-gradient-to-r from-zinc-600 to-zinc-800 text-white font-black shadow-lg ring-1 ring-zinc-300'
                : 'bg-zinc-900/80 text-amber-200 hover:bg-zinc-800 hover:text-amber-100'
            }`}
          >
            <SettingsIcon className="w-4 h-4" />
            <span>System & Gamepad</span>
          </button>
        </div>

        {/* TAB BODY CONTENTS */}
        <div className="flex-1 overflow-y-auto p-4 sm:p-6 bg-slate-950/80 font-sans">
          {/* TAB 1: EQUIPMENT & PAPER DOLL STATS */}
          {activeTab === 'equipment' && (
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
              {/* Paper Doll Profile & Stats */}
              <div className="bg-zinc-900/90 border border-amber-700/50 rounded-xl p-4 flex flex-col gap-4 shadow-xl">
                <div className="flex items-center gap-3 border-b border-amber-900/60 pb-3">
                  <div className="w-14 h-14 rounded-xl bg-gradient-to-br from-amber-950 to-zinc-900 border-2 border-amber-400 flex flex-col items-center justify-center font-bold shadow-lg">
                    <span className="text-[9px] text-amber-400 uppercase tracking-widest">LVL</span>
                    <span className="text-xl text-amber-100 font-mono leading-none">{level}</span>
                  </div>
                  <div className="flex flex-col">
                    <span className="font-black text-amber-100 text-base font-cinzel">Shadow Alucard</span>
                    <span className="text-xs text-amber-400/80">Belmont Vampire Hunter</span>
                  </div>
                </div>

                {/* SOTN Character Stats Breakdown */}
                <div className="flex flex-col gap-2 text-xs">
                  <div className="flex justify-between border-b border-zinc-800 pb-1">
                    <span className="text-zinc-400 font-medium">Health (HP):</span>
                    <strong className="text-red-400 font-mono">{hp} / {maxHp}</strong>
                  </div>
                  <div className="flex justify-between border-b border-zinc-800 pb-1">
                    <span className="text-zinc-400 font-medium">Mana (MP):</span>
                    <strong className="text-sky-400 font-mono">{mp} / {maxMp}</strong>
                  </div>
                  <div className="flex justify-between border-b border-zinc-800 pb-1">
                    <span className="text-zinc-400 font-medium">Total Attack Power:</span>
                    <strong className="text-amber-300 font-mono font-bold text-sm">+{totalAtk}</strong>
                  </div>
                  <div className="flex justify-between border-b border-zinc-800 pb-1">
                    <span className="text-zinc-400 font-medium">Total Armor Defense:</span>
                    <strong className="text-blue-300 font-mono font-bold text-sm">+{totalDef}</strong>
                  </div>
                  <div className="flex justify-between border-b border-zinc-800 pb-1">
                    <span className="text-zinc-400 font-medium">Critical Strike Rate:</span>
                    <strong className="text-red-300 font-mono">{(totalCrit * 100).toFixed(0)}%</strong>
                  </div>
                  <div className="flex justify-between border-b border-zinc-800 pb-1">
                    <span className="text-zinc-400 font-medium">Strength (STR):</span>
                    <strong className="text-amber-100 font-mono">{str}</strong>
                  </div>
                  <div className="flex justify-between border-b border-zinc-800 pb-1">
                    <span className="text-zinc-400 font-medium">Constitution (CON):</span>
                    <strong className="text-amber-100 font-mono">{con}</strong>
                  </div>
                  <div className="flex justify-between border-b border-zinc-800 pb-1">
                    <span className="text-zinc-400 font-medium">Intelligence (INT):</span>
                    <strong className="text-amber-100 font-mono">{int}</strong>
                  </div>
                  <div className="flex justify-between border-b border-zinc-800 pb-1">
                    <span className="text-zinc-400 font-medium">Luck (LCK):</span>
                    <strong className="text-amber-100 font-mono">{lck}</strong>
                  </div>
                </div>
              </div>

              {/* Equipment Slots Column */}
              <div className="flex flex-col gap-2.5">
                <h3 className="text-xs font-bold text-amber-400 uppercase tracking-widest">Active Equipment Slots</h3>
                {(Object.keys(slotLabels) as EquipmentSlotType[]).map((slot) => {
                  const item = engine.equipped[slot];
                  const isSelected = selectedSlot === slot;

                  return (
                    <button
                      key={slot}
                      onClick={() => {
                        setSelectedSlot(slot);
                        audio.playUiClick();
                      }}
                      className={`p-3 rounded-xl border text-left flex items-center justify-between transition ${
                        isSelected
                          ? 'bg-amber-950/80 border-amber-400 ring-2 ring-amber-400/50 shadow-lg'
                          : 'bg-zinc-900/80 border-amber-900/40 hover:border-amber-600'
                      }`}
                    >
                      <div className="flex flex-col">
                        <span className="text-[10px] text-amber-400 font-semibold uppercase">{slotLabels[slot]}</span>
                        <span className="text-xs font-bold text-amber-100">{item ? item.name : 'Unequipped'}</span>
                      </div>
                      {item && (
                        <span className="text-[10px] bg-amber-900/80 border border-amber-600/50 text-amber-200 px-2 py-0.5 rounded font-mono font-bold">
                          +{item.stats.attack || item.stats.defense || 0}
                        </span>
                      )}
                    </button>
                  );
                })}
              </div>

              {/* Available Gear Inventory */}
              <div className="flex flex-col gap-3">
                <h3 className="text-xs font-bold text-amber-400 uppercase tracking-widest">
                  Available {slotLabels[selectedSlot]}s
                </h3>
                <div className="flex flex-col gap-2.5 max-h-96 overflow-y-auto pr-1">
                  {engine.inventory.filter((i) => i.slot === selectedSlot).length === 0 ? (
                    <div className="p-4 bg-zinc-900/50 border border-zinc-800 rounded-xl text-center text-xs text-zinc-500">
                      No weapons/gear collected for this slot yet. Explore Dracula's castle to find relics & treasure chests!
                    </div>
                  ) : (
                    engine.inventory
                      .filter((i) => i.slot === selectedSlot)
                      .map((item) => {
                        const isEquipped = engine.equipped[item.slot]?.id === item.id;

                        return (
                          <div
                            key={item.id}
                            onClick={() => handleEquip(item)}
                            className={`p-3 rounded-xl border cursor-pointer flex items-center justify-between transition ${
                              isEquipped
                                ? 'bg-amber-900/60 border-amber-400 ring-1 ring-amber-400'
                                : 'bg-zinc-900/90 border-zinc-800 hover:border-amber-600'
                            }`}
                          >
                            <div className="flex flex-col">
                              <span className="text-xs font-bold text-white flex items-center gap-1.5">
                                {item.name}
                                {isEquipped && <Check className="w-3.5 h-3.5 text-amber-400" />}
                              </span>
                              <span className="text-[10px] text-zinc-400">{item.description}</span>
                            </div>
                            <button className="text-[10px] bg-amber-500 text-black font-bold px-2.5 py-1 rounded-md hover:bg-amber-400 transition">
                              {isEquipped ? 'Equipped' : 'Equip'}
                            </button>
                          </div>
                        );
                      })
                  )}
                </div>
              </div>
            </div>
          )}

          {/* TAB 2: RELICS & ABILITIES (SOTN RELIC SYSTEM) */}
          {activeTab === 'relics' && (
            <div className="flex flex-col gap-6">
              <div className="flex justify-between items-center bg-zinc-900/80 border border-sky-900/60 p-4 rounded-xl">
                <div>
                  <h2 className="text-sm font-extrabold text-sky-200 uppercase tracking-widest font-cinzel">
                    Vampire Hunter Relic Artifacts
                  </h2>
                  <p className="text-xs text-zinc-400">
                    Symphony of the Night style passive ability relics. Toggle ON or OFF to customize player movement & combat arcana.
                  </p>
                </div>
                <span className="text-xs bg-amber-950 border border-amber-600/60 px-3.5 py-1.5 rounded-lg text-amber-300 font-bold font-mono">
                  Soul Shards: {soulShards}
                </span>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {[
                  {
                    key: 'double_jump',
                    name: 'Leap Stone (Spectral Wings)',
                    desc: 'Grants secondary jump mid-air on spirit energy waves.',
                    type: 'Movement Relic'
                  },
                  {
                    key: 'dash_slide',
                    name: 'Form of Mist (Flash Shift)',
                    desc: 'Phase dash providing complete invulnerability through attacks & hazards.',
                    type: 'Phase Relic'
                  },
                  {
                    key: 'holy_flame',
                    name: 'Holy Flame Arcana',
                    desc: 'Unleashes sacred purifying fire consuming dark demons.',
                    type: 'Combat Arcana'
                  },
                  {
                    key: 'parry_counter',
                    name: 'Guarded Parry Counter',
                    desc: 'Deflects incoming enemy strikes triggering automatic counter-attack.',
                    type: 'Combat Relic'
                  },
                  {
                    key: 'soul_steal',
                    name: 'Soul Steal Siphon',
                    desc: 'Drains life energy from slain foes restoring player HP.',
                    type: 'Vampiric Relic'
                  },
                  {
                    key: 'gravity_boots',
                    name: 'Gravity Boots High Jump',
                    desc: 'Supercharges jump velocity to launch skyward into high towers.',
                    type: 'Movement Relic'
                  }
                ].map((relic) => {
                  const isUnlocked = engine.abilities[relic.key];

                  return (
                    <div
                      key={relic.key}
                      className={`p-4 rounded-xl border flex flex-col justify-between gap-3 transition ${
                        isUnlocked
                          ? 'bg-gradient-to-br from-sky-950/80 to-zinc-900 border-sky-500 shadow-lg'
                          : 'bg-zinc-900/40 border-zinc-800 opacity-60'
                      }`}
                    >
                      <div className="flex flex-col gap-1">
                        <div className="flex justify-between items-start">
                          <span className="font-extrabold text-sm text-white">{relic.name}</span>
                          <span className="text-[10px] font-bold text-sky-400 uppercase tracking-wider">{relic.type}</span>
                        </div>
                        <p className="text-xs text-zinc-300">{relic.desc}</p>
                      </div>

                      <div className="flex justify-between items-center border-t border-zinc-800/80 pt-2">
                        <span className="text-xs font-mono font-bold text-sky-300">
                          {isUnlocked ? 'RELIC ACTIVE' : 'LOCKED ARTIFACT'}
                        </span>
                        <button
                          onClick={() => toggleRelic(relic.key)}
                          className={`px-3 py-1.5 rounded-lg text-xs font-bold uppercase tracking-wider transition ${
                            isUnlocked
                              ? 'bg-emerald-500 text-black shadow-md hover:bg-emerald-400'
                              : 'bg-zinc-800 text-zinc-400 hover:bg-zinc-700'
                          }`}
                        >
                          {isUnlocked ? 'ON' : 'OFF'}
                        </button>
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>
          )}

          {/* TAB 3: CASTLE MAP (SOTN 200%+ GRID MATRIX) */}
          {activeTab === 'map' && (
            <div className="flex flex-col gap-4">
              <div className="flex flex-wrap justify-between items-center bg-zinc-900/90 border border-amber-700/60 p-4 rounded-xl gap-2">
                <div>
                  <h2 className="text-sm font-extrabold text-amber-200 uppercase tracking-widest font-cinzel">
                    Dracula's Castle Matrix — Exploration Record
                  </h2>
                  <p className="text-xs text-amber-400/80">
                    Authentic Symphony of the Night interconnected grid. Current Player Chamber indicated by pulsating compass beacon.
                  </p>
                </div>
                <div className="flex items-center gap-3">
                  <span className="text-xs bg-amber-950 border border-amber-500/80 px-3.5 py-1.5 rounded-lg text-emerald-400 font-bold font-mono text-sm shadow">
                    Completion: {completionPercent}% ({exploredCount}/{totalRooms} Rooms)
                  </span>
                </div>
              </div>

              {/* Biome Filter Toolbar */}
              <div className="flex flex-wrap gap-2 text-xs">
                <button
                  onClick={() => setSelectedZoneFilter('all')}
                  className={`px-3 py-1 rounded-md font-bold transition ${
                    selectedZoneFilter === 'all' ? 'bg-amber-500 text-black' : 'bg-zinc-900 text-amber-200'
                  }`}
                >
                  All Castle Biomes
                </button>
                {engine.zones.map((zone) => (
                  <button
                    key={zone.id}
                    onClick={() => setSelectedZoneFilter(zone.id)}
                    className={`px-3 py-1 rounded-md font-bold transition flex items-center gap-1.5 ${
                      selectedZoneFilter === zone.id ? 'ring-2 ring-amber-400 text-white' : 'bg-zinc-900 text-zinc-300'
                    }`}
                    style={{ backgroundColor: selectedZoneFilter === zone.id ? zone.themeColor : undefined }}
                  >
                    <span className="w-2 h-2 rounded-full bg-white" />
                    {zone.name.split('&')[0]}
                  </button>
                ))}
              </div>

              {/* Map Grid Container */}
              <div className="p-4 bg-zinc-950 border border-amber-900/60 rounded-xl overflow-auto max-h-[55vh]">
                <div className="grid grid-cols-4 sm:grid-cols-6 md:grid-cols-8 lg:grid-cols-12 gap-2.5">
                  {engine.zones.map((zone) => {
                    if (selectedZoneFilter !== 'all' && selectedZoneFilter !== zone.id) return null;

                    return zone.rooms.map((room) => {
                      const isCurrent = room.id === currentRoom?.id;
                      const isExplored = room.explored;

                      return (
                        <div
                          key={room.id}
                          className={`h-20 rounded-lg border-2 p-1.5 flex flex-col justify-between transition-all relative overflow-hidden ${
                            isCurrent
                              ? 'bg-amber-900/90 border-amber-400 ring-2 ring-amber-400 shadow-[0_0_20px_rgba(251,191,36,0.6)] animate-pulse'
                              : isExplored
                              ? 'bg-zinc-900 border-amber-800/80 hover:border-amber-400'
                              : 'bg-zinc-950 border-zinc-800 opacity-30'
                          }`}
                          style={{ borderColor: isCurrent ? '#f59e0b' : isExplored ? zone.themeColor : undefined }}
                        >
                          <div className="flex justify-between items-start text-[9px] font-bold">
                            <span className="text-amber-200 truncate">{isExplored ? zone.name.split(' ')[0] : '???'}</span>
                            {isCurrent && <Compass className="w-3.5 h-3.5 text-amber-300 animate-spin" />}
                          </div>

                          <div className="flex items-center gap-1 my-0.5">
                            {room.saveStatue && <Shield className="w-3 h-3 text-sky-400" title="Sanctuary Save Altar" />}
                            {room.bossTrigger && <Crown className="w-3 h-3 text-red-500 animate-bounce" title="Boss Lair" />}
                            {room.teleporter && <Zap className="w-3 h-3 text-purple-400" title="Castle Portal" />}
                            {room.chests && room.chests.length > 0 && <Sparkles className="w-3 h-3 text-emerald-400" title="Relic Chest" />}
                          </div>

                          <div className="flex justify-between text-[8px] font-mono text-zinc-400">
                            <span>[{room.gridX},{room.gridY}]</span>
                            <span>{room.doors.length} Doors</span>
                          </div>
                        </div>
                      );
                    });
                  })}
                </div>
              </div>

              {/* Map Legend */}
              <div className="flex flex-wrap justify-around items-center gap-3 bg-zinc-900/80 border border-amber-900/40 p-3 rounded-xl text-xs text-zinc-300">
                <div className="flex items-center gap-1.5"><Compass className="w-4 h-4 text-amber-400" /> Active Player Location</div>
                <div className="flex items-center gap-1.5"><Shield className="w-4 h-4 text-sky-400" /> Save Sanctuary Altar</div>
                <div className="flex items-center gap-1.5"><Crown className="w-4 h-4 text-red-500" /> Boss Arena Lair</div>
                <div className="flex items-center gap-1.5"><Zap className="w-4 h-4 text-purple-400" /> Teleport Portal Gate</div>
                <div className="flex items-center gap-1.5"><Sparkles className="w-4 h-4 text-emerald-400" /> Relic Chest</div>
              </div>
            </div>
          )}

          {/* TAB 4: SOUL FORGE */}
          {activeTab === 'forge' && (
            <div className="flex flex-col gap-6">
              <div className="flex justify-between items-center bg-zinc-900/80 border border-orange-900/60 p-4 rounded-xl">
                <div>
                  <h2 className="text-sm font-extrabold text-orange-300 uppercase tracking-widest font-cinzel">
                    Gothic Soul Forge & Alchemy
                  </h2>
                  <p className="text-xs text-zinc-400">
                    Transmute Dark Ore & Soul Shards into mythic vampire-slaying armaments.
                  </p>
                </div>
                <div className="flex items-center gap-4 text-xs font-mono">
                  <span className="bg-zinc-900 border border-sky-500/50 px-3 py-1 rounded text-sky-300 font-bold">
                    Shards: {soulShards}
                  </span>
                  <span className="bg-zinc-900 border border-amber-500/50 px-3 py-1 rounded text-amber-300 font-bold">
                    Dark Ore: {darkOre}
                  </span>
                </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {[
                  {
                    name: "Dracula's Bane Obsidian Scythe",
                    desc: "Mythic heavy scythe with dark energy arc waves.",
                    shards: 35,
                    ore: 10,
                    atk: 85
                  },
                  {
                    name: "Executioner Obsidian Plate Armor",
                    desc: "Thick gothic steel granting heavy damage mitigation.",
                    shards: 40,
                    ore: 12,
                    def: 35
                  }
                ].map((item, idx) => {
                  const canForge = soulShards >= item.shards && darkOre >= item.ore;

                  return (
                    <div
                      key={idx}
                      className="p-4 rounded-xl bg-zinc-900/80 border border-amber-900/50 flex flex-col justify-between gap-3"
                    >
                      <div className="flex flex-col gap-1">
                        <span className="font-extrabold text-sm text-amber-200">{item.name}</span>
                        <p className="text-xs text-zinc-300">{item.desc}</p>
                      </div>

                      <div className="flex justify-between items-center border-t border-zinc-800 pt-2">
                        <span className="text-xs font-mono text-zinc-400">
                          Req: {item.shards} Shards | {item.ore} Ore
                        </span>
                        <button
                          disabled={!canForge}
                          className={`px-3 py-1.5 rounded-md text-xs font-bold transition ${
                            canForge
                              ? 'bg-amber-500 text-black hover:bg-amber-400 shadow-md'
                              : 'bg-zinc-800 text-zinc-500 cursor-not-allowed'
                          }`}
                        >
                          Forge Weapon
                        </button>
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>
          )}

          {/* TAB 5: SYSTEM & GAMEPAD CONTROLS */}
          {activeTab === 'settings' && (
            <div className="flex flex-col gap-6">
              {/* Gamepad Status Banner */}
              <div className="bg-gradient-to-r from-zinc-900 via-amber-950/60 to-zinc-900 border border-amber-500/60 p-4 rounded-xl flex items-center justify-between shadow-lg">
                <div className="flex items-center gap-3">
                  <Gamepad2 className="w-8 h-8 text-amber-400 animate-pulse" />
                  <div>
                    <h3 className="text-sm font-extrabold text-amber-200 font-cinzel">
                      {engine.gamepadConnected ? `🎮 Controller Active: ${engine.gamepadName}` : '🎮 Standard USB / Bluetooth Gamepad Support'}
                    </h3>
                    <p className="text-xs text-amber-400/80">
                      Xbox, PlayStation DualSense, Nintendo Switch Pro & generic USB gamepads supported natively.
                    </p>
                  </div>
                </div>
                <span
                  className={`px-3 py-1 rounded-md text-xs font-bold font-mono ${
                    engine.gamepadConnected ? 'bg-emerald-950 text-emerald-400 border border-emerald-500' : 'bg-zinc-800 text-zinc-400'
                  }`}
                >
                  {engine.gamepadConnected ? 'CONNECTED' : 'DISCONNECTED'}
                </span>
              </div>

              {/* Gamepad Layout Diagram */}
              <div className="bg-zinc-900/90 border border-amber-900/50 p-4 rounded-xl flex flex-col gap-3">
                <h4 className="text-xs font-bold text-amber-400 uppercase tracking-widest font-cinzel">
                  Gamepad & Keyboard Controller Mapping
                </h4>
                <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-3 text-xs">
                  <div className="bg-zinc-950 border border-zinc-800 p-2.5 rounded-lg flex flex-col">
                    <span className="text-amber-400 font-bold">Left Analog / D-Pad</span>
                    <span className="text-zinc-300">Player Movement (Left / Right / Crouch)</span>
                  </div>
                  <div className="bg-zinc-950 border border-zinc-800 p-2.5 rounded-lg flex flex-col">
                    <span className="text-emerald-400 font-bold">Button A / Cross</span>
                    <span className="text-zinc-300">Jump / Double Jump (Spacebar)</span>
                  </div>
                  <div className="bg-zinc-950 border border-zinc-800 p-2.5 rounded-lg flex flex-col">
                    <span className="text-sky-400 font-bold">Button X / Square</span>
                    <span className="text-zinc-300">Weapon Swing Attack (J / Enter)</span>
                  </div>
                  <div className="bg-zinc-950 border border-zinc-800 p-2.5 rounded-lg flex flex-col">
                    <span className="text-purple-400 font-bold">Button Y / Triangle / LB</span>
                    <span className="text-zinc-300">Cast Holy Flame Spell (U / E)</span>
                  </div>
                  <div className="bg-zinc-950 border border-zinc-800 p-2.5 rounded-lg flex flex-col">
                    <span className="text-red-400 font-bold">Button B / Circle / RB</span>
                    <span className="text-zinc-300">Flash Shift Phase Dash (Shift / K)</span>
                  </div>
                  <div className="bg-zinc-950 border border-zinc-800 p-2.5 rounded-lg flex flex-col">
                    <span className="text-amber-300 font-bold">Start / Options</span>
                    <span className="text-zinc-300">Toggle SOTN Pause Menu</span>
                  </div>
                </div>
              </div>

              {/* Audio Controls */}
              <div className="bg-zinc-900/90 border border-amber-900/50 p-4 rounded-xl flex flex-col gap-4">
                <div className="flex justify-between items-center">
                  <h3 className="text-xs font-bold text-amber-400 uppercase tracking-widest flex items-center gap-2">
                    <Volume2 className="w-4 h-4" /> Gothic Audio Synthesizer
                  </h3>
                  <button
                    onClick={() => {
                      const muted = audio.toggleMute();
                      setIsMuted(muted);
                    }}
                    className="text-xs bg-zinc-800 hover:bg-zinc-700 text-amber-300 px-3 py-1 rounded border border-amber-700/50 flex items-center gap-1.5 transition"
                  >
                    {isMuted ? <VolumeX className="w-3.5 h-3.5 text-red-400" /> : <Volume2 className="w-3.5 h-3.5 text-emerald-400" />}
                    {isMuted ? 'Unmute Sound' : 'Mute Sound'}
                  </button>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 text-xs">
                  <div className="flex flex-col gap-1">
                    <div className="flex justify-between text-zinc-300">
                      <span>Master Volume</span>
                      <span>{Math.floor(masterVol * 100)}%</span>
                    </div>
                    <input
                      type="range"
                      min="0"
                      max="1"
                      step="0.05"
                      value={masterVol}
                      onChange={(e) => handleMasterChange(parseFloat(e.target.value))}
                      className="accent-amber-500 cursor-pointer"
                    />
                  </div>

                  <div className="flex flex-col gap-1">
                    <div className="flex justify-between text-zinc-300">
                      <span>Orchestral BGM</span>
                      <span>{Math.floor(musicVol * 100)}%</span>
                    </div>
                    <input
                      type="range"
                      min="0"
                      max="1"
                      step="0.05"
                      value={musicVol}
                      onChange={(e) => handleMusicChange(parseFloat(e.target.value))}
                      className="accent-amber-500 cursor-pointer"
                    />
                  </div>

                  <div className="flex flex-col gap-1">
                    <div className="flex justify-between text-zinc-300">
                      <span>Sound Effects</span>
                      <span>{Math.floor(sfxVol * 100)}%</span>
                    </div>
                    <input
                      type="range"
                      min="0"
                      max="1"
                      step="0.05"
                      value={sfxVol}
                      onChange={(e) => handleSfxChange(parseFloat(e.target.value))}
                      className="accent-amber-500 cursor-pointer"
                    />
                  </div>
                </div>
              </div>

              {/* Touch Controls Toggle */}
              <div className="bg-zinc-900/90 border border-amber-900/50 p-4 rounded-xl flex items-center justify-between">
                <div>
                  <span className="text-xs font-bold text-amber-200">On-Screen Virtual Touch D-Pad</span>
                  <p className="text-[10px] text-zinc-400">Display virtual touchscreen buttons for mobile & tablet gameplay.</p>
                </div>
                <button
                  onClick={() => {
                    setShowTouchControls(!showTouchControls);
                    audio.playUiClick();
                  }}
                  className={`px-4 py-2 rounded-lg text-xs font-bold transition ${
                    showTouchControls ? 'bg-emerald-600 text-white' : 'bg-zinc-800 text-zinc-400'
                  }`}
                >
                  {showTouchControls ? 'ENABLED' : 'DISABLED'}
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
