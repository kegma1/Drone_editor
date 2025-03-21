using System.Collections.Generic;
using UnityEngine;

public class LerpAnimation : MonoBehaviour, IAnimation
{
    public float Time { get; set; } = 0;
    public float Duration { get; set; }
    public List<DronePath> Paths { get; set; } = new();

    public void GeneratePaths()
    {
        Graphic thisGraphic = GetComponent<Graphic>();
        Graphic nextGraphic = GetComponentInChildren<Graphic>();

        if (thisGraphic.edgePoints.Count < 1) thisGraphic.GeneratePointsFromPath();
        if (nextGraphic.edgePoints.Count < 1) nextGraphic.GeneratePointsFromPath();

        Debug.Log("This graphic: " + thisGraphic.edgePoints.Count);
        Debug.Log("Next graphic: " + nextGraphic.edgePoints.Count);

        for (int i = 0; i < thisGraphic.edgePoints.Count; i++)
        {
            var path = new DronePath() {
                Start = thisGraphic.edgePoints[i].pos,
                ControllA = thisGraphic.edgePoints[i].pos,
                ControllB = nextGraphic.edgePoints[i].pos,
                NextSegment = new() {
                    Start = nextGraphic.edgePoints[i].pos,
                    ControllA = Vector3.zero,
                    ControllB = Vector3.zero,
                    NextSegment = null
                }
            };
            Paths.Add(path);
        }
    }

    public void Play(float t)
    {
        throw new System.NotImplementedException();
    }
}
