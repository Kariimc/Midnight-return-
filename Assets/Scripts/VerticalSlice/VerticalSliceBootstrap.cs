using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidnightReturn.Data;
using MidnightReturn.Level;
using MidnightReturn.Map;
using MidnightReturn.Player;
using MidnightReturn.Enemies.Types;
using MidnightReturn.Systems;
using MidnightReturn.Utils;

namespace MidnightReturn.VerticalSlice
{
    // ══════════════════════════════════════════════════════════════════════════
    //  VerticalSliceBootstrap — assembles two playable rooms in code.
    //
    //  Room 1: Entrance Hall — Threshold
    //    • 44×16 stone corridor, ceiling, walls, pit, ledge, platform, ladder
    //    • Zombie on ledge + Skeleton on floor
    //    • Kill both → exit door unlocks (ProceduralRoomTransition)
    //    • Exit → 0.3s fade to black → Room 2 swap → fade in + room banner
    //
    //  Room 2: Catacombs — Shattered Hall
    //    • Open eerie cavern, same 44×16 grid
    //    • Three cascading platforms over a massive central void
    //    • Two placeholder enemies (Shade, Wraith) — roster TBD
    //
    //  RoomTransitionCompleteEvent is emitted for EACH room; MusicDirector and
    //  ZoneLightingController (if present) react with zero direct wiring.
    // ══════════════════════════════════════════════════════════════════════════
    public sealed class VerticalSliceBootstrap : MonoBehaviour
    {
        [Header("Tile Textures — Stone floor/platforms (optional, PBR HDRP/Lit)")]
        [Tooltip("BaseColor atlas for stone floor tiles. Polyhaven: cobblestone_floor_08_diff_2k.")]
        [SerializeField] private Texture2D _stoneBaseColor;
        [Tooltip("Normal map (GL convention). Polyhaven: cobblestone_floor_08_nor_gl_2k.")]
        [SerializeField] private Texture2D _stoneNormalMap;
        [Tooltip("HDRP MaskMap (R=Metallic G=AO B=Detail A=Smoothness).")]
        [SerializeField] private Texture2D _stoneMaskMap;

        [Header("Tile Textures — Brick walls/ceiling (optional)")]
        [SerializeField] private Texture2D _brickBaseColor;
        [SerializeField] private Texture2D _brickNormalMap;
        [SerializeField] private Texture2D _brickMaskMap;

        [Header("Tile Rendering (legacy — overrides PBR if set)")]
        [Tooltip("8×8 tile atlas. If set, overrides the individual PBR texture slots.")]
        [SerializeField] private Texture2D _tileAtlas;
        [Tooltip("Full material override. Takes priority over all texture slots.")]
        [SerializeField] private Material  _tileMaterial;

        [Header("Enemies")]
        [Tooltip("Layer index for spawned enemies — MUST match PlayerCombat's enemy LayerMask.")]
        [SerializeField] private int _enemyLayer = 0;

        [Header("Boot")]
        [Tooltip("Reposition the tagged Player to the room entrance on boot.")]
        [SerializeField] private bool _movePlayerToSpawn = true;

        // ── Room 1 state ─────────────────────────────────────────────────────
        private readonly List<GameObject>  _room1Objects  = new();
        private readonly HashSet<string>   _room1EnemyIds = new();
        private          int               _room1EnemiesAlive;
        private          ProceduralRoomTransition _exitDoor;

        private Vector3 Room1SpawnPoint => new(
            VerticalSliceContent.ColX(3),
            VerticalSliceContent.SurfaceY(VerticalSliceContent.FLOOR_TOP_ROW) + 0.2f,
            0f);

        private Vector3 Room2SpawnPoint => new(
            CatacombsContent.ColX(CatacombsContent.SPAWN_COL),
            CatacombsContent.SurfaceY(CatacombsContent.HIGH_PLAT_ROW) + 0.2f,
            0f);

        private Vector3 Room3SpawnPoint => new(
            ClocktowerContent.ColX(ClocktowerContent.SPAWN_COL),
            ClocktowerContent.SurfaceY(ClocktowerContent.FLOOR_TOP_ROW) + 0.2f,
            0f);

        // ── Lifecycle ─────────────────────────────────────────────────────────
        private void Start()
        {
            transform.position = Vector3.zero;
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);

            BuildRoom1();

            if (_movePlayerToSpawn)
                RepositionPlayer(Room1SpawnPoint);

