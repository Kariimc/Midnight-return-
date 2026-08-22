import * as THREE from 'three';
import { TextureGenerator } from '../utils/textureGenerator';

export interface RigBones {
  hips: THREE.Bone;
  spine: THREE.Bone;
  chest: THREE.Bone;
  neck: THREE.Bone;
  head: THREE.Bone;
  leftShoulder: THREE.Bone;
  leftArm: THREE.Bone;
  leftForearm: THREE.Bone;
  leftHand: THREE.Bone;
  rightShoulder: THREE.Bone;
  rightArm: THREE.Bone;
  rightForearm: THREE.Bone;
  rightHand: THREE.Bone;
  leftUpLeg: THREE.Bone;
  leftLeg: THREE.Bone;
  leftFoot: THREE.Bone;
  rightUpLeg: THREE.Bone;
  rightLeg: THREE.Bone;
  rightFoot: THREE.Bone;
}

export class CharacterSkeletalRig {
  public group: THREE.Group = new THREE.Group();
  public skeleton!: THREE.Skeleton;
  public bones!: RigBones;
  public skinnedMesh!: THREE.SkinnedMesh;
  public weaponGroup: THREE.Group = new THREE.Group();

  constructor() {
    this.buildSkeleton();
    this.buildSkinnedMesh();
    this.buildWeapon();
  }

  /**
   * Constructs 19-Bone Humanoid Skeletal Hierarchy
   */
  private buildSkeleton() {
    const hips = new THREE.Bone();
    hips.name = 'hips';
    hips.position.set(0, 0, 0);

    const spine = new THREE.Bone();
    spine.name = 'spine';
    spine.position.set(0, 12, 0);
    hips.add(spine);

    const chest = new THREE.Bone();
    chest.name = 'chest';
    chest.position.set(0, 14, 0);
    spine.add(chest);

    const neck = new THREE.Bone();
    neck.name = 'neck';
    neck.position.set(0, 12, 0);
    chest.add(neck);

    const head = new THREE.Bone();
    head.name = 'head';
    head.position.set(0, 8, 0);
    neck.add(head);

    // Left Arm
    const leftShoulder = new THREE.Bone();
    leftShoulder.position.set(-14, 10, 0);
    chest.add(leftShoulder);

    const leftArm = new THREE.Bone();
    leftArm.position.set(-4, -2, 0);
    leftShoulder.add(leftArm);

    const leftForearm = new THREE.Bone();
    leftForearm.position.set(0, -12, 0);
    leftArm.add(leftForearm);

    const leftHand = new THREE.Bone();
    leftHand.position.set(0, -10, 0);
    leftForearm.add(leftHand);

    // Right Arm (Weapon Arm)
    const rightShoulder = new THREE.Bone();
    rightShoulder.position.set(14, 10, 0);
    chest.add(rightShoulder);

    const rightArm = new THREE.Bone();
    rightArm.position.set(4, -2, 0);
    rightShoulder.add(rightArm);

    const rightForearm = new THREE.Bone();
    rightForearm.position.set(0, -12, 0);
    rightArm.add(rightForearm);

    const rightHand = new THREE.Bone();
    rightHand.position.set(0, -10, 0);
    rightForearm.add(rightHand);

    // Left Leg
    const leftUpLeg = new THREE.Bone();
    leftUpLeg.position.set(-8, -4, 0);
    hips.add(leftUpLeg);

    const leftLeg = new THREE.Bone();
    leftLeg.position.set(0, -16, 0);
    leftUpLeg.add(leftLeg);

    const leftFoot = new THREE.Bone();
    leftFoot.position.set(0, -14, 2);
    leftLeg.add(leftFoot);

    // Right Leg
    const rightUpLeg = new THREE.Bone();
    rightUpLeg.position.set(8, -4, 0);
    hips.add(rightUpLeg);

    const rightLeg = new THREE.Bone();
    rightLeg.position.set(0, -16, 0);
    rightUpLeg.add(rightLeg);

    const rightFoot = new THREE.Bone();
    rightFoot.position.set(0, -14, 2);
    rightLeg.add(rightFoot);

    const allBones = [
      hips, spine, chest, neck, head,
      leftShoulder, leftArm, leftForearm, leftHand,
      rightShoulder, rightArm, rightForearm, rightHand,
      leftUpLeg, leftLeg, leftFoot,
      rightUpLeg, rightLeg, rightFoot,
    ];

    this.skeleton = new THREE.Skeleton(allBones);
    this.bones = {
      hips, spine, chest, neck, head,
      leftShoulder, leftArm, leftForearm, leftHand,
      rightShoulder, rightArm, rightForearm, rightHand,
      leftUpLeg, leftLeg, leftFoot,
      rightUpLeg, rightLeg, rightFoot,
    };

    this.group.add(hips);
  }

