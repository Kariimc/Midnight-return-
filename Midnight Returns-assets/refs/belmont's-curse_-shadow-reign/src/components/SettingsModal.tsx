/**
 * Belmont's Curse: Shadow Reign - Settings & Options Modal
 * Controls master volume, music volume, sfx volume, camera shake, and visual retro options.
 */

import React, { useState } from 'react';
import { Settings as SettingsIcon, X, Volume2, VolumeX, Monitor, ShieldAlert } from 'lucide-react';
import { audio } from '../utils/audio';

interface SettingsModalProps {
  onClose: () => void;
  showTouchControls: boolean;
  setShowTouchControls: (val: boolean) => void;
}

export const SettingsModal: React.FC<SettingsModalProps> = ({
  onClose,
  showTouchControls,
  setShowTouchControls
}) => {
  const [masterVol, setMasterVol] = useState(0.8);
  const [musicVol, setMusicVol] = useState(0.4);
  const [sfxVol, setSfxVol] = useState(0.7);
  const [isMuted, setIsMuted] = useState(false);

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

  const handleToggleMute = () => {
    const muted = audio.toggleMute();
    setIsMuted(muted);
  };

  return (
    <div className="fixed inset-0 z-50 bg-black/80 backdrop-blur-md flex items-center justify-center p-4 select-none">
      <div className="bg-zinc-950 border-2 border-amber-800/80 rounded-xl w-full max-w-xl max-h-[90vh] flex flex-col overflow-hidden shadow-2xl text-amber-100">
        {/* Header */}
        <div className="flex justify-between items-center p-4 border-b border-amber-900/50 bg-zinc-900/90">
          <div className="flex items-center gap-2">
            <SettingsIcon className="w-5 h-5 text-amber-400" />
            <h2 className="text-lg font-bold uppercase tracking-wider text-amber-200">Audio & Display Options</h2>
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

        {/* Content */}
        <div className="flex flex-col gap-6 p-6 overflow-y-auto">
          {/* Audio Controls */}
          <div className="flex flex-col gap-4 bg-zinc-900/80 border border-amber-900/40 rounded-lg p-4">
            <div className="flex justify-between items-center">
              <h3 className="text-xs font-bold text-amber-400 uppercase tracking-wider flex items-center gap-2">
                <Volume2 className="w-4 h-4" /> Sound Synthesizer
              </h3>
              <button
                onClick={handleToggleMute}
                className="text-xs bg-zinc-800 hover:bg-zinc-700 text-amber-300 px-3 py-1 rounded border border-amber-700/50 flex items-center gap-1.5 transition"
              >
                {isMuted ? <VolumeX className="w-3.5 h-3.5 text-red-400" /> : <Volume2 className="w-3.5 h-3.5 text-emerald-400" />}
                {isMuted ? 'Unmute' : 'Mute Sound'}
              </button>
            </div>

            <div className="flex flex-col gap-3 text-xs">
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
                  <span>Gothic Orchestra BGM</span>
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
                  <span>Sound Effects & Combat Hits</span>
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

          {/* Touch / Mobile Overlay Toggle */}
          <div className="flex items-center justify-between bg-zinc-900/80 border border-amber-900/40 rounded-lg p-4">
            <div className="flex flex-col gap-0.5">
              <span className="text-xs font-bold text-amber-200">On-Screen Touch Controls</span>
              <span className="text-[10px] text-zinc-400">Display virtual D-Pad and touch buttons for mobile/tablet play.</span>
            </div>
            <button
              onClick={() => {
                setShowTouchControls(!showTouchControls);
                audio.playUiClick();
              }}
              className={`px-4 py-2 rounded text-xs font-bold transition ${
                showTouchControls ? 'bg-emerald-600 text-white' : 'bg-zinc-800 text-zinc-400'
              }`}
            >
              {showTouchControls ? 'ENABLED' : 'DISABLED'}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
