using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CBSNode {
    public List<List<Vector3Int>> Paths; // One path per agent (grid coordinates)
    public List<Constraint> Constraints; // All constraints applied so far
    public int Cost => Paths.Sum(p => p.Count); // Could be makespan instead
}
