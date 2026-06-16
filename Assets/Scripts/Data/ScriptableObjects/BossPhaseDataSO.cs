using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace MidnightReturn.Data
{
    public enum BossAttackType
    {
        ScytheSweep,     // Horizontal melee arc — hits wide, moderate damage
        TeleportSlam,    // Vanish + reappear above player → vertical slam
        DashSlash,       // Fast horizontal charge, slashes on contact
        Barrage,         // 5-way spread of slow-moving projectiles
        SummonMinions,   // Spawns 2 skeletal minions from ground
        SpinAttack,      // 360° scythe spin — arena-wide, only Phase 3
    }

    [Serializable]
    public struct AttackEntry
    {
        public BossAttackType Type;
        [Range(0f, 1f)] public float Weight;  // relative probability in this phase
        public float     Cooldown;             // min seconds between this specific attack
        public float     TelegraphDuration;    // wind-up time before hitbox is active
        public int       Damage;
    }

    [CreateAssetMenu(menuName = "MidnightReturn/Boss Phase Data", fileName = "BossPhase")]
    public class BossPhaseDataSO : ScriptableObject
    {
        [Header("Threshold")]
        [Range(0f, 1f)]
        [Tooltip("Phase activates when boss HP drops below this fraction.")]
        public float HPThreshold = 1f;

        [Header("Movement")]
        public float MoveSpeed           = 4f;
        public float AggressionMultiplier= 1f;  // scales attack frequency and speed

        [Header("Attacks")]
        public AttackEntry[] AttackPatterns;

        [Header("Transition FX")]
        public VisualEffectAsset TransitionVFX;
        public Color             TransitionFlashColor = new Color(0.8f, 0f, 0.2f);
        [Tooltip("Broadcast as notification in HUD.")]
        public string            PhaseIntroText = "";

        [Header("Music")]
        public AudioClip PhaseMusicTrack;

        // ── Weighted random attack picker ─────────────────────────────
        // Returns an index into AttackPatterns, or -1 if empty.
        // Excludes any attack whose individual cooldown isn't met (pass
        // per-type last-used times from BossController).
        public int PickAttack(float[] lastUsedTimes, float now)
        {
            if (AttackPatterns == null || AttackPatterns.Length == 0) return -1;

            float total = 0f;
            for (int i = 0; i < AttackPatterns.Length; i++)
            {
                var a = AttackPatterns[i];
                float lastUsed = lastUsedTimes != null && i < lastUsedTimes.Length
                               ? lastUsedTimes[i] : 0f;
                if (now - lastUsed >= a.Cooldown)
                    total += a.Weight;
            }
            if (total <= 0f) return -1;

            float roll = UnityEngine.Random.Range(0f, total);
            float acc  = 0f;
            for (int i = 0; i < AttackPatterns.Length; i++)
            {
                var a = AttackPatterns[i];
                float lastUsed = lastUsedTimes != null && i < lastUsedTimes.Length
                               ? lastUsedTimes[i] : 0f;
                if (now - lastUsed < a.Cooldown) continue;
                acc += a.Weight;
                if (roll < acc) return i;
            }
            return -1;
        }
    }
}
