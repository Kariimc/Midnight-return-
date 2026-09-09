using NUnit.Framework;
using UnityEngine;
using MidnightReturn.Data;

namespace MidnightReturn.Tests.EditMode
{
    // EnemyDataSO.GetResistance (Assets/Scripts/Data/ScriptableObjects/EnemyDataSO.cs)
    // drives EnemyBase.TakeDamage's elemental reduction:
    //   dmg = Max(1, Floor(raw * (1 - res/100)))
    // A wrong lookup silently makes enemies immune or hyper-vulnerable, so pin it.
    public class EnemyResistanceTests
    {
        private EnemyDataSO _data;

        [SetUp]
        public void SetUp()
        {
            _data = ScriptableObject.CreateInstance<EnemyDataSO>();
            _data.Resistances.Add(new DamageResistance { Type = DamageType.Fire, Percentage = 50f });
            _data.Resistances.Add(new DamageResistance { Type = DamageType.Ice, Percentage = -100f }); // weakness
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_data);
        }

        [Test]
        public void GetResistance_ReturnsConfiguredPercentage()
        {
            Assert.AreEqual(50f, _data.GetResistance(DamageType.Fire), 1e-5f);
        }

        [Test]
        public void GetResistance_NegativeMeansWeakness()
        {
            Assert.AreEqual(-100f, _data.GetResistance(DamageType.Ice), 1e-5f);
        }

        [Test]
        public void GetResistance_UnlistedType_DefaultsToZero()
        {
            Assert.AreEqual(0f, _data.GetResistance(DamageType.Holy), 1e-5f);
            Assert.AreEqual(0f, _data.GetResistance(DamageType.Physical), 1e-5f);
        }
    }
}
