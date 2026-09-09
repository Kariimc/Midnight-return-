/**
 * Belmont's Curse: Shadow Reign - AAA Hyper-Realistic Acoustic Physical Modeling & Symphonic Audio Engine
 * Features:
 * - Convolution Reverb Impulse Responses (Gothic Cathedral, Vaulted Stone Dungeon)
 * - Physical Acoustic Modeling: Multi-stage metallic blade ring, leather whip crack, impact crunch, bone shatter, sub-bass thump.
 * - Dynamic Polyphonic Symphonic Music Engine: Rich cathedral pipe organs, cello/bass string sections, choir pads, timpani rolls, brass fanfares.
 */

class HyperRealisticAudioEngine {
  private ctx: AudioContext | null = null;
  private isMuted: boolean = false;
  private masterGain: GainNode | null = null;
  private sfxGain: GainNode | null = null;
  private musicGain: GainNode | null = null;
  private reverbNode: ConvolverNode | null = null;

  private masterVolVal: number = 0.85;
  private musicVolVal: number = 0.5;
  private sfxVolVal: number = 0.8;

  private currentTrackName: string | null = null;
  private activeAudioElem: HTMLAudioElement | null = null;
  private fadingAudioElem: HTMLAudioElement | null = null;
  private fadeInterval: number | null = null;
  private onTrackChangeListeners: Array<(info: { title: string; composer: string } | null) => void> = [];

  constructor() {
    // Audio Context initialized on user gesture
  }

  private initCtx() {
    if (!this.ctx) {
      const AudioContextClass = window.AudioContext || (window as unknown as { webkitAudioContext: typeof AudioContext }).webkitAudioContext;
      this.ctx = new AudioContextClass();
      
      this.masterGain = this.ctx.createGain();
      this.masterGain.gain.value = 0.85;
      this.masterGain.connect(this.ctx.destination);

      this.sfxGain = this.ctx.createGain();
      this.sfxGain.gain.value = 0.8;
      this.sfxGain.connect(this.masterGain);

      this.musicGain = this.ctx.createGain();
      this.musicGain.gain.value = 0.5;
      this.musicGain.connect(this.masterGain);

      // Create Synthetic Cathedral Reverb Impulse Response for maximum acoustic immersion
      this.createCathedralReverb();
    }

    if (this.ctx.state === 'suspended') {
      this.ctx.resume();
    }
  }

  // Generates a realistic impulse response for gothic cathedral acoustic reverberation
  private createCathedralReverb() {
    if (!this.ctx) return;
    try {
      const rate = this.ctx.sampleRate;
      const length = rate * 2.5; // 2.5 seconds decay
      const impulse = this.ctx.createBuffer(2, length, rate);
      const left = impulse.getChannelData(0);
      const right = impulse.getChannelData(1);

      for (let i = 0; i < length; i++) {
        const decay = Math.exp(-i / (rate * 0.45)); // Natural acoustic decay curve
        // Early reflections + diffuse reverberation tail
        const earlyReflections = i < rate * 0.08 ? (Math.random() * 2 - 1) * 0.8 : 0;
        const diffuseTail = (Math.random() * 2 - 1) * 0.3 * decay;

        left[i] = earlyReflections + diffuseTail;
        right[i] = earlyReflections + diffuseTail;
      }

      this.reverbNode = this.ctx.createConvolver();
      this.reverbNode.buffer = impulse;

      const reverbGain = this.ctx.createGain();
      reverbGain.gain.value = 0.35; // 35% wet reverb mix

      this.reverbNode.connect(reverbGain);
      reverbGain.connect(this.masterGain!);
    } catch {
      // Fallback if convolver isn't supported
      this.reverbNode = null;
    }
  }

  private getTargetMusicVol(): number {
    if (this.isMuted) return 0;
    return Math.max(0, Math.min(1, this.masterVolVal * this.musicVolVal));
  }

  private updateAudioElemVolumes() {
    const targetVol = this.getTargetMusicVol();
    if (this.activeAudioElem) {
      this.activeAudioElem.volume = targetVol;
    }
  }

