/**
 * Belmont's Curse: Shadow Reign - Skill & Ability Tree Modal
 * Interactive skill tree for unlocking Double Jump, Flash Shift, Soul Steal, and Gravity Boots.
 */

import React from 'react';
import { GameEngine } from '../engine/gameEngine';
import { Sparkles, X, Check, Lock } from 'lucide-react';
import { audio } from '../utils/audio';

interface SkillTreeModalProps {
  engine: GameEngine;
  onClose: () => void;
}

interface SkillNode {
  id: string;
  name: string;
  description: string;
  cost: number;
  unlocked: boolean;
  type: 'movement' | 'combat' | 'magic';
}

export const SkillTreeModal: React.FC<SkillTreeModalProps> = ({ engine, onClose }) => {
  const { soulShards } = engine.playerStats;

  const skillNodes: SkillNode[] = [
    {
      id: 'double_jump',
      name: 'Spectral Wings (Double Jump)',
      description: 'Perform a secondary jump mid-air on a cushion of spirit energy.',
      cost: 0,
      unlocked: engine.abilities.double_jump,
      type: 'movement'
    },
    {
      id: 'dash_slide',
      name: 'Flash Shift Dash',
      description: 'Rapid phase dash providing invulnerability frames through hazards & attacks.',
      cost: 15,
      unlocked: engine.abilities.dash_slide,
      type: 'movement'
    },
    {
      id: 'parry_counter',
      name: 'Guarded Parry Counter',
      description: 'Block incoming light enemy attacks to trigger a devastating riposte counter.',
      cost: 25,
      unlocked: engine.abilities.parry_counter,
      type: 'combat'
    },
    {
      id: 'holy_flame',
      name: 'Holy Flame Burst',
      description: 'Unleash a wave of sacred flame burning surrounding dark foes.',
      cost: 30,
      unlocked: engine.abilities.holy_flame,
      type: 'magic'
    },
    {
      id: 'soul_steal',
      name: 'Soul Steal Siphon',
      description: 'Drain health from slain enemies to restore player HP in combat.',
      cost: 45,
      unlocked: engine.abilities.soul_steal,
      type: 'magic'
    },
    {
      id: 'gravity_boots',
      name: 'Gravity Boots High Jump',
      description: 'Massively increase jump velocity to scale skyward towers and secret passages.',
      cost: 60,
      unlocked: engine.abilities.gravity_boots,
      type: 'movement'
    }
  ];

  const handleUnlockSkill = (skill: SkillNode) => {
    if (skill.unlocked) return;
    if (soulShards < skill.cost) return;

    engine.playerStats.soulShards -= skill.cost;
    engine.abilities[skill.id] = true;

    audio.playItemPick();
    engine.addFloatingText(`UNLOCKED: ${skill.name}!`, engine.playerPos.x, engine.playerPos.y - 30, '#38bdf8', 1.4);
  };

  return (
    <div className="fixed inset-0 z-50 bg-black/80 backdrop-blur-md flex items-center justify-center p-4 select-none">
      <div className="bg-zinc-950 border-2 border-amber-800/80 rounded-xl w-full max-w-3xl max-h-[90vh] flex flex-col overflow-hidden shadow-2xl text-amber-100">
        {/* Header */}
        <div className="flex justify-between items-center p-4 border-b border-amber-900/50 bg-zinc-900/90">
          <div className="flex items-center gap-2">
            <Sparkles className="w-5 h-5 text-sky-400" />
            <h2 className="text-lg font-bold uppercase tracking-wider text-amber-200">Ability & Relic Tree</h2>
          </div>

          <div className="flex items-center gap-4">
            <span className="text-xs bg-amber-950/80 border border-amber-700/60 px-3 py-1 rounded text-sky-300 font-bold">
              Soul Shards: {soulShards}
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

        {/* Skill Nodes Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 p-6 overflow-y-auto">
          {skillNodes.map((skill) => {
            const canAfford = soulShards >= skill.cost;

            return (
              <div
                key={skill.id}
                className={`p-4 rounded-lg border flex flex-col justify-between gap-3 transition ${
                  skill.unlocked
                    ? 'bg-amber-950/40 border-amber-500'
                    : canAfford
                    ? 'bg-zinc-900/80 border-amber-800/60 hover:border-sky-400'
                    : 'bg-zinc-900/40 border-zinc-800 opacity-60'
                }`}
              >
                <div className="flex flex-col gap-1">
                  <div className="flex justify-between items-start">
                    <span className="font-extrabold text-sm text-white">{skill.name}</span>
                    <span className="text-[10px] font-semibold text-amber-400 uppercase tracking-widest">{skill.type}</span>
                  </div>
                  <p className="text-xs text-zinc-300">{skill.description}</p>
                </div>

                <div className="flex justify-between items-center border-t border-zinc-800 pt-2 mt-1">
                  <span className="text-xs font-mono text-sky-300">
                    {skill.unlocked ? 'ACTIVE' : `Cost: ${skill.cost} Shards`}
                  </span>

                  <button
                    disabled={skill.unlocked || !canAfford}
                    onClick={() => handleUnlockSkill(skill)}
                    className={`px-3 py-1.5 rounded text-xs font-bold uppercase tracking-wider flex items-center gap-1.5 transition ${
                      skill.unlocked
                        ? 'bg-emerald-950 border border-emerald-500 text-emerald-300 cursor-default'
                        : canAfford
                        ? 'bg-sky-500 hover:bg-sky-400 text-zinc-950 shadow-md'
                        : 'bg-zinc-800 text-zinc-500 cursor-not-allowed'
                    }`}
                  >
                    {skill.unlocked ? (
                      <>
                        <Check className="w-3.5 h-3.5" />
                        Unlocked
                      </>
                    ) : (
                      <>
                        <Lock className="w-3.5 h-3.5" />
                        Unlock
                      </>
                    )}
                  </button>
                </div>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
};
