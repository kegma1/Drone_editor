using System;
using System.IO;
using UnityEngine;
using UnityEngine.Android;

public class droneShow : MonoBehaviour
{
    public string SourceFilePath;
    private IAnimation FirstAnimation;

    private float DroneRadius;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (SourceFilePath != null) {
            ParseShow();
        }
    }

    void Play() {

    }

    void ParseShow() {
        string JsonContent = File.ReadAllText(SourceFilePath);
        Debug.Log(JsonContent);
        DroneShowData ShowData = JsonUtility.FromJson<DroneShowData>(JsonContent);
        DroneRadius = ShowData.Global.DroneRadius;
        FirstAnimation = GetAnimation(ShowData.AnimationStart);

    }

    private IAnimation GetAnimation(AnimationData data) {
        string animationType = data.Type;
        Debug.Log(animationType);
        Type T = Type.GetType(animationType);
        IAnimation animation = (IAnimation)Activator.CreateInstance(T);

        animation.Graphic = GetGraphic(data);

        animation.Position = new Vector3(data.Position[0], data.Position[1], data.Position[2]);

        animation.Rotation = new Vector3(data.Rotation[0], data.Rotation[1], data.Rotation[2]);

        animation.Duration = data.Duration;
        Debug.Log(data.NextAnimation);
        if(data.NextAnimation != null) {
            Debug.Log("here");
            animation.NextAnimation = GetAnimation(data.NextAnimation);
        }
        Debug.Log(animation.NextAnimation);
        // animation.GeneratePaths();

        return animation;
    }

    private Graphic GetGraphic(AnimationData data) {
        Graphic graphic = new();
        graphic.svgFilePath = data.Graphic.Source;
        graphic.Scale = data.Graphic.Scale;
        graphic.Outline = data.Graphic.Outline;
        graphic.OutlineSpacing = data.Graphic.OutlineSpacing;
        graphic.Fill = data.Graphic.Fill;
        graphic.FillSpacing = data.Graphic.FillSpacing;
        graphic.FillOffset = new Vector2(data.Graphic.FillOffset[0], data.Graphic.FillOffset[1]);
        graphic.FillRotation = data.Graphic.FillRotation;
        return graphic;
    }

}
