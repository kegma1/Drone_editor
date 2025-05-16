using System.Collections.Generic;
using UnityEngine;


public class ChunkedGrid
{
    private Dictionary<Vector3Int, Chunk> chunks = new();
    public float NodeSize { get; private set; }
    public Vector3 GridOrigin { get; set; } = Vector3.zero;

    public ChunkedGrid(float nodeSize)
    {
        NodeSize = nodeSize;
    }

    public IEnumerable<Node> Nodes
{
    get
    {
        foreach (var chunk in chunks.Values)
        {
            foreach (var node in chunk.Nodes)
            {
                yield return node;
            }
        }
    }
}



    public Vector3Int GetChunkCoord(Vector3Int gridPos)
    {
        return new Vector3Int(
            Mathf.FloorToInt((float)gridPos.x / Chunk.ChunkSize),
            Mathf.FloorToInt((float)gridPos.y / Chunk.ChunkSize),
            Mathf.FloorToInt((float)gridPos.z / Chunk.ChunkSize)
        );

    }

    public Node GetNode(Vector3Int worldPos)
{
    Vector3Int chunkCoord = GetChunkCoord(worldPos);

    if (!chunks.TryGetValue(chunkCoord, out var chunk))
    {
        chunk = new Chunk(chunkCoord, NodeSize);
        chunks[chunkCoord] = chunk;
    }

    Vector3Int localPos = worldPos - chunkCoord * Chunk.ChunkSize; 

    return chunk.GetOrCreateNode(localPos);  // Store/retrieve using local pos
}

    public bool IsWalkable(Vector3Int worldPos)
    {
        var node = GetNode(worldPos);
        return node != null && node.Walkable;
    }

    public Vector3Int WorldToCell(Vector3 worldPos)
    {
        Vector3 local = worldPos - GridOrigin;
        return new Vector3Int(
            Mathf.FloorToInt(local.x / NodeSize),
            Mathf.FloorToInt(local.y / NodeSize),
            Mathf.FloorToInt(local.z / NodeSize)
        );
    }

    public Vector3 CellToWorld(Vector3Int cell)
    {
        return GridOrigin + new Vector3(cell.x, cell.y, cell.z) * NodeSize;
    }

    public List<Node> GetNeighbors(Node node)
{
    List<Node> neighbors = new();
    Vector3Int[] directions = {
        Vector3Int.right, Vector3Int.left,
        Vector3Int.up, Vector3Int.down,
        new Vector3Int(0, 0, 1), new Vector3Int(0, 0, -1)
    };

    foreach (var dir in directions)
    {
        Vector3Int neighborPos = node.Position + dir;

        // Ensure the chunk for this neighbor is loaded
        Chunk chunk = GetChunkForPosition(neighborPos);
        if (chunk == null)
        {
            LoadChunkForPosition(neighborPos);
            chunk = GetChunkForPosition(neighborPos);

            if (chunk == null)
            {
                Debug.LogWarning($"[ChunkedGrid] Failed to load chunk at {GetChunkCoord(neighborPos)}");
                continue;
            }
        }

        // Convert to local chunk coordinates
        Vector3Int localPos = neighborPos - chunk.ChunkCoord * Chunk.ChunkSize;
        Node neighborNode = chunk.GetOrCreateNode(localPos);
        Vector3Int fromChunk = GetChunkCoord(node.Position);
        Vector3Int toChunk = GetChunkCoord(neighborPos);

        if (fromChunk != toChunk) {
            //Debug.Log($"[ChunkedGrid] CROSSING chunk boundary: {node.Position} ({fromChunk}) → {neighborPos} ({toChunk})");
        }

        if (neighborNode != null)
        {
            neighbors.Add(neighborNode);
        }

    }

    return neighbors;
}


    public void PreloadChunksAroundBounds(Vector3 center, Vector3 size)
{
    Vector3 min = center - size / 2f;
    Vector3 max = center + size / 2f;

    Vector3Int minCell = WorldToCell(min);
    Vector3Int maxCell = WorldToCell(max);

    int preloadCount = 0;

    for (int x = minCell.x; x <= maxCell.x; x++)
    {
        for (int y = minCell.y; y <= maxCell.y; y++)
        {
            for (int z = minCell.z; z <= maxCell.z; z++)
            {
                Vector3Int cell = new Vector3Int(x, y, z);
                var node = GetNode(cell);  // Automatically creates node and chunk
                preloadCount++;
            }
        }
    }

    //Debug.Log($"[ChunkedGrid] Preloaded {preloadCount} nodes from {minCell} to {maxCell}");
}

    public IEnumerable<Chunk> Debug_GetAllChunks()
    {
        return chunks.Values;
    }

    public Chunk GetChunkForPosition(Vector3Int worldPos)
{
    Vector3Int chunkCoord = GetChunkCoord(worldPos);
    if (chunks.TryGetValue(chunkCoord, out var chunk))
    {
        return chunk;
    }
    return null;  // Chunk doesn't exist
}

    public void LoadChunkForPosition(Vector3Int worldPos)
{
    Vector3Int chunkCoord = GetChunkCoord(worldPos);
    if (!chunks.ContainsKey(chunkCoord))
    {
        Chunk newChunk = new Chunk(chunkCoord, NodeSize);
        chunks[chunkCoord] = newChunk;
    }
}



}
