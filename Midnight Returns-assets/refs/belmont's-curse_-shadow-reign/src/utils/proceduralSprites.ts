/**
 * Belmont's Curse: Shadow Reign - Metroid Dread Level AAA 2.5D Graphics Engine
 * Renders PBR textured masonry, glowing neon cyan electric power conduits,
 * metallic brushed armor, volumetric light shafts, foreground 3D framing arches,
 * and high-resolution character/boss artwork matching Lord Belmont's design sheet.
 */

import { CapePoint, EquipmentItem, HairParticle, WeaponType } from '../types/game';

export class EnvironmentRenderer {
  /**
   * Render high-fidelity 2.5D platform with PBR stone/metal textures,
   * glowing neon cyan power conduits, metallic rivets, and bright rim light edge highlights.
   */
  public static drawPlatform(
    ctx: CanvasRenderingContext2D,
    x: number,
    y: number,
    width: number,
    height: number,
    zoneType: string = 'zone_courtyard'
  ) {
    ctx.save();
    const time = Date.now() * 0.003;

    // 1. PBR Base Material Gradient per Biome
    const pGrad = ctx.createLinearGradient(x, y, x, y + height);
    if (zoneType === 'zone_courtyard' || zoneType === 'zone_cathedral') {
      // Dark Slate Gothic Stone
      pGrad.addColorStop(0, '#2d3342');
      pGrad.addColorStop(0.3, '#1c202b');
      pGrad.addColorStop(1, '#0e1118');
    } else if (zoneType === 'zone_forge' || zoneType === 'zone_clockwork') {
      // Dark Industrial Steel & Iron
      pGrad.addColorStop(0, '#334155');
      pGrad.addColorStop(0.3, '#1e293b');
      pGrad.addColorStop(1, '#0f172a');
    } else if (zoneType === 'zone_aqueduct' || zoneType === 'zone_abyss') {
      // Wet Mossy Obsidian
      pGrad.addColorStop(0, '#1e3a8a');
      pGrad.addColorStop(0.3, '#172554');
      pGrad.addColorStop(1, '#080e1e');
    } else if (zoneType === 'zone_crypt') {
      // Dark Tomb Marble
      pGrad.addColorStop(0, '#3b0764');
      pGrad.addColorStop(0.3, '#1e1b4b');
      pGrad.addColorStop(1, '#090514');
    } else if (zoneType === 'zone_library') {
      // Polished Dark Mahogany & Brass
      pGrad.addColorStop(0, '#451a03');
      pGrad.addColorStop(0.3, '#290e02');
      pGrad.addColorStop(1, '#0f0501');
    } else if (zoneType === 'zone_mines' || zoneType === 'zone_volcano') {
      // Basalt Rock & Lava Veins
      pGrad.addColorStop(0, '#261a15');
      pGrad.addColorStop(0.3, '#170f0c');
      pGrad.addColorStop(1, '#0a0504');
    } else {
      // Royal Throne Room Obsidian
      pGrad.addColorStop(0, '#3f0f1c');
      pGrad.addColorStop(0.3, '#21070e');
      pGrad.addColorStop(1, '#0a0204');
    }

    ctx.fillStyle = pGrad;
    ctx.fillRect(x, y, width, height);

    // 2. Chiseled Masonry Blocks & Metallic Grid Lines
    ctx.strokeStyle = 'rgba(0, 0, 0, 0.45)';
    ctx.lineWidth = 1.5;
    const blockW = 32;
    const blockH = 16;
    for (let bx = x; bx < x + width; bx += blockW) {
      for (let by = y + 8; by < y + height; by += blockH) {
        const offset = (Math.floor(by / blockH) % 2) * (blockW / 2);
        ctx.strokeRect(bx + offset, by, blockW, blockH);
      }
    }

    // 3. Metallic Rivets / Studs for Industrial & Gothic Mechanical Feel
    ctx.fillStyle = '#64748b';
    for (let rx = x + 16; rx < x + width; rx += 64) {
      ctx.beginPath();
      ctx.arc(rx, y + 10, 2, 0, Math.PI * 2);
      ctx.fill();
    }

    // 4. GLOWING NEON CYAN ELECTRIC POWER CONDUITS (Metroid Dread Style)
    ctx.save();
    ctx.shadowColor = '#06b6d4';
    ctx.shadowBlur = 12;
    ctx.strokeStyle = '#22d3ee';
    ctx.lineWidth = 2.5;

    ctx.beginPath();
    // Conduit line along platform front
    ctx.moveTo(x + 10, y + 6);
    ctx.lineTo(x + width - 10, y + 6);
    ctx.stroke();

    // Animated Pulsing Electric Energy Nodes
    for (let nx = x + 40; nx < x + width - 30; nx += 120) {
      const pulse = Math.sin(time + nx * 0.05) * 0.5 + 0.5;
      ctx.fillStyle = `rgba(56, 189, 248, ${0.4 + pulse * 0.6})`;
      ctx.beginPath();
      ctx.arc(nx, y + 6, 3.5 + pulse * 1.5, 0, Math.PI * 2);
      ctx.fill();

      // Vertical energy drop wire
      ctx.strokeStyle = 'rgba(34, 211, 238, 0.6)';
      ctx.lineWidth = 1;
      ctx.beginPath();
      ctx.moveTo(nx, y + 6);
      ctx.lineTo(nx, y + Math.min(height, 24));
      ctx.stroke();
    }
    ctx.restore();

    // 5. METROID DREAD RIM LIGHT HIGHLIGHT (Top Edge Crisp Specular Line)
    const rimGrad = ctx.createLinearGradient(x, y, x + width, y);
    rimGrad.addColorStop(0, 'rgba(255, 255, 255, 0.9)');
    rimGrad.addColorStop(0.5, 'rgba(186, 230, 253, 0.95)');
    rimGrad.addColorStop(1, 'rgba(255, 255, 255, 0.9)');

    ctx.fillStyle = rimGrad;
    ctx.fillRect(x, y, width, 2);

    // Subtle Cyan Rim Glow
    ctx.fillStyle = 'rgba(56, 189, 248, 0.4)';
    ctx.fillRect(x, y + 2, width, 1.5);

    // 6. Bottom Beveled Shadow with Ambient Occlusion
    ctx.fillStyle = 'rgba(0, 0, 0, 0.7)';
    ctx.fillRect(x, y + height - 5, width, 5);

    ctx.restore();
  }

