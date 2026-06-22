using UnityEngine;
using MidnightReturn.Systems.Rendering;

namespace MidnightReturn.Player
{
    // ══════════════════════════════════════════════════════════════════════════
    //  PlayerSpriteContent — code-authored SpriteSheetDataSO factory for the
    //  generated Alucard sheets (no hand-written .asset files), mirroring the
    //  VerticalSliceContent pattern.
    //
    //  TWO atlas layouts, each shared by its v1 (original) and v2 (anime-samurai)
    //  PNG — both versions were generated from the SAME row contract, so the
    //  StartFrame / FrameCount maps below apply to either texture:
    //
    //    CORE     (16 rows) → Alucard_Core_v1.png     / Alucard_Core_v2.png
    //    ADVANCED (10 rows) → Alucard_Advanced_v1.png / Alucard_Advanced_v2.png
    //
    //  GRID ASSUMPTION (important):
    //    Columns = the widest row's frame count; Rows = the row count. Frames are
    //    indexed row-major: absFrame = rowIndex * Columns + col. Because these are
    //    AI-generated sheets, the sprites are NOT guaranteed to sit on a perfectly
    //    uniform pixel grid — expect to fine-tune Columns / per-clip StartFrame, or
    //    re-slice into a clean atlas. Everything here is a structured starting point
    //    keyed to the generation prompt, not a pixel-measured slice.
    // ══════════════════════════════════════════════════════════════════════════
    public static class PlayerSpriteContent
    {
        // ── CORE sheet: 16 rows, widest row (Walk) = 10 frames ──────────────────
        public const int CORE_COLUMNS = 10;
        public const int CORE_ROWS    = 16;

        // ── ADVANCED sheet: 10 rows, widest row = 8 frames ──────────────────────
        public const int ADV_COLUMNS  = 8;
        public const int ADV_ROWS     = 10;

        // Builds the CORE atlas config. Covers the full PlayerSpriteController
        // clip contract (Idle/Run/Jump/Fall/Dash/WallSlide/Attack1-3/AirAttack)
        // plus Walk/Crouch/Hurt/Death/Climb for FSM states that already exist.
        public static SpriteSheetDataSO BuildCore(Texture2D sheet)
        {
            var so = ScriptableObject.CreateInstance<SpriteSheetDataSO>();
            so.name    = "PlayerSheet_Core";
            so.Sheet   = sheet;
            so.Columns = CORE_COLUMNS;
            so.Rows    = CORE_ROWS;

            int C = CORE_COLUMNS;
            so.Clips = new[]
            {
                Clip("Idle",      0 * C, 8,  8f,  true),  // ROW 1  IDLE        (8)
                Clip("Walk",      1 * C, 10, 10f, true),  // ROW 2  WALK        (10)
                Clip("Run",       2 * C, 8,  12f, true),  // ROW 3  RUN         (8)
                Clip("Crouch",    3 * C, 4,  8f,  true),  // ROW 4  CROUCH      (4)
                Clip("Jump",      4 * C, 6,  12f, false), // ROW 5  JUMP ASCENT (6)
                Clip("JumpApex",  5 * C, 4,  10f, true),  // ROW 6  JUMP APEX   (4)
                Clip("Fall",      6 * C, 6,  10f, true),  // ROW 7  FALL        (6)
                Clip("Dash",      7 * C, 6,  16f, true),  // ROW 8  DASH        (6)
                Clip("WallSlide", 8 * C, 6,  8f,  true),  // ROW 9  WALL SLIDE  (6)
                Clip("Attack1",   9 * C, 6,  14f, false), // ROW 10 ATTACK 1    (6)
                Clip("Attack2",  10 * C, 6,  14f, false), // ROW 11 ATTACK 2    (6)
                Clip("Attack3",  11 * C, 8,  16f, false), // ROW 12 ATTACK 3    (8)
                Clip("AirAttack",12 * C, 6,  14f, false), // ROW 13 AIR ATTACK  (6)
                Clip("Hurt",     13 * C, 4,  10f, false), // ROW 14 HURT        (4)
                Clip("Death",    14 * C, 8,  10f, false), // ROW 15 DEATH       (8)
                Clip("Climb",    15 * C, 8,  8f,  true),  // ROW 16 CLIMB       (8)
            };
            return so;
        }

        // Builds the ADVANCED atlas config (move library: turn/back-dash/sub-weapon/
        // spell/dragon-kick/double-jump/air-dash/landing/wall-jump/kneel). These map
        // to the project's extra FSM states once PlayerSpriteController routes them
        // (or use PlayerSpriteBootstrap preview mode to eyeball every row).
        public static SpriteSheetDataSO BuildAdvanced(Texture2D sheet)
        {
            var so = ScriptableObject.CreateInstance<SpriteSheetDataSO>();
            so.name    = "PlayerSheet_Advanced";
            so.Sheet   = sheet;
            so.Columns = ADV_COLUMNS;
            so.Rows    = ADV_ROWS;

            int C = ADV_COLUMNS;
            so.Clips = new[]
            {
                Clip("TurnAround", 0 * C, 6, 16f, false), // ROW 1  TURN AROUND      (6)
                Clip("BackDash",   1 * C, 8, 18f, false), // ROW 2  BACK DASH        (8)
                Clip("SubWeapon",  2 * C, 8, 16f, false), // ROW 3  SUB-WEAPON THROW (8)
                Clip("SpellCast",  3 * C, 8, 12f, false), // ROW 4  SPELL CAST       (8)
                Clip("DragonKick", 4 * C, 8, 16f, false), // ROW 5  DRAGON KICK DIVE (8)
                Clip("DoubleJump", 5 * C, 8, 14f, false), // ROW 6  DOUBLE JUMP      (8)
                Clip("AirDash",    6 * C, 6, 18f, true),  // ROW 7  AIR DASH         (6)
                Clip("Landing",    7 * C, 5, 14f, false), // ROW 8  LANDING          (5)
                Clip("WallJump",   8 * C, 6, 14f, false), // ROW 9  WALL JUMP        (6)
                Clip("KneelSave",  9 * C, 6, 8f,  false), // ROW 10 KNEEL / SAVE     (6)
            };
            return so;
        }

        private static SpriteClip Clip(string name, int start, int count, float fps, bool loop) =>
            new SpriteClip { Name = name, StartFrame = start, FrameCount = count, Fps = fps, Loop = loop };
    }
}
