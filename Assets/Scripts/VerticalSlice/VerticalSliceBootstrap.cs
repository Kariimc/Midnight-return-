using System.Collections;
using UnityEngine;
using MidnightReturn.Data;
using MidnightReturn.Level;
using MidnightReturn.Map;
using MidnightReturn.Enemies.Types;
using MidnightReturn.Utils;

namespace MidnightReturn.VerticalSlice
{
    // ══════════════════════════════════════════════════════════════════════════
    //  VerticalSliceBootstrap — assembles the first playable room in code.
    //
    //  Drop this on one empty GameObject (at world origin) in a scene that also
    //  contains a Player (tagged "Player") and a GameManager. On Start it:
    //    1. bakes the tilemap (background + collidable main) into combined meshes
    //    2. spawns two patrolling enemies on the configured enemy layer
    //    3. places a save statue on the high platform
    //    4. drops a climbable ladder from the floor to that platform
    //    5. repositions the player at the room entrance
    //    6. emits ONE RoomTransitionCompleteEvent
    //
    //  Step 6 is the integration proof: MusicDirector and ZoneLightingController
    //  (if present in the scene with authored catalogues) react to that single
    //  event with zero direct wiring from this bootstrap.
    // ══════════════════════════════════════════════════════════════════════════
    public sealed class VerticalSliceBootstrap : MonoBehaviour
    {
        [Header("Tile Rendering (optional — falls back to a flat lit material)")]
        [Tooltip("8×8 tile atlas. If unset, tiles render as a flat stone colour.")]
        [SerializeField] private Texture2D _tileAtlas;
        [Tooltip("Override material for tiles. If unset, one is created at runtime.")]
        [SerializeField] private Material  _tileMaterial;

        [Header("Enemies")]
        [Tooltip("Layer index for spawned enemies — MUST match PlayerCombat's enemy LayerMask.")]
        [SerializeField] private int _enemyLayer = 0;

        [Header("Boot")]
        [Tooltip("Reposition the tagged Player to the room entrance on boot.")]
        [SerializeField] private bool _movePlayerToSpawn = true;

        private Vector3 SpawnPoint => new(
            VerticalSliceContent.ColX(3),
            VerticalSliceContent.SurfaceY(VerticalSliceContent.FLOOR_TOP_ROW) + 0.2f,
            0f);

        private void Start()
        {
            transform.position = Vector3.zero; // room is authored in world space from origin

            BuildTilemap();
            BuildEnemies();
            BuildStatue();
            BuildLadder();

            if (_movePlayerToSpawn)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player) player.transform.position = SpawnPoint;
            }

