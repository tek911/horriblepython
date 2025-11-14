using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using VRBoatCombat.Weapons;

namespace VRBoatCombat.Core
{
    /// <summary>
    /// Handles VR controller input for boat steering and weapon control.
    /// Left controller: Steering wheel interaction with grab mechanics
    /// Right controller: Weapon aiming and firing
    /// </summary>
    [RequireComponent(typeof(BoatPhysics))]
    public class VRBoatController : MonoBehaviour
    {
        [Header("Controller References")]
        [SerializeField] private XRController leftController;
        [SerializeField] private XRController rightController;
        [SerializeField] private XRGrabInteractable steeringWheelInteractable;

        [Header("Steering Settings")]
        [SerializeField] private float steeringSpeed = 2.0f;
        [SerializeField] private float maxSteeringAngle = 45f;
        [SerializeField] private float steeringDeadzone = 0.1f;
        [SerializeField] private Transform steeringWheelVisual;

        [Header("Throttle Settings")]
        [SerializeField] private float throttleSpeed = 1.5f;
        [SerializeField] private float maxThrottle = 1.0f;
        [SerializeField] private float throttlePushThreshold = 0.3f;

        [Header("Weapon Control")]
        [SerializeField] private Transform weaponMount;
        [SerializeField] private float weaponRotationSpeed = 3.0f;
        [SerializeField] private float maxWeaponPitch = 30f;
        [SerializeField] private float maxWeaponYaw = 60f;

        [Header("Haptic Feedback")]
        [SerializeField] private float steeringHapticStrength = 0.3f;
        [SerializeField] private float fireHapticStrength = 0.8f;
        [SerializeField] private float fireHapticDuration = 0.1f;

        // Private state
        private BoatPhysics boatPhysics;
        private WeaponSystem weaponSystem;
        private bool isSteeringGrabbed = false;
        private float currentSteeringInput = 0f;
        private float currentThrottle = 0f;
        private Vector3 initialSteeringPosition;
        private Quaternion initialSteeringRotation;

        // Input device references
        private InputDevice leftDevice;
        private InputDevice rightDevice;

        private void Awake()
        {
            // Validate required components
            boatPhysics = GetComponent<BoatPhysics>();
            if (boatPhysics == null)
            {
                Debug.LogError($"[VRBoatController] BoatPhysics component not found on {gameObject.name}");
            }

            weaponSystem = GetComponentInChildren<WeaponSystem>();
            if (weaponSystem == null)
            {
                Debug.LogWarning($"[VRBoatController] WeaponSystem component not found on {gameObject.name}");
            }

            // Store initial steering wheel state
            if (steeringWheelVisual != null)
            {
                initialSteeringPosition = steeringWheelVisual.localPosition;
                initialSteeringRotation = steeringWheelVisual.localRotation;
            }
        }

        private void Start()
        {
            // Initialize XR devices
            InitializeControllers();

            // Setup steering wheel interaction callbacks
            if (steeringWheelInteractable != null)
            {
                steeringWheelInteractable.selectEntered.AddListener(OnSteeringGrabbed);
                steeringWheelInteractable.selectExited.AddListener(OnSteeringReleased);
            }
            else
            {
                Debug.LogWarning("[VRBoatController] Steering wheel interactable not assigned");
            }
        }

        private void InitializeControllers()
        {
            var leftHandDevices = new System.Collections.Generic.List<InputDevice>();
            var rightHandDevices = new System.Collections.Generic.List<InputDevice>();

            InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandDevices);
            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightHandDevices);

            if (leftHandDevices.Count > 0)
            {
                leftDevice = leftHandDevices[0];
                Debug.Log($"[VRBoatController] Left controller initialized: {leftDevice.name}");
            }

