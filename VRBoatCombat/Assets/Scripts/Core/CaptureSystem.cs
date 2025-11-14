using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using VRBoatCombat.AI;

namespace VRBoatCombat.Core
{
    /// <summary>
    /// Handles the capture mechanics for enemy vessels.
    /// Manages capture progress, health tracking, and state transitions.
    /// </summary>
    public class CaptureSystem : MonoBehaviour
    {
        [Header("Capture Settings")]
        [SerializeField] private float captureTime = 10f;
        [SerializeField] private float captureRange = 20f;
        [SerializeField] private bool requireLowHealth = true;
        [SerializeField] private float captureHealthThreshold = 0.3f; // 30% health or below

        [Header("Capture Progress")]
        [SerializeField] private float captureDecayRate = 0.5f; // Progress lost per second when out of range
        [SerializeField] private bool resetOnDamage = true;
        [SerializeField] private float damageResetPenalty = 0.3f; // 30% progress lost on damage

        [Header("References")]
        [SerializeField] private Transform playerBoat;
        [SerializeField] private HealthSystem targetHealthSystem;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject captureUI;
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Color captureHighlightColor = Color.yellow;
        [SerializeField] private string captureColorProperty = "_EmissionColor";

        [Header("Audio")]
        [SerializeField] private AudioSource captureProgressSound;
        [SerializeField] private AudioSource captureCompleteSound;
        [SerializeField] private AudioSource captureFailedSound;

        [Header("Events")]
        public UnityEvent OnCaptureStarted;
        public UnityEvent OnCaptureProgressed;
        public UnityEvent OnCaptureCompleted;
        public UnityEvent OnCaptureFailed;
        public UnityEvent OnCaptureInterrupted;

        // State
        private CaptureState currentState = CaptureState.Idle;
        private float captureProgress = 0f; // 0 to 1
        private Material targetMaterial;
        private Color originalEmissionColor;
        private bool isInCaptureRange = false;

        public enum CaptureState
        {
            Idle,
            InProgress,
            Paused,
            Completed,
            Failed
        }

        private void Awake()
        {
            // Get target health system if not assigned
            if (targetHealthSystem == null)
            {
                targetHealthSystem = GetComponent<HealthSystem>();
                if (targetHealthSystem == null)
                {
                    Debug.LogWarning($"[CaptureSystem] No HealthSystem found on {gameObject.name}");
                }
            }

            // Get target material for highlighting
            if (targetRenderer != null)
            {
                targetMaterial = targetRenderer.material;
                if (targetMaterial.HasProperty(captureColorProperty))
                {
                    originalEmissionColor = targetMaterial.GetColor(captureColorProperty);
                }
            }

            // Find player boat if not assigned
            if (playerBoat == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerBoat = player.transform;
                }
                else
                {
                    Debug.LogWarning("[CaptureSystem] Player boat not found!");
                }
            }

            // Hide capture UI initially
            if (captureUI != null)
            {
                captureUI.SetActive(false);
            }
        }

        private void Update()
        {
            // Check capture conditions
            CheckCaptureConditions();

            // Update capture progress
            UpdateCaptureProgress();

            // Update visual feedback
            UpdateVisualFeedback();
        }

        private void CheckCaptureConditions()
        {
            if (playerBoat == null) return;

            // Check if player is in range
            float distance = Vector3.Distance(transform.position, playerBoat.position);
            isInCaptureRange = distance <= captureRange;

            // Check health threshold if required
            bool healthConditionMet = true;
            if (requireLowHealth && targetHealthSystem != null)
            {
                healthConditionMet = targetHealthSystem.GetHealthPercentage() <= captureHealthThreshold;
            }

            // Determine if capture can proceed
            bool canCapture = isInCaptureRange && healthConditionMet;

            // Update state based on conditions
            if (canCapture && currentState == CaptureState.Idle && captureProgress == 0f)
            {
                StartCapture();
            }
            else if (canCapture && currentState == CaptureState.Paused)
            {
                ResumeCapture();
            }
            else if (!canCapture && currentState == CaptureState.InProgress)
            {
                PauseCapture();
            }
        }

        private void UpdateCaptureProgress()
        {
            switch (currentState)
            {
                case CaptureState.InProgress:
                    // Increase capture progress
                    captureProgress += Time.deltaTime / captureTime;
                    captureProgress = Mathf.Clamp01(captureProgress);

                    // Play progress sound
                    if (captureProgressSound != null && !captureProgressSound.isPlaying)
                    {
                        captureProgressSound.Play();
                    }

                    // Trigger progress event
                    OnCaptureProgressed?.Invoke();

                    // Check if capture complete
                    if (captureProgress >= 1f)
                    {
                        CompleteCapture();
                    }
                    break;

                case CaptureState.Paused:
                    // Decay capture progress when paused
                    captureProgress -= Time.deltaTime * captureDecayRate;
                    captureProgress = Mathf.Max(0f, captureProgress);

                    // Stop progress sound
                    if (captureProgressSound != null && captureProgressSound.isPlaying)
                    {
                        captureProgressSound.Stop();
                    }

                    // Return to idle if progress fully decayed
                    if (captureProgress <= 0f)
                    {
                        currentState = CaptureState.Idle;
                        OnCaptureInterrupted?.Invoke();
                    }
                    break;
            }
        }

