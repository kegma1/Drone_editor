using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Pool;
using System.Collections.Generic;
using SimpleFileBrowser;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;
using System.Linq;


public class droneShow : MonoBehaviour
{
    private bool firstAnimationPlayed = false;

    public ErrorManager errorManager;
    public string SourceFilePath;

    private float DroneRadius;

    public int MaxDrones = 1500;

    public GameObject dronePrefab;

    private ObjectPool<GameObject> _pool;

    public ComputeShader bezierShader;

    private GameObject currentAnimation = null;

    private Queue<GameObject> activeDrones = new();
    private Queue<GameObject> groundedDrones = new(); 

    private bool gridCreated = false;
    public InGameMenuController ui;
    public TMP_Text counter;

    public bool isShowRunning = false;
    
    private bool _isPaused = false;
    public bool IsPaused {
        get => _isPaused;
        set {
            _isPaused = value;
            foreach (var indicator in playPauseIndicators) {
                indicator.isPaused = _isPaused;
            }
        }
    } 

    private bool isLoopingNextCycle = false;
    private float elapsedShowTime = 0f;
    private float ShowLength = 0;

    public List<PlayPauseManager> playPauseIndicators;



    public bool IsLooping;

    [Range(5f, 120f)]
    public float animationInterval = 30f;
    public float animationTimer = 0f;

    public InputActionAsset inputActions;
    private InputAction playPauseAction;

    private InputAction restartAction;



    private float t = 0f;

    void Start()
    {
        if (dronePrefab != null)
        {
            _pool = new ObjectPool<GameObject>(CreateDrone, null, onDroneRelease, defaultCapacity: MaxDrones);
        }

        
        var playerMap = inputActions.FindActionMap("Player");
        playPauseAction = playerMap.FindAction("PlayPause");
        playPauseAction.performed += OnPlayPause;
        playPauseAction.Enable();

        restartAction = playerMap.FindAction("Restart");
        restartAction.performed += OnRestart;
        restartAction.Enable();

        ui.ToggleMenu();
        StartCoroutine(PickFile());
    }

    void OnDisable()
    {
        if (playPauseAction != null)
        {
            playPauseAction.performed -= OnPlayPause;
            playPauseAction.Disable();
        }
        if (restartAction != null)
        {
            restartAction.performed -= OnRestart;
            restartAction.Disable();
        }
    }

    private void OnPlayPause(InputAction.CallbackContext ctx)
    {
        ToggleShow();
    }

    private void ToggleShow()
    {

        if (!isShowRunning)
        {
            isShowRunning = true;
            IsPaused = false;
            animationTimer = 0f;
            Play();
        }
        else
        {
            IsPaused = !IsPaused;
        }
    }

    private void OnRestart(InputAction.CallbackContext ctx)
    {
        RestartShow();
    }

    public void RestartShow()
    {
        animationTimer = 0f;
        elapsedShowTime = 0f;
        IsPaused = false;
        isShowRunning = true;
        currentAnimation = null;
        Play();
    }



