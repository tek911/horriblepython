using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace VRBoatCombat
{
    /// <summary>
    /// Main game manager handling game state, score, difficulty, and events.
    /// Singleton pattern for easy access from other scripts.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // Singleton instance
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private GameState currentGameState = GameState.MainMenu;
        [SerializeField] private float gameTime = 0f;

        [Header("Player Stats")]
        [SerializeField] private int score = 0;
        [SerializeField] private int enemiesDestroyed = 0;
        [SerializeField] private int vesselsCaptured = 0;
        #pragma warning disable 0414 // Field assigned but never used (reserved for future implementation)
        [SerializeField] private float playerHealth = 100f;
        #pragma warning restore 0414

        [Header("Difficulty")]
        [SerializeField] private float difficultyMultiplier = 1f;
        [SerializeField] private float difficultyIncreaseRate = 0.1f;
        [SerializeField] private float maxDifficultyMultiplier = 3f;
        [SerializeField] private bool enableDynamicDifficulty = true;

        [Header("Capture Mechanics")]
        [SerializeField] private int maxSimultaneousCaptures = 1;
        [SerializeField] private float captureSpawnMultiplier = 2f;
        private List<GameObject> objectsBeingCaptured = new List<GameObject>();

        [Header("References")]
        [SerializeField] private Transform playerBoat;
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private UI.UIManager uiManager;

        [Header("Events")]
        public UnityEvent<GameState> OnGameStateChanged;
        public UnityEvent<int> OnScoreChanged;
        public UnityEvent<int> OnEnemyDestroyed;
        public UnityEvent<int> OnVesselCaptured;
        public UnityEvent OnGameOver;
        public UnityEvent OnGameWin;

        // Game state enum
        public enum GameState
        {
            MainMenu,
            Playing,
            Paused,
            GameOver,
            Victory
        }

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Find references if not assigned
            if (playerBoat == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerBoat = player.transform;
                }
            }

            if (enemySpawner == null)
            {
                enemySpawner = FindObjectOfType<EnemySpawner>();
            }

            if (uiManager == null)
            {
                uiManager = FindObjectOfType<UI.UIManager>();
            }
        }

        private void Start()
        {
            // Initialize game
            ChangeGameState(GameState.MainMenu);
        }

        private void Update()
        {
            if (currentGameState == GameState.Playing)
            {
                // Update game time
                gameTime += Time.deltaTime;

                // Update dynamic difficulty
                if (enableDynamicDifficulty)
                {
                    UpdateDifficulty();
                }

                // Check win/lose conditions
                CheckGameConditions();
            }
        }

        private void UpdateDifficulty()
        {
            // Gradually increase difficulty over time
            float timeMultiplier = 1f + (gameTime / 60f) * difficultyIncreaseRate;

            // Add multiplier based on player performance
            float performanceMultiplier = 1f + (vesselsCaptured * 0.1f);

            // Calculate final difficulty
            difficultyMultiplier = Mathf.Min(
                timeMultiplier * performanceMultiplier,
                maxDifficultyMultiplier
            );

            // Update enemy spawner difficulty
            if (enemySpawner != null)
            {
                enemySpawner.SetDifficultyMultiplier(difficultyMultiplier);
            }
        }

        private void CheckGameConditions()
        {
            // Check player health
            if (playerBoat != null)
            {
                Core.HealthSystem playerHealth = playerBoat.GetComponent<Core.HealthSystem>();
                if (playerHealth != null && playerHealth.IsDead())
                {
                    GameOver();
                }
            }

            // Check win condition (example: capture 10 vessels)
            if (vesselsCaptured >= 10)
            {
                GameWin();
            }
        }

        public void ChangeGameState(GameState newState)
        {
            if (currentGameState == newState) return;

            Debug.Log($"[GameManager] Game state changed: {currentGameState} → {newState}");

            currentGameState = newState;
            OnGameStateChanged?.Invoke(newState);

            // Handle state transitions
            switch (newState)
            {
                case GameState.Playing:
                    Time.timeScale = 1f;
                    if (enemySpawner != null)
                    {
                        enemySpawner.StartSpawning();
                    }
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;

                case GameState.GameOver:
                    Time.timeScale = 0f;
                    OnGameOver?.Invoke();
                    break;

                case GameState.Victory:
                    Time.timeScale = 0f;
                    OnGameWin?.Invoke();
                    break;
            }

            // Update UI
            if (uiManager != null)
            {
                uiManager.OnGameStateChanged(newState);
            }
        }

        public void StartGame()
        {
            // Reset game state
            score = 0;
            enemiesDestroyed = 0;
            vesselsCaptured = 0;
            gameTime = 0f;
            difficultyMultiplier = 1f;
            objectsBeingCaptured.Clear();

            ChangeGameState(GameState.Playing);

            Debug.Log("[GameManager] Game started");
        }

        public void PauseGame()
        {
            if (currentGameState == GameState.Playing)
            {
                ChangeGameState(GameState.Paused);
            }
        }

        public void ResumeGame()
        {
            if (currentGameState == GameState.Paused)
            {
                ChangeGameState(GameState.Playing);
            }
        }

        public void GameOver()
        {
            ChangeGameState(GameState.GameOver);
            Debug.Log($"[GameManager] Game Over! Final Score: {score}");
        }

        public void GameWin()
        {
            ChangeGameState(GameState.Victory);
            Debug.Log($"[GameManager] Victory! Final Score: {score}");
        }

        public void RestartGame()
        {
            // Reload scene or reset game
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }

        public void QuitGame()
        {
            Debug.Log("[GameManager] Quitting game");
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        // Score and stats methods
        public void AddScore(int points)
        {
            score += points;
            OnScoreChanged?.Invoke(score);

            if (uiManager != null)
            {
                uiManager.UpdateScore(score);
            }

            Debug.Log($"[GameManager] Score: {score} (+{points})");
        }

        public void OnEnemyDestroyedEvent(GameObject enemy)
        {
            enemiesDestroyed++;
            AddScore(100); // Base score for enemy destruction

            OnEnemyDestroyed?.Invoke(enemiesDestroyed);

            Debug.Log($"[GameManager] Enemy destroyed. Total: {enemiesDestroyed}");
        }

        public void OnCaptureStarted(GameObject vessel)
        {
            if (!objectsBeingCaptured.Contains(vessel))
            {
                objectsBeingCaptured.Add(vessel);

                // Increase enemy spawn rate during capture
                if (enemySpawner != null)
                {
                    enemySpawner.SetSpawnRateMultiplier(captureSpawnMultiplier);
                }

                Debug.Log($"[GameManager] Capture started. Active captures: {objectsBeingCaptured.Count}");
            }
        }

        public void OnCaptureCompleted(GameObject vessel)
        {
            if (objectsBeingCaptured.Contains(vessel))
            {
                objectsBeingCaptured.Remove(vessel);
            }

            vesselsCaptured++;
            AddScore(500); // Bonus score for capturing

            OnVesselCaptured?.Invoke(vesselsCaptured);

            // Reset spawn rate if no active captures
            if (objectsBeingCaptured.Count == 0 && enemySpawner != null)
            {
                enemySpawner.SetSpawnRateMultiplier(1f);
            }

            Debug.Log($"[GameManager] Vessel captured! Total: {vesselsCaptured}");
        }

        // Public getters
        public GameState GetCurrentGameState() => currentGameState;
        public int GetScore() => score;
        public int GetEnemiesDestroyed() => enemiesDestroyed;
        public int GetVesselsCaptured() => vesselsCaptured;
        public float GetGameTime() => gameTime;
        public float GetDifficultyMultiplier() => difficultyMultiplier;
        public Transform GetPlayerBoat() => playerBoat;

        // Public setters
        public void SetDifficultyMultiplier(float multiplier)
        {
            difficultyMultiplier = Mathf.Clamp(multiplier, 0.1f, maxDifficultyMultiplier);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            difficultyMultiplier = Mathf.Max(0.1f, difficultyMultiplier);
            difficultyIncreaseRate = Mathf.Max(0f, difficultyIncreaseRate);
            maxDifficultyMultiplier = Mathf.Max(1f, maxDifficultyMultiplier);
            maxSimultaneousCaptures = Mathf.Max(1, maxSimultaneousCaptures);
            captureSpawnMultiplier = Mathf.Max(1f, captureSpawnMultiplier);
        }
#endif
    }
}
