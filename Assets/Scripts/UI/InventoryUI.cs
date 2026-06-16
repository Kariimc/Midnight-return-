using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using MidnightReturn.Data;
using MidnightReturn.Player;
using MidnightReturn.Systems.Inventory;
using MidnightReturn.Utils;

namespace MidnightReturn.UI
{
    // Full-screen inventory/equipment panel — toggle with Tab (keyboard) or Start (gamepad).
    // Entirely runtime-built via UI Toolkit; no UXML required.
    //
    // Layout: [Equipment Doll] | [Item Grid] | [Stats + Spell Slots]
    [RequireComponent(typeof(UIDocument))]
    public sealed class InventoryUI : MonoBehaviour
    {
        UIDocument    _doc;
        VisualElement _panel;
        VisualElement _itemGrid;
        VisualElement _tooltip;
        Label         _tooltipName, _tooltipDesc;

        readonly Dictionary<string, VisualElement> _equipSlots  = new();
        readonly VisualElement[]                   _spellSlots  = new VisualElement[PlayerSpellSystem.SLOT_COUNT];
        readonly Dictionary<string, Label>         _statLabels  = new();

        bool _open;

        void Awake() => _doc = GetComponent<UIDocument>();

        void OnEnable()
        {
            EventBus.Subscribe<EquipmentChangedEvent>(_ => { if (_open) Refresh(); });
            EventBus.Subscribe<SpellEquippedEvent>(_ => { if (_open) RefreshSpellSlots(); });
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<EquipmentChangedEvent>(_ => { });
            EventBus.Unsubscribe<SpellEquippedEvent>(_ => { });
        }

        void Start()
        {
            _panel = new VisualElement { name = "inventory-panel" };
            _doc.rootVisualElement.Add(_panel);
            BuildUI();
            SetOpen(false);
        }

