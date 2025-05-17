using Google.OrTools.Sat;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class DroneConstraintSolverORTools
{
    public static List<MotionPlan> SchedulePaths(List<DronePath> paths, float droneRadius, float maxSpeed, float minSeparationTime = 0.5f)
    {
        Debug.Log("Solver is running...");
        var solver = new CpModel();

        int numDrones = paths.Count;
        int maxTime = 10000;
        int baseDuration = 20;
        float cullDistance = droneRadius;
        float cellSize = cullDistance;
        float timestep = 0.05f;


        var starts = new IntVar[numDrones];
        var durations = new IntVar[numDrones];


        var commonStartTime = solver.NewIntVar(0, maxTime, "common_start_time");

        for (int i = 0; i < numDrones; i++)
        {
            starts[i] = commonStartTime;

            float pathLength = paths[i].GetPathLength();
            int minDuration = Mathf.CeilToInt((pathLength / maxSpeed) * 100f); 

            durations[i] = solver.NewIntVar(minDuration, maxTime, $"duration_{i}");
        }


        var sampledPaths = new List<List<Vector3>>();

        for (int i = 0; i < numDrones; i++)
        {
            float estimatedDuration = baseDuration / 100f; 
            var sampledPath = paths[i].SamplePathByTime(estimatedDuration, timestep);
            sampledPaths.Add(sampledPath);
        }

        int minLength = sampledPaths.Min(p => p.Count);

        //neighbor offsets
        Vector3Int[] neighborOffsets = (
            from x in new[] { -1, 0, 1 }
            from y in new[] { -1, 0, 1 }
            from z in new[] { -1, 0, 1 }
            where !(x == 0 && y == 0 && z == 0)
            select new Vector3Int(x, y, z)
        ).ToArray();

        //collision constraints with spatial hashing
        for (int t = 0; t < minLength; t++)
        {
            Dictionary<Vector3Int, List<int>> spatialHash = new();

            for (int i = 0; i < numDrones; i++)
            {
                Vector3 pos = sampledPaths[i][t];
                Vector3Int cell = HashPosition(pos, cellSize);

                if (!spatialHash.ContainsKey(cell))
                    spatialHash[cell] = new List<int>();

                spatialHash[cell].Add(i);
            }

            foreach (var kvp in spatialHash)
            {
                Vector3Int cell = kvp.Key;
                List<int> dronesInCell = kvp.Value;

                foreach (var offset in neighborOffsets)
                {
                    Vector3Int neighbor = cell + offset;

                    if (!spatialHash.TryGetValue(neighbor, out var neighborDrones))
                        continue;

                    foreach (int i in dronesInCell)
                    {
                        foreach (int j in neighborDrones)
                        {
                            if (j <= i) continue;

                            Vector3 posA = sampledPaths[i][t];
                            Vector3 posB = sampledPaths[j][t];
                            float dist = Vector3.Distance(posA, posB);

                            if (dist < droneRadius*0.5)
                            {
                                Debug.Log($"Conflict detected at time {t} between drones {i} and {j}, distance: {dist}");

                                var noOverlap1 = solver.NewBoolVar($"no_overlap1_{i}_{j}_{t}");
                                var noOverlap2 = solver.NewBoolVar($"no_overlap2_{i}_{j}_{t}");

                                solver.Add(starts[i] + durations[i] <= starts[j]).OnlyEnforceIf(noOverlap1);
                                solver.Add(starts[j] + durations[j] <= starts[i]).OnlyEnforceIf(noOverlap2);
                                solver.AddBoolOr(new[] { noOverlap1, noOverlap2 });
                            }
                        }
                    }
                }
            }
        }

        //synced arrival
        IntVar commonEndTime = solver.NewIntVar(0, maxTime, "common_end_time");
        for (int i = 0; i < numDrones; i++)
        {
            IntVar endTime = solver.NewIntVar(0, maxTime, $"end_{i}");
            solver.Add(endTime == starts[i] + durations[i]);
            solver.Add(endTime == commonEndTime); 
        }

        //Maximize speed
        var durationSum = new List<IntVar>();
        for (int i = 0; i < numDrones; i++)
        {
            durationSum.Add(durations[i]);
        }

        var totalDuration = LinearExpr.Sum(durationSum); 

        solver.Minimize(totalDuration);


        var cpSolver = new CpSolver();
        var status = cpSolver.Solve(solver);

        var plans = new List<MotionPlan>();

        if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
        {
            for (int i = 0; i < numDrones; i++)
            {
                float offset = (float)cpSolver.Value(commonStartTime) / 100f;

                float duration = (float)cpSolver.Value(durations[i]) / 100f;
                float pathLength = paths[i].GetPathLength();
                float speed = pathLength / duration;

                plans.Add(new MotionPlan
                {
                    Path = paths[i],
                    TimeOffset = offset,
                    Speed = speed
                });
            }

        
            // foreach (var plan in plans)
            // {
            //     Debug.Log($"Path: {plan.Path}, TimeOffset: {plan.TimeOffset}, Speed: {plan.Speed}");
            // }
        }
        else
        {
            Debug.LogError("Failed to find a feasible solution with OR-Tools.");

            Debug.LogError($"Solver status: {status}");
        }

        return plans;
    }

    private static Vector3Int HashPosition(Vector3 pos, float cellSize)
    {
        return new Vector3Int(
            Mathf.FloorToInt(pos.x / cellSize),
            Mathf.FloorToInt(pos.y / cellSize),
            Mathf.FloorToInt(pos.z / cellSize)
        );
    }
}
