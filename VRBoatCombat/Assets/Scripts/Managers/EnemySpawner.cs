using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VRBoatCombat
{
    /// <summary>
    /// Manages enemy boat spawning with difficulty scaling and wave management.
    /// Supports multiple spawn points and enemy types.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private bool autoStart = false;
        [SerializeField] private float spawnInterval = 10f;
        [SerializeField] private int maxEnemies = 10;
        [SerializeField] private float spawnRadius = 100f;

        [Header("Enemy Prefabs")]
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private float[] spawnWeights; // Probability weights for each enemy type

        [Header("Spawn Locations")]
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private bool useRandomSpawnPoints = true;
        [SerializeField] private float minDistanceFromPlayer = 50f;

        [Header("Difficulty Scaling")]
        [SerializeField] private float difficultyMultiplier = 1f;
        [SerializeField] private float spawnRateMultiplier = 1f;
        [SerializeField] private AnimationCurve difficultyScalingCurve = AnimationCurve.Linear(0, 1, 1, 2);

        [Header("Wave System")]
        [SerializeField] private bool enableWaves = false;
        [SerializeField] private int enemiesPerWave = 5;
        [SerializeField] private float timeBetweenWaves = 30f;
        [SerializeField] private int currentWave = 0;

        [Header("Advanced Settings")]
        [SerializeField] private bool enableObjectPooling = true;
        [SerializeField] private LayerMask oceanLayer;
        [SerializeField] private float spawnHeight = 0f;

        // State tracking
        private List<GameObject> activeEnemies = new List<GameObject>();
        private bool isSpawning = false;
        private float lastSpawnTime = 0f;
        private Transform playerTransform;
        private ObjectPooler objectPooler;

        private void Awake()
        {
            // Validate enemy prefabs
            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            {
                Debug.LogError("[EnemySpawner] No enemy prefabs assigned!");
                enabled = false;
                return;
            }

            // Initialize spawn weights if not set
            if (spawnWeights == null || spawnWeights.Length != enemyPrefabs.Length)
            {
                spawnWeights = new float[enemyPrefabs.Length];
                for (int i = 0; i < spawnWeights.Length; i++)
                {
                    spawnWeights[i] = 1f;
                }
            }

            // Generate spawn points if none assigned
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                GenerateSpawnPoints();
            }

            // Get object pooler
            objectPooler = FindObjectOfType<ObjectPooler>();
            if (objectPooler == null && enableObjectPooling)
            {
                Debug.LogWarning("[EnemySpawner] ObjectPooler not found. Object pooling disabled.");
                enableObjectPooling = false;
            }
        }

        private void Start()
        {
            // Find player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("[EnemySpawner] Player not found!");
            }

            // Auto start if configured
            if (autoStart)
            {
                StartSpawning();
            }
        }

        private void Update()
        {
            if (!isSpawning) return;

            // Clean up destroyed enemies from list
            CleanupEnemyList();

            // Check spawn conditions
            if (enableWaves)
            {
                UpdateWaveSpawning();
            }
            else
            {
                UpdateContinuousSpawning();
            }
        }

        private void UpdateContinuousSpawning()
        {
            // Check if we can spawn more enemies
            if (activeEnemies.Count >= maxEnemies) return;

            // Check spawn interval
            float adjustedInterval = spawnInterval / (spawnRateMultiplier * difficultyMultiplier);
            if (Time.time - lastSpawnTime >= adjustedInterval)
            {
                SpawnEnemy();
                lastSpawnTime = Time.time;
            }
        }

        private void UpdateWaveSpawning()
        {
            // Check if all enemies in wave are defeated
            if (activeEnemies.Count == 0)
            {
                // Start next wave after delay
                if (Time.time - lastSpawnTime >= timeBetweenWaves)
                {
                    StartCoroutine(SpawnWaveCoroutine());
                }
            }
        }

        private IEnumerator SpawnWaveCoroutine()
        {
            currentWave++;
            Debug.Log($"[EnemySpawner] Starting Wave {currentWave}");

            int enemiesToSpawn = Mathf.RoundToInt(enemiesPerWave * difficultyMultiplier);

            for (int i = 0; i < enemiesToSpawn; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(0.5f); // Slight delay between spawns
            }

            lastSpawnTime = Time.time;
        }

        private void SpawnEnemy()
        {
            // Select enemy type
            GameObject enemyPrefab = SelectEnemyPrefab();
            if (enemyPrefab == null)
            {
                Debug.LogError("[EnemySpawner] Failed to select enemy prefab!");
                return;
            }

            // Get spawn position
            Vector3 spawnPosition = GetSpawnPosition();

            // Spawn enemy
            GameObject enemy;
            if (enableObjectPooling && objectPooler != null)
            {
                enemy = objectPooler.SpawnFromPool("Enemy", spawnPosition, Quaternion.identity);
            }
            else
            {
                enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            }

            if (enemy == null)
            {
                Debug.LogError("[EnemySpawner] Failed to spawn enemy!");
                return;
            }

            // Configure enemy
            ConfigureEnemy(enemy);

            // Add to active enemies list
            activeEnemies.Add(enemy);

            Debug.Log($"[EnemySpawner] Spawned enemy at {spawnPosition}. Active enemies: {activeEnemies.Count}");
        }

        private GameObject SelectEnemyPrefab()
        {
            // Weighted random selection
            float totalWeight = 0f;
            for (int i = 0; i < spawnWeights.Length; i++)
            {
                totalWeight += spawnWeights[i];
            }

            float randomValue = Random.Range(0f, totalWeight);
            float cumulativeWeight = 0f;

            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                cumulativeWeight += spawnWeights[i];
                if (randomValue <= cumulativeWeight)
                {
                    return enemyPrefabs[i];
                }
            }

            // Fallback to first prefab
            return enemyPrefabs[0];
        }

        private Vector3 GetSpawnPosition()
        {
            Vector3 position = Vector3.zero;
            int maxAttempts = 10;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                // Get base position
                if (useRandomSpawnPoints && spawnPoints.Length > 0)
                {
                    // Random spawn point
                    Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                    position = spawnPoint.position;

                    // Add random offset
                    Vector2 randomOffset = Random.insideUnitCircle * spawnRadius * 0.5f;
                    position += new Vector3(randomOffset.x, 0f, randomOffset.y);
                }
                else if (playerTransform != null)
                {
                    // Random position around player at minimum distance
                    Vector2 randomDirection = Random.insideUnitCircle.normalized;
                    float distance = Random.Range(minDistanceFromPlayer, minDistanceFromPlayer + spawnRadius);
                    position = playerTransform.position + new Vector3(randomDirection.x, 0f, randomDirection.y) * distance;
                }
                else
                {
                    // Fallback: random position in spawn radius
                    Vector2 randomPosition = Random.insideUnitCircle * spawnRadius;
                    position = transform.position + new Vector3(randomPosition.x, 0f, randomPosition.y);
                }

                // Set Y position to spawn height
                position.y = spawnHeight;

                // Check if position is valid (not too close to player)
                if (playerTransform == null || Vector3.Distance(position, playerTransform.position) >= minDistanceFromPlayer)
                {
                    break;
                }

                attempts++;
            }

            return position;
        }

        private void ConfigureEnemy(GameObject enemy)
        {
            // Apply difficulty scaling to enemy AI
            AI.EnemyAI enemyAI = enemy.GetComponent<AI.EnemyAI>();
            if (enemyAI != null)
            {
                // Enemy AI will get difficulty from GameManager
            }

            // Set player as target
            if (playerTransform != null)
            {
                AI.EnemyAI ai = enemy.GetComponent<AI.EnemyAI>();
                if (ai != null)
                {
                    ai.SetTarget(playerTransform);
                }
            }

            // Subscribe to death event
            Core.HealthSystem healthSystem = enemy.GetComponent<Core.HealthSystem>();
            if (healthSystem != null)
            {
                healthSystem.OnDeath.AddListener(() => OnEnemyDeath(enemy));
            }
        }

        private void OnEnemyDeath(GameObject enemy)
        {
            // Remove from active list
            if (activeEnemies.Contains(enemy))
            {
                activeEnemies.Remove(enemy);
            }

            // Notify game manager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnEnemyDestroyedEvent(enemy);
            }

            Debug.Log($"[EnemySpawner] Enemy destroyed. Remaining: {activeEnemies.Count}");
        }

        private void CleanupEnemyList()
        {
            // Remove null or destroyed enemies
            activeEnemies.RemoveAll(enemy => enemy == null);
        }

        private void GenerateSpawnPoints()
        {
            int numPoints = 8;
            spawnPoints = new Transform[numPoints];
            float radius = spawnRadius;

            for (int i = 0; i < numPoints; i++)
            {
                GameObject spawnPoint = new GameObject($"SpawnPoint_{i}");
                spawnPoint.transform.parent = transform;

                float angle = (i / (float)numPoints) * 360f;
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                    spawnHeight,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * radius
                );

                spawnPoint.transform.position = transform.position + offset;
                spawnPoints[i] = spawnPoint.transform;
            }

            Debug.Log($"[EnemySpawner] Generated {numPoints} spawn points");
        }

        // Public methods
        public void StartSpawning()
        {
            isSpawning = true;
            lastSpawnTime = Time.time;
            Debug.Log("[EnemySpawner] Started spawning");
        }

        public void StopSpawning()
        {
            isSpawning = false;
            Debug.Log("[EnemySpawner] Stopped spawning");
        }

        public void ClearAllEnemies()
        {
            foreach (GameObject enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }

            activeEnemies.Clear();
            Debug.Log("[EnemySpawner] Cleared all enemies");
        }

        public void SetDifficultyMultiplier(float multiplier)
        {
            difficultyMultiplier = Mathf.Max(0.1f, multiplier);
        }

        public void SetSpawnRateMultiplier(float multiplier)
        {
            spawnRateMultiplier = Mathf.Max(0.1f, multiplier);
        }

        public void ForceSpawnWave()
        {
            if (enableWaves)
            {
                StartCoroutine(SpawnWaveCoroutine());
            }
        }

        // Public getters
        public int GetActiveEnemyCount() => activeEnemies.Count;
        public int GetCurrentWave() => currentWave;
        public bool IsSpawning() => isSpawning;

