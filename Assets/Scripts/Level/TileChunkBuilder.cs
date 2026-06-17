using System.Collections.Generic;
using UnityEngine;
using MidnightReturn.Data;

namespace MidnightReturn.Level
{
    // Bakes a TilemapLayerSO into a single combined mesh (one draw call per layer)
    // plus merged box colliders for solid tiles. No per-tile GameObjects — the
    // whole layer is one MeshFilter/MeshRenderer. Rebuilds in editor on demand.
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class TileChunkBuilder : MonoBehaviour
    {
        [SerializeField] private TileSetSO       _tileSet;
        [SerializeField] private TilemapLayerSO  _layer;
        [SerializeField] private bool            _buildOnAwake = true;

        private MeshFilter   _filter;
        private MeshRenderer _renderer;

        private void Awake()
        {
            _filter   = GetComponent<MeshFilter>();
            _renderer = GetComponent<MeshRenderer>();
            if (_buildOnAwake) Build();
        }

        // Runtime injection point — lets a procedural builder assign the palette
        // and layer after AddComponent, since both fields are serialized/private.
        public void Configure(TileSetSO tileSet, TilemapLayerSO layer)
        {
            _tileSet = tileSet;
            _layer   = layer;
        }

        [ContextMenu("Build Tilemap")]
        public void Build()
        {
            if (_tileSet == null || _layer == null) return;
            if (_filter == null)   _filter   = GetComponent<MeshFilter>();
            if (_renderer == null) _renderer = GetComponent<MeshRenderer>();

            _layer.EnsureSize();
            BuildMesh();
            if (_tileSet.Material != null) _renderer.sharedMaterial = _tileSet.Material;
            if (_layer.BuildColliders) BuildColliders();
        }

        private void BuildMesh()
        {
            float s = _tileSet.TileWorldSize;
            float z = _layer.ZDepth;

            var verts = new List<Vector3>();
            var uvs   = new List<Vector2>();
            var tris  = new List<int>();
            var norms = new List<Vector3>();

            for (int y = 0; y < _layer.Height; y++)
            {
                for (int x = 0; x < _layer.Width; x++)
                {
                    int tile = _layer.Get(x, y);
                    if (tile < 0) continue;

                    // World position — y inverted so row 0 sits at the top.
                    float wx = x * s;
                    float wy = (_layer.Height - 1 - y) * s;

                    int vbase = verts.Count;
                    verts.Add(new Vector3(wx,     wy,     z));
                    verts.Add(new Vector3(wx + s, wy,     z));
                    verts.Add(new Vector3(wx + s, wy + s, z));
                    verts.Add(new Vector3(wx,     wy + s, z));

                    var uv = _tileSet.GetUV(tile); // (uMin,vMin,uMax,vMax)
                    uvs.Add(new Vector2(uv.x, uv.y));
                    uvs.Add(new Vector2(uv.z, uv.y));
                    uvs.Add(new Vector2(uv.z, uv.w));
                    uvs.Add(new Vector2(uv.x, uv.w));

                    for (int n = 0; n < 4; n++) norms.Add(Vector3.forward);

                    tris.Add(vbase);     tris.Add(vbase + 1); tris.Add(vbase + 2);
                    tris.Add(vbase);     tris.Add(vbase + 2); tris.Add(vbase + 3);
                }
            }

            var mesh = new Mesh { name = $"TileChunk_{_layer.name}" };
            if (verts.Count > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetNormals(norms);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            _filter.sharedMesh = mesh;
        }

        // Merge horizontal runs of solid tiles per row into single BoxColliders.
        private void BuildColliders()
        {
            // Clear previous colliders
            foreach (var c in GetComponents<BoxCollider>()) DestroyImmediateSafe(c);

            float s = _tileSet.TileWorldSize;

            for (int y = 0; y < _layer.Height; y++)
            {
                int runStart = -1;
                for (int x = 0; x <= _layer.Width; x++)
                {
                    int tile  = x < _layer.Width ? _layer.Get(x, y) : -1;
                    bool solid = tile >= 0 && _tileSet.IsSolid(tile);

                    if (solid && runStart < 0) runStart = x;
                    else if (!solid && runStart >= 0)
                    {
                        int len = x - runStart;
                        float wy = (_layer.Height - 1 - y) * s;
                        var box = gameObject.AddComponent<BoxCollider>();
                        box.size   = new Vector3(len * s, s, s);
                        box.center = new Vector3(runStart * s + len * s * 0.5f, wy + s * 0.5f, 0f);
                        runStart = -1;
                    }
                }
            }
        }

        private static void DestroyImmediateSafe(Object o)
        {
            if (Application.isPlaying) Destroy(o);
            else DestroyImmediate(o);
        }
    }
}
