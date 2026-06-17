using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MidnightReturn.VerticalSlice
{
    // ══════════════════════════════════════════════════════════════════════════
    //  AbilityAcquisitionPresenter — two-phase dramatic ability pickup screen.
    //
    //  Phase 1 (0.6s realtime): Time freezes. Full-screen purple flash pulses.
    //    Large "POWER ACQUIRED / [NAME]" text fades in.
    //
    //  Phase 2: Black cinematic bars slide in from top and bottom (0.35s).
    //    Description text and "[Press any button]" prompt appear.
    //    Waits for ANY input (using unscaled time so timeScale=0 is fine).
    //
    //  Phase 3 (0.25s): Bars slide out. Time unpauses. onComplete fires.
    //    Presenter self-destructs.
    //
    //  Usage: AbilityAcquisitionPresenter.Show("Air Dash", "Dash mid-air.", cb);
    // ══════════════════════════════════════════════════════════════════════════
    public sealed class AbilityAcquisitionPresenter : MonoBehaviour
    {
        public static void Show(string abilityName, string description, Action onComplete = null)
        {
            var go = new GameObject("AbilityAcquisitionPresenter");
            DontDestroyOnLoad(go);
            var p = go.AddComponent<AbilityAcquisitionPresenter>();
            p._abilityName = abilityName;
            p._description = description;
            p._onComplete  = onComplete;
        }

        private string _abilityName;
        private string _description;
        private Action _onComplete;

        private void Start() => StartCoroutine(RunPresentation());

        private IEnumerator RunPresentation()
        {
            Time.timeScale = 0f;

            var (canvas, flash, barTop, barBot, header, desc, prompt) = BuildUI();

            // ── Phase 1: freeze flash (0.6s) ───────────────────────────────────
            float t = 0f;
            while (t < 0.6f)
            {
                t += Time.unscaledDeltaTime;
                float norm = t / 0.6f;
                // pulse: in 0→0.3s, out 0.3→0.6s
                float a = norm < 0.5f ? norm * 2f : (1f - norm) * 2f;
                flash.color = new Color(0.45f, 0.2f, 0.85f, a * 0.75f);
                // header fades in over whole phase
                var hc = header.color;
                hc.a = Mathf.SmoothStep(0f, 1f, norm);
                header.color = hc;
                yield return null;
            }
            flash.color = Color.clear;

            // ── Phase 2a: bars slide in (0.35s) ────────────────────────────────
            var topRT = barTop.GetComponent<RectTransform>();
            var botRT = barBot.GetComponent<RectTransform>();
            t = 0f;
            while (t < 0.35f)
            {
                t += Time.unscaledDeltaTime;
                float pct = Mathf.SmoothStep(0f, 1f, t / 0.35f);
                SetBarSize(topRT, Mathf.Lerp(0f, BarHeight, pct));
                SetBarSize(botRT, Mathf.Lerp(0f, BarHeight, pct));
                yield return null;
            }
            SetBarSize(topRT, BarHeight);
            SetBarSize(botRT, BarHeight);

            // ── Phase 2b: show text, wait for input ────────────────────────────
            desc.gameObject.SetActive(true);
            prompt.gameObject.SetActive(true);

            yield return new WaitForSecondsRealtime(0.5f); // min display time
            // Wait for any button
            while (!Input.anyKeyDown) yield return null;

            // ── Phase 3: bars slide out (0.25s) ────────────────────────────────
            desc.gameObject.SetActive(false);
            prompt.gameObject.SetActive(false);
            t = 0f;
            while (t < 0.25f)
            {
                t += Time.unscaledDeltaTime;
                float pct = Mathf.SmoothStep(0f, 1f, t / 0.25f);
                SetBarSize(topRT, Mathf.Lerp(BarHeight, 0f, pct));
                SetBarSize(botRT, Mathf.Lerp(BarHeight, 0f, pct));
                // fade header out
                var hc = header.color;
                hc.a = 1f - pct;
                header.color = hc;
                yield return null;
            }

            Time.timeScale = 1f;
            _onComplete?.Invoke();
            Destroy(gameObject);
        }

        // ── UI construction ───────────────────────────────────────────────────
        private const float BarHeight = 0.18f; // fraction of screen

        private static void SetBarSize(RectTransform rt, float frac)
        {
            // Top bar grows DOWN from top edge, bottom bar grows UP from bottom edge
            // We control via offsetMin/Max keeping anchor pinned to each edge.
            var ap = rt.anchoredPosition;
            ap.y = 0f;
            rt.anchoredPosition = ap;
            rt.sizeDelta = new Vector2(0f, frac * Screen.height);
        }

        private (GameObject canvas, Image flash, GameObject barTop, GameObject barBot,
                 Text header, Text desc, Text prompt)
            BuildUI()
        {
            // Root canvas
            var root = new GameObject("Canvas");
            root.transform.SetParent(transform, false);
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            root.AddComponent<GraphicRaycaster>();

            // Flash overlay
            var flashGO  = MakeGO("Flash", root.transform);
            var flashRT  = flashGO.GetComponent<RectTransform>();
            Stretch(flashRT);
            var flashImg = flashGO.AddComponent<Image>();
            flashImg.color = Color.clear;

            // Black bars (anchored to screen edges, grow inward)
            var topGO = MakeGO("BarTop", root.transform);
            var topRT = topGO.GetComponent<RectTransform>();
            topRT.anchorMin = new Vector2(0f, 1f);
            topRT.anchorMax = new Vector2(1f, 1f);
            topRT.pivot     = new Vector2(0.5f, 1f);
            topRT.sizeDelta = Vector2.zero;
            topGO.AddComponent<Image>().color = new Color(0.02f, 0.01f, 0.04f);

            var botGO = MakeGO("BarBot", root.transform);
            var botRT = botGO.GetComponent<RectTransform>();
            botRT.anchorMin = new Vector2(0f, 0f);
            botRT.anchorMax = new Vector2(1f, 0f);
            botRT.pivot     = new Vector2(0.5f, 0f);
            botRT.sizeDelta = Vector2.zero;
            botGO.AddComponent<Image>().color = new Color(0.02f, 0.01f, 0.04f);

            // Header text: "POWER ACQUIRED / [Name]"
            // Sits in the vertical center of the screen (not inside bars)
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var headerGO = MakeGO("Header", root.transform);
            var headerRT = headerGO.GetComponent<RectTransform>();
            Stretch(headerRT);
            var headerText = headerGO.AddComponent<Text>();
            headerText.font      = font;
            headerText.fontSize  = 28;
            headerText.alignment = TextAnchor.MiddleCenter;
            headerText.color     = new Color(0.9f, 0.8f, 1f, 0f);
            headerText.text      = $"POWER ACQUIRED\n{_abilityName.ToUpper()}";

            // Description text — inside bottom bar area
            var descGO = MakeGO("Desc", botGO.transform);
            descGO.SetActive(false);
            var descRT = descGO.GetComponent<RectTransform>();
            Stretch(descRT);
            var descText = descGO.AddComponent<Text>();
            descText.font      = font;
            descText.fontSize  = 15;
            descText.alignment = TextAnchor.MiddleCenter;
            descText.color     = new Color(0.75f, 0.7f, 0.85f);
            descText.text      = _description;

            // Prompt — below description
            var promptGO = MakeGO("Prompt", botGO.transform);
            promptGO.SetActive(false);
            var promptRT = promptGO.GetComponent<RectTransform>();
            promptRT.anchorMin = new Vector2(0f, 0f);
            promptRT.anchorMax = new Vector2(1f, 0.25f);
            promptRT.sizeDelta = Vector2.zero;
            var promptText = promptGO.AddComponent<Text>();
            promptText.font      = font;
            promptText.fontSize  = 11;
            promptText.alignment = TextAnchor.MiddleCenter;
            promptText.color     = new Color(0.45f, 0.4f, 0.55f);
            promptText.text      = "[ Press any button ]";

            return (root, flashImg, topGO, botGO, headerText, descText, promptText);
        }

        private static GameObject MakeGO(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
        }
    }
}
