/**
 * Belmont's Curse: Shadow Reign - Start & Title Screen Component
 * Cinematic dark gothic entrance with pouring rain storm canvas, dynamic thunderstorm
 * lightning flashes, screen rumble reaction, and high-fidelity silver-haired hero showcase artwork.
 */

import React, { useEffect, useRef, useState } from 'react';
import { Play, Sparkles, Volume2, Zap, CloudLightning, Shield, Sword } from 'lucide-react';
import { audio } from '../utils/audio';

interface StartScreenProps {
  onStartGame: () => void;
  onOpenSettings: () => void;
}

export const StartScreen: React.FC<StartScreenProps> = ({ onStartGame, onOpenSettings }) => {
  const bgCanvasRef = useRef<HTMLCanvasElement | null>(null);
  const heroCanvasRef = useRef<HTMLCanvasElement | null>(null);
  const [lightningIntensity, setLightningIntensity] = useState<number>(0);
  const [isShaking, setIsShaking] = useState<boolean>(false);

  const triggerLightning = () => {
    setLightningIntensity(1.0);
    setIsShaking(true);
    audio.playThunderRumble();

    // Fade lightning
    let flash = 1.0;
    const interval = setInterval(() => {
      flash -= 0.15;
      if (flash <= 0) {
        setLightningIntensity(0);
        setIsShaking(false);
        clearInterval(interval);
      } else {
        setLightningIntensity(flash);
      }
    }, 40);
  };

  // Background Parallax & Heavy Rain Storm Simulation
  useEffect(() => {
    const canvas = bgCanvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    let animId: number;
    let width = (canvas.width = window.innerWidth);
    let height = (canvas.height = window.innerHeight);

    const handleResize = () => {
      if (!canvas) return;
      width = canvas.width = window.innerWidth;
      height = canvas.height = window.innerHeight;
    };
    window.addEventListener('resize', handleResize);

    // Raindrop Particle System
    const raindrops: Array<{ x: number; y: number; speed: number; length: number; opacity: number }> = [];
    for (let i = 0; i < 280; i++) {
      raindrops.push({
        x: Math.random() * width * 1.2 - width * 0.1,
        y: Math.random() * height,
        speed: 18 + Math.random() * 22,
        length: 20 + Math.random() * 30,
        opacity: 0.2 + Math.random() * 0.6,
      });
    }

    // Rain Splash Particles
    const splashes: Array<{ x: number; y: number; radius: number; maxRadius: number; opacity: number }> = [];

    // Periodic automatic random lightning strike
    const lightningTimer = setInterval(() => {
      if (Math.random() < 0.35) {
        triggerLightning();
      }
    }, 4000);

    let mouseX = 0;
    const handleMouseMove = (e: MouseEvent) => {
      mouseX = (e.clientX / window.innerWidth - 0.5) * 30;
    };
    window.addEventListener('mousemove', handleMouseMove);

    const render = () => {
      ctx.clearRect(0, 0, width, height);

      // 1. Dark Midnight Sky with Lightning Glow Reaction
      const skyGrad = ctx.createLinearGradient(0, 0, 0, height);
      if (lightningIntensity > 0) {
        skyGrad.addColorStop(0, `rgba(186, 230, 253, ${0.4 * lightningIntensity})`);
        skyGrad.addColorStop(0.3, `rgba(56, 189, 248, ${0.2 * lightningIntensity})`);
        skyGrad.addColorStop(1, '#020617');
      } else {
        skyGrad.addColorStop(0, '#020617');
        skyGrad.addColorStop(0.5, '#090d16');
        skyGrad.addColorStop(1, '#0f172a');
      }
      ctx.fillStyle = skyGrad;
      ctx.fillRect(0, 0, width, height);

      // 2. Parallax Layer 1: Giant Full Gothic Blood/Cyan Moon
      ctx.save();
      ctx.shadowColor = lightningIntensity > 0 ? '#38bdf8' : '#7dd3fc';
      ctx.shadowBlur = 50 + lightningIntensity * 40;
      ctx.fillStyle = lightningIntensity > 0 ? '#f0f9ff' : '#e0f2fe';
      ctx.beginPath();
      ctx.arc(width * 0.7 + mouseX * 0.2, height * 0.25, 110, 0, Math.PI * 2);
      ctx.fill();
      ctx.restore();

      // 3. Parallax Layer 2: Gothic Castle Citadel Spires & Cathedral Silhouette
      ctx.fillStyle = lightningIntensity > 0 ? '#1e293b' : '#090d16';
      ctx.beginPath();
      const spireOffset = mouseX * 0.4;
      ctx.moveTo(0, height);
      ctx.lineTo(width * 0.15 + spireOffset, height * 0.45);
      ctx.lineTo(width * 0.22 + spireOffset, height * 0.6);
      ctx.lineTo(width * 0.32 + spireOffset, height * 0.3);
      ctx.lineTo(width * 0.4 + spireOffset, height * 0.55);
      ctx.lineTo(width * 0.55 + spireOffset, height * 0.25); // Main Keep Spire
      ctx.lineTo(width * 0.68 + spireOffset, height * 0.5);
      ctx.lineTo(width * 0.8 + spireOffset, height * 0.38);
      ctx.lineTo(width, height * 0.7);
      ctx.lineTo(width, height);
      ctx.closePath();
      ctx.fill();

      // Glowing Stained Glass Rose Window in Distant Citadel
      ctx.save();
      ctx.shadowColor = '#fbbf24';
      ctx.shadowBlur = 30;
      ctx.fillStyle = 'rgba(251, 191, 36, 0.7)';
      ctx.beginPath();
      ctx.arc(width * 0.55 + spireOffset, height * 0.35, 18, 0, Math.PI * 2);
      ctx.fill();
      ctx.restore();

      // 4. Pouring Rain Droplets
      ctx.strokeStyle = lightningIntensity > 0 ? 'rgba(224, 242, 254, 0.8)' : 'rgba(148, 163, 184, 0.5)';
      ctx.lineWidth = 1.5;
      for (let i = 0; i < raindrops.length; i++) {
        const p = raindrops[i];
        p.x += 4; // Wind angle
        p.y += p.speed;

        if (p.y > height) {
          p.y = -p.length;
          p.x = Math.random() * width * 1.2 - width * 0.1;

          // Splash on bottom
          if (Math.random() < 0.4) {
            splashes.push({
              x: p.x,
              y: height - Math.random() * 20,
              radius: 1,
              maxRadius: 6 + Math.random() * 8,
              opacity: 0.8,
            });
          }
        }

        ctx.beginPath();
        ctx.moveTo(p.x, p.y);
        ctx.lineTo(p.x - 6, p.y + p.length);
        ctx.stroke();
      }

      // 5. Rain Splash Rings on Ground
      for (let i = splashes.length - 1; i >= 0; i--) {
        const s = splashes[i];
        s.radius += 0.8;
        s.opacity -= 0.05;

        if (s.opacity <= 0) {
          splashes.splice(i, 1);
          continue;
        }

        ctx.strokeStyle = `rgba(186, 230, 253, ${s.opacity})`;
        ctx.lineWidth = 1;
        ctx.beginPath();
        ctx.ellipse(s.x, s.y, s.radius, s.radius * 0.4, 0, 0, Math.PI * 2);
        ctx.stroke();
      }

      // 6. Atmospheric Mist Layer
      const mistGrad = ctx.createLinearGradient(0, height * 0.7, 0, height);
      mistGrad.addColorStop(0, 'rgba(15, 23, 42, 0)');
      mistGrad.addColorStop(1, 'rgba(2, 6, 23, 0.95)');
      ctx.fillStyle = mistGrad;
      ctx.fillRect(0, height * 0.7, width, height * 0.3);

      animId = requestAnimationFrame(render);
    };

    render();

    return () => {
      cancelAnimationFrame(animId);
      clearInterval(lightningTimer);
      window.removeEventListener('resize', handleResize);
      window.removeEventListener('mousemove', handleMouseMove);
    };
  }, [lightningIntensity]);

  // Main Hero Character Artwork Canvas (Silver Hair, Black Armor, Crimson Cape, Blue Gem, Broadsword)
  useEffect(() => {
    const canvas = heroCanvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    let animId: number;
    let time = 0;

    const renderHero = () => {
      time += 0.03;
      ctx.clearRect(0, 0, canvas.width, canvas.height);

      const cx = canvas.width / 2;
      const cy = canvas.height / 2 + 30;

      ctx.save();

      // 1. Gargoyle Stone Pedestal
      ctx.fillStyle = '#1e293b';
      ctx.beginPath();
      ctx.moveTo(cx - 70, cy + 120);
      ctx.lineTo(cx + 70, cy + 120);
      ctx.lineTo(cx + 50, cy + 50);
      ctx.lineTo(cx - 50, cy + 50);
      ctx.closePath();
      ctx.fill();

      // Gargoyle Eyes Glow
      ctx.fillStyle = '#f59e0b';
      ctx.shadowColor = '#f59e0b';
      ctx.shadowBlur = 10;
      ctx.fillRect(cx - 20, cy + 70, 6, 4);
      ctx.fillRect(cx + 14, cy + 70, 6, 4);
      ctx.shadowBlur = 0;

      // 2. Billowing Crimson-Lined Velvet Cape (Dynamic Wind Physics)
      const wind1 = Math.sin(time * 2) * 20;
      const wind2 = Math.cos(time * 1.5) * 25;

      const capeGrad = ctx.createLinearGradient(cx - 80, cy - 80, cx + 90, cy + 100);
      capeGrad.addColorStop(0, '#991b1b'); // Crimson velvet
      capeGrad.addColorStop(0.5, '#7f1d1d');
      capeGrad.addColorStop(1, '#020617'); // Dark outer shadow

      ctx.fillStyle = capeGrad;
      ctx.beginPath();
      ctx.moveTo(cx - 15, cy - 80);
      ctx.quadraticCurveTo(cx - 90 + wind1, cy - 20, cx - 110 + wind2, cy + 80);
      ctx.lineTo(cx + 110 + wind1, cy + 85);
      ctx.quadraticCurveTo(cx + 80 - wind2, cy - 20, cx + 15, cy - 80);
      ctx.closePath();
      ctx.fill();

      // Gold Filigree Border on Cape
      ctx.strokeStyle = '#fbbf24';
      ctx.lineWidth = 3;
      ctx.stroke();

      // 3. Silver Armor Boots & Legs
      ctx.fillStyle = '#0f172a';
      ctx.fillRect(cx - 22, cy + 10, 18, 45);
      ctx.fillRect(cx + 4, cy + 10, 18, 45);

      const bootGrad = ctx.createLinearGradient(cx - 25, cy + 20, cx + 25, cy + 55);
      bootGrad.addColorStop(0, '#64748b');
      bootGrad.addColorStop(0.5, '#94a3b8');
      bootGrad.addColorStop(1, '#1e293b');
      ctx.fillStyle = bootGrad;
      ctx.fillRect(cx - 24, cy + 25, 20, 30);
      ctx.fillRect(cx + 4, cy + 25, 20, 30);

      // 4. Dark High-Tech Bodysuit Armor & Pauldrons
      const armorGrad = ctx.createLinearGradient(cx - 30, cy - 80, cx + 30, cy + 10);
      armorGrad.addColorStop(0, '#1e293b');
      armorGrad.addColorStop(0.5, '#334155');
      armorGrad.addColorStop(1, '#0f172a');
      ctx.fillStyle = armorGrad;
      ctx.fillRect(cx - 25, cy - 75, 50, 85);

      // Silver Shoulder Pauldrons
      ctx.fillStyle = '#cbd5e1';
      ctx.beginPath();
      ctx.arc(cx - 32, cy - 70, 16, 0, Math.PI * 2);
      ctx.arc(cx + 32, cy - 70, 16, 0, Math.PI * 2);
      ctx.fill();

      // Glowing Sapphire Chest Gem Brooch
      ctx.fillStyle = '#38bdf8';
      ctx.shadowColor = '#38bdf8';
      ctx.shadowBlur = 20;
      ctx.beginPath();
      ctx.arc(cx, cy - 55, 8, 0, Math.PI * 2);
      ctx.fill();

      // Gem Core Pulse
      ctx.fillStyle = '#ffffff';
      ctx.beginPath();
      ctx.arc(cx, cy - 55, 3 + Math.sin(time * 3) * 1.5, 0, Math.PI * 2);
      ctx.fill();
      ctx.shadowBlur = 0;

      // 5. Face & Piercing Violet/Blue Vampiric Eyes
      ctx.fillStyle = '#fef08a'; // Pale skin
      ctx.fillRect(cx - 12, cy - 110, 24, 30);

      ctx.fillStyle = '#38bdf8'; // Glowing eye
      ctx.shadowColor = '#38bdf8';
      ctx.shadowBlur = 8;
      ctx.fillRect(cx + 2, cy - 98, 4, 4);
      ctx.shadowBlur = 0;

      // 6. Multi-Layer Long Flowing Silver Hair (Past Shoulders)
      ctx.fillStyle = '#f8fafc';
      ctx.beginPath();
      ctx.arc(cx, cy - 112, 18, Math.PI, 0);
      ctx.fill();

      // Long Silver Hair Strands Fluttering in Wind
      ctx.strokeStyle = '#e2e8f0';
      ctx.lineWidth = 4;
      for (let s = -3; s <= 3; s++) {
        ctx.beginPath();
        ctx.moveTo(cx + s * 5, cy - 110);
        ctx.quadraticCurveTo(cx - 40 + s * 6 + wind1, cy - 50, cx - 70 + s * 8 + wind2, cy + 20);
        ctx.stroke();
      }

      // 7. Broadsword Held in Hand with Blue Gem Pommel
      ctx.save();
      ctx.translate(cx + 28, cy - 40);
      ctx.rotate(-0.35 + Math.sin(time) * 0.05);

      // Blade
      const bladeGrad = ctx.createLinearGradient(-5, -120, 5, 0);
      bladeGrad.addColorStop(0, '#f8fafc');
      bladeGrad.addColorStop(0.5, '#cbd5e1');
      bladeGrad.addColorStop(1, '#64748b');
      ctx.fillStyle = bladeGrad;
      ctx.fillRect(-6, -120, 12, 120);

      // Blade Runic Groove
      ctx.fillStyle = '#38bdf8';
      ctx.shadowColor = '#38bdf8';
      ctx.shadowBlur = 12;
      ctx.fillRect(-1.5, -100, 3, 90);

      // Crossguard
      ctx.fillStyle = '#fbbf24';
      ctx.fillRect(-20, 0, 40, 8);

      // Handle Grip & Sapphire Pommel
      ctx.fillStyle = '#78350f';
      ctx.fillRect(-3, 8, 6, 20);

      ctx.fillStyle = '#38bdf8';
      ctx.beginPath();
      ctx.arc(0, 32, 7, 0, Math.PI * 2);
      ctx.fill();
      ctx.restore();

      ctx.restore();

      animId = requestAnimationFrame(renderHero);
    };

    renderHero();

    return () => {
      cancelAnimationFrame(animId);
    };
  }, []);

  const handleStart = () => {
    audio.playUiClick();
    audio.startMusicTrack('courtyard');
    onStartGame();
  };

  return (
    <div
      className={`relative w-full h-full bg-zinc-950 flex flex-col items-center justify-between p-6 sm:p-8 text-amber-100 select-none overflow-hidden font-sans transition-transform duration-75 ${
        isShaking ? 'translate-x-1 translate-y-1 scale-[1.005]' : ''
      }`}
    >
      {/* Background Animated Parallax & Pouring Rain Canvas */}
      <canvas ref={bgCanvasRef} className="absolute inset-0 w-full h-full pointer-events-none z-0" />

      {/* Screen Reactive Lightning Flash Overlay */}
      {lightningIntensity > 0 && (
        <div
          className="absolute inset-0 bg-sky-200 pointer-events-none z-10 transition-opacity duration-75"
          style={{ opacity: lightningIntensity * 0.35 }}
        />
      )}

      {/* Top Banner Header */}
      <div className="relative z-20 flex flex-col items-center mt-6 text-center">
        <div className="flex items-center gap-2 text-amber-300 text-xs tracking-[0.3em] font-semibold uppercase mb-2 bg-zinc-950/70 border border-amber-800/60 px-4 py-1.5 rounded-full backdrop-blur-xl shadow-2xl">
          <Sparkles className="w-3.5 h-3.5 text-amber-400 animate-pulse" />
          <span>Next-Gen Gothic Action Metroidvania</span>
        </div>

        <h1 className="text-4xl sm:text-6xl md:text-7xl font-black tracking-tight text-transparent bg-clip-text bg-gradient-to-b from-amber-100 via-amber-300 to-amber-700 drop-shadow-[0_10px_25px_rgba(0,0,0,0.95)] uppercase font-serif">
          Midnight Returns
        </h1>
      </div>

      {/* Hero Showcase Center Layout */}
      <div className="relative z-20 my-auto flex flex-col md:flex-row items-center justify-center gap-8 max-w-4xl w-full">
        {/* Left: Main Character Art Canvas Showcase */}
        <div className="relative w-64 h-80 sm:w-72 sm:h-96 rounded-2xl bg-zinc-950/80 border border-amber-600/50 shadow-[0_0_50px_rgba(0,0,0,0.9)] overflow-hidden flex items-center justify-center backdrop-blur-md group">
          <canvas ref={heroCanvasRef} width={320} height={400} className="w-full h-full block" />
          <div className="absolute bottom-2 left-2 right-2 bg-zinc-950/90 border border-amber-900/60 rounded-lg p-2 text-center text-[11px] text-amber-300 font-serif">
            <span className="font-bold text-amber-200">Lord Belmont</span> — Silver Knight
          </div>
        </div>

        {/* Right: Menu Actions & Thunder Trigger */}
        <div className="flex flex-col gap-4 w-full max-w-xs">
          <button
            onClick={handleStart}
            className="group relative w-full bg-gradient-to-r from-amber-600 via-amber-500 to-amber-700 hover:from-amber-500 hover:to-amber-600 text-zinc-950 font-extrabold py-4 px-6 rounded-xl shadow-[0_0_35px_rgba(245,158,11,0.4)] hover:shadow-[0_0_55px_rgba(245,158,11,0.6)] transition-all duration-200 flex items-center justify-center gap-3 text-base uppercase tracking-wider"
          >
            <Play className="w-5 h-5 fill-current" />
            <span>Enter Castle</span>
          </button>

          <button
            onClick={triggerLightning}
            className="w-full bg-zinc-900/90 hover:bg-zinc-800 border border-sky-600/50 hover:border-sky-400 text-sky-200 font-bold py-3 px-6 rounded-xl transition-all duration-200 flex items-center justify-center gap-2 text-xs uppercase tracking-wider backdrop-blur-md shadow-lg"
          >
            <CloudLightning className="w-4 h-4 text-sky-400 animate-bounce" />
            <span>Summon Thunder Strike</span>
          </button>

          <button
            onClick={onOpenSettings}
            className="w-full bg-zinc-900/90 hover:bg-zinc-800 border border-amber-800/50 hover:border-amber-400 text-amber-200 font-bold py-3 px-6 rounded-xl transition-all duration-200 flex items-center justify-center gap-2 text-xs uppercase tracking-wider backdrop-blur-md"
          >
            <Volume2 className="w-4 h-4 text-amber-400" />
            <span>Audio & Display Options</span>
          </button>
        </div>
      </div>

      {/* Controls Quick Reference Card */}
      <div className="relative z-20 bg-zinc-950/90 border border-amber-900/50 rounded-xl p-3.5 max-w-xl w-full backdrop-blur-xl text-xs text-zinc-300 flex flex-wrap justify-around gap-3 mb-2 shadow-2xl">
        <div className="flex items-center gap-2">
          <kbd className="bg-zinc-800 border border-zinc-700 px-2 py-0.5 rounded text-amber-300 font-mono text-[10px]">A / D</kbd>
          <span>Move Left/Right</span>
        </div>
        <div className="flex items-center gap-2">
          <kbd className="bg-zinc-800 border border-zinc-700 px-2 py-0.5 rounded text-amber-300 font-mono text-[10px]">SPACE</kbd>
          <span>Jump / Double Jump</span>
        </div>
        <div className="flex items-center gap-2">
          <kbd className="bg-zinc-800 border border-zinc-700 px-2 py-0.5 rounded text-amber-300 font-mono text-[10px]">SHIFT / K</kbd>
          <span>Flash Shift Dash</span>
        </div>
        <div className="flex items-center gap-2">
          <kbd className="bg-zinc-800 border border-zinc-700 px-2 py-0.5 rounded text-amber-300 font-mono text-[10px]">J / ENTER</kbd>
          <span>Weapon Attack</span>
        </div>
        <div className="flex items-center gap-2">
          <kbd className="bg-zinc-800 border border-zinc-700 px-2 py-0.5 rounded text-amber-300 font-mono text-[10px]">U / E</kbd>
          <span>Holy Flame Spell</span>
        </div>
      </div>
    </div>
  );
};

