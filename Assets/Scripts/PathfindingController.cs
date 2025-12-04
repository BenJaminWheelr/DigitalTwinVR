using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

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

    // ========================================================================
    // STATE
    // ========================================================================
    private NavMeshPath _currentPath; 
    private int _currentCornerIndex = 0; 
    private float _flashTimer; 

    // PUBLIC PROPERTY: THE "ALARM SPY" CHECK
    public bool IsActive
    {
        get
        {
            // Safety check
            if (alarmManager == null || alarmManager.fireAlarmButton == null) return false;

            // SPY LOGIC: In FireAlarmManager, when active, the button turns Green.
            // We check if the current button color matches the manager's "Green" color.
            return alarmManager.fireAlarmButton.colors.normalColor == alarmManager.greenColor;
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
    }

    // ========================================================================
    // FRAME UPDATE (Visuals)
    // ========================================================================
    void Update()
    {
        // 1. SAFETY CHECK: If Alarm is OFF, Hide everything
        if (!IsActive)
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
                    }
                }
            }
        }
        _currentPath = bestPath;
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