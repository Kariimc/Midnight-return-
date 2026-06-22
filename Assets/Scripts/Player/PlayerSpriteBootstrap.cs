using UnityEngine;
using UnityEngine.InputSystem;
using MidnightReturn.Systems.Rendering;

namespace MidnightReturn.Player
{
    // ══════════════════════════════════════════════════════════════════════════
    //  PlayerSpriteBootstrap — builds TWO sprite-in-3D quads for Alucard and
    //  keeps them in sync:
    //
    //    Core quad    — Idle/Run/Jump/Fall/Dash/WallSlide/Attack… clips from the
    //                   16-row Core atlas. Driven by PlayerSpriteController FSM.
    //    Advanced quad— TurnAround/SubWeapon/DragonKick clips from the 10-row
    //                   Advanced atlas. Activated automatically by
    //                   PlayerSpriteController when those FSM states fire.
    //
    //  Both quads always hold matching style versions (v1 = original,
    //  v2 = anime-samurai). Tab cycles style; P toggles per-atlas preview.
    //
    //  Drop the 4 PNGs at Assets/Art/Reference/Player/ and wire them into the
    //  four Texture2D slots. See that folder's README for filenames + job IDs.
    //
    //  Hotkeys (play mode):  Tab = next style  ·  P = toggle preview
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
        [Tooltip("Cycle every clip of the active atlas detached from gameplay — A/B review.")]
        [SerializeField] private bool  _previewMode = false;
        [SerializeField] private float _previewSecondsPerClip = 1.5f;

        [Header("Quad Transform")]
        [Tooltip("Auto-found by 'Player' tag when left empty.")]
        [SerializeField] private PlayerController _player;
        [SerializeField] private Vector3 _localOffset = new(0f, 1.4f, 0f);
        [SerializeField] private Vector3 _quadScale   = new(3.2f, 3.2f, 1f);

        [Header("Hotkeys")]
        [SerializeField] private bool _enableHotkeys = true;

        // Core quad
        private SpriteAnimator         _anim;
        private PlayerSpriteController _spriteController;
        private MeshRenderer           _renderer;

        // Advanced quad — visibility managed by PlayerSpriteController
        private SpriteAnimator _advAnim;
        private MeshRenderer   _advRenderer;

