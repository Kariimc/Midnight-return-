/**
 * Belmont's Curse: Shadow Reign - Hollow Knight & Castlevania SOTN Interconnected World Map Modal
 * Interactive visual canvas renderer showing interconnected room chambers, biomes, secret passages,
 * save altars, teleporters, boss arenas, and exploration metrics.
 */

import React, { useState } from 'react';
import { GameEngine } from '../engine/gameEngine';
import { Map, X, Compass, Shield, Crown, Zap, Sparkles, Filter } from 'lucide-react';
import { audio } from '../utils/audio';

interface MapModalProps {
  engine: GameEngine;
  onClose: () => void;
}

export const MapModal: React.FC<MapModalProps> = ({ engine, onClose }) => {
  const currentRoom = engine.getCurrentRoom();
  const [selectedZoneFilter, setSelectedZoneFilter] = useState<string | 'all'>('all');

  // Calculate Map Completion Statistics
  let totalRooms = 0;
  let exploredCount = 0;

  engine.zones.forEach((zone) => {
    zone.rooms.forEach((room) => {
      totalRooms++;
      if (room.explored) exploredCount++;
    });
  });

  const completionPercent = Math.min(100, Math.floor((exploredCount / totalRooms) * 100));

  return (
    <div className="fixed inset-0 z-50 bg-black/90 backdrop-blur-md flex items-center justify-center p-4 select-none">
      <div className="bg-zinc-950 border-2 border-amber-800/80 rounded-xl w-full max-w-5xl h-[88vh] flex flex-col overflow-hidden shadow-2xl text-amber-100">
        {/* Header */}
        <div className="flex flex-wrap justify-between items-center p-4 border-b border-amber-900/50 bg-zinc-900/90 gap-2">
          <div className="flex items-center gap-3">
            <Map className="w-6 h-6 text-emerald-400" />
            <div>
              <h2 className="text-xl font-bold uppercase tracking-widest text-amber-200">
                Dracula's Realm — Interconnected Metroidvania World Map
              </h2>
              <p className="text-xs text-amber-400/80">
                Symphony of the Night & Hollow Knight Style Sprawling Castle Matrix
              </p>
            </div>
          </div>

          <div className="flex items-center gap-4">
            <span className="text-xs text-amber-300 font-semibold bg-zinc-900 border border-amber-800/60 px-3.5 py-1.5 rounded-md shadow-inner">
              Castle Completion: <strong className="text-emerald-400 font-mono text-sm">{completionPercent}%</strong> ({exploredCount}/{totalRooms} Rooms)
            </span>
            <button
              onClick={() => {
                audio.playUiClick();
                onClose();
              }}
              className="p-1.5 rounded-lg hover:bg-zinc-800 text-amber-400 hover:text-white transition"
            >
              <X className="w-6 h-6" />
            </button>
          </div>
        </div>

        {/* Zone Filter Toolbar */}
        <div className="px-6 py-2.5 bg-zinc-900/60 border-b border-amber-900/30 flex flex-wrap items-center justify-between gap-2 text-xs">
          <div className="flex items-center gap-2 text-amber-400 font-medium">
            <Filter className="w-4 h-4 text-amber-400" />
            <span>Biome Filter:</span>
          </div>
          <div className="flex flex-wrap gap-1.5">
            <button
              onClick={() => {
                audio.playUiClick();
                setSelectedZoneFilter('all');
              }}
              className={`px-3 py-1 rounded text-[11px] font-semibold transition ${
                selectedZoneFilter === 'all'
                  ? 'bg-amber-600 text-black font-bold shadow'
                  : 'bg-zinc-800 text-amber-200 hover:bg-zinc-700'
              }`}
            >
              All Castle Biomes
            </button>
            {engine.zones.map((zone) => (
              <button
                key={zone.id}
                onClick={() => {
                  audio.playUiClick();
                  setSelectedZoneFilter(zone.id);
                }}
                className={`px-2.5 py-1 rounded text-[11px] font-semibold transition flex items-center gap-1.5 ${
                  selectedZoneFilter === zone.id
                    ? 'ring-1 ring-amber-400 text-white font-bold'
                    : 'bg-zinc-900/80 text-zinc-300 hover:bg-zinc-800'
                }`}
                style={{
                  backgroundColor: selectedZoneFilter === zone.id ? zone.themeColor : undefined,
                  color: selectedZoneFilter === zone.id ? '#000000' : undefined
                }}
              >
                <span className="w-2 h-2 rounded-full" style={{ backgroundColor: zone.themeColor }} />
                {zone.name.split('&')[0]}
              </button>
            ))}
          </div>
        </div>

        {/* Map Grid Matrix Container */}
        <div className="flex-1 p-6 overflow-auto bg-zinc-950 flex items-center justify-center relative">
          <div className="w-full max-w-4xl grid grid-cols-4 sm:grid-cols-6 md:grid-cols-8 lg:grid-cols-12 gap-3 p-6 bg-zinc-900/90 border border-amber-900/50 rounded-xl relative shadow-2xl">
            {engine.zones.map((zone) => {
              if (selectedZoneFilter !== 'all' && selectedZoneFilter !== zone.id) return null;

              return zone.rooms.map((room) => {
                const isCurrent = room.id === currentRoom?.id;
                const isExplored = room.explored;

                return (
                  <div
                    key={room.id}
                    className={`h-24 rounded-lg border-2 p-2 flex flex-col justify-between transition-all relative overflow-hidden group ${
                      isCurrent
                        ? 'bg-amber-900/80 border-amber-400 ring-2 ring-amber-400 shadow-[0_0_15px_rgba(251,191,36,0.5)] animate-pulse'
                        : isExplored
                        ? 'bg-zinc-900 border-amber-800/80 hover:border-amber-500 hover:shadow-md'
                        : 'bg-zinc-950 border-zinc-800/60 opacity-30'
                    }`}
                    style={{
                      borderColor: isCurrent ? '#f59e0b' : isExplored ? zone.themeColor : undefined
                    }}
                  >
                    {/* Zone Accent Stripe */}
                    <div
                      className="absolute top-0 left-0 right-0 h-1"
                      style={{ backgroundColor: zone.themeColor }}
                    />

                    {/* Room Top Header */}
                    <div className="flex justify-between items-start text-[10px] font-bold mt-1">
                      <span className="text-amber-200 truncate max-w-[80px]" title={zone.name}>
                        {isExplored ? zone.name.split(' ')[0] : '???'}
                      </span>
                      {isCurrent && <Compass className="w-4 h-4 text-amber-400 animate-spin" />}
                    </div>

                    {/* Room Icons (Save, Teleport, Boss, Chest) */}
                    <div className="flex items-center gap-1.5 my-1">
                      {room.saveStatue && (
                        <Shield className="w-3.5 h-3.5 text-sky-400 drop-shadow" title="Sanctuary Save Altar" />
                      )}
                      {room.bossTrigger && (
                        <Crown className="w-3.5 h-3.5 text-red-500 drop-shadow animate-bounce" title="Boss Arena" />
                      )}
                      {room.teleporter && (
                        <Zap className="w-3.5 h-3.5 text-purple-400 drop-shadow" title="Teleport Portal" />
                      )}
                      {room.chests && room.chests.length > 0 && (
                        <Sparkles className="w-3.5 h-3.5 text-emerald-400 drop-shadow" title="Relic Chest" />
                      )}
                    </div>

                    {/* Room Doors / Coordinates indicator */}
                    <div className="flex justify-between items-end text-[9px] text-zinc-400 font-mono">
                      <span>{isExplored ? `[${room.gridX},${room.gridY}]` : '?'}</span>
                      <span className="text-[8px] uppercase tracking-tighter text-amber-400/80">
                        {room.doors.length} Way{room.doors.length > 1 ? 's' : ''}
                      </span>
                    </div>
                  </div>
                );
              });
            })}
          </div>
        </div>

        {/* Map Legend */}
        <div className="flex flex-wrap items-center justify-around gap-4 bg-zinc-900/80 border-t border-amber-900/50 p-4 text-xs text-zinc-300">
          <div className="flex items-center gap-2">
            <Compass className="w-4 h-4 text-amber-400" />
            <span>Active Character Location</span>
          </div>
          <div className="flex items-center gap-2">
            <Shield className="w-4 h-4 text-sky-400" />
            <span>Sanctuary Save Altar</span>
          </div>
          <div className="flex items-center gap-2">
            <Crown className="w-4 h-4 text-red-500" />
            <span>Boss & Sub-Boss Lairs</span>
          </div>
          <div className="flex items-center gap-2">
            <Zap className="w-4 h-4 text-purple-400" />
            <span>Castle Teleport Portal</span>
          </div>
          <div className="flex items-center gap-2">
            <Sparkles className="w-4 h-4 text-emerald-400" />
            <span>Relic Equipment Chest</span>
          </div>
        </div>
      </div>
    </div>
  );
};