            RenderSettings.ambientLight = new Color(0.06f, 0.05f, 0.09f);
            StartCoroutine(KickRoomEvent("vs_entrance_threshold",
                                          "Entrance Hall — Threshold",
                                          ZoneType.EntranceHall));
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Room 1 — Entrance Hall
        // ══════════════════════════════════════════════════════════════════════
        private void BuildRoom1()
        {
            BuildTilemap(VerticalSliceContent.BuildMainLayer(),
                         VerticalSliceContent.BuildBackgroundLayer(),
                         "Room1");

            SpawnPatrol(VerticalSliceContent.BuildSkeleton(),
                new Vector3(VerticalSliceContent.ColX(15),
                            VerticalSliceContent.SurfaceY(VerticalSliceContent.FLOOR_TOP_ROW) + 0.1f, 0f),
                new Color(0.85f, 0.85f, 0.7f));

            SpawnPatrol(VerticalSliceContent.BuildZombie(),
                new Vector3(VerticalSliceContent.ColX(9),
                            VerticalSliceContent.SurfaceY(VerticalSliceContent.LEDGE_ROW) + 0.1f, 0f),
                new Color(0.3f, 0.7f, 0.25f));

            BuildStatue(
                new Vector3(VerticalSliceContent.ColX(33),
                            VerticalSliceContent.SurfaceY(VerticalSliceContent.PLAT_ROW) + 0.05f, 0f));

            // Wall-mounted torch sconces — two orange-warm point lights with organic flicker.
            // These cast real-time HDRP shadows on tiles and characters (contact shadows on).
            BuildTorchLight(
                new Vector3(VerticalSliceContent.ColX(1),
                            VerticalSliceContent.SurfaceY(6) - 0.5f, -0.2f),
                TorchLightController.FlickerMode.Torch,
                new Color(1.00f, 0.62f, 0.22f), new Color(1.00f, 0.36f, 0.08f),
                intensityLux: 950f, range: 9f);

            BuildTorchLight(
                new Vector3(VerticalSliceContent.ColX(1),
                            VerticalSliceContent.SurfaceY(12) - 0.5f, -0.2f),
                TorchLightController.FlickerMode.Torch,
                new Color(1.00f, 0.62f, 0.22f), new Color(1.00f, 0.36f, 0.08f),
                intensityLux: 950f, range: 9f);

            BuildLadder(VerticalSliceContent.ColX(VerticalSliceContent.LADDER_COL),
                        VerticalSliceContent.SurfaceY(VerticalSliceContent.FLOOR_TOP_ROW),
                        VerticalSliceContent.SurfaceY(VerticalSliceContent.PLAT_ROW));

            // Door starts locked — unlocked when all Room 1 enemies die.
            BuildExitDoor("ExitDoor_Room1",
                new Vector3((VerticalSliceContent.W - 1) * VerticalSliceContent.TILE - 0.15f,
                             VerticalSliceContent.SurfaceY(VerticalSliceContent.FLOOR_TOP_ROW) * 0.5f, 0f),
                VerticalSliceContent.SurfaceY(VerticalSliceContent.FLOOR_TOP_ROW),
                SwitchToRoom2, startUnlocked: false);

            // Ability gate shortcut: Air Dash barrier between ledge and hidden platform.
            // Player on ledge → air-dashes past spectral barrier → discovers Double Jump.
            BuildAbilityGate(
                new Vector3(
                    VerticalSliceContent.ColX(14),
                    VerticalSliceContent.SurfaceY(VerticalSliceContent.LEDGE_ROW) + 1.5f,
                    0f),
                GateType.Spectral,
                RequiredAbility.AirDash,
                new Vector3(0.3f, 4f, 1f));

            // Hidden floating platform inside the pit area (same elevation as the ledge)
            BuildHiddenPlatform(
                new Vector3(
                    VerticalSliceContent.ColX(20),
                    VerticalSliceContent.SurfaceY(VerticalSliceContent.LEDGE_ROW),
                    0f),
                width: 3f);

            // Double Jump pickup on the hidden platform
            BuildAbilityPickup(
                new Vector3(
                    VerticalSliceContent.ColX(21),
                    VerticalSliceContent.SurfaceY(VerticalSliceContent.LEDGE_ROW) + 0.8f,
                    0f),
                "Double Jump",
                "Press Jump again while airborne to leap a second time.",
                pm => pm.CanDoubleJump = true,
                new Color(0.3f, 0.7f, 1f));

            // INT Fragment on col 22 of the hidden platform — backtrack reward
            BuildStatPickup(
                new Vector3(
                    VerticalSliceContent.ColX(22),
                    VerticalSliceContent.SurfaceY(VerticalSliceContent.LEDGE_ROW) + 0.5f,
                    0f),
                "int_fragment",
                pc => { pc.Stats.Int += 3; pc.Stats.MaxMp += 15; pc.Stats.Mp = pc.Stats.MaxMp; });

            // Soul Tether pickup — on the hidden platform, col 24 (extended area)
            BuildAbilityPickup(
                new Vector3(
                    VerticalSliceContent.ColX(24),
                    VerticalSliceContent.SurfaceY(VerticalSliceContent.LEDGE_ROW) + 0.8f,
                    0f),
                "Soul Tether",
                "While airborne, press Up+Spell to latch a spectral chain onto the ceiling and get yanked up.",
                pm => pm.CanSoulTether = true,
                new Color(0.5f, 0.1f, 1f));

            // TetherAnchor: ceiling hook directly above the hidden platform (Room 1)
            BuildTetherAnchor(new Vector3(
                VerticalSliceContent.ColX(21),
                VerticalSliceContent.SurfaceY(1),
                0f));

            // TetherAnchor: near the entrance arch
            BuildTetherAnchor(new Vector3(
                VerticalSliceContent.ColX(5),
                VerticalSliceContent.SurfaceY(2),
                0f));

            // PhaseableWall: thin wall at col 25 between the hidden platform and the
            // right platform — lets you Wraith Step through to reach the high platform.
            BuildPhaseableWall(
                new Vector3(
                    VerticalSliceContent.ColX(25),
                    VerticalSliceContent.SurfaceY(VerticalSliceContent.LEDGE_ROW) + 1f,
                    0f),
                new Vector3(0.3f, 3f, 1f));

            // Wraith Step pickup — on the high-right platform, reward for phasing through
            BuildAbilityPickup(
                new Vector3(
                    VerticalSliceContent.ColX(30),
                    VerticalSliceContent.SurfaceY(VerticalSliceContent.PLAT_ROW) + 0.8f,
                    0f),
                "Wraith Step",
                "Dash into a shimmering wall to phase through it as a ghost.",
                pm => pm.CanWraithStep = true,
                new Color(0.3f, 0.8f, 1f));
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Room 2 — Catacombs: Shattered Hall
        // ══════════════════════════════════════════════════════════════════════
        private void SwitchToRoom2()
        {
            // Tear down Room 1
            foreach (var go in _room1Objects)
                if (go) Destroy(go);
            _room1Objects.Clear();
            _room1EnemyIds.Clear();

            // Build Room 2
            BuildTilemap(CatacombsContent.BuildMainLayer(),
                         CatacombsContent.BuildBackgroundLayer(),
                         "Room2");

            SpawnPatrol(CatacombsContent.BuildShade(),
                new Vector3(CatacombsContent.ColX(5),
                            CatacombsContent.SurfaceY(CatacombsContent.HIGH_PLAT_ROW) + 0.1f, 0f),
                new Color(0.5f, 0.5f, 0.65f));

            SpawnPatrol(CatacombsContent.BuildWraith(),
                new Vector3(CatacombsContent.ColX(35),
                            CatacombsContent.SurfaceY(CatacombsContent.FLOOR_TOP_ROW) + 0.1f, 0f),
                new Color(0.65f, 0.3f, 0.8f));

            BuildBossTrigger();

            // Cracked wall gate: requires Double Jump to reach the secret ledge above
            BuildAbilityGate(
                new Vector3(
                    CatacombsContent.ColX(13),
                    CatacombsContent.SurfaceY(3) + 1f,
                    0f),
                GateType.CrackedWall,
                RequiredAbility.DoubleJump,
                new Vector3(0.3f, 3.5f, 1f));

            // Secret ledge above the cracked wall (cols 14-17, row 3)
            BuildHiddenPlatform(
                new Vector3(
                    CatacombsContent.ColX(14),
                    CatacombsContent.SurfaceY(3),
                    0f),
                width: 4f);

            // Lore item on the secret ledge — just an ItemPickedUpEvent placeholder
            BuildLorePickup(
                new Vector3(
                    CatacombsContent.ColX(16),
                    CatacombsContent.SurfaceY(3) + 0.5f,
                    0f));

            // TetherAnchors in Room 2 (ceiling hooks above platforms)
            BuildTetherAnchor(new Vector3(CatacombsContent.ColX(18), CatacombsContent.SurfaceY(1), 0f));
            BuildTetherAnchor(new Vector3(CatacombsContent.ColX(39), CatacombsContent.SurfaceY(1), 0f));

            // PhaseableWall between LOW platform right edge and right floor section
            BuildPhaseableWall(
                new Vector3(CatacombsContent.ColX(25),
                            CatacombsContent.SurfaceY(CatacombsContent.LOW_PLAT_ROW) + 1f,
                            0f),
                new Vector3(0.3f, 2.5f, 1f));

            // Iron Gate at the far-right exit: blocks the Clocktower passage.
            // Requires Double Jump — player must backtrack to Room 1 after Air Dash.
            BuildAbilityGate(
                new Vector3(
                    CatacombsContent.ColX(41),
                    CatacombsContent.SurfaceY(CatacombsContent.FLOOR_TOP_ROW) * 0.5f + 0.5f,
                    0f),
                GateType.IronGate,
                RequiredAbility.DoubleJump,
                new Vector3(0.3f, CatacombsContent.SurfaceY(CatacombsContent.FLOOR_TOP_ROW) - 0.5f, 1f));

            // Exit door BEHIND the iron gate (col 42) → Clocktower. Starts unlocked;
            // the iron gate is the real barrier, so the player can only reach this
            // door once Double Jump has dissolved the gate.
            BuildExitDoor("ExitDoor_Room2",
                new Vector3((CatacombsContent.W - 1) * CatacombsContent.TILE - 0.15f,
                             CatacombsContent.SurfaceY(CatacombsContent.FLOOR_TOP_ROW) * 0.5f, 0f),
                CatacombsContent.SurfaceY(CatacombsContent.FLOOR_TOP_ROW),
                SwitchToRoom3, startUnlocked: true);

            RepositionPlayer(Room2SpawnPoint);

            RenderSettings.ambientLight = new Color(0.04f, 0.03f, 0.07f); // darker
            StartCoroutine(KickRoomEvent("catacombs_shattered_hall",
                                          "Catacombs — Shattered Hall",
                                          ZoneType.Catacombs));
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Room 3 — Clocktower: Gearworks Ascent
        // ══════════════════════════════════════════════════════════════════════
        private void SwitchToRoom3()
        {
            // Tear down Room 2
            foreach (var go in _room1Objects)
                if (go) Destroy(go);
            _room1Objects.Clear();
            _room1EnemyIds.Clear();
            _room1EnemiesAlive = 0;

            // Build Room 3
            BuildTilemap(ClocktowerContent.BuildMainLayer(),
                         ClocktowerContent.BuildBackgroundLayer(),
                         "Room3");

            // Gear Golem — heavy patroller on platform B
            SpawnPatrol(ClocktowerContent.BuildGearGolem(),
                new Vector3(ClocktowerContent.ColX(17),
                            ClocktowerContent.SurfaceY(ClocktowerContent.PLAT_B_ROW) + 0.1f, 0f),
                new Color(0.8f, 0.5f, 0.15f));

            // Vampire Bat — flyer haunting the central shaft
            SpawnFlying(ClocktowerContent.BuildVampireBat(),
                new Vector3(ClocktowerContent.ColX(ClocktowerContent.PENDULUM_COL),
                            ClocktowerContent.SurfaceY(ClocktowerContent.PLAT_C_ROW) + 2f, 0f),
                new Color(0.45f, 0.05f, 0.08f));

            // Boss trigger near the top-right platform entrance
            BuildClockworkBossTrigger();

            RepositionPlayer(Room3SpawnPoint);

            RenderSettings.ambientLight = new Color(0.06f, 0.06f, 0.09f); // cold mechanical
            StartCoroutine(KickRoomEvent("clocktower_gearworks_ascent",
                                          "Clocktower — Gearworks Ascent",
                                          ZoneType.Clocktower));
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Kill tracking — unlocks exit when all Room 1 enemies die
        // ══════════════════════════════════════════════════════════════════════
        private void OnEnemyDied(EnemyDiedEvent e)
        {
            if (!_room1EnemyIds.Remove(e.EnemyId)) return;
            _room1EnemiesAlive = Mathf.Max(0, _room1EnemiesAlive - 1);

            if (_room1EnemiesAlive == 0 && _exitDoor != null)
                _exitDoor.Unlock();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Shared builders
        // ══════════════════════════════════════════════════════════════════════
        private void BuildTilemap(TilemapLayerSO main, TilemapLayerSO bg, string prefix)
        {
            var mainSet = VerticalSliceContent.BuildTileSet(_tileAtlas, TileMaterial());
            var bgSet   = VerticalSliceContent.BuildTileSet(
                              _brickBaseColor != null ? _brickBaseColor : _tileAtlas,
                              BrickMaterial());

            var mainGO = BakeLayer($"{prefix}_TileLayer_Main", main, mainSet);
            var bgGO   = BakeLayer($"{prefix}_TileLayer_BG",   bg,   bgSet);

            _room1Objects.Add(mainGO);
            _room1Objects.Add(bgGO);
        }

        private GameObject BakeLayer(string name, TilemapLayerSO layer, TileSetSO set)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var builder = go.AddComponent<TileChunkBuilder>();
            builder.Configure(set, layer);
            builder.Build();
            return go;
        }

        private void SpawnPatrol(EnemyDataSO data, Vector3 pos, Color tint)
        {
            var go = new GameObject("Enemy_" + data.EnemyId) { layer = _enemyLayer };
            go.transform.position = pos;
            _room1Objects.Add(go);

            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.6f; cc.radius = 0.4f; cc.center = new Vector3(0f, 0.8f, 0f);

            var vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vis.name = "Visual";
            vis.layer = _enemyLayer;
            vis.transform.SetParent(go.transform, false);
            vis.transform.localScale    = new Vector3(0.8f, 1.6f, 0.4f);
            vis.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            var primCol = vis.GetComponent<Collider>();
            if (primCol) Destroy(primCol);
            vis.GetComponent<MeshRenderer>().sharedMaterial = LitMaterial(tint);

            var enemy = go.AddComponent<PatrolEnemy>();
            enemy.Configure(data);

            // Register for kill tracking (Room 1 and Room 2 enemies share the same tracker;
            // Room 2 enemies don't gate anything so the unlocked door is a no-op).
            _room1EnemyIds.Add(data.EnemyId);
            _room1EnemiesAlive++;
        }

        private void SpawnFlying(EnemyDataSO data, Vector3 pos, Color tint)
        {
            var go = new GameObject("Enemy_" + data.EnemyId) { layer = _enemyLayer };
            go.transform.position = pos;
            _room1Objects.Add(go);

            // Flyers still need a CharacterController for EnemyBase, but ignore gravity
            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.0f; cc.radius = 0.45f; cc.center = new Vector3(0f, 0.5f, 0f);

            var vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vis.name = "Visual";
            vis.layer = _enemyLayer;
            vis.transform.SetParent(go.transform, false);
            vis.transform.localScale    = new Vector3(0.7f, 0.5f, 0.4f);
            vis.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            var primCol = vis.GetComponent<Collider>();
            if (primCol) Destroy(primCol);
            vis.GetComponent<MeshRenderer>().sharedMaterial = LitMaterial(tint);

            var enemy = go.AddComponent<FlyingEnemy>();
            enemy.Configure(data);

            _room1EnemyIds.Add(data.EnemyId);
            _room1EnemiesAlive++;
        }

        private void BuildStatue(Vector3 pos)
        {
            var go = new GameObject("SaveStatue");
            go.transform.position = pos;
            _room1Objects.Add(go);

            var vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vis.transform.SetParent(go.transform, false);
            vis.transform.localScale    = new Vector3(0.6f, 1.4f, 0.6f);
            vis.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            var col = vis.GetComponent<Collider>();
            if (col) Destroy(col);
            vis.GetComponent<MeshRenderer>().sharedMaterial = LitMaterial(new Color(0.2f, 0.35f, 0.8f));

            // Crystal pulse light — blue-white, slow shimmer (activated: warm gold).
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            TorchLightController.Configure(
                go,
                TorchLightController.FlickerMode.Crystal,
                new Color(0.28f, 0.52f, 1.00f),  // primary: cool sapphire
                new Color(0.80f, 0.90f, 1.00f),  // hot: white-shimmer
                intensityLux: 600f,
                range: 6.5f);

            go.AddComponent<SaveStatue>();
        }

        private void BuildTorchLight(
            Vector3 pos,
            TorchLightController.FlickerMode mode,
            Color primary, Color hot,
            float intensityLux, float range)
        {
            var go = new GameObject("TorchLight");
            go.transform.position = pos;
            _room1Objects.Add(go);

            go.AddComponent<Light>().type = LightType.Point;
            TorchLightController.Configure(go, mode, primary, hot, intensityLux, range);
        }

        private void BuildLadder(float x, float yBottom, float yTop)
        {
            var go = new GameObject("Ladder");
            go.transform.position = new Vector3(x, (yBottom + yTop) * 0.5f, 0f);
            _room1Objects.Add(go);

            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(1f, yTop - yBottom, 1f);

            go.AddComponent<ClimbableVolume>();
        }

        private void BuildAbilityGate(Vector3 pos, GateType gType, RequiredAbility req, Vector3 size)
        {
            var go = new GameObject($"AbilityGate_{gType}");
            go.transform.position = pos;
            _room1Objects.Add(go);
            go.AddComponent<BoxCollider>(); // AbilityGate.Start() configures this
            var gate = go.AddComponent<AbilityGate>();
            gate.Configure(gType, req, size);
        }

        private void BuildHiddenPlatform(Vector3 leftEdgePos, float width)
        {
            var go = new GameObject("HiddenPlatform");
            go.transform.position = leftEdgePos;
            _room1Objects.Add(go);

            var box = go.AddComponent<BoxCollider>();
            box.size   = new Vector3(width, 0.2f, 1.2f);
            box.center = new Vector3(width * 0.5f, 0f, 0f);

            var vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vis.name = "Visual";
            vis.transform.SetParent(go.transform, false);
            vis.transform.localScale    = new Vector3(width, 0.22f, 0.9f);
            vis.transform.localPosition = new Vector3(width * 0.5f, 0f, 0f);
            if (vis.TryGetComponent<Collider>(out var c)) Destroy(c);
            vis.GetComponent<MeshRenderer>().sharedMaterial = LitMaterial(new Color(0.38f, 0.35f, 0.44f));
        }

        private void BuildAbilityPickup(
            Vector3 pos, string name, string desc,
            System.Action<PlayerMovement> grant, Color color)
        {
            var go = new GameObject("AbilityPickup_" + name);
            go.transform.position = pos;
            _room1Objects.Add(go);
            go.tag = "Untagged";
            go.AddComponent<SphereCollider>(); // AbilityPickup.Init() configures this
            var pickup = go.AddComponent<AbilityPickup>();
            pickup.Init(name, desc, grant, color);
        }

        private void BuildLorePickup(Vector3 pos)
        {
            // Placeholder: a glowing cube that emits ItemPickedUpEvent when touched
            var go = new GameObject("LorePickup");
            go.transform.position = pos;
            _room1Objects.Add(go);

            var vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vis.name = "Visual";
            vis.transform.SetParent(go.transform, false);
            vis.transform.localScale = Vector3.one * 0.35f;
            var mat = new Material(Shader.Find("HDRP/Lit") ?? Shader.Find("Standard"));
            if (mat.HasProperty("_BaseColor"))     mat.SetColor("_BaseColor",     new Color(0.9f, 0.7f, 0.2f));
            if (mat.HasProperty("_EmissiveColor")) mat.SetColor("_EmissiveColor", new Color(0.7f, 0.5f, 0.1f));
            vis.GetComponent<MeshRenderer>().sharedMaterial = mat;
            if (vis.TryGetComponent<Collider>(out var vc)) { vc.isTrigger = true; }

            go.AddComponent<LorePickupTrigger>();
        }

        private void BuildStatPickup(Vector3 pos, string itemId, System.Action<PlayerController> apply)
        {
            var go = new GameObject("StatPickup_" + itemId);
            go.transform.position = pos;
            _room1Objects.Add(go);

            // Gold gem visual
            var vis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            vis.name = "Visual";
            vis.transform.SetParent(go.transform, false);
            vis.transform.localScale = Vector3.one * 0.3f;
            var gold = new Color(0.9f, 0.7f, 0.1f);
            var mat  = new Material(Shader.Find("HDRP/Lit") ?? Shader.Find("Standard"));
            if (mat.HasProperty("_BaseColor"))     mat.SetColor("_BaseColor",     gold);
            if (mat.HasProperty("_EmissiveColor")) mat.SetColor("_EmissiveColor", gold * 1.8f);
            vis.GetComponent<MeshRenderer>().sharedMaterial = mat;
            if (vis.TryGetComponent<Collider>(out var vc)) Destroy(vc);

            var light = go.AddComponent<Light>();
            light.type = LightType.Point; light.range = 4f; light.intensity = 1.5f;
            light.color = gold;

            var sphere = go.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius    = 0.5f;

            var trigger = go.AddComponent<StatPickupTrigger>();
            trigger.DisplayText = itemId;
            trigger.Apply       = apply;
        }

        private void BuildClockworkBossTrigger()
        {
            float topFloorY = ClocktowerContent.SurfaceY(ClocktowerContent.PLAT_TOP_ROW);
            var go = new GameObject("BossTrigger_Room3");
            go.transform.position = new Vector3(
                ClocktowerContent.ColX(32),
                topFloorY * 0.5f,
                0f);
            _room1Objects.Add(go);

            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(4f, topFloorY, 1f);

            var trigger = go.AddComponent<BossTrigger>();
            trigger.OnPlayerEntered = _ => SpawnClockworkSentinel();
        }

        private void SpawnClockworkSentinel()
        {
            var data = ScriptableObject.CreateInstance<Data.EnemyDataSO>();
            data.EnemyId        = "clockwork_sentinel";
            data.DisplayName    = "Clockwork Sentinel";
            data.Zone           = Data.EnemyZone.Clocktower;
            data.MaxHp          = 250;
            data.Attack         = 22;
            data.Defense        = 8;
            data.ExpReward      = 450;
            data.Behavior       = Data.AIBehavior.Boss;
            data.MoveSpeed      = 3.5f;
            data.DetectionRange = 50f;
            data.AttackRange    = 2.0f;
            data.AggroRange     = 50f;
            data.AttackCooldown = 2.4f;
            data.AttackWindup   = 0.38f;
            data.DeathColor     = new Color(1f, 0.6f, 0.1f);

            float topY = ClocktowerContent.SurfaceY(ClocktowerContent.PLAT_TOP_ROW);
            var pos = new Vector3(
                ClocktowerContent.ColX(ClocktowerContent.PLAT_TOP_X1 - 3),
                topY + 0.1f,
                0f);

            var go = new GameObject("Boss_ClockworkSentinel") { layer = _enemyLayer };
            go.transform.position = pos;
            _room1Objects.Add(go);

            var cc = go.AddComponent<CharacterController>();
            cc.height = 2.0f; cc.radius = 0.5f; cc.center = new Vector3(0f, 1.0f, 0f);

            var vis = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            vis.name  = "Visual";
            vis.layer = _enemyLayer;
            vis.transform.SetParent(go.transform, false);
            vis.transform.localScale    = new Vector3(0.95f, 1.2f, 0.5f);
            vis.transform.localPosition = new Vector3(0f, 1.0f, 0f);
            if (vis.TryGetComponent<Collider>(out var vc)) Destroy(vc);
            vis.GetComponent<MeshRenderer>().sharedMaterial = LitMaterial(new Color(0.65f, 0.45f, 0.15f));

            var boss = go.AddComponent<ClockworkSentinelBoss>();
            boss.Configure(data);
        }

        private void BuildBossTrigger()
        {
            // Placed at column 37 on the right floor — fires once when player crosses
            var go = new GameObject("BossTrigger_Room2");
            float floorY  = CatacombsContent.SurfaceY(CatacombsContent.FLOOR_TOP_ROW);
            go.transform.position = new Vector3(
                CatacombsContent.ColX(37),
                floorY * 0.5f,
                0f);
            _room1Objects.Add(go);

            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(4f, floorY, 1f);

            var trigger = go.AddComponent<BossTrigger>();
            trigger.OnPlayerEntered = _ => SpawnDeathBoss();
        }

        private void SpawnDeathBoss()
        {
            var data = ScriptableObject.CreateInstance<Data.EnemyDataSO>();
            data.EnemyId        = "death";
            data.DisplayName    = "Death";
            data.Zone           = Data.EnemyZone.Catacombs;
            data.MaxHp          = 300;
            data.Attack         = 25;
            data.Defense        = 5;
            data.ExpReward      = 500;
            data.Behavior       = Data.AIBehavior.Boss;
            data.MoveSpeed      = 4f;
            data.DetectionRange = 50f;
            data.AttackRange    = 1.8f;
            data.AggroRange     = 50f;
            data.AttackCooldown = 2.2f;
            data.AttackWindup   = 0.35f;
            data.DeathColor     = new Color(0.5f, 0f, 0.8f);

            float floorY = CatacombsContent.SurfaceY(CatacombsContent.FLOOR_TOP_ROW);
            var pos = new Vector3(CatacombsContent.ColX(39), floorY + 0.1f, 0f);

            var go = new GameObject("Boss_Death") { layer = _enemyLayer };
            go.transform.position = pos;
            _room1Objects.Add(go);

            var cc = go.AddComponent<CharacterController>();
            cc.height = 2.2f; cc.radius = 0.5f; cc.center = new Vector3(0f, 1.1f, 0f);

            var vis = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            vis.name = "Visual";
            vis.layer = _enemyLayer;
            vis.transform.SetParent(go.transform, false);
            vis.transform.localScale    = new Vector3(0.9f, 1.3f, 0.5f);
            vis.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            if (vis.TryGetComponent<Collider>(out var vc)) Destroy(vc);
            vis.GetComponent<MeshRenderer>().sharedMaterial = LitMaterial(new Color(0.08f, 0f, 0.12f));

            var boss = go.AddComponent<DeathBoss>();
            boss.Configure(data);

            EventBus.Emit(new BossStartedEvent { BossId = "death" });
        }

        private void BuildTetherAnchor(Vector3 pos)
        {
            var go = new GameObject("TetherAnchor");
            go.transform.position = pos;
            _room1Objects.Add(go);

            var col = go.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius    = 0.4f;

            go.AddComponent<TetherAnchor>();

            // Faint glow light so players can spot anchors in dark environments
            var light = go.AddComponent<Light>();
            light.type      = LightType.Point;
            light.range     = 3f;
            light.intensity = 0.6f;
            light.color     = new Color(0.45f, 0.1f, 1f);
        }

        private void BuildPhaseableWall(Vector3 pos, Vector3 size)
        {
            var go = new GameObject("PhaseableWall");
            go.transform.position = pos;
            _room1Objects.Add(go);

            var box = go.AddComponent<BoxCollider>();
            box.size = size;

            var vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vis.name = "Visual";
            vis.transform.SetParent(go.transform, false);
            vis.transform.localScale = size;
            if (vis.TryGetComponent<Collider>(out var c)) Destroy(c);

            var sh  = Shader.Find("HDRP/Lit")
                   ?? Shader.Find("Universal Render Pipeline/Lit")
                   ?? Shader.Find("Standard");
            var mat = new Material(sh);
            if (mat.HasProperty("_BaseColor"))     mat.SetColor("_BaseColor",     new Color(0.3f, 0.05f, 0.7f, 0.4f));
            if (mat.HasProperty("_EmissiveColor")) mat.SetColor("_EmissiveColor", new Color(0.2f, 0.02f, 0.5f));
            mat.renderQueue = 3000;
            vis.GetComponent<MeshRenderer>().sharedMaterial = mat;

            go.AddComponent<PhaseableWall>();
        }

        private void BuildExitDoor(string name, Vector3 pos, float wallHeight,
                                   System.Action onMidpoint, bool startUnlocked)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            _room1Objects.Add(go);

            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(1.2f, wallHeight, 1f);

            _exitDoor = go.AddComponent<ProceduralRoomTransition>();
            _exitDoor.OnMidpoint = onMidpoint;
            if (startUnlocked) _exitDoor.Unlock();
        }

        // ── Room event emission ───────────────────────────────────────────────
        private IEnumerator KickRoomEvent(string roomId, string displayName, ZoneType zone)
        {
            yield return null; // let Awake/OnEnable subscriptions register first
            EventBus.Emit(new RoomTransitionCompleteEvent
            {
                RoomId      = roomId,
                DisplayName = displayName,
                Zone        = zone,
            });
        }

        private static void RepositionPlayer(Vector3 pos)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player) player.transform.position = pos;
        }

