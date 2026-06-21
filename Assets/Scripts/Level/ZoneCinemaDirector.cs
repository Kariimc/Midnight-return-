using System.Collections.Generic;
using UnityEngine;
using MidnightReturn.Data;
using MidnightReturn.Map;
using MidnightReturn.Utils;

namespace MidnightReturn.Level
{
    // ══════════════════════════════════════════════════════════════════════════
    //  ZoneCinemaDirector — FEAT-01 brain. EventBus-driven, fully decoupled.
    //
    //  On RoomTransitionCompleteEvent it resolves the matching
    //  ZoneAtmosphereProfile and pushes it to:
    //    • ProceduralZoneBackground  (the 6-band parallax scenery)
    //    • the active ScreenJuiceManager (damage CA ceiling + color grade)
    //
    //  Coexists with ZoneLightingController (HDRP light/post) — both react to the
    //  same event independently, so a zone's full "cinema" comes up with zero
    //  direct wiring between the two systems.
    // ══════════════════════════════════════════════════════════════════════════
    public sealed class ZoneCinemaDirector : MonoBehaviour
    {
        public static ZoneCinemaDirector Instance { get; private set; }

        [Header("Atmosphere Catalogue")]
        [SerializeField] private ZoneAtmosphereProfile[] _profiles;

        [Header("Scenery Renderer")]
        [SerializeField] private ProceduralZoneBackground _background;

        private readonly Dictionary<ZoneType, ZoneAtmosphereProfile> _byZone = new();

        public ZoneAtmosphereProfile Current { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            foreach (var p in _profiles)
                if (p != null) _byZone[p.Zone] = p;
        }

        private void OnEnable()  => EventBus.Subscribe<RoomTransitionCompleteEvent>(OnRoomEntered);
        private void OnDisable() => EventBus.Unsubscribe<RoomTransitionCompleteEvent>(OnRoomEntered);

        private void OnRoomEntered(RoomTransitionCompleteEvent e)
        {
            if (!_byZone.TryGetValue(e.Zone, out var profile) || profile == null) return;
            Apply(profile);
        }

        // Apply instantly (e.g. on boot before the first transition completes).
        public void ApplyImmediate(ZoneType zoneType)
        {
            if (_byZone.TryGetValue(zoneType, out var profile) && profile != null)
                Apply(profile);
        }

        private void Apply(ZoneAtmosphereProfile profile)
        {
            if (profile == Current) return;
            Current = profile;
            _background?.SwitchZone(profile);
        }
    }
}
