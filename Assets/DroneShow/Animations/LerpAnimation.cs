using System.Collections.Generic;
using UnityEngine;

public class LerpAnimation : MonoBehaviour, IAnimation
{
    public float Time { get; set; } = 0;
    public float Duration { get; set; }
    public Dictionary<VirtualDrone, DronePath> Paths { get; set; } = new();

    public void GeneratePaths()
    {
        Graphic thisGraphic = GetComponent<Graphic>();
        Graphic nextGraphic = null;

        foreach (Transform child in transform) {
            nextGraphic = child.GetComponent<Graphic>();
            if (nextGraphic != null)
                break;
        }

        if (nextGraphic == null) {
            Debug.Log("no NextGraphic found");
            return;
        }

        if (thisGraphic.edgePoints.Count < 1) thisGraphic.GeneratePointsFromPath();
        if (nextGraphic.edgePoints.Count < 1) nextGraphic.GeneratePointsFromPath();

        Debug.Log("This graphic: " + thisGraphic.edgePoints.Count);
        Debug.Log("Next graphic: " + nextGraphic.edgePoints.Count);

        for (int i = 0; i < thisGraphic.edgePoints.Count; i++)
        {
            var thisPoint = thisGraphic.edgePoints[i].ApplyTransformation(thisGraphic.transform, thisGraphic.sceneViewport, thisGraphic.Scale);
            // var nextPoint = Vector3.zero;
            var nextPoint = nextGraphic.edgePoints[i].ApplyTransformation(nextGraphic.transform, nextGraphic.sceneViewport, nextGraphic.Scale);
     
            var path = new DronePath() {
                Start = thisPoint,
                ControllA = thisPoint,
                ControllB = nextPoint,
                NextSegment = new() {
                    Start = nextPoint,
                    ControllA = null,
                    ControllB = null,
                    NextSegment = null
                }
            };
            Paths[thisGraphic.edgePoints[i]] = path;
        }
    }

    public void Play(float t)
    {
        throw new System.NotImplementedException();
    }
}
