using UnityEngine;
using System.Collections;
using VRBoatCombat.Weapons;

namespace VRBoatCombat.Core
{
    /// <summary>
    /// Handles zipline traversal along grapple ropes.
    /// Manages smooth movement, camera transitions, and vessel switching.
    /// </summary>
    public class ZiplineController : MonoBehaviour
    {
        [Header("Zipline Settings")]
        [SerializeField] private float ziplineSpeed = 10f;
        [SerializeField] private AnimationCurve speedCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        #pragma warning disable 0414 // Field assigned but never used (reserved for future bidirectional zipline feature)
        [SerializeField] private bool allowBidirectional = true;
        #pragma warning restore 0414

        [Header("Camera Transitions")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float transitionDuration = 0.5f;
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Visual Effects")]
        [SerializeField] private GameObject handModelLeft;
        [SerializeField] private GameObject handModelRight;
        [SerializeField] private ParticleSystem windEffect;
        [SerializeField] private float windEffectIntensity = 1f;

        [Header("Motion Effects")]
        [SerializeField] private bool enableMotionBlur = true;
        [SerializeField] private float motionBlurAmount = 0.5f;
        [SerializeField] private float fieldOfViewChange = 10f;

        [Header("Audio")]
        [SerializeField] private AudioSource ziplineSound;
        [SerializeField] private AudioSource windSound;
        [SerializeField] private float windVolumeCurve = 1f;

        [Header("Input")]
        [SerializeField] private UnityEngine.XR.InputDevice leftController;
        [SerializeField] private UnityEngine.XR.InputDevice rightController;

        // Component references
        private GrappleHook grappleHook;
        private Transform playerBoat;
        private Transform targetBoat;

        // State
        private bool isZiplining = false;
        private float ziplineProgress = 0f;
        private Vector3 ziplineStart;
        private Vector3 ziplineEnd;
        private float originalFOV;

        // Post-processing (if available)
        private UnityEngine.Rendering.PostProcessing.PostProcessVolume postProcessVolume;
        private UnityEngine.Rendering.PostProcessing.MotionBlur motionBlur;

        private void Awake()
        {
            // Get references
            grappleHook = GetComponent<GrappleHook>();
            if (grappleHook == null)
            {
                Debug.LogError($"[ZiplineController] GrappleHook component not found on {gameObject.name}!");
            }

            // Get player camera
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }

            if (playerCamera != null)
            {
                originalFOV = playerCamera.fieldOfView;
            }

            // Setup post-processing
            SetupPostProcessing();

            // Hide hand models initially
            SetHandModelsActive(false);
        }

        private void SetupPostProcessing()
        {
            if (!enableMotionBlur) return;

            // Try to get post-process volume
            postProcessVolume = playerCamera?.GetComponent<UnityEngine.Rendering.PostProcessing.PostProcessVolume>();

            if (postProcessVolume != null && postProcessVolume.profile != null)
            {
                postProcessVolume.profile.TryGetSettings(out motionBlur);
            }
        }

        private void Update()
        {
            // Check for zipline input when grapple is active
            if (!isZiplining && grappleHook != null && grappleHook.IsGrappleActive())
            {
                CheckZiplineInput();
            }

            // Update zipline movement
            if (isZiplining)
            {
                UpdateZiplineMovement();
            }
        }

        private void CheckZiplineInput()
        {
            // Check for both triggers pressed (or other input)
            bool leftTrigger = false;
            bool rightTrigger = false;

            if (leftController.isValid)
            {
                leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out leftTrigger);
            }

            if (rightController.isValid)
            {
                rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out rightTrigger);
            }

            // Alternative: Check for grip buttons
            bool leftGrip = false;
            bool rightGrip = false;

