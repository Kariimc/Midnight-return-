/**
 * Belmont's Curse: Shadow Reign - Volumetric Lighting & Atmospheric Post-Processing
 * God rays, rolling ground mist/fog, bloom glow passes, dark vignette, and radial lights.
 */

import { DynamicLight } from '../types/game';

export class LightingEngine {
  public static renderLighting(
    ctx: CanvasRenderingContext2D,
    width: number,
    height: number,
    ambientColor: string,
    lights: DynamicLight[]
  ) {
    // --- 1. VOLUMETRIC GOD RAYS (STAINED GLASS LIGHT SHAFTS) ---
    ctx.save();
    ctx.globalCompositeOperation = 'lighter';
    const time = Date.now() * 0.0008;

    for (let i = 0; i < 3; i++) {
      const rayAngle = Math.PI * 0.25 + Math.sin(time + i) * 0.02;
      const rayWidth = 120 + i * 40;
      const rayX = 300 + i * 450;

      const rayGrad = ctx.createLinearGradient(rayX, 0, rayX + Math.cos(rayAngle) * height, height);
      rayGrad.addColorStop(0, 'rgba(56, 189, 248, 0.18)');
      rayGrad.addColorStop(0.5, 'rgba(168, 85, 247, 0.08)');
      rayGrad.addColorStop(1, 'rgba(0, 0, 0, 0)');

      ctx.fillStyle = rayGrad;
      ctx.beginPath();
      ctx.moveTo(rayX - rayWidth / 2, 0);
      ctx.lineTo(rayX + rayWidth / 2, 0);
      ctx.lineTo(rayX + rayWidth / 2 + Math.cos(rayAngle) * height, height);
      ctx.lineTo(rayX - rayWidth / 2 + Math.cos(rayAngle) * height, height);
      ctx.closePath();
      ctx.fill();
    }
    ctx.restore();

    // --- 2. AMBIENT DARKNESS MASK LAYER ---
    ctx.save();
    ctx.fillStyle = ambientColor;
    ctx.fillRect(0, 0, width, height);

    // Cut light holes out of the darkness mask
    ctx.globalCompositeOperation = 'destination-out';

    lights.forEach((light) => {
      const radius = light.flicker
        ? light.radius + Math.sin(Date.now() * 0.01 + light.x) * 6
        : light.radius;

      const gradient = ctx.createRadialGradient(
        light.x,
        light.y,
        0,
        light.x,
        light.y,
        radius
      );

      gradient.addColorStop(0, `rgba(255, 255, 255, ${light.intensity})`);
      gradient.addColorStop(0.5, `rgba(255, 255, 255, ${light.intensity * 0.5})`);
      gradient.addColorStop(1, 'rgba(255, 255, 255, 0)');

      ctx.fillStyle = gradient;
      ctx.beginPath();
      ctx.arc(light.x, light.y, radius, 0, Math.PI * 2);
      ctx.fill();
    });

    ctx.restore();

    // --- 3. COLORED LIGHT GLOW OVERLAYS (FIRE & MAGIC) ---
    ctx.save();
    ctx.globalCompositeOperation = 'lighter';
    lights.forEach((light) => {
      if (light.color && light.color !== '#ffffff') {
        const radius = light.radius * 1.2;
        const gradient = ctx.createRadialGradient(
          light.x,
          light.y,
          0,
          light.x,
          light.y,
          radius
        );

        gradient.addColorStop(0, light.color);
        gradient.addColorStop(1, 'transparent');

        ctx.fillStyle = gradient;
        ctx.beginPath();
        ctx.arc(light.x, light.y, radius, 0, Math.PI * 2);
        ctx.fill();
      }
    });
    ctx.restore();

    // --- 4. ROLLING ATMOSPHERIC GROUND FOG ---
    ctx.save();
    ctx.globalCompositeOperation = 'screen';
    const fogY = height - 120;
    const fogGrad = ctx.createLinearGradient(0, fogY, 0, height);
    fogGrad.addColorStop(0, 'rgba(0,0,0,0)');
    fogGrad.addColorStop(0.5, 'rgba(56, 189, 248, 0.08)');
    fogGrad.addColorStop(1, 'rgba(30, 41, 59, 0.25)');

    ctx.fillStyle = fogGrad;
    ctx.fillRect(0, fogY, width, 120);
    ctx.restore();

    // --- 5. CINEMATIC DARK VIGNETTE EDGES ---
    ctx.save();
    const vigGrad = ctx.createRadialGradient(
      width / 2,
      height / 2,
      Math.min(width, height) * 0.4,
      width / 2,
      height / 2,
      Math.max(width, height) * 0.8
    );
    vigGrad.addColorStop(0, 'rgba(0, 0, 0, 0)');
    vigGrad.addColorStop(1, 'rgba(2, 6, 23, 0.85)');

    ctx.fillStyle = vigGrad;
    ctx.fillRect(0, 0, width, height);
    ctx.restore();
  }
}

