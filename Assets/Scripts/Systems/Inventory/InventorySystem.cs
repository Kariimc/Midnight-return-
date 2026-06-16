using System.Collections.Generic;
using UnityEngine;
using MidnightReturn.Data;
using MidnightReturn.Player;
using MidnightReturn.Utils;

namespace MidnightReturn.Systems.Inventory
{
    // Singleton catalog: holds all item/weapon/armor/spell SOs, drives equip + use logic.
    public sealed class InventorySystem : MonoBehaviour
    {
        public static InventorySystem Instance { get; private set; }

        [Header("Catalogs — assign all SOs in Inspector")]
        [SerializeField] WeaponDataSO[] _weapons;
        [SerializeField] ArmorDataSO[]  _armors;
        [SerializeField] SpellDataSO[]  _spells;
        [SerializeField] ItemDataSO[]   _items;

        readonly Dictionary<string, WeaponDataSO> _weaponMap = new();
        readonly Dictionary<string, ArmorDataSO>  _armorMap  = new();
        readonly Dictionary<string, SpellDataSO>  _spellMap  = new();
        readonly Dictionary<string, ItemDataSO>   _itemMap   = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            foreach (var w in _weapons) if (w) _weaponMap[w.WeaponId] = w;
            foreach (var a in _armors)  if (a) _armorMap[a.ArmorId]   = a;
            foreach (var s in _spells)  if (s) _spellMap[s.SpellId]   = s;
            foreach (var i in _items)   if (i) _itemMap[i.ItemId]     = i;
        }

        // ── Lookups ──────────────────────────────────────────────────────────
        public WeaponDataSO GetWeapon(string id) => string.IsNullOrEmpty(id) || !_weaponMap.TryGetValue(id, out var v) ? null : v;
        public ArmorDataSO  GetArmor (string id) => string.IsNullOrEmpty(id) || !_armorMap .TryGetValue(id, out var a) ? null : a;
        public SpellDataSO  GetSpell (string id) => string.IsNullOrEmpty(id) || !_spellMap .TryGetValue(id, out var s) ? null : s;
        public ItemDataSO   GetItem  (string id) => string.IsNullOrEmpty(id) || !_itemMap  .TryGetValue(id, out var i) ? null : i;

        // ── Weapons ───────────────────────────────────────────────────────────
        public void EquipWeapon(string id, bool rightHand = true)
        {
            var gm = Core.GameManager.Instance;
            if (gm == null) return;
            if (rightHand) gm.Save.RightHandId = id;
            else           gm.Save.LeftHandId  = id;

            if (rightHand)
            {
                var so     = GetWeapon(id);
                var combat = FindFirstObjectByType<PlayerCombat>();
                if (combat != null && so != null) combat.EquipWeapon(so);
            }
            EventBus.Emit(new EquipmentChangedEvent { SlotName = rightHand ? "RightHand" : "LeftHand", ItemId = id });
            RecalcStats();
        }

        // ── Armor ─────────────────────────────────────────────────────────────
        public void EquipArmor(string id)
        {
            var gm = Core.GameManager.Instance;
            if (gm == null) return;
            var so = GetArmor(id);
            if (so == null) return;

            switch (so.Slot)
            {
                case ArmorSlot.Helmet:    gm.Save.HelmetId    = id; break;
                case ArmorSlot.Body:      gm.Save.BodyId      = id; break;
                case ArmorSlot.Cloak:     gm.Save.CloakId     = id; break;
                case ArmorSlot.Boots:     gm.Save.BootsId     = id; break;
                case ArmorSlot.Accessory:
                    if (string.IsNullOrEmpty(gm.Save.Accessory1Id)) gm.Save.Accessory1Id = id;
                    else                                              gm.Save.Accessory2Id = id;
                    break;
            }
            EventBus.Emit(new EquipmentChangedEvent { SlotName = so.Slot.ToString(), ItemId = id });
            RecalcStats();
        }

        public void UnequipSlot(string slotName)
        {
            var gm = Core.GameManager.Instance;
            if (gm == null) return;
            switch (slotName)
            {
                case "RightHand":  gm.Save.RightHandId  = ""; break;
                case "LeftHand":   gm.Save.LeftHandId   = ""; break;
                case "Helmet":     gm.Save.HelmetId     = ""; break;
                case "Body":       gm.Save.BodyId       = ""; break;
                case "Cloak":      gm.Save.CloakId      = ""; break;
                case "Boots":      gm.Save.BootsId      = ""; break;
                case "Accessory1": gm.Save.Accessory1Id = ""; break;
                case "Accessory2": gm.Save.Accessory2Id = ""; break;
            }
            EventBus.Emit(new EquipmentChangedEvent { SlotName = slotName, ItemId = "" });
            RecalcStats();
        }

