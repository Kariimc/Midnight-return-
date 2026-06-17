using UnityEngine;
using MidnightReturn.Data;
using MidnightReturn.Level;
using MidnightReturn.Map;

namespace MidnightReturn.VerticalSlice
{
    // ══════════════════════════════════════════════════════════════════════════
    //  CatacombsContent — code-authored layout for the second playable room.
    //
    //  Room: "Catacombs — Shattered Hall" (Zone: Catacombs)
    //  Vibe: open, eerie, vast vertical space. Player descends via three
    //  cascading platforms — High (entry) → Mid → Low — over a massive void
    //  before reaching the right floor and the exit.
    //
    //    ┌────────────────────────────────────────────────────────────────┐ ← ceiling
    //    │  ▓▓                           ▓▓                              │
    //    │  ▓▓ stalactites               ▓▓                              │
    //    │ ══════════                    ▓▓                              │ ← HIGH platform (entry)
    //    │                ══════════════                                 │ ← MID platform
    //    │                               ═══════════                     │ ← LOW platform
    //    │ ░░░░░░░            void              ░░░░░░         ══════   │ ← floor (left + right)
    //    └────────────────────────────────────────────────────────────────┘
    // ══════════════════════════════════════════════════════════════════════════
    public static class CatacombsContent
    {
        // ── Room dimensions (tiles) ────────────────────────────────────────────
        public const int   W = 44;
        public const int   H = 16;
        public const float TILE = 1f;

        // ── Tile atlas indices (shared with VerticalSliceContent) ──────────────
        public const int FLOOR = 0;
        public const int WALL  = 1;
        public const int BRICK = 8;
        public const int EMPTY = -1;

        // ── Feature coordinates (grid space, row 0 = top) ─────────────────────
        public const int FLOOR_TOP_ROW = 14;    // floor occupies rows 14–15

        // Three cascading platforms (player descends right)
        public const int HIGH_PLAT_ROW = 5, HIGH_PLAT_X0 = 2,  HIGH_PLAT_X1 = 11;
        public const int MID_PLAT_ROW  = 8, MID_PLAT_X0  = 14, MID_PLAT_X1  = 23;
        public const int LOW_PLAT_ROW  = 11, LOW_PLAT_X0  = 26, LOW_PLAT_X1  = 36;

        // Floor: left landing only (cols 1-6) + right landing (cols 32-42)
        // Everything in between is void.
        public const int LEFT_FLOOR_X1  = 6;
        public const int RIGHT_FLOOR_X0 = 32;

        // Stalactites (2-wide solid columns hanging from ceiling)
        public const int STALA1_COL = 9,  STALA1_ROWS = 5;
        public const int STALA2_COL = 24, STALA2_ROWS = 4;

        // Spawn (left ledge entry) + exit (right floor)
        public const int SPAWN_COL    = 3;
        public const int EXIT_DOOR_COL = 42;

        // World-space helpers (same convention as VerticalSliceContent)
        public static float SurfaceY(int row) => (H - 1 - row) * TILE + TILE;
        public static float ColX(int col)     => col * TILE + TILE * 0.5f;

        // ══════════════════════════════════════════════════════════════════════
        //  Tilemap layers
        // ══════════════════════════════════════════════════════════════════════
        public static TilemapLayerSO BuildMainLayer()
        {
            var g = NewGrid(EMPTY);

            // Ceiling + side walls
            SetRow(g, 0, 0, W - 1, WALL);
            SetCol(g, 0, 0, H - 1, WALL);
            SetCol(g, W - 1, 0, H - 1, WALL);

            // Floor — left landing + right landing only (void in between)
            SetRect(g, 1,              FLOOR_TOP_ROW, LEFT_FLOOR_X1,  H - 1, FLOOR);
            SetRect(g, RIGHT_FLOOR_X0, FLOOR_TOP_ROW, W - 2,          H - 1, FLOOR);

            // Three cascading platforms
            SetRow(g, HIGH_PLAT_ROW, HIGH_PLAT_X0, HIGH_PLAT_X1, FLOOR);
            SetRow(g, MID_PLAT_ROW,  MID_PLAT_X0,  MID_PLAT_X1,  FLOOR);
            SetRow(g, LOW_PLAT_ROW,  LOW_PLAT_X0,  LOW_PLAT_X1,  FLOOR);

            // Stalactites (hanging from ceiling)
            SetRect(g, STALA1_COL, 0, STALA1_COL + 1, STALA1_ROWS, WALL);
            SetRect(g, STALA2_COL, 0, STALA2_COL + 1, STALA2_ROWS, WALL);

            var layer = ScriptableObject.CreateInstance<TilemapLayerSO>();
            layer.name           = "Catacombs_Main";
            layer.Width          = W;
            layer.Height         = H;
            layer.Tiles          = g;
            layer.ZDepth         = 0f;
            layer.BuildColliders = true;
            return layer;
        }

