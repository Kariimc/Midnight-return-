/**
 * Belmont's Curse: Shadow Reign - Procedural PBR Texture & Asset Generator
 * Generates high-resolution 512x512 / 1024x1024 PBR Albedo, Normal, Roughness,
 * and Emissive Maps dynamically at runtime for Three.js WebGL materials.
 */

import * as THREE from 'three';

export class TextureGenerator {
  /**
   * Create HD Stone Masonry Albedo & Bump Texture Map
   */
  public static createStoneTexture(): THREE.CanvasTexture {
    const canvas = document.createElement('canvas');
    canvas.width = 512;
    canvas.height = 512;
    const ctx = canvas.getContext('2d')!;

    // Base Slate Gradient
    const baseGrad = ctx.createLinearGradient(0, 0, 512, 512);
    baseGrad.addColorStop(0, '#2d3342');
    baseGrad.addColorStop(0.5, '#1c202b');
    baseGrad.addColorStop(1, '#0e1118');
    ctx.fillStyle = baseGrad;
    ctx.fillRect(0, 0, 512, 512);

    // Weathered Micro-Surface Noise
    const imgData = ctx.getImageData(0, 0, 512, 512);
    const data = imgData.data;
    for (let i = 0; i < data.length; i += 4) {
      const noise = (Math.random() - 0.5) * 40;
      data[i] = Math.min(255, Math.max(0, data[i] + noise));
      data[i + 1] = Math.min(255, Math.max(0, data[i + 1] + noise + 3));
      data[i + 2] = Math.min(255, Math.max(0, data[i + 2] + noise + 10));
    }
    ctx.putImageData(imgData, 0, 0);

    // Chiseled Stone Blocks Grid Lines with Mortar
    ctx.strokeStyle = '#090c12';
    ctx.lineWidth = 4;
    const blockW = 64;
    const blockH = 32;

    for (let y = 0; y < 512; y += blockH) {
      const offset = (Math.floor(y / blockH) % 2) * (blockW / 2);
      ctx.beginPath();
      ctx.moveTo(0, y);
      ctx.lineTo(512, y);
      ctx.stroke();

      for (let x = offset; x < 512; x += blockW) {
        ctx.beginPath();
        ctx.moveTo(x, y);
        ctx.lineTo(x, y + blockH);
        ctx.stroke();
      }
    }

    const texture = new THREE.CanvasTexture(canvas);
    texture.wrapS = THREE.RepeatWrapping;
    texture.wrapT = THREE.RepeatWrapping;
    return texture;
  }

  /**
   * Create Stone Normal Map for Realistic PBR Depth & Beveled Edges
   */
  public static createStoneNormalMap(): THREE.CanvasTexture {
    const canvas = document.createElement('canvas');
    canvas.width = 512;
    canvas.height = 512;
    const ctx = canvas.getContext('2d')!;

    // Neutral Normal Base (128, 128, 255 -> #8080ff)
    ctx.fillStyle = '#8080ff';
    ctx.fillRect(0, 0, 512, 512);

    const blockW = 64;
    const blockH = 32;

    // Normal Bevels on Block Joints
    for (let y = 0; y < 512; y += blockH) {
      const offset = (Math.floor(y / blockH) % 2) * (blockW / 2);

      // Top Bevel (Facing Up -> Y Normal positive)
      ctx.fillStyle = '#80ff80';
      ctx.fillRect(0, y, 512, 3);

      // Bottom Bevel (Facing Down -> Y Normal negative)
      ctx.fillStyle = '#800080';
      ctx.fillRect(0, y + blockH - 3, 512, 3);

      for (let x = offset; x < 512; x += blockW) {
        // Left Bevel (Facing Left -> X Normal negative)
        ctx.fillStyle = '#008080';
        ctx.fillRect(x, y, 3, blockH);

        // Right Bevel (Facing Right -> X Normal positive)
        ctx.fillStyle = '#ff8080';
        ctx.fillRect(x + blockW - 3, y, 3, blockH);
      }
    }

    const texture = new THREE.CanvasTexture(canvas);
    texture.wrapS = THREE.RepeatWrapping;
    texture.wrapT = THREE.RepeatWrapping;
    return texture;
  }

