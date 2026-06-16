using UnityEngine;
using UnityEngine.UIElements;
using MidnightReturn.Map;
using MidnightReturn.Utils;

namespace MidnightReturn.UI
{
    // ══════════════════════════════════════════════════════════════════
    //  MinimapController — UI Toolkit minimap.
    //
    //  Renders the castle room graph as a grid of VisualElements.
    //  Visited rooms are revealed; unvisited rooms are fogged (#000).
    //  Current room pulses gold. Room-type icons overlay each cell.
    //
    //  UXML: expects a VisualElement with name="minimap-root".
    //  Each room cell is created at runtime — no UXML per room needed.
    // ══════════════════════════════════════════════════════════════════
    [RequireComponent(typeof(UIDocument))]
    public sealed class MinimapController : MonoBehaviour
    {
        [Header("Graph reference")]
        [SerializeField] RoomGraph _graph;

        [Header("Cell sizing")]
        [SerializeField] int _cellPx   = 12;  // pixel size per grid cell
        [SerializeField] int _gapPx    = 2;   // gap between cells

        UIDocument        _doc;
        VisualElement     _root;
        VisualElement[,]  _cells;
        Vector2Int        _gridMin;
        Vector2Int        _gridMax;

        // Zone tint palette (matches HDRPSettings.md)
        static readonly Color32[] ZoneColors =
        {
            new Color32(0xCC, 0x88, 0x22, 0xFF), // EntranceHall  — warm orange
            new Color32(0x22, 0x66, 0x88, 0xFF), // Catacombs     — cold blue-green
            new Color32(0x99, 0x77, 0x22, 0xFF), // CursedLibrary — amber
            new Color32(0x88, 0x99, 0xAA, 0xFF), // Clocktower    — silver
            new Color32(0x88, 0x11, 0x44, 0xFF), // ThroneRoom    — crimson
        };

        static readonly string[] RoomIcons =
        {
            "",     // Normal
            "☠",   // BossRoom
            "✦",   // SaveRoom
            "⚙",   // ShopRoom
            "?",   // SecretRoom
            "▶",   // Transition
        };

        void Awake()
        {
            _doc = GetComponent<UIDocument>();
        }

        void OnEnable()
        {
            EventBus.Subscribe<RoomTransitionCompleteEvent>(OnRoomChanged);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<RoomTransitionCompleteEvent>(OnRoomChanged);
        }

        void Start()
        {
            _root = _doc.rootVisualElement.Q("minimap-root");
            if (_root == null)
            {
                Debug.LogWarning("MinimapController: no VisualElement named 'minimap-root' found.");
                return;
            }
            _graph?.Init();
            BuildGrid();
            RefreshAll();
        }

        void BuildGrid()
        {
            if (_graph?.Rooms == null) return;

            // Compute bounds of all rooms
            _gridMin = new Vector2Int(int.MaxValue, int.MaxValue);
            _gridMax = new Vector2Int(int.MinValue, int.MinValue);
            foreach (var r in _graph.Rooms)
            {
                if (r == null) continue;
                _gridMin = Vector2Int.Min(_gridMin, r.MapPosition);
                _gridMax = Vector2Int.Max(_gridMax, r.MapPosition + r.MapSize - Vector2Int.one);
            }

            int w = _gridMax.x - _gridMin.x + 1;
            int h = _gridMax.y - _gridMin.y + 1;
            _cells = new VisualElement[w, h];

            // Create container sized to fit grid
            int totalW = w * (_cellPx + _gapPx);
            int totalH = h * (_cellPx + _gapPx);
            _root.style.width  = totalW;
            _root.style.height = totalH;
            _root.style.position = Position.Relative;

            foreach (var room in _graph.Rooms)
            {
                if (room == null) continue;

                for (int gy = 0; gy < room.MapSize.y; gy++)
                for (int gx = 0; gx < room.MapSize.x; gx++)
                {
                    int cx = room.MapPosition.x + gx - _gridMin.x;
                    int cy = room.MapPosition.y + gy - _gridMin.y;
                    if (cx < 0 || cy < 0 || cx >= w || cy >= h) continue;

                    var cell = new VisualElement();
                    cell.name = $"cell_{room.RoomId}_{gx}_{gy}";
                    cell.style.position = Position.Absolute;
                    cell.style.left     = cx * (_cellPx + _gapPx);
                    cell.style.top      = cy * (_cellPx + _gapPx);
                    cell.style.width    = _cellPx;
                    cell.style.height   = _cellPx;
                    cell.style.backgroundColor = Color.black; // fogged default
                    cell.userData = room;

                    // Icon label (room type, only shown when visited)
                    if (gx == 0 && gy == 0 && room.Type != RoomType.Normal)
                    {
                        var icon = new Label(RoomIcons[(int)room.Type]);
                        icon.name = "icon";
                        icon.style.fontSize = _cellPx - 2;
                        icon.style.color    = Color.white;
                        icon.style.unityTextAlign = TextAnchor.MiddleCenter;
                        icon.style.display  = DisplayStyle.None; // hidden until visited
                        cell.Add(icon);
                    }

                    _root.Add(cell);
                    _cells[cx, cy] = cell;
                }
            }
        }

        void RefreshAll()
        {
            if (_graph?.Rooms == null) return;
            var save = Core.GameManager.Instance?.Save;
            var currentId = RoomManager.Instance?.CurrentRoomId;

            foreach (var room in _graph.Rooms)
            {
                if (room == null) continue;

                bool visited  = save != null && save.MapExplored.Contains(room.RoomId);
                bool isCurrent= room.RoomId == currentId;

                Color cellColor = Color.black;
                if (visited)
                    cellColor = ZoneColors[(int)room.Zone];
                if (isCurrent)
                    cellColor = new Color(1f, 0.9f, 0.3f);  // gold pulse for current

                for (int gy = 0; gy < room.MapSize.y; gy++)
                for (int gx = 0; gx < room.MapSize.x; gx++)
                {
                    int cx = room.MapPosition.x + gx - _gridMin.x;
                    int cy = room.MapPosition.y + gy - _gridMin.y;
                    int w  = _gridMax.x - _gridMin.x + 1;
                    int h  = _gridMax.y - _gridMin.y + 1;
                    if (cx < 0 || cy < 0 || cx >= w || cy >= h) continue;

                    var cell = _cells[cx, cy];
                    if (cell == null) continue;

                    cell.style.backgroundColor = cellColor;

                    // Show icon when visited
                    var icon = cell.Q<Label>("icon");
                    if (icon != null)
                        icon.style.display = visited ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }
        }

        void OnRoomChanged(RoomTransitionCompleteEvent evt)
        {
            RefreshAll();
        }
    }
}
