using UnityEngine;
using MidnightReturn.Data;
using MidnightReturn.Level;
using MidnightReturn.Map;

namespace MidnightReturn.VerticalSlice
{
    // ══════════════════════════════════════════════════════════════════════════
    //  ClocktowerContent — code-authored layout for the third playable room.
    //
    //  Room: "Clocktower — Gearworks Ascent" (Zone: Clocktower)
    //  Vibe: vertical mechanical tower. The player ASCENDS via four staggered
    //  platforms, double-jumping across gear-cog stepping stones, dodging a
    //  swinging pendulum over the central shaft, toward the top-right exit.
    //
    //    ┌────────────────────────────────────────────────────────────────┐ ← ceiling
    //    │                                              ════════════════  │ ← TOP platform (exit)
    //    │              ◷ pendulum                                         │
    //    │                          ════════════                          │ ← C platform
    //    │            ⚙ gear                                               │
    //    │        ════════════                                            │ ← B platform
    //    │     ⚙ gear                                                      │
    //    │  ════════                                                       │ ← A platform
    //    │ ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░  (entry floor)  │ ← floor
    //    └────────────────────────────────────────────────────────────────┘
    // ══════════════════════════════════════════════════════════════════════════
    public static class ClocktowerContent
    {
        // ── Room dimensions (tiles) ────────────────────────────────────────────
        public const int   W = 44;
        public const int   H = 16;
        public const float TILE = 1f;

        // ── Tile atlas indices (shared with the other rooms) ───────────────────
        public const int FLOOR = 0;
        public const int WALL  = 1;
        public const int BRICK = 8;
        public const int EMPTY = -1;

        // ── Feature coordinates (grid space, row 0 = top) ─────────────────────
        public const int FLOOR_TOP_ROW = 14;    // entry floor occupies rows 14–15

        // Four staggered platforms (player ascends left → right)
        public const int PLAT_A_ROW = 11, PLAT_A_X0 = 2,  PLAT_A_X1 = 9;
        public const int PLAT_B_ROW = 8,  PLAT_B_X0 = 13, PLAT_B_X1 = 21;
        public const int PLAT_C_ROW = 5,  PLAT_C_X0 = 24, PLAT_C_X1 = 31;
        public const int PLAT_TOP_ROW = 2, PLAT_TOP_X0 = 34, PLAT_TOP_X1 = 42;

        // Gear-cog stepping stones (2×2 solid blocks bridging the big gaps)
        public const int GEAR1_COL = 11, GEAR1_ROW = 10;  // between A and B
        public const int GEAR2_COL = 22, GEAR2_ROW = 7;   // between B and C
        public const int GEAR3_COL = 32, GEAR3_ROW = 4;   // between C and TOP

        // Swinging pendulum hazard over the central shaft (anchored to ceiling)
        public const int PENDULUM_COL = 27;

        // Spawn (bottom-left entry) + exit (top-right toward Throne Room)
        public const int SPAWN_COL     = 3;
        public const int EXIT_DOOR_COL = 42;

        // World-space helpers (same convention as the other rooms)
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

            // Entry floor — full width (solid landing)
            SetRect(g, 1, FLOOR_TOP_ROW, W - 2, H - 1, FLOOR);

            // Four staggered ascent platforms
            SetRow(g, PLAT_A_ROW,   PLAT_A_X0,   PLAT_A_X1,   FLOOR);
            SetRow(g, PLAT_B_ROW,   PLAT_B_X0,   PLAT_B_X1,   FLOOR);
            SetRow(g, PLAT_C_ROW,   PLAT_C_X0,   PLAT_C_X1,   FLOOR);
            SetRow(g, PLAT_TOP_ROW, PLAT_TOP_X0, PLAT_TOP_X1, FLOOR);

            // Gear-cog stepping stones (2×2 solids)
            SetRect(g, GEAR1_COL, GEAR1_ROW, GEAR1_COL + 1, GEAR1_ROW + 1, FLOOR);
            SetRect(g, GEAR2_COL, GEAR2_ROW, GEAR2_COL + 1, GEAR2_ROW + 1, FLOOR);
            SetRect(g, GEAR3_COL, GEAR3_ROW, GEAR3_COL + 1, GEAR3_ROW + 1, FLOOR);

            var layer = ScriptableObject.CreateInstance<TilemapLayerSO>();
            layer.name           = "Clocktower_Main";
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
            layer.name           = "Clocktower_BG";
            layer.Width          = W;
            layer.Height         = H;
            layer.Tiles          = g;
            layer.ZDepth         = 3f;
            layer.BuildColliders = false;
            return layer;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Tile palette (identical setup to the other rooms)
        // ══════════════════════════════════════════════════════════════════════
        public static TileSetSO BuildTileSet(Texture2D atlas, Material material)
        {
            var set = ScriptableObject.CreateInstance<TileSetSO>();
            set.name             = "Clocktower_TileSet";
            set.Atlas            = atlas;
            set.Material         = material;
            set.Columns          = 8;
            set.Rows             = 8;
            set.TileWorldSize    = TILE;
            set.SolidTileIndices = new[] { FLOOR, WALL };
            return set;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Clocktower roster — Gear Golem (heavy patroller) + Vampire Bat (flyer).
        //  Tuned to the vertical-slice world-unit scale (player ≈120 HP), not the
        //  raw prototype pixel-scale values.
        // ══════════════════════════════════════════════════════════════════════
        public static EnemyDataSO BuildGearGolem()
        {
            var d = ScriptableObject.CreateInstance<EnemyDataSO>();
            d.name           = "Clocktower_GearGolem";
            d.EnemyId        = "gear_golem";
            d.DisplayName    = "Gear Golem";
            d.Zone           = EnemyZone.Clocktower;
            d.MaxHp          = 70;
            d.Attack         = 20;
            d.Defense        = 10;
            d.ExpReward      = 60;
            d.Behavior       = AIBehavior.PatrolJump;
            d.MoveSpeed      = 1.6f;
            d.DetectionRange = 7f;
            d.AttackRange    = 1.5f;
            d.AggroRange     = 7f;
            d.AttackCooldown = 1.8f;
            d.AttackWindup   = 0.4f;
            d.DeathColor     = new Color(1f, 0.55f, 0.1f);
            return d;
        }

        public static EnemyDataSO BuildVampireBat()
        {
            var d = ScriptableObject.CreateInstance<EnemyDataSO>();
            d.name           = "Clocktower_VampireBat";
            d.EnemyId        = "vampire_bat";
            d.DisplayName    = "Vampire Bat";
            d.Zone           = EnemyZone.Clocktower;
            d.MaxHp          = 18;
            d.Attack         = 10;
            d.Defense        = 0;
            d.ExpReward      = 15;
            d.Behavior       = AIBehavior.Flying;
            d.MoveSpeed      = 3.4f;
            d.DetectionRange = 9f;
            d.AttackRange    = 1.2f;
            d.AggroRange     = 9f;
            d.AttackCooldown = 1.0f;
            d.AttackWindup   = 0.2f;
            d.DeathColor     = new Color(0.4f, 0f, 0f);
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
