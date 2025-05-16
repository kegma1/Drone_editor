using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Pool;
using System.Collections.Generic;
using System.Linq;

public class droneShow : MonoBehaviour
{
    private bool firstAnimationPlayed = false;

    public string SourceFilePath;

    private float DroneRadius;

    public int MaxDrones = 800;

    public GameObject dronePrefab;

    private ObjectPool<GameObject> _pool;

    public ComputeShader bezierShader;

    private GameObject currentAnimation = null;

    private Queue<GameObject> activeDrones = new();

    private Queue<GameObject> groundedDrones = new(); 

    private float t = 0f;

    void Start()
    {
        if (dronePrefab != null)
        {
            _pool = new ObjectPool<GameObject>(CreateDrone, null, onDroneRelease, defaultCapacity: MaxDrones, maxSize: MaxDrones);
        }

        if (SourceFilePath != null && bezierShader != null && dronePrefab != null)
        {
            ParseShow();
        }
    }

    private void onDroneRelease(GameObject @object)
    {
        var animComp = @object.GetComponent<AnimationPlayer>();
        animComp.PlayFromStart();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Play();
        }
    }

    private GameObject CreateDrone()
    {
        var drone = Instantiate(dronePrefab);


        var droneComp = drone.GetComponent<Drone>();
        if (droneComp != null)
        {
            droneComp.color = Color.black;
            droneComp.radius = DroneRadius;
        }

        var orca = drone.GetComponent<OrcaAgent>();
        if (orca == null)
        {
            orca = drone.AddComponent<OrcaAgent>();
        }

        float r = orca.Radius;
        orca.maxSpeed = 15f;

        orca.heightTolerance = 1.0f;

        return drone;
    }

    void Play()
    {
        IAnimation AnimationComp;
        Graphic GraphicComp = null;

        if (currentAnimation == null)
        {
            AnimationComp = new StartAnimation();
            AnimationComp.Speed = 25;
            GraphicComp = GetComponentInChildren<Graphic>();
        }
        else
        {
            AnimationComp = currentAnimation.GetComponent<IAnimation>();
            if (AnimationComp == null) return;

            foreach (Transform child in currentAnimation.transform)
            {
                GraphicComp = child.GetComponent<Graphic>();
                if (GraphicComp != null) break;
            }
        }

        if (GraphicComp == null) return;

        var currentDrones = new List<GameObject>();
        var paths = new List<DronePath>();

        if (activeDrones.Count == 0)
        {
            int gridSize = Mathf.CeilToInt(Mathf.Sqrt(MaxDrones));
            float spacing = 5f;  //spacing between drones
            float offset = 20;

            //creates grid of drones
            for (int i = 0; i < MaxDrones; i++)
            {
                int row = i % gridSize;
                int col = i / gridSize;

                float xOffset = row * spacing;
                float zOffset = col * spacing + offset;
                float yOffset = 0f;  

                var drone = _pool.Get();
                drone.transform.position = new Vector3(xOffset, yOffset, zOffset);
                groundedDrones.Enqueue(drone);
            }
        }

        List<Vector3> goals = GraphicComp.edgePoints
            .Select(p => p.ApplyTransformation(GraphicComp.transform, GraphicComp.sceneViewport, GraphicComp.Scale))
            .ToList();

        var difference = Math.Abs(goals.Count - activeDrones.Count);
        for (int i = 0; i < difference; i++)
        {
            if (goals.Count > activeDrones.Count)
            {
                var traitorDrone = groundedDrones.Dequeue();
                activeDrones.Enqueue(traitorDrone);
            }
            else
            {
                var traitorDrone = activeDrones.Dequeue();
                groundedDrones.Enqueue(traitorDrone);
            }
        }



        List<Vector3> starts = activeDrones.Select(d => d.transform.position).ToList();
        

        List<Color> goalColors = GraphicComp.edgePoints
            .Select(p => p.color)
            .ToList();


  
        List<(GameObject drone, Vector3 goal, Color goalColor)> goalAssignments = GoalAssignment.AssignGoalsToDrones(activeDrones.ToList(), goals, goalColors);

 
        if (goalAssignments.Count != activeDrones.Count)
        {
            Debug.LogWarning($"Warning: Number of assigned goals ({goalAssignments.Count}) does not match the number of drones ({activeDrones.Count})");
        }
        else
        {
            Debug.Log($"Success: {goalAssignments.Count} goals assigned to {activeDrones.Count} drones.");
        }

 
        foreach (var (drone, goal, goalColor) in goalAssignments)
        {
            var path = DronePathBuilder.FromStartToGoal(drone.transform.position, goal);
            paths.Add(path);

            var animComp = drone.GetComponent<AnimationPlayer>();
            animComp.targetColor = goalColor;  
        }


        List<MotionPlan> plans;

        // if (!firstAnimationPlayed)
        // {
        //     plans = paths.Select(path => new MotionPlan
        //     {
        //         Path = path,
        //         Speed = AnimationComp.Speed,
        //         TimeOffset = 0f
        //     }).ToList();

        //     firstAnimationPlayed = true;
        // }
        // else
        
        plans = DroneConstraintSolverORTools.SchedulePaths(paths, DroneRadius, AnimationComp.Speed);
        

        for (int i = 0; i < plans.Count; i++)
        {
            GameObject drone = activeDrones.Count > 0 ? activeDrones.Dequeue() : _pool.Get();
            var animComp = drone.GetComponent<AnimationPlayer>();
        

            animComp.Speed = plans[i].Speed ?? AnimationComp.Speed;
            animComp.Path = plans[i].Path;
            animComp.TimeOffset = plans[i].TimeOffset ?? 0f;
            animComp.startPosition = starts[i];

            animComp.PlayFromStart();
            currentDrones.Add(drone);
        }

        activeDrones.Clear(); 

        foreach (var drone in currentDrones)
        {
            activeDrones.Enqueue(drone);
        }

        currentDrones.Clear(); 

        if (currentAnimation == null)
        {
            currentAnimation = transform.GetChild(0).gameObject;
        }
        else
        {
            var nextAnimation = currentAnimation.transform.GetChild(0).gameObject;
            if (nextAnimation) currentAnimation = nextAnimation;
        }
    }


    void ParseShow()
    {
        string JsonContent = File.ReadAllText(SourceFilePath);
        DroneShowData ShowData = JsonConvert.DeserializeObject<DroneShowData>(JsonContent);

        DroneRadius = ShowData.Global.DroneRadius;

        int maxDronesNeeded = CalculateMaxDrones(ShowData.AnimationStart);

        MaxDrones = maxDronesNeeded;

        _pool = new ObjectPool<GameObject>(CreateDrone, null, onDroneRelease, defaultCapacity: MaxDrones, maxSize: MaxDrones);
        
        GetAnimation(ShowData.AnimationStart, transform);
    }

    private int CalculateMaxDrones(AnimationData data)
    {
        int totalEdgePoints = 0;

        if (data != null)
        {
            Graphic graphic = GetGraphic(data, new GameObject());
            totalEdgePoints += graphic.edgePoints.Count;

            if (data.NextAnimation != null)
            {
                totalEdgePoints += CalculateMaxDrones(data.NextAnimation);
            }
        }

        return totalEdgePoints;
    }


    private void GetAnimation(AnimationData data, Transform parent)
    {
        GameObject animationObject = new();
        animationObject.transform.parent = parent;

        string animationType = data.Type;
        Type T = Type.GetType(animationType);
        IAnimation animation = (IAnimation)animationObject.AddComponent(T);

        GetGraphic(data, animationObject);

        if (data.Position != null)
        {
            Vector3 pos = new((float)data.Position[0], (float)data.Position[1], (float)data.Position[2]);
            animationObject.transform.position = pos;
        }
        else
        {
            animationObject.transform.position = parent.position;
        }

        if (data.Rotation != null)
        {
            Vector3 rot = new((float)data.Rotation[0], (float)data.Rotation[1], (float)data.Rotation[2]);
            animationObject.transform.eulerAngles = rot;
        }
        else
        {
            animationObject.transform.eulerAngles = parent.eulerAngles;
        }

        animation.Speed = data.Speed;
        if (data.NextAnimation != null)
        {
            GetAnimation(data.NextAnimation, animationObject.transform);
        }
    }

    private Graphic GetGraphic(AnimationData data, GameObject parent)
    {
        Graphic graphic = parent.AddComponent<Graphic>();
        graphic.svgFilePath = data.Graphic.Source;
        graphic.Scale = data.Graphic.Scale;
        graphic.Duration = data.Graphic.Duration;
        graphic.Outline = data.Graphic.Outline;
        graphic.OutlineSpacing = data.Graphic.OutlineSpacing;
        graphic.Fill = data.Graphic.Fill;
        graphic.FillSpacing = data.Graphic.FillSpacing;
        graphic.FillOffset = new Vector2(data.Graphic.FillOffset[0], data.Graphic.FillOffset[1]);
        graphic.FillRotation = data.Graphic.FillRotation;
        graphic.pointRadius = DroneRadius;
        graphic.bezierShader = bezierShader;

        graphic.GeneratePointsFromPath();
        return graphic;
    }

    void UpdateDrones(List<MotionPlan> plans)
    {
        foreach (var plan in plans)
        {
            var drone = activeDrones.Dequeue(); 
            var animComp = drone.GetComponent<AnimationPlayer>();
            animComp.Path = plan.Path;  
            animComp.Speed = plan.Speed.Value;  
            animComp.TimeOffset = plan.TimeOffset.Value;  
            
            animComp.PlayFromStart();
        }
    }

    public class GoalAssignment
    {
        
        public static List<(GameObject drone, Vector3 goal, Color goalColor)> AssignGoalsToDrones(
            List<GameObject> drones,
            List<Vector3> goals,
            List<Color> goalColors)
        {
            List<(GameObject drone, Vector3 goal, Color goalColor)> assignments = new List<(GameObject, Vector3, Color)>();


            if (drones == null || goals == null || goalColors == null || drones.Count == 0 || goals.Count == 0 || goalColors.Count == 0)
            {
                return assignments; 
            }

    
            List<bool> goalAssigned = new List<bool>(new bool[goals.Count]);

            Vector3 furthestDronePosition = Vector3.zero;
            float maxDistance = 0;
            int goalCount = 0;

            foreach (var drone in drones)
            {
                if (goalCount < goals.Count)
                {
                    Vector3 dronePosition = drone.transform.position;
                    float closestDistance = float.MaxValue;
                    int closestGoalIndex = -1;

                    for (int i = 0; i < goals.Count; i++)
                    {
                        if (goalAssigned[i]) continue; 

                        float distance = Vector3.Distance(dronePosition, goals[i]);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestGoalIndex = i;
                        }
                    }

                    if (closestGoalIndex != -1)
                    {
                        goalAssigned[closestGoalIndex] = true;  
                        goalCount++;
                        assignments.Add((drone, goals[closestGoalIndex], goalColors[closestGoalIndex])); 

                        float distanceToOrigin = dronePosition.magnitude;
                        if (distanceToOrigin > maxDistance)
                        {
                            maxDistance = distanceToOrigin;
                            furthestDronePosition = dronePosition;
                        }
                    }
                }
                else
                {
                    Vector3 direction = (drone.transform.position - furthestDronePosition).normalized;

                    Vector3 relativePosition = drone.transform.position + direction * 3f;

                    assignments.Add((drone, relativePosition, Color.black));
                }
            }


            return assignments;
        }
    }

}