        // ── Spells ────────────────────────────────────────────────────────────
        public void EquipSpell(string id, int slot)
        {
            var gm = Core.GameManager.Instance;
            if (gm == null || (uint)slot >= PlayerSpellSystem.SLOT_COUNT) return;
            gm.Save.SpellSlots[slot] = id;
            EventBus.Emit(new SpellEquippedEvent { SpellId = id, Slot = slot });
        }

        // ── Consumables ───────────────────────────────────────────────────────
        public bool UseItem(string id)
        {
            var gm = Core.GameManager.Instance;
            if (gm == null) return false;
            if (!gm.Save.Inventory.TryGetValue(id, out int qty) || qty <= 0) return false;
            var so = GetItem(id);
            if (so == null) return false;

            var player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                if (so.HpRestore > 0) player.Heal(so.HpRestore);
                if (so.MpRestore > 0) player.RestoreMp(so.MpRestore);
            }

            int remaining = qty - 1;
            if (remaining <= 0) gm.Save.Inventory.Remove(id);
            else                gm.Save.Inventory[id] = remaining;

            EventBus.Emit(new ItemUsedEvent { ItemId = id, Remaining = remaining });
            return true;
        }

        // ── Stat computation ─────────────────────────────────────────────────
        // Returns a copy of base stats with all equipped armor/weapon bonuses folded in.
        public StatBlock GetEffectiveStats()
        {
            var gm = Core.GameManager.Instance;
            if (gm == null) return new StatBlock();
            var s = gm.Save.Stats;

            var eff = new StatBlock
            {
                Level = s.Level, Exp = s.Exp, ExpToNext = s.ExpToNext,
                Str = s.Str, Con = s.Con, Int = s.Int, Lck = s.Lck,
                Hp = s.Hp, MaxHp = s.MaxHp, Mp = s.Mp, MaxMp = s.MaxMp,
                Atk = s.Atk, Def = s.Def,
                FireRes      = s.FireRes,      IceRes       = s.IceRes,
                LightningRes = s.LightningRes, DarkRes      = s.DarkRes,
                HolyRes      = s.HolyRes,      PoisonRes    = s.PoisonRes,
            };

            ApplyArmor(eff, gm.Save.HelmetId);
            ApplyArmor(eff, gm.Save.BodyId);
            ApplyArmor(eff, gm.Save.CloakId);
            ApplyArmor(eff, gm.Save.BootsId);
            ApplyArmor(eff, gm.Save.Accessory1Id);
            ApplyArmor(eff, gm.Save.Accessory2Id);

            var rh = GetWeapon(gm.Save.RightHandId);
            if (rh != null) eff.Atk += rh.Attack;

            return eff;
        }

        void ApplyArmor(StatBlock eff, string id)
        {
            var a = GetArmor(id);
            if (a == null) return;
            eff.Def   += a.Defense;
            eff.Str   += a.StrMod;
            eff.Con   += a.ConMod;
            eff.Int   += a.IntMod;
            eff.Lck   += a.LckMod;
            eff.MaxHp += a.HpMod;
            eff.MaxMp += a.MpMod;
            foreach (var r in a.Resistances)
            {
                switch (r.Type)
                {
                    case DamageType.Fire:      eff.FireRes      += r.Percentage; break;
                    case DamageType.Ice:       eff.IceRes       += r.Percentage; break;
                    case DamageType.Lightning: eff.LightningRes += r.Percentage; break;
                    case DamageType.Dark:      eff.DarkRes      += r.Percentage; break;
                    case DamageType.Holy:      eff.HolyRes      += r.Percentage; break;
                    case DamageType.Poison:    eff.PoisonRes    += r.Percentage; break;
                }
            }
        }

        void RecalcStats()
        {
            var gm = Core.GameManager.Instance;
            if (gm == null) return;
            var eff = GetEffectiveStats();
            gm.Save.Stats.Atk   = eff.Atk;
            gm.Save.Stats.Def   = eff.Def;
            gm.Save.Stats.MaxHp = eff.MaxHp;
            gm.Save.Stats.MaxMp = eff.MaxMp;
        }
    }
}