  /**
   * Render Grand Gothic Stained Glass Window with Lead Tracery & Radial Sunlight/Moonlight Rays
   */
  public static drawStainedGlassWindow(
    ctx: CanvasRenderingContext2D,
    x: number,
    y: number,
    width: number,
    height: number
  ) {
    ctx.save();

    // Arch Outer Frame Masonry
    ctx.fillStyle = '#0f172a';
    ctx.beginPath();
    ctx.arc(x, y - height / 2, width / 2 + 12, Math.PI, 0);
    ctx.lineTo(x + width / 2 + 12, y + height / 2 + 12);
    ctx.lineTo(x - width / 2 - 12, y + height / 2 + 12);
    ctx.closePath();
    ctx.fill();

    // Inner Glass Color Gradient
    const glassGrad = ctx.createRadialGradient(x, y, 10, x, y, height);
    glassGrad.addColorStop(0, 'rgba(56, 189, 248, 0.9)');
    glassGrad.addColorStop(0.35, 'rgba(168, 85, 247, 0.8)');
    glassGrad.addColorStop(0.75, 'rgba(225, 29, 72, 0.7)');
    glassGrad.addColorStop(1, 'rgba(15, 23, 42, 0.95)');

    ctx.fillStyle = glassGrad;
    ctx.beginPath();
    ctx.arc(x, y - height / 2, width / 2, Math.PI, 0);
    ctx.lineTo(x + width / 2, y + height / 2);
    ctx.lineTo(x - width / 2, y + height / 2);
    ctx.closePath();
    ctx.fill();

    // Gothic Lead Tracery & Rose Window Spokes
    ctx.strokeStyle = '#020617';
    ctx.lineWidth = 3;
    for (let a = 0; a < Math.PI * 2; a += Math.PI / 4) {
      ctx.beginPath();
      ctx.moveTo(x, y - height / 2);
      ctx.lineTo(x + Math.cos(a) * (width / 2), y - height / 2 + Math.sin(a) * (width / 2));
      ctx.stroke();
    }

    // Outer Glow Halo
    ctx.shadowColor = '#38bdf8';
    ctx.shadowBlur = 30;
    ctx.strokeStyle = 'rgba(56, 189, 248, 0.5)';
    ctx.lineWidth = 2;
    ctx.stroke();

    ctx.restore();
  }

  /**
   * Render Gothic Stone Pillar with Fluted Grooves & Neon Cyan Power Conduits
   */
  public static drawPillar(
    ctx: CanvasRenderingContext2D,
    x: number,
    y: number,
    height: number
  ) {
    ctx.save();
    const w = 48;

    // 3D Cylinder Shading
    const pGrad = ctx.createLinearGradient(x - w / 2, y, x + w / 2, y);
    pGrad.addColorStop(0, '#090d16');
    pGrad.addColorStop(0.3, '#334155');
    pGrad.addColorStop(0.7, '#1e293b');
    pGrad.addColorStop(1, '#020617');

    ctx.fillStyle = pGrad;
    ctx.fillRect(x - w / 2, y - height / 2, w, height);

    // Vertical Fluted Grooves
    ctx.strokeStyle = 'rgba(0, 0, 0, 0.6)';
    ctx.lineWidth = 2;
    for (let gx = -16; gx <= 16; gx += 8) {
      ctx.beginPath();
      ctx.moveTo(x + gx, y - height / 2 + 20);
      ctx.lineTo(x + gx, y + height / 2 - 20);
      ctx.stroke();
    }

    // Wrapped Neon Cyan Power Wire
    ctx.save();
    ctx.shadowColor = '#38bdf8';
    ctx.shadowBlur = 10;
    ctx.strokeStyle = '#06b6d4';
    ctx.lineWidth = 2.5;
    ctx.beginPath();
    for (let py = y - height / 2 + 30; py < y + height / 2 - 30; py += 40) {
      ctx.moveTo(x - w / 2, py);
      ctx.quadraticCurveTo(x, py + 12, x + w / 2, py + 24);
    }
    ctx.stroke();
    ctx.restore();

    // Top Capital & Gold Trim
    ctx.fillStyle = '#334155';
    ctx.fillRect(x - w / 2 - 8, y - height / 2, w + 16, 20);
    ctx.fillStyle = '#fbbf24';
    ctx.fillRect(x - w / 2 - 8, y - height / 2, w + 16, 3);

    // Pedestal Base
    ctx.fillStyle = '#1e293b';
    ctx.fillRect(x - w / 2 - 10, y + height / 2 - 22, w + 20, 22);

    ctx.restore();
  }