  /**
   * Create Glowing Neon Cyan Power Conduit Emissive Circuit Map
   */
  public static createConduitEmissiveMap(): THREE.CanvasTexture {
    const canvas = document.createElement('canvas');
    canvas.width = 512;
    canvas.height = 512;
    const ctx = canvas.getContext('2d')!;

    // Black Background (Non-Emissive)
    ctx.fillStyle = '#000000';
    ctx.fillRect(0, 0, 512, 512);

    // Glowing Runic Cyan Circuits
    ctx.strokeStyle = '#22d3ee';
    ctx.lineWidth = 6;
    ctx.shadowColor = '#06b6d4';
    ctx.shadowBlur = 18;

    // Main horizontal power trace
    ctx.beginPath();
    ctx.moveTo(0, 256);
    ctx.lineTo(512, 256);
    ctx.stroke();

    // Circuit branches & nodes
    for (let x = 64; x < 512; x += 128) {
      ctx.beginPath();
      ctx.moveTo(x, 256);
      ctx.lineTo(x + 32, 128);
      ctx.lineTo(x + 96, 128);
      ctx.stroke();

      // Runic Node Rings
      ctx.fillStyle = '#38bdf8';
      ctx.beginPath();
      ctx.arc(x + 96, 128, 9, 0, Math.PI * 2);
      ctx.fill();
    }

    const texture = new THREE.CanvasTexture(canvas);
    texture.wrapS = THREE.RepeatWrapping;
    texture.wrapT = THREE.RepeatWrapping;
    return texture;
  }

  /**
   * Create Gothic Stained Glass Window Texture with Lead Tracery
   */
  public static createStainedGlassTexture(): THREE.CanvasTexture {
    const canvas = document.createElement('canvas');
    canvas.width = 512;
    canvas.height = 1024;
    const ctx = canvas.getContext('2d')!;

    // Radial Jewel Gradient
    const rad = ctx.createRadialGradient(256, 300, 20, 256, 512, 500);
    rad.addColorStop(0, '#38bdf8');
    rad.addColorStop(0.35, '#a855f7');
    rad.addColorStop(0.7, '#e11d48');
    rad.addColorStop(1, '#0f172a');

    ctx.fillStyle = rad;
    ctx.fillRect(0, 0, 512, 1024);

    // Rose Window Lead Tracery
    ctx.strokeStyle = '#020617';
    ctx.lineWidth = 8;

    ctx.beginPath();
    ctx.arc(256, 300, 200, 0, Math.PI * 2);
    ctx.stroke();

    for (let a = 0; a < Math.PI * 2; a += Math.PI / 6) {
      ctx.beginPath();
      ctx.moveTo(256, 300);
      ctx.lineTo(256 + Math.cos(a) * 200, 300 + Math.sin(a) * 200);
      ctx.stroke();
    }

    const texture = new THREE.CanvasTexture(canvas);
    return texture;
  }

