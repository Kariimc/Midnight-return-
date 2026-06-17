using UnityEngine;

namespace MidnightReturn.Level
{
    // Builds a fully-wired HDRP/Lit material for tilemap layers.
    //
    // All PBR channels (BaseColor, Normal, MaskMap) are optional — the method
    // returns a valid material whether or not textures are supplied.  Callers
    // should assign the result to TileSetSO.Material before TileChunkBuilder.Build().
    //
    // HDRP MaskMap channel packing:  R=Metallic  G=AO  B=DetailMask  A=Smoothness
    public static class HDRPTileMaterial
    {
        // ── Stone (rough, non-metallic) ────────────────────────────────────────
        public static Material BuildStoneMaterial(
            Texture2D baseColor  = null,
            Texture2D normalMap  = null,
            Texture2D maskMap    = null,
            float     normalScale = 1.4f,
            float     smoothness  = 0.14f)
        {
            var mat = CreateLitMaterial(new Color(0.38f, 0.34f, 0.42f));

            ApplyBaseColor(mat, baseColor);
            ApplyNormalMap(mat, normalMap, normalScale);
            ApplyMaskMap(mat, maskMap, metallic: 0f, smoothness: smoothness);

            // Stone-specific: slight ambient occlusion baked into tint
            if (mat.HasProperty("_AORemapMin")) mat.SetFloat("_AORemapMin", 0f);

            return mat;
        }

        // ── Brick / mortar ──────────────────────────────────────────────────────
        public static Material BuildBrickMaterial(
            Texture2D baseColor  = null,
            Texture2D normalMap  = null,
            Texture2D maskMap    = null)
        {
            var mat = CreateLitMaterial(new Color(0.28f, 0.24f, 0.32f));
            ApplyBaseColor(mat, baseColor);
            ApplyNormalMap(mat, normalMap, 1.8f);
            ApplyMaskMap(mat, maskMap, metallic: 0f, smoothness: 0.08f);
            return mat;
        }

        // ── Metal trim (chains, ladders) ────────────────────────────────────────
        public static Material BuildMetalMaterial(
            Texture2D baseColor = null,
            Texture2D normalMap = null,
            Texture2D maskMap   = null)
        {
            var mat = CreateLitMaterial(new Color(0.55f, 0.48f, 0.36f));
            ApplyBaseColor(mat, baseColor);
            ApplyNormalMap(mat, normalMap, 1.2f);
            ApplyMaskMap(mat, maskMap, metallic: 0.85f, smoothness: 0.45f);
            return mat;
        }

        // ── Helpers ─────────────────────────────────────────────────────────────
        private static Material CreateLitMaterial(Color fallbackColor)
        {
            var sh = Shader.Find("HDRP/Lit")
                  ?? Shader.Find("Universal Render Pipeline/Lit")
                  ?? Shader.Find("Standard");
            var mat = new Material(sh ?? Shader.Find("Sprites/Default"));
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", fallbackColor);
            if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", fallbackColor);
            return mat;
        }

        private static void ApplyBaseColor(Material mat, Texture2D tex)
        {
            if (tex == null) return;
            if (mat.HasProperty("_BaseColorMap")) mat.SetTexture("_BaseColorMap", tex);
            else if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
        }

        private static void ApplyNormalMap(Material mat, Texture2D tex, float scale)
        {
            if (tex == null) return;
            if (mat.HasProperty("_NormalMap"))
            {
                mat.SetTexture("_NormalMap", tex);
                if (mat.HasProperty("_NormalScale")) mat.SetFloat("_NormalScale", scale);
                mat.EnableKeyword("_NORMALMAP");
            }
            else if (mat.HasProperty("_BumpMap"))
            {
                mat.SetTexture("_BumpMap", tex);
                if (mat.HasProperty("_BumpScale")) mat.SetFloat("_BumpScale", scale);
            }
        }

        private static void ApplyMaskMap(Material mat, Texture2D tex, float metallic, float smoothness)
        {
            if (tex != null && mat.HasProperty("_MaskMap"))
            {
                mat.SetTexture("_MaskMap", tex);
                mat.EnableKeyword("_MASKMAP");
            }

            if (mat.HasProperty("_Metallic"))    mat.SetFloat("_Metallic",    metallic);
            if (mat.HasProperty("_Smoothness"))  mat.SetFloat("_Smoothness",  smoothness);
            if (mat.HasProperty("_Glossiness"))  mat.SetFloat("_Glossiness",  smoothness);
        }
    }
}
