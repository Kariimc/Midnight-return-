using NUnit.Framework;
using MidnightReturn.Utils;

namespace MidnightReturn.Tests.EditMode
{
    // Deterministic math primitives from Assets/Scripts/Utils/MathUtils.cs.
    // Approach() is the horizontal accel/friction move used every frame in
    // PlayerMovement.TickHorizontal — if it regresses, movement feel breaks
    // silently. ExpThreshold() is the SotN level curve.
    public class MathUtilsTests
    {
        // Approach(current, target, delta):
        //   current < target -> Min(current + delta, target)
        //   else             -> Max(current - delta, target)
        [Test]
        public void Approach_StepsTowardTarget_WithoutOvershoot()
        {
            Assert.AreEqual(3f, MathUtils.Approach(0f, 10f, 3f), 1e-5f);   // partial step up
            Assert.AreEqual(10f, MathUtils.Approach(0f, 10f, 15f), 1e-5f); // clamps at target, no overshoot
            Assert.AreEqual(7f, MathUtils.Approach(10f, 0f, 3f), 1e-5f);   // partial step down
            Assert.AreEqual(0f, MathUtils.Approach(10f, 0f, 15f), 1e-5f);  // clamps at target going down
        }

        [Test]
        public void Approach_AtTarget_HoldsStill()
        {
            Assert.AreEqual(5f, MathUtils.Approach(5f, 5f, 3f), 1e-5f);
        }

        [Test]
        public void Lerp_IsLinearInterpolation()
        {
            Assert.AreEqual(2f, MathUtils.Lerp(2f, 4f, 0f), 1e-5f);
            Assert.AreEqual(4f, MathUtils.Lerp(2f, 4f, 1f), 1e-5f);
            Assert.AreEqual(5f, MathUtils.Lerp(0f, 10f, 0.5f), 1e-5f);
        }

        [Test]
        public void Clamp_BoundsValue()
        {
            Assert.AreEqual(5f, MathUtils.Clamp(5f, 0f, 10f), 1e-5f);
            Assert.AreEqual(0f, MathUtils.Clamp(-3f, 0f, 10f), 1e-5f);
            Assert.AreEqual(10f, MathUtils.Clamp(15f, 0f, 10f), 1e-5f);
        }

        [Test]
        public void Sign_ReturnsMinusOneZeroOne()
        {
            Assert.AreEqual(-1, MathUtils.Sign(-2f));
            Assert.AreEqual(0, MathUtils.Sign(0f));
            Assert.AreEqual(1, MathUtils.Sign(3f));
        }

        // ExpThreshold(level) = FloorToInt(100 * 1.45^(level-1))
        // Verified numerically: 100, 145, 210, 304, 442 for levels 1..5.
        [Test]
        public void ExpThreshold_FollowsGrowthCurve()
        {
            Assert.AreEqual(100, MathUtils.ExpThreshold(1));
            Assert.AreEqual(145, MathUtils.ExpThreshold(2));
            Assert.AreEqual(210, MathUtils.ExpThreshold(3));
            Assert.AreEqual(304, MathUtils.ExpThreshold(4));
            Assert.AreEqual(442, MathUtils.ExpThreshold(5));
        }

        [Test]
        public void ExpThreshold_IsStrictlyIncreasing()
        {
            for (int level = 1; level < 20; level++)
                Assert.Less(MathUtils.ExpThreshold(level), MathUtils.ExpThreshold(level + 1));
        }
    }
}
