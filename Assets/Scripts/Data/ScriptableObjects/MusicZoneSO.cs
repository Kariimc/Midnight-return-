using UnityEngine;
using UnityEngine.Audio;
using MidnightReturn.Map;

namespace MidnightReturn.Data
{
    // Defines the full audio identity of a zone: layered music stems, ambient bed,
    // and reverb character. The MusicDirector resolves one of these per zone and
    // hands the stems to the AudioManager for layered playback + adaptive mixing.
    //
    // STEM CONTRACT (for adaptive layering to sound right):
    //   ExplorationTrack and CombatLayer should be the SAME length / tempo / key,
    //   authored as parallel stems so the combat layer can fade in over the base
    //   without re-syncing. CombatLayer plays continuously at volume 0 and rises
    //   with combat intensity. BossTrack is a standalone replacement (own loop).
    [CreateAssetMenu(fileName = "MusicZone_", menuName = "MidnightReturn/Music Zone")]
    public class MusicZoneSO : ScriptableObject
    {
        [Header("Identity")]
        public ZoneType Zone;
        public string   DisplayName;

        [Header("Music Stems")]
        [Tooltip("Base exploration loop — always playing while in this zone.")]
        public AudioClip ExplorationTrack;
        [Tooltip("Combat stem — parallel to exploration, faded in by combat intensity.")]
        public AudioClip CombatLayer;
        [Tooltip("Standalone boss loop — replaces the zone music during boss fights.")]
        public AudioClip BossTrack;

        [Header("Ambient Bed")]
        [Tooltip("Looping environmental texture — wind, drips, distant chains.")]
        public AudioClip AmbientBed;
        [Range(0f, 1f)] public float AmbientVolume = 0.4f;

        [Header("Mix")]
        [Range(0f, 1f)] public float MusicVolume     = 0.7f;
        [Range(0f, 1f)] public float CombatLayerMax  = 0.85f;
        [Tooltip("Seconds to crossfade when entering this zone.")]
        public float CrossfadeTime = 2.0f;

        [Header("Reverb")]
        [Tooltip("Reverb character of the space — applied to the SFX bus.")]
        public AudioReverbPreset Reverb = AudioReverbPreset.StoneCorridor;
        [Range(0f, 1f)] public float ReverbWet = 0.5f;

        [Header("Mixer Snapshot (optional)")]
        [Tooltip("If an AudioMixer is used, transition to this snapshot on entry.")]
        public AudioMixerSnapshot Snapshot;
    }
}
