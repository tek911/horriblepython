using UnityEngine;
using UnityEngine.Events;
using VRBoatCombat.AI;

namespace VRBoatCombat.Core
{
    /// <summary>
    /// Health system for boats and other damageable entities.
    /// Handles damage, healing, death, and health-related events.
    /// </summary>
    public class HealthSystem : MonoBehaviour, Weapons.IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;
        [SerializeField] private bool invulnerable = false;
        [SerializeField] private bool destroyOnDeath = true;
        [SerializeField] private float destroyDelay = 2f;

        [Header("Damage Modifiers")]
        [SerializeField] private float damageMultiplier = 1f;
        [SerializeField] private bool hasArmor = false;
        [SerializeField] private float armorReduction = 0.5f; // 50% damage reduction

        [Header("Regeneration")]
        [SerializeField] private bool enableRegeneration = false;
        [SerializeField] private float regenerationRate = 1f; // HP per second
        [SerializeField] private float regenerationDelay = 5f; // Delay after taking damage

        [Header("Visual Feedback")]
        [SerializeField] private GameObject damageEffectPrefab;
        [SerializeField] private GameObject deathEffectPrefab;
        [SerializeField] private Renderer meshRenderer;
        [SerializeField] private Color lowHealthTint = Color.red;
        [SerializeField] private float lowHealthThreshold = 0.3f;

        [Header("Audio")]
        [SerializeField] private AudioSource damageSound;
        [SerializeField] private AudioSource deathSound;
        [SerializeField] private AudioClip[] damageSounds;

        [Header("Events")]
        public UnityEvent<float> OnDamaged; // Passes damage amount
        public UnityEvent<float> OnHealed; // Passes heal amount
        public UnityEvent OnDeath;
        public UnityEvent<float> OnHealthChanged; // Passes current health percentage

        // State
        private bool isDead = false;
        private float lastDamageTime = 0f;
        private Material meshMaterial;
        private Color originalColor;

        private void Awake()
        {
            // Initialize health
            currentHealth = maxHealth;

            // Get mesh material for color tinting
            if (meshRenderer != null)
            {
                meshMaterial = meshRenderer.material;
                originalColor = meshMaterial.color;
            }
        }

        private void Start()
        {
            // Initialize health UI if present
            UpdateHealthUI();
        }

        private void Update()
        {
            // Handle regeneration
            if (enableRegeneration && !isDead && currentHealth < maxHealth)
            {
                if (Time.time - lastDamageTime >= regenerationDelay)
                {
                    Heal(regenerationRate * Time.deltaTime);
                }
            }

            // Update visual feedback
            UpdateVisualFeedback();
        }

        /// <summary>
        /// Apply damage to this entity
        /// </summary>
        public void TakeDamage(Weapons.DamageInfo damageInfo)
        {
            if (isDead || invulnerable) return;

            float finalDamage = damageInfo.amount * damageMultiplier;

            // Apply armor reduction
            if (hasArmor)
            {
                finalDamage *= (1f - armorReduction);
            }

            // Apply weak point multiplier (already in damageInfo)
            // The damage amount already includes weak point multiplier from projectile

            // Reduce health
            currentHealth -= finalDamage;
            currentHealth = Mathf.Max(0f, currentHealth);

            lastDamageTime = Time.time;

            Debug.Log($"[HealthSystem] {gameObject.name} took {finalDamage} damage. Health: {currentHealth}/{maxHealth}");

            // Trigger events
            OnDamaged?.Invoke(finalDamage);
            OnHealthChanged?.Invoke(GetHealthPercentage());

            // Notify capture system if present
            CaptureSystem captureSystem = GetComponent<CaptureSystem>();
            if (captureSystem != null)
            {
                captureSystem.OnTargetDamaged();
            }

            // Visual and audio feedback
            PlayDamageEffects(damageInfo.point);

            // Check for death
            if (currentHealth <= 0f && !isDead)
            {
                Die();
            }

            // Update UI
            UpdateHealthUI();
        }

        /// <summary>
        /// Apply damage without DamageInfo struct
        /// </summary>
        public void TakeDamage(float damage)
        {
            Weapons.DamageInfo damageInfo = new Weapons.DamageInfo
            {
                amount = damage,
                point = transform.position,
                direction = Vector3.zero,
                isWeakPoint = false,
                source = null
            };

            TakeDamage(damageInfo);
        }

        /// <summary>
        /// Heal this entity
        /// </summary>
        public void Heal(float amount)
        {
            if (isDead) return;

            float healAmount = Mathf.Min(amount, maxHealth - currentHealth);
            currentHealth += healAmount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            if (healAmount > 0)
            {
                OnHealed?.Invoke(healAmount);
                OnHealthChanged?.Invoke(GetHealthPercentage());

                Debug.Log($"[HealthSystem] {gameObject.name} healed {healAmount}. Health: {currentHealth}/{maxHealth}");

                // Update UI
                UpdateHealthUI();
            }
        }

        private void Die()
        {
            if (isDead) return;

            isDead = true;
            Debug.Log($"[HealthSystem] {gameObject.name} has died");

            // Trigger death event
            OnDeath?.Invoke();

            // Play death effects
            PlayDeathEffects();

            // Disable components
            DisableComponents();

            // Destroy after delay if configured
            if (destroyOnDeath)
            {
                Destroy(gameObject, destroyDelay);
            }
        }