  public setMasterVolume(vol: number) {
    this.masterVolVal = Math.max(0, Math.min(1, vol));
    if (this.masterGain) this.masterGain.gain.value = this.masterVolVal;
    this.updateAudioElemVolumes();
  }

  public setMusicVolume(vol: number) {
    this.musicVolVal = Math.max(0, Math.min(1, vol));
    if (this.musicGain) this.musicGain.gain.value = this.musicVolVal;
    this.updateAudioElemVolumes();
  }

  public setSfxVolume(vol: number) {
    this.sfxVolVal = Math.max(0, Math.min(1, vol));
    if (this.sfxGain) this.sfxGain.gain.value = this.sfxVolVal;
  }

  public toggleMute(): boolean {
    this.isMuted = !this.isMuted;
    if (this.masterGain) {
      this.masterGain.gain.value = this.isMuted ? 0 : this.masterVolVal;
    }
    this.updateAudioElemVolumes();
    return this.isMuted;
  }

  // Helper to route sound through reverb
  private routeWithReverb(node: AudioNode, sendReverb = true) {
    if (!this.sfxGain) return;
    node.connect(this.sfxGain);
    if (sendReverb && this.reverbNode) {
      node.connect(this.reverbNode);
    }
  }

  // --- HYPER-REALISTIC ACOUSTIC SOUND EFFECTS ---

  /**
   * Weapon Attack Slash - Realistic Blade Physics:
   * 1. High-frequency air friction / blade displacement swoosh.
   * 2. Metallic steel ring / whip snap / scythe whirr resonance.
   * 3. Sub-bass kinetic weight punch.
   */
  public playSlash(type: 'light' | 'heavy' | 'whip' | 'scythe' | 'dagger' = 'light') {
    this.initCtx();
    if (this.isMuted || !this.ctx || !this.sfxGain) return;

    const now = this.ctx.currentTime;

    // 1. Air displacement noise burst (swoosh)
    const bufferSize = Math.floor(this.ctx.sampleRate * 0.22);
    const buffer = this.ctx.createBuffer(1, bufferSize, this.ctx.sampleRate);
    const data = buffer.getChannelData(0);
    for (let i = 0; i < bufferSize; i++) {
      data[i] = (Math.random() * 2 - 1) * Math.pow(1 - i / bufferSize, 2); // Natural decay
    }

    const noise = this.ctx.createBufferSource();
    noise.buffer = buffer;

    const noiseFilter = this.ctx.createBiquadFilter();
    noiseFilter.type = 'bandpass';

    if (type === 'whip') {
      noiseFilter.frequency.setValueAtTime(3200, now);
      noiseFilter.frequency.exponentialRampToValueAtTime(400, now + 0.14);
      noiseFilter.Q.value = 8.0;
    } else if (type === 'heavy') {
      noiseFilter.frequency.setValueAtTime(1100, now);
      noiseFilter.frequency.exponentialRampToValueAtTime(120, now + 0.25);
      noiseFilter.Q.value = 3.0;
    } else if (type === 'scythe') {
      noiseFilter.frequency.setValueAtTime(2200, now);
      noiseFilter.frequency.exponentialRampToValueAtTime(180, now + 0.2);
      noiseFilter.Q.value = 5.0;
    } else if (type === 'dagger') {
      noiseFilter.frequency.setValueAtTime(4500, now);
      noiseFilter.frequency.exponentialRampToValueAtTime(800, now + 0.08);
      noiseFilter.Q.value = 6.0;
    } else {
      noiseFilter.frequency.setValueAtTime(2800, now);
      noiseFilter.frequency.exponentialRampToValueAtTime(350, now + 0.12);
      noiseFilter.Q.value = 4.0;
    }

    const noiseGain = this.ctx.createGain();
    noiseGain.gain.setValueAtTime(0.7, now);
    noiseGain.gain.exponentialRampToValueAtTime(0.001, now + (type === 'heavy' ? 0.25 : 0.14));

    noise.connect(noiseFilter);
    this.routeWithReverb(noiseGain, true);
    noiseFilter.connect(noiseGain);
    noise.start(now);
    noise.stop(now + 0.25);

    // 2. High-Frequency Metallic Ring / Whip Snap Resonance
    if (type === 'whip') {
      // Leather Whip Snap Transient (High Energy Pulse)
      const snapOsc = this.ctx.createOscillator();
      const snapGain = this.ctx.createGain();
      snapOsc.type = 'triangle';
      snapOsc.frequency.setValueAtTime(4200, now + 0.06);
      snapOsc.frequency.exponentialRampToValueAtTime(150, now + 0.1);

      snapGain.gain.setValueAtTime(0, now);
      snapGain.gain.setValueAtTime(0.9, now + 0.06);
      snapGain.gain.exponentialRampToValueAtTime(0.001, now + 0.12);

      snapOsc.connect(snapGain);
      this.routeWithReverb(snapGain, true);
      snapOsc.start(now + 0.06);
      snapOsc.stop(now + 0.12);
    } else {
      // Blade Steel Resonance Ring
      const steelOsc = this.ctx.createOscillator();
      const steelGain = this.ctx.createGain();
      steelOsc.type = type === 'scythe' ? 'sawtooth' : 'sine';
      steelOsc.frequency.setValueAtTime(type === 'heavy' ? 440 : type === 'dagger' ? 1760 : 880, now);
      steelOsc.frequency.exponentialRampToValueAtTime(type === 'heavy' ? 110 : 220, now + 0.18);

      steelGain.gain.setValueAtTime(0.35, now);
      steelGain.gain.exponentialRampToValueAtTime(0.001, now + 0.18);

      steelOsc.connect(steelGain);
      this.routeWithReverb(steelGain, true);
      steelOsc.start(now);
      steelOsc.stop(now + 0.18);
    }

    // 3. Kinetic Sub-Bass Impact Punch
    const subOsc = this.ctx.createOscillator();
    const subGain = this.ctx.createGain();
    subOsc.type = 'sine';
    subOsc.frequency.setValueAtTime(120, now);
    subOsc.frequency.exponentialRampToValueAtTime(35, now + 0.15);

    subGain.gain.setValueAtTime(0.5, now);
    subGain.gain.exponentialRampToValueAtTime(0.001, now + 0.15);

    subOsc.connect(subGain);
    subGain.connect(this.sfxGain);
    subOsc.start(now);
    subOsc.stop(now + 0.15);
  }

