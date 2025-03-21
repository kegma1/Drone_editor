using System.Collections.Generic;
using UnityEngine;

public class NoneAnimation : MonoBehaviour, IAnimation
{
    // public Graphic Graphic { get; set; }
    public float Time { get; set; } = 0;
    public float Duration { get; set; }
    public List<DronePath> Paths { get; set; }
    // public IAnimation NextAnimation { get; set; }
    // public Vector3? Position { get; set; }
    // public Vector3? Rotation { get; set; }

    public void GeneratePaths() {}

    public void Play() {}
}