        // ── Material helpers ─────────────────────────────────────────────────
        private Material TileMaterial()
        {
            if (_tileMaterial != null) return _tileMaterial;

            if (_tileAtlas != null)
            {
                var m = LitMaterial(Color.white);
                if (m.HasProperty("_BaseColorMap")) m.SetTexture("_BaseColorMap", _tileAtlas);
                return m;
            }

            if (_stoneBaseColor != null || _stoneNormalMap != null || _stoneMaskMap != null)
                return HDRPTileMaterial.BuildStoneMaterial(_stoneBaseColor, _stoneNormalMap, _stoneMaskMap);

            return LitMaterial(new Color(0.4f, 0.38f, 0.45f));
        }

        private Material BrickMaterial()
        {
            if (_brickBaseColor != null || _brickNormalMap != null || _brickMaskMap != null)
                return HDRPTileMaterial.BuildBrickMaterial(_brickBaseColor, _brickNormalMap, _brickMaskMap);
            return LitMaterial(new Color(0.14f, 0.12f, 0.18f));
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

    // One-shot stat pickup — applies a permanent stat delta to PlayerController, then dissolves.
    internal sealed class StatPickupTrigger : MonoBehaviour
    {
        public string                       DisplayText;
        public System.Action<PlayerController> Apply;

        private bool _taken;
        private void OnTriggerEnter(Collider other)
        {
            if (_taken || !other.CompareTag("Player")) return;
            var pc = other.GetComponent<PlayerController>();
            if (pc == null) return;
            _taken = true;
            Apply?.Invoke(pc);
            EventBus.Emit(new ScreenFlashEvent { Color = new Color(0.9f, 0.7f, 0.1f, 0.45f), Duration = 0.3f });
            EventBus.Emit(new ItemPickedUpEvent { ItemId = "stat_int_fragment", Quantity = 1 });
            Destroy(gameObject);
        }
    }

    // One-shot lore pickup trigger — fires ItemPickedUpEvent then destroys itself.
    internal sealed class LorePickupTrigger : MonoBehaviour
    {
        private bool _taken;
        private void OnTriggerEnter(Collider other)
        {
            if (_taken || !other.CompareTag("Player")) return;
            _taken = true;
            EventBus.Emit(new ItemPickedUpEvent { ItemId = "lore_shadow_king", Quantity = 1 });
            Destroy(gameObject);
        }
    }
}
