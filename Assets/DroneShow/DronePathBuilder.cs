using System.Collections.Generic;
using UnityEngine;

public static class DronePathBuilder
{

   public static DronePath FromVectorPath(List<Vector3> rawPath, Vector3? trueFinalGoal = null)
    {
        if (rawPath == null || rawPath.Count < 2)
        {
            return new DronePath
            {
                Start = rawPath != null && rawPath.Count > 0 ? rawPath[0] : Vector3.zero,
                SmoothedPoints = rawPath ?? new List<Vector3>()
            };
        }


        rawPath = PadEndpoints(rawPath);
        var smoothed = GenerateCatmullRomPath(rawPath, 10);


        Vector3 finalGoal = trueFinalGoal ?? rawPath[^2]; 
        if ((smoothed[^1] - finalGoal).sqrMagnitude > 0.001f)
        {
            smoothed.Add(finalGoal); 
        }

        return new DronePath { SmoothedPoints = smoothed };
    }



    public static List<Vector3> GenerateCatmullRomPath(List<Vector3> points, int resolutionPerSegment = 10)
    {
        if (points.Count < 4)
            return new List<Vector3>(points);  

        List<Vector3> smoothedPath = new();
        
        for (int i = 0; i < points.Count - 3; i++)
        {
            Vector3 p0 = points[i];
            Vector3 p1 = points[i + 1];
            Vector3 p2 = points[i + 2];
            Vector3 p3 = points[i + 3];

            float angle = Vector3.Angle(p1 - p0, p2 - p1) + Vector3.Angle(p2 - p1, p3 - p2);

            int segmentResolution = resolutionPerSegment;
            if (angle > 45f)  
            {
                segmentResolution = Mathf.Max(resolutionPerSegment / 2, 5);  
            }

            for (int j = 0; j <= segmentResolution; j++)
            {
                float t = j / (float)segmentResolution;
                Vector3 interpolated = CatmullRom(p0, p1, p2, p3, t);
                smoothedPath.Add(interpolated);
            }
        }

        return smoothedPath;
    }


    public static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            2f * p1 +
            (p2 - p0) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }

    private static List<Vector3> PadEndpoints(List<Vector3> path)
    {
        if (path.Count < 2)
            return new List<Vector3>(path);

        Vector3 first = path[0];
        Vector3 second = path[1];
        Vector3 last = path[^1];
        Vector3 secondLast = path[^2];

        List<Vector3> padded = new();
        padded.Add(first + (first - second));   
        padded.AddRange(path);
        padded.Add(last + (last - secondLast));      

        return padded;
    }

    public static DronePath FromStartToGoal(Vector3 start, Vector3 goal)
    {
        List<Vector3> controlPoints = new() { start };

        controlPoints.AddRange(WaypointGenerator.GenerateWaypointsBetween(start, goal, 20, 0.15f, 0.6f));

        controlPoints.Add(goal);

        controlPoints = PadEndpoints(controlPoints);
        var smoothed = GenerateCatmullRomPath(controlPoints, 40);

        return new DronePath { Start = start, End = goal, SmoothedPoints = smoothed };
    }


}