#if UNITY_EDITOR
        private void OnValidate()
        {
            spawnInterval = Mathf.Max(0.1f, spawnInterval);
            maxEnemies = Mathf.Max(1, maxEnemies);
            spawnRadius = Mathf.Max(1f, spawnRadius);
            minDistanceFromPlayer = Mathf.Max(1f, minDistanceFromPlayer);
            difficultyMultiplier = Mathf.Max(0.1f, difficultyMultiplier);
            spawnRateMultiplier = Mathf.Max(0.1f, spawnRateMultiplier);
            enemiesPerWave = Mathf.Max(1, enemiesPerWave);
            timeBetweenWaves = Mathf.Max(1f, timeBetweenWaves);
        }

        private void OnDrawGizmosSelected()
        {
            // Draw spawn radius
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, spawnRadius);

            // Draw minimum distance from player
            if (playerTransform != null)
            {
                Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
                Gizmos.DrawWireSphere(playerTransform.position, minDistanceFromPlayer);
            }

            // Draw spawn points
            if (spawnPoints != null)
            {
                Gizmos.color = Color.green;
                foreach (Transform point in spawnPoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawWireSphere(point.position, 2f);
                        Gizmos.DrawLine(transform.position, point.position);
                    }
                }
            }
        }
#endif
    }
}
