using System;
using System.Collections.Generic;

namespace MidnightReturn.Utils
{
    // Typed, zero-GC event bus using System.Action delegates
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public static void Subscribe<T>(Action<T> handler)
        {
            var t = typeof(T);
            if (!_handlers.ContainsKey(t)) _handlers[t] = new List<Delegate>();
            _handlers[t].Add(handler);
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            if (_handlers.TryGetValue(typeof(T), out var list))
                list.Remove(handler);
        }

        public static void Emit<T>(T evt)
        {
            if (!_handlers.TryGetValue(typeof(T), out var list)) return;
            // Iterate copy to allow unsubscribe during emission
            for (int i = list.Count - 1; i >= 0; i--)
                ((Action<T>)list[i])(evt);
        }

        public static void Clear() => _handlers.Clear();
    }

    // ── Event structs (value types — zero heap alloc) ─────────────────────────
    public struct PlayerDamagedEvent  { public int Damage; public int CurrentHp; }
    public struct PlayerDiedEvent     { }
    public struct PlayerLeveledUpEvent{ public int Level; }
    public struct EnemyDiedEvent      { public string EnemyId; public int Exp; public UnityEngine.Vector3 Position; }
    public struct ItemPickedUpEvent   { public string ItemId; public int Quantity; }
    public struct RoomEnteredEvent    { public string RoomId; }
    public struct BossStartedEvent    { public string BossId; }
    public struct BossDefeatedEvent   { public string BossId; }
    public struct PlayerAttackEvent   { public UnityEngine.Vector3 Origin; public int Direction; public int ComboIndex; }
    public struct ScreenFlashEvent    { public UnityEngine.Color Color; public float Duration; }
    public struct CameraShakeEvent    { public float Intensity; public float Duration; }

    // ── Map / room events ──────────────────────────────────────────────
    public struct RoomTransitionStartedEvent  { public string FromRoomId; public string ToRoomId; }
    public struct RoomTransitionCompleteEvent { public string RoomId; public string DisplayName; public MidnightReturn.Map.ZoneType Zone; }
    public struct RoomTransitionBlockedEvent  { public string RequiredItemId; public MidnightReturn.Map.DoorType DoorType; }
    public struct SaveStatueActivatedEvent    { public string StatueId; }
}
