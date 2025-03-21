using System.Collections.Generic;

public interface IAnimation {
    public float Time { get; set; }
    public float Duration { get; set; }
    public List<DronePath> Paths { get; set; }

    public void GeneratePaths();
    public void Play();
}