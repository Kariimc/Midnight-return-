using System;
using UnityEngine;
using UnityEngine.Audio;

namespace MidnightReturn.Map
{
    public enum DoorDirection { North, South, East, West }
    public enum DoorType      { Open, Locked, Breakable, OneWay }
    public enum RoomType      { Normal, BossRoom, SaveRoom, ShopRoom, SecretRoom, Transition }
    public enum ZoneType      { EntranceHall, Catacombs, CursedLibrary, Clocktower, ThroneRoom }

    [Serializable]
    public struct RoomConnection
    {
        public string       TargetRoomId;
        public DoorDirection Direction;       // direction the door faces from THIS room
        public DoorType     DoorType;
        public string       UnlockItemId;     // empty = always open
        // Scene-space positions (set in Inspector or by level designer)
        public Vector3      DoorWorldPos;     // where the door trigger sits in this room
        public Vector3      SpawnWorldPos;    // where the player spawns in TARGET room from this door
    }

    [CreateAssetMenu(menuName = "MidnightReturn/Room Data", fileName = "Room_")]
    public class RoomDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string   RoomId;
        public string   DisplayName;   // shown in HUD when entering
        public string   SceneName;     // Unity scene to load (additive)

        [Header("Classification")]
        public RoomType Type = RoomType.Normal;
        public ZoneType Zone = ZoneType.EntranceHall;

        [Header("Map Layout (grid units)")]
        public Vector2Int MapPosition; // top-left cell on minimap grid
        public Vector2Int MapSize = Vector2Int.one; // width × height in cells

        [Header("Connections")]
        public RoomConnection[] Connections;

        [Header("Ambient")]
        public AudioClip  MusicTrack;
        public Color      AmbientLightColor = new Color(0.05f, 0.04f, 0.08f);
        [Range(0f, 1f)] public float FogDensity = 0.3f;

        [Header("Flags")]
        public bool IsStartRoom;

        // Returns the connection whose direction matches, or null.
        public RoomConnection? GetConnection(DoorDirection from)
        {
            foreach (var c in Connections)
                if (c.Direction == from) return c;
            return null;
        }

        // Opposite direction — used to find spawn point in target room.
        public static DoorDirection Opposite(DoorDirection d) => d switch
        {
            DoorDirection.North => DoorDirection.South,
            DoorDirection.South => DoorDirection.North,
            DoorDirection.East  => DoorDirection.West,
            _                   => DoorDirection.East,
        };
    }
}