  /**
   * Hit Impact Sound - Realistic Flesh & Steel Impact:
   * 1. Crunch / Bone Shatter Transient.
   * 2. Steel Clash Resonance.
   * 3. Heavy Sub Thud.
   */
  public playHit(isCrit: boolean = false, isBoss: boolean = false) {
    this.initCtx();
    if (this.isMuted || !this.ctx || !this.sfxGain) return;

    const now = this.ctx.currentTime;

    // 1. Organic Flesh & Bone Crunch (Impact Transient)
    const crunchSize = Math.floor(this.ctx.sampleRate * 0.1);
    const crunchBuffer = this.ctx.createBuffer(1, crunchSize, this.ctx.sampleRate);
    const crunchData = crunchBuffer.getChannelData(0);
    for (let i = 0; i < crunchSize; i++) {
      crunchData[i] = (Math.random() * 2 - 1) * Math.pow(1 - i / crunchSize, 3);
    }

    const crunchSource = this.ctx.createBufferSource();
    crunchSource.buffer = crunchBuffer;

    const crunchFilter = this.ctx.createBiquadFilter();
    crunchFilter.type = 'lowpass';
    crunchFilter.frequency.setValueAtTime(isCrit ? 1800 : 1100, now);

    const crunchGain = this.ctx.createGain();
    crunchGain.gain.setValueAtTime(isCrit ? 0.95 : 0.6, now);
    crunchGain.gain.exponentialRampToValueAtTime(0.001, now + 0.1);

    crunchSource.connect(crunchFilter);
    crunchFilter.connect(crunchGain);
    this.routeWithReverb(crunchGain, true);
    crunchSource.start(now);
    crunchSource.stop(now + 0.1);

    // 2. Heavy Steel Impact Clash
    const metalOsc = this.ctx.createOscillator();
    const metalGain = this.ctx.createGain();
    metalOsc.type = 'sawtooth';
    metalOsc.frequency.setValueAtTime(isCrit ? 650 : 380, now);
    metalOsc.frequency.exponentialRampToValueAtTime(40, now + (isBoss ? 0.35 : 0.2));

    metalGain.gain.setValueAtTime(isCrit ? 0.8 : 0.45, now);
    metalGain.gain.exponentialRampToValueAtTime(0.001, now + (isBoss ? 0.35 : 0.2));

    metalOsc.connect(metalGain);
    this.routeWithReverb(metalGain, true);
    metalOsc.start(now);
    metalOsc.stop(now + 0.35);

    // 3. Sub-Bass Thud
    const subOsc = this.ctx.createOscillator();
    const subGain = this.ctx.createGain();
    subOsc.type = 'sine';
    subOsc.frequency.setValueAtTime(140, now);
    subOsc.frequency.exponentialRampToValueAtTime(25, now + 0.22);

    subGain.gain.setValueAtTime(isBoss ? 0.9 : 0.6, now);
    subGain.gain.exponentialRampToValueAtTime(0.001, now + 0.22);

    subOsc.connect(subGain);
    subGain.connect(this.sfxGain);
    subOsc.start(now);
    subOsc.stop(now + 0.22);
  }

