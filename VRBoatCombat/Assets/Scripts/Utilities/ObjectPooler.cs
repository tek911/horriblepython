using UnityEngine;
using System.Collections.Generic;

namespace VRBoatCombat
{
    /// <summary>
    /// Object pooling system for performance optimization.
    /// Manages pools of frequently instantiated objects like projectiles and effects.
    /// </summary>
    public class ObjectPooler : MonoBehaviour
    {
        [System.Serializable]
        public class Pool
        {
            public string tag;
            public GameObject prefab;
            public int size;
            public bool expandable = true;
        }

        [Header("Pool Configuration")]
        [SerializeField] private List<Pool> pools = new List<Pool>();
        [SerializeField] private bool createPoolsOnAwake = true;

        [Header("Performance")]
        [SerializeField] private int maxPoolSize = 1000;
        [SerializeField] private bool enablePoolPruning = true;
        [SerializeField] private float pruneInterval = 30f;
        [SerializeField] private int pruneThreshold = 50; // Prune if pool exceeds this

        // Dictionary for fast pool lookup
        private Dictionary<string, Queue<GameObject>> poolDictionary;
        private Dictionary<string, Transform> poolParents;
        private Dictionary<string, Pool> poolConfigs;
        private float lastPruneTime = 0f;

        private void Awake()
        {
            // Initialize dictionaries
            poolDictionary = new Dictionary<string, Queue<GameObject>>();
            poolParents = new Dictionary<string, Transform>();
            poolConfigs = new Dictionary<string, Pool>();

            // Create pools on awake if configured
            if (createPoolsOnAwake)
            {
                CreatePools();
            }
        }

        private void Update()
        {
            // Periodic pool pruning
            if (enablePoolPruning && Time.time - lastPruneTime >= pruneInterval)
            {
                PrunePools();
                lastPruneTime = Time.time;
            }
        }

        /// <summary>
        /// Create all configured pools
        /// </summary>
        public void CreatePools()
        {
            foreach (Pool pool in pools)
            {
                CreatePool(pool);
            }

            Debug.Log($"[ObjectPooler] Created {pools.Count} pools");
        }

        /// <summary>
        /// Create a specific pool
        /// </summary>
        private void CreatePool(Pool pool)
        {
            if (string.IsNullOrEmpty(pool.tag))
            {
                Debug.LogError("[ObjectPooler] Pool tag cannot be empty!");
                return;
            }

            if (pool.prefab == null)
            {
                Debug.LogError($"[ObjectPooler] Pool '{pool.tag}' has no prefab assigned!");
                return;
            }

            // Create parent object for organization
            Transform parent = new GameObject($"Pool_{pool.tag}").transform;
            parent.SetParent(transform);
            poolParents[pool.tag] = parent;

            // Create queue
            Queue<GameObject> objectQueue = new Queue<GameObject>();

            // Instantiate initial objects
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = CreatePooledObject(pool.prefab, parent);
                objectQueue.Enqueue(obj);
            }

            // Register pool
            poolDictionary[pool.tag] = objectQueue;
            poolConfigs[pool.tag] = pool;

            Debug.Log($"[ObjectPooler] Created pool '{pool.tag}' with {pool.size} objects");
        }

