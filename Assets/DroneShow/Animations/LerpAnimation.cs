using System.Collections.Generic;
using UnityEngine;

public class LerpAnimation : MonoBehaviour, IAnimation
{
    public float Speed { get; set; }
    public Dictionary<Vector3Int, List<DronePath>> Paths { get; set; } = new();

    public DronePath GeneratePath(Vector3 from, Vector3 to, List<DronePath> existingPaths = null)
    {
        var path = new DronePath
        {
            Start = from,
            ControllA = from,
            ControllB = to,
            Waypoints = new List<Vector3> { from, to },
            NextSegment = new DronePath
            {
                Start = to,
                ControllA = null,
                ControllB = null,
                Waypoints = new List<Vector3> { to },
                NextSegment = null
            }
        };

        Vector3Int key = Vector3Int.FloorToInt(from);

        if (!Paths.ContainsKey(key))
            Paths[key] = new List<DronePath>();

        Paths[key].Add(path);

        return path;
    }
}
