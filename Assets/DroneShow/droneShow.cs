using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Pool;
using Unity.VisualScripting;

public class droneShow : MonoBehaviour
{
    public string SourceFilePath;

    private float DroneRadius;
    
    public GameObject dronePrefab; 

    private ObjectPool<GameObject> _pool;

    public ComputeShader bezierShader;

    private float t = 0f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (dronePrefab != null) {
            _pool = new ObjectPool<GameObject>(CreateDrone, null, OnPutBackInPool, defaultCapacity: 500);
        }

        if (SourceFilePath != null && bezierShader != null && dronePrefab != null) {
            ParseShow();
            Play();
        }
    }

    void Update()
    {
        
    }

    private void OnPutBackInPool(GameObject @object)
    {
        @object.SetActive(false);
    }

    private GameObject CreateDrone()
    {
        var drone = Instantiate(dronePrefab);
        return drone;
    }

    void Play() {
        IAnimation firstAnimation = GetComponentInChildren<IAnimation>();
        if (firstAnimation == null) return;

        Graphic firstGraphic = GetComponentInChildren<Graphic>();
        if (firstGraphic == null) return;

        foreach (var Vdrone in firstGraphic.edgePoints)
        {
            var drone = _pool.Get();
            drone.transform.position = Vdrone.ApplyTransformation(firstGraphic.transform, firstGraphic.sceneViewport, firstGraphic.Scale);
            var droneComp = drone.GetComponent<Drone>();
            droneComp.color = Vdrone.color;
            droneComp.radius = DroneRadius;
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

        animation.GeneratePaths();
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
        return graphic;
    }

}
