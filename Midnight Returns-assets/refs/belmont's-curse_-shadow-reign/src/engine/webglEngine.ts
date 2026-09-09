/**
 * Belmont's Curse: Shadow Reign - WebGL 2.5D Metroid Dread Engine Renderer
 * Built with Three.js (PBR materials, dynamic 3D lighting, PCF soft shadows,
 * volumetric fog, 2.5D parallax depth, glowing neon cyan power conduits,
 * and dynamic 3D particle systems).
 */

import * as THREE from 'three';
import { GameEngine } from './gameEngine';
import { EquipmentItem, WeaponType } from '../types/game';
import { TextureGenerator } from '../utils/textureGenerator';
import {
  CharacterSkeletalRig,
  BossExecutionerRig,
  SkeletonWarriorRig,
  FlyingGargoyleRig,
  ArachnidCrawlerRig,
} from './skeletalRig';

export class WebGL3DRenderer {
  private container: HTMLElement;
  private renderer: THREE.WebGLRenderer;
  private scene: THREE.Scene;
  private camera: THREE.PerspectiveCamera;

  // Lighting
  private ambientLight: THREE.AmbientLight;
  private dirLight: THREE.DirectionalLight;
  private playerPointLight: THREE.PointLight;
  private pointLightPool: THREE.PointLight[] = [];

  // PBR Materials Cache
  private materials: { [key: string]: THREE.Material } = {};
  private geometries: { [key: string]: THREE.BufferGeometry } = {};

  // World Meshes
  private platformGroup: THREE.Group = new THREE.Group();
  private backgroundGroup: THREE.Group = new THREE.Group();
  private foregroundGroup: THREE.Group = new THREE.Group();
  private enemyGroup: THREE.Group = new THREE.Group();
  private rotatingGears: THREE.Mesh[] = [];
  private enemyMeshMap: Map<string, { group: THREE.Group; rig: any }> = new Map();
  private particlePoints!: THREE.Points;
  private particlePositions!: Float32Array;
  private particleColors!: Float32Array;
  private particleCount = 600;

  // Player Mesh Assembly
  private playerGroup: THREE.Group = new THREE.Group();
  private playerRig!: CharacterSkeletalRig;
  private capeMesh!: THREE.Mesh;
  private capeGeometry!: THREE.PlaneGeometry;
  private weaponTrailMesh!: THREE.Mesh;
  private gemLight!: THREE.PointLight;

  // Camera Dynamics & Shake
  private cameraShakeIntensity = 0;
  private targetCameraZ = 700;

  // State Tracking
  private currentRoomId: string = '';
  private currentZoneId: string = '';
  private isInitialized = false;

  constructor(container: HTMLElement) {
    this.container = container;

    // 1. Initialize WebGL Renderer with ACES Filmic Tone Mapping & Soft Shadows
    this.renderer = new THREE.WebGLRenderer({ antialias: true, alpha: false, powerPreference: 'high-performance' });
    this.renderer.setSize(window.innerWidth, window.innerHeight);
    this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    this.renderer.shadowMap.enabled = true;
    this.renderer.shadowMap.type = THREE.PCFSoftShadowMap;
    this.renderer.toneMapping = THREE.ACESFilmicToneMapping;
    this.renderer.toneMappingExposure = 1.25;

    this.container.appendChild(this.renderer.domElement);

    // 2. Initialize Scene & Camera (2.5D Perspective View)
    this.scene = new THREE.Scene();
    this.scene.fog = new THREE.FogExp2('#020617', 0.0012);

    this.camera = new THREE.PerspectiveCamera(45, window.innerWidth / window.innerHeight, 1, 3000);
    this.camera.position.set(0, 0, 700);

    // 3. Global Lights
    this.ambientLight = new THREE.AmbientLight('#1e293b', 0.8);
    this.scene.add(this.ambientLight);

    this.dirLight = new THREE.DirectionalLight('#e0f2fe', 1.8);
    this.dirLight.position.set(300, 600, 400);
    this.dirLight.castShadow = true;
    this.dirLight.shadow.mapSize.width = 2048;
    this.dirLight.shadow.mapSize.height = 2048;
    this.dirLight.shadow.camera.near = 10;
    this.dirLight.shadow.camera.far = 1500;
    this.dirLight.shadow.bias = -0.0005;
    this.scene.add(this.dirLight);

    // Dynamic Player Light (Glows from Sapphire Gem & Weapon)
    this.playerPointLight = new THREE.PointLight('#38bdf8', 3.0, 350);
    this.playerPointLight.castShadow = true;
    this.scene.add(this.playerPointLight);

    // Point Light Pool for torches, projectiles, and energy conduits
    for (let i = 0; i < 20; i++) {
      const pl = new THREE.PointLight('#38bdf8', 0, 200);
      pl.visible = false;
      this.scene.add(pl);
      this.pointLightPool.push(pl);
    }

    // 4. Add Groups to Scene
    this.scene.add(this.backgroundGroup);
    this.scene.add(this.platformGroup);
    this.scene.add(this.playerGroup);
    this.scene.add(this.enemyGroup);
    this.scene.add(this.foregroundGroup);

    this.initSharedMaterials();
    this.buildPlayerMesh();
    this.initParticles();

    this.isInitialized = true;

    window.addEventListener('resize', this.onWindowResize);
  }