  /**
   * AAA Hero Gothic Sci-Fi Armor Texture Map
   */
  public static createHeroArmorTexture(): THREE.CanvasTexture {
    const canvas = document.createElement('canvas');
    canvas.width = 512;
    canvas.height = 512;
    const ctx = canvas.getContext('2d')!;

    // Dark Slate Steel Base
    const grad = ctx.createLinearGradient(0, 0, 512, 512);
    grad.addColorStop(0, '#1e293b');
    grad.addColorStop(0.5, '#334155');
    grad.addColorStop(1, '#0f172a');
    ctx.fillStyle = grad;
    ctx.fillRect(0, 0, 512, 512);

    // Carbon Fiber Weave / Metal Grain
    const imgData = ctx.getImageData(0, 0, 512, 512);
    const data = imgData.data;
    for (let i = 0; i < data.length; i += 4) {
      const noise = (Math.random() - 0.5) * 25;
      data[i] = Math.min(255, Math.max(0, data[i] + noise));
      data[i + 1] = Math.min(255, Math.max(0, data[i + 1] + noise + 2));
      data[i + 2] = Math.min(255, Math.max(0, data[i + 2] + noise + 8));
    }
    ctx.putImageData(imgData, 0, 0);

    // Gold/Silver Filigree Scrollwork & Energy Channels
    ctx.strokeStyle = '#f59e0b';
    ctx.lineWidth = 3;
    ctx.shadowColor = '#fbbf24';
    ctx.shadowBlur = 8;

    ctx.strokeRect(32, 32, 448, 448);
    ctx.beginPath();
    ctx.arc(256, 256, 120, 0, Math.PI * 2);
    ctx.stroke();

    // Glowing Blue Cyber Veins
    ctx.strokeStyle = '#38bdf8';
    ctx.lineWidth = 4;
    ctx.shadowColor = '#0284c7';
    ctx.shadowBlur = 12;

    ctx.beginPath();
    ctx.moveTo(64, 256);
    ctx.lineTo(200, 256);
    ctx.lineTo(256, 200);
    ctx.lineTo(256, 64);
    ctx.moveTo(448, 256);
    ctx.lineTo(312, 256);
    ctx.lineTo(256, 312);
    ctx.lineTo(256, 448);
    ctx.stroke();

    const texture = new THREE.CanvasTexture(canvas);
    texture.wrapS = THREE.RepeatWrapping;
    texture.wrapT = THREE.RepeatWrapping;
    return texture;
  }

  /**
   * AAA Boss Executioner Heavy Iron Armor Texture
   */
  public static createBossArmorTexture(): THREE.CanvasTexture {
    const canvas = document.createElement('canvas');
    canvas.width = 512;
    canvas.height = 512;
    const ctx = canvas.getContext('2d')!;

    // Black Obsidian Steel
    ctx.fillStyle = '#0f172a';
    ctx.fillRect(0, 0, 512, 512);

    // Volcanic Glowing Red Vents & Battle Damage Scratches
    ctx.strokeStyle = '#ef4444';
    ctx.lineWidth = 5;
    ctx.shadowColor = '#b91c1c';
    ctx.shadowBlur = 16;

    for (let y = 64; y < 512; y += 128) {
      ctx.beginPath();
      ctx.moveTo(64, y);
      ctx.lineTo(448, y);
      ctx.stroke();
    }

    // Heavy Metal Rivets
    ctx.fillStyle = '#94a3b8';
    ctx.shadowBlur = 0;
    for (let x = 32; x < 512; x += 64) {
      for (let y = 32; y < 512; y += 64) {
        ctx.beginPath();
        ctx.arc(x, y, 5, 0, Math.PI * 2);
        ctx.fill();
      }
    }

    const texture = new THREE.CanvasTexture(canvas);
    texture.wrapS = THREE.RepeatWrapping;
    texture.wrapT = THREE.RepeatWrapping;
    return texture;
  }

  /**
   * AAA Gargoyle Scale / Dragon Leather Texture
   */
  public static createGargoyleSkinTexture(): THREE.CanvasTexture {
    const canvas = document.createElement('canvas');
    canvas.width = 512;
    canvas.height = 512;
    const ctx = canvas.getContext('2d')!;

    ctx.fillStyle = '#18181b';
    ctx.fillRect(0, 0, 512, 512);

    ctx.strokeStyle = '#881337';
    ctx.lineWidth = 2;

    const scaleSize = 24;
    for (let y = 0; y < 512; y += scaleSize) {
      const xOffset = (Math.floor(y / scaleSize) % 2) * (scaleSize / 2);
      for (let x = xOffset; x < 512; x += scaleSize) {
        ctx.beginPath();
        ctx.arc(x, y, scaleSize / 2, 0, Math.PI);
        ctx.stroke();
      }
    }

    const texture = new THREE.CanvasTexture(canvas);
    texture.wrapS = THREE.RepeatWrapping;
    texture.wrapT = THREE.RepeatWrapping;
    return texture;
  }