        // Per-style config pairs (both built from the same clip map, different textures)
        private SpriteSheetDataSO _coreCfgV1, _coreCfgV2, _advCfgV1, _advCfgV2;
        private SpriteSheetDataSO _activeCfg;   // used by preview cycling

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
            BuildQuads();
            ApplySheet(_activeSheet);
        }

        private void BuildConfigs()
        {
            if (_coreV1     != null) _coreCfgV1 = PlayerSpriteContent.BuildCore(_coreV1);
            if (_coreV2     != null) _coreCfgV2 = PlayerSpriteContent.BuildCore(_coreV2);
            if (_advancedV1 != null) _advCfgV1  = PlayerSpriteContent.BuildAdvanced(_advancedV1);
            if (_advancedV2 != null) _advCfgV2  = PlayerSpriteContent.BuildAdvanced(_advancedV2);
        }

        private void BuildQuads()
        {
            Transform parent = _player != null ? _player.transform : transform;

            // ── Core quad ─────────────────────────────────────────────────────
            var coreGO = MakeSpriteQuad("PlayerSpriteQuad_Core", parent,
                                        _localOffset, _quadScale, _coreV2 ?? _coreV1);
            _renderer = coreGO.GetComponent<MeshRenderer>();
            _anim     = coreGO.AddComponent<SpriteAnimator>();
            coreGO.AddComponent<SpriteBillboard>();
            _spriteController = coreGO.AddComponent<PlayerSpriteController>();

            // ── Advanced quad ─────────────────────────────────────────────────
            var advGO = MakeSpriteQuad("PlayerSpriteQuad_Advanced", parent,
                                       _localOffset, _quadScale, _advancedV2 ?? _advancedV1);
            _advRenderer = advGO.GetComponent<MeshRenderer>();
            _advAnim     = advGO.AddComponent<SpriteAnimator>();
            advGO.AddComponent<SpriteBillboard>();
            _advRenderer.enabled = false;   // hidden until an Advanced FSM state fires

            // Hand the Advanced animator to the controller so it can toggle quads
            _spriteController.ConfigureAdvanced(_advAnim);
        }

        private static GameObject MakeSpriteQuad(string name, Transform parent,
            Vector3 offset, Vector3 scale, Texture2D sheet)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;
            Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = offset;
            go.transform.localScale    = scale;

            var mr = go.GetComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.sharedMaterial    = SpriteMaterial.BuildSpriteMaterial(sheet);
            return go;
        }

        // Swap the active style. Both quads always receive matching versions (v1↔v2)
        // so Core and Advanced clips never show mismatched art styles.
        public void ApplySheet(PlayerSheet sheet)
        {
            _activeSheet = sheet;

            bool v2         = sheet == PlayerSheet.CoreV2 || sheet == PlayerSheet.AdvancedV2;
            bool previewAdv = _previewMode
                              && (sheet == PlayerSheet.AdvancedV1 || sheet == PlayerSheet.AdvancedV2);

            var coreCfg = v2 ? _coreCfgV2 : _coreCfgV1;
            var advCfg  = v2 ? _advCfgV2  : _advCfgV1;

            // ── Reconfigure animators ──────────────────────────────────────────
            if (coreCfg != null)
            {
                _anim.Configure(coreCfg);
                SetMaterialSheet(_renderer, coreCfg.Sheet);
            }
            else
            {
                Debug.LogWarning($"[PlayerSpriteBootstrap] No Core texture for style {(v2 ? "V2" : "V1")} — drop the PNG and assign it.");
            }

            if (_advAnim != null)
            {
                if (advCfg != null)
                {
                    _advAnim.Configure(advCfg);
                    SetMaterialSheet(_advRenderer, advCfg.Sheet);
                }
                else
                {
                    Debug.LogWarning($"[PlayerSpriteBootstrap] No Advanced texture for style {(v2 ? "V2" : "V1")} — drop the PNG and assign it.");
                }
            }

            // ── Gameplay vs. preview ───────────────────────────────────────────
            bool gameplay = IsGameplayDriven();
            _spriteController.enabled = gameplay;
            _spriteController.ResetClip();

            if (!gameplay)
            {
                // Preview mode: show only the atlas being cycled; hide the other.
                _renderer.enabled    = !previewAdv;
                if (_advRenderer != null) _advRenderer.enabled = previewAdv;
            }

            // _activeCfg drives preview clip cycling
            _activeCfg    = previewAdv ? advCfg : coreCfg;
            _previewIndex = 0;
            _previewTimer = 0f;

            if (!gameplay && _activeCfg?.Clips.Length > 0)
            {
                var firstAnim = previewAdv && _advAnim != null ? _advAnim : _anim;
                firstAnim.Play(_activeCfg.Clips[0].Name);
            }
        }

        private static void SetMaterialSheet(MeshRenderer mr, Texture2D sheet)
        {
            if (mr == null || sheet == null) return;
            if (mr.sharedMaterial.HasProperty("_BaseColorMap"))
                mr.sharedMaterial.SetTexture("_BaseColorMap", sheet);
            else if (mr.sharedMaterial.HasProperty("_MainTex"))
                mr.sharedMaterial.SetTexture("_MainTex", sheet);
        }

        // Core sheet + player found + not in preview = FSM drives everything.
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
                _previewTimer  = 0f;
                _previewIndex  = (_previewIndex + 1) % _activeCfg.Clips.Length;

                bool previewAdv = _previewMode
                    && (_activeSheet == PlayerSheet.AdvancedV1 || _activeSheet == PlayerSheet.AdvancedV2);
                var previewAnim = previewAdv && _advAnim != null ? _advAnim : _anim;
                previewAnim.Play(_activeCfg.Clips[_previewIndex].Name);
            }
        }

        [ContextMenu("Apply Active Sheet")]
        private void ApplyActiveFromInspector()
        {
            if (_anim == null) return;
            BuildConfigs();
            ApplySheet(_activeSheet);
        }
    }
}
