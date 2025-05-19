// denne filen og disse klassene definerer hvordan prosjekt fil formatert er byggd opp. setter også standard verdier
[System.Serializable]
public class DroneShowData {
    public GlobalData Global = new();
    public AnimationData AnimationStart = null;
}

[System.Serializable]
public class GlobalData {
    public float DroneRadius = .25f;
    public int MaxDrones = 1000;
    public bool IsLooping = true;
}

[System.Serializable]
public class AnimationData {
    public float[] Position ;
    public float[] Rotation;
    public GraphicData Graphic;
    public string Type;
    public float Speed = 1;
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
    public string Source= "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 114.682 114.682\" width=\"114.682px\" height=\"114.682px\">\r\n  <path fill=\"#ffe000\" d=\"M57.341,0 C89.002,0 114.682,25.68 114.682,57.341 C114.682,89.002 89.002,114.682 57.341,114.682 C25.68,114.682 0,89.002 0,57.341 C0,25.68 25.68,0 57.341,0 Z\"/>\r\n  <path fill=\"#ffffff\" d=\"M33.467,26.342 C41.132,26.342 47.368,32.578 47.368,40.243 C47.368,47.908 41.132,54.144 33.467,54.144 C25.802,54.144 19.566,47.908 19.566,40.243 C19.566,32.578 25.802,26.342 33.467,26.342 Z\"/>\r\n  <path fill=\"#ffffff\" d=\"M82.467,26.342 C90.132,26.342 96.368,32.578 96.368,40.243 C96.368,47.908 90.132,54.144 82.467,54.144 C74.802,54.144 68.566,47.908 68.566,40.243 C68.566,32.578 74.802,26.342 82.467,26.342 Z\"/>\r\n  <path fill=\"#ffffff\" d=\"M17.811,64.552 C18.509,102.836 99.977,93.212 99.479,65.856 Z\"/>\r\n</svg>\r\n";
    public float Duration = 10;
    public float Scale = 0.5f;
    public bool Outline = true;
    public float OutlineSpacing = 1;
    public bool Fill;
    public float FillSpacing;
    public float[] FillOffset;
    public float FillRotation;

    public bool FlipHorizontal = true;
    public bool FlipVertical;

    public GraphicData() {
        FillOffset = new float[] { 0f, 0f };
    }
}