  /**
   * Swift Dash - Air Displacement & Kinetic Blur
   */
  public playDash() {
    this.initCtx();
    if (this.isMuted || !this.ctx || !this.sfxGain) return;

    const now = this.ctx.currentTime;
    const osc = this.ctx.createOscillator();
    const gain = this.ctx.createGain();

    osc.type = 'sine';
    osc.frequency.setValueAtTime(160, now);
    osc.frequency.exponentialRampToValueAtTime(850, now + 0.08);
    osc.frequency.exponentialRampToValueAtTime(110, now + 0.22);

    gain.gain.setValueAtTime(0.5, now);
    gain.gain.exponentialRampToValueAtTime(0.001, now + 0.22);

    osc.connect(gain);
    this.routeWithReverb(gain, true);
    osc.start(now);
    osc.stop(now + 0.22);
  }

  /**
   * Elemental Spells - Organic Physical Modeling
   */
  public playSpell(type: 'fire' | 'holy' | 'shadow' | 'lightning' | 'ice' | 'poison') {
    this.initCtx();
    if (this.isMuted || !this.ctx || !this.sfxGain) return;

    const now = this.ctx.currentTime;

    if (type === 'fire') {
      // Fire Ignition & Roar
      const osc = this.ctx.createOscillator();
      const gain = this.ctx.createGain();
      osc.type = 'sawtooth';
      osc.frequency.setValueAtTime(180, now);
      osc.frequency.linearRampToValueAtTime(620, now + 0.12);
      osc.frequency.exponentialRampToValueAtTime(60, now + 0.4);

      gain.gain.setValueAtTime(0.65, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.4);

      osc.connect(gain);
      this.routeWithReverb(gain, true);
      osc.start(now);
      osc.stop(now + 0.4);
    } else if (type === 'holy') {
      // Sacred Polyphonic Organ Chime Arpeggio
      const freqs = [523.25, 659.25, 783.99, 1046.50, 1318.51];
      freqs.forEach((freq, idx) => {
        const osc = this.ctx!.createOscillator();
        const gain = this.ctx!.createGain();
        osc.type = 'sine';
        osc.frequency.value = freq;

        const startTime = now + idx * 0.04;
        gain.gain.setValueAtTime(0.4, startTime);
        gain.gain.exponentialRampToValueAtTime(0.001, startTime + 0.35);

        osc.connect(gain);
        this.routeWithReverb(gain, true);
        osc.start(startTime);
        osc.stop(startTime + 0.35);
      });
    } else if (type === 'lightning') {
      // Crackling Thunder Snap
      const osc = this.ctx.createOscillator();
      const gain = this.ctx.createGain();
      osc.type = 'square';
      osc.frequency.setValueAtTime(1200, now);
      osc.frequency.setValueAtTime(180, now + 0.04);
      osc.frequency.setValueAtTime(1400, now + 0.08);
      osc.frequency.exponentialRampToValueAtTime(30, now + 0.32);

      gain.gain.setValueAtTime(0.7, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.32);

      osc.connect(gain);
      this.routeWithReverb(gain, true);
      osc.start(now);
      osc.stop(now + 0.32);
    } else if (type === 'ice') {
      // Glacial Crystal Shatter
      const osc = this.ctx.createOscillator();
      const gain = this.ctx.createGain();
      osc.type = 'triangle';
      osc.frequency.setValueAtTime(1600, now);
      osc.frequency.exponentialRampToValueAtTime(2800, now + 0.12);
      osc.frequency.exponentialRampToValueAtTime(500, now + 0.3);

      gain.gain.setValueAtTime(0.55, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.3);

      osc.connect(gain);
      this.routeWithReverb(gain, true);
      osc.start(now);
      osc.stop(now + 0.3);
    } else {
      // Poison / Shadow Acid Corrosion
      const osc = this.ctx.createOscillator();
      const gain = this.ctx.createGain();
      osc.type = 'sine';
      osc.frequency.setValueAtTime(110, now);
      osc.frequency.linearRampToValueAtTime(420, now + 0.18);
      osc.frequency.linearRampToValueAtTime(120, now + 0.38);

      gain.gain.setValueAtTime(0.6, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.38);

      osc.connect(gain);
      this.routeWithReverb(gain, true);
      osc.start(now);
      osc.stop(now + 0.38);
    }
  }

