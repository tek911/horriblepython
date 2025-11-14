using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace VRBoatCombat.AI
{
    /// <summary>
    /// Enemy boat AI controller with state machine.
    /// Handles patrol, pursuit, attack, and flank behaviors.
    /// </summary>
    [RequireComponent(typeof(Core.BoatPhysics))]
    public class EnemyAI : MonoBehaviour
    {
        [Header("AI State")]
        [SerializeField] private AIState currentState = AIState.Patrol;
        [SerializeField] private AIPersonality personality = AIPersonality.Balanced;

        [Header("Detection")]
        [SerializeField] private float detectionRange = 50f;
        [SerializeField] private float attackRange = 30f;
        [SerializeField] private float loseTargetRange = 80f;
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private float detectionInterval = 0.5f;

        [Header("Patrol Settings")]
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float patrolSpeed = 0.5f;
        [SerializeField] private float patrolWaitTime = 2f;
        [SerializeField] private bool randomPatrol = true;

        [Header("Combat Settings")]
        [SerializeField] private float pursuitSpeed = 0.8f;
        [SerializeField] private float flankDistance = 20f;
        [SerializeField] private float flankAngle = 90f;
        [SerializeField] private float fireAccuracy = 0.8f;
        [SerializeField] private float fireRate = 1f;

        [Header("Evasion Settings")]
        [SerializeField] private bool enableEvasion = true;
        [SerializeField] private float evasionChance = 0.3f;
        [SerializeField] private float evasionDuration = 2f;

        [Header("Difficulty Scaling")]
        [SerializeField] private float difficultyMultiplier = 1f;
        [SerializeField] private bool scaleWithPlayerProgress = true;

        // Component references
        private Core.BoatPhysics boatPhysics;
        private Weapons.WeaponSystem weaponSystem;
        private Core.HealthSystem healthSystem;
        private Transform currentTarget;

        // State tracking
        private int currentPatrolIndex = 0;
        private float lastDetectionTime = 0f;
        private float lastFireTime = 0f;
        private bool isEvading = false;
        private Vector3 flankPosition;

        public enum AIState
        {
            Patrol,
            Pursue,
            Attack,
            Flank,
            Evade,
            Retreat
        }

        public enum AIPersonality
        {
            Aggressive,  // Pursues and attacks relentlessly
            Defensive,   // Maintains distance, defensive tactics
            Balanced,    // Mix of offensive and defensive
            Tactical     // Uses flanking and evasion
        }

        private void Awake()
        {
            // Get required components
            boatPhysics = GetComponent<Core.BoatPhysics>();
            if (boatPhysics == null)
            {
                Debug.LogError($"[EnemyAI] BoatPhysics not found on {gameObject.name}!");
            }

            weaponSystem = GetComponentInChildren<Weapons.WeaponSystem>();
            if (weaponSystem == null)
            {
                Debug.LogWarning($"[EnemyAI] WeaponSystem not found on {gameObject.name}");
            }

            healthSystem = GetComponent<Core.HealthSystem>();
            if (healthSystem == null)
            {
                Debug.LogWarning($"[EnemyAI] HealthSystem not found on {gameObject.name}");
            }

            // Generate patrol points if none assigned
            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                GeneratePatrolPoints();
            }
        }

        private void Start()
        {
            // Initialize state
            TransitionToState(AIState.Patrol);

            // Apply difficulty scaling
            if (scaleWithPlayerProgress)
            {
                ApplyDifficultyScaling();
            }
        }

        private void Update()
        {
            // Periodic target detection
            if (Time.time - lastDetectionTime >= detectionInterval)
            {
                DetectTargets();
                lastDetectionTime = Time.time;
            }

            // Update AI state machine
            UpdateStateMachine();

            // Update combat
            UpdateCombat();
        }

        private void UpdateStateMachine()
        {
            switch (currentState)
            {
                case AIState.Patrol:
                    UpdatePatrol();
                    break;

                case AIState.Pursue:
                    UpdatePursuit();
                    break;

                case AIState.Attack:
                    UpdateAttack();
                    break;

                case AIState.Flank:
                    UpdateFlank();
                    break;

                case AIState.Evade:
                    UpdateEvasion();
                    break;

                case AIState.Retreat:
                    UpdateRetreat();
                    break;
            }
        }

        private void DetectTargets()
        {
            // Find all potential targets in range
            Collider[] potentialTargets = Physics.OverlapSphere(transform.position, detectionRange, targetLayer);

            float closestDistance = float.MaxValue;
            Transform closestTarget = null;

            foreach (Collider col in potentialTargets)
            {
                // Skip self
                if (col.gameObject == gameObject) continue;

                // Check if target is alive
                Core.HealthSystem targetHealth = col.GetComponent<Core.HealthSystem>();
                if (targetHealth != null && targetHealth.IsDead()) continue;

                // Check if target is player or friendly
                if (col.CompareTag("Player") || col.CompareTag("Friendly"))
                {
                    float distance = Vector3.Distance(transform.position, col.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = col.transform;
                    }
                }
            }

            // Update current target
            if (closestTarget != null)
            {
                currentTarget = closestTarget;

                // Transition to combat state if we have a target
                if (currentState == AIState.Patrol)
                {
                    TransitionToState(AIState.Pursue);
                }
            }
            else if (currentTarget != null && closestDistance > loseTargetRange)
            {
                // Lose target if too far
                currentTarget = null;
                if (currentState != AIState.Patrol)
                {
                    TransitionToState(AIState.Patrol);
                }
            }
        }

        private void UpdatePatrol()
        {
            if (patrolPoints == null || patrolPoints.Length == 0) return;

            Transform targetPoint = patrolPoints[currentPatrolIndex];
            if (targetPoint == null) return;

            // Move towards patrol point
            MoveTowards(targetPoint.position, patrolSpeed);

            // Check if reached patrol point
            float distance = Vector3.Distance(transform.position, targetPoint.position);
            if (distance < 5f)
            {
                // Move to next patrol point
                if (randomPatrol)
                {
                    currentPatrolIndex = Random.Range(0, patrolPoints.Length);
                }
                else
                {
                    currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                }

                // Wait at patrol point
                StartCoroutine(PatrolWaitCoroutine());
            }
        }

        private IEnumerator PatrolWaitCoroutine()
        {
            boatPhysics.SetThrottleInput(0f);
            yield return new WaitForSeconds(patrolWaitTime);
        }

        private void UpdatePursuit()
        {
            if (currentTarget == null)
            {
                TransitionToState(AIState.Patrol);
                return;
            }

            float distance = Vector3.Distance(transform.position, currentTarget.position);

            // Check if in attack range
            if (distance <= attackRange)
            {
                // Transition based on personality
                if (personality == AIPersonality.Tactical && Random.value > 0.5f)
                {
                    TransitionToState(AIState.Flank);
                }
                else
                {
                    TransitionToState(AIState.Attack);
                }
            }
            else
            {
                // Continue pursuing
                MoveTowards(currentTarget.position, pursuitSpeed);
            }
        }

        private void UpdateAttack()
        {
            if (currentTarget == null)
            {
                TransitionToState(AIState.Patrol);
                return;
            }

            float distance = Vector3.Distance(transform.position, currentTarget.position);

            // Maintain attack range
            if (distance > attackRange * 1.2f)
            {
                TransitionToState(AIState.Pursue);
            }
            else if (distance < attackRange * 0.5f && personality != AIPersonality.Aggressive)
            {
                // Back off if too close (except aggressive personality)
                MoveTowards(currentTarget.position, -0.3f);
            }
            else
            {
                // Maintain position and orbit
                OrbitTarget(currentTarget.position, attackRange);
            }

            // Random evasion
            if (enableEvasion && !isEvading && Random.value < evasionChance * Time.deltaTime)
            {
                StartCoroutine(EvasionManeuverCoroutine());
            }
        }

        private void UpdateFlank()
        {
            if (currentTarget == null)
            {
                TransitionToState(AIState.Patrol);
                return;
            }

            // Calculate flank position
            Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
            float angle = Random.value > 0.5f ? flankAngle : -flankAngle;
            Vector3 flankDirection = Quaternion.Euler(0, angle, 0) * directionToTarget;
            flankPosition = currentTarget.position + flankDirection * flankDistance;

            // Move to flank position
            MoveTowards(flankPosition, pursuitSpeed);

            // Check if reached flank position
            if (Vector3.Distance(transform.position, flankPosition) < 10f)
            {
                TransitionToState(AIState.Attack);
            }
        }

        private void UpdateEvasion()
        {
            // Handled by coroutine
        }

        private void UpdateRetreat()
        {
            if (currentTarget == null) return;

            // Move away from target
            Vector3 retreatDirection = (transform.position - currentTarget.position).normalized;
            Vector3 retreatPosition = transform.position + retreatDirection * 50f;

            MoveTowards(retreatPosition, pursuitSpeed);

            // Check health to re-engage
            if (healthSystem != null && healthSystem.GetHealthPercentage() > 0.5f)
            {
                TransitionToState(AIState.Pursue);
            }
        }

        private void UpdateCombat()
        {
            if (currentTarget == null || weaponSystem == null) return;
            if (currentState == AIState.Patrol || currentState == AIState.Retreat) return;

            // Aim at target
            AimAtTarget(currentTarget);

            // Fire weapon
            float timeSinceLastFire = Time.time - lastFireTime;
            if (timeSinceLastFire >= (1f / fireRate))
            {
                // Apply accuracy
                if (Random.value <= fireAccuracy * difficultyMultiplier)
                {
                    weaponSystem.TryFire();
                    lastFireTime = Time.time;
                }
            }
        }

        private void MoveTowards(Vector3 targetPosition, float speedMultiplier)
        {
            if (boatPhysics == null) return;

            // Calculate direction to target
            Vector3 direction = (targetPosition - transform.position).normalized;

            // Calculate steering angle
            Vector3 forward = transform.forward;
            float angle = Vector3.SignedAngle(forward, direction, Vector3.up);

            // Apply steering
            float steeringInput = Mathf.Clamp(angle / 45f, -1f, 1f);
            boatPhysics.SetSteeringInput(steeringInput);

            // Apply throttle
            boatPhysics.SetThrottleInput(Mathf.Abs(speedMultiplier));
        }

        private void OrbitTarget(Vector3 targetPosition, float orbitRadius)
        {
            // Calculate tangent direction for orbiting
            Vector3 toTarget = (targetPosition - transform.position).normalized;
            Vector3 orbitDirection = Vector3.Cross(toTarget, Vector3.up).normalized;

            // Random orbit direction
            if (Random.value > 0.5f)
            {
                orbitDirection = -orbitDirection;
            }

            Vector3 orbitPosition = transform.position + orbitDirection * 10f;
            MoveTowards(orbitPosition, 0.5f);
        }

        private void AimAtTarget(Transform target)
        {
            if (weaponSystem == null) return;

            // Get weapon mount transform
            Transform weaponMount = weaponSystem.transform;

            // Calculate direction to target with lead prediction
            Vector3 targetVelocity = Vector3.zero;
            Rigidbody targetRb = target.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                targetVelocity = targetRb.velocity;
            }

            // Simple lead prediction
            float projectileSpeed = 50f; // Should match weapon system
            float timeToTarget = Vector3.Distance(transform.position, target.position) / projectileSpeed;
            Vector3 predictedPosition = target.position + targetVelocity * timeToTarget;

            // Aim weapon
            Vector3 aimDirection = (predictedPosition - weaponMount.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(aimDirection);
            weaponMount.rotation = Quaternion.Slerp(weaponMount.rotation, targetRotation, Time.deltaTime * 2f);
        }

        private IEnumerator EvasionManeuverCoroutine()
        {
            isEvading = true;
            AIState previousState = currentState;
            currentState = AIState.Evade;

            float elapsed = 0f;
            Vector3 evasionDirection = Random.insideUnitCircle.normalized;

            while (elapsed < evasionDuration)
            {
                // Quick zigzag movement
                float steering = Mathf.Sin(elapsed * 10f);
                boatPhysics.SetSteeringInput(steering);
                boatPhysics.SetThrottleInput(1f);

                elapsed += Time.deltaTime;
                yield return null;
            }

            isEvading = false;
            TransitionToState(previousState);
        }

        private void TransitionToState(AIState newState)
        {
            Debug.Log($"[EnemyAI] {gameObject.name} transitioning from {currentState} to {newState}");
            currentState = newState;

            // State entry actions
            switch (newState)
            {
                case AIState.Retreat:
                    // Check if should retreat based on health
                    if (healthSystem != null && healthSystem.GetHealthPercentage() > 0.3f && personality != AIPersonality.Defensive)
                    {
                        // Don't retreat unless very low health or defensive personality
                        currentState = AIState.Attack;
                    }
                    break;
            }
        }

        private void GeneratePatrolPoints()
        {
            // Generate random patrol points around spawn position
            int numPoints = 4;
            patrolPoints = new Transform[numPoints];
            float radius = 100f;

            for (int i = 0; i < numPoints; i++)
            {
                GameObject patrolPoint = new GameObject($"PatrolPoint_{i}");
                patrolPoint.transform.parent = transform;

                float angle = (i / (float)numPoints) * 360f;
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                    0f,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * radius
                );

                patrolPoint.transform.position = transform.position + offset;
                patrolPoints[i] = patrolPoint.transform;
            }

            Debug.Log($"[EnemyAI] Generated {numPoints} patrol points for {gameObject.name}");
        }

        private void ApplyDifficultyScaling()
        {
            GameManager gameManager = FindAnyObjectByType<GameManager>();
            if (gameManager != null)
            {
                difficultyMultiplier = gameManager.GetDifficultyMultiplier();

                // Scale AI parameters
                fireAccuracy *= difficultyMultiplier;
                fireRate *= difficultyMultiplier;
                detectionRange *= Mathf.Lerp(1f, 1.5f, (difficultyMultiplier - 1f) / 2f);

                Debug.Log($"[EnemyAI] Applied difficulty scaling: {difficultyMultiplier}x");
            }
        }

        // Public methods
        public void SetTarget(Transform target)
        {
            currentTarget = target;
            if (target != null && currentState == AIState.Patrol)
            {
                TransitionToState(AIState.Pursue);
            }
        }

        public AIState GetCurrentState() => currentState;

        public void ForceState(AIState state)
        {
            TransitionToState(state);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            detectionRange = Mathf.Max(1f, detectionRange);
            attackRange = Mathf.Max(1f, attackRange);
            loseTargetRange = Mathf.Max(attackRange, loseTargetRange);
            fireAccuracy = Mathf.Clamp01(fireAccuracy);
            fireRate = Mathf.Max(0.1f, fireRate);
            difficultyMultiplier = Mathf.Max(0.1f, difficultyMultiplier);
        }

        private void OnDrawGizmosSelected()
        {
            // Draw detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Draw patrol points
            if (patrolPoints != null)
            {
                Gizmos.color = Color.blue;
                foreach (Transform point in patrolPoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawWireSphere(point.position, 2f);
                        Gizmos.DrawLine(transform.position, point.position);
                    }
                }
            }

            // Draw current target
            if (currentTarget != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, currentTarget.position);
            }
        }
#endif
    }
}