            if (rightHandDevices.Count > 0)
            {
                rightDevice = rightHandDevices[0];
                Debug.Log($"[VRBoatController] Right controller initialized: {rightDevice.name}");
            }
        }

        private void Update()
        {
            // Reinitialize controllers if needed
            if (!leftDevice.isValid || !rightDevice.isValid)
            {
                InitializeControllers();
            }

            // Process steering input
            if (isSteeringGrabbed)
            {
                ProcessSteeringInput();
                ProcessThrottleInput();
            }

            // Process weapon aiming
            ProcessWeaponAiming();

            // Process weapon firing
            ProcessWeaponFiring();

            // Apply inputs to boat physics
            ApplyInputsToBoat();
        }

        private void ProcessSteeringInput()
        {
            if (leftController == null) return;

            // Get controller rotation and position
            Vector2 thumbstickInput = Vector2.zero;
            leftDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out thumbstickInput);

            // Calculate steering from thumbstick horizontal axis
            float targetSteering = Mathf.Clamp(thumbstickInput.x, -1f, 1f);

            // Apply deadzone
            if (Mathf.Abs(targetSteering) < steeringDeadzone)
            {
                targetSteering = 0f;
            }

            // Smooth steering transition
            currentSteeringInput = Mathf.Lerp(currentSteeringInput, targetSteering, Time.deltaTime * steeringSpeed);

            // Update visual steering wheel rotation
            if (steeringWheelVisual != null)
            {
                float wheelRotation = currentSteeringInput * maxSteeringAngle;
                steeringWheelVisual.localRotation = initialSteeringRotation * Quaternion.Euler(0f, 0f, -wheelRotation);
            }

            // Haptic feedback for steering
            if (Mathf.Abs(targetSteering) > steeringDeadzone)
            {
                SendHapticFeedback(leftDevice, steeringHapticStrength * Mathf.Abs(targetSteering), Time.deltaTime);
            }
        }

        private void ProcessThrottleInput()
        {
            if (leftController == null) return;

            // Get trigger value for throttle control
            float triggerValue = 0f;
            leftDevice.TryGetFeatureValue(CommonUsages.trigger, out triggerValue);

            // Update throttle based on trigger press
            if (triggerValue > throttlePushThreshold)
            {
                float throttleInput = Mathf.InverseLerp(throttlePushThreshold, 1f, triggerValue);
                currentThrottle = Mathf.Lerp(currentThrottle, throttleInput * maxThrottle, Time.deltaTime * throttleSpeed);
            }
            else
            {
                // Gradual throttle decay when not pressed
                currentThrottle = Mathf.Lerp(currentThrottle, 0f, Time.deltaTime * throttleSpeed * 0.5f);
            }

            // Update visual steering wheel position (push/pull for throttle)
            if (steeringWheelVisual != null)
            {
                Vector3 pushOffset = Vector3.forward * currentThrottle * -0.05f; // 5cm max push
                steeringWheelVisual.localPosition = initialSteeringPosition + pushOffset;
            }
        }

        private void ProcessWeaponAiming()
        {
            if (rightController == null || weaponMount == null) return;

            // Get right controller rotation
            Quaternion controllerRotation = rightController.transform.rotation;

            // Calculate target rotation for weapon mount
            Quaternion targetRotation = Quaternion.Lerp(
                weaponMount.rotation,
                controllerRotation,
                Time.deltaTime * weaponRotationSpeed
            );

            // Apply rotation limits
            Vector3 localEuler = (Quaternion.Inverse(transform.rotation) * targetRotation).eulerAngles;

            // Normalize angles to -180 to 180
            float pitch = localEuler.x > 180 ? localEuler.x - 360 : localEuler.x;
            float yaw = localEuler.y > 180 ? localEuler.y - 360 : localEuler.y;

            // Clamp angles
            pitch = Mathf.Clamp(pitch, -maxWeaponPitch, maxWeaponPitch);
            yaw = Mathf.Clamp(yaw, -maxWeaponYaw, maxWeaponYaw);

            // Apply clamped rotation
            weaponMount.localRotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        private void ProcessWeaponFiring()
        {
            if (rightController == null || weaponSystem == null) return;

            // Get trigger value
            float triggerValue = 0f;
            rightDevice.TryGetFeatureValue(CommonUsages.trigger, out triggerValue);

            // Fire weapon when trigger is fully pressed
            if (triggerValue > 0.9f)
            {
                bool fired = weaponSystem.TryFire();

                if (fired)
                {
                    // Haptic feedback for firing
                    SendHapticFeedback(rightDevice, fireHapticStrength, fireHapticDuration);
                }
            }
        }

        private void ApplyInputsToBoat()
        {
            if (boatPhysics == null) return;

            // Apply steering and throttle to boat physics
            boatPhysics.SetSteeringInput(currentSteeringInput);
            boatPhysics.SetThrottleInput(currentThrottle);
        }

        private void OnSteeringGrabbed(SelectEnterEventArgs args)
        {
            isSteeringGrabbed = true;
            Debug.Log("[VRBoatController] Steering wheel grabbed");

            // Send haptic feedback
            SendHapticFeedback(leftDevice, 0.5f, 0.1f);
        }

        private void OnSteeringReleased(SelectExitEventArgs args)
        {
            isSteeringGrabbed = false;
            Debug.Log("[VRBoatController] Steering wheel released");

            // Steering input will gradually decay but not immediately reset
            // This preserves momentum
        }

        private void SendHapticFeedback(InputDevice device, float amplitude, float duration)
        {
            if (device.isValid)
            {
                HapticCapabilities capabilities;
                if (device.TryGetHapticCapabilities(out capabilities))
                {
                    if (capabilities.supportsImpulse)
                    {
                        device.SendHapticImpulse(0, amplitude, duration);
                    }
                }
            }
        }

        // Public methods for external access
        public float GetCurrentSteeringInput() => currentSteeringInput;
        public float GetCurrentThrottle() => currentThrottle;
        public bool IsSteeringGrabbed() => isSteeringGrabbed;

        private void OnDestroy()
        {
            // Clean up event listeners
            if (steeringWheelInteractable != null)
            {
                steeringWheelInteractable.selectEntered.RemoveListener(OnSteeringGrabbed);
                steeringWheelInteractable.selectExited.RemoveListener(OnSteeringReleased);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Clamp values in editor
            maxSteeringAngle = Mathf.Clamp(maxSteeringAngle, 0f, 90f);
            maxThrottle = Mathf.Clamp01(maxThrottle);
            steeringDeadzone = Mathf.Clamp(steeringDeadzone, 0f, 0.5f);
            maxWeaponPitch = Mathf.Clamp(maxWeaponPitch, 0f, 90f);
            maxWeaponYaw = Mathf.Clamp(maxWeaponYaw, 0f, 180f);
        }
#endif
    }
}
