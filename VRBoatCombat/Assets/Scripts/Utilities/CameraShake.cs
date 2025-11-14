using UnityEngine;
using System.Collections;

namespace VRBoatCombat
{
    /// <summary>
    /// Camera shake system for impact feedback and explosions.
    /// Provides smooth shake with customizable intensity and duration.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        [Header("Shake Settings")]
        [SerializeField] private float shakeIntensity = 0.3f;
        [SerializeField] private float shakeDuration = 0.2f;
        [SerializeField] private float shakeFrequency = 25f;

        [Header("VR Comfort Settings")]
        [SerializeField] private bool vrComfortMode = true;
        [SerializeField] [Range(0f, 1f)] private float vrShakeReduction = 0.5f;

        [Header("Shake Types")]
        [SerializeField] private AnimationCurve shakeDecayCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        // State
        private Vector3 originalPosition;
        private bool isShaking = false;
        private Coroutine currentShake;

        private void Awake()
        {
            originalPosition = transform.localPosition;
        }

        /// <summary>
        /// Trigger camera shake
        /// </summary>
        public void Shake(float intensity, float duration)
        {
            // Stop current shake if running
            if (currentShake != null)
            {
                StopCoroutine(currentShake);
            }

            // Apply VR comfort settings
            if (vrComfortMode)
            {
                intensity *= (1f - vrShakeReduction);
                duration *= (1f - vrShakeReduction * 0.5f);
            }

            // Start new shake
            currentShake = StartCoroutine(ShakeCoroutine(intensity, duration));
        }

        /// <summary>
        /// Trigger shake with default settings
        /// </summary>
        public void Shake()
        {
            Shake(shakeIntensity, shakeDuration);
        }

        /// <summary>
        /// Trigger directional shake (e.g., from impact)
        /// </summary>
        public void ShakeDirectional(Vector3 direction, float intensity, float duration)
        {
            if (currentShake != null)
            {
                StopCoroutine(currentShake);
            }

            if (vrComfortMode)
            {
                intensity *= (1f - vrShakeReduction);
                duration *= (1f - vrShakeReduction * 0.5f);
            }

            currentShake = StartCoroutine(DirectionalShakeCoroutine(direction.normalized, intensity, duration));
        }

        /// <summary>
        /// Trigger explosion shake that decreases with distance
        /// </summary>
        public void ShakeExplosion(Vector3 explosionPosition, float explosionIntensity, float maxDistance)
        {
            float distance = Vector3.Distance(transform.position, explosionPosition);

            if (distance > maxDistance) return;

            // Calculate intensity based on distance
            float normalizedDistance = distance / maxDistance;
            float distanceFalloff = 1f - normalizedDistance;
            float intensity = explosionIntensity * distanceFalloff;
            float duration = shakeDuration * (1f + distanceFalloff);

            // Calculate direction
            Vector3 direction = (transform.position - explosionPosition).normalized;

            ShakeDirectional(direction, intensity, duration);
        }

        private IEnumerator ShakeCoroutine(float intensity, float duration)
        {
            isShaking = true;
            float elapsed = 0f;
            originalPosition = transform.localPosition;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                // Calculate shake amount with decay
                float progress = elapsed / duration;
                float decayMultiplier = shakeDecayCurve.Evaluate(progress);
                float currentIntensity = intensity * decayMultiplier;

                // Generate random shake offset using Perlin noise
                float x = (Mathf.PerlinNoise(Time.time * shakeFrequency, 0f) - 0.5f) * 2f * currentIntensity;
                float y = (Mathf.PerlinNoise(0f, Time.time * shakeFrequency) - 0.5f) * 2f * currentIntensity;
                float z = (Mathf.PerlinNoise(Time.time * shakeFrequency, Time.time * shakeFrequency) - 0.5f) * 2f * currentIntensity;

                Vector3 offset = new Vector3(x, y, z);

                // Apply shake
                transform.localPosition = originalPosition + offset;

                yield return null;
            }

            // Reset to original position
            transform.localPosition = originalPosition;
            isShaking = false;
            currentShake = null;
        }

        private IEnumerator DirectionalShakeCoroutine(Vector3 direction, float intensity, float duration)
        {
            isShaking = true;
            float elapsed = 0f;
            originalPosition = transform.localPosition;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                // Calculate shake amount with decay
                float progress = elapsed / duration;
                float decayMultiplier = shakeDecayCurve.Evaluate(progress);
                float currentIntensity = intensity * decayMultiplier;

                // Generate shake along direction with some randomness
                float mainShake = Mathf.Sin(elapsed * shakeFrequency * Mathf.PI * 2f) * currentIntensity;
                Vector3 mainOffset = direction * mainShake;

                // Add perpendicular noise
                Vector3 perpendicular1 = Vector3.Cross(direction, Vector3.up).normalized;
                Vector3 perpendicular2 = Vector3.Cross(direction, perpendicular1).normalized;

                float perp1Noise = (Mathf.PerlinNoise(Time.time * shakeFrequency, 0f) - 0.5f) * currentIntensity * 0.3f;
                float perp2Noise = (Mathf.PerlinNoise(0f, Time.time * shakeFrequency) - 0.5f) * currentIntensity * 0.3f;

                Vector3 offset = mainOffset + perpendicular1 * perp1Noise + perpendicular2 * perp2Noise;

                // Apply shake
                transform.localPosition = originalPosition + offset;

                yield return null;
            }

            // Reset to original position
            transform.localPosition = originalPosition;
            isShaking = false;
            currentShake = null;
        }

        /// <summary>
        /// Stop current shake immediately
        /// </summary>
        public void StopShake()
        {
            if (currentShake != null)
            {
                StopCoroutine(currentShake);
                currentShake = null;
            }

            transform.localPosition = originalPosition;
            isShaking = false;
        }

        // Public getters
        public bool IsShaking() => isShaking;

        // Public setters
        public void SetVRComfortMode(bool enabled)
        {
            vrComfortMode = enabled;
        }

        public void SetVRShakeReduction(float reduction)
        {
            vrShakeReduction = Mathf.Clamp01(reduction);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            shakeIntensity = Mathf.Max(0f, shakeIntensity);
            shakeDuration = Mathf.Max(0f, shakeDuration);
            shakeFrequency = Mathf.Max(0.1f, shakeFrequency);
        }

        // Test shake in editor
        [ContextMenu("Test Shake")]
        private void TestShake()
        {
            Shake();
        }

        [ContextMenu("Test Explosion Shake")]
        private void TestExplosionShake()
        {
            Vector3 explosionPos = transform.position + transform.forward * 10f;
            ShakeExplosion(explosionPos, 1f, 20f);
        }
#endif
    }
}