  /**
   * Render Industrial Clockwork Brass Gear for Forge & Subterranean Mechanical Chambers
   */
  public static drawClockworkGear(
    ctx: CanvasRenderingContext2D,
    x: number,
    y: number,
    radius: number,
    rotation: number
  ) {
    ctx.save();
    ctx.translate(x, y);
    ctx.rotate(rotation);

    // Outer Gear Body
    const gearGrad = ctx.createRadialGradient(0, 0, 5, 0, 0, radius);
    gearGrad.addColorStop(0, '#fbbf24');
    gearGrad.addColorStop(0.6, '#b45309');
    gearGrad.addColorStop(1, '#451a03');

    ctx.fillStyle = gearGrad;
    ctx.beginPath();
    ctx.arc(0, 0, radius, 0, Math.PI * 2);
    ctx.fill();

    // Teeth
    const teeth = 12;
    ctx.fillStyle = '#78350f';
    for (let i = 0; i < teeth; i++) {
      const angle = (i * Math.PI * 2) / teeth;
      ctx.save();
      ctx.rotate(angle);
      ctx.fillRect(-6, -radius - 8, 12, 10);
      ctx.restore();
    }

    // Inner Glowing Cyan Hub
    ctx.shadowColor = '#38bdf8';
    ctx.shadowBlur = 15;
    ctx.fillStyle = '#38bdf8';
    ctx.beginPath();
    ctx.arc(0, 0, radius * 0.35, 0, Math.PI * 2);
    ctx.fill();

    ctx.restore();
  }

  /**
   * Render Wall-Mounted Neon Cyan Power Conduit Matrix
   */
  public static drawPowerConduitMatrix(
    ctx: CanvasRenderingContext2D,
    x: number,
    y: number,
    width: number,
    height: number
  ) {
    ctx.save();
    const time = Date.now() * 0.002;

    // Metallic Box Casing
    ctx.fillStyle = '#1e293b';
    ctx.fillRect(x, y, width, height);
    ctx.strokeStyle = '#475569';
    ctx.lineWidth = 2;
    ctx.strokeRect(x, y, width, height);

    // Glowing Neon Cyan Power Tubes
    ctx.shadowColor = '#22d3ee';
    ctx.shadowBlur = 15;
    ctx.strokeStyle = '#38bdf8';
    ctx.lineWidth = 3;

    for (let ty = y + 10; ty < y + height - 10; ty += 14) {
      const offset = Math.sin(time + ty) * 4;
      ctx.beginPath();
      ctx.moveTo(x + 5, ty);
      ctx.lineTo(x + width - 5 + offset, ty);
      ctx.stroke();
    }

    ctx.restore();
  }

  /**
   * Render Weathered Stone Gargoyle Statue
   */
  public static drawGargoyleStatue(
    ctx: CanvasRenderingContext2D,
    x: number,
    y: number,
    facingLeft: boolean = false
  ) {
    ctx.save();
    ctx.translate(x, y);
    if (facingLeft) ctx.scale(-1, 1);

    // Stone Perch
    ctx.fillStyle = '#1e293b';
    ctx.fillRect(-20, 0, 40, 15);

    // Body
    ctx.fillStyle = '#334155';
    ctx.beginPath();
    ctx.arc(0, -25, 14, 0, Math.PI * 2);
    ctx.fill();

    // Wings
    ctx.fillStyle = '#0f172a';
    ctx.beginPath();
    ctx.moveTo(-10, -25);
    ctx.lineTo(-35, -50);
    ctx.lineTo(-15, -10);
    ctx.fill();

    // Glowing Crimson Eyes
    ctx.fillStyle = '#ef4444';
    ctx.shadowColor = '#ef4444';
    ctx.shadowBlur = 8;
    ctx.fillRect(4, -28, 3, 3);

    ctx.restore();
  }

  /**
   * Render Save Shrine Altar with Floating Sapphire Energy Crystal
   */
  public static drawSaveAltar(
    ctx: CanvasRenderingContext2D,
    x: number,
    y: number
  ) {
    ctx.save();
    const time = Date.now() * 0.003;
    const floatY = Math.sin(time) * 6;

    // Pedestal Base
    ctx.fillStyle = '#0f172a';
    ctx.fillRect(x - 24, y - 15, 48, 15);
    ctx.fillStyle = '#334155';
    ctx.fillRect(x - 18, y - 35, 36, 20);

    // Floating Sapphire Crystal
    ctx.shadowColor = '#38bdf8';
    ctx.shadowBlur = 30;
    ctx.fillStyle = '#38bdf8';

    ctx.beginPath();
    ctx.moveTo(x, y - 65 + floatY);
    ctx.lineTo(x + 14, y - 48 + floatY);
    ctx.lineTo(x, y - 30 + floatY);
    ctx.lineTo(x - 14, y - 48 + floatY);
    ctx.closePath();
    ctx.fill();

    // Crystal Core
    ctx.fillStyle = '#ffffff';
    ctx.beginPath();
    ctx.arc(x, y - 48 + floatY, 4, 0, Math.PI * 2);
    ctx.fill();

    ctx.restore();
  }

