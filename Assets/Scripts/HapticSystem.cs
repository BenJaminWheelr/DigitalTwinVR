using UnityEngine;
using UnityEngine.XR; // Core Hardware Library

public class HapticSystem : MonoBehaviour
{
    // ========================================================================
    // CONFIGURATION
    // ========================================================================
    [Header("References")]
    public PathfindingController pathfinder;
    public Transform playerHead; // Drag 'Main Camera' here

    [Header("Tuning")]
    [SerializeField] private float deadzone = 15.0f;       // Angle to consider "Forward"
    [SerializeField] private float wrongWayThreshold = 90.0f; // Angle to consider "Wrong Way"

    [Header("Haptic Intensities")]
    [SerializeField] private float heartbeatIntensity = 0.5f; 
    [SerializeField] private float turnIntensity = 0.8f;      
    [SerializeField] private float wrongWayIntensity = 1.0f; 

    // Timers to prevent motor locking (spamming the hardware)
    private float _heartbeatTimer;
    private float _heartbeatInterval = 1.0f; 
    private float _continuousTimer;
    private float _continuousInterval = 0.1f; 

    // ========================================================================
    // LOGIC
    // ========================================================================
    void Start()
    {
        if (pathfinder == null) pathfinder = FindAnyObjectByType<PathfindingController>();
        if (playerHead == null && Camera.main != null) playerHead = Camera.main.transform;
    }

    void Update()
    {
        // 1. Safety Checks
        if (pathfinder == null || playerHead == null) return;

        // 2. ALARM SYNC: If the Pathfinding is off (Alarm hasn't started), do nothing.
        if (pathfinder.IsActive == false) return; 

        // 3. Calculate Direction
        Vector3 nextWaypoint = pathfinder.GetNextWaypoint();
        Vector3 forward = playerHead.forward; forward.y = 0; 
        Vector3 directionToTarget = (nextWaypoint - playerHead.position).normalized; directionToTarget.y = 0;

        float signedAngle = Vector3.SignedAngle(forward, directionToTarget, Vector3.up);
        float absAngle = Mathf.Abs(signedAngle);

        // 4. Decision Loop
        if (absAngle > wrongWayThreshold)
        {
            // WRONG WAY (Vibrate Both Hands Strong)
            PlayContinuousHaptic(XRNode.LeftHand, wrongWayIntensity);
            PlayContinuousHaptic(XRNode.RightHand, wrongWayIntensity);
        }
        else if (absAngle < deadzone)
        {
            // FORWARD (Heartbeat Pulse)
            PlayHeartbeat();
        }
        else 
        {
            // TURN (Vibrate Left or Right Hand)
            if (signedAngle > 0)
                PlayContinuousHaptic(XRNode.RightHand, turnIntensity);
            else
                PlayContinuousHaptic(XRNode.LeftHand, turnIntensity);
        }
    }

    // ========================================================================
    // HAPTIC PATTERNS
    // ========================================================================
    private void PlayHeartbeat()
    {
        _heartbeatTimer += Time.deltaTime;
        if (_heartbeatTimer > _heartbeatInterval)
        {
            SendHaptics(XRNode.LeftHand, heartbeatIntensity, 0.05f);
            SendHaptics(XRNode.RightHand, heartbeatIntensity, 0.05f);
            _heartbeatTimer = 0;
        }
    }

    private void PlayContinuousHaptic(XRNode node, float intensity)
    {
        // We pulse rapidly to simulate a continuous sensation
        _continuousTimer += Time.deltaTime;
        if (_continuousTimer > _continuousInterval)
        {
            SendHaptics(node, intensity, _continuousInterval);
            _continuousTimer = 0;
        }
    }

    // ========================================================================
    // HARDWARE INTERFACE
    // ========================================================================
    private void SendHaptics(XRNode node, float amplitude, float duration)
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(node);
        if (device.isValid)
        {
            // Channel 0 is the main motor
            device.SendHapticImpulse(0, amplitude, duration);
        }
    }
}