  /**
   * Generates Skinned Mesh with Vertex Skinning Indices & Weights
   */
  private buildSkinnedMesh() {
    // Multi-segmented Humanoid Body Geometry
    const height = 64;
    const radius = 10;
    const geometry = new THREE.CylinderGeometry(radius, radius * 0.8, height, 16, 32, false);
    
    // Calculate skin indices & skin weights for vertex mesh skinning
    const position = geometry.attributes.position;
    const skinIndices: number[] = [];
    const skinWeights: number[] = [];

    for (let i = 0; i < position.count; i++) {
      const y = position.getY(i); // -32 to +32

      if (y > 16) {
        // Head & Neck (Bones 3, 4)
        skinIndices.push(3, 4, 2, 0);
        skinWeights.push(0.6, 0.4, 0.0, 0.0);
      } else if (y > 0) {
        // Spine & Chest (Bones 1, 2)
        skinIndices.push(1, 2, 0, 0);
        skinWeights.push(0.5, 0.5, 0.0, 0.0);
      } else if (y > -16) {
        // Hips & Thighs (Bones 0, 13, 16)
        skinIndices.push(0, 13, 16, 1);
        skinWeights.push(0.6, 0.2, 0.2, 0.0);
      } else {
        // Lower Legs & Feet (Bones 14, 17)
        skinIndices.push(14, 17, 13, 16);
        skinWeights.push(0.5, 0.5, 0.0, 0.0);
      }
    }

    geometry.setAttribute('skinIndex', new THREE.Uint16BufferAttribute(skinIndices, 4));
    geometry.setAttribute('skinWeight', new THREE.Float32BufferAttribute(skinWeights, 4));

    // Metallic Gothic Cuirass Material with AAA Mesh Texture Map
    const material = new THREE.MeshStandardMaterial({
      map: TextureGenerator.createHeroArmorTexture(),
      color: new THREE.Color('#334155'),
      roughness: 0.25,
      metalness: 0.85,
    });

    this.skinnedMesh = new THREE.SkinnedMesh(geometry, material);
    this.skinnedMesh.add(this.bones.hips);
    this.skinnedMesh.bind(this.skeleton);
    this.skinnedMesh.castShadow = true;

    this.group.add(this.skinnedMesh);

    // Attach Flared High Gothic Collar to Chest Bone
    const collarExtMat = new THREE.MeshStandardMaterial({
      color: new THREE.Color('#111625'),
      roughness: 0.3,
      metalness: 0.8,
    });
    const collarIntMat = new THREE.MeshStandardMaterial({
      color: new THREE.Color('#b91c1c'),
      roughness: 0.4,
      metalness: 0.1,
    });

    const collarGeo = new THREE.ConeGeometry(12, 18, 4, 1, true, -Math.PI / 3, (Math.PI * 2) / 3);
    const leftCollar = new THREE.Mesh(collarGeo, collarIntMat);
    leftCollar.position.set(-6, 8, -2);
    leftCollar.rotation.set(-0.3, 0.4, -0.4);
    this.bones.chest.add(leftCollar);

    const rightCollar = new THREE.Mesh(collarGeo, collarIntMat);
    rightCollar.position.set(6, 8, -2);
    rightCollar.rotation.set(-0.3, -0.4, 0.4);
    this.bones.chest.add(rightCollar);

    // Attach Angular Upward-Pointed Pauldrons to Left/Right Shoulder Bones
    const pauldronMat = new THREE.MeshStandardMaterial({
      color: new THREE.Color('#212638'),
      roughness: 0.2,
      metalness: 0.9,
    });
    const pauldronGeo = new THREE.ConeGeometry(7, 14, 4);

    const leftPauldron = new THREE.Mesh(pauldronGeo, pauldronMat);
    leftPauldron.position.set(-2, 4, 0);
    leftPauldron.rotation.set(0, 0, 0.6);
    leftPauldron.castShadow = true;
    this.bones.leftShoulder.add(leftPauldron);

    const rightPauldron = new THREE.Mesh(pauldronGeo, pauldronMat);
    rightPauldron.position.set(2, 4, 0);
    rightPauldron.rotation.set(0, 0, -0.6);
    rightPauldron.castShadow = true;
    this.bones.rightShoulder.add(rightPauldron);

    // Attach Flowing Silver Hair Strands to Head Bone
    const hairMat = new THREE.MeshStandardMaterial({
      color: new THREE.Color('#f8fafc'),
      roughness: 0.1,
      metalness: 0.3,
    });
    const hairGeo = new THREE.CylinderGeometry(4, 8, 36, 8);
    const hairMesh = new THREE.Mesh(hairGeo, hairMat);
    hairMesh.position.set(0, -12, -4);
    hairMesh.rotation.x = -0.2;
    this.bones.head.add(hairMesh);

    // Attach Sapphire Chest Gem to Chest Bone
    const gemMat = new THREE.MeshPhysicalMaterial({
      color: new THREE.Color('#38bdf8'),
      emissive: new THREE.Color('#0284c7'),
      emissiveIntensity: 3.0,
      roughness: 0.1,
      metalness: 0.2,
    });
    const chestGem = new THREE.Mesh(new THREE.OctahedronGeometry(4), gemMat);
    chestGem.position.set(0, 4, 9);
    chestGem.rotation.z = Math.PI / 4;
    this.bones.chest.add(chestGem);
  }