  /**
   * Render Teleporter Portal with Rotating Arcane Energy Rings
   */
  public static drawTeleporterPortal(
    ctx: CanvasRenderingContext2D,
    x: number,
    y: number
  ) {
    ctx.save();
    const time = Date.now() * 0.002;

    ctx.shadowColor = '#c084fc';
    ctx.shadowBlur = 35;

    // Outer Vortex
    const vGrad = ctx.createRadialGradient(x, y - 30, 5, x, y - 30, 35);
    vGrad.addColorStop(0, '#f472b6');
    vGrad.addColorStop(0.5, '#c084fc');
    vGrad.addColorStop(1, 'transparent');

    ctx.fillStyle = vGrad;
    ctx.beginPath();
    ctx.arc(x, y - 30, 35, 0, Math.PI * 2);
    ctx.fill();

    // Rotating Energy Rings
    ctx.strokeStyle = '#e879f9';
    ctx.lineWidth = 2.5;
    for (let r = 0; r < 3; r++) {
      ctx.save();
      ctx.translate(x, y - 30);
      ctx.rotate(time * (r + 1) * 0.5);
      ctx.beginPath();
      ctx.ellipse(0, 0, 28 - r * 6, 12 - r * 2, 0, 0, Math.PI * 2);
      ctx.stroke();
      ctx.restore();
    }

    ctx.restore();
  }

  /**
   * Render Treasure Chest with Metallic Filigree
   */
  public static drawTreasureChest(
    ctx: CanvasRenderingContext2D,
    x: number,
    y: number,
    opened: boolean
  ) {
    ctx.save();
    ctx.translate(x, y);

    if (opened) {
      // Opened Chest Lid
      ctx.fillStyle = '#451a03';
      ctx.fillRect(-15, -12, 30, 12);
      ctx.fillStyle = '#fbbf24';
      ctx.shadowColor = '#fbbf24';
      ctx.shadowBlur = 20;
      ctx.fillRect(-12, -28, 24, 16);
    } else {
      // Closed Chest
      ctx.fillStyle = '#78350f';
      ctx.fillRect(-15, -20, 30, 20);

      // Gold Trim
      ctx.fillStyle = '#fbbf24';
      ctx.shadowColor = '#fbbf24';
      ctx.shadowBlur = 12;
      ctx.strokeRect(-15, -20, 30, 20);

      // Lock Gem
      ctx.fillStyle = '#38bdf8';
      ctx.fillRect(-3, -12, 6, 6);
    }

    ctx.restore();
  }

  /**
   * Render Foreground 3D Framing Archway / Pillars (Out-of-Focus Parallax Layer)
   * Gives a stunning Metroid Dread orthographic depth impression!
   */
  public static drawForegroundFraming(
    ctx: CanvasRenderingContext2D,
    width: number,
    height: number,
    cameraX: number
  ) {
    ctx.save();
    const fgParallaxX = cameraX * 0.3; // Moves faster than gameplay plane

    // Apply slight blur effect for camera depth-of-field
    ctx.filter = 'blur(3px)';

    // Left Foreground Archway Pillar
    const leftX = -200 + (fgParallaxX % 900);
    const archGrad = ctx.createLinearGradient(leftX, 0, leftX + 180, 0);
    archGrad.addColorStop(0, '#020617');
    archGrad.addColorStop(0.7, '#0f172a');
    archGrad.addColorStop(1, 'transparent');

    ctx.fillStyle = archGrad;
    ctx.beginPath();
    ctx.moveTo(leftX, 0);
    ctx.lineTo(leftX + 180, 0);
    ctx.quadraticCurveTo(leftX + 90, height * 0.4, leftX, height);
    ctx.closePath();
    ctx.fill();

    // Right Foreground Archway Pillar
    const rightX = width + 200 - (fgParallaxX % 900);
    const rArchGrad = ctx.createLinearGradient(rightX, 0, rightX - 180, 0);
    rArchGrad.addColorStop(0, '#020617');
    rArchGrad.addColorStop(0.7, '#0f172a');
    rArchGrad.addColorStop(1, 'transparent');

    ctx.fillStyle = rArchGrad;
    ctx.beginPath();
    ctx.moveTo(rightX, 0);
    ctx.lineTo(rightX - 180, 0);
    ctx.quadraticCurveTo(rightX - 90, height * 0.4, rightX, height);
    ctx.closePath();
    ctx.fill();

    // Hanging Iron Chains from Ceiling
    ctx.strokeStyle = '#1e293b';
    ctx.lineWidth = 4;
    for (let cx = 100; cx < width; cx += 350) {
      const chainX = cx - (fgParallaxX % 350);
      ctx.beginPath();
      ctx.moveTo(chainX, 0);
      ctx.lineTo(chainX, 120 + Math.sin(cx) * 40);
      ctx.stroke();

      // Hanging Iron Lantern at end of chain
      ctx.fillStyle = '#0f172a';
      ctx.fillRect(chainX - 10, 120 + Math.sin(cx) * 40, 20, 25);
      ctx.fillStyle = '#f59e0b';
      ctx.shadowColor = '#f59e0b';
      ctx.shadowBlur = 15;
      ctx.fillRect(chainX - 4, 128 + Math.sin(cx) * 40, 8, 10);
    }

    ctx.restore();
  }
}