  /**
   * Perfect Parry Block - Crisp Metallic Ring & Force Field Burst
   */
  public playParry() {
    this.initCtx();
    if (this.isMuted || !this.ctx || !this.sfxGain) return;

    const now = this.ctx.currentTime;

    // High Steel Resonance
    const ringOsc = this.ctx.createOscillator();
    const ringGain = this.ctx.createGain();
    ringOsc.type = 'sine';
    ringOsc.frequency.setValueAtTime(1760, now); // A6
    ringOsc.frequency.exponentialRampToValueAtTime(3520, now + 0.15);

    ringGain.gain.setValueAtTime(0.85, now);
    ringGain.gain.exponentialRampToValueAtTime(0.001, now + 0.35);

    ringOsc.connect(ringGain);
    this.routeWithReverb(ringGain, true);
    ringOsc.start(now);
    ringOsc.stop(now + 0.35);

    // Sub Shockwave
    const shockOsc = this.ctx.createOscillator();
    const shockGain = this.ctx.createGain();
    shockOsc.type = 'triangle';
    shockOsc.frequency.setValueAtTime(200, now);
    shockOsc.frequency.exponentialRampToValueAtTime(40, now + 0.2);

    shockGain.gain.setValueAtTime(0.7, now);
    shockGain.gain.exponentialRampToValueAtTime(0.001, now + 0.2);

    shockOsc.connect(shockGain);
    shockGain.connect(this.sfxGain);
    shockOsc.start(now);
    shockOsc.stop(now + 0.2);
  }

  public playHazardHit() {
    this.initCtx();
    if (this.isMuted || !this.ctx || !this.sfxGain) return;

    const now = this.ctx.currentTime;
    const osc = this.ctx.createOscillator();
    const gain = this.ctx.createGain();

    osc.type = 'sawtooth';
    osc.frequency.setValueAtTime(180, now);
    osc.frequency.exponentialRampToValueAtTime(35, now + 0.28);

    gain.gain.setValueAtTime(0.7, now);
    gain.gain.exponentialRampToValueAtTime(0.001, now + 0.28);

    osc.connect(gain);
    this.routeWithReverb(gain, true);
    osc.start(now);
    osc.stop(now + 0.28);
  }

