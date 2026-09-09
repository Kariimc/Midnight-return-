/**
 * Belmont's Curse: Shadow Reign - Main Canvas Game Renderer Component
 * Runs 60FPS requestAnimationFrame loop rendering WebGL 3D PBR environment,
 * player with real-time gear, enemies, particle effects, lighting mask layer, and floating texts.
 */

import React, { useEffect, useRef } from 'react';
import { GameEngine } from '../engine/gameEngine';
import { LightingEngine } from '../engine/lighting';
import { ProceduralRenderer, EnvironmentRenderer } from '../utils/proceduralSprites';
import { WebGL3DRenderer } from '../engine/webglEngine';

interface GameCanvasProps {
  engine: GameEngine;
  onOpenInventory: () => void;
  onOpenSkills: () => void;
  onOpenMap: () => void;
}

export const GameCanvas: React.FC<GameCanvasProps> = ({
  engine,
  onOpenInventory,
  onOpenSkills,
  onOpenMap
}) => {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const webglContainerRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    const canvas = canvasRef.current;
    const webglContainer = webglContainerRef.current;
    if (!canvas || !webglContainer) return;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    // Initialize Metroid Dread 3D WebGL Engine
    const webglRenderer = new WebGL3DRenderer(webglContainer);

    let animId: number;
    let lastTime = performance.now();

    // Resize Handler
    const handleResize = () => {
      canvas.width = window.innerWidth;
      canvas.height = window.innerHeight;
      engine.screenWidth = canvas.width;
      engine.screenHeight = canvas.height;
    };

    handleResize();
    window.addEventListener('resize', handleResize);

    // Keyboard Event Listeners
    const handleKeyDown = (e: KeyboardEvent) => {
      engine.keys[e.key] = true;

      // Hotkey triggers
      if (e.key === 'i' || e.key === 'I') onOpenInventory();
      if (e.key === 'k' || e.key === 'K') onOpenSkills();
      if (e.key === 'm' || e.key === 'M') onOpenMap();
    };

    const handleKeyUp = (e: KeyboardEvent) => {
      engine.keys[e.key] = false;
    };

    window.addEventListener('keydown', handleKeyDown);
    window.addEventListener('keyup', handleKeyUp);

    // --- MAIN 60 FPS GAME ENGINE LOOP ---
    const loop = (currentTime: number) => {
      const dt = Math.min(0.033, (currentTime - lastTime) / 1000); // Clamp dt to max 30ms
      lastTime = currentTime;

      // 1. UPDATE ENGINE STATE
      engine.update(dt);

      // 2. RENDER 3D WEBGL SCENE (PBR Shaders, PCF Soft Shadows, Volumetric Fog)
      webglRenderer.render(engine);

      // 3. CLEAR CANVAS OVERLAY FOR HUD & EFFECTS
      ctx.clearRect(0, 0, canvas.width, canvas.height);

      const room = engine.getCurrentRoom();
      const zone = engine.getCurrentZone();

      if (room && zone) {
        ctx.save();
        ctx.translate(-engine.camera.x, -engine.camera.y);

        // --- A. ENEMY HEALTH BARS OVERLAY ---
        engine.activeEnemies.forEach((e) => {
          if (e.hp < e.maxHp) {
            const barW = Math.max(36, e.type.width);
            const barX = e.x + e.type.width / 2 - barW / 2;
            const barY = e.y - 14;
            ctx.fillStyle = 'rgba(15, 23, 42, 0.85)';
            ctx.fillRect(barX - 1, barY - 1, barW + 2, 7);
            ctx.fillStyle = '#ef4444';
            ctx.fillRect(barX, barY, barW * (Math.max(0, e.hp) / e.maxHp), 5);
            ctx.strokeStyle = '#f87171';
            ctx.lineWidth = 1;
            ctx.strokeRect(barX, barY, barW, 5);
          }
        });

        // --- B. WEAPON SWING ENERGY TRAILS ---
        if (engine.isAttacking) {
          ProceduralRenderer.drawWeaponArc(
            ctx,
            engine.playerPos.x + engine.playerWidth / 2,
            engine.playerPos.y + engine.playerHeight / 2,
            engine.facing,
            engine.equipped.weapon?.weaponType || 'greatsword',
            engine.equipped.weapon?.visualStyle?.bladeColor || '#fde047',
            engine.attackProgress
          );
        }

        // --- C. DYNAMIC PARTICLES ---
        engine.particleEngine.updateAndDraw(ctx, dt);

        ctx.restore();

        // --- D. DYNAMIC AMBIENT LIGHTING MASK LAYER ---
        const lights = engine.getLightSources();
        LightingEngine.renderLighting(
          ctx,
          canvas.width,
          canvas.height,
          zone.ambientLight,
          lights
        );

        // --- E. FOREGROUND 3D FRAMING ARCHWAYS (Metroid Dread Out-of-Focus Parallax) ---
        EnvironmentRenderer.drawForegroundFraming(ctx, canvas.width, canvas.height, engine.camera.x);

        // --- F. FLOATING DAMAGE & LEVEL TEXTS ---
        ctx.save();
        ctx.translate(-engine.camera.x, -engine.camera.y);
        engine.floatingTexts.forEach((ft) => {
          ctx.font = `bold ${Math.floor(18 * ft.scale)}px cinzel, serif`;
          ctx.fillStyle = ft.color;
          ctx.shadowColor = '#000000';
          ctx.shadowBlur = 8;
          ctx.fillText(ft.text, ft.x, ft.y);
        });
        ctx.restore();
      }

      animId = requestAnimationFrame(loop);
    };

    animId = requestAnimationFrame(loop);

    return () => {
      cancelAnimationFrame(animId);
      webglRenderer.dispose();
      window.removeEventListener('resize', handleResize);
      window.removeEventListener('keydown', handleKeyDown);
      window.removeEventListener('keyup', handleKeyUp);
    };
  }, [engine]);

  return (
    <div className="relative w-full h-full overflow-hidden select-none bg-black">
      <div ref={webglContainerRef} className="absolute inset-0 w-full h-full z-0 pointer-events-none" />
      <canvas
        ref={canvasRef}
        className="absolute inset-0 w-full h-full z-10 block cursor-crosshair"
      />
    </div>
  );
};

