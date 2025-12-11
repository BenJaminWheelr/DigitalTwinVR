using UnityEngine;
using UnityEngine.XR;

public class HapticSystem : MonoBehaviour
{
    // ========================================================================
    // CONFIGURATION
    // ========================================================================
    [Header("References")]
    public PathfindingController pathfinder;
    public Transform playerHead;

    [Header("Tuning")]
    [SerializeField] private float deadzone = 15.0f;
    [SerializeField] private float wrongWayThreshold = 90.0f;

    [Header("Haptic Intensities")]
    [SerializeField] private float heartbeatIntensity = 0.5f;
    [SerializeField] private float turnIntensity = 0.8f;
    [SerializeField] private float wrongWayIntensity = 1.0f;

    // ========================================================================
    // INTERNAL STATE
    // ========================================================================
    private float _heartbeatTimer;
    private float _heartbeatInterval = 1.0f;

    private float _leftTimer;
    private float _rightTimer;
    private float _continuousInterval = 0.1f;

    // Cache device references (better performance)
    private InputDevice _leftDevice;
    private InputDevice _rightDevice;

    // ========================================================================
    // INIT
    // ========================================================================
    void Start()
    {
        if (pathfinder == null)
            pathfinder = FindAnyObjectByType<PathfindingController>();

        if (playerHead == null && Camera.main != null)
            playerHead = Camera.main.transform;

        CacheDevices();
    }

    private void CacheDevices()
    {
        _leftDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        _rightDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    // ========================================================================
    // LOOP
    // ========================================================================
    void Update()
    {
        if (pathfinder == null || playerHead == null)
            return;

        // HARD OFF CONDITIONS
        if (!pathfinder.IsActive || !pathfinder.areHapticsEnabled())
        {
            StopAllHaptics();
            return;
        }

        // COMPUTE DIRECTION
        Vector3 target = pathfinder.GetNextWaypoint();
        Vector3 flatForward = playerHead.forward;
        flatForward.y = 0;

        Vector3 toTarget = target - playerHead.position;
        toTarget.y = 0;

        float distance = toTarget.magnitude;

        // ZERO-VECTOR GUARD
        if (distance < 0.2f)
        {
            StopAllHaptics();
            return;
        }

        Vector3 direction = toTarget.normalized;
        float signedAngle = Vector3.SignedAngle(flatForward, direction, Vector3.up);
        float absAngle = Mathf.Abs(signedAngle);

        // STATE MACHINE
        if (absAngle > wrongWayThreshold)
        {
            ResetHeartbeat();
            PlayContinuousBoth(wrongWayIntensity);
        }
        else if (absAngle < deadzone)
        {
            ResetTurnTimers();
            PlayHeartbeat();
        }
        else
        {
            ResetHeartbeat();
            PlayTurnHaptics(signedAngle);
        }
    }

    // ========================================================================
    // PATTERNS
    // ========================================================================

    private void PlayHeartbeat()
    {
        _heartbeatTimer += Time.deltaTime;
        if (_heartbeatTimer >= _heartbeatInterval)
        {
            SendHaptic(_leftDevice, heartbeatIntensity, 0.06f);
            SendHaptic(_rightDevice, heartbeatIntensity, 0.06f);
            _heartbeatTimer = 0;
        }
    }

    private void PlayTurnHaptics(float angle)
    {
        if (angle > 0)
            PlayContinuous(ref _rightTimer, _rightDevice, turnIntensity);
        else
            PlayContinuous(ref _leftTimer, _leftDevice, turnIntensity);
    }

    private void PlayContinuousBoth(float intensity)
    {
        PlayContinuous(ref _leftTimer, _leftDevice, intensity);
        PlayContinuous(ref _rightTimer, _rightDevice, intensity);
    }

    private void PlayContinuous(ref float timer, InputDevice device, float intensity)
    {
        timer += Time.deltaTime;
        if (timer >= _continuousInterval)
        {
            SendHaptic(device, intensity, _continuousInterval);
            timer = 0;
        }
    }

    // ========================================================================
    // RESET HELPERS
    // ========================================================================

    private void ResetHeartbeat()
    {
        _heartbeatTimer = 0;
    }

    private void ResetTurnTimers()
    {
        _leftTimer = 0;
        _rightTimer = 0;
    }

    // ========================================================================
    // HARDWARE INTERFACE
    // ========================================================================

    private void SendHaptic(InputDevice device, float amplitude, float duration)
    {
        if (!device.isValid)
            CacheDevices();

        if (device.isValid)
            device.SendHapticImpulse(0, amplitude, duration);
    }

    private void StopAllHaptics()
    {
        SendHaptic(_leftDevice, 0f, 0f);
        SendHaptic(_rightDevice, 0f, 0f);

        ResetHeartbeat();
        ResetTurnTimers();
    }
}
