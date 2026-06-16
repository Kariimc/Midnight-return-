using System.Collections.Generic;
using UnityEngine;

namespace MidnightReturn.Map
{
    // ══════════════════════════════════════════════════════════════════
    //  RoomGraph  — SO that owns the full castle room graph.
    //  Assign all RoomDataSO assets here; RoomManager calls Init()
    //  once on Start to build the O(1) lookup dictionary.
    // ══════════════════════════════════════════════════════════════════
    [CreateAssetMenu(menuName = "MidnightReturn/Room Graph", fileName = "CastleGraph")]
    public class RoomGraph : ScriptableObject
    {
        [Header("Castle Rooms (assign every RoomDataSO here)")]
        public RoomDataSO[] Rooms;

        [Header("Start")]
        public string StartRoomId = "entrance_hall_01";

        // Runtime — built by Init(), not serialised
        [System.NonSerialized]
        private Dictionary<string, RoomDataSO> _map;

        public void Init()
        {
            _map = new Dictionary<string, RoomDataSO>(Rooms.Length);
            foreach (var r in Rooms)
                if (r != null) _map[r.RoomId] = r;
        }

        public RoomDataSO GetRoom(string id)
        {
            if (_map == null) Init();
            return _map.TryGetValue(id, out var r) ? r : null;
        }

        // Returns all directly connected rooms (ignores lock state).
        public RoomDataSO[] GetNeighbors(string roomId)
        {
            var room = GetRoom(roomId);
            if (room == null || room.Connections == null) return System.Array.Empty<RoomDataSO>();

            var result = new List<RoomDataSO>(room.Connections.Length);
            foreach (var c in room.Connections)
            {
                var neighbor = GetRoom(c.TargetRoomId);
                if (neighbor != null) result.Add(neighbor);
            }
            return result.ToArray();
        }

        // Returns whether a door connection is passable given current
        // inventory. "Open" doors always pass; "Locked" check inventory.
        public bool CanPass(RoomConnection conn, Core.SaveData save)
        {
            return conn.DoorType switch
            {
                DoorType.Open      => true,
                DoorType.OneWay    => true,  // entry check handled by direction
                DoorType.Locked    => !string.IsNullOrEmpty(conn.UnlockItemId)
                                      && save.UnlockedDoors.Contains(conn.UnlockItemId),
                DoorType.Breakable => save.UnlockedDoors.Contains(conn.TargetRoomId + "_wall"),
                _                  => false,
            };
        }
    }
}