            RenderSettings.ambientLight = new Color(0.06f, 0.05f, 0.09f);
            StartCoroutine(KickRoomEntered());
        }

        // ── 1. Tilemap ────────────────────────────────────────────────────────
        private void BuildTilemap()
        {
            var set = VerticalSliceContent.BuildTileSet(_tileAtlas, TileMaterial());
            BakeLayer("TileLayer_BG",   VerticalSliceContent.BuildBackgroundLayer(), set);
            BakeLayer("TileLayer_Main", VerticalSliceContent.BuildMainLayer(),       set);
        }

        private void BakeLayer(string name, TilemapLayerSO layer, TileSetSO set)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var builder = go.AddComponent<TileChunkBuilder>(); // adds MeshFilter+Renderer
            builder.Configure(set, layer);
            builder.Build();
        }

        // ── 2. Enemies ──────────────────────────────────────────────────────────
        private void BuildEnemies()
        {
            // Skeleton patrols the main floor near the entrance
            SpawnPatrol(VerticalSliceContent.BuildSkeleton(),
                new Vector3(VerticalSliceContent.ColX(15),
                            VerticalSliceContent.SurfaceY(VerticalSliceContent.FLOOR_TOP_ROW) + 0.1f, 0f),
                new Color(0.85f, 0.85f, 0.7f));

            // Zombie patrols the low-left ledge
            SpawnPatrol(VerticalSliceContent.BuildZombie(),
                new Vector3(VerticalSliceContent.ColX(9),
                            VerticalSliceContent.SurfaceY(VerticalSliceContent.LEDGE_ROW) + 0.1f, 0f),
                new Color(0.3f, 0.7f, 0.25f));
        }

        private void SpawnPatrol(EnemyDataSO data, Vector3 pos, Color tint)
        {
            var go = new GameObject("Enemy_" + data.EnemyId) { layer = _enemyLayer };
            go.transform.position = pos;

            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.6f; cc.radius = 0.4f; cc.center = new Vector3(0f, 0.8f, 0f);

            var vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vis.name = "Visual";
            vis.layer = _enemyLayer;
            vis.transform.SetParent(go.transform, false);
            vis.transform.localScale    = new Vector3(0.8f, 1.6f, 0.4f);
            vis.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            // Drop the primitive's own collider — the CharacterController is the
            // hittable collider PlayerCombat's OverlapBox resolves to.
            var primCol = vis.GetComponent<Collider>();
            if (primCol) Destroy(primCol);
            vis.GetComponent<MeshRenderer>().sharedMaterial = LitMaterial(tint);

            // PatrolEnemy.Awake caches the CC + child renderer and reads origin
            // from transform.position, so position & components must exist first.
            var enemy = go.AddComponent<PatrolEnemy>();
            enemy.Configure(data);
        }

        // ── 3. Save statue ──────────────────────────────────────────────────────
        private void BuildStatue()
        {
            var go = new GameObject("SaveStatue");
            go.transform.position = new Vector3(
                VerticalSliceContent.ColX(33),
                VerticalSliceContent.SurfaceY(VerticalSliceContent.PLAT_ROW) + 0.05f, 0f);

            var vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vis.transform.SetParent(go.transform, false);
            vis.transform.localScale    = new Vector3(0.6f, 1.4f, 0.6f);
            vis.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            var col = vis.GetComponent<Collider>();
            if (col) Destroy(col);
            vis.GetComponent<MeshRenderer>().sharedMaterial = LitMaterial(new Color(0.2f, 0.35f, 0.8f));

            var light = go.AddComponent<Light>();
            light.type = LightType.Point; light.range = 6f; light.intensity = 2f;
            light.color = new Color(0.3f, 0.5f, 1f);

            go.AddComponent<SaveStatue>();
        }

        // ── 4. Ladder ───────────────────────────────────────────────────────────
        private void BuildLadder()
        {
            float yBottom = VerticalSliceContent.SurfaceY(VerticalSliceContent.FLOOR_TOP_ROW);
            float yTop    = VerticalSliceContent.SurfaceY(VerticalSliceContent.PLAT_ROW);
            float x       = VerticalSliceContent.ColX(VerticalSliceContent.LADDER_COL);

            var go = new GameObject("Ladder");
            go.transform.position = new Vector3(x, (yBottom + yTop) * 0.5f, 0f);

            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(1f, yTop - yBottom, 1f);

            go.AddComponent<ClimbableVolume>();
        }

        // ── 5/6. Announce the room ──────────────────────────────────────────────
        private IEnumerator KickRoomEntered()
        {
            yield return null; // let every Awake/OnEnable subscribe first
            EventBus.Emit(new RoomTransitionCompleteEvent
            {
                RoomId      = "vs_entrance_threshold",
                DisplayName = "Entrance Hall — Threshold",
                Zone        = ZoneType.EntranceHall,
            });
        }

        // ── Material helpers ────────────────────────────────────────────────────
        private Material TileMaterial()
        {
            if (_tileMaterial != null) return _tileMaterial;
            var m = LitMaterial(new Color(0.4f, 0.38f, 0.45f));
            if (_tileAtlas != null && m.HasProperty("_BaseColorMap"))
                m.SetTexture("_BaseColorMap", _tileAtlas);
            return m;
        }

        private static Material LitMaterial(Color c)
        {
            Shader sh = Shader.Find("HDRP/Lit")
                     ?? Shader.Find("Universal Render Pipeline/Lit")
                     ?? Shader.Find("Standard")
                     ?? Shader.Find("Sprites/Default");
            var m = new Material(sh);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (m.HasProperty("_Color"))     m.SetColor("_Color", c);
            return m;
        }
    }
}
