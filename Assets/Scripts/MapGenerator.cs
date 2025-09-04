using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public List<GameObject> allTilePrefabs;
    public int height = 10;
    public int width = 10;
    public float tileSize = 1f;

    private enum Direction { Top, Bottom, Left, Right }

    private List<GameObject>[,] grid; // Keep as-is for WFC
    private bool[,] isCollapsed;

    public GameObject[,] generatedGrid; // New array for final tile instances

    public List<GameObject> generatedRoads = new List<GameObject>();
    public List<GameObject> generatedNonRoads = new List<GameObject>();

    public GameObject carPrefab;
    public int numberOfCars = 3; // specify how many cars to spawn
    public float carSpawnHeight = 0.5f; // how high the car spawns above the tile

    public GameObject playerCarPrefab;  // Assign your custom car prefab
    public float playerCarSpawnHeight = 0.5f; //

    public GameObject waypointPrefab;
    public int minPathLength = 5; // Minimum path length for player objective
    private List<GameObject> waypoints = new List<GameObject>();

    public float timeLimit = 120f;

    private void Start()
    {
        InitializeGrid();
        CollapseOneRandomCell();

        int maxCollapseAttempts = width * height * 10; // fail-safe
        int attempts = 0;

        while (!AllCollapsed())
        {
            CollapseNextCell();
            attempts++;
            if (attempts > maxCollapseAttempts)
            {
                Debug.LogWarning("Infinite collapse loop prevented! Some cells may remain uncollapsed.");
                break;
            }
        }

        CreateGeneratedGrid();      // WFC map done
        SpawnMultipleCars();        // Spawn AI cars
        SpawnPlayerCar();           // Spawn player car after everything
    }


    void InitializeGrid()
    {
        grid = new List<GameObject>[width, height];
        isCollapsed = new bool[width, height];
        generatedGrid = new GameObject[width, height]; // Initialize generatedGrid

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new List<GameObject>(allTilePrefabs);
                isCollapsed[x, y] = false;
            }
        }
    }

    void CollapseOneRandomCell()
    {
        int x = Random.Range(0, width);
        int y = Random.Range(0, height);
        CollapseCell(x, y);
        PropagateConstraints(x, y);
    }

    void CollapseNextCell()
    {
        int minOptions = int.MaxValue;
        Vector2Int bestCell = new Vector2Int(-1, -1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!isCollapsed[x, y])
                {
                    int options = grid[x, y].Count;
                    if (options < minOptions && options > 0)
                    {
                        minOptions = options;
                        bestCell = new Vector2Int(x, y);
                    }
                }
            }
        }

        if (bestCell.x != -1)
        {
            CollapseCell(bestCell.x, bestCell.y);
            PropagateConstraints(bestCell.x, bestCell.y);
        }
    }

    void CollapseCell(int x, int y)
    {
        List<GameObject> options = grid[x, y];

        if (options.Count == 0)
        {
            Debug.LogWarning($"No options to collapse at ({x}, {y})");
            return;
        }

        GameObject selectedPrefab = options[Random.Range(0, options.Count)];
        Vector3 position = new Vector3(x * tileSize, 0, y * tileSize);
        GameObject spawnedTile = Instantiate(selectedPrefab, position, selectedPrefab.transform.rotation);

        // Replace prefab list with actual instantiated tile reference
        grid[x, y] = new List<GameObject> { spawnedTile };
        isCollapsed[x, y] = true;
    }

    void PropagateConstraints(int StartX, int StartY)
    {
        Queue<Vector2Int> toCheck = new Queue<Vector2Int>();
        toCheck.Enqueue(new Vector2Int(StartX, StartY));

        while (toCheck.Count > 0)
        {
            Vector2Int current = toCheck.Dequeue();
            int x = current.x;
            int y = current.y;

            if (!isCollapsed[x, y]) return;

            GameObject selectedTile = grid[x, y][0];
            BaseTile tileComp = selectedTile.GetComponent<BaseTile>();

            FilterNeighbor(x, y + 1, tileComp.sockets.top, Direction.Bottom, toCheck);
            FilterNeighbor(x, y - 1, tileComp.sockets.bottom, Direction.Top, toCheck);
            FilterNeighbor(x - 1, y, tileComp.sockets.left, Direction.Right, toCheck);
            FilterNeighbor(x + 1, y, tileComp.sockets.right, Direction.Left, toCheck);
        }
    }

    void FilterNeighbor(int nx, int ny, List<SocketType> requiredMatch, Direction neighborSocketSide, Queue<Vector2Int> queue)
    {
        if (nx < 0 || nx >= width || ny < 0 || ny >= height) return;
        if (isCollapsed[nx, ny]) return;

        List<GameObject> possibleTiles = grid[nx, ny];
        int beforeCount = possibleTiles.Count;

        possibleTiles.RemoveAll(tile =>
        {
            var comp = tile.GetComponent<BaseTile>();
            List<SocketType> socket = GetSocketList(comp, neighborSocketSide);
            return !(socket.Count == requiredMatch.Count && !socket.Except(requiredMatch).Any());
        });

        if (possibleTiles.Count == 0) return;

        if (possibleTiles.Count < beforeCount)
            queue.Enqueue(new Vector2Int(nx, ny));

        if (possibleTiles.Count == 1 && !isCollapsed[nx, ny])
        {
            isCollapsed[nx, ny] = true;
            Vector3 pos = new Vector3(nx * tileSize, 0, ny * tileSize);
            GameObject spawnedTile = Instantiate(possibleTiles[0], pos, possibleTiles[0].transform.rotation);
            grid[nx, ny] = new List<GameObject> { spawnedTile };
            PropagateConstraints(nx, ny);
        }
    }

    List<SocketType> GetSocketList(BaseTile tile, Direction dir)
    {
        switch (dir)
        {
            case Direction.Top: return tile.sockets.top;
            case Direction.Bottom: return tile.sockets.bottom;
            case Direction.Left: return tile.sockets.left;
            case Direction.Right: return tile.sockets.right;
            default: return new List<SocketType>();
        }
    }

    bool AllCollapsed()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (!isCollapsed[x, y])
                    return false;
        return true;
    }

    void CreateGeneratedGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y].Count > 0)
                {
                    GameObject tile = grid[x, y][0];
                    generatedGrid[x, y] = tile;

                    // Separate road and non-road tiles
                    BaseTile tileComp = tile.GetComponent<BaseTile>();
                    if (tileComp != null && tileComp.isRoad) // Use a boolean flag in BaseTile
                    {
                        generatedRoads.Add(tile);
                    }
                    else
                    {
                        generatedNonRoads.Add(tile);
                    }
                }
            }
        }

        Debug.Log($"Total road tiles: {generatedRoads.Count}");
        Debug.Log($"Total non-road tiles: {generatedNonRoads.Count}");
    }

    private void SpawnMultipleCars()
    {
        if (generatedRoads.Count == 0 || carPrefab == null)
        {
            Debug.LogWarning("No road tiles or car prefab assigned!");
            return;
        }

        int carsSpawned = 0;
        int maxAttemptsPerCar = 50;

        while (carsSpawned < numberOfCars)
        {
            int attempts = 0;
            bool carSpawned = false;

            while (!carSpawned && attempts < maxAttemptsPerCar)
            {
                attempts++;

                GameObject startTile = generatedRoads[Random.Range(0, generatedRoads.Count)];
                GameObject goalTile = startTile;

                if (generatedRoads.Count > 1)
                {
                    int safeAttempts = 0;
                    while (goalTile == startTile && safeAttempts < maxAttemptsPerCar)
                    {
                        goalTile = generatedRoads[Random.Range(0, generatedRoads.Count)];
                        safeAttempts++;
                    }
                }

                Vector2Int startNode = GetTileCoord(startTile);
                Vector2Int goalNode = GetTileCoord(goalTile);

                List<Vector2Int> path = FindPath(startNode, goalNode);

                if (path != null && path.Count > 0)
                {
                    Vector3 spawnPos = startTile.transform.position + Vector3.up * carSpawnHeight;
                    GameObject car = Instantiate(carPrefab, spawnPos, carPrefab.transform.rotation);

                    // Convert path to world positions
                    List<Vector3> worldPath = path.Select(node =>
                        generatedGrid[node.x, node.y].transform.position + Vector3.up * carSpawnHeight
                    ).ToList();

                    StartCoroutine(MoveCarAlongPath(car, worldPath, 5f));

                    Debug.Log($"Car {carsSpawned + 1} spawned at {spawnPos}");
                    Debug.Log($"Start node: {startNode}, Goal node: {goalNode}");
                    Debug.Log($"Path nodes: {string.Join(" -> ", path.Select(p => $"({p.x},{p.y})"))}");

                    carSpawned = true;
                    carsSpawned++;
                }
                else
                {
                    Debug.LogWarning($"Attempt {attempts} for car {carsSpawned + 1}: No valid path, retrying...");
                }
            }

            if (!carSpawned)
            {
                Debug.LogError($"Failed to spawn car {carsSpawned + 1} after {maxAttemptsPerCar} attempts!");
                carsSpawned++;
            }
        }
    }


    private IEnumerator MoveCarAlongPath(GameObject car, List<Vector3> path, float speed)
    {
        foreach (Vector3 targetPos in path)
        {
            while (Vector3.Distance(car.transform.position, targetPos) > 0.05f)
            {
                car.transform.position = Vector3.MoveTowards(car.transform.position, targetPos, speed * Time.deltaTime);
                car.transform.LookAt(targetPos); // optional: face movement direction
                yield return null;
            }
        }
    }

    Vector2Int GetTileCoord(GameObject tile)
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (generatedGrid[x, y] == tile)
                    return new Vector2Int(x, y);

        return new Vector2Int(-1, -1); // not found
    }

    List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        if (start.x < 0 || goal.x < 0) return null;

        List<Vector2Int> openSet = new List<Vector2Int> { start };
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        Dictionary<Vector2Int, int> gScore = new Dictionary<Vector2Int, int>();
        gScore[start] = 0;

        Dictionary<Vector2Int, int> fScore = new Dictionary<Vector2Int, int>();
        fScore[start] = Heuristic(start, goal);

        while (openSet.Count > 0)
        {
            // Node with lowest fScore
            Vector2Int current = openSet[0];
            foreach (var node in openSet)
                if (fScore.ContainsKey(node) && fScore[node] < fScore[current])
                    current = node;

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current);

            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                int tentativeG = gScore[current] + 1;

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, goal);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null; // no path found
    }

    int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    List<Vector2Int> GetNeighbors(Vector2Int node)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        GameObject currentTileObj = generatedGrid[node.x, node.y];

        if (currentTileObj == null) return neighbors; // skip null tiles

        BaseTile tile = currentTileObj.GetComponent<BaseTile>();
        if (tile == null) return neighbors; // skip if no BaseTile

        // Up
        if (tile.canConnectTop && node.y + 1 < height)
        {
            GameObject upTileObj = generatedGrid[node.x, node.y + 1];
            if (upTileObj != null)
            {
                BaseTile nTile = upTileObj.GetComponent<BaseTile>();
                if (nTile != null && nTile.isRoad && nTile.canConnectBottom)
                    neighbors.Add(new Vector2Int(node.x, node.y + 1));
            }
        }

        // Down
        if (tile.canConnectBottom && node.y - 1 >= 0)
        {
            GameObject downTileObj = generatedGrid[node.x, node.y - 1];
            if (downTileObj != null)
            {
                BaseTile nTile = downTileObj.GetComponent<BaseTile>();
                if (nTile != null && nTile.isRoad && nTile.canConnectTop)
                    neighbors.Add(new Vector2Int(node.x, node.y - 1));
            }
        }

        // Left
        if (tile.canConnectLeft && node.x - 1 >= 0)
        {
            GameObject leftTileObj = generatedGrid[node.x - 1, node.y];
            if (leftTileObj != null)
            {
                BaseTile nTile = leftTileObj.GetComponent<BaseTile>();
                if (nTile != null && nTile.isRoad && nTile.canConnectRight)
                    neighbors.Add(new Vector2Int(node.x - 1, node.y));
            }
        }

        // Right
        if (tile.canConnectRight && node.x + 1 < width)
        {
            GameObject rightTileObj = generatedGrid[node.x + 1, node.y];
            if (rightTileObj != null)
            {
                BaseTile nTile = rightTileObj.GetComponent<BaseTile>();
                if (nTile != null && nTile.isRoad && nTile.canConnectLeft)
                    neighbors.Add(new Vector2Int(node.x + 1, node.y));
            }
        }

        return neighbors;
    }

    List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        List<Vector2Int> path = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        return path;
    }

    private void SpawnPlayerCar()
    {
        if (generatedRoads.Count == 0 || playerCarPrefab == null)
        {
            Debug.LogWarning("No road tiles or player car prefab assigned!");
            return;
        }

        // Try to find a valid path for the player
        if (TryFindValidPath(minPathLength, out List<Vector2Int> path, out Vector2Int startNode))
        {
            // Spawn player at start of path
            GameObject startTile = generatedGrid[startNode.x, startNode.y];
            Vector3 spawnPos = startTile.transform.position + Vector3.up * playerCarSpawnHeight;
            GameObject playerCar = Instantiate(playerCarPrefab, spawnPos, playerCarPrefab.transform.rotation);

            // Create waypoints along the path
            CreateWaypointsAlongPath(path);

            Debug.Log($"Player car spawned at {spawnPos} with a path of {path.Count} steps.");
        }
        else
        {
            // Fallback: spawn at random road tile without waypoints
            Debug.LogWarning("No valid path found for player objective. Spawning at random position.");
            GameObject spawnTile = generatedRoads[Random.Range(0, generatedRoads.Count)];
            Vector3 spawnPos = spawnTile.transform.position + Vector3.up * playerCarSpawnHeight;
            Instantiate(playerCarPrefab, spawnPos, playerCarPrefab.transform.rotation);
        }
    }

    private bool TryFindValidPath(int minPathLength, out List<Vector2Int> path, out Vector2Int startNode)
    {
        path = null;
        startNode = new Vector2Int(-1, -1);

        if (generatedRoads.Count < 2) return false;

        // Shuffle road tiles to try different starts
        List<GameObject> shuffledRoads = new List<GameObject>(generatedRoads);
        shuffledRoads = shuffledRoads.OrderBy(x => Random.value).ToList();

        int maxAttempts = Mathf.Min(20, shuffledRoads.Count); // Limit attempts for performance

        foreach (GameObject startTile in shuffledRoads.Take(maxAttempts))
        {
            Vector2Int start = GetTileCoord(startTile);
            if (start.x == -1) continue;

            // Try to find a goal that's far enough away
            foreach (GameObject goalTile in shuffledRoads)
            {
                if (goalTile == startTile) continue;

                Vector2Int goal = GetTileCoord(goalTile);
                if (goal.x == -1) continue;

                // Check if the Manhattan distance is sufficient
                if (Heuristic(start, goal) < minPathLength) continue;

                List<Vector2Int> foundPath = FindPath(start, goal);
                if (foundPath != null && foundPath.Count >= minPathLength)
                {
                    path = foundPath;
                    startNode = start;
                    return true;
                }
            }
        }

        return false;
    }

    private void CreateWaypointsAlongPath(List<Vector2Int> path)
    {
        // Clear any existing waypoints
        foreach (GameObject waypoint in waypoints)
        {
            if (waypoint != null) Destroy(waypoint);
        }
        waypoints.Clear();

        // Create waypoints along the path (skip the first point where player spawns)
        for (int i = 1; i < path.Count; i++)
        {
            Vector2Int node = path[i];
            GameObject tile = generatedGrid[node.x, node.y];
            Vector3 waypointPos = tile.transform.position + Vector3.up * 0.5f; // Slightly above the tile

            GameObject waypoint = Instantiate(waypointPrefab, waypointPos, Quaternion.identity);
            waypoints.Add(waypoint);

            // Optional: Add visual indicator like a number or arrow
            WaypointIndicator indicator = waypoint.GetComponent<WaypointIndicator>();
            if (indicator != null)
            {
                indicator.SetOrderNumber(i);
            }
        }

        Debug.Log($"Created {waypoints.Count} waypoints along the path");

        // Start the timer after creating waypoints
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetTotalWaypoints(waypoints.Count);
            UIManager.Instance.StartTimer(timeLimit);
        }
    }
}

