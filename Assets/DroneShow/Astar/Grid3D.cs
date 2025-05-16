using System.Collections.Generic;
using UnityEngine;

public class Grid {
    public int Width;
    public int Height;
    public int Depth;
    public Vector3 GridOrigin { get; set; } = Vector3.zero;

    public float NodeSize;
    public Node[,,] Nodes;

    public Grid(int width, int height, int depth, float nodeSize) {
        Width = width;
        Height = height;
        Depth = depth;
        NodeSize = nodeSize;
        Nodes = new Node[width, height, depth];
        InitializeNodes();
    }

    private void InitializeNodes() {
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                for (int z = 0; z < Depth; z++) {
                    Vector3Int gridPos = new Vector3Int(x, y, z);
                    Nodes[x, y, z] = new Node(gridPos, true); // Assume all nodes are walkable
                }
            }
        }
    }

    public Node GetNode(Vector3Int position) {
        if (position.x >= 0 && position.x < Width &&
            position.y >= 0 && position.y < Height &&
            position.z >= 0 && position.z < Depth) {
            return Nodes[position.x, position.y, position.z];
        }
        return null;
    }

    public List<Node> GetNeighbors(Node node) {
        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                for (int z = -1; z <= 1; z++) {
                    if (x == 0 && y == 0 && z == 0) continue;

                    Vector3Int checkPos = node.Position + new Vector3Int(x, y, z);
                    Node neighbor = GetNode(checkPos);
                    if (neighbor != null && neighbor.Walkable) {
                        neighbors.Add(neighbor);
                    }
                }
            }
        }

        return neighbors;
    }

    public IEnumerable<Node> GetAllNodes() //For gizmos
    {
        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++)
        for (int z = 0; z < Depth; z++)
        {
            yield return Nodes[x, y, z];
        }
    }

    public Vector3Int WorldToCell(Vector3 worldPosition)
    {
        Vector3 relativePos = worldPosition - GridOrigin;

        int x = Mathf.FloorToInt(relativePos.x / NodeSize);
        int y = Mathf.FloorToInt(relativePos.y / NodeSize);
        int z = Mathf.FloorToInt(relativePos.z / NodeSize);

        return new Vector3Int(x, y, z);
    }

    public Vector3 CellToWorld(Vector3Int cell)
    {
        return GridOrigin + new Vector3(cell.x, cell.y, cell.z) * NodeSize;
    }

    public bool IsWalkable(Vector3Int pos)
    {
        var node = GetNode(pos);
        return node != null && node.Walkable;
    }

}