export class ProceduralRenderer {
  /**
   * Render High-Fidelity Lord Belmont Character matching reference sheet:
   * Silver hair, black slate armor, crimson velvet cape, sapphire chest gem,
   * brushed metallic boots & gauntlets, rim highlights, and broadsword with energy trails.
   */
  public static drawPlayer(
    ctx: CanvasRenderingContext2D,
    x: number,
    y: number,
    facing: 'left' | 'right',
    animState: 'idle' | 'run' | 'jump' | 'fall' | 'slide' | 'attack' | 'dash' | 'hit',
    animFrame: number,
    equipped: {
      weapon: EquipmentItem | null;
      helmet: EquipmentItem | null;
      armor: EquipmentItem | null;
      cape: EquipmentItem | null;
    },
    capePoints: CapePoint[],
    hairPoints: HairParticle[],
    invulnerable: boolean = false
  ) {
    ctx.save();

    if (invulnerable && Math.floor(Date.now() / 60) % 2 === 0) {
      ctx.globalAlpha = 0.35;
    }

    const dir = facing === 'right' ? 1 : -1;
    ctx.translate(x, y);
    ctx.scale(dir, 1);

    // Color Palette matching reference sheet
    const armorColor = equipped.armor?.visualStyle?.armorColor || '#1e293b';
    const capeColor = equipped.cape?.visualStyle?.capeColor || '#991b1b';
    const helmetColor = equipped.helmet?.visualStyle?.armorColor || '#0f172a';
    const weaponColor = equipped.weapon?.visualStyle?.bladeColor || '#f8fafc';
    const weaponGlow = equipped.weapon?.visualStyle?.glowColor || '#38bdf8';

    let legOffset = Math.sin(animFrame * 0.4) * 10;
    let headOffset = 0;
    let armAngle = 0;

    if (animState === 'idle') {
      headOffset = Math.sin(Date.now() * 0.004) * 2;
      legOffset = 0;
    } else if (animState === 'jump') {
      legOffset = -8;
    } else if (animState === 'attack') {
      armAngle = Math.sin(animFrame * 0.5) * 0.7;
    }

    // --- 1. FLARED GOTHIC HIGH COLLAR (DARK OUTSIDE, CRIMSON RED INSIDE) ---
    ctx.save();
    // High flared collar behind head
    ctx.fillStyle = '#111625'; // Dark coat exterior
    ctx.beginPath();
    ctx.moveTo(-12, -36 + headOffset);
    ctx.lineTo(-18, -54 + headOffset);
    ctx.lineTo(-6, -46 + headOffset);
    ctx.closePath();
    ctx.fill();

    ctx.fillStyle = '#b91c1c'; // Crimson collar lining
    ctx.beginPath();
    ctx.moveTo(-11, -36 + headOffset);
    ctx.lineTo(-16, -52 + headOffset);
    ctx.lineTo(-6, -45 + headOffset);
    ctx.closePath();
    ctx.fill();

    ctx.fillStyle = '#111625';
    ctx.beginPath();
    ctx.moveTo(12, -36 + headOffset);
    ctx.lineTo(18, -54 + headOffset);
    ctx.lineTo(6, -46 + headOffset);
    ctx.closePath();
    ctx.fill();

    ctx.fillStyle = '#b91c1c';
    ctx.beginPath();
    ctx.moveTo(11, -36 + headOffset);
    ctx.lineTo(16, -52 + headOffset);
    ctx.lineTo(6, -45 + headOffset);
    ctx.closePath();
    ctx.fill();
    ctx.restore();

    // --- 2. BACK-SLUNG GREATSWORD (WHEN NOT ATTACKING) ---
    if (animState !== 'attack') {
      ctx.save();
      ctx.translate(-2, -30 + headOffset);
      ctx.rotate(-Math.PI / 3.8); // Diagonal back mount

      // Sword Blade behind cape
      ctx.fillStyle = '#cbd5e1';
      ctx.fillRect(-2, -35, 4, 42);

      // Ornate Guard & Diamond Sapphire Gem
      ctx.fillStyle = '#475569';
      ctx.fillRect(-8, -35, 16, 3);

      ctx.fillStyle = '#38bdf8';
      ctx.shadowColor = '#38bdf8';
      ctx.shadowBlur = 10;
      ctx.beginPath();
      ctx.moveTo(0, -38);
      ctx.lineTo(4, -35);
      ctx.lineTo(0, -32);
      ctx.lineTo(-4, -35);
      ctx.closePath();
      ctx.fill();
      ctx.shadowBlur = 0;

      // Handle & Pommel
      ctx.fillStyle = '#1e293b';
      ctx.fillRect(-1.5, -46, 3, 8);
      ctx.fillStyle = '#38bdf8';
      ctx.beginPath();
      ctx.arc(0, -47, 2.5, 0, Math.PI * 2);
      ctx.fill();

      ctx.restore();
    }

    // --- 3. CAPE RENDERING (VERLET CLOTH PHYSICS WITH CRIMSON VELVET SHADING) ---
    if (capePoints.length >= 4) {
      ctx.save();
      const capeGrad = ctx.createLinearGradient(0, -30, 0, 20);
      capeGrad.addColorStop(0, '#111625'); // Dark outer coat
      capeGrad.addColorStop(0.3, capeColor); // Crimson lining
      capeGrad.addColorStop(0.8, '#7f1d1d');
      capeGrad.addColorStop(1, '#020617');

      ctx.fillStyle = capeGrad;
      ctx.beginPath();
      ctx.moveTo(-8, -32 + headOffset);
      ctx.lineTo(8, -32 + headOffset);

      for (let i = 0; i < capePoints.length; i++) {
        const relX = (capePoints[i].x - x) * dir;
        const relY = capePoints[i].y - y;
        ctx.lineTo(relX, relY);
      }
      ctx.closePath();
      ctx.fill();

      // Gold Filigree Border Lining
      ctx.strokeStyle = '#f59e0b';
      ctx.lineWidth = 1.5;
      ctx.shadowColor = '#f59e0b';
      ctx.shadowBlur = 4;
      ctx.stroke();
      ctx.restore();
    }

    // --- 4. LEGS & ARMORED BOOTS ---
    ctx.fillStyle = '#1a1d28'; // Dark Slate/Indigo Suit Trousers
    ctx.fillRect(-8 + (animState === 'run' ? legOffset : 0), -14, 7, 14);
    ctx.fillRect(1 - (animState === 'run' ? legOffset : 0), -14, 7, 14);

    // Silver Seam Highlights on Suit
    ctx.strokeStyle = '#64748b';
    ctx.lineWidth = 1;
    ctx.beginPath();
    ctx.moveTo(-5, -14);
    ctx.lineTo(-5, -2);
    ctx.moveTo(4, -14);
    ctx.lineTo(4, -2);
    ctx.stroke();

    // Brushed Steel Boots with Specular Highlights
    const bootGrad = ctx.createLinearGradient(-10, -8, 10, 0);
    bootGrad.addColorStop(0, '#334155');
    bootGrad.addColorStop(0.5, '#64748b');
    bootGrad.addColorStop(1, '#0f172a');
    ctx.fillStyle = bootGrad;
    ctx.fillRect(-10 + (animState === 'run' ? legOffset : 0), -6, 9, 6);
    ctx.fillRect(0 - (animState === 'run' ? legOffset : 0), -6, 9, 6);

    ctx.fillStyle = '#f8fafc';
    ctx.fillRect(-9 + (animState === 'run' ? legOffset : 0), -6, 7, 1.5);

    // --- 5. TORSO & CUIRASS (BLACK/INDIGO PLATE ARMOR WITH SILVER FILIGREE & SAPPHIRE BROOCH) ---
    const armorGrad = ctx.createLinearGradient(-10, -34, 10, -12);
    armorGrad.addColorStop(0, '#212638');
    armorGrad.addColorStop(0.5, '#333b52');
    armorGrad.addColorStop(1, '#111625');
    ctx.fillStyle = armorGrad;
    ctx.fillRect(-10, -34 + headOffset, 20, 20);

    // Silver Filigree Scrollwork on Chest
    ctx.strokeStyle = '#94a3b8';
    ctx.lineWidth = 1.2;
    ctx.beginPath();
    ctx.arc(-4, -28 + headOffset, 3, Math.PI * 0.5, Math.PI * 1.5);
    ctx.arc(4, -28 + headOffset, 3, Math.PI * 1.5, Math.PI * 0.5);
    ctx.stroke();

    // ANGULAR POINTED PAULDRONS (SHOULDER GUARDS - MATCHING REF SHEET)
    ctx.fillStyle = '#212638';
    ctx.strokeStyle = '#94a3b8';
    ctx.lineWidth = 1.5;

    // Left Pauldron (Pointed Upward)
    ctx.beginPath();
    ctx.moveTo(-10, -32 + headOffset);
    ctx.lineTo(-18, -42 + headOffset);
    ctx.lineTo(-16, -28 + headOffset);
    ctx.closePath();
    ctx.fill();
    ctx.stroke();

    // Right Pauldron (Pointed Upward)
    ctx.beginPath();
    ctx.moveTo(10, -32 + headOffset);
    ctx.lineTo(18, -42 + headOffset);
    ctx.lineTo(16, -28 + headOffset);
    ctx.closePath();
    ctx.fill();
    ctx.stroke();

    // Glowing Diamond Sapphire Chest Gem Brooch
    ctx.fillStyle = '#38bdf8';
    ctx.shadowColor = '#38bdf8';
    ctx.shadowBlur = 14;
    ctx.beginPath();
    ctx.moveTo(0, -30 + headOffset);
    ctx.lineTo(4, -26 + headOffset);
    ctx.lineTo(0, -22 + headOffset);
    ctx.lineTo(-4, -26 + headOffset);
    ctx.closePath();
    ctx.fill();
    ctx.shadowBlur = 0;

    // Belt & Silver Buckle
    ctx.fillStyle = '#0f172a';
    ctx.fillRect(-10, -14 + headOffset, 20, 4);
    ctx.fillStyle = '#cbd5e1';
    ctx.fillRect(-3, -14 + headOffset, 6, 4);

    // --- 6. HEAD, FACE & LONG SILKY FLOWING SILVER HAIR ---
    ctx.fillStyle = '#fce7f3'; // Aristocratic Pale Skin
    ctx.fillRect(-5, -44 + headOffset, 10, 10);

    // Stern Eyebrows & Piercing Blue Vampiric Eyes
    ctx.fillStyle = '#1e293b';
    ctx.fillRect(1, -43 + headOffset, 4, 1); // Eyebrow

    ctx.fillStyle = '#38bdf8';
    ctx.shadowColor = '#38bdf8';
    ctx.shadowBlur = 8;
    ctx.fillRect(2, -41 + headOffset, 2.5, 2.5);
    ctx.shadowBlur = 0;

    // Multi-Layer Long Silver Hair past Waist
    ctx.fillStyle = '#f8fafc';
    ctx.beginPath();
    ctx.arc(-1, -44 + headOffset, 7.5, Math.PI, 0);
    ctx.fill();

    // Side Hair Strands Framing Face
    ctx.fillStyle = '#e2e8f0';
    ctx.fillRect(-7, -42 + headOffset, 3, 16);
    ctx.fillRect(4, -42 + headOffset, 3, 16);

    if (hairPoints.length >= 3) {
      ctx.strokeStyle = '#f8fafc';
      ctx.lineWidth = 4;
      ctx.beginPath();
      ctx.moveTo(-3, -44 + headOffset);
      for (let i = 0; i < hairPoints.length; i++) {
        const hx = (hairPoints[i].x - x) * dir;
        const hy = hairPoints[i].y - y;
        ctx.lineTo(hx, hy);
      }
      ctx.stroke();

      // Hair Highlight
      ctx.strokeStyle = '#ffffff';
      ctx.lineWidth = 1.5;
      ctx.stroke();
    }

    if (equipped.helmet) {
      ctx.fillStyle = helmetColor;
      ctx.fillRect(-8, -48 + headOffset, 16, 7);
      ctx.fillStyle = '#fbbf24';
      ctx.fillRect(-3, -51 + headOffset, 6, 4);
    }

    // --- 7. ARMS & WEAPON IN HAND (WHEN ATTACKING) ---
    ctx.save();
    ctx.translate(6, -28 + headOffset);
    ctx.rotate(armAngle);

    ctx.fillStyle = '#212638';
    ctx.fillRect(-3, 0, 6, 14);

    // Silver Gauntlet
    ctx.fillStyle = '#94a3b8';
    ctx.fillRect(-3.5, 8, 7, 6);

    if (animState === 'attack') {
      this.drawWeaponInHand(ctx, equipped.weapon, weaponColor, weaponGlow, animState, animFrame);
    }

    ctx.restore();

    // METROID DREAD SILHOUETTE RIM LIGHTING
    ctx.strokeStyle = 'rgba(186, 230, 253, 0.35)';
    ctx.lineWidth = 1.2;
    ctx.strokeRect(-11, -48 + headOffset, 22, 48);

    ctx.restore();
  }