        private void PlayDamageEffects(Vector3 impactPoint)
        {
            // Spawn damage effect
            if (damageEffectPrefab != null)
            {
                GameObject effect = Instantiate(damageEffectPrefab, impactPoint, Quaternion.identity);
                Destroy(effect, 2f);
            }

            // Play damage sound
            if (damageSound != null && damageSounds != null && damageSounds.Length > 0)
            {
                AudioClip clip = damageSounds[Random.Range(0, damageSounds.Length)];
                damageSound.PlayOneShot(clip);
            }
        }

        private void PlayDeathEffects()
        {
            // Spawn death effect (explosion)
            if (deathEffectPrefab != null)
            {
                GameObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

                // Apply explosion force to nearby objects
                Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, 10f);
                foreach (Collider col in nearbyColliders)
                {
                    Rigidbody rb = col.GetComponent<Rigidbody>();
                    if (rb != null && rb != GetComponent<Rigidbody>())
                    {
                        rb.AddExplosionForce(1000f, transform.position, 10f, 3f);
                    }
                }

                Destroy(effect, 5f);
            }

            // Play death sound
            if (deathSound != null)
            {
                // Detach audio source so it continues playing after object is destroyed
                deathSound.transform.SetParent(null);
                deathSound.Play();
                Destroy(deathSound.gameObject, deathSound.clip.length);
            }
        }

        private void UpdateVisualFeedback()
        {
            if (meshMaterial == null) return;

            // Tint mesh based on health percentage
            float healthPercent = GetHealthPercentage();

            if (healthPercent <= lowHealthThreshold)
            {
                // Interpolate between original color and low health tint
                float tintAmount = 1f - (healthPercent / lowHealthThreshold);
                Color targetColor = Color.Lerp(originalColor, lowHealthTint, tintAmount);
                meshMaterial.color = Color.Lerp(meshMaterial.color, targetColor, Time.deltaTime * 2f);
            }
            else
            {
                // Return to original color
                meshMaterial.color = Color.Lerp(meshMaterial.color, originalColor, Time.deltaTime * 2f);
            }
        }

        private void UpdateHealthUI()
        {
            // Find and update health bar if it exists
            HealthBarUI healthBar = GetComponentInChildren<HealthBarUI>();
            if (healthBar != null)
            {
                healthBar.SetHealth(currentHealth, maxHealth);
            }
        }

        private void DisableComponents()
        {
            // Disable AI
            var ai = GetComponent<EnemyAI>();
            if (ai != null) ai.enabled = false;

            // Disable weapons
            var weapons = GetComponentsInChildren<Weapons.WeaponSystem>();
            foreach (var weapon in weapons)
            {
                weapon.enabled = false;
            }

            // Disable capture system
            var captureSystem = GetComponent<CaptureSystem>();
            if (captureSystem != null) captureSystem.enabled = false;

            // Make rigidbody kinematic or disable physics
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                // Don't make kinematic so explosion force still applies
            }
        }

        // Public getters
        public float GetCurrentHealth() => currentHealth;
        public float GetMaxHealth() => maxHealth;
        public float GetHealthPercentage() => currentHealth / maxHealth;
        public bool IsDead() => isDead;
        public bool IsInvulnerable() => invulnerable;

        // Public setters
        public void SetInvulnerable(bool value) => invulnerable = value;
        public void SetMaxHealth(float value)
        {
            maxHealth = Mathf.Max(1f, value);
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            UpdateHealthUI();
        }

        public void SetCurrentHealth(float value)
        {
            currentHealth = Mathf.Clamp(value, 0f, maxHealth);
            UpdateHealthUI();
        }

        // Public methods
        public void FullHeal()
        {
            Heal(maxHealth);
        }

        public void Kill()
        {
            TakeDamage(currentHealth);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            maxHealth = Mathf.Max(1f, maxHealth);
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
            damageMultiplier = Mathf.Max(0f, damageMultiplier);
            armorReduction = Mathf.Clamp01(armorReduction);
            regenerationRate = Mathf.Max(0f, regenerationRate);
            regenerationDelay = Mathf.Max(0f, regenerationDelay);
            lowHealthThreshold = Mathf.Clamp01(lowHealthThreshold);
            destroyDelay = Mathf.Max(0f, destroyDelay);
        }
#endif

        private void OnDestroy()
        {
            // Clean up material instance
            if (meshMaterial != null)
            {
                Destroy(meshMaterial);
            }
        }
    }

    /// <summary>
    /// Simple health bar UI component
    /// </summary>
    public class HealthBarUI : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Image fillImage;
        [SerializeField] private TMPro.TextMeshProUGUI healthText;
        [SerializeField] private bool worldSpace = true;

        private void Update()
        {
            // Make health bar face camera if world space
            if (worldSpace && Camera.main != null)
            {
                transform.LookAt(Camera.main.transform);
                transform.Rotate(0, 180, 0); // Face towards camera
            }
        }

        public void SetHealth(float current, float max)
        {
            if (fillImage != null)
            {
                fillImage.fillAmount = current / max;
            }

            if (healthText != null)
            {
                healthText.text = $"{Mathf.RoundToInt(current)}/{Mathf.RoundToInt(max)}";
            }
        }
    }
}
