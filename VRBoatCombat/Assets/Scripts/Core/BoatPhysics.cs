using UnityEngine;

namespace VRBoatCombat.Core
{
    /// <summary>
    /// Handles boat physics including buoyancy, wave interaction, and movement.
    /// Uses Rigidbody for physics-based movement with custom buoyancy calculations.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class BoatPhysics : MonoBehaviour
    {
        [Header("Boat Properties")]
        [SerializeField] private float mass = 1000f;
        [SerializeField] private float drag = 0.5f;
        [SerializeField] private float angularDrag = 2.0f;

        [Header("Movement Settings")]
        [SerializeField] private float maxSpeed = 20f;
        [SerializeField] private float acceleration = 5f;
        [SerializeField] private float turnSpeed = 30f;
        [SerializeField] private float turnSpeedAtSpeed = 15f; // Turning is harder at high speed

        [Header("Buoyancy Settings")]
        [SerializeField] private Transform[] buoyancyPoints; // Points where buoyancy is calculated
        [SerializeField] private float buoyancyForce = 15f;
        [SerializeField] private float waterLevel = 0f;
        [SerializeField] private float waterDensity = 1000f;
        [SerializeField] private float dampingCoefficient = 5f;

        [Header("Wave Interaction")]
        [SerializeField] private bool useWaveSystem = true;
        [SerializeField] private float waveHeightMultiplier = 1.0f;

        [Header("Momentum Settings")]
        [SerializeField] private float momentumDecay = 0.95f; // How quickly boat slows when no throttle
        [SerializeField] private float lateralDrag = 2.0f; // Resistance to sideways movement

        [Header("Tilt Settings")]
        [SerializeField] private float maxRollAngle = 25f;
        [SerializeField] private float rollSpeed = 2f;
        [SerializeField] private float maxPitchAngle = 15f;
        [SerializeField] private float pitchSpeed = 1.5f;

        // Component references
        private Rigidbody rb;
        private WaveManager waveManager;

        // Input state
        private float steeringInput = 0f;
        private float throttleInput = 0f;

        // Physics state
        private float currentSpeed = 0f;
        private float targetRoll = 0f;
        private float targetPitch = 0f;

        private void Awake()
        {
            // Get or add Rigidbody
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                Debug.LogWarning($"[BoatPhysics] Rigidbody was missing on {gameObject.name}, added automatically");
            }

            // Configure Rigidbody
            rb.mass = mass;
            rb.drag = drag;
            rb.angularDrag = angularDrag;
            rb.useGravity = false; // We'll handle gravity through buoyancy
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Validate buoyancy points
            if (buoyancyPoints == null || buoyancyPoints.Length == 0)
            {
                Debug.LogError($"[BoatPhysics] No buoyancy points assigned on {gameObject.name}. Boat will not float correctly!");
            }
        }

        private void Start()
        {
            // Find wave manager in scene
            waveManager = FindObjectOfType<WaveManager>();
            if (waveManager == null && useWaveSystem)
            {
                Debug.LogWarning("[BoatPhysics] WaveManager not found in scene. Wave interaction disabled.");
                useWaveSystem = false;
            }
        }

        private void FixedUpdate()
        {
            // Apply buoyancy forces
            ApplyBuoyancy();

            // Apply movement forces
            ApplyMovement();

            // Apply turning
            ApplyTurning();

            // Apply lateral drag (reduces sideways sliding)
            ApplyLateralDrag();

            // Update tilt based on movement
            UpdateTilt();

            // Update current speed
            currentSpeed = rb.velocity.magnitude;
        }

        private void ApplyBuoyancy()
        {
            if (buoyancyPoints == null || buoyancyPoints.Length == 0) return;

            foreach (Transform buoyancyPoint in buoyancyPoints)
            {
                if (buoyancyPoint == null) continue;

                // Get height at this buoyancy point (from wave system or flat water)
                float waveHeight = GetWaveHeightAtPosition(buoyancyPoint.position);
                float waterHeight = waterLevel + waveHeight * waveHeightMultiplier;

                // Calculate submersion depth
                float depth = waterHeight - buoyancyPoint.position.y;

                if (depth > 0)
                {
                    // Calculate buoyancy force (Archimedes principle)
                    float displacementVolume = depth; // Simplified - assumes point represents volume
                    Vector3 buoyancyForceVector = Vector3.up * waterDensity * displacementVolume * buoyancyForce;

                    // Apply force at buoyancy point
                    rb.AddForceAtPosition(buoyancyForceVector, buoyancyPoint.position, ForceMode.Force);

                    // Apply damping force (resistance to vertical movement in water)
                    Vector3 pointVelocity = rb.GetPointVelocity(buoyancyPoint.position);
                    Vector3 dampingForce = -pointVelocity * dampingCoefficient * depth;
                    rb.AddForceAtPosition(dampingForce, buoyancyPoint.position, ForceMode.Force);
                }
            }

            // Apply gravity manually since we disabled it on rigidbody
            rb.AddForce(Physics.gravity * rb.mass, ForceMode.Force);
        }