  /**
   * Draw High-Detail Weapon in Hand
   */
  private static drawWeaponInHand(
    ctx: CanvasRenderingContext2D,
    weapon: EquipmentItem | null,
    color: string,
    glow: string,
    animState: string,
    animFrame: number
  ) {
    const type: WeaponType = weapon?.weaponType || 'greatsword';

    ctx.save();
    ctx.shadowColor = glow;
    ctx.shadowBlur = 14;

    if (type === 'greatsword') {
      ctx.fillStyle = '#78350f';
      ctx.fillRect(-1, 8, 4, 10);
      ctx.fillStyle = '#fbbf24';
      ctx.beginPath();
      ctx.arc(1, 19, 4, 0, Math.PI * 2);
      ctx.fill();

      ctx.fillStyle = '#f59e0b';
      ctx.fillRect(-10, 6, 22, 4);

      const bladeGrad = ctx.createLinearGradient(-4, -45, 6, -45);
      bladeGrad.addColorStop(0, '#e2e8f0');
      bladeGrad.addColorStop(0.5, '#ffffff');
      bladeGrad.addColorStop(1, '#94a3b8');
      ctx.fillStyle = bladeGrad;
      ctx.fillRect(-4, -42, 10, 48);

      ctx.fillStyle = glow;
      ctx.fillRect(0, -35, 2, 30);

      ctx.beginPath();
      ctx.moveTo(-4, -42);
      ctx.lineTo(1, -52);
      ctx.lineTo(6, -42);
      ctx.fill();
    } else if (type === 'whip') {
      ctx.fillStyle = '#78350f';
      ctx.fillRect(-1, 8, 4, 8);
      ctx.strokeStyle = color;
      ctx.lineWidth = 4;
      ctx.beginPath();
      ctx.arc(1, 16, 10, 0, Math.PI * 1.5);
      ctx.stroke();
    } else if (type === 'scythe') {
      ctx.fillStyle = '#334155';
      ctx.fillRect(0, -35, 4, 52);
      ctx.fillStyle = color;
      ctx.beginPath();
      ctx.moveTo(2, -35);
      ctx.quadraticCurveTo(35, -55, 40, -10);
      ctx.quadraticCurveTo(20, -30, 2, -22);
      ctx.fill();
    }

    ctx.restore();
  }

