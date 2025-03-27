using System.Collections.Generic;
using UnityEngine;

public interface IAnimation {
    public float Time { get; set; }
    public float Duration { get; set; }
    public Dictionary<Vector3, DronePath> Paths { get; set; }

    public DronePath GeneratePaths(Vector3 from, Vector3 to);
    public void Play(float t);
}