  private onWindowResize = () => {
    if (!this.renderer || !this.camera) return;
    this.camera.aspect = window.innerWidth / window.innerHeight;
    this.camera.updateProjectionMatrix();
    this.renderer.setSize(window.innerWidth, window.innerHeight);
  };

  /**
   * Initialize PBR Materials matching Metroid Dread & Lord Belmont visual sheet
   */
  private initSharedMaterials() {
    const stoneTex = TextureGenerator.createStoneTexture();
    const stoneNormal = TextureGenerator.createStoneNormalMap();
    const conduitEmissive = TextureGenerator.createConduitEmissiveMap();
    const stainedGlassTex = TextureGenerator.createStainedGlassTexture();

    // 1. Dark Gothic Slate Stone (PBR with Metallic Roughness & Normal Maps)
    this.materials.stonePlatform = new THREE.MeshStandardMaterial({
      color: new THREE.Color('#cbd5e1'),
      map: stoneTex,
      normalMap: stoneNormal,
      normalScale: new THREE.Vector2(1.5, 1.5),
      roughness: 0.65,
      metalness: 0.2,
    });

    // 2. Metallic Dark Industrial Steel with AAA Grating Texture Map
    const metalGratingTex = TextureGenerator.createMetalGratingTexture();
    this.materials.metalPlatform = new THREE.MeshStandardMaterial({
      map: metalGratingTex,
      color: new THREE.Color('#334155'),
      normalMap: stoneNormal,
      normalScale: new THREE.Vector2(0.8, 0.8),
      roughness: 0.25,
      metalness: 0.9,
    });

    // 3. Glowing Neon Cyan Electric Conduit (Metroid Dread Signatures)
    this.materials.neonCyan = new THREE.MeshStandardMaterial({
      color: new THREE.Color('#22d3ee'),
      emissive: new THREE.Color('#06b6d4'),
      emissiveMap: conduitEmissive,
      emissiveIntensity: 3.5,
      roughness: 0.1,
      metalness: 0.9,
    });

    // 4. Stained Glass Material
    this.materials.stainedGlass = new THREE.MeshStandardMaterial({
      map: stainedGlassTex,
      emissive: new THREE.Color('#38bdf8'),
      emissiveMap: stainedGlassTex,
      emissiveIntensity: 1.2,
      roughness: 0.2,
      metalness: 0.1,
      transparent: true,
      opacity: 0.85,
    });

    // Volumetric God Ray Light Shaft
    this.materials.godRay = new THREE.MeshBasicMaterial({
      color: new THREE.Color('#38bdf8'),
      transparent: true,
      opacity: 0.12,
      blending: THREE.AdditiveBlending,
      side: THREE.DoubleSide,
    });

    // Weapon Arc Trail
    this.materials.weaponTrail = new THREE.MeshBasicMaterial({
      color: new THREE.Color('#38bdf8'),
      transparent: true,
      opacity: 0.0,
      blending: THREE.AdditiveBlending,
      side: THREE.DoubleSide,
    });

    // 5. Crimson Velvet Cape Material
    this.materials.crimsonCape = new THREE.MeshStandardMaterial({
      color: new THREE.Color('#991b1b'),
      roughness: 0.8,
      metalness: 0.1,
      side: THREE.DoubleSide,
    });

    // 6. Silver Armor Material
    this.materials.silverArmor = new THREE.MeshStandardMaterial({
      color: new THREE.Color('#f1f5f9'),
      roughness: 0.15,
      metalness: 0.95,
    });

    // 7. Sapphire Gem Material
    this.materials.sapphire = new THREE.MeshPhysicalMaterial({
      color: new THREE.Color('#38bdf8'),
      emissive: new THREE.Color('#0284c7'),
      emissiveIntensity: 2.8,
      roughness: 0.1,
      metalness: 0.2,
      transmission: 0.9,
      ior: 1.5,
    });

    // 8. Gold Trim Filigree
    this.materials.goldTrim = new THREE.MeshStandardMaterial({
      color: new THREE.Color('#fbbf24'),
      roughness: 0.3,
      metalness: 0.9,
    });

    // 9. Cyber Red Glowing Sensor Eye
    this.materials.cyberRedEye = new THREE.MeshStandardMaterial({
      color: new THREE.Color('#ef4444'),
      emissive: new THREE.Color('#f87171'),
      emissiveIntensity: 4.0,
      roughness: 0.1,
      metalness: 0.9,
    });

    // 10. Clockwork Brass Machinery Gear
    this.materials.gearBrass = new THREE.MeshStandardMaterial({
      color: new THREE.Color('#d97706'),
      roughness: 0.35,
      metalness: 0.85,
    });

    // 11. Heavy Dark Iron Plate
    this.materials.darkSteel = new THREE.MeshStandardMaterial({
      map: TextureGenerator.createSciFiPanelTexture(),
      color: new THREE.Color('#1e293b'),
      roughness: 0.4,
      metalness: 0.9,
    });

    // Geometries
    this.geometries.box = new THREE.BoxGeometry(1, 1, 1);
    this.geometries.cylinder = new THREE.CylinderGeometry(1, 1, 1, 16);
    this.geometries.sphere = new THREE.SphereGeometry(1, 16, 16);
  }