  /**
   * Attach Weapon Mesh to Right Hand Bone
   */
  private buildWeapon() {
    const bladeMat = new THREE.MeshStandardMaterial({
      color: new THREE.Color('#f1f5f9'),
      roughness: 0.1,
      metalness: 0.95,
    });
    const goldMat = new THREE.MeshStandardMaterial({
      color: new THREE.Color('#fbbf24'),
      roughness: 0.3,
      metalness: 0.9,
    });

    const blade = new THREE.Mesh(new THREE.BoxGeometry(3, 48, 1.5), bladeMat);
    blade.position.y = 24;
    blade.castShadow = true;
    this.weaponGroup.add(blade);

    const guard = new THREE.Mesh(new THREE.BoxGeometry(18, 3, 3), goldMat);
    guard.position.y = 0;
    this.weaponGroup.add(guard);

    this.weaponGroup.position.set(0, -4, 0);
    this.weaponGroup.rotation.x = Math.PI / 2;
    this.bones.rightHand.add(this.weaponGroup);
  }

  /**
   * Forward Kinematics (FK) Skeletal Pose Solver
   */
  public updatePose(
    state: string,
    velX: number,
    velY: number,
    time: number,
    isAttacking: boolean
  ) {
    const b = this.bones;

    // Reset Bone Rotations
    b.hips.rotation.set(0, 0, 0);
    b.spine.rotation.set(0, 0, 0);
    b.chest.rotation.set(0, 0, 0);
    b.neck.rotation.set(0, 0, 0);
    b.head.rotation.set(0, 0, 0);

    b.leftArm.rotation.set(0, 0, 0);
    b.leftForearm.rotation.set(0, 0, 0);
    b.rightArm.rotation.set(0, 0, 0);
    b.rightForearm.rotation.set(0, 0, 0);

    b.leftUpLeg.rotation.set(0, 0, 0);
    b.leftLeg.rotation.set(0, 0, 0);
    b.rightUpLeg.rotation.set(0, 0, 0);
    b.rightLeg.rotation.set(0, 0, 0);

    if (isAttacking) {
      if (this.weaponGroup.parent !== b.rightHand) {
        b.rightHand.add(this.weaponGroup);
        this.weaponGroup.position.set(0, -4, 0);
        this.weaponGroup.rotation.set(Math.PI / 2, 0, 0);
      }
      // --- WHIP / SWORD ATTACK SWEEP POSE ---
      b.spine.rotation.y = 0.4;
      b.chest.rotation.x = 0.2;
      b.rightArm.rotation.x = -Math.PI / 2 + Math.sin(time * 30) * 0.8;
      b.rightArm.rotation.z = Math.cos(time * 30) * 0.5;
      b.rightForearm.rotation.x = -0.4;
      b.leftArm.rotation.x = 0.5;
    } else {
      if (this.weaponGroup.parent !== b.spine) {
        b.spine.add(this.weaponGroup);
        this.weaponGroup.position.set(-2, 10, -6);
        this.weaponGroup.rotation.set(0, 0, -Math.PI / 3.8);
      }
    }

    if (!isAttacking) {
      if (state === 'wall_slide' || state === 'wallSlide') {
        // --- WALL SLIDE SKELETAL POSE ---
        b.spine.rotation.z = -0.2;
        b.rightArm.rotation.x = -Math.PI / 3;
        b.rightArm.rotation.z = 0.5;
        b.leftArm.rotation.x = 0.3;
        b.leftUpLeg.rotation.x = -0.4;
        b.rightUpLeg.rotation.x = 0.2;
      } else if (state === 'dash') {
      // --- AERODYNAMIC DASH POSE ---
      b.hips.rotation.x = 0.5;
      b.spine.rotation.x = 0.3;
      b.leftUpLeg.rotation.x = -0.8;
      b.rightUpLeg.rotation.x = 0.9;
      b.rightArm.rotation.x = -1.2;
      b.leftArm.rotation.x = 1.2;
    } else if (Math.abs(velY) > 0.5) {
      // --- JUMP / FALL AIRBORNE SKELETAL POSE ---
      const jumpSign = velY < 0 ? -1 : 1;
      b.spine.rotation.x = jumpSign * 0.2;
      b.leftUpLeg.rotation.x = -0.6;
      b.leftLeg.rotation.x = 0.8;
      b.rightUpLeg.rotation.x = -0.3;
      b.rightLeg.rotation.x = 0.4;

      b.leftArm.rotation.z = -0.6;
      b.rightArm.rotation.z = 0.6;
    } else if (Math.abs(velX) > 0.5) {
      // --- 12-BONE RUN LOCOMOTION GAIT CYCLE ---
      const runFreq = time * 14;
      const legAngle1 = Math.sin(runFreq) * 0.75;
      const legAngle2 = Math.sin(runFreq + Math.PI) * 0.75;

      b.hips.position.y = Math.abs(Math.cos(runFreq * 2)) * 2;
      b.spine.rotation.x = 0.15;
      b.spine.rotation.y = Math.sin(runFreq) * 0.15;

      // Leg Mechanics
      b.leftUpLeg.rotation.x = legAngle1;
      b.leftLeg.rotation.x = legAngle1 < 0 ? Math.abs(legAngle1) * 0.8 : 0.1;

      b.rightUpLeg.rotation.x = legAngle2;
      b.rightLeg.rotation.x = legAngle2 < 0 ? Math.abs(legAngle2) * 0.8 : 0.1;

      // Counter Arm Swing
      b.leftArm.rotation.x = -legAngle1 * 0.8;
      b.rightArm.rotation.x = -legAngle2 * 0.8;
      b.rightForearm.rotation.x = -0.5;
    } else {
      // --- IDLE NATURAL BREATHING POSE ---
      const breath = Math.sin(time * 3) * 0.05;
      b.hips.position.y = breath * 2;
      b.spine.rotation.x = breath;
      b.chest.rotation.x = breath * 0.5;

      b.leftArm.rotation.z = -0.15 + breath * 0.5;
      b.rightArm.rotation.z = 0.15 - breath * 0.5;
      b.rightForearm.rotation.x = -0.3;
    }
    }
  }
}