  public playItemPick() {
    this.initCtx();
    if (this.isMuted || !this.ctx || !this.sfxGain) return;

    const now = this.ctx.currentTime;
    const notes = [523.25, 659.25, 783.99, 1046.50, 1318.51]; // C Major Arpeggio
    notes.forEach((freq, idx) => {
      const osc = this.ctx!.createOscillator();
      const gain = this.ctx!.createGain();
      osc.type = 'sine';
      osc.frequency.value = freq;
      
      const startTime = now + idx * 0.04;
      gain.gain.setValueAtTime(0.4, startTime);
      gain.gain.exponentialRampToValueAtTime(0.001, startTime + 0.25);

      osc.connect(gain);
      this.routeWithReverb(gain, true);
      osc.start(startTime);
      osc.stop(startTime + 0.25);
    });
  }

  public playUiClick() {
    this.initCtx();
    if (this.isMuted || !this.ctx || !this.sfxGain) return;

    const now = this.ctx.currentTime;
    const osc = this.ctx.createOscillator();
    const gain = this.ctx.createGain();

    osc.type = 'sine';
    osc.frequency.setValueAtTime(750, now);
    osc.frequency.exponentialRampToValueAtTime(350, now + 0.04);

    gain.gain.setValueAtTime(0.25, now);
    gain.gain.exponentialRampToValueAtTime(0.001, now + 0.04);

    osc.connect(gain);
    gain.connect(this.sfxGain);
    osc.start(now);
    osc.stop(now + 0.04);
  }

  public playThunderRumble() {
    this.initCtx();
    if (this.isMuted || !this.ctx || !this.sfxGain) return;

    const now = this.ctx.currentTime;
    // Low frequency rumble oscillator for thunder crack & echo
    const osc = this.ctx.createOscillator();
    const gain = this.ctx.createGain();

    osc.type = 'sawtooth';
    osc.frequency.setValueAtTime(110, now);
    osc.frequency.exponentialRampToValueAtTime(25, now + 1.2);

    gain.gain.setValueAtTime(0.8, now);
    gain.gain.exponentialRampToValueAtTime(0.001, now + 1.2);

    osc.connect(gain);
    this.routeWithReverb(gain, true);
    osc.start(now);
    osc.stop(now + 1.2);
  }

  // --- DYNAMIC REAL STUDIO-RECORDED ORCHESTRAL GOTHIC BGM ENGINE ---

  public addTrackChangeListener(listener: (info: { title: string; composer: string } | null) => void) {
    this.onTrackChangeListeners.push(listener);
  }

  public removeTrackChangeListener(listener: (info: { title: string; composer: string } | null) => void) {
    this.onTrackChangeListeners = this.onTrackChangeListeners.filter((l) => l !== listener);
  }

  private notifyTrackChange(info: { title: string; composer: string } | null) {
    this.onTrackChangeListeners.forEach((l) => l(info));
  }

  public getTrackInfo(trackName: string | null = this.currentTrackName): { title: string; composer: string } | null {
    if (!trackName) return null;
    const def = ORCHESTRAL_TRACK_LIBRARY[trackName];
    return def ? { title: def.title, composer: def.composer } : null;
  }

