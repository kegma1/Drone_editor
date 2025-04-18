using System.Collections.Generic;
using UnityEngine;

public interface IAnimation {
    public float Speed { get; set; }
    public Dictionary<Vector3, DronePath> Paths { get; set; }

    public DronePath GeneratePath(Vector3 from, Vector3 to);
}