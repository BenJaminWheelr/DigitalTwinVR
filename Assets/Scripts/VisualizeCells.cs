using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VisualizeCells : MonoBehaviour
{
    [Header("Cell Settings")]
    public float cellSize = 1f;
    public GameObject cellPrefab;
    public bool showCellsInEditor = false;
    public float yOffset = 0.01f;

    [Header("Fire Settings")]
    public Material burningMaterial;      // material to apply when burning
    public float spreadInterval = 5f;     // seconds between spread
    [Range(0, 1)]
    public float spreadChance = 0.25f;    // 25% chance
    public float maxFog = 0.75f;

    [Header("Player Tracking")]
    public Transform player;

    private int gridX, gridZ;
    private GameObject[,] cellObjects;    // instantiated tiles
    private bool[,] burning;              // is burning?

    private Vector2Int lastPlayerCell = new Vector2Int(-1, -1);

    private int totalTiles;               // total valid tiles
    private int burningTiles;             // currently burning tiles

    private bool cellsVisible = false; // cells invisible by default


    void Start()
    {
        // Setup fog
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = new Color(0.2f, 0.2f, 0.2f);
        RenderSettings.fogDensity = 0.01f;

        Renderer rend = GetComponent<Renderer>();
        Vector3 size = rend.bounds.size;
        gridX = Mathf.CeilToInt(size.x / cellSize);
        gridZ = Mathf.CeilToInt(size.z / cellSize);

        cellObjects = new GameObject[gridX, gridZ];
        burning = new bool[gridX, gridZ];

        Vector3 origin = transform.position - new Vector3(size.x, 0, size.z) / 2f;

        // Spawn cells on this floor
        for (int x = 0; x < gridX; x++)
        {
            for (int z = 0; z < gridZ; z++)
            {
                if (cellPrefab == null) continue;

                Vector3 rayOrigin = origin + new Vector3(x * cellSize + cellSize / 2f, 1f, z * cellSize + cellSize / 2f);

                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 10f))
                {
                    if (hit.collider.gameObject == this.gameObject)
                    {
                        // Use hit.point.y for accurate floor height
                        Vector3 cellPos = new Vector3(rayOrigin.x, hit.point.y + yOffset, rayOrigin.z);
                        GameObject cell = Instantiate(cellPrefab, cellPos, Quaternion.identity);
                        cell.transform.localScale = new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f);
                        cell.transform.parent = transform;
                        cell.GetComponent<Renderer>().material.color = Color.green;
                        cell.SetActive(cellsVisible); // invisible by default
                        cellObjects[x, z] = cell;
                        totalTiles++;
                    }
                }
            }
        }
    }

    public string getFogStatus()
    {
        return $"Fog Density: {RenderSettings.fogDensity:F2} / {maxFog}";
    }


    public string getFireStatus()
    {
        if (totalTiles == 0) return "Fire: 0 / 0 (0%)";

        float percent = ((float)burningTiles / totalTiles) * 100f;
        return $"Fire: {burningTiles} / {totalTiles} ({percent:F1}%)";
    }



    public void ToggleCellsVisibility()
    {
        if (cellObjects == null) return;
        cellsVisible = !cellsVisible;

        for (int x = 0; x < gridX; x++)
            for (int z = 0; z < gridZ; z++)
                if (cellObjects[x, z] != null)
                    cellObjects[x, z].SetActive(cellsVisible);
    }

    public IEnumerator StartFireRoutine()
    {
        yield return new WaitForSeconds(1f);

        // Build list of valid tiles
        List<Vector2Int> validTiles = new List<Vector2Int>();
        for (int x = 0; x < gridX; x++)
            for (int z = 0; z < gridZ; z++)
                if (cellObjects[x, z] != null)
                    validTiles.Add(new Vector2Int(x, z));

        if (validTiles.Count == 0)
        {
            Debug.LogWarning("No valid tiles to start fire!");
            yield break;
        }

        // Pick random valid tile
        Vector2Int start = validTiles[Random.Range(0, validTiles.Count)];
        Debug.Log($"Initial fire at cell ({start.x}, {start.y})");
        IgniteCell(start.x, start.y);

        // Spread forever
        while (true)
        {
            yield return new WaitForSeconds(spreadInterval);
            SpreadFire();
        }
    }

    void IgniteCell(int x, int z)
    {
        if (x < 0 || x >= gridX || z < 0 || z >= gridZ) return;
        if (cellObjects[x, z] == null) return;
        if (burning[x, z]) return;

        burning[x, z] = true;
        burningTiles++;
        Debug.Log($"New fire at cell ({x}, {z})");

        // Change material
        if (burningMaterial != null)
            cellObjects[x, z].GetComponent<Renderer>().material = burningMaterial;
        else
            cellObjects[x, z].GetComponent<Renderer>().material.color = Color.red;

        // Update fog quickly
        UpdateFog();
    }

    void SpreadFire()
    {
        List<Vector2Int> newFires = new List<Vector2Int>();

        for (int x = 0; x < gridX; x++)
        {
            for (int z = 0; z < gridZ; z++)
            {
                if (cellObjects[x, z] == null || !burning[x, z])
                    continue;

                TryIgniteNeighbor(x + 1, z, newFires);
                TryIgniteNeighbor(x - 1, z, newFires);
                TryIgniteNeighbor(x, z + 1, newFires);
                TryIgniteNeighbor(x, z - 1, newFires);
            }
        }

        foreach (var pos in newFires)
            IgniteCell(pos.x, pos.y);
    }

    void TryIgniteNeighbor(int nx, int nz, List<Vector2Int> newFires)
    {
        if (nx < 0 || nx >= gridX || nz < 0 || nz >= gridZ) return;
        if (cellObjects[nx, nz] == null) return;
        if (burning[nx, nz]) return;

        if (Random.value <= spreadChance)
            newFires.Add(new Vector2Int(nx, nz));
    }

    void UpdateFog()
    {
        // Calculate target fog density based on % tiles burning
        float percent = (float)burningTiles / totalTiles;
        float targetDensity = Mathf.Lerp(0.01f, maxFog, percent); 

        // Lerp quickly to target density
        RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, targetDensity, Time.deltaTime * 30f); 

        // Print current fog density
        Debug.Log($"Current fog density: {RenderSettings.fogDensity}");
    }




    void Update()
    {
        if (player == null) return;

        Vector2Int cell = GetCellFromWorldPos(player.position);
        if (cell.x >= 0 && cell.x < gridX && cell.y >= 0 && cell.y < gridZ)
        {
            if (cellObjects[cell.x, cell.y] != null && cell != lastPlayerCell)
            {
                Debug.Log($"Player is standing on cell: ({cell.x}, {cell.y})");
                lastPlayerCell = cell;
            }
        }
    }

    Vector2Int GetCellFromWorldPos(Vector3 worldPos)
    {
        Renderer rend = GetComponent<Renderer>();
        Vector3 size = rend.bounds.size;
        Vector3 origin = transform.position - new Vector3(size.x, 0, size.z) / 2f;

        float localX = worldPos.x - origin.x;
        float localZ = worldPos.z - origin.z;

        int cellX = Mathf.FloorToInt(localX / cellSize);
        int cellZ = Mathf.FloorToInt(localZ / cellSize);

        return new Vector2Int(cellX, cellZ);
    }

    void OnDrawGizmosSelected()
    {
        if (!showCellsInEditor) return;

        Renderer rend = GetComponent<Renderer>();
        if (rend == null) return;

        Vector3 size = rend.bounds.size;
        Vector3 origin = transform.position - new Vector3(size.x, 0, size.z) / 2f;

        Gizmos.color = Color.green;

        for (int x = 0; x < gridX; x++)
            for (int z = 0; z < gridZ; z++)
                if (cellObjects != null && cellObjects[x, z] != null)
                {
                    Vector3 cellPos = origin + new Vector3(x * cellSize + cellSize / 2f, 0.05f, z * cellSize + cellSize / 2f);
                    Gizmos.DrawWireCube(cellPos, new Vector3(cellSize, 0.1f, cellSize));
                }
    }
}
