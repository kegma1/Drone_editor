using UnityEngine;

public class Conflict {
    public int AgentA, AgentB;
    public Vector3Int Position;    // Used for vertex conflicts OR destination of edge
    public Vector3Int PositionB;   // Only used in edge conflicts (source of edge for AgentB)
    public int TimeStep;
    public bool IsEdgeConflict;

    public Conflict() {
        IsEdgeConflict = false;
        PositionB = Vector3Int.zero;
    }
}
