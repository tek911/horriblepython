using UnityEngine;
using System.Collections;

namespace VRBoatCombat.Weapons
{
    /// <summary>
    /// Grappling hook system for boat-to-boat connections.
    /// Handles hook launching, rope physics, and zipline traversal.
    /// </summary>
    public class GrappleHook : MonoBehaviour
    {
        [Header("Grapple Settings")]
        [SerializeField] private float maxGrappleDistance = 50f;
        [SerializeField] private float grappleSpeed = 30f;
        [SerializeField] private float grappleCooldown = 2f;
        [SerializeField] private LayerMask grappleableLayer;

        [Header("Rope Settings")]
        [SerializeField] private LineRenderer ropeRenderer;
        [SerializeField] private int ropeSegments = 25;
        [SerializeField] private float ropeWidth = 0.1f;
        [SerializeField] private Material ropeMaterial;

        [Header("Rope Physics")]
        [SerializeField] private float ropeStiffness = 100f;
        [SerializeField] private float ropeDamping = 5f;
        [SerializeField] private bool enableRopePhysics = true;

        [Header("Hook Prefab")]
        [SerializeField] private GameObject hookPrefab;
        [SerializeField] private Transform hookLaunchPoint;

        [Header("Audio")]
        [SerializeField] private AudioSource launchSound;
        [SerializeField] private AudioSource attachSound;
        [SerializeField] private AudioSource detachSound;

        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem attachEffect;
        [SerializeField] private ParticleSystem detachEffect;

        // State
        private bool isGrappleActive = false;
        private bool isGrappling = false;
        private GameObject activeHook;
        private Transform grapplePoint;
        private Rigidbody playerBoatRigidbody;
        private Rigidbody targetBoatRigidbody;
        private float lastGrappleTime;
        private SpringJoint springJoint;

        // Rope simulation
        private Vector3[] ropePositions;

        private void Awake()
        {
            // Get player boat rigidbody
            playerBoatRigidbody = GetComponentInParent<Rigidbody>();
            if (playerBoatRigidbody == null)
            {
                Debug.LogError($"[GrappleHook] No Rigidbody found in parent of {gameObject.name}!");
            }

            // Setup rope renderer
            if (ropeRenderer == null)
            {
                ropeRenderer = gameObject.AddComponent<LineRenderer>();
            }

            ConfigureRopeRenderer();

            // Initialize rope positions array
            ropePositions = new Vector3[ropeSegments];
        }

        private void ConfigureRopeRenderer()
        {
            if (ropeRenderer == null) return;

            ropeRenderer.positionCount = ropeSegments;
            ropeRenderer.startWidth = ropeWidth;
            ropeRenderer.endWidth = ropeWidth;
            ropeRenderer.material = ropeMaterial;
            ropeRenderer.enabled = false;

            // Use simple line for performance
            ropeRenderer.useWorldSpace = true;
            ropeRenderer.numCapVertices = 2;
            ropeRenderer.numCornerVertices = 2;
        }

        private void Update()
        {
            // Update rope visualization if grapple is active
            if (isGrappleActive && grapplePoint != null)
            {
                UpdateRopeVisualization();
            }
        }

        private void FixedUpdate()
        {
            // Update rope physics if enabled
            if (isGrappleActive && enableRopePhysics && grapplePoint != null)
            {
                UpdateRopePhysics();
            }
        }

        /// <summary>
        /// Launch grappling hook towards target
        /// </summary>
        public bool LaunchGrapple(Vector3 targetPosition)
        {
            // Check cooldown
            if (Time.time - lastGrappleTime < grappleCooldown)
            {
                Debug.Log("[GrappleHook] Grapple on cooldown");
                return false;
            }

            // Check if already grappling
            if (isGrappling)
            {
                Debug.Log("[GrappleHook] Already grappling");
                return false;
            }

            // Check distance
            float distance = Vector3.Distance(hookLaunchPoint.position, targetPosition);
            if (distance > maxGrappleDistance)
            {
                Debug.Log($"[GrappleHook] Target too far: {distance}m (max: {maxGrappleDistance}m)");
                return false;
            }

            // Start grapple coroutine
            StartCoroutine(GrappleCoroutine(targetPosition));
            return true;
        }