  /**
   * Build 3D Lord Belmont Skinned Mesh & Skeletal Hierarchy
   */
  private buildPlayerMesh() {
    this.playerRig = new CharacterSkeletalRig();
    this.playerGroup.add(this.playerRig.group);

    // Sapphire Chest Light
    this.gemLight = new THREE.PointLight('#38bdf8', 2.0, 100);
    this.gemLight.position.set(0, 4, 12);
    this.playerRig.bones.chest.add(this.gemLight);

    // Billowing Crimson Velvet Cape Mesh attached to Spine
    this.capeGeometry = new THREE.PlaneGeometry(32, 50, 10, 10);
    this.capeMesh = new THREE.Mesh(this.capeGeometry, this.materials.crimsonCape);
    this.capeMesh.position.set(0, -6, -8);
    this.capeMesh.castShadow = true;
    this.playerRig.bones.spine.add(this.capeMesh);

    // Weapon Arc Slash Motion Trail
    const arcGeo = new THREE.RingGeometry(20, 70, 20, 1, 0, Math.PI * 0.75);
    this.weaponTrailMesh = new THREE.Mesh(arcGeo, this.materials.weaponTrail);
    this.weaponTrailMesh.rotation.y = Math.PI / 2;
    this.weaponTrailMesh.position.set(10, 10, 0);
    this.playerGroup.add(this.weaponTrailMesh);
  }

  /**
   * Initialize 3D Instanced Particles for Rain, Embers, Sparkles
   */
  private initParticles() {
    const geo = new THREE.BufferGeometry();
    this.particlePositions = new Float32Array(this.particleCount * 3);
    this.particleColors = new Float32Array(this.particleCount * 3);

    for (let i = 0; i < this.particleCount; i++) {
      this.particlePositions[i * 3] = (Math.random() - 0.5) * 1600;
      this.particlePositions[i * 3 + 1] = (Math.random() - 0.5) * 1000;
      this.particlePositions[i * 3 + 2] = (Math.random() - 0.5) * 300;

      this.particleColors[i * 3] = 0.22;
      this.particleColors[i * 3 + 1] = 0.82;
      this.particleColors[i * 3 + 2] = 0.93;
    }

    geo.setAttribute('position', new THREE.BufferAttribute(this.particlePositions, 3));
    geo.setAttribute('color', new THREE.BufferAttribute(this.particleColors, 3));

    const pMat = new THREE.PointsMaterial({
      size: 3.5,
      vertexColors: true,
      transparent: true,
      opacity: 0.75,
      blending: THREE.AdditiveBlending,
    });

    this.particlePoints = new THREE.Points(geo, pMat);
    this.scene.add(this.particlePoints);
  }

