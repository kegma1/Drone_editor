using UnityEngine;
using Nebukam.ORCA;
using Unity.Mathematics;
using System.Collections.Generic;
using Nebukam.Common;


public class OrcaSimulationManager : MonoBehaviour
{
    public static OrcaSimulationManager Instance { get; private set; }
    public ORCABundle<Agent> bundle;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            bundle = new ORCABundle<Agent>();
            bundle.plane = AxisPair.XZ; 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        bundle.orca.Schedule(Time.deltaTime);
    }

    void LateUpdate()
    {
    
        bundle.orca.Complete();
    }

}
