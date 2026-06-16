using System;
using System.Collections.Generic;
using UnityEngine;

namespace MidnightReturn.Data
{
    public enum ArmorSlot { Helmet, Body, Cloak, Boots, Accessory }

    [CreateAssetMenu(fileName = "Armor_", menuName = "MidnightReturn/Armor Data")]
    public class ArmorDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string ArmorId;
        public string DisplayName;
        public ArmorSlot Slot;
        [TextArea(1,3)] public string Description;

        [Header("Stat Modifiers")]
        public int Defense;
        public int StrMod;
        public int ConMod;
        public int IntMod;
        public int LckMod;
        public int HpMod;
        public int MpMod;

        [Header("Resistances")]
        public List<DamageResistance> Resistances = new();

        [Header("Passive Ability")]
        public string PassiveAbilityId;

        [Header("Visual")]
        public Sprite Icon;
        public Material OverrideMaterial; // Applied to character mesh when equipped
    }
}