        void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current?.tabKey.wasPressedThisFrame == true)
                SetOpen(!_open);
        }

        // ── Build ─────────────────────────────────────────────────────────────
        void BuildUI()
        {
            _panel.style.position  = Position.Absolute;
            _panel.style.left      = _panel.style.top =
            _panel.style.right     = _panel.style.bottom = 0;
            _panel.style.flexDirection  = FlexDirection.Row;
            _panel.style.backgroundColor= new Color(0.04f, 0.03f, 0.07f, 0.95f);
            _panel.style.paddingLeft = _panel.style.paddingRight =
            _panel.style.paddingTop  = _panel.style.paddingBottom = 24;

            var left   = MakeColumn(260f);
            var center = MakeColumn(0f, flex: 1f);
            var right  = MakeColumn(200f);
            _panel.Add(left); _panel.Add(center); _panel.Add(right);

            BuildEquipDoll(left);
            BuildItemGrid(center);
            BuildRightPanel(right);
            BuildTooltip();
        }

        void BuildEquipDoll(VisualElement parent)
        {
            parent.Add(SectionLabel("EQUIPMENT"));
            parent.Add(Spacer(8));

            var doll = new VisualElement { style = { flexGrow = 1, position = Position.Relative } };
            parent.Add(doll);

            // (slot-name, label-text, left%, top%)
            (string n, string lbl, float x, float y)[] slots =
            {
                ("Helmet",     "HEAD",   36f,  2f),
                ("Body",       "BODY",   36f, 28f),
                ("RightHand",  "R.HAND",  2f, 28f),
                ("LeftHand",   "L.HAND", 70f, 28f),
                ("Cloak",      "CLOAK",  36f, 55f),
                ("Boots",      "FEET",   36f, 76f),
                ("Accessory1", "ACC 1",   2f, 70f),
                ("Accessory2", "ACC 2",  70f, 70f),
            };

            foreach (var (n, lbl, x, y) in slots)
            {
                var cell = EquipCell(n, lbl, x, y);
                doll.Add(cell);
                _equipSlots[n] = cell;
                string captured = n;
                cell.RegisterCallback<ClickEvent>(_ => { InventorySystem.Instance?.UnequipSlot(captured); Refresh(); });
            }
        }

        void BuildItemGrid(VisualElement parent)
        {
            parent.Add(SectionLabel("INVENTORY"));
            parent.Add(Spacer(8));
            _itemGrid = new VisualElement();
            _itemGrid.style.flexDirection = FlexDirection.Row;
            _itemGrid.style.flexWrap      = Wrap.Wrap;
            _itemGrid.style.flexGrow      = 1;
            parent.Add(_itemGrid);
        }

        void BuildRightPanel(VisualElement parent)
        {
            parent.Add(SectionLabel("STATS"));
            parent.Add(Spacer(4));
            foreach (var key in new[] { "LV","STR","CON","INT","LCK","HP","MP","ATK","DEF","FIRE","ICE","LTN","DARK","HOLY" })
            {
                var row = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, marginBottom = 2 } };
                var k   = new Label(key) { style = { color = new Color(0.68f, 0.62f, 0.46f), fontSize = 11 } };
                var v   = new Label("-")  { style = { color = Color.white, fontSize = 11 } };
                row.Add(k); row.Add(v);
                parent.Add(row);
                _statLabels[key] = v;
            }

            parent.Add(Spacer(16));
            parent.Add(SectionLabel("SPELLS"));
            parent.Add(Spacer(6));
            for (int i = 0; i < PlayerSpellSystem.SLOT_COUNT; i++)
            {
                _spellSlots[i] = SpellSlotCell(i);
                parent.Add(_spellSlots[i]);
                parent.Add(Spacer(4));
            }
        }

        void BuildTooltip()
        {
            _tooltip = new VisualElement();
            _tooltip.style.position         = Position.Absolute;
            _tooltip.style.backgroundColor  = new Color(0.07f, 0.05f, 0.1f, 0.96f);
            _tooltip.style.borderTopWidth   = _tooltip.style.borderRightWidth  =
            _tooltip.style.borderBottomWidth= _tooltip.style.borderLeftWidth   = 1f;
            _tooltip.style.borderTopColor   = _tooltip.style.borderRightColor  =
            _tooltip.style.borderBottomColor= _tooltip.style.borderLeftColor   = new Color(0.68f, 0.55f, 0.18f);
            _tooltip.style.paddingLeft = _tooltip.style.paddingRight =
            _tooltip.style.paddingTop  = _tooltip.style.paddingBottom = 8f;
            _tooltip.style.width        = 180f;
            _tooltip.style.display      = DisplayStyle.None;

            _tooltipName = new Label { style = { color = new Color(1f, 0.9f, 0.3f), fontSize = 12, unityFontStyleAndWeight = FontStyle.Bold } };
            _tooltipDesc = new Label { style = { color = new Color(0.8f, 0.77f, 0.74f), fontSize = 10, whiteSpace = WhiteSpace.Normal } };
            _tooltip.Add(_tooltipName);
            _tooltip.Add(Spacer(4));
            _tooltip.Add(_tooltipDesc);
            _panel.Add(_tooltip);
        }

        // ── Refresh ───────────────────────────────────────────────────────────
        public void Refresh()
        {
            RefreshEquipSlots();
            RefreshItemGrid();
            RefreshStats();
            RefreshSpellSlots();
        }

        void RefreshEquipSlots()
        {
            var gm  = Core.GameManager.Instance;
            var inv = InventorySystem.Instance;
            if (gm == null) return;

            UpdateEquipCell("RightHand",  gm.Save.RightHandId,  id => inv?.GetWeapon(id)?.Icon);
            UpdateEquipCell("LeftHand",   gm.Save.LeftHandId,   id => inv?.GetWeapon(id)?.Icon);
            UpdateEquipCell("Helmet",     gm.Save.HelmetId,     id => inv?.GetArmor(id)?.Icon);
            UpdateEquipCell("Body",       gm.Save.BodyId,       id => inv?.GetArmor(id)?.Icon);
            UpdateEquipCell("Cloak",      gm.Save.CloakId,      id => inv?.GetArmor(id)?.Icon);
            UpdateEquipCell("Boots",      gm.Save.BootsId,      id => inv?.GetArmor(id)?.Icon);
            UpdateEquipCell("Accessory1", gm.Save.Accessory1Id, id => inv?.GetArmor(id)?.Icon);
            UpdateEquipCell("Accessory2", gm.Save.Accessory2Id, id => inv?.GetArmor(id)?.Icon);
        }

        void UpdateEquipCell(string slotName, string itemId, System.Func<string, Sprite> getIcon)
        {
            if (!_equipSlots.TryGetValue(slotName, out var cell)) return;
            var icon = cell.Q<VisualElement>("icon");
            bool equipped = !string.IsNullOrEmpty(itemId);
            cell.style.borderTopColor = cell.style.borderRightColor =
            cell.style.borderBottomColor = cell.style.borderLeftColor =
                equipped ? new Color(0.85f, 0.7f, 0.2f) : new Color(0.4f, 0.34f, 0.18f);
            if (icon == null) return;
            var sprite = equipped ? getIcon(itemId) : null;
            icon.style.backgroundImage = sprite != null
                ? new StyleBackground(sprite)
                : StyleKeyword.None;
        }

        void RefreshItemGrid()
        {
            _itemGrid.Clear();
            var gm = Core.GameManager.Instance;
            if (gm == null) return;
            foreach (var (id, qty) in gm.Save.Inventory)
            {
                if (qty <= 0) continue;
                var so = InventorySystem.Instance?.GetItem(id);
                _itemGrid.Add(ItemCell(id, qty, so?.Icon, so?.DisplayName ?? id, so?.Description ?? ""));
            }
        }

        void RefreshStats()
        {
            var eff = InventorySystem.Instance?.GetEffectiveStats();
            if (eff == null) return;
            void Set(string k, string v) { if (_statLabels.TryGetValue(k, out var l)) l.text = v; }
            Set("LV",   eff.Level.ToString());
            Set("STR",  eff.Str.ToString());
            Set("CON",  eff.Con.ToString());
            Set("INT",  eff.Int.ToString());
            Set("LCK",  eff.Lck.ToString());
            Set("HP",   $"{eff.Hp}/{eff.MaxHp}");
            Set("MP",   $"{eff.Mp}/{eff.MaxMp}");
            Set("ATK",  eff.Atk.ToString());
            Set("DEF",  eff.Def.ToString());
            Set("FIRE", $"{eff.FireRes:+0;-0;0}");
            Set("ICE",  $"{eff.IceRes:+0;-0;0}");
            Set("LTN",  $"{eff.LightningRes:+0;-0;0}");
            Set("DARK", $"{eff.DarkRes:+0;-0;0}");
            Set("HOLY", $"{eff.HolyRes:+0;-0;0}");
        }

        void RefreshSpellSlots()
        {
            var gm  = Core.GameManager.Instance;
            var inv = InventorySystem.Instance;
            if (gm == null) return;
            for (int i = 0; i < PlayerSpellSystem.SLOT_COUNT; i++)
            {
                var so = inv?.GetSpell(gm.Save.SpellSlots[i]);
                var s = _spellSlots[i];
                if (s == null) continue;
                var nameLbl = s.Q<Label>("sname");
                var mpLbl   = s.Q<Label>("smp");
                if (nameLbl) nameLbl.text = so?.DisplayName ?? "—";
                if (mpLbl)   mpLbl.text   = so != null ? $"{so.MpCost} MP" : "";
            }
        }

        // ── Toggle ────────────────────────────────────────────────────────────
        public void SetOpen(bool open)
        {
            _open = open;
            _panel.style.display = open ? DisplayStyle.Flex : DisplayStyle.None;
            if (open) Refresh();
            // Pause game while browsing inventory
            Time.timeScale = open ? 0f : 1f;
            var gm = Core.GameManager.Instance;
            if (gm != null)
                gm.SetPhase(open ? Core.GamePhase.Paused : Core.GamePhase.Playing);
        }

        // ── Cell factories ────────────────────────────────────────────────────
        VisualElement EquipCell(string slotName, string labelText, float xPct, float yPct)
        {
            var cell = new VisualElement { name = $"equip-{slotName}" };
            cell.style.position  = Position.Absolute;
            cell.style.left      = new Length(xPct, LengthUnit.Percent);
            cell.style.top       = new Length(yPct, LengthUnit.Percent);
            cell.style.width = cell.style.height = 54f;
            SetBorder(cell, 1f, new Color(0.4f, 0.34f, 0.18f));
            cell.style.backgroundColor = new Color(0.09f, 0.07f, 0.12f);
            cell.style.alignItems      = Align.Center;
            cell.style.justifyContent  = Justify.Center;

            var icon = new VisualElement { name = "icon" };
            icon.style.width = icon.style.height = 38f;
            cell.Add(icon);

            var lbl = new Label(labelText) { name = "lbl" };
            lbl.style.position      = Position.Absolute;
            lbl.style.bottom        = 2f;
            lbl.style.left = lbl.style.right = 0;
            lbl.style.color         = new Color(0.5f, 0.44f, 0.28f);
            lbl.style.fontSize      = 7f;
            lbl.style.unityTextAlign= TextAnchor.MiddleCenter;
            cell.Add(lbl);
            return cell;
        }

        VisualElement ItemCell(string id, int qty, Sprite icon, string displayName, string desc)
        {
            var cell = new VisualElement();
            cell.style.width = cell.style.height = 50f;
            cell.style.margin = 3f;
            SetBorder(cell, 1f, new Color(0.38f, 0.32f, 0.18f));
            cell.style.backgroundColor = new Color(0.07f, 0.05f, 0.1f);
            cell.style.alignItems      = Align.Center;
            cell.style.justifyContent  = Justify.Center;
            cell.style.position        = Position.Relative;

            if (icon != null)
            {
                var img = new VisualElement { style = { width = 34f, height = 34f } };
                img.style.backgroundImage = new StyleBackground(icon);
                cell.Add(img);
            }

            var q = new Label($"×{qty}");
            q.style.position  = Position.Absolute;
            q.style.bottom    = 2f;
            q.style.right     = 4f;
            q.style.fontSize  = 9f;
            q.style.color     = new Color(0.9f, 0.84f, 0.46f);
            cell.Add(q);

            cell.RegisterCallback<MouseEnterEvent>(_ => ShowTooltip(cell, displayName, desc));
            cell.RegisterCallback<MouseLeaveEvent>(_ => _tooltip.style.display = DisplayStyle.None);

            string captured = id;
            cell.RegisterCallback<ClickEvent>(_ =>
            {
                InventorySystem.Instance?.UseItem(captured);
                RefreshItemGrid();
                RefreshStats();
            });
            return cell;
        }

        VisualElement SpellSlotCell(int idx)
        {
            var cell = new VisualElement();
            cell.style.flexDirection  = FlexDirection.Row;
            cell.style.alignItems     = Align.Center;
            cell.style.backgroundColor= new Color(0.07f, 0.05f, 0.12f);
            SetBorder(cell, 1f, new Color(0.28f, 0.22f, 0.46f));
            cell.style.paddingLeft = cell.style.paddingRight = 6f;
            cell.style.paddingTop  = cell.style.paddingBottom= 5f;

            var num = new Label($"{idx + 1}") { style = { color = new Color(0.48f, 0.42f, 0.68f), fontSize = 10f, width = 14f } };
            var name= new Label("—") { name = "sname", style = { color = Color.white, fontSize = 11f, flexGrow = 1 } };
            var mp  = new Label("")  { name = "smp",   style = { color = new Color(0.28f, 0.48f, 0.9f), fontSize = 10f } };
            cell.Add(num); cell.Add(name); cell.Add(mp);
            return cell;
        }

        void ShowTooltip(VisualElement anchor, string name, string desc)
        {
            _tooltipName.text = name;
            _tooltipDesc.text = desc;
            _tooltip.style.display = DisplayStyle.Flex;
            var r = anchor.layout;
            _tooltip.style.left = r.xMax + 6f;
            _tooltip.style.top  = r.yMin;
        }

        // ── Style helpers ─────────────────────────────────────────────────────
        static VisualElement MakeColumn(float width, float flex = 0f)
        {
            var col = new VisualElement { style = { flexDirection = FlexDirection.Column, paddingLeft = 12f, paddingRight = 12f } };
            if (width > 0) col.style.width   = width;
            if (flex  > 0) col.style.flexGrow = flex;
            return col;
        }

        static Label SectionLabel(string text)
        {
            var l = new Label(text);
            l.style.color         = new Color(0.85f, 0.7f, 0.2f);
            l.style.fontSize      = 13f;
            l.style.unityFontStyleAndWeight = FontStyle.Bold;
            l.style.borderBottomWidth = 1f;
            l.style.borderBottomColor = new Color(0.5f, 0.4f, 0.1f);
            l.style.paddingBottom = 4f;
            l.style.marginBottom  = 4f;
            return l;
        }

        static VisualElement Spacer(float h) => new() { style = { height = h } };

        static void SetBorder(VisualElement ve, float width, Color color)
        {
            ve.style.borderTopWidth = ve.style.borderRightWidth =
            ve.style.borderBottomWidth = ve.style.borderLeftWidth = width;
            ve.style.borderTopColor = ve.style.borderRightColor =
            ve.style.borderBottomColor = ve.style.borderLeftColor = color;
        }
    }
}
