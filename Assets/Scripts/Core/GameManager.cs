using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MidnightReturn.Utils;
using MidnightReturn.Player;

namespace MidnightReturn.Core
{
    public enum GamePhase { Boot, Menu, Playing, Paused, Dead, Cutscene, BossFight }

    [System.Serializable]
    public class SaveData
    {
        public int    Slot;
        public string PlayerName  = "Alucard";
        public StatBlock Stats    = new();
        public Dictionary<string, int>  Inventory      = new();
        public HashSet<string>          MapExplored    = new();
        public HashSet<string>          DefeatedBosses = new();
        public string CurrentRoom = "entrance_hall";
        public float  Playtime    = 0f;

        // Equipped item IDs
        public string RightHandId, LeftHandId, HelmetId, BodyId, CloakId, BootsId;
        public string Accessory1Id, Accessory2Id;
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GamePhase Phase { get; private set; } = GamePhase.Boot;
        public bool      IsPaused => Phase == GamePhase.Paused;
        public SaveData  Save     { get; private set; } = new SaveData();

        [SerializeField] private string _menuSceneName  = "MainMenu";
        [SerializeField] private string _gameSceneName  = "CastleEntrance";

        private float _playtimeAccumulator;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Subscribe<BossStartedEvent>(_ => SetPhase(GamePhase.BossFight));
            EventBus.Subscribe<BossDefeatedEvent>(_ => SetPhase(GamePhase.Playing));
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        private void Update()
        {
            if (Phase == GamePhase.Playing || Phase == GamePhase.BossFight)
                Save.Playtime += Time.deltaTime;
        }

        public void StartNewGame()
        {
            Save = new SaveData();
            SetPhase(GamePhase.Playing);
            SceneManager.LoadScene(_gameSceneName);
        }

        public void LoadGame(int slot)
        {
            var json = PlayerPrefs.GetString($"save_{slot}");
            if (!string.IsNullOrEmpty(json))
                Save = JsonUtility.FromJson<SaveData>(json);
            SetPhase(GamePhase.Playing);
            SceneManager.LoadScene(_gameSceneName);
        }

        public void SaveGame(int slot)
        {
            PlayerPrefs.SetString($"save_{slot}", JsonUtility.ToJson(Save));
            PlayerPrefs.Save();
        }

        public void TogglePause()
        {
            if (Phase == GamePhase.Playing || Phase == GamePhase.BossFight)
            {
                SetPhase(GamePhase.Paused);
                Time.timeScale = 0f;
            }
            else if (Phase == GamePhase.Paused)
            {
                SetPhase(GamePhase.Playing);
                Time.timeScale = 1f;
            }
        }

        public void SetPhase(GamePhase phase) => Phase = phase;

        public void AddToInventory(string itemId, int qty = 1)
        {
            Save.Inventory.TryGetValue(itemId, out int current);
            Save.Inventory[itemId] = current + qty;
            EventBus.Emit(new ItemPickedUpEvent { ItemId = itemId, Quantity = qty });
        }

        public void MarkRoomVisited(string roomId)
        {
            Save.MapExplored.Add(roomId);
            EventBus.Emit(new RoomEnteredEvent { RoomId = roomId });
        }

        public void DefeatBoss(string bossId)
        {
            if (Save.DefeatedBosses.Add(bossId))
                EventBus.Emit(new BossDefeatedEvent { BossId = bossId });
        }

        private void OnPlayerDied(PlayerDiedEvent _)
        {
            SetPhase(GamePhase.Dead);
            Invoke(nameof(ReturnToMenu), 3f);
        }

        private void ReturnToMenu()
        {
            Time.timeScale = 1f;
            SetPhase(GamePhase.Menu);
            SceneManager.LoadScene(_menuSceneName);
        }
    }
}
