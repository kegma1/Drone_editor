using System.Collections.Generic;
using UnityEngine;

public class NoneAnimation : MonoBehaviour, IAnimation
{
    public float Time { get; set; } = 0;
    public float Duration { get; set; }
    public Dictionary<Vector3, DronePath> Paths { get; set; }

    public DronePath GeneratePaths(Vector3 from, Vector3 to) {return new DronePath();}

    public void Play(float t) {}
}
