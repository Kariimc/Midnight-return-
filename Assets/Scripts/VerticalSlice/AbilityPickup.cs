using System;
using System.Collections;
using UnityEngine;
using MidnightReturn.Player;
using MidnightReturn.Utils;

namespace MidnightReturn.VerticalSlice
{
    // ══════════════════════════════════════════════════════════════════════════
    //  AbilityPickup — floating glowing orb that triggers AbilityAcquisition-
    //  Presenter when the player walks into it.
    //
    //  Usage (from Bootstrap):
    //    var pickup = SpawnAbilityPickup(pos, new Color(0.4f, 0.2f, 0.9f));
    //    pickup.Init("Double Jump",
    //                "Press Jump again while airborne.",
    //                pm => pm.CanDoubleJump = true);
    // ══════════════════════════════════════════════════════════════════════════
    [RequireComponent(typeof(SphereCollider))]
    public sealed class AbilityPickup : MonoBehaviour
    {
        private string                           _abilityName;
        private string                           _description;
        private Action<PlayerMovement>    _grantAbility;
        private Color                            _color;
        private Light                            _light;
        private bool                             _collected;

        public void Init(
            string                        abilityName,
            string                        description,
            Action<PlayerMovement> grantAbility,
            Color                         color)
        {
            _abilityName  = abilityName;
            _description  = description;
            _grantAbility = grantAbility;
            _color        = color;

            BuildVisual();
            StartCoroutine(BobAndPulse());
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_collected || !other.CompareTag("Player")) return;
            var pm = other.GetComponent<PlayerMovement>();
            if (pm == null) return;
            _collected = true;
            Collect(pm);
        }

        private void Collect(PlayerMovement pm)
        {
            // Particles burst
            EventBus.Emit(new ScreenFlashEvent
            {
                Color    = new Color(_color.r, _color.g, _color.b, 0.6f),
                Duration = 0.4f,
            });
            EventBus.Emit(new CameraShakeEvent { Intensity = 0.2f, Duration = 0.3f });
            EventBus.Emit(new ItemPickedUpEvent
            {
                ItemId   = "ability_" + _abilityName.ToLower().Replace(' ', '_'),
                Quantity = 1,
            });

            // Hide orb immediately while fanfare runs
            if (_light) _light.enabled = false;
            foreach (var r in GetComponentsInChildren<MeshRenderer>())
                r.enabled = false;

            AbilityAcquisitionPresenter.Show(
                _abilityName,
                _description,
                () =>
                {
                    _grantAbility?.Invoke(pm);
                    Destroy(gameObject);
                });
        }

        // ── Visual ────────────────────────────────────────────────────────────
        private void BuildVisual()
        {
            var col = GetComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius    = 0.65f;

            // Inner bright sphere
            var inner = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            inner.transform.SetParent(transform, false);
            inner.transform.localScale = Vector3.one * 0.55f;
            if (inner.TryGetComponent<Collider>(out var c)) Destroy(c);
            var mat = new Material(Shader.Find("HDRP/Lit") ?? Shader.Find("Standard"));
            if (mat.HasProperty("_BaseColor"))      mat.SetColor("_BaseColor", _color);
            if (mat.HasProperty("_Color"))          mat.SetColor("_Color",     _color);
            if (mat.HasProperty("_EmissiveColor"))  mat.SetColor("_EmissiveColor", _color * 2.5f);
            inner.GetComponent<MeshRenderer>().sharedMaterial = mat;

            // Outer wispy shell
            var outer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            outer.transform.SetParent(transform, false);
            outer.transform.localScale = Vector3.one * 0.9f;
            if (outer.TryGetComponent<Collider>(out var c2)) Destroy(c2);
            var mat2 = new Material(mat);
            var shellCol = new Color(_color.r, _color.g, _color.b, 0.22f);
            if (mat2.HasProperty("_BaseColor")) mat2.SetColor("_BaseColor", shellCol);
            if (mat2.HasProperty("_Color"))     mat2.SetColor("_Color",     shellCol);
            mat2.renderQueue = 3000;
            outer.GetComponent<MeshRenderer>().sharedMaterial = mat2;

            // Point light
            _light = gameObject.AddComponent<Light>();
            _light.type      = LightType.Point;
            _light.color     = _color;
            _light.range     = 5f;
            _light.intensity = 2.5f;
        }

        private IEnumerator BobAndPulse()
        {
            var origin = transform.localPosition;
            while (true)
            {
                float t = Time.time;
                transform.localPosition  = origin + new Vector3(0f, Mathf.Sin(t * 1.8f) * 0.18f, 0f);
                transform.localRotation  = Quaternion.Euler(0f, t * 45f, 0f);
                if (_light) _light.intensity = 2f + Mathf.Sin(t * 3.5f) * 0.6f;
                yield return null;
            }
        }
    }
}
