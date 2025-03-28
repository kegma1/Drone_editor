using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Pool;
using System.Collections.Generic;
using System.Linq;

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


    private float t = 0f;
    
    void Start()
    {
        if (dronePrefab != null) {
            _pool = new ObjectPool<GameObject>(CreateDrone, null, onDroneRelease, defaultCapacity: MaxDrones, maxSize: MaxDrones);           
        }

        if (SourceFilePath != null && bezierShader != null && dronePrefab != null) {
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
        if (Input.GetKeyDown(KeyCode.Space)) {
            Play();
        }
    }

    private GameObject CreateDrone()
    {
        var drone = Instantiate(dronePrefab);
        var droneComp = drone.GetComponent<Drone>();
        droneComp.color = Color.black;
        droneComp.radius = DroneRadius;
        return drone;
    }

    void Play() {
        IAnimation AnimationComp;

        Graphic GraphicComp = null;

        if (currentAnimation == null) {
            AnimationComp = new StartAnimation();
            AnimationComp.Duration = 2;

            GraphicComp = GetComponentInChildren<Graphic>();
        } else {
            AnimationComp = currentAnimation.GetComponent<IAnimation>();
            if (AnimationComp == null) return;


            foreach (Transform child in currentAnimation.transform) {
                GraphicComp = child.GetComponent<Graphic>();
                if (GraphicComp != null)
                    break;
            }
        }

        if (GraphicComp == null) {
            return;
        }

        var curretnDrones = new List<GameObject>();

        foreach (var Vdrone in GraphicComp.edgePoints)
        {
            GameObject drone;

            if (activeDrones.Count > 0) {
                drone = activeDrones.Dequeue();
            } else {
                drone = _pool.Get();
            }

            var animComp = drone.GetComponent<AnimationPlayer>();
            animComp.Duration = AnimationComp.Duration;
            animComp.targetColor = Vdrone.color;
            animComp.startPosition = drone.transform.position;

            animComp.Path = AnimationComp.GeneratePath(
                drone.transform.position, 
                Vdrone.ApplyTransformation(GraphicComp.transform, GraphicComp.sceneViewport, GraphicComp.Scale)
            );
            animComp.PlayFromStart();
            curretnDrones.Add(drone);
        }

        foreach (var drone in activeDrones) {
            var animComp = drone.GetComponent<AnimationPlayer>();
            animComp.Duration = AnimationComp.Duration;
            animComp.targetColor = Color.black;

            animComp.Path = AnimationComp.GeneratePath(
                drone.transform.position, 
                animComp.startPosition
            );
            _pool.Release(drone);
        }
        activeDrones.Clear();

        foreach (var drone in curretnDrones) {
            activeDrones.Enqueue(drone);
        }
        curretnDrones.Clear();

        if (currentAnimation == null) {
            currentAnimation = transform.GetChild(0).gameObject;
        } else {
            var nextAnimation = currentAnimation.transform.GetChild(0).gameObject;
            if(nextAnimation) {
                currentAnimation = nextAnimation;
            }
        }
    }


    void ParseShow() {
        string JsonContent = File.ReadAllText(SourceFilePath);
        DroneShowData ShowData = JsonConvert.DeserializeObject<DroneShowData>(JsonContent);
        
        DroneRadius = ShowData.Global.DroneRadius;
        GetAnimation(ShowData.AnimationStart, transform);
    }

    private void GetAnimation(AnimationData data, Transform parrent) {
        GameObject animtionObject = new();
        animtionObject.transform.parent = parrent;

        string animationType = data.Type;
        Type T = Type.GetType(animationType);
        IAnimation animation = (IAnimation)animtionObject.AddComponent(T);

        GetGraphic(data, animtionObject);

        if (data.Position != null) {
            Vector3 pos = new((float)data.Position[0], (float)data.Position[1], (float)data.Position[2]);
            animtionObject.transform.position = pos;
        } else {
            animtionObject.transform.position = parrent.position;
        }

        if (data.Rotation != null) {
            Vector3 rot = new((float)data.Rotation[0], (float)data.Rotation[1], (float)data.Rotation[2]);
            animtionObject.transform.eulerAngles = rot;
        } else {
            animtionObject.transform.eulerAngles = parrent.eulerAngles;
        }

        animation.Duration = data.Duration;
        if(data.NextAnimation != null) {
            GetAnimation(data.NextAnimation, animtionObject.transform);
        }
    }

    private Graphic GetGraphic(AnimationData data, GameObject parrent) {
        Graphic graphic = parrent.AddComponent<Graphic>();
        graphic.svgFilePath = data.Graphic.Source;
        graphic.Scale = data.Graphic.Scale;
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

}
