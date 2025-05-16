using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CBS {
    private int astarRunCount = 0;
    private ChunkedGrid grid;
    public AStar astar;
    private const int MaxConstraintCount = 50;

    private const int MaxIterations = 100000;
    private const int ConflictThreshold = 1000;
    private const int MaxOpenListSize = 100000;

    public CBS(ChunkedGrid grid) {
        this.grid = grid;
        this.astar = new AStar(grid);
    }

    public List<List<Vector3>> Solve(List<Vector3> starts, List<Vector3> goals)
    {
        Debug.Log("CBS solving...");

        var open = new MinHeap<CBSNode>();

        var visitedConstraintHashes = new HashSet<string>();

        var root = new CBSNode {
            Constraints = new List<Constraint>(),
            Paths = new List<List<Vector3Int>>()
        };

        for (int i = 0; i < starts.Count; i++) {
            var path = PlanForAgent(i, starts[i], goals[i], root.Constraints);
            if (path == null) return null;
            root.Paths.Add(path);
        }

        // ✅ Enqueue root node with its cost
        open.Enqueue(root, GetTotalCost(root.Paths));

        int iterationCount = 0;

        while (open.Count > 0) {
            if (++iterationCount > MaxIterations) {
                Debug.LogWarning("CBS exceeded maximum iterations.");
                return null;
            }

            if (iterationCount % 1000 == 0) {
                Debug.Log($"[CBS] Iteration {iterationCount}, open size: {open.Count}");
            }

            // ✅ Dequeue the node with lowest cost
            var node = open.Dequeue();

            var conflict = FindFirstConflict(node.Paths);
            if (conflict == null) {
                Debug.Log("[CBS] No conflicts found. Solution complete.");
                Debug.Log($"A* ran {astarRunCount} times.");
                return ConvertToWorldPaths(node.Paths, goals);
            }

            foreach (int agent in new[] { conflict.AgentA, conflict.AgentB }) {
                var newConstraintsSet = new HashSet<Constraint>(node.Constraints);

                bool added;
                if (conflict.IsEdgeConflict) {
                    added = newConstraintsSet.Add(new Constraint(agent, conflict.PositionB, conflict.Position, conflict.TimeStep));
                } else {
                    added = newConstraintsSet.Add(new Constraint(agent, conflict.Position, conflict.TimeStep));
                }

                if (!added) {
                    Debug.LogWarning($"Constraint reapplied for Agent {agent} at {conflict.Position} @ {conflict.TimeStep}");
                }

                // Convert to list for planning
                var tempConstraints = newConstraintsSet.ToList();

                // Plan path with the added constraint
                var newPath = PlanForAgent(agent, starts[agent], goals[agent], tempConstraints);
                if (newPath == null) continue;

                // Estimate arrival time based on newly computed path
                Vector3Int goalCell = grid.WorldToCell(goals[agent]);
                int arrivalTime = newPath.Count - 1;

                // Add reservation constraints AFTER path planning, before storing node
                // for (int t = arrivalTime + 1; t < arrivalTime + 20; t++) {
                //     newConstraintsSet.Add(new Constraint(agent, goalCell, t));
                // }

                var finalConstraints = newConstraintsSet.ToList();
                string constraintHash = GetConstraintHash(finalConstraints);
                if (visitedConstraintHashes.Contains(constraintHash)) continue;

                var newNode = new CBSNode {
                    Constraints = finalConstraints,
                    Paths = node.Paths.Select(p => new List<Vector3Int>(p)).ToList()
                };

                newNode.Paths[agent] = newPath;

                if (CountConflicts(newNode.Paths) > ConflictThreshold) continue;

                visitedConstraintHashes.Add(constraintHash);
                open.Enqueue(newNode, GetTotalCost(newNode.Paths));

                if (open.Count > MaxOpenListSize) {
                    Debug.LogWarning("CBS open list too large — aborting.");
                    return null;
                }
            }

        }


        return null;
    }

    private List<List<Vector3>> ConvertToWorldPaths(List<List<Vector3Int>> gridPaths, List<Vector3> goals)
    {
        var worldPaths = new List<List<Vector3>>();

        for (int i = 0; i < gridPaths.Count; i++) {
            var worldPath = gridPaths[i]
                .Select(v => grid.GridOrigin + new Vector3(v.x, v.y, v.z) * grid.NodeSize)
                .ToList();

            if (!worldPath.Contains(goals[i]))
                worldPath.Add(goals[i]); // Ensure goal is included

            worldPaths.Add(worldPath);
        }

        return worldPaths;
    }

    private string GetConstraintHash(List<Constraint> constraints)
{
    return string.Join("|", constraints
        .OrderBy(c => c.AgentId)
        .ThenBy(c => c.TimeStep)
        .ThenBy(c => c.StartPosition.x)
        .ThenBy(c => c.StartPosition.y)
        .ThenBy(c => c.StartPosition.z)
        .ThenBy(c => c.EndPosition.x)
        .ThenBy(c => c.EndPosition.y)
        .ThenBy(c => c.EndPosition.z)
        .Select(c =>
            $"{c.AgentId}:{c.StartPosition.x},{c.StartPosition.y},{c.StartPosition.z}->{c.EndPosition.x},{c.EndPosition.y},{c.EndPosition.z}@{c.TimeStep}"));
}




    private List<Vector3Int> PlanForAgent(int agentId, Vector3 start, Vector3 goal, List<Constraint> constraints)
    {
        Vector3Int goalCell = grid.WorldToCell(goal);
        if (!grid.IsWalkable(goalCell)) {
            Debug.LogWarning($"[CBS] Goal not walkable for agent {agentId}: {goalCell}");
        }
        
        astarRunCount++;

        return astar.FindPathWithConstraints(start, goal, agentId, constraints);
    }

    private Conflict FindFirstConflict(List<List<Vector3Int>> paths)
{
    int maxLength = paths.Max(p => p.Count);

    for (int t = 0; t < maxLength; t++) {
        for (int i = 0; i < paths.Count; i++) {
            Vector3Int posA = GetPositionAt(paths[i], t);
            for (int j = i + 1; j < paths.Count; j++) {
                Vector3Int posB = GetPositionAt(paths[j], t);

                // Vertex conflict
                if (posA == posB) {
                    return new Conflict {
                        AgentA = i,
                        AgentB = j,
                        Position = posA,
                        TimeStep = t,
                        IsEdgeConflict = false
                    };
                }

                // Edge conflict
                if (t > 0) {
                    Vector3Int prevA = GetPositionAt(paths[i], t - 1);
                    Vector3Int prevB = GetPositionAt(paths[j], t - 1);

                    if (prevA == posB && prevB == posA) {
                        return new Conflict {
                            AgentA = i,
                            AgentB = j,
                            Position = posA, // destination position of agent A
                            TimeStep = t,
                            IsEdgeConflict = true,
                            PositionB = prevA // destination of agent B
                        };
                    }
                }
            }
        }
    }

    return null;
}


    private Vector3Int GetPositionAt(List<Vector3Int> path, int t)
    {
        return t >= path.Count ? path[^1] : path[t];
    }

    private int GetTotalCost(List<List<Vector3Int>> paths)
    {
        return paths.Sum(path => path.Count);
    }

    private int CountConflicts(List<List<Vector3Int>> paths)
    {
        int maxTime = paths.Max(p => p.Count);
        int conflictCount = 0;

        for (int t = 0; t < maxTime; t++)
        {
            var positionsAtT = new Dictionary<Vector3Int, int>();
            for (int i = 0; i < paths.Count; i++) {
                Vector3Int pos = GetPositionAt(paths[i], t);
                if (positionsAtT.ContainsKey(pos))
                    conflictCount++;
                else
                    positionsAtT[pos] = i;
            }
        }

        return conflictCount;
    }
}