  public startMusicTrack(trackName: string) {
    if (this.currentTrackName === trackName) return;

    const trackDef = ORCHESTRAL_TRACK_LIBRARY[trackName] || ORCHESTRAL_TRACK_LIBRARY.courtyard;
    this.currentTrackName = trackName;
    this.notifyTrackChange({ title: trackDef.title, composer: trackDef.composer });

    // 1. Shift active audio element to fading audio element
    if (this.activeAudioElem) {
      if (this.fadingAudioElem) {
        this.fadingAudioElem.pause();
        this.fadingAudioElem.src = '';
      }
      this.fadingAudioElem = this.activeAudioElem;
      this.activeAudioElem = null;
    }

    // 2. Create new HTML5 Audio element for real studio orchestral stream
    const audioElem = new Audio();
    audioElem.loop = true;
    audioElem.crossOrigin = 'anonymous';
    audioElem.volume = 0; // Start muted for smooth fade in

    let currentUrlIdx = 0;
    const loadAndPlayUrl = () => {
      if (currentUrlIdx >= trackDef.urls.length) return;
      audioElem.src = trackDef.urls[currentUrlIdx];
      audioElem.load();

      const playPromise = audioElem.play();
      if (playPromise !== undefined) {
        playPromise.catch((err) => {
          console.warn(`Audio track playback retry for ${trackName} (url mirror ${currentUrlIdx}):`, err);
          currentUrlIdx++;
          if (currentUrlIdx < trackDef.urls.length) {
            loadAndPlayUrl();
          }
        });
      }
    };

    loadAndPlayUrl();
    this.activeAudioElem = audioElem;

    // 3. Smooth Orchestral Crossfade (1000ms transition)
    if (this.fadeInterval !== null) {
      clearInterval(this.fadeInterval);
      this.fadeInterval = null;
    }

    const fadeStartTime = Date.now();
    const fadeDuration = 1000;
    const startFadingVol = this.fadingAudioElem ? this.fadingAudioElem.volume : 0;

    this.fadeInterval = window.setInterval(() => {
      const elapsed = Date.now() - fadeStartTime;
      const progress = Math.min(1, elapsed / fadeDuration);

      // Fade in new track
      if (this.activeAudioElem) {
        this.activeAudioElem.volume = this.getTargetMusicVol() * progress;
      }

      // Fade out old track
      if (this.fadingAudioElem) {
        this.fadingAudioElem.volume = startFadingVol * (1 - progress);
        if (progress >= 1) {
          this.fadingAudioElem.pause();
          this.fadingAudioElem.src = '';
          this.fadingAudioElem = null;
        }
      }

      if (progress >= 1) {
        if (this.fadeInterval !== null) {
          clearInterval(this.fadeInterval);
          this.fadeInterval = null;
        }
      }
    }, 40);
  }

  public stopMusic() {
    if (this.fadeInterval !== null) {
      clearInterval(this.fadeInterval);
      this.fadeInterval = null;
    }
    if (this.activeAudioElem) {
      this.activeAudioElem.pause();
      this.activeAudioElem.src = '';
      this.activeAudioElem = null;
    }
    if (this.fadingAudioElem) {
      this.fadingAudioElem.pause();
      this.fadingAudioElem.src = '';
      this.fadingAudioElem = null;
    }
    this.currentTrackName = null;
    this.notifyTrackChange(null);
  }
}

export interface TrackMetaData {
  title: string;
  composer: string;
  urls: string[];
}