  /**
   * Rebuild Room 3D World Geometry when changing rooms
   */
  public rebuildRoom(engine: GameEngine) {
    const room = engine.getCurrentRoom();
    const zone = engine.getCurrentZone();
    if (!room || !zone) return;

    this.currentRoomId = room.id;
    this.currentZoneId = zone.id;

    // Clear old meshes
    while (this.platformGroup.children.length > 0) {
      this.platformGroup.remove(this.platformGroup.children[0]);
    }
    while (this.backgroundGroup.children.length > 0) {
      this.backgroundGroup.remove(this.backgroundGroup.children[0]);
    }
    while (this.foregroundGroup.children.length > 0) {
      this.foregroundGroup.remove(this.foregroundGroup.children[0]);
    }

    // Set Biome Fog & Background Color
    if (zone.id === 'zone_courtyard' || zone.id === 'zone_cathedral') {
      this.scene.fog = new THREE.FogExp2('#020617', 0.001);
      this.scene.background = new THREE.Color('#020617');
    } else if (zone.id === 'zone_forge') {
      this.scene.fog = new THREE.FogExp2('#1a0804', 0.0015);
      this.scene.background = new THREE.Color('#0f0402');
    } else if (zone.id === 'zone_aqueduct') {
      this.scene.fog = new THREE.FogExp2('#041628', 0.0012);
      this.scene.background = new THREE.Color('#020b14');
    } else {
      this.scene.fog = new THREE.FogExp2('#090514', 0.0015);
      this.scene.background = new THREE.Color('#05020a');
    }

    // 1. Build 3D Platforms with PBR Stone/Metal and Neon Cyan Energy Conduits
    room.platforms.forEach((p) => {
      // Platform Body
      const pMesh = new THREE.Mesh(this.geometries.box, this.materials.stonePlatform);
      pMesh.scale.set(p.width, p.height, 60);
      pMesh.position.set(p.x + p.width / 2, -p.y - p.height / 2, -30);
      pMesh.receiveShadow = true;
      pMesh.castShadow = true;
      this.platformGroup.add(pMesh);

      // Glowing Neon Cyan Power Conduit Strip along top front edge
      const cMesh = new THREE.Mesh(this.geometries.box, this.materials.neonCyan);
      cMesh.scale.set(p.width - 8, 4, 4);
      cMesh.position.set(p.x + p.width / 2, -p.y - 2, 2);
      this.platformGroup.add(cMesh);
    });

    // 2. Build 3D Background Props (Stained Glass Windows, Pillars, Clockwork Gears)
    this.rotatingGears = [];
    if (room.backgroundProps) {
      room.backgroundProps.forEach((prop, idx) => {
        if (prop.type === 'pillar') {
          const pillarMesh = new THREE.Mesh(this.geometries.cylinder, this.materials.metalPlatform);
          pillarMesh.scale.set(24, 500, 24);
          pillarMesh.position.set(prop.x, -prop.y - 200, -120);
          pillarMesh.castShadow = true;
          this.backgroundGroup.add(pillarMesh);
        } else if (prop.type === 'stained_glass') {
          const glassMesh = new THREE.Mesh(this.geometries.box, this.materials.stainedGlass);
          glassMesh.scale.set(120, 240, 10);
          glassMesh.position.set(prop.x, -prop.y, -180);
          this.backgroundGroup.add(glassMesh);
        } else {
          // Clockwork Gear Machinery
          const gearGroup = new THREE.Group();
          const gearMesh = new THREE.Mesh(this.geometries.cylinder, this.materials.gearBrass);
          gearMesh.scale.set(60 + (idx % 3) * 20, 12, 60 + (idx % 3) * 20);
          gearMesh.rotation.x = Math.PI / 2;
          gearGroup.add(gearMesh);

          // Gear Teeth
          for (let t = 0; t < 8; t++) {
            const tooth = new THREE.Mesh(this.geometries.box, this.materials.gearBrass);
            const angle = (t / 8) * Math.PI * 2;
            const r = 35 + (idx % 3) * 10;
            tooth.scale.set(8, 12, 12);
            tooth.position.set(Math.cos(angle) * r, Math.sin(angle) * r, 0);
            tooth.rotation.z = angle;
            gearGroup.add(tooth);
          }

          // Center Glowing Axle
          const axle = new THREE.Mesh(this.geometries.cylinder, this.materials.neonCyan);
          axle.scale.set(12, 20, 12);
          axle.rotation.x = Math.PI / 2;
          gearGroup.add(axle);

          gearGroup.position.set(prop.x, -prop.y, -140);
          gearGroup.userData = { speed: (idx % 2 === 0 ? 1 : -1) * 0.015 };
          this.backgroundGroup.add(gearGroup);
          this.rotatingGears.push(gearGroup as unknown as THREE.Mesh);
        }
      });
    }

    // 3. Build 3D Foreground Framing Archways (Out-of-Focus Parallax Layer)
    for (let fx = 0; fx < room.width; fx += 800) {
      const archMesh = new THREE.Mesh(this.geometries.cylinder, this.materials.metalPlatform);
      archMesh.scale.set(45, 700, 45);
      archMesh.position.set(fx + 100, -350, 150);
      this.foregroundGroup.add(archMesh);
    }
  }

