using UnityEngine;

namespace MidnightReturn.Data
{
    public enum ItemCategory { Consumable, KeyItem, Material, Relic }

    [CreateAssetMenu(fileName = "Item_", menuName = "MidnightReturn/Item Data")]
    public class ItemDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string ItemId;
        public string DisplayName;
        [TextArea(1, 3)] public string Description;
        public ItemCategory Category;
        public Sprite Icon;
        public int MaxStack = 9;

        [Header("Consumable Effects")]
        public int   HpRestore;
        public int   MpRestore;
        public int   StrMod, ConMod, IntMod, LckMod;  // permanent stat boost on use
        public float EffectDuration;                   // > 0 = temporary buff
    }
}