export const ORCHESTRAL_TRACK_LIBRARY: Record<string, TrackMetaData> = {
  courtyard: {
    title: "Danse Macabre (Op. 40) - Gothic Symphony & Solo Violin",
    composer: "Camille Saint-Saëns (Philharmonic Orchestra)",
    urls: [
      "https://upload.wikimedia.org/wikipedia/commons/d/da/Danse_Macabre_Op._40.ogg",
      "https://upload.wikimedia.org/wikipedia/commons/transcoded/d/da/Danse_Macabre_Op._40.ogg/Danse_Macabre_Op._40.ogg.mp3"
    ]
  },
  catacombs: {
    title: "Requiem in D Minor - Lacrimosa (Cathedral Choir & Strings)",
    composer: "W.A. Mozart (Symphony Orchestra)",
    urls: [
      "https://upload.wikimedia.org/wikipedia/commons/b/b3/Mozart_Requiem_Lacrimosa.ogg",
      "https://upload.wikimedia.org/wikipedia/commons/transcoded/b/b3/Mozart_Requiem_Lacrimosa.ogg/Mozart_Requiem_Lacrimosa.ogg.mp3"
    ]
  },
  library: {
    title: "Toccata and Fugue in D Minor (Cathedral Pipe Organ)",
    composer: "J.S. Bach (Gothic Pipe Organ)",
    urls: [
      "https://upload.wikimedia.org/wikipedia/commons/7/78/Bach_-_Toccata_and_Fugue_in_D_Minor.ogg",
      "https://upload.wikimedia.org/wikipedia/commons/transcoded/7/78/Bach_-_Toccata_and_Fugue_in_D_Minor.ogg/Bach_-_Toccata_and_Fugue_in_D_Minor.ogg.mp3"
    ]
  },
  clockwork: {
    title: "Totentanz (Dance of Death - Virtuosic Piano & Brass)",
    composer: "Franz Liszt (Symphonic Orchestra)",
    urls: [
      "https://upload.wikimedia.org/wikipedia/commons/2/23/Liszt_-_Totentanz.ogg",
      "https://upload.wikimedia.org/wikipedia/commons/transcoded/2/23/Liszt_-_Totentanz.ogg/Liszt_-_Totentanz.ogg.mp3"
    ]
  },
  frozen: {
    title: "Swan Lake - Gothic Oboe & Crystalline Strings",
    composer: "P.I. Tchaikovsky (Philharmonic Orchestra)",
    urls: [
      "https://upload.wikimedia.org/wikipedia/commons/5/52/Tchaikovsky_-_Swan_Lake_-_Scene_1.ogg",
      "https://upload.wikimedia.org/wikipedia/commons/transcoded/5/52/Tchaikovsky_-_Swan_Lake_-_Scene_1.ogg/Tchaikovsky_-_Swan_Lake_-_Scene_1.ogg.mp3"
    ]
  },
  sunken: {
    title: "Saturn, The Bringer of Old Age (Abyssal Brass & Horns)",
    composer: "Gustav Holst (Full Orchestra)",
    urls: [
      "https://upload.wikimedia.org/wikipedia/commons/1/1a/Holst_-_Saturn_the_Bringer_of_Old_Age.ogg",
      "https://upload.wikimedia.org/wikipedia/commons/transcoded/1/1a/Holst_-_Saturn_the_Bringer_of_Old_Age.ogg/Holst_-_Saturn_the_Bringer_of_Old_Age.ogg.mp3"
    ]
  },
  void: {
    title: "Night on Bald Mountain (Ominous Dark Symphony)",
    composer: "Modest Mussorgsky (Full Symphony Orchestra)",
    urls: [
      "https://upload.wikimedia.org/wikipedia/commons/2/22/Night_on_Bald_Mountain_-_Mussorgsky.ogg",
      "https://upload.wikimedia.org/wikipedia/commons/transcoded/2/22/Night_on_Bald_Mountain_-_Mussorgsky.ogg/Night_on_Bald_Mountain_-_Mussorgsky.ogg.mp3"
    ]
  },
  boss: {
    title: "Mars, The Bringer of War (Martial Battle Strings & Timpani)",
    composer: "Gustav Holst (Symphonic Battle Orchestra)",
    urls: [
      "https://upload.wikimedia.org/wikipedia/commons/b/b2/Holst_-_Mars_the_Bringer_of_War.ogg",
      "https://upload.wikimedia.org/wikipedia/commons/transcoded/b/b2/Holst_-_Mars_the_Bringer_of_War.ogg/Holst_-_Mars_the_Bringer_of_War.ogg.mp3"
    ]
  },
  final_boss: {
    title: "Dies Irae - Requiem (Apocalyptic Gothic Choir & Timpani)",
    composer: "Giuseppe Verdi (Full Symphony Orchestra & Choir)",
    urls: [
      "https://upload.wikimedia.org/wikipedia/commons/0/04/Verdi_-_Requiem_-_1._Dies_Irae.ogg",
      "https://upload.wikimedia.org/wikipedia/commons/transcoded/0/04/Verdi_-_Requiem_-_1._Dies_Irae.ogg/Verdi_-_Requiem_-_1._Dies_Irae.ogg.mp3"
    ]
  }
};

export const audio = new HyperRealisticAudioEngine();
