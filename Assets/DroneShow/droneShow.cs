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

    public ErrorManager errorManager; // Referanse til objekt brukt for å vise feilmeldinger til brukeren
    public string SourceFilePath;

    private float DroneRadius;

    public int MaxDrones = 800;

    private float curvatureFactor = 0;
    private int MaxDronesNeeded;

    public GameObject dronePrefab;

    private ObjectPool<GameObject> _pool;

    private int MaxSolverTries = 100;

    public ComputeShader bezierShader;

    private GameObject currentAnimation = null;

    private Queue<GameObject> activeDrones = new();
    private Queue<GameObject> shadowDrones = new(); 

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
            bool formationDone = true;
            foreach (var drone in activeDrones)
            {
                
                var animComp = drone.GetComponent<AnimationPlayer>();
                if (animComp == null || !animComp.quiteDone)
                {
                    formationDone = false;
                    break;
                }
            }
            
            if (formationDone)
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
            int gridSize = Mathf.CeilToInt(Mathf.Sqrt(MaxDronesNeeded));
            float spacing = 5f; 
            float offsetx = 50;
            float offsetz = -100;

            for (int i = 0; i < MaxDronesNeeded; i++)
            {
                int row = i % gridSize;
                int col = i / gridSize;

                float xOffset = row * spacing + offsetx;
                float zOffset = col * spacing + offsetz;
                float yOffset = 0f;  

                var currentDrone = _pool.Get();
                currentDrone.transform.position = new Vector3(xOffset, yOffset, zOffset);
                shadowDrones.Enqueue(currentDrone);
                gridCreated = true;
            }
        }


        animationInterval = GraphicComp.Duration;

        counter.text = GraphicComp.edgePoints.Count.ToString();
        

        List<Vector3> goals = GraphicComp.edgePoints
            .Select(p => p.ApplyTransformation(GraphicComp.transform, GraphicComp.sceneViewport, GraphicComp.Scale, GraphicComp.FlipHorizontal, GraphicComp.FlipVertical))
            .ToList();
        
        int difference = goals.Count - activeDrones.Count; 
        List<GameObject> newlyShadowedDrones = new();     
        GameObject traitorDrone;  
        for (int i = 0; i < difference; i++)
        {
            if (goals.Count > activeDrones.Count)
            {   
                if (shadowDrones.Count > 0)
                {
                    traitorDrone = shadowDrones.Dequeue();
                    activeDrones.Enqueue(traitorDrone);
                }
                else
                {
                    traitorDrone = _pool.Get();
                    activeDrones.Enqueue(traitorDrone);
                }
            }
            else
            {
                traitorDrone = activeDrones.Dequeue();
                newlyShadowedDrones.Add(traitorDrone); 
                shadowDrones.Enqueue(traitorDrone);
            }
        }


        List<Vector3> starts = activeDrones.Select(d => d.transform.position).ToList();
        

        List<Color> goalColors = GraphicComp.edgePoints
            .Select(p => p.color)
            .ToList();


  
        List<(GameObject drone, Vector3 goal, Color goalColor)> goalAssignments = GoalAssignment.AssignGoalsToDrones(activeDrones.ToList(), goals, goalColors);

        foreach (var (drone, goal, goalColor) in goalAssignments)
        {
            var path = DronePathBuilder.FromStartToGoal(drone.transform.position, goal);
            paths.Add(path);

            var animComp = drone.GetComponent<AnimationPlayer>();
            animComp.targetColor = goalColor;  
        }

        Vector3 viewerPosition = Vector3.zero;
        Vector3 viewDirection = Vector3.forward;


        List<MotionPlan> plans = new List<MotionPlan>();
        int solverTries = 0;
        while (plans.Count == 0 && solverTries < MaxSolverTries)
        {   
            solverTries++;
            curvatureFactor += 4;
            foreach (var drone in newlyShadowedDrones) 
            {
                Vector3 currentPosition = drone.transform.position;
                Vector3 shadowGoal = currentPosition + (currentPosition - viewerPosition).normalized * 5f;

                var path = DronePathBuilder.FromStartToGoal(currentPosition, shadowGoal, curvatureFactor);
                paths.Add(path); 

                var animComp = drone.GetComponent<AnimationPlayer>();
                animComp.targetColor = Color.black;
            }

            foreach (var drone in shadowDrones)
            {
                if (newlyShadowedDrones.Contains(drone)) continue;

                Vector3 currentPosition = drone.transform.position;
                Vector3 directionToDrone = (currentPosition - viewerPosition).normalized;
                float forwardDot = Vector3.Dot(directionToDrone, viewDirection);

                if (forwardDot > 0f) 
                {
                    Vector3 shadowGoal = currentPosition + directionToDrone * 30f;

                    var path = DronePathBuilder.FromStartToGoal(currentPosition, shadowGoal, curvatureFactor);
                    paths.Add(path); 

                    var animComp = drone.GetComponent<AnimationPlayer>();
                    animComp.targetColor = Color.black;
                }
            }

            plans = DroneConstraintSolverORTools.SchedulePaths(paths, DroneRadius, AnimationComp.Speed);
            if (plans.Count == 0)
            {
                Debug.LogError("Failed to find a feasible solution even after calculating paths.");
            }
            else
            {
                Debug.Log("Paths successfully calculated.");
            }
        }


        var droneList = activeDrones.ToList();  
        var startPositions = droneList.Select(d => d.transform.position).ToList(); 

        for (int i = 0; i < plans.Count; i++)
        {
            GameObject drone;

            if (i < droneList.Count)
                drone = droneList[i];
            else
                drone = shadowDrones.Dequeue(); 

            var animComp = drone.GetComponent<AnimationPlayer>();
            animComp.Speed = plans[i].Speed ?? AnimationComp.Speed;
            animComp.Path = plans[i].Path;
            animComp.TimeOffset = plans[i].TimeOffset ?? 0f;
            animComp.startPosition = i < startPositions.Count ? startPositions[i] : drone.transform.position;
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
        MaxDronesNeeded = CalculateMaxDrones(ShowData.AnimationStart);


    }

    private int CalculateMaxDrones(AnimationData data)
    {
        if (data == null)
            return 0;

        // Count drones in this animation
        int currentCount = GetGraphic(data, new GameObject()).edgePoints.Count;

        // Recursively find max from the rest of the chain
        int nextMax = CalculateMaxDrones(data.NextAnimation);

        return Mathf.Max(currentCount, nextMax);
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
