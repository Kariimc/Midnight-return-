using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace MidnightReturn.Data
{
    public enum AIBehavior { Patrol, Chase, PatrolJump, Ranged, Flying, Boss }
    public enum DamageType { Physical, Fire, Ice, Lightning, Dark, Holy, Poison }
    public enum EnemyZone  { EntranceHall, Catacombs, CursedLibrary, Clocktower, ThroneRoom }

    [Serializable]
    public struct DropEntry
    {
        public string ItemId;
        [Range(0f, 100f)] public float Chance;
    }

    [Serializable]
    public struct DamageResistance
    {
        public DamageType Type;
        [Range(-200f, 100f)] public float Percentage; // negative = weakness
    }

    [CreateAssetMenu(fileName = "Enemy_", menuName = "MidnightReturn/Enemy Data")]
    public class EnemyDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string EnemyId;
        public string DisplayName;
        public EnemyZone Zone;
        [TextArea(2,4)] public string Description;

        [Header("Stats")]
        public int MaxHp;
        public int Attack;
        public int Defense;
        public int ExpReward;

        [Header("Movement")]
        public AIBehavior Behavior;
        public float MoveSpeed       = 80f;
        public float DetectionRange  = 320f;
        public float AttackRange     = 70f;
        public float AggroRange      = 300f;

        [Header("Hitbox")]
        public Vector2 HitboxSize    = new(28f, 52f);
        public Vector2 HitboxOffset  = Vector2.zero;

        [Header("Combat")]
        public float AttackCooldown  = 1.5f;
        public float AttackWindup    = 0.3f;
        public List<DamageResistance> Resistances = new();

        [Header("Drops")]
        public List<DropEntry> Drops = new();

        [Header("VFX — HDRP")]
        public VisualEffectAsset DeathVFX;
        public VisualEffectAsset HitVFX;
        public Color DeathColor = Color.red;
        public int DeathParticleCount = 60;
        public GameObject DissolveShaderPrefab; // Material with HDRP dissolve shader

        [Header("Audio")]
        public AudioClip DeathSound;
        public AudioClip HitSound;
        public AudioClip AttackSound;

        [Header("Prefab")]
        public GameObject Prefab;

        public float GetResistance(DamageType type)
        {
            foreach (var r in Resistances)
                if (r.Type == type) return r.Percentage;
            return 0f;
        }
    }
}
