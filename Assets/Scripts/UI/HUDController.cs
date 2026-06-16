using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using MidnightReturn.Utils;
using MidnightReturn.Core;

namespace MidnightReturn.UI
{
    // Uses Unity UI Toolkit for zero-draw-call, scalable HUD
    [RequireComponent(typeof(UIDocument))]
    public class HUDController : MonoBehaviour
    {
        private UIDocument    _doc;
        private VisualElement _root;

        // HP bar
        private VisualElement _hpFill;
        private Label         _hpLabel;

        // MP bar
        private VisualElement _mpFill;
        private Label         _mpLabel;

        // EXP bar
        private VisualElement _expFill;
        private Label         _lvlLabel;

        // Boss bar
        private VisualElement _bossBar;
        private VisualElement _bossFill;
        private Label         _bossNameLabel;

        // Status effects
        private VisualElement _statusContainer;

        // Notifications
        private VisualElement _notifContainer;

        private Player.PlayerController _player;

        private void Awake()
        {
            _doc  = GetComponent<UIDocument>();
            _root = _doc.rootVisualElement;
            QueryElements();
        }

        private void QueryElements()
        {
            _hpFill      = _root.Q<VisualElement>("hp-fill");
            _hpLabel     = _root.Q<Label>("hp-label");
            _mpFill      = _root.Q<VisualElement>("mp-fill");
            _mpLabel     = _root.Q<Label>("mp-label");
            _expFill     = _root.Q<VisualElement>("exp-fill");
            _lvlLabel    = _root.Q<Label>("lvl-label");
            _bossBar     = _root.Q<VisualElement>("boss-bar");
            _bossFill    = _root.Q<VisualElement>("boss-fill");
            _bossNameLabel  = _root.Q<Label>("boss-name");
            _statusContainer = _root.Q<VisualElement>("status-effects");
            _notifContainer  = _root.Q<VisualElement>("notifications");

            // Hide boss bar initially
            _bossBar?.AddToClassList("hidden");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            EventBus.Subscribe<PlayerLeveledUpEvent>(OnLevelUp);
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Subscribe<BossStartedEvent>(OnBossStart);
            EventBus.Subscribe<BossDefeatedEvent>(OnBossEnd);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            EventBus.Unsubscribe<PlayerLeveledUpEvent>(OnLevelUp);
            EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Unsubscribe<BossStartedEvent>(OnBossStart);
            EventBus.Unsubscribe<BossDefeatedEvent>(OnBossEnd);
        }

        private void Start()
        {
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null) _player = playerGO.GetComponent<Player.PlayerController>();
            RefreshBars();
        }

        private void Update()
        {
            // MP regenerates passively — refresh each frame
            RefreshMpBar();
        }

        private void RefreshBars()
        {
            if (_player == null) return;
            RefreshHpBar();
            RefreshMpBar();
            RefreshExpBar();
        }

        private void RefreshHpBar()
        {
            if (_player == null || _hpFill == null) return;
            var s = _player.Stats;
            float ratio = s.MaxHp > 0 ? (float)s.Hp / s.MaxHp : 0f;
            _hpFill.style.width = Length.Percent(ratio * 100f);
            _hpLabel?.SetText($"{s.Hp} / {s.MaxHp}");

            // Danger color ramp
            string color = ratio < 0.25f ? "#ff2222" : ratio < 0.5f ? "#ff8800" : "#cc2222";
            _hpFill.style.backgroundColor = new StyleColor(FromHex(color));
        }

        private void RefreshMpBar()
        {
            if (_player == null || _mpFill == null) return;
            var s = _player.Stats;
            float ratio = s.MaxMp > 0 ? (float)s.Mp / s.MaxMp : 0f;
            _mpFill.style.width = Length.Percent(ratio * 100f);
            _mpLabel?.SetText($"{s.Mp} / {s.MaxMp}");
        }

        private void RefreshExpBar()
        {
            if (_player == null || _expFill == null) return;
            var s = _player.Stats;
            float ratio = s.ExpToNext > 0 ? (float)s.Exp / s.ExpToNext : 0f;
            _expFill.style.width = Length.Percent(ratio * 100f);
            _lvlLabel?.SetText($"LVL {s.Level}");
        }

        // ── Event handlers ────────────────────────────────────────────────────
        private void OnPlayerDamaged(PlayerDamagedEvent e)
        {
            RefreshHpBar();
            StartCoroutine(ShakeBar(_hpFill));
        }

        private void OnLevelUp(PlayerLeveledUpEvent e)
        {
            RefreshBars();
            SpawnNotification($"✦ LEVEL UP!  LVL {e.Level}  ✦", "#ffdd00");
        }

        private void OnEnemyDied(EnemyDiedEvent e)
        {
            RefreshExpBar();
        }

        private void OnBossStart(BossStartedEvent e)
        {
            _bossBar?.RemoveFromClassList("hidden");
            _bossNameLabel?.SetText($"☠ {e.BossId.ToUpper()} ☠");
            SpawnNotification("⚠ BOSS ENCOUNTERED ⚠", "#ff4400");
        }

        private void OnBossEnd(BossDefeatedEvent e)
        {
            StartCoroutine(HideBossBar());
            SpawnNotification("✦ BOSS DEFEATED ✦", "#ffdd00");
        }

        private IEnumerator HideBossBar()
        {
            yield return new WaitForSeconds(2f);
            _bossBar?.AddToClassList("hidden");
        }

        private void SpawnNotification(string text, string hexColor)
        {
            if (_notifContainer == null) return;
            var lbl = new Label(text);
            lbl.style.color    = new StyleColor(FromHex(hexColor));
            lbl.AddToClassList("notification");
            _notifContainer.Add(lbl);
            StartCoroutine(FadeNotification(lbl, 2.5f));
        }

        private IEnumerator FadeNotification(Label lbl, float duration)
        {
            float t = 0f;
            yield return new WaitForSeconds(duration * 0.6f);
            while (t < duration * 0.4f)
            {
                t   += Time.deltaTime;
                lbl.style.opacity = 1f - t / (duration * 0.4f);
                yield return null;
            }
            _notifContainer?.Remove(lbl);
        }

        private IEnumerator ShakeBar(VisualElement el)
        {
            if (el == null) yield break;
            for (int i = 0; i < 4; i++)
            {
                el.style.translate = new Translate(4, 0, 0);
                yield return new WaitForSeconds(0.04f);
                el.style.translate = new Translate(-4, 0, 0);
                yield return new WaitForSeconds(0.04f);
            }
            el.style.translate = new Translate(0, 0, 0);
        }

        private static Color FromHex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out var c)) return c;
            return Color.white;
        }
    }
}
