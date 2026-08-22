/**
 * Belmont's Curse: Shadow Reign - Main React Application Entry
 * Integrates 60FPS Game Canvas, HUD, Start Screen, Inventory, Skill Tree, Map, and Forge Modals.
 */

import React, { useState, useRef, useEffect } from 'react';
import { GameEngine } from './engine/gameEngine';
import { GameCanvas } from './components/GameCanvas';
import { HUD } from './components/HUD';
import { StartScreen } from './components/StartScreen';
import { SotnMenuModal } from './components/SotnMenuModal';
import { ControlsOverlay } from './components/ControlsOverlay';

export default function App() {
  const engineRef = useRef<GameEngine>(new GameEngine());
  const engine = engineRef.current;

  const [gameState, setGameState] = useState<'start' | 'playing'>('start');
  const [activeModal, setActiveModal] = useState<'none' | 'equipment' | 'relics' | 'map' | 'forge' | 'settings'>('none');
  const [showTouchControls, setShowTouchControls] = useState<boolean>(false);

  // Connect engine callback for Gamepad Start/Select menu requests
  useEffect(() => {
    engine.onToggleMenuRequest = (tab) => {
      setActiveModal((prev) => (prev !== 'none' && prev === tab ? 'none' : tab || 'equipment'));
    };
  }, [engine]);

  return (
    <div className="relative w-screen h-screen bg-black overflow-hidden select-none font-sans">
      {gameState === 'start' ? (
        <StartScreen
          onStartGame={() => setGameState('playing')}
          onOpenSettings={() => setActiveModal('settings')}
        />
      ) : (
        <>
          {/* Main 60FPS Game Canvas */}
          <GameCanvas
            engine={engine}
            onOpenInventory={() => setActiveModal('equipment')}
            onOpenSkills={() => setActiveModal('relics')}
            onOpenMap={() => setActiveModal('map')}
          />

          {/* Gothic HUD */}
          <HUD
            engine={engine}
            onOpenInventory={() => setActiveModal('equipment')}
            onOpenSkills={() => setActiveModal('relics')}
            onOpenMap={() => setActiveModal('map')}
            onOpenForge={() => setActiveModal('forge')}
            onOpenSettings={() => setActiveModal('settings')}
          />

          {/* Mobile/Touch Controls Overlay */}
          {showTouchControls && <ControlsOverlay engine={engine} />}
        </>
      )}

      {/* SOTN UNIFIED PAUSE MENU & CASTLE MAP SYSTEM */}
      {activeModal !== 'none' && (
        <SotnMenuModal
          engine={engine}
          initialTab={activeModal}
          onClose={() => setActiveModal('none')}
          showTouchControls={showTouchControls}
          setShowTouchControls={setShowTouchControls}
        />
      )}
    </div>
  );
}