        private void UpdateVisualFeedback()
        {
            // Update capture UI
            if (captureUI != null)
            {
                captureUI.SetActive(currentState == CaptureState.InProgress || currentState == CaptureState.Paused);

                // Update progress bar if it has one
                CaptureProgressUI progressUI = captureUI.GetComponent<CaptureProgressUI>();
                if (progressUI != null)
                {
                    progressUI.SetProgress(captureProgress);
                }
            }

            // Update emission color based on capture progress
            if (targetMaterial != null && targetMaterial.HasProperty(captureColorProperty))
            {
                Color emissionColor = Color.Lerp(originalEmissionColor, captureHighlightColor, captureProgress);
                targetMaterial.SetColor(captureColorProperty, emissionColor);
            }
        }

        private void StartCapture()
        {
            if (currentState == CaptureState.Completed) return;

            currentState = CaptureState.InProgress;
            Debug.Log($"[CaptureSystem] Capture started on {gameObject.name}");

            OnCaptureStarted?.Invoke();

            // Notify game manager to increase enemy spawn
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.OnCaptureStarted(gameObject);
            }
        }

        private void PauseCapture()
        {
            if (currentState != CaptureState.InProgress) return;

            currentState = CaptureState.Paused;
            Debug.Log($"[CaptureSystem] Capture paused on {gameObject.name}");
        }

        private void ResumeCapture()
        {
            if (currentState != CaptureState.Paused) return;

            currentState = CaptureState.InProgress;
            Debug.Log($"[CaptureSystem] Capture resumed on {gameObject.name}");
        }

        private void CompleteCapture()
        {
            currentState = CaptureState.Completed;
            captureProgress = 1f;

            Debug.Log($"[CaptureSystem] Capture completed on {gameObject.name}!");

            // Stop progress sound
            if (captureProgressSound != null && captureProgressSound.isPlaying)
            {
                captureProgressSound.Stop();
            }

            // Play complete sound
            if (captureCompleteSound != null)
            {
                captureCompleteSound.Play();
            }

            OnCaptureCompleted?.Invoke();

            // Notify game manager
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.OnCaptureCompleted(gameObject);
            }

            // Convert vessel to player control
            ConvertToPlayerControl();
        }

        private void FailCapture()
        {
            currentState = CaptureState.Failed;
            captureProgress = 0f;

            Debug.Log($"[CaptureSystem] Capture failed on {gameObject.name}");

            // Stop progress sound
            if (captureProgressSound != null && captureProgressSound.isPlaying)
            {
                captureProgressSound.Stop();
            }

            // Play failed sound
            if (captureFailedSound != null)
            {
                captureFailedSound.Play();
            }

            OnCaptureFailed?.Invoke();
        }

        private void ConvertToPlayerControl()
        {
            // Disable enemy AI
            EnemyAI enemyAI = GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.enabled = false;
            }

            // Change to friendly team
            gameObject.tag = "Friendly";

            // Fully heal the vessel
            if (targetHealthSystem != null)
            {
                targetHealthSystem.Heal(targetHealthSystem.GetMaxHealth());
            }

            // Reset material
            if (targetMaterial != null && targetMaterial.HasProperty(captureColorProperty))
            {
                targetMaterial.SetColor(captureColorProperty, Color.green);
            }
        }

        /// <summary>
        /// Called when target takes damage (integrate with HealthSystem)
        /// </summary>
        public void OnTargetDamaged()
        {
            if (!resetOnDamage) return;

            if (currentState == CaptureState.InProgress || currentState == CaptureState.Paused)
            {
                // Apply damage penalty to capture progress
                captureProgress -= damageResetPenalty;
                captureProgress = Mathf.Max(0f, captureProgress);

                Debug.Log($"[CaptureSystem] Capture progress reduced due to damage: {captureProgress * 100f}%");
            }
        }

        // Public getters
        public float GetCaptureProgress() => captureProgress;
        public CaptureState GetCurrentState() => currentState;
        public bool IsInCaptureRange() => isInCaptureRange;

        // Public methods
        public void ForceCompleteCapture()
        {
            CompleteCapture();
        }

        public void ResetCapture()
        {
            currentState = CaptureState.Idle;
            captureProgress = 0f;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            captureTime = Mathf.Max(0.1f, captureTime);
            captureRange = Mathf.Max(1f, captureRange);
            captureHealthThreshold = Mathf.Clamp01(captureHealthThreshold);
            captureDecayRate = Mathf.Max(0f, captureDecayRate);
            damageResetPenalty = Mathf.Clamp01(damageResetPenalty);
        }

        private void OnDrawGizmosSelected()
        {
            // Draw capture range
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, captureRange);

            // Draw connection to player if in range
            if (playerBoat != null && isInCaptureRange)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, playerBoat.position);
            }
        }
#endif

        private void OnDestroy()
        {
            // Clean up material instance
            if (targetMaterial != null)
            {
                Destroy(targetMaterial);
            }
        }
    }

    /// <summary>
    /// Simple UI component for capture progress visualization
    /// </summary>
    public class CaptureProgressUI : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Image progressBar;
        [SerializeField] private TMPro.TextMeshProUGUI progressText;

        public void SetProgress(float progress)
        {
            if (progressBar != null)
            {
                progressBar.fillAmount = progress;
            }

            if (progressText != null)
            {
                progressText.text = $"Capturing: {Mathf.RoundToInt(progress * 100)}%";
            }
        }
    }
}
