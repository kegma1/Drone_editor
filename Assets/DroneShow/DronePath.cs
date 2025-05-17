using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DronePath
{
    public Vector3 Start;
    public Vector3 End;
    public List<Vector3> SmoothedPoints;

    public Vector3? ControllA;
    public Vector3? ControllB;
    public List<Vector3> Waypoints;
    public DronePath NextSegment;


    public List<Vector3> SamplePath(float timestep, float speed)
    {
        if (SmoothedPoints.Count < 2) return new List<Vector3>();

        List<Vector3> samples = new();
        float distancePerStep = timestep * speed;

        float totalDistance = 0f;
        int currentIndex = 0;

        samples.Add(SmoothedPoints[0]);

        while (currentIndex < SmoothedPoints.Count - 1)
        {
            Vector3 from = SmoothedPoints[currentIndex];
            Vector3 to = SmoothedPoints[currentIndex + 1];
            float segmentDistance = Vector3.Distance(from, to);

            if (totalDistance + segmentDistance >= samples.Count * distancePerStep)
            {
                float remaining = (samples.Count * distancePerStep) - totalDistance;
                Vector3 samplePoint = Vector3.Lerp(from, to, remaining / segmentDistance);
                samples.Add(samplePoint);
            }
            else
            {
                totalDistance += segmentDistance;
                currentIndex++;
            }
        }

        return samples;
    }


    public List<Vector3> SamplePathByTime(float totalTime, float timestep)
    {
        if (SmoothedPoints == null || SmoothedPoints.Count < 2)
            return new List<Vector3>();

        List<Vector3> samples = new();
        float totalPathLength = GetPathLength();
        float speed = totalPathLength / totalTime; 

        float distancePerStep = speed * timestep;
        float accumulatedDistance = 0f;
        int currentIndex = 0;

        samples.Add(SmoothedPoints[0]);

        while (currentIndex < SmoothedPoints.Count - 1)
        {
            Vector3 from = SmoothedPoints[currentIndex];
            Vector3 to = SmoothedPoints[currentIndex + 1];
            float segmentDistance = Vector3.Distance(from, to);

            if (accumulatedDistance + segmentDistance >= samples.Count * distancePerStep)
            {
                float remaining = (samples.Count * distancePerStep) - accumulatedDistance;
                Vector3 samplePoint = Vector3.Lerp(from, to, remaining / segmentDistance);
                samples.Add(samplePoint);
            }
            else
            {
                accumulatedDistance += segmentDistance;
                currentIndex++;
            }
        }

        if (samples[samples.Count - 1] != SmoothedPoints[^1])
            samples.Add(SmoothedPoints[^1]);

        return samples;
    }


    public float GetPathLength()
    {
        if (SmoothedPoints == null || SmoothedPoints.Count < 2)
            return 0f;

        float length = 0f;
        for (int i = 1; i < SmoothedPoints.Count; i++)
        {
            length += Vector3.Distance(SmoothedPoints[i - 1], SmoothedPoints[i]);
        }
        return length;
    }


}