/**
 * AAA BOSS EXECUTIONER SKELETAL RIG
 * 20-Bone Heavy Plate Skinned Mesh with Greataxe & Cyber Visor Light
 */
export class BossExecutionerRig {
  public group: THREE.Group = new THREE.Group();
  public skeleton!: THREE.Skeleton;
  public bones: Record<string, THREE.Bone> = {};
  public skinnedMesh!: THREE.SkinnedMesh;
  public axeGroup: THREE.Group = new THREE.Group();
  public visorLight!: THREE.PointLight;

  constructor() {
    this.buildSkeleton();
    this.buildSkinnedMesh();
  }

  private buildSkeleton() {
    const hips = new THREE.Bone();
    hips.name = 'hips';

    const spine = new THREE.Bone();
    spine.position.set(0, 20, 0);
    hips.add(spine);

    const chest = new THREE.Bone();
    chest.position.set(0, 25, 0);
    spine.add(chest);

    const head = new THREE.Bone();
    head.position.set(0, 20, 0);
    chest.add(head);

    const leftArm = new THREE.Bone();
    leftArm.position.set(-25, 10, 0);
    chest.add(leftArm);

    const leftForearm = new THREE.Bone();
    leftForearm.position.set(0, -25, 0);
    leftArm.add(leftForearm);

    const rightArm = new THREE.Bone();
    rightArm.position.set(25, 10, 0);
    chest.add(rightArm);

    const rightForearm = new THREE.Bone();
    rightForearm.position.set(0, -25, 0);
    rightArm.add(rightForearm);

    const rightHand = new THREE.Bone();
    rightHand.position.set(0, -20, 0);
    rightForearm.add(rightHand);

    const leftUpLeg = new THREE.Bone();
    leftUpLeg.position.set(-15, -10, 0);
    hips.add(leftUpLeg);

    const leftLeg = new THREE.Bone();
    leftLeg.position.set(0, -25, 0);
    leftUpLeg.add(leftLeg);

    const rightUpLeg = new THREE.Bone();
    rightUpLeg.position.set(15, -10, 0);
    hips.add(rightUpLeg);

    const rightLeg = new THREE.Bone();
    rightLeg.position.set(0, -25, 0);
    rightUpLeg.add(rightLeg);

    const allBones = [
      hips, spine, chest, head,
      leftArm, leftForearm,
      rightArm, rightForearm, rightHand,
      leftUpLeg, leftLeg,
      rightUpLeg, rightLeg
    ];

    this.skeleton = new THREE.Skeleton(allBones);
    this.bones = {
      hips, spine, chest, head,
      leftArm, leftForearm,
      rightArm, rightForearm, rightHand,
      leftUpLeg, leftLeg,
      rightUpLeg, rightLeg
    };

    this.group.add(hips);
  }

