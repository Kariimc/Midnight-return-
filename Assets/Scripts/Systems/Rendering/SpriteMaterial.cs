using UnityEngine;

namespace MidnightReturn.Systems.Rendering
{
    // Builds an HDRP/Lit material set up for atlas-sampled sprite quads:
    // alpha-clipped (crisp cutout edges), base map assigned, lit by the world.
    // Shader fallback chain mirrors HDRPTileMaterial: HDRP/Lit → URP/Lit →
    // Standard → Sprites/Default. SpriteAnimator drives _BaseColorMap_ST per frame.
    public static class SpriteMaterial
    {
        public static Material BuildSpriteMaterial(Texture2D sheet, float alphaCutoff = 0.5f)
        {
            var sh = Shader.Find("HDRP/Lit")
                  ?? Shader.Find("Universal Render Pipeline/Lit")
                  ?? Shader.Find("Standard")
                  ?? Shader.Find("Sprites/Default");

            var mat = new Material(sh);

            // Base color map (the atlas). SpriteAnimator re-binds this too via MPB,
            // but assigning here keeps the material valid for editor preview.
            if (sheet != null)
            {
                if (mat.HasProperty("_BaseColorMap")) mat.SetTexture("_BaseColorMap", sheet);
                else if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", sheet);
            }

            // White base tint so the texture shows unmodulated.
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
            if (mat.HasProperty("_Color"))     mat.SetColor("_Color",     Color.white);

            // Flat, non-metallic — sprites read their shading from world lights only.
            if (mat.HasProperty("_Metallic"))   mat.SetFloat("_Metallic",   0f);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0f);

            EnableAlphaClipping(mat, alphaCutoff);
            return mat;
        }

        // Alpha test ("cutout") — keeps hard sprite edges and correct sorting
        // without the transparency overdraw/order pitfalls of blended alpha.
        private static void EnableAlphaClipping(Material mat, float cutoff)
        {
            // HDRP/Lit
            if (mat.HasProperty("_AlphaCutoffEnable")) mat.SetFloat("_AlphaCutoffEnable", 1f);
            if (mat.HasProperty("_AlphaCutoff"))       mat.SetFloat("_AlphaCutoff",       cutoff);
            // URP/Lit + Standard
            if (mat.HasProperty("_AlphaClip"))         mat.SetFloat("_AlphaClip",         1f);
            if (mat.HasProperty("_Cutoff"))            mat.SetFloat("_Cutoff",            cutoff);

            mat.EnableKeyword("_ALPHATEST_ON");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
        }
    }
}
