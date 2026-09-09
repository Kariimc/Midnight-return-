/**
 * Belmont's Curse: Shadow Reign - Virtual Touch D-Pad & Controls Overlay
 * Allows seamless gameplay on mobile devices and tablets.
 */

import React from 'react';
import { GameEngine } from '../engine/gameEngine';
import { ArrowLeft, ArrowRight, ArrowUp, Zap, Sword, Shield } from 'lucide-react';

interface ControlsOverlayProps {
  engine: GameEngine;
}

export const ControlsOverlay: React.FC<ControlsOverlayProps> = ({ engine }) => {
  const setKey = (key: string, state: boolean) => {
    engine.keys[key] = state;
  };

  return (
    <div className="absolute inset-0 pointer-events-none flex justify-between items-end p-6 z-30 select-none">
      {/* Left Side: Touch D-Pad */}
      <div className="pointer-events-auto flex flex-col items-center gap-2">
        <button
          onTouchStart={() => setKey('w', true)}
          onTouchEnd={() => setKey('w', false)}
          onMouseDown={() => setKey('w', true)}
          onMouseUp={() => setKey('w', false)}
          className="w-14 h-14 bg-zinc-900/80 border-2 border-amber-700/60 rounded-lg flex items-center justify-center text-amber-300 active:bg-amber-600 active:text-black transition"
        >
          <ArrowUp className="w-6 h-6" />
        </button>

        <div className="flex gap-2">
          <button
            onTouchStart={() => setKey('a', true)}
            onTouchEnd={() => setKey('a', false)}
            onMouseDown={() => setKey('a', true)}
            onMouseUp={() => setKey('a', false)}
            className="w-14 h-14 bg-zinc-900/80 border-2 border-amber-700/60 rounded-lg flex items-center justify-center text-amber-300 active:bg-amber-600 active:text-black transition"
          >
            <ArrowLeft className="w-6 h-6" />
          </button>

          <button
            onTouchStart={() => setKey('d', true)}
            onTouchEnd={() => setKey('d', false)}
            onMouseDown={() => setKey('d', true)}
            onMouseUp={() => setKey('d', false)}
            className="w-14 h-14 bg-zinc-900/80 border-2 border-amber-700/60 rounded-lg flex items-center justify-center text-amber-300 active:bg-amber-600 active:text-black transition"
          >
            <ArrowRight className="w-6 h-6" />
          </button>
        </div>
      </div>

      {/* Right Side: Action Buttons */}
      <div className="pointer-events-auto flex items-center gap-3">
        {/* Dash */}
        <button
          onTouchStart={() => setKey('Shift', true)}
          onTouchEnd={() => setKey('Shift', false)}
          onMouseDown={() => setKey('Shift', true)}
          onMouseUp={() => setKey('Shift', false)}
          className="w-14 h-14 bg-zinc-900/80 border-2 border-sky-600/60 rounded-full flex flex-col items-center justify-center text-sky-300 active:bg-sky-500 active:text-black transition text-[10px] font-bold"
        >
          <Zap className="w-5 h-5" />
          <span>DASH</span>
        </button>

        {/* Attack */}
        <button
          onTouchStart={() => setKey('j', true)}
          onTouchEnd={() => setKey('j', false)}
          onMouseDown={() => setKey('j', true)}
          onMouseUp={() => setKey('j', false)}
          className="w-16 h-16 bg-gradient-to-br from-amber-600 to-amber-800 border-2 border-amber-400 rounded-full flex flex-col items-center justify-center text-amber-100 active:scale-95 transition text-xs font-extrabold shadow-lg"
        >
          <Sword className="w-6 h-6" />
          <span>ATK</span>
        </button>

        {/* Spell */}
        <button
          onTouchStart={() => setKey('u', true)}
          onTouchEnd={() => setKey('u', false)}
          onMouseDown={() => setKey('u', true)}
          onMouseUp={() => setKey('u', false)}
          className="w-14 h-14 bg-zinc-900/80 border-2 border-purple-600/60 rounded-full flex flex-col items-center justify-center text-purple-300 active:bg-purple-600 active:text-white transition text-[10px] font-bold"
        >
          <Shield className="w-5 h-5" />
          <span>SPELL</span>
        </button>
      </div>
    </div>
  );
};
