using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class AStarAnimation : MonoBehaviour, IAnimation
{
    public float Speed { get; set; }

    public Dictionary<Vector3Int, List<DronePath>> Paths { get; set; } = new();


    private ChunkedGrid grid;
    private CBS cbs;

    [SerializeField] private Vector3 gridOrigin = new Vector3(0, 0, 0);
    [SerializeField] private float nodeSize = 1f;

    private List<Vector3> starts;
    private List<Vector3> goals;

    private bool hasRunCBS = false;

    private void Awake()
{
    grid = new ChunkedGrid(nodeSize);
    grid.GridOrigin = gridOrigin;
    cbs = new CBS(grid);

    Debug.Log($"[AStarAnimation] Grid initialized with origin {gridOrigin}");

}


    public void RunCBSOnce(List<Vector3> allStarts, List<Vector3> allGoals)
{
    if (hasRunCBS) return;
    hasRunCBS = true;

    var snappedStarts = allStarts.Select(SnapToGrid).ToList();
    var snappedGoals = allGoals.Select(SnapToGrid).ToList();

    // Filter out duplicate goals
    var uniqueGoals = new List<Vector3>();
    var agentsWithDuplicateGoals = new List<int>();
    var goalMapping = new Dictionary<int, Vector3>();  // Map snapped goal to original goal position

    for (int i = 0; i < snappedGoals.Count; i++)
    {
        if (uniqueGoals.Contains(snappedGoals[i]))
        {
            agentsWithDuplicateGoals.Add(i);  // Exclude this agent from CBS
        }
        else
        {
            uniqueGoals.Add(snappedGoals[i]);
            goalMapping[i] = allGoals[i];  // Save the original goal position
        }
    }

    // Filtered starts and goals
    var filteredStarts = snappedStarts.Where((s, i) => !agentsWithDuplicateGoals.Contains(i)).ToList();
    var filteredGoals = uniqueGoals;  // This is where the duplicate goals are removed.

    Vector3 preloadSize = Vector3.one * 30f;
    foreach (var pos in filteredStarts.Concat(filteredGoals))
    {
        grid.PreloadChunksAroundBounds(pos, preloadSize);
    }

    // Now run CBS for the filtered starts and goals
    var cbsPaths = cbs.Solve(filteredStarts, filteredGoals);
    if (cbsPaths == null)
    {
        Debug.LogError("[AStarAnimation] CBS failed to find conflict-free paths.");
        return;
    }

    // Store the valid paths for later use
    // After solving CBS and finding paths
    for (int i = 0; i < cbsPaths.Count; i++)
    {
        // Retrieve the snapped goal and the corresponding original (non-snapped) goal
        Vector3 snappedGoal = filteredGoals[i];
        Vector3 originalGoal = goalMapping.ContainsKey(i) ? goalMapping[i] : snappedGoal;
        
        // Pass the original goal (float) to DronePathBuilder
        //var result = DronePathBuilder.FromVectorPath(cbsPaths[i], originalGoal);  // Use the original goal as float
        Vector3Int goalKey = grid.WorldToCell(snappedGoal);

        if (!Paths.ContainsKey(goalKey))
            Paths[goalKey] = new List<DronePath>();

        //Paths[goalKey].Add(result);
    }

}

    public DronePath GeneratePath(Vector3 from, Vector3 to, List<DronePath> existingPaths = null)
{
    
    return DronePathBuilder.FromStartToGoal(from, to);
}


    private Vector3 SnapToGrid(Vector3 pos)
    {
        return new Vector3(
            Mathf.Floor(pos.x / nodeSize) * nodeSize,
            Mathf.Floor(pos.y / nodeSize) * nodeSize,
            Mathf.Floor(pos.z / nodeSize) * nodeSize
        );
    }


    private void OnDrawGizmos()
    {
        if (grid == null) return;

        // Draw walkable nodes (cyan color)
        Gizmos.color = Color.cyan;
        // foreach (var chunk in grid.Debug_GetAllChunks())
        // {
        //     foreach (var node in chunk.Nodes)
        //     {
        //         if (!node.Walkable) continue;

        //         Vector3Int globalCell = chunk.ChunkCoord * Chunk.ChunkSize + node.Position;
        //         Vector3 worldPos = grid.CellToWorld(globalCell);
        //         Gizmos.DrawWireCube(worldPos + Vector3.one * nodeSize * 0.5f, Vector3.one * nodeSize * 0.9f);
        //     }
        // }

        // Draw chunk boundaries (yellow color)
        Gizmos.color = Color.yellow;
        foreach (var chunk in grid.Debug_GetAllChunks())
        {
            Vector3 chunkWorldPos = grid.CellToWorld(chunk.ChunkCoord * Chunk.ChunkSize);
            Vector3 chunkSize = Vector3.one * Chunk.ChunkSize * grid.NodeSize;
            Gizmos.DrawWireCube(chunkWorldPos + chunkSize / 2f, chunkSize);
        }

        // Draw entire grid bounds (green color)
        Gizmos.color = Color.green;
        Vector3Int minCoord = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
        Vector3Int maxCoord = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

        foreach (var chunk in grid.Debug_GetAllChunks())
        {
            minCoord = Vector3Int.Min(minCoord, chunk.ChunkCoord);
            maxCoord = Vector3Int.Max(maxCoord, chunk.ChunkCoord);
        }

        Vector3 gridMin = grid.CellToWorld(minCoord * Chunk.ChunkSize);
        Vector3 gridMax = grid.CellToWorld((maxCoord + Vector3Int.one) * Chunk.ChunkSize);
        Gizmos.DrawWireCube((gridMin + gridMax) / 2f, gridMax - gridMin);
    }

    
}
