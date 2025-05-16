using UnityEngine;
using Nebukam.ORCA;
using Unity.Mathematics;
using System.Collections.Generic;

public class OrcaAgent : MonoBehaviour
{
    public float baseY;
    public float heightTolerance = 0.1f;
    public float maxSpeed = 15.0f;

    private Agent orcaAgent;
    public Agent ORCAAgent => orcaAgent;
    public float Radius => orcaAgent?.radius ?? 0.0f;
    private float? _targetY = null;

    public void SetTargetHeight(float y)
    {
        _targetY = y;
    }

    void Start()
    {
        baseY = transform.position.y;

        var bundle = OrcaSimulationManager.Instance?.bundle;
        if (bundle != null)
        {
            float3 pos3D = new float3(transform.position.x, transform.position.y, transform.position.z);
            orcaAgent = bundle.NewAgent(pos3D);  
            orcaAgent.radius = 0.5f;
            orcaAgent.maxSpeed = maxSpeed;
            orcaAgent.timeHorizon = 1f; //how far into the future orca predicts collisions with agents
            orcaAgent.height = 1.0f;
            orcaAgent.radiusObst = 1.0f;
            orcaAgent.maxNeighbors = 25;
            orcaAgent.neighborDist = 5.0f;
            orcaAgent.timeHorizonObst = 1.0f;
            orcaAgent.navigationEnabled = true;
            orcaAgent.collisionEnabled = true;
        }
    }

    void LateUpdate()
    {
        if (orcaAgent == null) return;

        orcaAgent.pos = new float3(transform.position.x, transform.position.y, transform.position.z);
    }


    public void SetPreferredVelocity(Vector3 direction)
    {
        if (orcaAgent == null) return;

        float3 flatDir = new float3(direction.x, 0f, direction.z);

        if (math.length(flatDir) > orcaAgent.maxSpeed)
        {
            flatDir = math.normalize(flatDir) * orcaAgent.maxSpeed;
        }

        orcaAgent.prefVelocity = flatDir;
    }

    public bool GetTargetHeight(out float target)
    {

        if (_targetY.HasValue)
        {
            target = _targetY.Value;
            return true;
        }
        target = baseY;
        return false;
    }

}