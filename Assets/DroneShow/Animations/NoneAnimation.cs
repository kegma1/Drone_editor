using System.Collections.Generic;
using UnityEngine;

public class NoneAnimation : MonoBehaviour, IAnimation
{
    public float Time { get; set; } = 0;
    public float Duration { get; set; }
    public Dictionary<VirtualDrone, DronePath> Paths { get; set; }

    public void GeneratePaths() {}

    public void Play(float t) {}
}