        private void ApplyMovement()
        {
            if (Mathf.Approximately(throttleInput, 0f))
            {
                // Apply momentum decay when no throttle
                rb.velocity *= momentumDecay;
                return;
            }

            // Calculate forward force based on throttle
            Vector3 forwardForce = transform.forward * throttleInput * acceleration * rb.mass;

            // Limit to max speed
            if (currentSpeed < maxSpeed)
            {
                rb.AddForce(forwardForce, ForceMode.Force);
            }
            else
            {
                // Maintain max speed but don't exceed it
                Vector3 currentForward = Vector3.Project(rb.velocity, transform.forward);
                if (currentForward.magnitude > maxSpeed)
                {
                    rb.velocity = currentForward.normalized * maxSpeed + Vector3.Project(rb.velocity, transform.right);
                }
            }
        }

        private void ApplyTurning()
        {
            if (Mathf.Approximately(steeringInput, 0f)) return;

            // Calculate turn rate based on speed (harder to turn at high speed)
            float speedFactor = Mathf.InverseLerp(0f, maxSpeed, currentSpeed);
            float currentTurnSpeed = Mathf.Lerp(turnSpeed, turnSpeedAtSpeed, speedFactor);

            // Apply torque for turning
            float turnTorque = steeringInput * currentTurnSpeed * Time.fixedDeltaTime;
            rb.AddTorque(transform.up * turnTorque, ForceMode.VelocityChange);
        }

        private void ApplyLateralDrag()
        {
            // Calculate lateral (sideways) velocity
            Vector3 lateralVelocity = Vector3.Project(rb.velocity, transform.right);

            // Apply drag force to reduce sideways movement
            Vector3 lateralDragForce = -lateralVelocity * lateralDrag;
            rb.AddForce(lateralDragForce, ForceMode.Force);
        }

        private void UpdateTilt()
        {
            // Calculate target roll based on steering (boat leans into turns)
            targetRoll = -steeringInput * maxRollAngle;

            // Calculate target pitch based on throttle (boat pitches up when accelerating)
            targetPitch = -throttleInput * maxPitchAngle * 0.5f;

            // Get current rotation
            Vector3 currentEuler = transform.rotation.eulerAngles;
            float currentRoll = currentEuler.z > 180 ? currentEuler.z - 360 : currentEuler.z;
            float currentPitch = currentEuler.x > 180 ? currentEuler.x - 360 : currentEuler.x;

            // Smoothly interpolate to target tilt
            float newRoll = Mathf.Lerp(currentRoll, targetRoll, Time.fixedDeltaTime * rollSpeed);
            float newPitch = Mathf.Lerp(currentPitch, targetPitch, Time.fixedDeltaTime * pitchSpeed);

            // Apply rotation while preserving yaw
            Quaternion targetRotation = Quaternion.Euler(newPitch, currentEuler.y, newRoll);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rollSpeed));
        }

        private float GetWaveHeightAtPosition(Vector3 position)
        {
            if (!useWaveSystem || waveManager == null)
            {
                return 0f;
            }

            return waveManager.GetWaveHeight(position.x, position.z);
        }

        // Public methods for input
        public void SetSteeringInput(float input)
        {
            steeringInput = Mathf.Clamp(input, -1f, 1f);
        }

        public void SetThrottleInput(float input)
        {
            throttleInput = Mathf.Clamp01(input);
        }

        // Public getters
        public float GetCurrentSpeed() => currentSpeed;
        public float GetMaxSpeed() => maxSpeed;
        public float GetSpeedPercentage() => currentSpeed / maxSpeed;
        public Vector3 GetVelocity() => rb.velocity;
        public Rigidbody GetRigidbody() => rb;

        // Public methods for external forces (e.g., collisions, explosions)
        public void AddImpactForce(Vector3 force, Vector3 position)
        {
            rb.AddForceAtPosition(force, position, ForceMode.Impulse);
        }

        public void AddExplosionForce(float explosionForce, Vector3 explosionPosition, float explosionRadius)
        {
            rb.AddExplosionForce(explosionForce, explosionPosition, explosionRadius, 3.0f, ForceMode.Impulse);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Clamp values in editor
            mass = Mathf.Max(1f, mass);
            drag = Mathf.Max(0f, drag);
            angularDrag = Mathf.Max(0f, angularDrag);
            maxSpeed = Mathf.Max(0.1f, maxSpeed);
            acceleration = Mathf.Max(0f, acceleration);
            turnSpeed = Mathf.Max(0f, turnSpeed);
            buoyancyForce = Mathf.Max(0f, buoyancyForce);
            dampingCoefficient = Mathf.Max(0f, dampingCoefficient);
            momentumDecay = Mathf.Clamp01(momentumDecay);
            lateralDrag = Mathf.Max(0f, lateralDrag);
        }

        private void OnDrawGizmos()
        {
            // Visualize buoyancy points
            if (buoyancyPoints != null)
            {
                Gizmos.color = Color.cyan;
                foreach (Transform point in buoyancyPoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawWireSphere(point.position, 0.2f);
                    }
                }
            }

            // Draw water level
            Gizmos.color = new Color(0, 0.5f, 1f, 0.3f);
            Vector3 waterPlanePos = transform.position;
            waterPlanePos.y = waterLevel;
            Gizmos.DrawCube(waterPlanePos, new Vector3(5f, 0.1f, 5f));
        }
#endif
    }
}
