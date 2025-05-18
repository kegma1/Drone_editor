using System.Collections.Generic;
using UnityEngine;

public static class WaypointGenerator
{
    public static List<Vector3> GenerateWaypointsBetween(Vector3 start, Vector3 goal, int waypointCount = 3, float offsetStrength = 0.2f, float lateralCurve = 1f)
    {
        List<Vector3> waypoints = new();
        Vector3 direction = (goal - start);
        float totalDistance = direction.magnitude;
        Vector3 forward = direction.normalized;

        Vector3 up = Vector3.up;
        Vector3 right = Vector3.Cross(forward, up).normalized;

        for (int i = 1; i <= waypointCount; i++)
        {
            float t = i / (float)(waypointCount + 1);
            Vector3 basePoint = Vector3.Lerp(start, goal, t);

            float heightOffset = Mathf.Sin(t * Mathf.PI) * totalDistance * offsetStrength;
            float lateralOffset = Mathf.Sin(t * Mathf.PI) * totalDistance * offsetStrength * lateralCurve;

            lateralOffset *= 2f; 

            Vector3 offset = up * heightOffset + right * lateralOffset;
            Vector3 waypoint = basePoint + offset;

            waypoints.Add(waypoint);
        }

        return waypoints;
    }

}