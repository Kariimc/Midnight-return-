using NUnit.Framework;
using MidnightReturn.Player;

namespace MidnightReturn.Tests.EditMode
{
    // Deterministic parts of Assets/Scripts/Player/PlayerStats.cs (PlayerStatsSystem).
    // Level-up stat growth uses an internal System.Random, but the exp curve,
    // the exp carryover on level-up, and the damage-reduction formula are all
    // deterministic and asserted here with exact expected values.
    public class PlayerStatsTests
    {
        // ExpThreshold(level) = FloorToInt(100 * 1.45^(level-1))
        [Test]
        public void ExpThreshold_MatchesCurve()
        {
            Assert.AreEqual(100, PlayerStatsSystem.ExpThreshold(1));
            Assert.AreEqual(145, PlayerStatsSystem.ExpThreshold(2));
            Assert.AreEqual(210, PlayerStatsSystem.ExpThreshold(3));
        }

        [Test]
        public void AddExp_BelowThreshold_DoesNotLevel()
        {
            var s = new StatBlock();               // Level 1, ExpToNext 100
            bool leveled = PlayerStatsSystem.AddExp(s, 50);
            Assert.IsFalse(leveled);
            Assert.AreEqual(1, s.Level);
            Assert.AreEqual(50, s.Exp);
            Assert.AreEqual(100, s.ExpToNext);
        }

        [Test]
        public void AddExp_ExactThreshold_LevelsOnce_AndCarriesRemainder()
        {
            var s = new StatBlock();               // Level 1, ExpToNext 100
            bool leveled = PlayerStatsSystem.AddExp(s, 100);
            Assert.IsTrue(leveled);
            Assert.AreEqual(2, s.Level);
            Assert.AreEqual(0, s.Exp);             // 100 - 100 carried over
            Assert.AreEqual(145, s.ExpToNext);     // ExpThreshold(2)
        }

        // 300 exp on a fresh block cascades two level-ups:
        //   300 >= 100 -> Exp 200, Level 2, ExpToNext 145
        //   200 >= 145 -> Exp 55,  Level 3, ExpToNext 210
        //   55  <  210 -> stop
        [Test]
        public void AddExp_CascadesMultipleLevels_WithCorrectCarryover()
        {
            var s = new StatBlock();
            bool leveled = PlayerStatsSystem.AddExp(s, 300);
            Assert.IsTrue(leveled);
            Assert.AreEqual(3, s.Level);
            Assert.AreEqual(55, s.Exp);
            Assert.AreEqual(210, s.ExpToNext);
        }

        [Test]
        public void LevelUp_FullyRestoresHpAndMp()
        {
            var s = new StatBlock { Hp = 1, Mp = 0 };
            PlayerStatsSystem.AddExp(s, 100);      // forces one level-up
            Assert.AreEqual(s.MaxHp, s.Hp);
            Assert.AreEqual(s.MaxMp, s.Mp);
        }

        // CalcDamage: baseDmg = Max(1, Atk - Floor(defStat * 0.5)); final = Floor(baseDmg * variance * crit).
        // With Lck = 0 crit chance is 0, and with variance = 0 the multiplier is exactly 1,
        // so damage is deterministic: exactly baseDmg, never a crit.
        [Test]
        public void CalcDamage_NoLuckNoVariance_AppliesHalfDefense_Exactly()
        {
            var atk = new StatBlock { Atk = 100, Lck = 0 };
            var (dmg, crit) = PlayerStatsSystem.CalcDamage(atk, 40, variance: 0f);
            Assert.AreEqual(80, dmg);              // 100 - Floor(40*0.5)=20 => 80
            Assert.IsFalse(crit);
        }

        [Test]
        public void CalcDamage_ZeroDefense_DealsFullAttack()
        {
            var atk = new StatBlock { Atk = 100, Lck = 0 };
            var (dmg, _) = PlayerStatsSystem.CalcDamage(atk, 0, variance: 0f);
            Assert.AreEqual(100, dmg);
        }

        [Test]
        public void CalcDamage_OverwhelmingDefense_FloorsAtOne()
        {
            var atk = new StatBlock { Atk = 100, Lck = 0 };
            var (dmg, _) = PlayerStatsSystem.CalcDamage(atk, 1000, variance: 0f);
            Assert.AreEqual(1, dmg);               // Max(1, 100 - 500) => 1
        }

        // Variance stays within +/-10% and crit (Lck 0) never fires, so across many
        // rolls damage is bounded to Floor(100 * [0.9..1.1]) = [90..110].
        [Test]
        public void CalcDamage_VarianceStaysWithinBounds()
        {
            var atk = new StatBlock { Atk = 100, Lck = 0 };
            for (int i = 0; i < 1000; i++)
            {
                var (dmg, crit) = PlayerStatsSystem.CalcDamage(atk, 0, variance: 0.1f);
                Assert.IsFalse(crit);
                Assert.GreaterOrEqual(dmg, 90);
                Assert.LessOrEqual(dmg, 110);
            }
        }
    }
}