        /// <summary>
        /// Spawn object from pool
        /// </summary>
        public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPooler] Pool with tag '{tag}' doesn't exist!");
                return null;
            }

            GameObject objectToSpawn;
            Queue<GameObject> pool = poolDictionary[tag];

            // Check if pool has available objects
            if (pool.Count > 0)
            {
                objectToSpawn = pool.Dequeue();
            }
            else
            {
                // Expand pool if allowed
                Pool poolConfig = poolConfigs[tag];
                if (poolConfig.expandable && poolDictionary[tag].Count < maxPoolSize)
                {
                    objectToSpawn = CreatePooledObject(poolConfig.prefab, poolParents[tag]);
                    Debug.Log($"[ObjectPooler] Expanded pool '{tag}' (current size: {poolDictionary[tag].Count + 1})");
                }
                else
                {
                    Debug.LogWarning($"[ObjectPooler] Pool '{tag}' exhausted and cannot expand!");
                    return null;
                }
            }

            // Configure object
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
            objectToSpawn.SetActive(true);

            // Call OnObjectSpawn on IPooledObject interface
            IPooledObject pooledObj = objectToSpawn.GetComponent<IPooledObject>();
            if (pooledObj != null)
            {
                pooledObj.OnObjectSpawn();
            }

            return objectToSpawn;
        }

        /// <summary>
        /// Return object to pool
        /// </summary>
        public void ReturnToPool(GameObject obj)
        {
            if (obj == null) return;

            // Find which pool this object belongs to
            string poolTag = FindPoolTag(obj);
            if (string.IsNullOrEmpty(poolTag))
            {
                Debug.LogWarning($"[ObjectPooler] Object {obj.name} doesn't belong to any pool!");
                Destroy(obj);
                return;
            }

            // Reset and deactivate object
            obj.SetActive(false);
            obj.transform.SetParent(poolParents[poolTag]);

            // Return to pool
            poolDictionary[poolTag].Enqueue(obj);
        }

        /// <summary>
        /// Return object to pool after delay
        /// </summary>
        public void ReturnToPool(GameObject obj, float delay)
        {
            StartCoroutine(ReturnToPoolDelayed(obj, delay));
        }

        private System.Collections.IEnumerator ReturnToPoolDelayed(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            ReturnToPool(obj);
        }

        private GameObject CreatePooledObject(GameObject prefab, Transform parent)
        {
            GameObject obj = Instantiate(prefab, parent);
            obj.SetActive(false);

            // Add PooledObject component if not present
            if (obj.GetComponent<PooledObject>() == null)
            {
                obj.AddComponent<PooledObject>();
            }

            return obj;
        }

        private string FindPoolTag(GameObject obj)
        {
            PooledObject pooledObj = obj.GetComponent<PooledObject>();
            if (pooledObj != null && !string.IsNullOrEmpty(pooledObj.poolTag))
            {
                return pooledObj.poolTag;
            }

            // Fallback: search through all pools
            foreach (var kvp in poolParents)
            {
                if (obj.transform.parent == kvp.Value)
                {
                    return kvp.Key;
                }
            }

            return null;
        }

        private void PrunePools()
        {
            foreach (var kvp in poolDictionary)
            {
                string tag = kvp.Key;
                Queue<GameObject> pool = kvp.Value;

                if (pool.Count > pruneThreshold)
                {
                    int toPrune = pool.Count - pruneThreshold;
                    for (int i = 0; i < toPrune; i++)
                    {
                        if (pool.Count > 0)
                        {
                            GameObject obj = pool.Dequeue();
                            if (obj != null)
                            {
                                Destroy(obj);
                            }
                        }
                    }

                    Debug.Log($"[ObjectPooler] Pruned {toPrune} objects from pool '{tag}'");
                }
            }
        }

        /// <summary>
        /// Add a new pool at runtime
        /// </summary>
        public void AddPool(string tag, GameObject prefab, int size, bool expandable = true)
        {
            if (poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPooler] Pool '{tag}' already exists!");
                return;
            }

            Pool newPool = new Pool
            {
                tag = tag,
                prefab = prefab,
                size = size,
                expandable = expandable
            };

            pools.Add(newPool);
            CreatePool(newPool);
        }

        /// <summary>
        /// Clear all pools
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var kvp in poolDictionary)
            {
                Queue<GameObject> pool = kvp.Value;
                while (pool.Count > 0)
                {
                    GameObject obj = pool.Dequeue();
                    if (obj != null)
                    {
                        Destroy(obj);
                    }
                }
            }

            foreach (var kvp in poolParents)
            {
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value.gameObject);
                }
            }

            poolDictionary.Clear();
            poolParents.Clear();
            poolConfigs.Clear();

            Debug.Log("[ObjectPooler] Cleared all pools");
        }

        // Public getters
        public int GetPoolSize(string tag)
        {
            if (poolDictionary.ContainsKey(tag))
            {
                return poolDictionary[tag].Count;
            }
            return 0;
        }

        public bool HasPool(string tag)
        {
            return poolDictionary.ContainsKey(tag);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            maxPoolSize = Mathf.Max(1, maxPoolSize);
            pruneInterval = Mathf.Max(1f, pruneInterval);
            pruneThreshold = Mathf.Max(1, pruneThreshold);
        }
#endif
    }

    /// <summary>
    /// Interface for pooled objects
    /// </summary>
    public interface IPooledObject
    {
        void OnObjectSpawn();
    }

    /// <summary>
    /// Component added to pooled objects to track their pool
    /// </summary>
    public class PooledObject : MonoBehaviour
    {
        public string poolTag;
    }
}
