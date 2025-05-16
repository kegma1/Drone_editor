using UnityEngine;

public class Node {
    public Vector3Int Position;       // Local position in the chunk
    public bool Walkable = true;

    public int GCost = int.MaxValue;  // Start as "infinite" cost until updated
    public int HCost = 0;             // Heuristic cost
    public int FCost => GCost + HCost;

    public Node Parent = null;

    public Node(Vector3Int pos, bool walkable = true) {
        Position = pos;
        Walkable = walkable;

        GCost = int.MaxValue;  
        HCost = 0;
        Parent = null;
    }

    public void Reset() {
        GCost = int.MaxValue;
        HCost = 0;
        Parent = null;
    }

    // Converts this node's local position to world position
    public Vector3 GetWorldPosition(Vector3Int chunkCoord, float nodeSize) {
        Vector3Int worldCell = chunkCoord * Chunk.ChunkSize + Position;
        return (Vector3)worldCell * nodeSize;
    }
}
