using System.Collections.Generic;
using System.Numerics;

public interface IAnimation {
    public float Time { get; set; }
    public float Duration { get; set; }
    public Dictionary<VirtualDrone, DronePath> Paths { get; set; }

    public void GeneratePaths(Vector3 from, Vector3 to);
    public void Play(float t);
}