  private buildSkinnedMesh() {
    // Massive Heavy Armor Cylinder Geometry
    const geometry = new THREE.CylinderGeometry(25, 20, 110, 16, 16);
    const skinIndices: number[] = [];
    const skinWeights: number[] = [];

    const pos = geometry.attributes.position;
    for (let i = 0; i < pos.count; i++) {
      const y = pos.getY(i);
      if (y > 30) {
        skinIndices.push(3, 2, 0, 0); // Head & chest
        skinWeights.push(0.7, 0.3, 0, 0);
      } else if (y > 0) {
        skinIndices.push(2, 1, 0, 0); // Chest & spine
        skinWeights.push(0.6, 0.4, 0, 0);
      } else {
        skinIndices.push(0, 9, 11, 0); // Hips & thighs
        skinWeights.push(0.5, 0.25, 0.25, 0);
      }
    }

    geometry.setAttribute('skinIndex', new THREE.Uint16BufferAttribute(skinIndices, 4));
    geometry.setAttribute('skinWeight', new THREE.Float32BufferAttribute(skinWeights, 4));

    const material = new THREE.MeshStandardMaterial({
      map: TextureGenerator.createBossArmorTexture(),
      color: new THREE.Color('#1e293b'),
      roughness: 0.2,
      metalness: 0.9,
    });

    this.skinnedMesh = new THREE.SkinnedMesh(geometry, material);
    this.skinnedMesh.add(this.bones.hips);
    this.skinnedMesh.bind(this.skeleton);
    this.skinnedMesh.castShadow = true;
    this.group.add(this.skinnedMesh);

    // Glowing Cyber Red Visor Light
    const visorMat = new THREE.MeshBasicMaterial({ color: '#ef4444' });
    const visorMesh = new THREE.Mesh(new THREE.BoxGeometry(20, 5, 8), visorMat);
    visorMesh.position.set(0, 4, 12);
    this.bones.head.add(visorMesh);

    this.visorLight = new THREE.PointLight('#ef4444', 3.0, 120);
    this.visorLight.position.set(0, 4, 15);
    this.bones.head.add(this.visorLight);

    // Double-Headed Executioner Battleaxe
    const shaftMat = new THREE.MeshStandardMaterial({ color: '#0f172a', roughness: 0.5 });
    const axeHeadMat = new THREE.MeshStandardMaterial({ color: '#e2e8f0', roughness: 0.1, metalness: 0.95 });

    const shaft = new THREE.Mesh(new THREE.CylinderGeometry(3, 3, 110, 8), shaftMat);
    shaft.castShadow = true;
    this.axeGroup.add(shaft);

    const bladeLeft = new THREE.Mesh(new THREE.BoxGeometry(4, 50, 30), axeHeadMat);
    bladeLeft.position.set(-18, 30, 0);
    bladeLeft.castShadow = true;
    this.axeGroup.add(bladeLeft);

    const bladeRight = new THREE.Mesh(new THREE.BoxGeometry(4, 50, 30), axeHeadMat);
    bladeRight.position.set(18, 30, 0);
    bladeRight.castShadow = true;
    this.axeGroup.add(bladeRight);

    this.axeGroup.rotation.x = Math.PI / 2;
    this.bones.rightHand.add(this.axeGroup);
  }