  /**
   * Synchronize 3D Enemy Meshes matching Game Engine physics & states
   */
  private syncEnemies(engine: GameEngine, time: number) {
    const activeIds = new Set<string>();

    engine.activeEnemies.forEach((enemy) => {
      activeIds.add(enemy.id);
      let entry = this.enemyMeshMap.get(enemy.id);

      if (!entry) {
        const meshGroup = new THREE.Group();
        let rig: any = null;

        const st = enemy.type.spriteType;
        if (st === 'boss_executioner' || enemy.type.behavior === 'boss') {
          rig = new BossExecutionerRig();
          meshGroup.add(rig.group);
        } else if (st === 'gargoyle' || st === 'boss_gargoyle' || st === 'bat' || enemy.type.behavior === 'flyer') {
          rig = new FlyingGargoyleRig();
          meshGroup.add(rig.group);
        } else if (st === 'blood_leecher' || st === 'clockwork_drone') {
          rig = new ArachnidCrawlerRig();
          meshGroup.add(rig.group);
        } else {
          rig = new SkeletonWarriorRig();
          meshGroup.add(rig.group);
        }

        this.enemyGroup.add(meshGroup);
        entry = { group: meshGroup, rig };
        this.enemyMeshMap.set(enemy.id, entry);
      }

      // Sync Position & Facing
      const ex = enemy.x + enemy.type.width / 2;
      const ey = -enemy.y - enemy.type.height / 2;
      entry.group.position.set(ex, ey, 10);

      const targetRotY = enemy.facing === 'right' ? 0 : Math.PI;
      entry.group.rotation.y = targetRotY;

      // Update Rig FK Kinematics
      if (entry.rig) {
        const isMoving = Math.abs(enemy.vx) > 0.1 || Math.abs(enemy.vy) > 0.1;
        const isAttacking = enemy.state === 'attack' || enemy.attackCooldown > 0;
        entry.rig.updatePose(time, isAttacking || isMoving, isMoving);
      }
    });

    // Cleanup Dead Enemy Meshes
    for (const [id, entry] of this.enemyMeshMap.entries()) {
      if (!activeIds.has(id)) {
        this.enemyGroup.remove(entry.group);
        this.enemyMeshMap.delete(id);
      }
    }
  }

