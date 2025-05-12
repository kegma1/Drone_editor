using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Pool;
using System.Collections.Generic;
using System.Linq;
using SimpleFileBrowser;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;

public class droneShow : MonoBehaviour
{
    public string SourceFilePath;
    private float DroneRadius;
    public int MaxDrones = 800;
    public GameObject dronePrefab;
    private ObjectPool<GameObject> _pool;
    public ComputeShader bezierShader;
    private GameObject currentAnimation = null;
    private Queue<GameObject> activeDrones = new();
    public InGameMenuController ui;
    public TMP_Text counter;

    public bool isShowRunning = false;
    public bool isPaused = false;

    [Range(5f, 120f)]
    public float animationInterval = 30f;
    private float animationTimer = 0f;
    private float t = 0f;

    public InputActionAsset inputActions;
    private InputAction playPauseAction;

    void Start()
    {
        if (dronePrefab != null)
        {
            _pool = new ObjectPool<GameObject>(CreateDrone, null, onDroneRelease, defaultCapacity: MaxDrones, maxSize: MaxDrones);
        }

        var playerMap = inputActions.FindActionMap("Player");
        playPauseAction = playerMap.FindAction("PlayPause");
        playPauseAction.performed += OnPlayPause;
        playPauseAction.Enable();

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
            isPaused = false;
            animationTimer = 0f;
            Play();
        }
        else
        {
            isPaused = !isPaused;
        }
    }

    public IEnumerator PickFile()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Project", ".json"));
        FileBrowser.SetDefaultFilter(".json");
        FileBrowser.AddQuickLink("Users", "C:\\Users", null);

        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null, "Select Files", "Load");
        Debug.Log(FileBrowser.Success);

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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleShow();
        }

        if (isShowRunning && !isPaused)
        {
            animationTimer += Time.deltaTime;

            if (animationTimer >= animationInterval)
            {
                animationTimer = 0f;
                Play();
            }
        }
    }

    private GameObject CreateDrone()
    {
        var drone = Instantiate(dronePrefab);
        drone.transform.position = Vector3.zero;

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

        orca.maxSpeed = 5f;
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
            AnimationComp.Speed = 5;
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

        counter.text = GraphicComp.edgePoints.Count.ToString();
        var curretnDrones = new List<GameObject>();

        foreach (var Vdrone in GraphicComp.edgePoints)
        {
            GameObject drone = activeDrones.Count > 0 ? activeDrones.Dequeue() : _pool.Get();

            var animComp = drone.GetComponent<AnimationPlayer>();
            animComp.Speed = AnimationComp.Speed;
            animComp.targetColor = Vdrone.color;
            animComp.startPosition = drone.transform.position;

            animComp.Path = AnimationComp.GeneratePath(
                drone.transform.position,
                Vdrone.ApplyTransformation(GraphicComp.transform, GraphicComp.sceneViewport, GraphicComp.Scale, GraphicComp.FlipHorizontal, GraphicComp.FlipVertical)
            );

            animComp.PlayFromStart();
            curretnDrones.Add(drone);
        }

        foreach (var drone in activeDrones)
        {
            var animComp = drone.GetComponent<AnimationPlayer>();
            animComp.Speed = AnimationComp.Speed;
            animComp.targetColor = Color.black;

            animComp.Path = AnimationComp.GeneratePath(drone.transform.position, animComp.startPosition);
            _pool.Release(drone);
        }

        activeDrones.Clear();
        foreach (var drone in curretnDrones) activeDrones.Enqueue(drone);
        curretnDrones.Clear();

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
        }
    }

    void ParseShow()
    {
        string JsonContent = File.ReadAllText(SourceFilePath);
        DroneShowData ShowData = JsonConvert.DeserializeObject<DroneShowData>(JsonContent);

        DroneRadius = ShowData.Global.DroneRadius;
        GetAnimation(ShowData.AnimationStart, transform);
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

        graphic.FlipHorizontal = data.Graphic.FlipHorizontal;
        graphic.FlipVertical = data.Graphic.FlipVertical;

        graphic.GeneratePointsFromPath();
        return graphic;
    }
}