  public updatePose(time: number, isAttacking: boolean, isMoving: boolean) {
    const b = this.bones;
    b.hips.position.y = Math.sin(time * 3) * 2;

    if (isAttacking) {
      // Massive Overhead Execution Cleave
      const attackPhase = Math.sin(time * 8);
      b.chest.rotation.x = -0.4 + attackPhase * 0.6;
      b.rightArm.rotation.x = -Math.PI + attackPhase * 1.2;
      b.leftArm.rotation.x = -Math.PI / 2;
    } else if (isMoving) {
      // Heavy Armor March
      const stride = Math.sin(time * 6) * 0.5;
      b.leftUpLeg.rotation.x = stride;
      b.rightUpLeg.rotation.x = -stride;
      b.rightArm.rotation.x = -stride * 0.8;
      b.leftArm.rotation.x = stride * 0.8;
    } else {
      // Breathing Stance
      const breath = Math.sin(time * 2) * 0.08;
      b.chest.rotation.x = breath;
      b.rightArm.rotation.z = -0.3;
      b.leftArm.rotation.z = 0.3;
    }
  }
}

/**
 * AAA SKELETON WARRIOR RIG
 * Undead Ribcage Mesh with Falchion Blade & Shield
 */
export class SkeletonWarriorRig {
  public group: THREE.Group = new THREE.Group();
  public skeleton!: THREE.Skeleton;
  public bones: Record<string, THREE.Bone> = {};
  public skinnedMesh!: THREE.SkinnedMesh;

  constructor() {
    this.buildSkeleton();
    this.buildSkinnedMesh();
  }