            if (leftController.isValid)
            {
                leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out leftGrip);
            }

            if (rightController.isValid)
            {
                rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out rightGrip);
            }

            // Start zipline if both grips pressed
            if (leftGrip && rightGrip)
            {
                StartZipline();
            }
        }

        /// <summary>
        /// Start zipline traversal
        /// </summary>
        public void StartZipline()
        {
            if (isZiplining) return;
            if (grappleHook == null || !grappleHook.IsGrappleActive()) return;

            Transform grapplePoint = grappleHook.GetGrapplePoint();
            if (grapplePoint == null) return;

            isZiplining = true;
            ziplineProgress = 0f;

            // Set start and end points
            playerBoat = transform;
            targetBoat = grapplePoint;
            ziplineStart = playerCamera.transform.position;
            ziplineEnd = targetBoat.position + Vector3.up * 2f; // Offset for player height

            Debug.Log($"[ZiplineController] Starting zipline from {playerBoat.name} to {targetBoat.name}");

            // Show hand models
            SetHandModelsActive(true);

            // Start effects
            StartZiplineEffects();
        }

        private void UpdateZiplineMovement()
        {
            // Increment progress
            float normalizedSpeed = ziplineSpeed / Vector3.Distance(ziplineStart, ziplineEnd);
            ziplineProgress += Time.deltaTime * normalizedSpeed;

            // Apply speed curve
            float curvedProgress = speedCurve.Evaluate(ziplineProgress);

            // Calculate current position along zipline
            Vector3 currentPosition = Vector3.Lerp(ziplineStart, ziplineEnd, curvedProgress);

            // Move camera (or player rig)
            if (playerCamera != null)
            {
                playerCamera.transform.position = currentPosition;

                // Look towards target
                Vector3 direction = (ziplineEnd - currentPosition).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    playerCamera.transform.rotation = Quaternion.Slerp(
                        playerCamera.transform.rotation,
                        targetRotation,
                        Time.deltaTime * 5f
                    );
                }
            }

            // Update effects based on speed
            UpdateZiplineEffects(curvedProgress);

            // Check if reached destination
            if (ziplineProgress >= 1f)
            {
                EndZipline();
            }
        }

        private void StartZiplineEffects()
        {
            // Play zipline sound
            if (ziplineSound != null)
            {
                ziplineSound.Play();
            }

            // Play wind sound
            if (windSound != null)
            {
                windSound.Play();
            }

            // Start wind particles
            if (windEffect != null)
            {
                windEffect.Play();
                var emission = windEffect.emission;
                emission.rateOverTime = windEffectIntensity * 50f;
            }

            // Enable motion blur
            if (motionBlur != null)
            {
                motionBlur.enabled.Override(true);
                motionBlur.shutterAngle.Override(motionBlurAmount * 360f);
            }
        }

        private void UpdateZiplineEffects(float progress)
        {
            // Update FOV for speed feeling
            if (playerCamera != null)
            {
                float targetFOV = originalFOV + (fieldOfViewChange * Mathf.Sin(progress * Mathf.PI));
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * 5f);
            }

            // Update wind sound volume
            if (windSound != null)
            {
                float speedFactor = speedCurve.Evaluate(progress);
                windSound.volume = speedFactor * windVolumeCurve;
            }

            // Update wind particles
            if (windEffect != null)
            {
                var emission = windEffect.emission;
                emission.rateOverTime = windEffectIntensity * 50f * speedCurve.Evaluate(progress);
            }
        }

        private void EndZipline()
        {
            isZiplining = false;
            ziplineProgress = 0f;

            Debug.Log("[ZiplineController] Zipline traversal complete");

            // Hide hand models
            SetHandModelsActive(false);

            // Stop effects
            StopZiplineEffects();

            // Switch to target vessel
            SwitchToTargetVessel();
        }

        private void StopZiplineEffects()
        {
            // Stop sounds
            if (ziplineSound != null)
            {
                ziplineSound.Stop();
            }

            if (windSound != null)
            {
                windSound.Stop();
            }

            // Stop wind particles
            if (windEffect != null)
            {
                windEffect.Stop();
            }

            // Disable motion blur
            if (motionBlur != null)
            {
                motionBlur.enabled.Override(false);
            }

            // Reset FOV
            if (playerCamera != null)
            {
                StartCoroutine(ResetFOVCoroutine());
            }
        }

        private IEnumerator ResetFOVCoroutine()
        {
            float elapsed = 0f;
            float startFOV = playerCamera.fieldOfView;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = transitionCurve.Evaluate(elapsed / transitionDuration);
                playerCamera.fieldOfView = Mathf.Lerp(startFOV, originalFOV, t);
                yield return null;
            }

            playerCamera.fieldOfView = originalFOV;
        }

        private void SwitchToTargetVessel()
        {
            if (targetBoat == null) return;

            // Detach grapple
            if (grappleHook != null)
            {
                grappleHook.DetachGrapple();
            }

            // Transfer control to target vessel
            VRBoatController targetController = targetBoat.GetComponentInChildren<VRBoatController>();
            if (targetController != null)
            {
                // Enable target controller
                targetController.enabled = true;

                // Disable current controller
                VRBoatController currentController = playerBoat.GetComponentInChildren<VRBoatController>();
                if (currentController != null)
                {
                    currentController.enabled = false;
                }

                Debug.Log($"[ZiplineController] Switched control to {targetBoat.name}");
            }

            // Update camera parent
            if (playerCamera != null)
            {
                playerCamera.transform.SetParent(targetBoat);
            }
        }

        private void SetHandModelsActive(bool active)
        {
            if (handModelLeft != null)
            {
                handModelLeft.SetActive(active);
            }

            if (handModelRight != null)
            {
                handModelRight.SetActive(active);
            }
        }

        // Public getters
        public bool IsZiplining() => isZiplining;
        public float GetZiplineProgress() => ziplineProgress;

        // Public methods
        public void CancelZipline()
        {
            if (!isZiplining) return;

            StopZiplineEffects();
            SetHandModelsActive(false);
            isZiplining = false;
            ziplineProgress = 0f;

            Debug.Log("[ZiplineController] Zipline cancelled");
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ziplineSpeed = Mathf.Max(0.1f, ziplineSpeed);
            transitionDuration = Mathf.Max(0.1f, transitionDuration);
            windEffectIntensity = Mathf.Max(0f, windEffectIntensity);
            motionBlurAmount = Mathf.Clamp01(motionBlurAmount);
            fieldOfViewChange = Mathf.Clamp(fieldOfViewChange, 0f, 45f);
        }
#endif
    }
}
