using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AStar {
    private ChunkedGrid grid;
    public Vector3 GridOrigin = Vector3.zero;

    public AStar(ChunkedGrid grid) {
        this.grid = grid;
    }

    public List<Vector3> FindPath(Vector3 start, Vector3 end) {
    List<Node> visitedNodes = new();

    Node startNode = GetNodeFromWorldPosition(start);
    Node endNode = GetNodeFromWorldPosition(end);

    if (startNode == null || endNode == null || !startNode.Walkable || !endNode.Walkable) {
        Debug.LogWarning($"[AStar] Invalid start or end node. Start: {startNode?.Position}, End: {endNode?.Position}, Walkable: {startNode?.Walkable}, {endNode?.Walkable}");
        return null;
    }

    Debug.Log($"[AStar] Starting pathfinding from {startNode.Position} to {endNode.Position}");

    // Use MinHeap for the open list
    MinHeap<Node> openList = new MinHeap<Node>();
    HashSet<Node> closedList = new HashSet<Node>();

    startNode.GCost = 0;
    startNode.HCost = GetDistance(startNode, endNode);
    openList.Enqueue(startNode, startNode.FCost);  // Enqueue with FCost as priority
    visitedNodes.Add(startNode);

    int maxIterations = 10000;
    int iterationCount = 0;

    while (openList.Count > 0) {
        if (++iterationCount > maxIterations) {
            Debug.LogWarning("[AStar] Exceeded max iterations.");
            ResetNodes(visitedNodes);
            return null;
        }

        Node currentNode = openList.Dequeue();  // Get the node with the lowest FCost

        // Debug chunk move logging
        Vector3Int currentChunk = grid.GetChunkCoord(currentNode.Position);
        if (currentNode.Parent != null) {
            Vector3Int parentChunk = grid.GetChunkCoord(currentNode.Parent.Position);
            if (currentChunk != parentChunk) {
                Debug.Log($"[AStar] Drone moved from chunk {parentChunk} to {currentChunk} at node {currentNode.Position}");
            }
        }

        closedList.Add(currentNode);

        // If we reach the goal, retrace the path
        if (currentNode == endNode) {
            Debug.Log("[AStar] Goal reached. Retracing path.");
            var result = RetracePath(startNode, endNode);
            ResetNodes(visitedNodes);
            return result;
        }

        // Explore neighbors
        foreach (Node neighbor in grid.GetNeighbors(currentNode)) {
            if (!neighbor.Walkable || closedList.Contains(neighbor)) continue;

            int newCost = currentNode.GCost + GetDistance(currentNode, neighbor);
            if (newCost < neighbor.GCost || !openList.Contains(neighbor)) {
                neighbor.GCost = newCost;
                neighbor.HCost = GetDistance(neighbor, endNode);
                neighbor.Parent = currentNode;

                if (!visitedNodes.Contains(neighbor)) visitedNodes.Add(neighbor);
                if (!openList.Contains(neighbor)) {
                    openList.Enqueue(neighbor, neighbor.FCost);  // Enqueue with updated FCost
                }
            }
        }
    }

    Debug.LogWarning("[AStar] No path found.");
    ResetNodes(visitedNodes);
    return null;
}


    public List<Vector3Int> FindPathWithConstraints(Vector3 start, Vector3 end, int agentId, List<Constraint> constraints) {
        List<Node> visitedNodes = new();

        Node startNode = GetNodeFromWorldPosition(start);
        Node endNode = GetNodeFromWorldPosition(end);

        if (startNode == null || endNode == null || !startNode.Walkable || !endNode.Walkable)
            return null;

        var openList = new MinHeap<(Node node, int timeStep)>();
        var closedSet = new HashSet<(Vector3Int pos, int timeStep)>();

        startNode.GCost = 0;
        startNode.HCost = GetDistance(startNode, endNode);
        openList.Enqueue((startNode, 0), startNode.FCost);
        visitedNodes.Add(startNode);

        int maxIterations = 1000000;
        int iterationCount = 0;

        while (openList.Count > 0) {
            if (++iterationCount > maxIterations) {
                Debug.LogWarning("Exceeded max iterations in A* with constraints.");
                ResetNodes(visitedNodes);
                break;
            }

            var (currentNode, timeStep) = openList.Dequeue();

            if (currentNode.Position == endNode.Position &&
                !ViolatesConstraint(agentId, currentNode.Position, currentNode.Position, timeStep, constraints)) {
                var result = RetracePathGrid(startNode, currentNode);
                ResetNodes(visitedNodes);
                return result;
            }

            closedSet.Add((currentNode.Position, timeStep));


            foreach (var neighbor in grid.GetNeighbors(currentNode)) {
                if (!neighbor.Walkable) continue;

                if (ViolatesConstraint(agentId, currentNode.Position, neighbor.Position, timeStep + 1, constraints))
                    continue;

                int gCost = currentNode.GCost + GetDistance(currentNode, neighbor);

                if (closedSet.Contains((neighbor.Position, timeStep + 1)) || gCost >= neighbor.GCost)
                    continue;

                neighbor.GCost = gCost;
                neighbor.HCost = GetDistance(neighbor, endNode);
                neighbor.Parent = currentNode;

                if (!visitedNodes.Contains(neighbor)) visitedNodes.Add(neighbor);
                openList.Enqueue((neighbor, timeStep + 1), neighbor.FCost + timeStep + 1);
            }
        }

        ResetNodes(visitedNodes);
        return null;
    }

    private void ResetNodes(List<Node> visitedNodes) {
        foreach (var node in visitedNodes) {
            node.GCost = int.MaxValue;
            node.HCost = 0;
            node.Parent = null;
        }
    }

    private bool ViolatesConstraint(int agentId, Vector3Int startPosition, Vector3Int endPosition, int timeStep, List<Constraint> constraints) {
        foreach (var constraint in constraints) {
            if (constraint.AgentId == agentId && constraint.TimeStep == timeStep) {
                //Edge constraint
                if (IsAdjacent(startPosition, endPosition, constraint.StartPosition, constraint.EndPosition)) {
                    Debug.Log($"[A*] Constraint hit for Agent {agentId} at t={timeStep} from {startPosition} to {endPosition}");
                    return true;
                }

                // Vertex constraint
                if (constraint.StartPosition == endPosition && constraint.EndPosition == endPosition) {
                    Debug.Log($"[A*] Constraint hit for Agent {agentId} at t={timeStep} at vertex {endPosition}");
                    return true;
                }
            }
        }
        return false;
    }

    private bool IsAdjacent(Vector3Int start1, Vector3Int end1, Vector3Int start2, Vector3Int end2) {
        return (start1 == start2 && end1 == end2) || (start1 == end2 && end1 == start2);
    }

    private List<Vector3Int> RetracePathGrid(Node startNode, Node endNode) {
        List<Vector3Int> path = new();
        Node current = endNode;
        while (current != startNode) {
            path.Add(current.Position);
            current = current.Parent;
        }
        path.Add(startNode.Position);
        path.Reverse();
        return path;
    }

private List<Vector3> RetracePath(Node startNode, Node endNode) {
    List<Node> path = new();
    Node currentNode = endNode;
    while (currentNode != startNode) {
        path.Add(currentNode);
        currentNode = currentNode.Parent;
    }
    path.Add(startNode);
    path.Reverse();


    return path.Select(n =>
        grid.GridOrigin + new Vector3(
            n.Position.x * grid.NodeSize,
            n.Position.y * grid.NodeSize,
            n.Position.z * grid.NodeSize
        )).ToList();
}


    private Node GetNodeFromWorldPosition(Vector3 worldPosition) {
    Vector3 localPos = worldPosition - grid.GridOrigin;

    int x = Mathf.FloorToInt(localPos.x / grid.NodeSize);
    int y = Mathf.FloorToInt(localPos.y / grid.NodeSize);
    int z = Mathf.FloorToInt(localPos.z / grid.NodeSize);
    Vector3Int gridPos = new Vector3Int(x, y, z);

    // ✅ Always creates the chunk and node
    Node node = grid.GetNode(gridPos);

    if (node == null || !node.Walkable) {
        Debug.LogWarning($"[AStar] Node at {gridPos} is not walkable or missing.");
        return null;
    }

    return node;
}

    
    private int GetDistance(Node a, Node b) {
    int dstX = Mathf.Abs(a.Position.x - b.Position.x);
    int dstY = Mathf.Abs(a.Position.y - b.Position.y);
    int dstZ = Mathf.Abs(a.Position.z - b.Position.z);

    // Diagonal movement cost: prioritize straight movement if possible
    int minXY = Mathf.Min(dstX, dstY);
    int minXZ = Mathf.Min(dstX, dstZ);
    int minYZ = Mathf.Min(dstY, dstZ);
    
    // Assuming cost: straight = 10, diagonal (2D) = 14, 3D diagonal ≈ 17
    return 17 * Mathf.Min(minXY, minXZ, minYZ) +
           14 * (Mathf.Max(minXY, minXZ, minYZ) - Mathf.Min(minXY, minXZ, minYZ)) +
           10 * (dstX + dstY + dstZ - Mathf.Max(minXY, minXZ, minYZ) - Mathf.Min(minXY, minXZ, minYZ));
}

}