  private buildSkeleton() {
    const hips = new THREE.Bone();
    const spine = new THREE.Bone();
    spine.position.y = 12;
    hips.add(spine);

    const head = new THREE.Bone();
    head.position.y = 14;
    spine.add(head);

    const leftArm = new THREE.Bone();
    leftArm.position.set(-10, 8, 0);
    spine.add(leftArm);

    const rightArm = new THREE.Bone();
    rightArm.position.set(10, 8, 0);
    spine.add(rightArm);

    const leftLeg = new THREE.Bone();
    leftLeg.position.set(-6, -6, 0);
    hips.add(leftLeg);

    const rightLeg = new THREE.Bone();
    rightLeg.position.set(6, -6, 0);
    hips.add(rightLeg);

    const allBones = [hips, spine, head, leftArm, rightArm, leftLeg, rightLeg];
    this.skeleton = new THREE.Skeleton(allBones);
    this.bones = { hips, spine, head, leftArm, rightArm, leftLeg, rightLeg };
    this.group.add(hips);
  }

  private buildSkinnedMesh() {
    const geometry = new THREE.CylinderGeometry(8, 6, 45, 12, 8);
    const skinIndices: number[] = [];
    const skinWeights: number[] = [];

    const pos = geometry.attributes.position;
    for (let i = 0; i < pos.count; i++) {
      const y = pos.getY(i);
      if (y > 10) {
        skinIndices.push(2, 1, 0, 0);
        skinWeights.push(0.7, 0.3, 0, 0);
      } else {
        skinIndices.push(1, 0, 5, 6);
        skinWeights.push(0.5, 0.3, 0.1, 0.1);
      }
    }

    geometry.setAttribute('skinIndex', new THREE.Uint16BufferAttribute(skinIndices, 4));
    geometry.setAttribute('skinWeight', new THREE.Float32BufferAttribute(skinWeights, 4));

    const material = new THREE.MeshStandardMaterial({
      map: TextureGenerator.createBoneTexture(),
      color: new THREE.Color('#e2e8f0'),
      roughness: 0.6,
      metalness: 0.2,
    });

    this.skinnedMesh = new THREE.SkinnedMesh(geometry, material);
    this.skinnedMesh.add(this.bones.hips);
    this.skinnedMesh.bind(this.skeleton);
    this.skinnedMesh.castShadow = true;
    this.group.add(this.skinnedMesh);

    // Glowing Cyan Eye Sockets
    const eyeMat = new THREE.MeshBasicMaterial({ color: '#22d3ee' });
    const leftEye = new THREE.Mesh(new THREE.SphereGeometry(1.5, 8, 8), eyeMat);
    leftEye.position.set(-2.5, 4, 5);
    const rightEye = new THREE.Mesh(new THREE.SphereGeometry(1.5, 8, 8), eyeMat);
    rightEye.position.set(2.5, 4, 5);
    this.bones.head.add(leftEye);
    this.bones.head.add(rightEye);

    // Sword & Shield
    const blade = new THREE.Mesh(
      new THREE.BoxGeometry(2, 30, 1),
      new THREE.MeshStandardMaterial({ color: '#94a3b8', roughness: 0.2, metalness: 0.9 })
    );
    blade.position.set(0, -12, 0);
    this.bones.rightArm.add(blade);

    const shield = new THREE.Mesh(
      new THREE.CylinderGeometry(8, 8, 2, 12),
      new THREE.MeshStandardMaterial({ color: '#475569', roughness: 0.4, metalness: 0.7 })
    );
    shield.rotation.x = Math.PI / 2;
    shield.position.set(0, -6, 4);
    this.bones.leftArm.add(shield);
  }

  public updatePose(time: number, isMoving: boolean) {
    const b = this.bones;
    if (isMoving) {
      const walk = Math.sin(time * 8) * 0.6;
      b.leftLeg.rotation.x = walk;
      b.rightLeg.rotation.x = -walk;
      b.rightArm.rotation.x = -walk * 0.7;
    } else {
      b.leftLeg.rotation.x = 0;
      b.rightLeg.rotation.x = 0;
      b.spine.rotation.y = Math.sin(time * 2) * 0.1;
    }
  }
}

/**
 * AAA FLYING GARGOYLE RIG
 * Articulated Membranous Wings & Cyber Glowing Eye
 */
export class FlyingGargoyleRig {
  public group: THREE.Group = new THREE.Group();
  public leftWing: THREE.Group = new THREE.Group();
  public rightWing: THREE.Group = new THREE.Group();

