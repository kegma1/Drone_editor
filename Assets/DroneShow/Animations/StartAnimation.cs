using System.Collections.Generic;
using UnityEngine;

public class StartAnimation : IAnimation
{
    public float Speed { get; set; }
    public Dictionary<Vector3, DronePath> Paths { get; set; } = new();

    public DronePath GeneratePath(Vector3 from, Vector3 to)
    {
        var path = new DronePath() {
            Start = from,
            ControllA = from,
            ControllB = to,
            NextSegment = new() {
                Start = to,
                ControllA = null,
                ControllB = null,
                NextSegment = null
            }
        };
        Paths[from] = path; 
        return path;
    }
}