        /// <summary>
        /// Launch grapple using raycast from controller
        /// </summary>
        public bool LaunchGrappleRaycast(Transform aimTransform)
        {
            Ray ray = new Ray(aimTransform.position, aimTransform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxGrappleDistance, grappleableLayer))
            {
                Debug.Log($"[GrappleHook] Grapple target found: {hit.collider.gameObject.name}");
                return LaunchGrapple(hit.point);
            }
            else
            {
                Debug.Log("[GrappleHook] No valid grapple target found");
                return false;
            }
        }

        private IEnumerator GrappleCoroutine(Vector3 targetPosition)
        {
            isGrappling = true;
            lastGrappleTime = Time.time;

            // Play launch sound
            if (launchSound != null)
            {
                launchSound.Play();
            }

            // Spawn hook projectile
            if (hookPrefab != null)
            {
                activeHook = Instantiate(hookPrefab, hookLaunchPoint.position, Quaternion.identity);
                Rigidbody hookRb = activeHook.GetComponent<Rigidbody>();

                if (hookRb != null)
                {
                    Vector3 direction = (targetPosition - hookLaunchPoint.position).normalized;
                    hookRb.velocity = direction * grappleSpeed;
                }
            }

            // Enable rope renderer
            ropeRenderer.enabled = true;

            // Simulate hook flight
            float travelTime = Vector3.Distance(hookLaunchPoint.position, targetPosition) / grappleSpeed;
            float elapsed = 0f;

            while (elapsed < travelTime)
            {
                elapsed += Time.deltaTime;

                // Update rope from launch point to hook
                if (activeHook != null)
                {
                    DrawSimpleRope(hookLaunchPoint.position, activeHook.transform.position);
                }

                yield return null;
            }

            // Check if hook hit something
            Collider[] hits = Physics.OverlapSphere(targetPosition, 1f, grappleableLayer);
            if (hits.Length > 0)
            {
                AttachGrapple(hits[0].transform, targetPosition);
            }
            else
            {
                DetachGrapple();
            }

            isGrappling = false;
        }

        private void AttachGrapple(Transform target, Vector3 attachPoint)
        {
            isGrappleActive = true;
            grapplePoint = target;

            // Try to get target rigidbody
            targetBoatRigidbody = target.GetComponentInParent<Rigidbody>();

            Debug.Log($"[GrappleHook] Grapple attached to {target.name}");

            // Play attach sound
            if (attachSound != null)
            {
                attachSound.Play();
            }

            // Spawn attach effect
            if (attachEffect != null)
            {
                Instantiate(attachEffect, attachPoint, Quaternion.identity);
            }

            // Create spring joint for rope physics
            if (enableRopePhysics && playerBoatRigidbody != null && targetBoatRigidbody != null)
            {
                CreateRopeConstraint();
            }

            // Initialize rope positions
            InitializeRope();
        }

        /// <summary>
        /// Detach the grappling hook
        /// </summary>
        public void DetachGrapple()
        {
            if (!isGrappleActive) return;

            isGrappleActive = false;
            grapplePoint = null;
            targetBoatRigidbody = null;

            // Destroy hook
            if (activeHook != null)
            {
                Destroy(activeHook);
            }

            // Disable rope renderer
            ropeRenderer.enabled = false;

            // Play detach sound
            if (detachSound != null)
            {
                detachSound.Play();
            }

            // Spawn detach effect
            if (detachEffect != null && hookLaunchPoint != null)
            {
                Instantiate(detachEffect, hookLaunchPoint.position, Quaternion.identity);
            }

            // Remove spring joint
            if (springJoint != null)
            {
                Destroy(springJoint);
                springJoint = null;
            }

            Debug.Log("[GrappleHook] Grapple detached");
        }

