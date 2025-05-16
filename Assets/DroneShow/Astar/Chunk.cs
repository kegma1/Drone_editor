using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public Vector3Int ChunkCoord { get; }
    private Dictionary<Vector3Int, Node> nodes = new();
    private float nodeSize;
    public static int ChunkSize = 16;

    public Chunk(Vector3Int coord, float nodeSize)
    {
        ChunkCoord = coord;
        this.nodeSize = nodeSize;
    }

    public Node GetOrCreateNode(Vector3Int localPos)
{
    if (!nodes.TryGetValue(localPos, out var node))
    {
        node = new Node(localPos + ChunkCoord * Chunk.ChunkSize)
        {
            Walkable = true // ✅ Make it walkable by default!
        };
        nodes[localPos] = node;
    }
    return node;
}


    public Node GetNode(Vector3Int localPos)
{
    nodes.TryGetValue(localPos, out var node);
    return node;
}



    public IEnumerable<Node> Nodes => nodes.Values;
    
}
