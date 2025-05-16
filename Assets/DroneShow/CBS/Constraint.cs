using UnityEngine;

public class Constraint {
    public int AgentId;
    public Vector3Int StartPosition;
    public Vector3Int EndPosition;
    public int TimeStep;

    public Constraint(int agentId, Vector3Int pos, int timeStep) {
        AgentId = agentId;
        StartPosition = pos;
        EndPosition = pos;
        TimeStep = timeStep;
    }

    public Constraint(int agentId, Vector3Int from, Vector3Int to, int timeStep) {
        AgentId = agentId;
        StartPosition = from;
        EndPosition = to;
        TimeStep = timeStep;
    }

    public override bool Equals(object obj) {
        if (obj is not Constraint other) return false;
        return AgentId == other.AgentId &&
               StartPosition == other.StartPosition &&
               EndPosition == other.EndPosition &&
               TimeStep == other.TimeStep;
    }

    public override int GetHashCode() {
        return AgentId.GetHashCode() ^
               StartPosition.GetHashCode() ^
               EndPosition.GetHashCode() ^
               TimeStep.GetHashCode();
    }
}
