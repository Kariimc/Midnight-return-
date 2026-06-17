using UnityEngine;

namespace MidnightReturn.Level
{
    // Authored grid of tile indices for one depth layer of a room.
    // Tiles are row-major, top row first. -1 = empty cell.
    // Multiple layers (background fill, midground detail, foreground occluders)
    // stack at different ZDepth to compose a scene.
    [CreateAssetMenu(fileName = "TileLayer_", menuName = "MidnightReturn/Tilemap Layer")]
    public class TilemapLayerSO : ScriptableObject
    {
        [Header("Grid")]
        public int Width  = 16;
        public int Height = 12;

        [Tooltip("Row-major (top row first). Length must be Width×Height. -1 = empty.")]
        public int[] Tiles;

        [Header("Placement")]
        [Tooltip("World Z for this layer. Gameplay collision layer should be 0.")]
        public float ZDepth = 0f;
        [Tooltip("Generate merged box colliders for solid tiles on this layer.")]
        public bool  BuildColliders = true;

        public int Get(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return -1;
            int idx = y * Width + x;
            return (Tiles != null && idx < Tiles.Length) ? Tiles[idx] : -1;
        }

        // Convenience for editor/runtime authoring.
        public void EnsureSize()
        {
            int need = Width * Height;
            if (Tiles == null || Tiles.Length != need)
            {
                var resized = new int[need];
                for (int i = 0; i < need; i++)
                    resized[i] = (Tiles != null && i < Tiles.Length) ? Tiles[i] : -1;
                Tiles = resized;
            }
        }
    }
}