  /**
   * Draw Weapon Swing Arc with Motion Blur & Energy Flames
   */
  public static drawWeaponArc(
    ctx: CanvasRenderingContext2D,
    x: number,
    y: number,
    facing: 'left' | 'right',
    weaponType: WeaponType,
    color: string,
    progress: number
  ) {
    ctx.save();
    const dir = facing === 'right' ? 1 : -1;
    ctx.translate(x, y);
    ctx.scale(dir, 1);

    const radius = weaponType === 'greatsword' ? 75 : weaponType === 'scythe' ? 90 : 60;
    const startAngle = -Math.PI * 0.75 + progress * Math.PI * 1.3;
    const endAngle = startAngle + 0.65;

    ctx.shadowColor = color;
    ctx.shadowBlur = 25;
    ctx.strokeStyle = color;
    ctx.lineWidth = 14 * (1 - progress);

    ctx.beginPath();
    ctx.arc(10, -15, radius, startAngle, endAngle);
    ctx.stroke();

    ctx.strokeStyle = '#ffffff';
    ctx.lineWidth = 4.5 * (1 - progress);
    ctx.beginPath();
    ctx.arc(10, -15, radius - 4, startAngle, endAngle);
    ctx.stroke();

    ctx.restore();
  }

  /**
   * Draw High-Fidelity Enemies & Bosses
   */
  public static drawEnemy(
    ctx: CanvasRenderingContext2D,
    x: number,
    y: number,
    width: number,
    height: number,
    spriteType: string,
    facing: 'left' | 'right',
    hpRatio: number,
    state: string,
    phase: number = 1
  ) {
    ctx.save();
    const dir = facing === 'right' ? 1 : -1;
    ctx.translate(x + width / 2, y + height / 2);
    ctx.scale(dir, 1);

    if (spriteType === 'skeleton') {
      ctx.shadowColor = '#ef4444';
      ctx.shadowBlur = 10;

      ctx.fillStyle = '#e2e8f0';
      ctx.fillRect(-7, -22, 14, 12);
      ctx.fillStyle = '#000000';
      ctx.fillRect(2, -19, 3.5, 3.5);
      ctx.fillStyle = '#ef4444';
      ctx.fillRect(3, -18, 1.5, 1.5);

      ctx.fillStyle = '#94a3b8';
      ctx.fillRect(-6, -10, 12, 12);
      ctx.fillStyle = '#475569';
      ctx.fillRect(-7, -8, 14, 8);
    } else if (spriteType === 'bat') {
      ctx.fillStyle = '#334155';
      ctx.beginPath();
      ctx.arc(0, 0, 7, 0, Math.PI * 2);
      ctx.fill();

      const wingY = Math.sin(Date.now() * 0.02) * 12;
      ctx.fillStyle = '#1e293b';
      ctx.beginPath();
      ctx.moveTo(-4, -2);
      ctx.quadraticCurveTo(-15, wingY - 10, -22, wingY);
      ctx.quadraticCurveTo(-10, 5, -4, 4);
      ctx.fill();

      ctx.beginPath();
      ctx.moveTo(4, -2);
      ctx.quadraticCurveTo(15, wingY - 10, 22, wingY);
      ctx.quadraticCurveTo(10, 5, 4, 4);
      ctx.fill();
    } else if (spriteType === 'specter') {
      ctx.shadowColor = '#38bdf8';
      ctx.shadowBlur = 22;
      ctx.fillStyle = 'rgba(56, 189, 248, 0.85)';
      ctx.beginPath();
      ctx.arc(0, -12, 14, Math.PI, 0);
      ctx.lineTo(14, 20);
      ctx.quadraticCurveTo(0, 30, -14, 20);
      ctx.closePath();
      ctx.fill();
    } else if (spriteType === 'gargoyle') {
      ctx.shadowColor = '#64748b';
      ctx.shadowBlur = 12;
      ctx.fillStyle = '#475569';
      ctx.fillRect(-10, -20, 20, 24);
      ctx.beginPath();
      ctx.moveTo(-8, -15);
      ctx.lineTo(-24, -30);
      ctx.lineTo(-12, -5);
      ctx.fill();
      ctx.beginPath();
      ctx.moveTo(8, -15);
      ctx.lineTo(24, -30);
      ctx.lineTo(12, -5);
      ctx.fill();
      ctx.fillStyle = '#f59e0b';
      ctx.fillRect(-4, -16, 3, 3);
      ctx.fillRect(1, -16, 3, 3);
    } else if (spriteType === 'clockwork_drone') {
      ctx.shadowColor = '#f59e0b';
      ctx.shadowBlur = 18;
      ctx.fillStyle = '#b45309';
      ctx.beginPath();
      ctx.arc(0, -5, 14, 0, Math.PI * 2);
      ctx.fill();
      ctx.strokeStyle = '#fbbf24';
      ctx.lineWidth = 2.5;
      ctx.stroke();
    } else if (spriteType === 'boss_belmont') {
      ctx.shadowColor = '#dc2626';
      ctx.shadowBlur = 45;

      const cGrad = ctx.createLinearGradient(-22, -55, 22, 10);
      cGrad.addColorStop(0, '#450a0a');
      cGrad.addColorStop(0.5, '#7f1d1d');
      cGrad.addColorStop(1, '#020617');

      ctx.fillStyle = cGrad;
      ctx.fillRect(-22, -55, 44, 65);

      ctx.strokeStyle = '#fbbf24';
      ctx.lineWidth = 3;
      ctx.strokeRect(-22, -55, 44, 65);

      if (phase >= 2) {
        ctx.fillStyle = '#7f1d1d';
        ctx.beginPath();
        ctx.moveTo(-18, -35);
        ctx.quadraticCurveTo(-70, -80, -80, -25);
        ctx.quadraticCurveTo(-45, 5, -18, -12);
        ctx.fill();

        ctx.beginPath();
        ctx.moveTo(18, -35);
        ctx.quadraticCurveTo(70, -80, 80, -25);
        ctx.quadraticCurveTo(45, 5, 18, -12);
        ctx.fill();
      }
    } else {
      // Default Boss / Elite Enemy Visual
      ctx.shadowColor = '#ef4444';
      ctx.shadowBlur = 25;
      ctx.fillStyle = '#7f1d1d';
      ctx.fillRect(-20, -35, 40, 45);
      ctx.fillStyle = '#fbbf24';
      ctx.fillRect(-8, -25, 16, 4);
    }

    ctx.restore();
  }
}
