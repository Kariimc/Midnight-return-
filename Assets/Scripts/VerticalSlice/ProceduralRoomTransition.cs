using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MidnightReturn.VerticalSlice
{
    // ══════════════════════════════════════════════════════════════════════════
    //  ProceduralRoomTransition — fade-to-black room-swap trigger.
    //
    //  Place on a BoxCollider (isTrigger=true) at a room exit. When the Player
    //  walks in while not locked, runs:
    //    1. Fade to black (FadeDuration)
    //    2. Invoke OnMidpoint  ← VerticalSliceBootstrap swaps room content here
    //    3. Yield one frame    ← lets Destroy/Instantiate settle
    //    4. Fade back in (FadeDuration)
    //
    //  The UGUI fade overlay is created once and kept alive via DontDestroyOnLoad
    //  so it survives the room swap coroutine without losing its reference.
    // ══════════════════════════════════════════════════════════════════════════
    [RequireComponent(typeof(Collider))]
    public sealed class ProceduralRoomTransition : MonoBehaviour
    {
        [SerializeField] private bool  _locked       = true;
        [SerializeField] private float _fadeDuration = 0.3f;

        // Wired by VerticalSliceBootstrap — called at the black frame midpoint.
        public Action OnMidpoint;

        public bool IsLocked => _locked;

        private bool       _playerInside;
        private bool       _running;
        private CanvasGroup _overlay;
        private MeshRenderer _doorRenderer;

        private void Start()
        {
            // Optional: create a thin flat door mesh as a visual indicator
            var vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vis.name = "DoorVisual";
            vis.transform.SetParent(transform, false);
            vis.transform.localScale = new Vector3(0.15f, 3f, 0.5f);

            // Remove the primitive's own collider — the trigger BoxCollider above handles it.
            var c = vis.GetComponent<Collider>();
            if (c) Destroy(c);

            _doorRenderer = vis.GetComponent<MeshRenderer>();
            RefreshDoorColor();
        }

        public void Unlock()
        {
            _locked = false;
            RefreshDoorColor();
        }

        private void RefreshDoorColor()
        {
            if (_doorRenderer == null) return;
            // Locked → dark red grate / Unlocked → gold archway
            var sh = Shader.Find("HDRP/Lit") ?? Shader.Find("Standard");
            var mat = new Material(sh);
            var c = _locked ? new Color(0.35f, 0.04f, 0.04f) : new Color(0.7f, 0.55f, 0.1f);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
            if (mat.HasProperty("_Color"))     mat.SetColor("_Color", c);
            if (!_locked && mat.HasProperty("_EmissiveColor"))
                mat.SetColor("_EmissiveColor", new Color(0.6f, 0.45f, 0f));
            _doorRenderer.sharedMaterial = mat;
        }

        private void OnTriggerEnter(Collider other)
        { if (other.CompareTag("Player")) _playerInside = true; }

        private void OnTriggerExit(Collider other)
        { if (other.CompareTag("Player")) _playerInside = false; }

        private void Update()
        {
            if (!_playerInside || _locked || _running) return;
            _running = true;
            StartCoroutine(RunTransition());
        }

        private IEnumerator RunTransition()
        {
            _overlay = GetOrCreateOverlay();

            yield return StartCoroutine(FadeTo(1f));

            OnMidpoint?.Invoke();
            yield return null; // one frame for Destroy/Instantiate to settle

            yield return StartCoroutine(FadeTo(0f));

            _running = false;
        }

        private IEnumerator FadeTo(float target)
        {
            float start = _overlay.alpha;
            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _overlay.alpha = Mathf.Lerp(start, target, elapsed / _fadeDuration);
                yield return null;
            }
            _overlay.alpha = target;
        }

        private CanvasGroup GetOrCreateOverlay()
        {
            // Reuse the overlay if a prior transition already created it.
            var existing = FindFirstObjectByType<RoomFadeOverlay>();
            if (existing != null) return existing.GetComponent<CanvasGroup>();

            var root = new GameObject("RoomFadeOverlay");
            DontDestroyOnLoad(root);
            root.AddComponent<RoomFadeOverlay>();

            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            root.AddComponent<GraphicRaycaster>();

            // Full-screen black image
            var imgGO = new GameObject("BG");
            imgGO.transform.SetParent(root.transform, false);
            var img = imgGO.AddComponent<Image>();
            img.color = Color.black;
            var rt = imgGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            var cg = root.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            return cg;
        }
    }

    // Marker so FindFirstObjectByType can locate the overlay without a static ref.
    internal sealed class RoomFadeOverlay : MonoBehaviour { }
}
