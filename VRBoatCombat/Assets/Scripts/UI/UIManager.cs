using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace VRBoatCombat.UI
{
    /// <summary>
    /// Manages all UI elements and HUD displays.
    /// Handles score, health, ammo, and game state UI updates.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("HUD Elements")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private Image healthBar;
        [SerializeField] private TextMeshProUGUI ammoText;
        [SerializeField] private TextMeshProUGUI speedText;

        [Header("Game State UI")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;

        [Header("Capture UI")]
        [SerializeField] private GameObject captureProgressPanel;
        [SerializeField] private Image captureProgressBar;
        [SerializeField] private TextMeshProUGUI captureText;

        [Header("Settings")]
        [SerializeField] private bool showFPS = true;
        [SerializeField] private TextMeshProUGUI fpsText;

        // FPS tracking
        private float deltaTime = 0f;
        private float updateInterval = 0.5f;
        private float timeLeft;

        private void Start()
        {
            // Initialize UI state
            ShowMainMenu();
            timeLeft = updateInterval;
        }

        private void Update()
        {
            // Update FPS counter
            if (showFPS && fpsText != null)
            {
                deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
                timeLeft -= Time.deltaTime;

                if (timeLeft <= 0f)
                {
                    float fps = 1.0f / deltaTime;
                    fpsText.text = $"FPS: {Mathf.RoundToInt(fps)}";
                    timeLeft = updateInterval;
                }
            }
        }

        public void OnGameStateChanged(GameManager.GameState newState)
        {
            // Hide all panels first
            HideAllPanels();

            // Show appropriate panel
            switch (newState)
            {
                case GameManager.GameState.MainMenu:
                    ShowMainMenu();
                    break;

                case GameManager.GameState.Playing:
                    ShowHUD();
                    break;

                case GameManager.GameState.Paused:
                    ShowPauseMenu();
                    break;

                case GameManager.GameState.GameOver:
                    ShowGameOver();
                    break;

                case GameManager.GameState.Victory:
                    ShowVictory();
                    break;
            }
        }

        private void HideAllPanels()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
        }

        private void ShowMainMenu()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        }

        private void ShowHUD()
        {
            // HUD is always visible during gameplay
        }

        private void ShowPauseMenu()
        {
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        }

        private void ShowGameOver()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
        }

        private void ShowVictory()
        {
            if (victoryPanel != null) victoryPanel.SetActive(true);
        }

        // HUD update methods
        public void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }

        public void UpdateHealth(float current, float max)
        {
            if (healthText != null)
            {
                healthText.text = $"Health: {Mathf.RoundToInt(current)}";
            }

            if (healthBar != null)
            {
                healthBar.fillAmount = current / max;

                // Change color based on health
                if (current / max < 0.3f)
                {
                    healthBar.color = Color.red;
                }
                else if (current / max < 0.6f)
                {
                    healthBar.color = Color.yellow;
                }
                else
                {
                    healthBar.color = Color.green;
                }
            }
        }

        public void UpdateAmmo(int current, int max)
        {
            if (ammoText != null)
            {
                if (max < 0)
                {
                    ammoText.text = "Ammo: ∞";
                }
                else
                {
                    ammoText.text = $"Ammo: {current}/{max}";
                }
            }
        }

        public void UpdateSpeed(float speed, float maxSpeed)
        {
            if (speedText != null)
            {
                speedText.text = $"Speed: {Mathf.RoundToInt(speed)} / {Mathf.RoundToInt(maxSpeed)}";
            }
        }

        public void ShowCaptureProgress(float progress)
        {
            if (captureProgressPanel != null)
            {
                captureProgressPanel.SetActive(true);
            }

            if (captureProgressBar != null)
            {
                captureProgressBar.fillAmount = progress;
            }

            if (captureText != null)
            {
                captureText.text = $"Capturing: {Mathf.RoundToInt(progress * 100)}%";
            }
        }

        public void HideCaptureProgress()
        {
            if (captureProgressPanel != null)
            {
                captureProgressPanel.SetActive(false);
            }
        }

        // Button handlers (called from UI buttons)
        public void OnStartGameButton()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
            }
        }

        public void OnResumeButton()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }
        }

        public void OnRestartButton()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
        }

        public void OnQuitButton()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
        }

        public void OnMainMenuButton()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeGameState(GameManager.GameState.MainMenu);
            }
        }
    }
}
