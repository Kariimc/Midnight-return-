using NUnit.Framework;
using UnityEngine;
using MidnightReturn.Data;

namespace MidnightReturn.Tests.EditMode
{
    // BossPhaseDataSO.PickAttack (Assets/Scripts/Data/ScriptableObjects/BossPhaseDataSO.cs)
    // is a weighted-random picker, but its cooldown filtering and empty/edge handling
    // are deterministic: it returns -1 when nothing is eligible and a valid index otherwise.
    public class BossAttackPickerTests
    {
        private BossPhaseDataSO _phase;

        [TearDown]
        public void TearDown()
        {
            if (_phase != null) Object.DestroyImmediate(_phase);
        }

        private BossPhaseDataSO MakePhase(params AttackEntry[] attacks)
        {
            var p = ScriptableObject.CreateInstance<BossPhaseDataSO>();
            p.AttackPatterns = attacks;
            return p;
        }

        [Test]
        public void PickAttack_EmptyPatterns_ReturnsMinusOne()
        {
            _phase = MakePhase();
            Assert.AreEqual(-1, _phase.PickAttack(new float[0], now: 10f));
        }

        [Test]
        public void PickAttack_NullPatterns_ReturnsMinusOne()
        {
            _phase = ScriptableObject.CreateInstance<BossPhaseDataSO>();
            _phase.AttackPatterns = null;
            Assert.AreEqual(-1, _phase.PickAttack(null, now: 10f));
        }

        // Single attack, still on cooldown: now(10) - lastUsed(5) = 5 < Cooldown(10) => excluded => -1.
        [Test]
        public void PickAttack_AllOnCooldown_ReturnsMinusOne()
        {
            _phase = MakePhase(new AttackEntry { Type = BossAttackType.ScytheSweep, Weight = 1f, Cooldown = 10f });
            Assert.AreEqual(-1, _phase.PickAttack(new[] { 5f }, now: 10f));
        }

        // Single eligible attack: cooldown satisfied, so the only valid index (0) must be returned.
        [Test]
        public void PickAttack_SingleEligible_ReturnsThatIndex()
        {
            _phase = MakePhase(new AttackEntry { Type = BossAttackType.ScytheSweep, Weight = 1f, Cooldown = 1f });
            Assert.AreEqual(0, _phase.PickAttack(new[] { 0f }, now: 100f));
        }

        // Two attacks, only the second is off cooldown -> must pick index 1 every time.
        [Test]
        public void PickAttack_OnlyOneOffCooldown_AlwaysPicksIt()
        {
            _phase = MakePhase(
                new AttackEntry { Type = BossAttackType.ScytheSweep, Weight = 1f, Cooldown = 100f },
                new AttackEntry { Type = BossAttackType.DashSlash,   Weight = 1f, Cooldown = 1f });

            for (int i = 0; i < 50; i++)
                Assert.AreEqual(1, _phase.PickAttack(new[] { 99f, 0f }, now: 100f));
        }

        // Any eligible pick must land on a real, in-range index.
        [Test]
        public void PickAttack_AlwaysReturnsValidIndex()
        {
            _phase = MakePhase(
                new AttackEntry { Type = BossAttackType.ScytheSweep, Weight = 2f, Cooldown = 1f },
                new AttackEntry { Type = BossAttackType.Barrage,     Weight = 1f, Cooldown = 1f });

            for (int i = 0; i < 200; i++)
            {
                int idx = _phase.PickAttack(new[] { 0f, 0f }, now: 100f);
                Assert.GreaterOrEqual(idx, 0);
                Assert.Less(idx, 2);
            }
        }
    }
}
