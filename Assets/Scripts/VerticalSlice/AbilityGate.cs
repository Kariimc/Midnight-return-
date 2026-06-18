using System.Collections;
using UnityEngine;
using MidnightReturn.Player;
using MidnightReturn.Utils;

namespace MidnightReturn.VerticalSlice
{
    public enum GateType        { Spectral, IronGate, CrackedWall }
    public enum RequiredAbility { AirDash, DoubleJump, WallJump, SoulTether, WraithStep }

    // ══════════════════════════════════════════════════════════════════════════
    //  AbilityGate — code-placed passage blocker with three visual archetypes.
    //
    //  The main GameObject carries a solid BoxCollider (blocks CharacterController).
    //  A child "Sensor" carries an isTrigger BoxCollider; OnTriggerStay fires
    //  TryUnlock which checks the player's ability flags. On success:
    //    1. Solid collider disabled immediately (player can pass)
    //    2. 0.5s dissolve on the visual mesh
    //    3. ScreenFlashEvent + CameraShakeEvent emitted
    //    4. GameObject destroyed when dissolved
    // ══════════════════════════════════════════════════════════════════════════
    [RequireComponent(typeof(BoxCollider))]
    public sealed class AbilityGate : MonoBehaviour
    {
        [SerializeField] private GateType        _gateType = GateType.Spectral;
        [SerializeField] private RequiredAbility _requires = RequiredAbility.AirDash;
        [SerializeField] private Vector3         _size     = new(0.3f, 3.5f, 1f);

        private BoxCollider  _blocker;
        private MeshRenderer _renderer;
        private bool         _unlocked;

        private void Start()
        {
            _blocker = GetComponent<BoxCollider>();
            _blocker.isTrigger = false;
            _blocker.size      = _size;

            BuildVisual();
            BuildSensor();
        }

        public void TryUnlock(Collider playerCol)
        {
            if (_unlocked) return;
            var pm = playerCol.GetComponent<PlayerMovement>();
            if (pm == null || !HasAbility(pm)) return;
            _unlocked = true;
            _blocker.enabled = false;
            StartCoroutine(Dissolve());
        }

        private bool HasAbility(PlayerMovement pm) => _requires switch
        {
            RequiredAbility.AirDash    => pm.CanAirDash,
            RequiredAbility.DoubleJump => pm.CanDoubleJump,
            RequiredAbility.WallJump   => pm.CanWallJump,
            RequiredAbility.SoulTether => pm.CanSoulTether,
            RequiredAbility.WraithStep => pm.CanWraithStep,
            _                          => false,
        };

        private IEnumerator Dissolve()
        {
            EventBus.Emit(new ScreenFlashEvent
            {
                Color    = _gateType == GateType.Spectral
                    ? new Color(0.55f, 0.3f, 1f, 0.55f)
                    : new Color(0.75f, 0.65f, 0.35f, 0.5f),
                Duration = 0.35f,
            });
            EventBus.Emit(new CameraShakeEvent { Intensity = 0.25f, Duration = 0.25f });

            float t = 0f;
            while (t < 0.5f && _renderer != null)
            {
                t += Time.deltaTime;
                float a = 1f - t / 0.5f;
                var   c = _renderer.material.HasProperty("_BaseColor")
                    ? _renderer.material.GetColor("_BaseColor")
                    : _renderer.material.color;
                c.a = a;
                if (_renderer.material.HasProperty("_BaseColor"))
                    _renderer.material.SetColor("_BaseColor", c);
                else
                    _renderer.material.color = c;
                yield return null;
            }
            Destroy(gameObject);
        }

        // ── Visuals ──────────────────────────────────────────────────────────
        private void BuildVisual()
        {
            var vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vis.name = "GateVisual";
            vis.transform.SetParent(transform, false);
            vis.transform.localScale = _size;
            if (vis.TryGetComponent<Collider>(out var c)) Destroy(c);

            _renderer = vis.GetComponent<MeshRenderer>();
            var sh  = Shader.Find("HDRP/Lit")
                   ?? Shader.Find("Universal Render Pipeline/Lit")
                   ?? Shader.Find("Standard");
            var mat = new Material(sh);

            switch (_gateType)
            {
                case GateType.Spectral:
                    ApplyColor(mat, new Color(0.35f, 0.15f, 0.9f, 0.55f));
                    if (mat.HasProperty("_Smoothness"))   mat.SetFloat("_Smoothness",   0.85f);
                    if (mat.HasProperty("_Metallic"))     mat.SetFloat("_Metallic",     0.1f);
                    if (mat.HasProperty("_EmissiveColor"))
                        mat.SetColor("_EmissiveColor", new Color(0.2f, 0.05f, 0.5f));
                    mat.renderQueue = 3000; // transparent queue
                    break;

                case GateType.IronGate:
                    ApplyColor(mat, new Color(0.12f, 0.10f, 0.07f));
                    if (mat.HasProperty("_Metallic"))   mat.SetFloat("_Metallic",   0.85f);
                    if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.35f);
                    break;

                case GateType.CrackedWall:
                    ApplyColor(mat, new Color(0.26f, 0.22f, 0.20f));
                    if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.05f);
                    break;
            }
            _renderer.sharedMaterial = mat;
        }

        private static void ApplyColor(Material mat, Color c)
        {
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
            if (mat.HasProperty("_Color"))     mat.SetColor("_Color",     c);
        }

        // ── Sensor child — detects player with trigger ────────────────────────
        private void BuildSensor()
        {
            var sensor = new GameObject("Sensor");
            sensor.transform.SetParent(transform, false);
            var t = sensor.AddComponent<BoxCollider>();
            t.isTrigger = true;
            t.size      = _size * 1.15f; // slightly larger than blocking collider
            sensor.AddComponent<AbilityGateSensor>().Gate = this;
        }

        // ── Runtime configure (called from Bootstrap) ─────────────────────────
        public void Configure(GateType type, RequiredAbility req, Vector3 size)
        {
            _gateType = type;
            _requires = req;
            _size     = size;
        }
    }

    // Sensor component lives on the trigger child GO.
    internal sealed class AbilityGateSensor : MonoBehaviour
    {
        public AbilityGate Gate;
        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player")) Gate.TryUnlock(other);
        }
    }
}
