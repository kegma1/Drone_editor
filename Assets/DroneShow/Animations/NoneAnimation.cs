using System.Collections.Generic;
using UnityEngine;

public class NoneAnimation : MonoBehaviour, IAnimation
{
    public float Duration { get; set; }
    public Dictionary<Vector3, DronePath> Paths { get; set; }

    public DronePath GeneratePath(Vector3 from, Vector3 to) {return new DronePath();}
}
