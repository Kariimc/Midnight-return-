using UnityEngine;

namespace MidnightReturn.Systems.Rendering
{
    // Drives UV tiling + offset on an HDRP/Lit material via MaterialPropertyBlock.
    // Zero GC per frame — reuses _mpb and reads from SpriteSheetDataSO.
    // Attach alongside a MeshRenderer on the sprite quad.
    [RequireComponent(typeof(MeshRenderer))]
    public class SpriteAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteSheetDataSO _sheet;

        private MeshRenderer       _renderer;
        private MaterialPropertyBlock _mpb;

        private SpriteClip _current;
        private bool       _playing;
        private float      _timer;
        private int        _frame;   // local frame within clip
        private bool       _loop;

        // ── Shader property IDs (cached once) ────────────────────────────────
        private static readonly int _stId   = Shader.PropertyToID("_BaseColorMap_ST");
        private static readonly int _texId  = Shader.PropertyToID("_BaseColorMap");

        private void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
            _mpb      = new MaterialPropertyBlock();

            if (_sheet?.Sheet != null)
            {
                _renderer.GetPropertyBlock(_mpb);
                _mpb.SetTexture(_texId, _sheet.Sheet);
                _renderer.SetPropertyBlock(_mpb);
            }
        }

        public void Play(string clipName)
        {
            if (_sheet == null || !_sheet.TryGetClip(clipName, out var clip)) return;
            if (_playing && _current.Name == clipName) return;

            _current = clip;
            _playing = true;
            _frame   = 0;
            _timer   = 0f;
            ApplyFrame();
        }

        public void Stop() => _playing = false;

        public bool IsPlaying(string clipName) => _playing && _current.Name == clipName;

        private void Update()
        {
            if (!_playing || _current.FrameCount <= 0) return;

            float spf = 1f / Mathf.Max(1f, _current.Fps);
            _timer += Time.deltaTime;

            if (_timer >= spf)
            {
                _timer -= spf;
                _frame++;

                if (_frame >= _current.FrameCount)
                {
                    if (_current.Loop) _frame = 0;
                    else { _frame = _current.FrameCount - 1; _playing = false; }
                }

                ApplyFrame();
            }
        }

        private void ApplyFrame()
        {
            if (_sheet == null) return;

            int absFrame = _current.StartFrame + _frame;
            Vector4 uv   = _sheet.GetFrameUV(absFrame);

            // _BaseColorMap_ST: xy = tiling (scale), zw = offset
            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetVector(_stId, new Vector4(uv.z, uv.w, uv.x, uv.y));
            _renderer.SetPropertyBlock(_mpb);
        }

        // Expose current clip name for debug / editor
        public string CurrentClip => _current.Name;
        public int    CurrentFrame => _frame;
    }
}