  /**
   * AAA Industrial Metal Floor Grating Texture
   */
  public static createMetalGratingTexture(): THREE.CanvasTexture {
    const canvas = document.createElement('canvas');
    canvas.width = 512;
    canvas.height = 512;
    const ctx = canvas.getContext('2d')!;

    ctx.fillStyle = '#1e293b';
    ctx.fillRect(0, 0, 512, 512);

    // Diamond Tread Plate Pattern
    ctx.strokeStyle = '#475569';
    ctx.lineWidth = 3;

    for (let x = 0; x < 512; x += 32) {
      for (let y = 0; y < 512; y += 32) {
        ctx.beginPath();
        ctx.moveTo(x + 16, y);
        ctx.lineTo(x + 32, y + 16);
        ctx.lineTo(x + 16, y + 32);
        ctx.lineTo(x, y + 16);
        ctx.closePath();
        ctx.stroke();
      }
    }

    // Neon Cyan Tread Lights
    ctx.strokeStyle = '#06b6d4';
    ctx.lineWidth = 4;
    ctx.shadowColor = '#0891b2';
    ctx.shadowBlur = 10;
    ctx.beginPath();
    ctx.moveTo(0, 256);
    ctx.lineTo(512, 256);
    ctx.stroke();

    const texture = new THREE.CanvasTexture(canvas);
    texture.wrapS = THREE.RepeatWrapping;
    texture.wrapT = THREE.RepeatWrapping;
    return texture;
  }

  /**
   * AAA Skeleton Aged Bone Texture Map
   */
  public static createBoneTexture(): THREE.CanvasTexture {
    const canvas = document.createElement('canvas');
    canvas.width = 512;
    canvas.height = 512;
    const ctx = canvas.getContext('2d')!;

    // Aged Ivory Base
    ctx.fillStyle = '#f1f5f9';
    ctx.fillRect(0, 0, 512, 512);

    // Weathered Bone Cracks & Grain
    ctx.strokeStyle = '#94a3b8';
    ctx.lineWidth = 1;
    for (let i = 0; i < 40; i++) {
      ctx.beginPath();
      ctx.moveTo(Math.random() * 512, Math.random() * 512);
      ctx.lineTo(Math.random() * 512, Math.random() * 512);
      ctx.stroke();
    }

    const texture = new THREE.CanvasTexture(canvas);
    texture.wrapS = THREE.RepeatWrapping;
    texture.wrapT = THREE.RepeatWrapping;
    return texture;
  }

  /**
   * AAA Sci-Fi Wall Panel Texture Map
   */
  public static createSciFiPanelTexture(): THREE.CanvasTexture {
    const canvas = document.createElement('canvas');
    canvas.width = 512;
    canvas.height = 512;
    const ctx = canvas.getContext('2d')!;

    // Dark Industrial Alloy
    ctx.fillStyle = '#0f172a';
    ctx.fillRect(0, 0, 512, 512);

    // Beveled Panel Divisions
    ctx.strokeStyle = '#334155';
    ctx.lineWidth = 6;
    ctx.strokeRect(16, 16, 480, 480);
    ctx.strokeRect(128, 128, 256, 256);

    // Glowing Neon Cyan Conduits
    ctx.strokeStyle = '#22d3ee';
    ctx.lineWidth = 3;
    ctx.shadowColor = '#0891b2';
    ctx.shadowBlur = 12;

    ctx.beginPath();
    ctx.moveTo(16, 256);
    ctx.lineTo(128, 256);
    ctx.moveTo(384, 256);
    ctx.lineTo(496, 256);
    ctx.stroke();

    const texture = new THREE.CanvasTexture(canvas);
    texture.wrapS = THREE.RepeatWrapping;
    texture.wrapT = THREE.RepeatWrapping;
    return texture;
  }
}


