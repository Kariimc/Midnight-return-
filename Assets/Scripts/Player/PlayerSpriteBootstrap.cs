using UnityEngine;
using UnityEngine.InputSystem;
using MidnightReturn.Systems.Rendering;

namespace MidnightReturn.Player
{
    // ══════════════════════════════════════════════════════════════════════════
    //  PlayerSpriteBootstrap — assembles the player's sprite-in-3D quad in code
    //  and plugs in the generated Alucard sheets, mirroring VerticalSliceBootstrap.
    //
    //  Drop the 4 PNGs at Assets/Art/Reference/Player/ and wire them into the four
    //  Texture2D slots below (see that folder's README for filenames + import
    //  settings). On Start it builds:  Quad + MeshRenderer(SpriteMaterial) +
    //  SpriteAnimator + SpriteBillboard + PlayerSpriteController, then injects the
    //  active atlas via SpriteAnimator.Configure().
    //
    //  • CORE sheets (v1/v2) drive the real PlayerSpriteController from FSM state.
    //  • Preview mode (and the ADVANCED sheets) cycle through EVERY clip of the
    //    active sheet, detached from gameplay — the in-engine A/B comparison.
    //
    //  Hotkeys (play mode):  Tab = next of the 4 sheets   ·   P = toggle preview.
    // ══════════════════════════════════════════════════════════════════════════
    public sealed class PlayerSpriteBootstrap : MonoBehaviour
    {
        public enum PlayerSheet { CoreV1, CoreV2, AdvancedV1, AdvancedV2 }

        [Header("Generated Sheets — Assets/Art/Reference/Player/*.png")]
        [Tooltip("Alucard_Core_v1.png — original 16-row core sheet.")]
        [SerializeField] private Texture2D _coreV1;
        [Tooltip("Alucard_Core_v2.png — revised anime-samurai 16-row core sheet.")]
        [SerializeField] private Texture2D _coreV2;
        [Tooltip("Alucard_Advanced_v1.png — original 10-row advanced moves.")]
        [SerializeField] private Texture2D _advancedV1;
        [Tooltip("Alucard_Advanced_v2.png — revised anime-samurai 10-row advanced moves.")]
        [SerializeField] private Texture2D _advancedV2;

        [Header("Active Selection")]
        [SerializeField] private PlayerSheet _activeSheet = PlayerSheet.CoreV2;
        [Tooltip("Cycle EVERY clip of the active sheet, detached from gameplay — for A/B review.")]
        [SerializeField] private bool  _previewMode = false;
        [SerializeField] private float _previewSecondsPerClip = 1.5f;

        [Header("Quad Transform")]
        [Tooltip("Auto-found by 'Player' tag when left empty.")]
        [SerializeField] private PlayerController _player;
        [SerializeField] private Vector3 _localOffset = new(0f, 1.4f, 0f);
        [SerializeField] private Vector3 _quadScale   = new(3.2f, 3.2f, 1f);

        [Header("Hotkeys")]
        [Tooltip("Tab = next sheet · P = toggle preview (play mode).")]
        [SerializeField] private bool _enableHotkeys = true;

        private SpriteAnimator         _anim;
        private PlayerSpriteController _spriteController;
        private MeshRenderer           _renderer;

        private SpriteSheetDataSO _coreCfgV1, _coreCfgV2, _advCfgV1, _advCfgV2;
        private SpriteSheetDataSO _activeCfg;

        private int   _previewIndex;
        private float _previewTimer;

        private void Start()
        {
            if (_player == null)
            {
                var tagged = GameObject.FindGameObjectWithTag("Player");
                if (tagged != null) _player = tagged.GetComponent<PlayerController>();
            }

            BuildConfigs();
            BuildQuad();
            ApplySheet(_activeSheet);
        }

        private void BuildConfigs()
        {
            if (_coreV1     != null) _coreCfgV1 = PlayerSpriteContent.BuildCore(_coreV1);
            if (_coreV2     != null) _coreCfgV2 = PlayerSpriteContent.BuildCore(_coreV2);
            if (_advancedV1 != null) _advCfgV1  = PlayerSpriteContent.BuildAdvanced(_advancedV1);
            if (_advancedV2 != null) _advCfgV2  = PlayerSpriteContent.BuildAdvanced(_advancedV2);
        }

        private void BuildQuad()
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "PlayerSpriteQuad";
            Destroy(quad.GetComponent<Collider>());

            Transform parent = _player != null ? _player.transform : transform;
            quad.transform.SetParent(parent, false);
            quad.transform.localPosition = _localOffset;
            quad.transform.localScale    = _quadScale;

            _renderer = quad.GetComponent<MeshRenderer>();
            _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _renderer.sharedMaterial    = SpriteMaterial.BuildSpriteMaterial(_coreV2 ?? _coreV1);

            _anim = quad.AddComponent<SpriteAnimator>();
            quad.AddComponent<SpriteBillboard>();
            _spriteController = quad.AddComponent<PlayerSpriteController>();
        }

        // Swap the live atlas. Core → gameplay-driven; Advanced/preview → clip cycle.
        public void ApplySheet(PlayerSheet sheet)
        {
            _activeSheet = sheet;
            _activeCfg = sheet switch
            {
                PlayerSheet.CoreV1     => _coreCfgV1,
                PlayerSheet.CoreV2     => _coreCfgV2,
                PlayerSheet.AdvancedV1 => _advCfgV1,
                PlayerSheet.AdvancedV2 => _advCfgV2,
                _                      => _coreCfgV2,
            };

            if (_activeCfg == null)
            {
                Debug.LogWarning($"[PlayerSpriteBootstrap] No texture wired for {sheet} — drop the PNG and assign it.");
                return;
            }

            _anim.Configure(_activeCfg);
            if (_renderer != null && _activeCfg.Sheet != null && _renderer.sharedMaterial.HasProperty("_BaseColorMap"))
                _renderer.sharedMaterial.SetTexture("_BaseColorMap", _activeCfg.Sheet);

            _spriteController.enabled = IsGameplayDriven();

            _previewIndex = 0;
            _previewTimer = 0f;
            if (!IsGameplayDriven() && _activeCfg.Clips.Length > 0)
                _anim.Play(_activeCfg.Clips[0].Name);
        }

        // Core sheets with a real player & no preview animate from FSM state.
        private bool IsGameplayDriven() =>
            !_previewMode
            && _player != null
            && (_activeSheet == PlayerSheet.CoreV1 || _activeSheet == PlayerSheet.CoreV2);

        private void Update()
        {
            if (_enableHotkeys && Keyboard.current != null)
            {
                if (Keyboard.current.tabKey.wasPressedThisFrame)
                    ApplySheet((PlayerSheet)(((int)_activeSheet + 1) % 4));
                if (Keyboard.current.pKey.wasPressedThisFrame)
                {
                    _previewMode = !_previewMode;
                    ApplySheet(_activeSheet);
                }
            }

            if (IsGameplayDriven() || _activeCfg == null || _activeCfg.Clips.Length == 0)
                return;

            _previewTimer += Time.deltaTime;
            if (_previewTimer >= _previewSecondsPerClip)
            {
                _previewTimer = 0f;
                _previewIndex = (_previewIndex + 1) % _activeCfg.Clips.Length;
                _anim.Play(_activeCfg.Clips[_previewIndex].Name);
            }
        }

        [ContextMenu("Apply Active Sheet")]
        private void ApplyActiveFromInspector()
        {
            if (_activeCfg == null) { BuildConfigs(); if (_anim == null) return; }
            ApplySheet(_activeSheet);
        }
    }
}