  constructor() {
    const gargoyleTex = TextureGenerator.createGargoyleSkinTexture();
    const bodyMat = new THREE.MeshStandardMaterial({ map: gargoyleTex, color: '#1e293b', roughness: 0.3, metalness: 0.8 });
    const wingMat = new THREE.MeshStandardMaterial({ map: gargoyleTex, color: '#881337', roughness: 0.5, side: THREE.DoubleSide });

    // Torso & Head
    const body = new THREE.Mesh(new THREE.SphereGeometry(12, 12, 12), bodyMat);
    body.scale.set(1, 1.3, 1);
    body.castShadow = true;
    this.group.add(body);

    const eye = new THREE.Mesh(new THREE.SphereGeometry(3.5, 8, 8), new THREE.MeshBasicMaterial({ color: '#ef4444' }));
    eye.position.set(0, 4, 10);
    this.group.add(eye);

    // Left Wing Assembly
    const leftWingMesh = new THREE.Mesh(new THREE.PlaneGeometry(35, 20, 4, 4), wingMat);
    leftWingMesh.position.set(-18, 0, 0);
    leftWingMesh.castShadow = true;
    this.leftWing.add(leftWingMesh);
    this.leftWing.position.set(-8, 6, 0);
    this.group.add(this.leftWing);

    // Right Wing Assembly
    const rightWingMesh = new THREE.Mesh(new THREE.PlaneGeometry(35, 20, 4, 4), wingMat);
    rightWingMesh.position.set(18, 0, 0);
    rightWingMesh.castShadow = true;
    this.rightWing.add(rightWingMesh);
    this.rightWing.position.set(8, 6, 0);
    this.group.add(this.rightWing);
  }

  public updatePose(time: number) {
    const flap = Math.sin(time * 12) * 0.6;
    this.leftWing.rotation.z = flap;
    this.rightWing.rotation.z = -flap;
    this.group.position.y = Math.sin(time * 4) * 4;
  }
}

/**
 * AAA ARACHNID CRAWLER RIG
 * Quadruped Mechanical Crawler with Articulated Leg Joints
 */
export class ArachnidCrawlerRig {
  public group: THREE.Group = new THREE.Group();
  public legs: THREE.Group[] = [];

  constructor() {
    const metalMat = new THREE.MeshStandardMaterial({ color: '#334155', roughness: 0.3, metalness: 0.85 });
    const sensorMat = new THREE.MeshBasicMaterial({ color: '#22d3ee' });

    // Main Chassis
    const body = new THREE.Mesh(new THREE.SphereGeometry(12, 12, 10), metalMat);
    body.scale.set(1.2, 0.8, 1.2);
    body.castShadow = true;
    this.group.add(body);

    const sensor = new THREE.Mesh(new THREE.SphereGeometry(4, 8, 8), sensorMat);
    sensor.position.set(0, 2, 10);
    this.group.add(sensor);

    // 4 Articulated Leg Joints
    const legPositions = [
      { x: -10, z: 8, rot: 0.4 },
      { x: 10, z: 8, rot: -0.4 },
      { x: -10, z: -8, rot: 2.7 },
      { x: 10, z: -8, rot: -2.7 },
    ];

    legPositions.forEach((pos) => {
      const legGroup = new THREE.Group();
      legGroup.position.set(pos.x, 0, pos.z);

      const femur = new THREE.Mesh(new THREE.BoxGeometry(3, 16, 3), metalMat);
      femur.position.set(pos.x > 0 ? 6 : -6, -4, 0);
      femur.rotation.z = pos.x > 0 ? -0.8 : 0.8;
      legGroup.add(femur);

      this.group.add(legGroup);
      this.legs.push(legGroup);
    });
  }

  public updatePose(time: number, isMoving: boolean) {
    if (isMoving) {
      this.legs.forEach((leg, idx) => {
        const offset = idx % 2 === 0 ? 0 : Math.PI;
        leg.rotation.x = Math.sin(time * 14 + offset) * 0.4;
      });
    }
  }
}

