using System;
using UnityEngine;
using UnityEngine.VFX;

namespace MidnightReturn.Data
{
    public enum WeaponType { Sword, Whip, Axe, Spear, Scythe, Fist, Wand }

    [CreateAssetMenu(fileName = "Weapon_", menuName = "MidnightReturn/Weapon Data")]
    public class WeaponDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string WeaponId;
        public string DisplayName;
        public WeaponType Type;
        [TextArea(1,3)] public string Description;

        [Header("Stats")]
        public int Attack;
        public float Range           = 55f;
        public Vector2 HitboxSize    = new(55f, 24f);
        public DamageType DamageType = DamageType.Physical;
        [Range(0f, 50f)] public float CritBonus;

        [Header("Combo")]
        public int ComboHits         = 2;
        public float SwingDuration   = 0.25f; // seconds per swing
        public float ComboWindow     = 0.4f;  // seconds to chain next hit

        [Header("Special Move")]
        public string SpecialMoveId;
        public int SpecialMpCost     = 20;

        [Header("VFX — HDRP")]
        public VisualEffectAsset SwingVFX;
        public VisualEffectAsset HitVFX;
        public VisualEffectAsset SpecialVFX;
        // Trail renderer settings for weapon swing
        public Color TrailColor      = Color.white;
        public float TrailWidth      = 0.06f;
        public float TrailTime       = 0.12f;
        public Material TrailMaterial;

        [Header("Audio")]
        public AudioClip SwingSound;
        public AudioClip HitSound;
        public AudioClip SpecialSound;

        [Header("Art")]
        public Sprite Icon;
        public GameObject WorldPrefab; // 3D model for equipped display
    }
}
