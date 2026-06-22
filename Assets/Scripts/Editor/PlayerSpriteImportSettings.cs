#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MidnightReturn.EditorTools
{
    // Forces crisp, pixel-perfect, full-resolution import on the generated player
    // sheets the moment a PNG is dropped into Assets/Art/Reference/Player/ — so the
    // sprite atlas samples correctly with no manual Inspector fiddling.
    //
    // Matches the import contract documented in Assets/Art/Reference/SETUP.md.
    public sealed class PlayerSpriteImportSettings : AssetPostprocessor
    {
        private const string Folder = "Assets/Art/Reference/Player/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Folder)) return;

            var ti = (TextureImporter)assetImporter;
            ti.textureType         = TextureImporterType.Default;   // atlas, NOT Sprite
            ti.npotScale           = TextureImporterNPOTScale.None;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled       = false;
            ti.filterMode          = FilterMode.Point;              // crisp edges
            ti.wrapMode            = TextureWrapMode.Clamp;
            ti.isReadable          = false;
            ti.textureCompression  = TextureImporterCompression.Uncompressed;
            ti.maxTextureSize      = 8192;                          // sheets are 3072×5504
        }
    }
}
#endif
