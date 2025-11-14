using UnityEngine;
using System.Collections;

namespace VRBoatCombat.Weapons
{
    /// <summary>
    /// Manages boat-mounted weapons including primary cannon and projectile spawning.
    /// Handles firing, cooldown, ammunition, and weapon effects.
    /// </summary>
    public class WeaponSystem : MonoBehaviour
    {
        [Header("Weapon Configuration")]
        [SerializeField] private WeaponType weaponType = WeaponType.Cannon;
        [SerializeField] private float fireRate = 0.5f; // Shots per second
        [SerializeField] private int maxAmmo = -1; // -1 for infinite
        [SerializeField] private float reloadTime = 2.0f;

        [Header("Projectile Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float projectileSpeed = 50f;
        [SerializeField] private float projectileLifetime = 5f;
        [SerializeField] private float damage = 25f;

        [Header("Weapon Effects")]
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private AudioSource fireSound;
        [SerializeField] private GameObject impactEffectPrefab;
        [SerializeField] private float screenShakeIntensity = 0.2f;
        [SerializeField] private float screenShakeDuration = 0.1f;

        [Header("Recoil Settings")]
        [SerializeField] private float recoilForce = 500f;
        [SerializeField] private Vector3 recoilDirection = new Vector3(0, 0, -1);

        [Header("Weak Point Targeting")]
        [SerializeField] private bool enableWeakPointTargeting = true;
        [SerializeField] private LayerMask weakPointLayer;
        [SerializeField] private float weakPointDamageMultiplier = 2.5f;

        // Component references
        private ObjectPooler objectPooler;
        private Rigidbody boatRigidbody;

        // State tracking
        private float lastFireTime = 0f;
        private int currentAmmo;
        private bool isReloading = false;
        private float fireCooldown;

        public enum WeaponType
        {
            Cannon,
            MachineGun,
            Missile
        }

        private void Awake()
        {
            // Initialize ammo
            currentAmmo = maxAmmo;
            fireCooldown = 1f / fireRate;

            // Get references
            boatRigidbody = GetComponentInParent<Rigidbody>();
            if (boatRigidbody == null)
            {
                Debug.LogWarning($"[WeaponSystem] No Rigidbody found in parent of {gameObject.name}. Recoil disabled.");
            }

            // Validate fire point
            if (firePoint == null)
            {
                Debug.LogError($"[WeaponSystem] Fire point not assigned on {gameObject.name}!");
            }

            // Validate projectile prefab
            if (projectilePrefab == null)
            {
                Debug.LogError($"[WeaponSystem] Projectile prefab not assigned on {gameObject.name}!");
            }
        }

        private void Start()
        {
            // Get object pooler for projectile management
            objectPooler = FindAnyObjectByType<ObjectPooler>();
            if (objectPooler == null)
            {
                Debug.LogWarning("[WeaponSystem] ObjectPooler not found. Projectiles will be instantiated directly (less efficient).");
            }
        }

        /// <summary>
        /// Attempt to fire the weapon
        /// </summary>
        /// <returns>True if weapon fired successfully</returns>
        public bool TryFire()
        {
            // Check if we can fire
            if (!CanFire())
            {
                return false;
            }

            // Fire the weapon
            Fire();
            return true;
        }

        private bool CanFire()
        {
            // Check cooldown
            if (Time.time - lastFireTime < fireCooldown)
            {
                return false;
            }

            // Check if reloading
            if (isReloading)
            {
                return false;
            }

            // Check ammo (if not infinite)
            if (maxAmmo >= 0 && currentAmmo <= 0)
            {
                StartCoroutine(ReloadCoroutine());
                return false;
            }

            return true;
        }

        private void Fire()
        {
            // Update fire time
            lastFireTime = Time.time;

            // Consume ammo
            if (maxAmmo >= 0)
            {
                currentAmmo--;
                if (currentAmmo <= 0)
                {
                    StartCoroutine(ReloadCoroutine());
                }
            }

            // Spawn projectile
            SpawnProjectile();

            // Play effects
            PlayFireEffects();

            // Apply recoil
            ApplyRecoil();

            Debug.Log($"[WeaponSystem] Fired {weaponType}. Ammo remaining: {(maxAmmo >= 0 ? currentAmmo.ToString() : "∞")}");
        }

        private void SpawnProjectile()
        {
            if (projectilePrefab == null || firePoint == null) return;

            GameObject projectile;

            // Use object pooler if available
            if (objectPooler != null)
            {
                projectile = objectPooler.SpawnFromPool("Projectile", firePoint.position, firePoint.rotation);
            }
            else
            {
                projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            }

            if (projectile == null)
            {
                Debug.LogError("[WeaponSystem] Failed to spawn projectile!");
                return;
            }

            // Configure projectile
            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.Initialize(projectileSpeed, damage, projectileLifetime, impactEffectPrefab);

                // Enable weak point targeting if enabled
                if (enableWeakPointTargeting)
                {
                    projectileScript.SetWeakPointSettings(weakPointLayer, weakPointDamageMultiplier);
                }
            }
            else
            {
                // Fallback: Add velocity to rigidbody if no Projectile script
                Rigidbody rb = projectile.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = firePoint.forward * projectileSpeed;
                }

                // Destroy after lifetime
                Destroy(projectile, projectileLifetime);
            }
        }