    public IEnumerator PickFile()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Project", ".json"));
        FileBrowser.SetDefaultFilter(".json");
        FileBrowser.AddQuickLink("Users", "C:\\Users", null);

        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null, "Select Files", "Load");

        if (FileBrowser.Success)
            OnFilesSelected(FileBrowser.Result);
        }

    private void OnFilesSelected(string[] filePaths)
    {
        SourceFilePath = filePaths[0];

        if (SourceFilePath != null && bezierShader != null && dronePrefab != null)
        {
            ui.ToggleMenu();
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
        if (isShowRunning && !IsPaused)
        {
            animationTimer += Time.deltaTime;
            elapsedShowTime += Time.deltaTime;

            if (animationTimer >= animationInterval)
            {
                    animationTimer = 0f;
                Play();
            }

            if (IsLooping && elapsedShowTime >= ShowLength)
            {
                RestartShow();
            }
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

        orca.maxSpeed = 15f;
        orca.heightTolerance = 1.0f;

        return drone;
    }

    void Play()
    {
        IAnimation AnimationComp;
        DroneGraphic GraphicComp = null;

        if (currentAnimation == null)
        {
            AnimationComp = new StartAnimation();
            AnimationComp.Speed = 15;
            GraphicComp = GetComponentInChildren<DroneGraphic>();
        }
        else
        {
            AnimationComp = currentAnimation.GetComponent<IAnimation>();
            if (AnimationComp == null) return;

            foreach (Transform child in currentAnimation.transform)
            {
                GraphicComp = child.GetComponent<DroneGraphic>();
                if (GraphicComp != null)
                    break;
            }
        }

        if (GraphicComp == null) return;
        

        var currentDrones = new List<GameObject>();
        var paths = new List<DronePath>();

        if (!gridCreated)
        {
            int gridSize = Mathf.CeilToInt(Mathf.Sqrt(MaxDrones));
            float spacing = 5f; 
            float offset = 5;


            for (int i = 0; i < MaxDrones; i++)
            {
                int row = i % gridSize;
                int col = i / gridSize;

                float xOffset = row * spacing;
                float zOffset = col * spacing + offset;
                float yOffset = 0f;  

                var currentDrone = _pool.Get();
                currentDrone.transform.position = new Vector3(xOffset, yOffset, zOffset);

                groundedDrones.Enqueue(currentDrone);
                gridCreated = true;
            }
        }


        animationInterval = GraphicComp.Duration;

        counter.text = GraphicComp.edgePoints.Count.ToString();
        var curretnDrones = new List<GameObject>();

        

        List<Vector3> goals = GraphicComp.edgePoints
            .Select(p => p.ApplyTransformation(GraphicComp.transform, GraphicComp.sceneViewport, GraphicComp.Scale, GraphicComp.FlipHorizontal, GraphicComp.FlipVertical))
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
            animComp.droneShow = this;

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
            var nextAnimation = currentAnimation.transform.childCount > 0
                ? currentAnimation.transform.GetChild(0).gameObject
                : null;

            if (nextAnimation != null)
            {
                currentAnimation = nextAnimation;
            }
            else
            {
                if (IsLooping)
                {
                    isLoopingNextCycle = true;
                    animationTimer = 0f;
                }
                else
                {
                    IsPaused = true;
                }
        }
    }

    }

    void ParseShow()
    {
        string JsonContent;

        try {
            JsonContent = File.ReadAllText(SourceFilePath);
        } catch (Exception) {
            errorManager.DisplayError("ERROR: Unable to read file, please try a different file", 5);
            return;
        }

        DroneShowData ShowData;
        try {
            ShowData = JsonConvert.DeserializeObject<DroneShowData>(JsonContent);
        } catch (JsonException) {
            errorManager.DisplayError("ERROR: Malformed or unsupported json file, please try a different file", 5);
            return;
        }

        if (ShowData.Global == null) {
            errorManager.DisplayError("ERROR: Malformed or unsupported json file, please try a different file", 5);
            return;
        }
        DroneRadius = ShowData.Global.DroneRadius;
        MaxDrones = ShowData.Global.MaxDrones;
        IsLooping = ShowData.Global.IsLooping;
        GetAnimation(ShowData.AnimationStart, transform);
        ShowLength = GetShowLength(transform);

    }

    private int CalculateMaxDrones(AnimationData data)
    {
        int totalEdgePoints = 0;

        if (data != null)
        {
            DroneGraphic graphic = GetGraphic(data, new GameObject());
            totalEdgePoints += graphic.edgePoints.Count;

            if (data.NextAnimation != null)
            {
                totalEdgePoints += CalculateMaxDrones(data.NextAnimation);
            }
        }

        return totalEdgePoints;
    }

        private float GetShowLength(Transform parent)
    {
        float length = 0;
        Transform current = parent;

        while (current.childCount > 0)
        {
            current = current.GetChild(0);
            var currentGraphic = current.GetComponent<DroneGraphic>();
            if (currentGraphic == null) {
                Debug.Log("somethings wrong"); 
                return length;
            }

            length += currentGraphic.Duration;
        }

        return length;
    }


    private void GetAnimation(AnimationData data, Transform parrent)
    {
        GameObject animtionObject = new();
        animtionObject.transform.parent = parrent;

        Type T = Type.GetType(data.Type);
        IAnimation animation = (IAnimation)animtionObject.AddComponent(T);

        GetGraphic(data, animtionObject);

        if (data.Position != null)
        {
            Vector3 pos = new((float)data.Position[0], (float)data.Position[1], (float)data.Position[2]);
            animtionObject.transform.position = pos;
        }
        else
        {
            animtionObject.transform.position = parrent.position;
        }

        if (data.Rotation != null)
        {
            Vector3 rot = new((float)data.Rotation[0], (float)data.Rotation[1], (float)data.Rotation[2]);
            animtionObject.transform.eulerAngles = rot;
        }
        else
        {
            animtionObject.transform.eulerAngles = parrent.eulerAngles;
        }

        animation.Speed = data.Speed;
        if (data.NextAnimation != null)
        {
            GetAnimation(data.NextAnimation, animtionObject.transform);
        }
    }

    private DroneGraphic GetGraphic(AnimationData data, GameObject parrent)
    {
        DroneGraphic graphic = parrent.AddComponent<DroneGraphic>();
        graphic.svgContent = data.Graphic.Source;
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
        graphic.MaxDrones = MaxDrones;

        graphic.FlipHorizontal = data.Graphic.FlipHorizontal;
        graphic.FlipVertical = data.Graphic.FlipVertical;

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