        private void CreateRopeConstraint()
        {
            if (playerBoatRigidbody == null || targetBoatRigidbody == null) return;

            // Remove existing joint
            if (springJoint != null)
            {
                Destroy(springJoint);
            }

            // Create spring joint
            springJoint = playerBoatRigidbody.gameObject.AddComponent<SpringJoint>();
            springJoint.connectedBody = targetBoatRigidbody;
            springJoint.autoConfigureConnectedAnchor = false;
            springJoint.anchor = playerBoatRigidbody.transform.InverseTransformPoint(hookLaunchPoint.position);
            springJoint.connectedAnchor = targetBoatRigidbody.transform.InverseTransformPoint(grapplePoint.position);

            // Configure spring parameters
            springJoint.spring = ropeStiffness;
            springJoint.damper = ropeDamping;
            springJoint.maxDistance = Vector3.Distance(hookLaunchPoint.position, grapplePoint.position);
            springJoint.minDistance = 0f;
        }

        private void InitializeRope()
        {
            if (grapplePoint == null) return;

            // Initialize rope segments along straight line
            Vector3 start = hookLaunchPoint.position;
            Vector3 end = grapplePoint.position;

            for (int i = 0; i < ropeSegments; i++)
            {
                float t = i / (float)(ropeSegments - 1);
                ropePositions[i] = Vector3.Lerp(start, end, t);
            }
        }

        private void UpdateRopeVisualization()
        {
            if (grapplePoint == null || ropeRenderer == null) return;

            // Update rope positions
            ropeRenderer.SetPositions(ropePositions);
        }

        private void UpdateRopePhysics()
        {
            if (grapplePoint == null) return;

            Vector3 start = hookLaunchPoint.position;
            Vector3 end = grapplePoint.position;

            // Simple catenary curve simulation
            for (int i = 1; i < ropeSegments - 1; i++)
            {
                float t = i / (float)(ropeSegments - 1);

                // Target position (straight line)
                Vector3 targetPos = Vector3.Lerp(start, end, t);

                // Add gravity sag
                float sag = Mathf.Sin(t * Mathf.PI) * 2f; // Parabolic sag
                targetPos += Vector3.down * sag;

                // Smooth towards target
                ropePositions[i] = Vector3.Lerp(ropePositions[i], targetPos, Time.fixedDeltaTime * 5f);
            }

            // Force endpoints
            ropePositions[0] = start;
            ropePositions[ropeSegments - 1] = end;
        }

        private void DrawSimpleRope(Vector3 start, Vector3 end)
        {
            if (ropeRenderer == null) return;

            for (int i = 0; i < ropeSegments; i++)
            {
                float t = i / (float)(ropeSegments - 1);
                ropePositions[i] = Vector3.Lerp(start, end, t);
            }

            ropeRenderer.SetPositions(ropePositions);
        }

        // Public getters
        public bool IsGrappleActive() => isGrappleActive;
        public bool IsGrappling() => isGrappling;
        public Transform GetGrapplePoint() => grapplePoint;
        public float GetGrappleDistance()
        {
            if (grapplePoint != null && hookLaunchPoint != null)
            {
                return Vector3.Distance(hookLaunchPoint.position, grapplePoint.position);
            }
            return 0f;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            maxGrappleDistance = Mathf.Max(1f, maxGrappleDistance);
            grappleSpeed = Mathf.Max(1f, grappleSpeed);
            grappleCooldown = Mathf.Max(0f, grappleCooldown);
            ropeSegments = Mathf.Clamp(ropeSegments, 5, 100);
            ropeWidth = Mathf.Max(0.01f, ropeWidth);
            ropeStiffness = Mathf.Max(0f, ropeStiffness);
            ropeDamping = Mathf.Max(0f, ropeDamping);
        }

        private void OnDrawGizmosSelected()
        {
            if (hookLaunchPoint != null)
            {
                // Draw max grapple distance
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(hookLaunchPoint.position, maxGrappleDistance);

                // Draw launch point
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(hookLaunchPoint.position, 0.3f);

                // Draw current grapple connection
                if (isGrappleActive && grapplePoint != null)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(hookLaunchPoint.position, grapplePoint.position);
                }
            }
        }
#endif

        private void OnDestroy()
        {
            // Clean up spring joint
            if (springJoint != null)
            {
                Destroy(springJoint);
            }
        }
    }
}