  /**
   * Render Loop Synchronization with Game Engine 60FPS physics
   */
  public render(engine: GameEngine) {
    if (!this.isInitialized) return;

    const time = performance.now() * 0.002;
    const room = engine.getCurrentRoom();
    const zone = engine.getCurrentZone();

    if (room && (room.id !== this.currentRoomId || zone?.id !== this.currentZoneId)) {
      this.rebuildRoom(engine);
    }

    // 1. Cinematic Dynamic Camera System (Zoom & Shake)
    const isBossRoom = !!room?.bossTrigger || room?.id.includes('boss') || false;
    this.targetCameraZ = isBossRoom ? 820 : engine.isAttacking ? 620 : 700;

    const targetCamX = engine.camera.x + engine.screenWidth / 2;
    const targetCamY = -(engine.camera.y + engine.screenHeight / 2);

    this.camera.position.x += (targetCamX - this.camera.position.x) * 0.08;
    this.camera.position.y += (targetCamY - this.camera.position.y) * 0.08;
    this.camera.position.z += (this.targetCameraZ - this.camera.position.z) * 0.05;

    // Apply Camera Shake Impulse
    if (engine.isAttacking || engine.animState === 'dash') {
      this.cameraShakeIntensity = Math.max(this.cameraShakeIntensity, 3);
    }

    if (this.cameraShakeIntensity > 0.1) {
      this.camera.position.x += (Math.random() - 0.5) * this.cameraShakeIntensity;
      this.camera.position.y += (Math.random() - 0.5) * this.cameraShakeIntensity;
      this.cameraShakeIntensity *= 0.88;
    }

    this.dirLight.position.set(this.camera.position.x + 200, this.camera.position.y + 400, 400);

    // 2. Synchronize Player 3D Mesh
    const px = engine.playerPos.x + engine.playerWidth / 2;
    const py = -engine.playerPos.y - engine.playerHeight / 2;
    this.playerGroup.position.set(px, py, 0);

    // Facing rotation
    const targetRotY = engine.facing === 'right' ? 0 : Math.PI;
    this.playerGroup.rotation.y += (targetRotY - this.playerGroup.rotation.y) * 0.25;

    // 19-Bone Forward Kinematics Skeletal Rig Pose Solver
    if (this.playerRig) {
      this.playerRig.updatePose(
        engine.animState,
        engine.playerVel.x,
        engine.playerVel.y,
        time,
        engine.isAttacking
      );
    }

    // Player Light
    this.playerPointLight.position.set(px, py + 10, 30);

    // Weapon Arc Slash Motion Trail Opacity
    if (this.weaponTrailMesh) {
      const trailMat = this.materials.weaponTrail as THREE.MeshBasicMaterial;
      if (engine.isAttacking) {
        trailMat.opacity = 0.85;
        this.weaponTrailMesh.rotation.z = Math.sin(time * 20) * 0.5;
      } else {
        trailMat.opacity *= 0.7;
      }
    }

    // Cape Dynamic Cloth Wave Animation
    if (this.capeGeometry) {
      const posAttr = this.capeGeometry.attributes.position;
      for (let i = 0; i < posAttr.count; i++) {
        const u = posAttr.getY(i);
        const wave = Math.sin(time * 3 + u * 0.1) * (u < 0 ? 8 : 1);
        posAttr.setZ(i, wave);
      }
      posAttr.needsUpdate = true;
    }

    // 3. Rotate Background Clockwork Machinery Gears
    this.rotatingGears.forEach((gear) => {
      gear.rotation.z += gear.userData.speed || 0.01;
    });

    // 4. Synchronize 3D Enemy Meshes
    this.syncEnemies(engine, time);

    // 5. Update Dynamic 3D Rain & Energy Particles
    if (this.particlePositions) {
      const pos = this.particlePositions;
      for (let i = 0; i < this.particleCount; i++) {
        pos[i * 3 + 1] -= 8; // Fall speed
        pos[i * 3] += Math.sin(time + i) * 1.5;

        if (pos[i * 3 + 1] < -engine.screenHeight - 200) {
          pos[i * 3 + 1] = engine.screenHeight + 200;
          pos[i * 3] = px + (Math.random() - 0.5) * 1200;
        }
      }
      this.particlePoints.geometry.attributes.position.needsUpdate = true;
    }

    // 6. Render 3D WebGL Scene
    this.renderer.render(this.scene, this.camera);
  }

  public dispose() {
    window.removeEventListener('resize', this.onWindowResize);
    if (this.renderer && this.renderer.domElement) {
      this.renderer.domElement.remove();
    }
  }
}
