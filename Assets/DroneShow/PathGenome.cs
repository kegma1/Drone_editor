using UnityEngine;
using System.Collections.Generic;

public class PathGenome {
    public List<Vector3> ControlPoints = new();
    public float Fitness;

    public DronePath ToDronePath(Vector3 start, Vector3 end)
{
    DronePath head = new DronePath { Start = start };
    DronePath current = head;
    Vector3 lastPos = start;

    for (int i = 0; i < ControlPoints.Count; i += 2)
    {
        var a = ControlPoints[i];
        var b = ControlPoints[i + 1];

        var next = new DronePath
        {
            Start = lastPos,
            ControllA = a,
            ControllB = b
        };

        current.NextSegment = next;
        current = next;
        lastPos = BezierEvaluate(lastPos, a, b, 1f);
    }

    current.Start = end;
    return head;
}

    public static Vector3 BezierEvaluate(Vector3 start, Vector3 a, Vector3 b, float t)
    {
        float u = 1f - t;
        return u * u * start + 2f * u * t * a + t * t * b;
    }

}
