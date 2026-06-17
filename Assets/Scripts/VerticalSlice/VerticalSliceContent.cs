using UnityEngine;
using MidnightReturn.Data;
using MidnightReturn.Level;
using MidnightReturn.Map;

namespace MidnightReturn.VerticalSlice
{
    // ══════════════════════════════════════════════════════════════════════════
    //  VerticalSliceContent — code-authored content for the first playable room.
    //
    //  Instead of hand-writing Unity .asset files (GUIDs + meta, not authorable
    //  outside the editor), every ScriptableObject the slice needs is built here
    //  via ScriptableObject.CreateInstance. The grid is generated with rect/line
    //  helpers so the layout reads like the room and can't drift out of size.
    //
    //  Room: "Entrance Hall — Threshold" (Zone: EntranceHall)
    //    • 44×16 stone corridor, ceiling + side walls
    //    • a 4-tile pit to jump across
    //    • a low-left ledge and a high-right platform
    //    • a ladder from the floor up to the high platform (save statue on top)
    // ══════════════════════════════════════════════════════════════════════════
    public static class VerticalSliceContent
    {
        // ── Room dimensions (tiles) ───────────────────────────────────────────
        public const int   W = 44;
        public const int   H = 16;
        public const float TILE = 1f;

        // ── Tile atlas indices ────────────────────────────────────────────────
        public const int FLOOR = 0;   // solid
        public const int WALL  = 1;   // solid
        public const int BRICK = 8;   // background fill (non-solid)
        public const int EMPTY = -1;

        // ── Authored feature coordinates (grid space, row 0 = top) ────────────
        public const int FLOOR_TOP_ROW = 14;   // floor occupies rows 14–15
        public const int PIT_X0 = 20, PIT_X1 = 23;
        public const int LEDGE_ROW = 10, LEDGE_X0 = 6,  LEDGE_X1 = 12;
        public const int PLAT_ROW  = 8,  PLAT_X0  = 28, PLAT_X1  = 38;
        public const int LADDER_COL = 36;

        // World-space helper — top surface Y of a given grid row (matches
        // TileChunkBuilder's wy = (H-1-row)*TILE inversion, +1 tile for the top).
        public static float SurfaceY(int row) => (H - 1 - row) * TILE + TILE;
        public static float ColX(int col)     => col * TILE + TILE * 0.5f;

        // ══════════════════════════════════════════════════════════════════════
        //  Tilemap layers
        // ══════════════════════════════════════════════════════════════════════
        public static TilemapLayerSO BuildMainLayer()
        {
            var grid = NewGrid(EMPTY);

            // Ceiling + side walls
            SetRow(grid, 0, 0, W - 1, WALL);
            SetCol(grid, 0, 0, H - 1, WALL);
            SetCol(grid, W - 1, 0, H - 1, WALL);

            // Floor (rows 14–15) with a pit gap in the middle
            SetRect(grid, 1, FLOOR_TOP_ROW, W - 2, H - 1, FLOOR);
            SetRect(grid, PIT_X0, FLOOR_TOP_ROW, PIT_X1, H - 1, EMPTY);

            // Low-left ledge + high-right platform
            SetRect(grid, LEDGE_X0, LEDGE_ROW, LEDGE_X1, LEDGE_ROW, FLOOR);
            SetRect(grid, PLAT_X0,  PLAT_ROW,  PLAT_X1,  PLAT_ROW,  FLOOR);

            var layer = ScriptableObject.CreateInstance<TilemapLayerSO>();
            layer.name           = "VS_Main";
            layer.Width          = W;
            layer.Height         = H;
            layer.Tiles          = grid;
            layer.ZDepth         = 0f;
            layer.BuildColliders = true;
            return layer;
        }

        public static TilemapLayerSO BuildBackgroundLayer()
        {
            var grid = NewGrid(BRICK); // solid fill of background brick

            var layer = ScriptableObject.CreateInstance<TilemapLayerSO>();
            layer.name           = "VS_BG";
            layer.Width          = W;
            layer.Height         = H;
            layer.Tiles          = grid;
            layer.ZDepth         = 3f;     // behind the gameplay plane
            layer.BuildColliders = false;
            return layer;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Tile palette
        // ══════════════════════════════════════════════════════════════════════
        public static TileSetSO BuildTileSet(Texture2D atlas, Material material)
        {
            var set = ScriptableObject.CreateInstance<TileSetSO>();
            set.name             = "VS_TileSet";
            set.Atlas            = atlas;
            set.Material         = material;
            set.Columns          = 8;
            set.Rows             = 8;
            set.TileWorldSize    = TILE;
            set.SolidTileIndices = new[] { FLOOR, WALL };
            return set;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Enemy data — Entrance Hall roster (world-unit tuned, not pixel-tuned)
        // ══════════════════════════════════════════════════════════════════════
        public static EnemyDataSO BuildZombie()
        {
            var d = ScriptableObject.CreateInstance<EnemyDataSO>();
            d.name           = "VS_Zombie";
            d.EnemyId        = "zombie";
            d.DisplayName    = "Zombie";
            d.Zone           = EnemyZone.EntranceHall;
            d.MaxHp          = 30;
            d.Attack         = 8;
            d.Defense        = 2;
            d.ExpReward      = 15;
            d.Behavior       = AIBehavior.Patrol;
            d.MoveSpeed      = 2.0f;   // world units/sec (prototype used 50 px/sec)
            d.DetectionRange = 6f;
            d.AttackRange    = 1.3f;
            d.AggroRange     = 6f;
            d.AttackCooldown = 1.5f;
            d.AttackWindup   = 0.35f;
            d.DeathColor     = new Color(0.27f, 1f, 0.13f);
            return d;
        }

        public static EnemyDataSO BuildSkeleton()
        {
            var d = ScriptableObject.CreateInstance<EnemyDataSO>();
            d.name           = "VS_Skeleton";
            d.EnemyId        = "skeleton";
            d.DisplayName    = "Skeleton";
            d.Zone           = EnemyZone.EntranceHall;
            d.MaxHp          = 20;
            d.Attack         = 12;
            d.Defense        = 0;
            d.ExpReward      = 20;
            d.Behavior       = AIBehavior.Patrol;
            d.MoveSpeed      = 3.0f;
            d.DetectionRange = 7f;
            d.AttackRange    = 1.3f;
            d.AggroRange     = 7f;
            d.AttackCooldown = 1.2f;
            d.AttackWindup   = 0.3f;
            d.Resistances.Add(new DamageResistance { Type = DamageType.Holy, Percentage = -50f });
            d.DeathColor     = new Color(0.8f, 0.8f, 0.67f);
            return d;
        }

        // ── Grid helpers ──────────────────────────────────────────────────────
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
