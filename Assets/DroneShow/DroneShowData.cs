[System.Serializable]
public class DroneShowData {
    public GlobalData Global;
    public AnimationData AnimationStart;
}

[System.Serializable]
public class GlobalData {
    public float DroneRadius;
}

[System.Serializable]
public class AnimationData {
    public float[] Position ;
    public float[] Rotation;
    public GraphicData Graphic;
    public string Type;
    public float Duration;
#nullable enable
    public AnimationData? NextAnimation;
#nullable disable


    public AnimationData() {
        Position = new float[] { 0f, 0f, 0f };
        Rotation = new float[] { 0f, 0f, 0f };
        Graphic = new();
    }
}

[System.Serializable]
public class GraphicData {
    public string Source;
    public float Scale;
    public bool Outline;
    public float OutlineSpacing;
    public bool Fill;
    public float FillSpacing;
    public float[] FillOffset;
    public float FillRotation;

    public GraphicData() {
        FillOffset = new float[] { 0f, 0f };
    }
}