        public static TilemapLayerSO BuildBackgroundLayer()
        {
            var g = NewGrid(BRICK);

            var layer = ScriptableObject.CreateInstance<TilemapLayerSO>();
            layer.name           = "Catacombs_BG";
            layer.Width          = W;
            layer.Height         = H;
            layer.Tiles          = g;
            layer.ZDepth         = 3f;
            layer.BuildColliders = false;
            return layer;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Tile palette (identical setup to VerticalSliceContent)
        // ══════════════════════════════════════════════════════════════════════
        public static TileSetSO BuildTileSet(Texture2D atlas, Material material)
        {
            var set = ScriptableObject.CreateInstance<TileSetSO>();
            set.name             = "Catacombs_TileSet";
            set.Atlas            = atlas;
            set.Material         = material;
            set.Columns          = 8;
            set.Rows             = 8;
            set.TileWorldSize    = TILE;
            set.SolidTileIndices = new[] { FLOOR, WALL };
            return set;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Placeholder enemies — roster TBD (confirmed by designer pass).
        //  Named "Shade" and "Wraith" as stand-ins; darker tuning than Room 1.
        // ══════════════════════════════════════════════════════════════════════
        public static EnemyDataSO BuildShade()
        {
            var d = ScriptableObject.CreateInstance<EnemyDataSO>();
            d.name           = "Catacombs_Shade";
            d.EnemyId        = "shade";
            d.DisplayName    = "Shade";
            d.Zone           = EnemyZone.Catacombs;
            d.MaxHp          = 35;
            d.Attack         = 14;
            d.Defense        = 3;
            d.ExpReward      = 30;
            d.Behavior       = AIBehavior.Patrol;
            d.MoveSpeed      = 2.2f;
            d.DetectionRange = 6f;
            d.AttackRange    = 1.3f;
            d.AggroRange     = 6f;
            d.AttackCooldown = 1.4f;
            d.AttackWindup   = 0.3f;
            d.DeathColor     = new Color(0.5f, 0.5f, 0.65f);
            return d;
        }

        public static EnemyDataSO BuildWraith()
        {
            var d = ScriptableObject.CreateInstance<EnemyDataSO>();
            d.name           = "Catacombs_Wraith";
            d.EnemyId        = "wraith";
            d.DisplayName    = "Wraith";
            d.Zone           = EnemyZone.Catacombs;
            d.MaxHp          = 25;
            d.Attack         = 18;
            d.Defense        = 0;
            d.ExpReward      = 35;
            d.Behavior       = AIBehavior.Patrol;
            d.MoveSpeed      = 2.8f;
            d.DetectionRange = 8f;
            d.AttackRange    = 1.3f;
            d.AggroRange     = 8f;
            d.AttackCooldown = 1.1f;
            d.AttackWindup   = 0.25f;
            d.DeathColor     = new Color(0.7f, 0.4f, 0.9f);
            return d;
        }

        // ── Grid helpers ───────────────────────────────────────────────────────
        private static int[] NewGrid(int fill)
        {
            var g = new int[W * H];
            for (int i = 0; i < g.Length; i++) g[i] = fill;
            return g;
        }

        private static void Set(int[] g, int x, int y, int v)
        {
            if (x < 0 || x >= W || y < 0 || y >= H) return;
            g[y * W + x] = v;
        }

        private static void SetRow(int[] g, int y, int x0, int x1, int v)
        { for (int x = x0; x <= x1; x++) Set(g, x, y, v); }

        private static void SetCol(int[] g, int x, int y0, int y1, int v)
        { for (int y = y0; y <= y1; y++) Set(g, x, y, v); }

        private static void SetRect(int[] g, int x0, int y0, int x1, int y1, int v)
        { for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++) Set(g, x, y, v); }
    }
}
