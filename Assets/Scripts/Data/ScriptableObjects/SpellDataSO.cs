using UnityEngine;
using UnityEngine.VFX;

namespace MidnightReturn.Data
{
    public enum SpellType    { Projectile, AreaBurst, Buff, Drain, Summon }
    public enum SpellElement { Neutral, Fire, Ice, Lightning, Dark, Holy, Poison }

    [CreateAssetMenu(fileName = "Spell_", menuName = "MidnightReturn/Spell Data")]
    public class SpellDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string SpellId;
        public string DisplayName;
        [TextArea(1, 3)] public string Description;

        [Header("Properties")]
        public SpellType    Type;
        public SpellElement Element;
        public int   MpCost          = 10;
        public float Cooldown        = 1f;
        public int   BaseDamage;
        public float ProjectileSpeed = 12f;
        public float Range           = 16f;
        public float AoeRadius       = 2f;

        [Header("Buff")]
        public int   StatBonus;
        public float BuffDuration;

        [Header("Art / VFX")]
        public Sprite Icon;
        public VisualEffectAsset CastVFX;
        public VisualEffectAsset ImpactVFX;

        [Header("Audio")]
        public AudioClip CastSound;

        [Header("Prefab")]
        public GameObject ProjectilePrefab;
    }
}
