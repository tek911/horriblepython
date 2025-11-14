using UnityEngine;

namespace VRBoatCombat.Weapons
{
    /// <summary>
    /// Handles projectile physics, collision detection, and damage application.
    /// Supports weak point targeting and impact effects.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        [Header("Visual Effects")]
        [SerializeField] private TrailRenderer trailRenderer;
        [SerializeField] private GameObject projectileModel;

        // Runtime configuration
        private float speed;
        private float damage;
        private float lifetime;
        private GameObject impactEffect;
        private float spawnTime;

        // Weak point settings
        private bool weakPointTargetingEnabled = false;
        private LayerMask weakPointLayer;
        private float weakPointMultiplier = 1f;

        // Component references
        private Rigidbody rb;
        private bool hasImpacted = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }

            // Configure rigidbody for projectile
            rb.useGravity = true; // Realistic arc
            rb.drag = 0.01f; // Minimal air resistance
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        /// <summary>
        /// Initialize projectile with parameters
        /// </summary>
        public void Initialize(float projectileSpeed, float projectileDamage, float projectileLifetime, GameObject impactEffectPrefab)
        {
            speed = projectileSpeed;
            damage = projectileDamage;
            lifetime = projectileLifetime;
            impactEffect = impactEffectPrefab;
            spawnTime = Time.time;
            hasImpacted = false;

            // Apply velocity
            rb.velocity = transform.forward * speed;

            // Enable trail if present
            if (trailRenderer != null)
            {
                trailRenderer.Clear();
                trailRenderer.emitting = true;
            }

            // Show model
            if (projectileModel != null)
            {
                projectileModel.SetActive(true);
            }
        }

        /// <summary>
        /// Configure weak point targeting
        /// </summary>
        public void SetWeakPointSettings(LayerMask weakPointLayerMask, float multiplier)
        {
            weakPointTargetingEnabled = true;
            weakPointLayer = weakPointLayerMask;
            weakPointMultiplier = multiplier;
        }

        private void Update()
        {
            // Check lifetime
            if (Time.time - spawnTime >= lifetime)
            {
                DestroyProjectile();
            }

            // Orient projectile to velocity direction
            if (rb.velocity.sqrMagnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(rb.velocity);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (hasImpacted) return;

            hasImpacted = true;

            // Calculate damage
            float finalDamage = damage;
            bool isWeakPoint = false;

            // Check for weak point hit
            if (weakPointTargetingEnabled)
            {
                int hitLayer = 1 << collision.gameObject.layer;
                if ((weakPointLayer.value & hitLayer) != 0)
                {
                    finalDamage *= weakPointMultiplier;
                    isWeakPoint = true;
                    Debug.Log($"[Projectile] Weak point hit! Damage: {finalDamage}");
                }
            }

            // Apply damage to target
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                DamageInfo damageInfo = new DamageInfo
                {
                    amount = finalDamage,
                    point = collision.contacts[0].point,
                    direction = rb.velocity.normalized,
                    isWeakPoint = isWeakPoint,
                    source = gameObject
                };

                damageable.TakeDamage(damageInfo);
            }

            // Apply impact force
            Rigidbody targetRb = collision.rigidbody;
            if (targetRb != null)
            {
                Vector3 impactForce = rb.velocity.normalized * (rb.mass * speed * 0.5f);
                targetRb.AddForceAtPosition(impactForce, collision.contacts[0].point, ForceMode.Impulse);
            }

            // Spawn impact effect
            SpawnImpactEffect(collision.contacts[0].point, collision.contacts[0].normal);

            // Destroy projectile
            DestroyProjectile();
        }

        private void SpawnImpactEffect(Vector3 position, Vector3 normal)
        {
            if (impactEffect == null) return;

            // Spawn from pool if available
            ObjectPooler pooler = FindObjectOfType<ObjectPooler>();
            GameObject effect;

            if (pooler != null)
            {
                effect = pooler.SpawnFromPool("ImpactEffect", position, Quaternion.LookRotation(normal));
            }
            else
            {
                effect = Instantiate(impactEffect, position, Quaternion.LookRotation(normal));
            }

            // Auto-destroy effect after some time
            if (effect != null)
            {
                ParticleSystem ps = effect.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
                }
                else
                {
                    Destroy(effect, 2f);
                }
            }
        }

        private void DestroyProjectile()
        {
            // Disable trail
            if (trailRenderer != null)
            {
                trailRenderer.emitting = false;
            }

            // Hide model
            if (projectileModel != null)
            {
                projectileModel.SetActive(false);
            }

            // Return to pool or destroy
            ObjectPooler pooler = FindObjectOfType<ObjectPooler>();
            if (pooler != null)
            {
                pooler.ReturnToPool(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Public getters
        public float GetDamage() => damage;
        public float GetSpeed() => speed;
    }

    /// <summary>
    /// Interface for objects that can take damage
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(DamageInfo damageInfo);
    }

    /// <summary>
    /// Damage information structure
    /// </summary>
    public struct DamageInfo
    {
        public float amount;
        public Vector3 point;
        public Vector3 direction;
        public bool isWeakPoint;
        public GameObject source;
    }
}
