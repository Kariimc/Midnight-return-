using System;
using UnityEngine;
using MidnightReturn.Utils;

namespace MidnightReturn.Player
{
    [Serializable]
    public class StatBlock
    {
        public int Level    = 1;
        public int Exp      = 0;
        public int ExpToNext= 100;

        // Primary stats (SotN faithful)
        public int Str = 10;
        public int Con = 10;
        public int Int = 10;
        public int Lck = 10;

        // Derived
        public int Hp,    MaxHp   = 120;
        public int Mp,    MaxMp   = 50;
        public int Atk          = 10;
        public int Def           = 5;

        // Elemental resistances (0–100, negative = weakness)
        public float FireRes, IceRes, LightningRes, DarkRes, HolyRes, PoisonRes;

        public StatBlock() { Hp = MaxHp; Mp = MaxMp; }
    }

    public static class PlayerStatsSystem
    {
        private static readonly System.Random _rng = new();

        public static int ExpThreshold(int level) =>
            Mathf.FloorToInt(100 * Mathf.Pow(1.45f, level - 1));

        public static bool AddExp(StatBlock s, int amount)
        {
            s.Exp += amount;
            bool leveled = false;
            while (s.Exp >= s.ExpToNext)
            {
                s.Exp -= s.ExpToNext;
                LevelUp(s);
                leveled = true;
            }
            return leveled;
        }

        private static void LevelUp(StatBlock s)
        {
            s.Level++;
            s.ExpToNext = ExpThreshold(s.Level);

            // Stat growth with variance (SotN-style)
            s.MaxHp  += Mathf.FloorToInt(5  + s.Con * 0.4f + (float)_rng.NextDouble() * 4f);
            s.MaxMp  += Mathf.FloorToInt(2  + s.Int * 0.25f + (float)_rng.NextDouble() * 3f);
            s.Atk    += Mathf.FloorToInt(0.3f + s.Str * 0.05f);
            s.Def    += Mathf.FloorToInt(0.2f + s.Con * 0.04f);
            s.Str++;
            s.Con++;
            s.Int++;
            if (_rng.NextDouble() > 0.7) s.Lck++;

            // Full restore on level up (SotN tradition)
            s.Hp = s.MaxHp;
            s.Mp = s.MaxMp;
        }

        public static (int damage, bool crit) CalcDamage(StatBlock attacker, int defStat, float variance = 0.1f)
        {
            float baseDmg  = Mathf.Max(1, attacker.Atk - Mathf.FloorToInt(defStat * 0.5f));
            float critChance = Mathf.Clamp(attacker.Lck * 0.5f, 0f, 25f) / 100f;
            bool isCrit    = (float)_rng.NextDouble() < critChance;
            float v        = 1f + ((float)_rng.NextDouble() * 2f - 1f) * variance;
            int dmg        = Mathf.FloorToInt(baseDmg * v * (isCrit ? 2f : 1f));
            return (dmg, isCrit);
        }
    }
}
