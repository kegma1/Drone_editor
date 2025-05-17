using System.Collections.Generic;
using UnityEngine;


public class NoneAnimation : MonoBehaviour, IAnimation
{
    public float Speed { get; set; }
    public Dictionary<Vector3Int, List<DronePath>> Paths { get; set; } = new();


    public DronePath GeneratePath(Vector3 from, Vector3 to, List<DronePath> existingPaths = null) {return new DronePath();}
}
