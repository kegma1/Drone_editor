using UnityEngine;
using System.Collections.Generic;

public interface IAnimation
{
    float Speed { get; set; }

    // Change Vector3 to Vector3Int
    Dictionary<Vector3Int, List<DronePath>> Paths { get; set; }

    DronePath GeneratePath(Vector3 from, Vector3 to, List<DronePath> existingPaths);
}