        private void PlayFireEffects()
        {
            // Muzzle flash
            if (muzzleFlash != null)
            {
                muzzleFlash.Play();
            }

            // Fire sound
            if (fireSound != null)
            {
                fireSound.Play();
            }

            // Screen shake (handled by camera controller)
            if (screenShakeIntensity > 0)
            {
                CameraShake cameraShake = Camera.main?.GetComponent<CameraShake>();
                if (cameraShake != null)
                {
                    cameraShake.Shake(screenShakeIntensity, screenShakeDuration);
                }
            }
        }

        private void ApplyRecoil()
        {
            if (boatRigidbody == null) return;

            // Calculate recoil force in world space
            Vector3 worldRecoilDirection = transform.TransformDirection(recoilDirection.normalized);
            Vector3 recoilForceVector = worldRecoilDirection * recoilForce;

            // Apply force at fire point
            boatRigidbody.AddForceAtPosition(recoilForceVector, firePoint.position, ForceMode.Impulse);
        }

        private IEnumerator ReloadCoroutine()
        {
            if (isReloading) yield break;

            isReloading = true;
            Debug.Log($"[WeaponSystem] Reloading {weaponType}...");

            yield return new WaitForSeconds(reloadTime);

            currentAmmo = maxAmmo;
            isReloading = false;

            Debug.Log($"[WeaponSystem] Reload complete. Ammo: {currentAmmo}");
        }

        // Public getters
        public int GetCurrentAmmo() => currentAmmo;
        public int GetMaxAmmo() => maxAmmo;
        public bool IsReloading() => isReloading;
        public float GetReloadProgress()
        {
            // This would need to be implemented with a reload start time
            return isReloading ? 0.5f : 1f;
        }

        public WeaponType GetWeaponType() => weaponType;

        // Public setters for runtime modification
        public void SetFireRate(float newFireRate)
        {
            fireRate = Mathf.Max(0.1f, newFireRate);
            fireCooldown = 1f / fireRate;
        }

        public void SetDamage(float newDamage)
        {
            damage = Mathf.Max(0f, newDamage);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Clamp values in editor
            fireRate = Mathf.Max(0.1f, fireRate);
            fireCooldown = 1f / fireRate;
            projectileSpeed = Mathf.Max(1f, projectileSpeed);
            projectileLifetime = Mathf.Max(0.1f, projectileLifetime);
            damage = Mathf.Max(0f, damage);
            reloadTime = Mathf.Max(0.1f, reloadTime);
            recoilForce = Mathf.Max(0f, recoilForce);
            weakPointDamageMultiplier = Mathf.Max(1f, weakPointDamageMultiplier);
        }

        private void OnDrawGizmosSelected()
        {
            if (firePoint != null)
            {
                // Draw fire direction
                Gizmos.color = Color.red;
                Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.forward * 3f);
                Gizmos.DrawWireSphere(firePoint.position, 0.2f);

                // Draw recoil direction
                Gizmos.color = Color.yellow;
                Vector3 recoilDir = firePoint.TransformDirection(recoilDirection.normalized);
                Gizmos.DrawLine(firePoint.position, firePoint.position + recoilDir * 2f);
            }
        }
#endif
    }
}
