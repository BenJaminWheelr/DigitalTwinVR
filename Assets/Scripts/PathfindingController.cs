using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PathfindingController : MonoBehaviour
{
    // ========================================================================
    // CONFIGURATION
    // ========================================================================
    [Header("References")]
    public Transform playerTransform; 
    public Transform playerHead; 
    
    [Header("Triggers")]
    [Tooltip("Drag the object with FireAlarmManager here")]
    public FireAlarmManager alarmManager; // <--- CHANGED: Now looks for Alarm

    [Header("Visuals - AR Arrows")]
    public LineRenderer pathRenderer; 
    public float arrowFlowSpeed = -2.0f;
    public float floorOffset = 0.05f;
    public float drawDistance = 25.0f;

    [Header("Visuals - Wrong Way UI")]
    public GameObject wrongWayIndicator; 
    public float fieldOfView = 150f; 
    public float flashSpeed = 0.5f; 

    [Header("Target Configuration")]
    public Transform[] availableExits; 

    [Header("Path Tuning")]
    [SerializeField] private float calculationInterval = 0.5f;
    [SerializeField] private float waypointThreshold = 1.5f; 
    [SerializeField] private float searchRange = 2.0f;

    [Header("UI")]
    public Button togglePathfindingButton;
    public Button toggleSoundBeaconButton;
    public Button toggleHapticsButton;
    public Color greenColor = new Color(0f, 1f, 0f, 1f); // exit signs showing
    public Color redColor = new Color(1f, 0f, 0f, 1f);   // no exit signs
    [Range(0f, 1f)]
    public float highlightMultiplier = 0.8f; // darken by 20% when hovered

    public AudioSource clickAudioSource;

    // ========================================================================
    // STATE
    // ========================================================================
    private NavMeshPath _currentPath; 
    private int _currentCornerIndex = 0; 
    private float _flashTimer;
    private bool shouldPathfindingActivate = false;
    private bool shouldSoundBeaconActivate = false;
    private bool shouldHapticsActivate = false;
    private Transform currentExit = null;
    private GameObject activeSoundBeacon = null;
    public bool areHapticsEnabled()
    {
        return shouldHapticsActivate;
    }
    public void TogglePathfinding()
    {
        if (clickAudioSource != null) clickAudioSource.Play();

        shouldPathfindingActivate = !shouldPathfindingActivate;

        if (shouldPathfindingActivate)
            _currentCornerIndex = 0;

        UpdateButtonColors(togglePathfindingButton, shouldPathfindingActivate);
    }

    public void ToggleSound()
    {
        if (clickAudioSource != null) clickAudioSource.Play();

        shouldSoundBeaconActivate = !shouldSoundBeaconActivate;
        UpdateButtonColors(toggleSoundBeaconButton, shouldSoundBeaconActivate);

        if (!shouldSoundBeaconActivate)
            DisableCurrentBeacon();   // <-- REQUIRED
    }


    public void ToggleHaptics()
    {
        if (clickAudioSource != null) clickAudioSource.Play();

        shouldHapticsActivate = !shouldHapticsActivate;
        UpdateButtonColors(toggleHapticsButton, shouldHapticsActivate);
    }


    private void UpdateButtonColors(Button buttonToUpdate, bool shouldActivate)
    {
        Color normal = shouldActivate ? greenColor : redColor;
        Color highlighted = normal * highlightMultiplier; // makes it darker
        Color pressed = normal * 0.6f; // optional: even darker when pressed

        ColorBlock cb = buttonToUpdate.colors;
        cb.normalColor = normal;
        cb.highlightedColor = highlighted;
        cb.pressedColor = pressed;
        cb.selectedColor = normal; // stays same as normal
        cb.disabledColor = Color.gray;
        cb.colorMultiplier = 1f; // ensures the color changes are applied
        buttonToUpdate.colors = cb;
    }

    // PUBLIC PROPERTY: THE "ALARM SPY" CHECK
    public bool IsActive
    {
        get
        {
            // Safety check
            if (alarmManager == null || !alarmManager.isActive) return false;

            // SPY LOGIC: In FireAlarmManager, when active, the button turns Green.
            // We check if the current button color matches the manager's "Green" color.
            return alarmManager.isActive;
        }
    }

    public Vector3 GetNextWaypoint() 
    {
        if (!IsActive || _currentPath == null || _currentPath.corners.Length <= _currentCornerIndex) 
            return playerTransform != null ? playerTransform.position : transform.position; 
        return _currentPath.corners[_currentCornerIndex];
    }

    void Start()
    {
        _currentPath = new NavMeshPath(); 
        
        if (playerTransform == null && Camera.main != null) 
            playerTransform = Camera.main.transform.root;
        if (playerHead == null && Camera.main != null)
            playerHead = Camera.main.transform;
        
        // Auto-find AlarmManager if not assigned
        if (alarmManager == null) alarmManager = FindAnyObjectByType<FireAlarmManager>();

        StartCoroutine(UpdatePathRoutine());
        UpdateButtonColors(togglePathfindingButton, shouldPathfindingActivate);
        UpdateButtonColors(toggleSoundBeaconButton, shouldSoundBeaconActivate);
        UpdateButtonColors(toggleHapticsButton, shouldHapticsActivate);

        DisableAllBeacons();

    }

    // ========================================================================
    // FRAME UPDATE (Visuals)
    // ========================================================================
    void Update()
    {
        // 1. SAFETY CHECK: If Alarm is OFF, Hide everything
        if (!IsActive || !shouldPathfindingActivate)
        {
            if (pathRenderer != null) pathRenderer.enabled = false;
            if (wrongWayIndicator != null) wrongWayIndicator.SetActive(false);
            return; 
        }

        // 2. ARROW FLOW
        if (pathRenderer != null && pathRenderer.material != null)
        {
            float offset = Time.time * arrowFlowSpeed; 
            pathRenderer.material.mainTextureOffset = new Vector2(offset, 0);
        }

        // 3. WRONG WAY FLASHING
        if (playerHead != null && wrongWayIndicator != null)
        {
            Vector3 targetPoint = GetNextWaypoint();
            Vector3 directionToTarget = (targetPoint - playerHead.position).normalized;
            Vector3 flatForward = playerHead.forward; flatForward.y = 0;
            Vector3 flatTarget = directionToTarget; flatTarget.y = 0;

            float angle = Vector3.Angle(flatForward, flatTarget);
            bool isWrongWay = angle > fieldOfView;

            if (isWrongWay)
            {
                _flashTimer += Time.deltaTime;
                if (_flashTimer >= flashSpeed)
                {
                    wrongWayIndicator.SetActive(!wrongWayIndicator.activeSelf);
                    _flashTimer = 0; 
                }
            }
            else
            {
                wrongWayIndicator.SetActive(false);
                _flashTimer = flashSpeed; 
            }
        }
    }

    // ========================================================================
    // COROUTINE (Math)
    // ========================================================================
    private IEnumerator UpdatePathRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(calculationInterval);
        
        while (true)
        {
            // WAIT FOR ALARM
            if (!IsActive) 
            {
                yield return wait;
                continue; 
            }

            if (playerTransform == null) yield break;

            CalculateShortestPath(playerTransform.position);
            UpdateWaypointProgress();
            DrawPathGeometry(); 
            
            yield return wait; 
        }
    }

    // ------------------------------------------------------------------------
    // STANDARD PATHFINDING HELPERS (Unchanged)
    // ------------------------------------------------------------------------
    
    private void CalculateShortestPath(Vector3 startPos)
    {
        float shortestLength = Mathf.Infinity;
        NavMeshPath bestPath = null;

        foreach (Transform exit in availableExits)
        {
            if (exit == null) continue;
            // Note: This still checks if doors are physically blocked by fire, 
            // even if we are triggered by the alarm. This is correct behavior.
            if (!IsExitSafe(exit)) continue;

            NavMeshHit exitHit;
            if (!NavMesh.SamplePosition(exit.position, out exitHit, searchRange, NavMesh.AllAreas))
                continue; 

            NavMeshPath testPath = new NavMeshPath();
            if (NavMesh.CalculatePath(startPos, exitHit.position, NavMesh.AllAreas, testPath))
            {
                if (testPath.status == NavMeshPathStatus.PathComplete)
                {
                    float pathLength = GetPathLength(testPath);
                    if (pathLength < shortestLength)
                    {
                        shortestLength = pathLength;
                        bestPath = testPath;
                        currentExit = exit;
                    }
                }
            }
        }
        _currentPath = bestPath;
        UpdateSoundBeacon();
    }

    private void UpdateSoundBeacon()
    {
        // Hard reset if disabled or invalid
        if (!IsActive || !shouldSoundBeaconActivate || currentExit == null || _currentPath == null || _currentPath.corners.Length < 2)
        {
            DisableCurrentBeacon();
            return;
        }

        Transform beaconTransform = currentExit.Find("SoundBeacon");

        // No beacon on this exit
        if (beaconTransform == null)
        {
            DisableCurrentBeacon();
            return;
        }

        GameObject newBeacon = beaconTransform.gameObject;

        // Turn off anything that's NOT the correct one
        if (activeSoundBeacon != null && activeSoundBeacon != newBeacon)
        {
            DisableCurrentBeacon();
        }

        // Force-activate correct beacon every update
        if (activeSoundBeacon != newBeacon)
        {
            activeSoundBeacon = newBeacon;
            activeSoundBeacon.SetActive(true);

            AudioSource src = activeSoundBeacon.GetComponent<AudioSource>();
            if (src != null && !src.isPlaying)
                src.Play();
        }
    }

    private void DisableAllBeacons()
    {
        foreach (Transform exit in availableExits)
        {
            if (exit == null) continue;

            Transform beacon = exit.Find("SoundBeacon");
            if (beacon == null) continue;

            AudioSource src = beacon.GetComponent<AudioSource>();
            if (src != null)
                src.Stop();

            beacon.gameObject.SetActive(false);
        }

        activeSoundBeacon = null;
    }



    private void DisableCurrentBeacon()
    {
        if (activeSoundBeacon != null)
        {
            activeSoundBeacon.SetActive(false);
            activeSoundBeacon = null;
        }
    }



    private bool IsExitSafe(Transform exitNode)
    {
        DoorBehavior[] doors = exitNode.GetComponentsInChildren<DoorBehavior>();
        if (doors.Length == 0) return true;
        foreach (DoorBehavior door in doors)
        {
            if (door.getFireStatus() == true) return false; 
        }
        return true; 
    }

    private float GetPathLength(NavMeshPath path)
    {
        if (path.corners.Length < 2) return Mathf.Infinity;
        float length = 0;
        for (int i = 0; i < path.corners.Length - 1; i++)
            length += Vector3.Distance(path.corners[i], path.corners[i + 1]);
        return length;
    }

    private void UpdateWaypointProgress()
    {
        if (_currentPath == null || _currentPath.status != NavMeshPathStatus.PathComplete) return;

        if (_currentCornerIndex < _currentPath.corners.Length)
        {
            Vector3 playerPos = playerTransform.position;
            Vector3 targetPos = _currentPath.corners[_currentCornerIndex];
            playerPos.y = 0; targetPos.y = 0;

            if (Vector3.Distance(playerPos, targetPos) < waypointThreshold)
            {
                if (_currentCornerIndex < _currentPath.corners.Length - 1)
                    _currentCornerIndex++;
            }
        }
        if (_currentCornerIndex >= _currentPath.corners.Length) 
            _currentCornerIndex = Mathf.Max(0, _currentPath.corners.Length - 1);
    }

    private void DrawPathGeometry()
    {

        if (!shouldPathfindingActivate) 
        {
            if (pathRenderer != null) pathRenderer.enabled = false;
            return;
        }

        if (_currentPath != null && _currentPath.corners.Length > 1)
        {
            pathRenderer.enabled = true;
            List<Vector3> drawingPoints = new List<Vector3>();
            Vector3 startPoint = _currentPath.corners[0];
            drawingPoints.Add(startPoint + (Vector3.up * floorOffset));
            
            float accumulatedDist = 0f;

            for (int i = 0; i < _currentPath.corners.Length - 1; i++)
            {
                Vector3 segmentStart = _currentPath.corners[i];
                Vector3 segmentEnd = _currentPath.corners[i + 1];
                float segmentDist = Vector3.Distance(segmentStart, segmentEnd);
                
                if (accumulatedDist + segmentDist <= drawDistance)
                {
                    drawingPoints.Add(segmentEnd + (Vector3.up * floorOffset));
                    accumulatedDist += segmentDist;
                }
                else
                {
                    float remaining = drawDistance - accumulatedDist;
                    Vector3 cutPoint = Vector3.MoveTowards(segmentStart, segmentEnd, remaining);
                    drawingPoints.Add(cutPoint + (Vector3.up * floorOffset));
                    break; 
                }
            }
            pathRenderer.positionCount = drawingPoints.Count;
            pathRenderer.SetPositions(drawingPoints.ToArray());
        }
        else
        {
            pathRenderer.enabled = false;
        }
    }
}