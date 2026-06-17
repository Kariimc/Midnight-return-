# Audio Mixer & Phase 6 Audio Setup

Phase 6 ships a layered, event-driven audio engine. The C# runs without an
AudioMixer (it falls back to direct `AudioSource` volumes), but for production
you want a proper mixer for bus routing, ducking sends, and reverb. This doc is
the wiring guide.

---

## 1. Runtime Component Rig

Put these on a persistent `__Audio` GameObject (DontDestroyOnLoad) in the boot scene:

```
__Audio
├── AudioManager        (Systems/AudioManager.cs)
└── MusicDirector       (Systems/Audio/MusicDirector.cs)
```

`AudioManager` builds all its own AudioSources at runtime (two base music
ping-pong sources, one combat-layer source, one ambient source, a 16-voice SFX
pool). You do **not** wire sources in the Inspector.

`MusicDirector` needs its **Zones** array populated with one `MusicZoneSO` per
`ZoneType` (5 zones).

---

## 2. Create the AudioMixer (optional but recommended)

`Assets → Create → Audio Mixer` → name it `MasterMixer`.

Groups:
```
Master
├── Music      (exposed param: MusicVol)
│   └── Music_Reverb send
├── Ambient    (exposed param: AmbientVol)
└── SFX        (exposed param: SfxVol)
```

Expose the **Master** group volume too (`MasterVol`).

To expose a param: select the group → right-click the **Volume** field in the
Inspector → *Expose 'Volume' to script* → rename in the Audio Mixer's
**Exposed Parameters** dropdown to match:

| Group   | Exposed name |
|---------|--------------|
| Master  | `MasterVol`  |
| Music   | `MusicVol`   |
| SFX     | `SfxVol`     |
| Ambient | `AmbientVol` |

Drag `MasterMixer` into `AudioManager._mixer`. The param-name fields default to
the names above. `AudioManager.SetMasterVolume/SetMusicVolume/...` convert
linear 0–1 to dB and call `mixer.SetFloat`.

> Without a mixer assigned, those setters still work — they scale the runtime
> AudioSource volumes directly.

---

## 3. Snapshots (per-zone mix states)

Create one snapshot per zone in the mixer (e.g. `Snap_Catacombs`,
`Snap_ThroneRoom`). Use them to shift EQ / reverb wetness / bus balance for the
space. Assign each snapshot to its `MusicZoneSO.Snapshot` field — `MusicDirector`
calls `TransitionToSnapshot()` on zone entry over `CrossfadeTime` seconds.

---

## 4. Reverb

Two options:

1. **AudioReverbFilter (simple):** add an `AudioReverbFilter` to the SFX bus
   GameObject and drag it into `AudioManager._reverbFilter`. `SetReverb(preset)`
   swaps the `reverbPreset` per zone. Each `MusicZoneSO.Reverb` picks the preset
   (StoneCorridor for halls, Cave for catacombs, etc.).
2. **Mixer reverb send (production):** add an **SFX Reverb** effect to a send bus
   and drive its wetness via snapshots. Preferred for a unified space sound.

---

## 5. MusicZoneSO — the stem contract

`Assets → Create → MidnightReturn → Music Zone`, one per zone.

| Field | Purpose |
|-------|---------|
| `ExplorationTrack` | base loop, always playing in-zone |
| `CombatLayer` | **parallel stem** — same length/tempo/key as exploration |
| `BossTrack` | standalone boss loop (replaces base) |
| `AmbientBed` | looping environmental texture (wind, drips) |
| `Reverb` / `ReverbWet` | space character |
| `MusicVolume` / `CombatLayerMax` | mix ceilings |
| `CrossfadeTime` | zone-entry crossfade seconds |
| `Snapshot` | optional mixer snapshot |

**Authoring rule:** export `ExplorationTrack` and `CombatLayer` as parallel
stems from the same session so the combat layer can fade in over the base
without re-syncing. The combat layer plays continuously at volume 0 and rises
with combat intensity.

---

## 6. Dynamic Mix — how it reacts

`MusicDirector` listens on the EventBus:

| Event | Reaction |
|-------|----------|
| `RoomTransitionCompleteEvent` | resolve zone → crossfade exploration, arm combat layer, swap ambient, set reverb |
| `PlayerAttackEvent` | intensity += 0.18 |
| `PlayerDamagedEvent` (dmg>0) | intensity += 0.40 |
| `EnemyDiedEvent` | intensity += 0.30 |
| *(per frame)* | intensity decays 0.35/s → combat-layer volume |
| `BossStartedEvent` | duck sting → swap to `BossTrack`, suspend layering |
| `BossDefeatedEvent` | return to zone exploration + combat layer |
| `PlayerDiedEvent` | fade everything to silence |

Combat-layer volume = `smoothed(intensity) × CombatLayerMax × MusicVol × Master`.
That's the adaptive layering — the music *intensifies as you fight* and settles
when you stop.

---

## 7. Foley

`PlayerAudio` (Player/PlayerAudio.cs) on the Player root drives footsteps
(timed to run state), jump/land/dash one-shots, a looping wall-slide scrape, and
a hurt grunt (on `PlayerDamagedEvent`). Assign the clip fields in the Inspector.
Footsteps route through `AudioManager.PlaySFX` (positional 3D).

---

## 8. Browser Reference

`web-prototype/public/audio-mixer-demo.html` is a live Web Audio mirror of this
system — synthesized stems, the intensity meter, zone reverb swaps, boss
override, and bus faders. Open it to *hear* the adaptive mix before authoring
real stems. Keys: `1–5` zones, `A` attack, `H` hit, `K` kill, `B` boss.
