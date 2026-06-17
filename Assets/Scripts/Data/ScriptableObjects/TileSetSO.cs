using System.Collections.Generic;
using UnityEngine;

namespace MidnightReturn.Data
{
    // Atlas-backed tile palette. Tiles are referenced by their atlas index
    // (row-major, 0-based) inside a TilemapLayerSO grid. Solidity is a sparse
    // set — only collidable indices need listing.
    [CreateAssetMenu(fileName = "TileSet_", menuName = "MidnightReturn/Tile Set")]
    public class TileSetSO : ScriptableObject
    {
        [Header("Atlas")]
        public Texture2D Atlas;
        public Material  Material;     // HDRP/Lit using the atlas as Base Color Map
        public int       Columns = 8;
        public int       Rows    = 8;

        [Header("World")]
        [Tooltip("Edge length of one tile in world units.")]
        public float TileWorldSize = 1f;

        [Header("Collision")]
        [Tooltip("Atlas indices that are solid (generate colliders).")]
        public int[] SolidTileIndices;

        private HashSet<int> _solidLookup;

        public bool IsSolid(int index)
        {
            if (_solidLookup == null)
            {
                _solidLookup = new HashSet<int>();
                if (SolidTileIndices != null)
                    foreach (var i in SolidTileIndices) _solidLookup.Add(i);
            }
            return _solidLookup.Contains(index);
        }

        // UV rect for an atlas index: (uMin, vMin, uMax, vMax). Atlas is top-left
        // origin (row 0 at top); UVs are bottom-left, so the row is flipped.
        public Vector4 GetUV(int index)
        {
            int col = index % Columns;
            int row = index / Columns;
            int flippedRow = (Rows - 1) - row;
            float uw = 1f / Columns;
            float vh = 1f / Rows;
            float uMin = col * uw;
            float vMin = flippedRow * vh;
            // Tiny inset prevents atlas bleeding at tile seams.
            const float pad = 0.0005f;
            return new Vector4(uMin + pad, vMin + pad, uMin + uw - pad, vMin + vh - pad);
        }
    }
}
