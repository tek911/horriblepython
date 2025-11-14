using UnityEngine;

namespace VRBoatCombat.Core
{
    /// <summary>
    /// Manages ocean wave simulation using Perlin noise.
    /// Provides wave height calculations for boat buoyancy and visual effects.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [Header("Wave Parameters")]
        [SerializeField] private float waveHeight = 1.0f;
        [SerializeField] private float waveFrequency = 0.5f;
        [SerializeField] private float waveSpeed = 1.0f;

        [Header("Perlin Noise Settings")]
        [SerializeField] private float noiseScale = 0.1f;
        [SerializeField] private int octaves = 3;
        [SerializeField] [Range(0f, 1f)] private float persistence = 0.5f;
        [SerializeField] private float lacunarity = 2.0f;

        [Header("Wind Direction")]
        [SerializeField] private Vector2 windDirection = new Vector2(1, 0);
        [SerializeField] private float windSpeed = 1.0f;

        [Header("Performance")]
        [SerializeField] private bool enableCaching = true;
        [SerializeField] private float cacheGridSize = 1.0f;
        [SerializeField] private int cacheUpdateFrequency = 5; // Update every N frames

        // Private state
        private float timeOffset = 0f;
        private int frameCount = 0;

        // Cache for performance
        private System.Collections.Generic.Dictionary<Vector2Int, float> heightCache;

        private void Awake()
        {
            if (enableCaching)
            {
                heightCache = new System.Collections.Generic.Dictionary<Vector2Int, float>();
            }

            // Normalize wind direction
            windDirection = windDirection.normalized;
        }

        private void Update()
        {
            // Update time offset for wave animation
            timeOffset += Time.deltaTime * waveSpeed;

            // Clear cache periodically for fresh calculations
            if (enableCaching)
            {
                frameCount++;
                if (frameCount >= cacheUpdateFrequency)
                {
                    heightCache.Clear();
                    frameCount = 0;
                }
            }
        }

        /// <summary>
        /// Calculate wave height at a specific world position
        /// </summary>
        public float GetWaveHeight(float x, float z)
        {
            if (enableCaching)
            {
                // Check cache first
                Vector2Int cacheKey = new Vector2Int(
                    Mathf.RoundToInt(x / cacheGridSize),
                    Mathf.RoundToInt(z / cacheGridSize)
                );

                if (heightCache.TryGetValue(cacheKey, out float cachedHeight))
                {
                    return cachedHeight;
                }

                // Calculate and cache
                float height = CalculateWaveHeight(x, z);
                heightCache[cacheKey] = height;
                return height;
            }
            else
            {
                return CalculateWaveHeight(x, z);
            }
        }

        /// <summary>
        /// Get wave height with normal vector (for visual effects and physics)
        /// </summary>
        public void GetWaveData(float x, float z, out float height, out Vector3 normal)
        {
            height = GetWaveHeight(x, z);

            // Calculate normal by sampling nearby points
            float delta = 0.5f;
            float heightL = GetWaveHeight(x - delta, z);
            float heightR = GetWaveHeight(x + delta, z);
            float heightD = GetWaveHeight(x, z - delta);
            float heightU = GetWaveHeight(x, z + delta);

            // Calculate tangent vectors
            Vector3 tangentX = new Vector3(delta * 2, heightR - heightL, 0);
            Vector3 tangentZ = new Vector3(0, heightU - heightD, delta * 2);

            // Calculate normal
            normal = Vector3.Cross(tangentZ, tangentX).normalized;
        }

        private float CalculateWaveHeight(float x, float z)
        {
            float total = 0f;
            float amplitude = waveHeight;
            float frequency = waveFrequency;

            // Apply wind offset
            Vector2 windOffset = windDirection * windSpeed * timeOffset;
            x += windOffset.x;
            z += windOffset.y;

            // Multi-octave Perlin noise
            for (int i = 0; i < octaves; i++)
            {
                float sampleX = x * frequency * noiseScale;
                float sampleZ = z * frequency * noiseScale;

                // Use Perlin noise
                float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2f - 1f;

                total += perlinValue * amplitude;

                // Update for next octave
                amplitude *= persistence;
                frequency *= lacunarity;
            }

            return total;
        }

        /// <summary>
        /// Get wave velocity at a position (useful for floating objects)
        /// </summary>
        public Vector3 GetWaveVelocity(float x, float z)
        {
            // Sample wave heights at two time points to calculate velocity
            float currentHeight = GetWaveHeight(x, z);

            // Estimate velocity based on wind direction and wave speed
            Vector3 velocity = new Vector3(windDirection.x, 0f, windDirection.y) * windSpeed * waveHeight;

            return velocity;
        }

        /// <summary>
        /// Set wave parameters at runtime
        /// </summary>
        public void SetWaveParameters(float height, float frequency, float speed)
        {
            waveHeight = Mathf.Max(0f, height);
            waveFrequency = Mathf.Max(0.01f, frequency);
            waveSpeed = Mathf.Max(0f, speed);

            if (enableCaching)
            {
                heightCache.Clear();
            }
        }

        /// <summary>
        /// Set wind parameters at runtime
        /// </summary>
        public void SetWindParameters(Vector2 direction, float speed)
        {
            windDirection = direction.normalized;
            windSpeed = Mathf.Max(0f, speed);

            if (enableCaching)
            {
                heightCache.Clear();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Clamp values in editor
            waveHeight = Mathf.Max(0f, waveHeight);
            waveFrequency = Mathf.Max(0.01f, waveFrequency);
            waveSpeed = Mathf.Max(0f, waveSpeed);
            noiseScale = Mathf.Max(0.001f, noiseScale);
            octaves = Mathf.Clamp(octaves, 1, 8);
            lacunarity = Mathf.Max(1f, lacunarity);
            windSpeed = Mathf.Max(0f, windSpeed);
            cacheGridSize = Mathf.Max(0.1f, cacheGridSize);
            cacheUpdateFrequency = Mathf.Max(1, cacheUpdateFrequency);

            if (windDirection.sqrMagnitude > 0.01f)
            {
                windDirection = windDirection.normalized;
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw wave preview in editor
            Gizmos.color = Color.cyan;
            int gridSize = 20;
            float spacing = 2f;
            Vector3 origin = transform.position;

            for (int x = 0; x < gridSize; x++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    float worldX = origin.x + (x - gridSize / 2) * spacing;
                    float worldZ = origin.z + (z - gridSize / 2) * spacing;

                    float height = GetWaveHeight(worldX, worldZ);
                    Vector3 point = new Vector3(worldX, origin.y + height, worldZ);

                    Gizmos.DrawWireSphere(point, 0.1f);

                    // Draw connections
                    if (x < gridSize - 1)
                    {
                        float nextHeight = GetWaveHeight(worldX + spacing, worldZ);
                        Vector3 nextPoint = new Vector3(worldX + spacing, origin.y + nextHeight, worldZ);
                        Gizmos.DrawLine(point, nextPoint);
                    }

                    if (z < gridSize - 1)
                    {
                        float nextHeight = GetWaveHeight(worldX, worldZ + spacing);
                        Vector3 nextPoint = new Vector3(worldX, origin.y + nextHeight, worldZ + spacing);
                        Gizmos.DrawLine(point, nextPoint);
                    }
                }
            }

            // Draw wind direction
            Gizmos.color = Color.yellow;
            Vector3 windStart = origin;
            Vector3 windEnd = origin + new Vector3(windDirection.x, 0f, windDirection.y) * 5f;
            Gizmos.DrawLine(windStart, windEnd);
            Gizmos.DrawSphere(windEnd, 0.3f);
        }
#endif
    }
}
