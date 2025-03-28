using UnityEngine;

public class AnimationPlayer : MonoBehaviour
{
    public DronePath Path;
    public float T = 0f;
    public float Duration = 0f;
    private bool IsPlaying = false;
    private DronePath currentSegment;
    public Drone Drone;
    public Color targetColor;
    private Color startColor;
    public Vector3 startPosition;

    void Start()
    {
        Drone = GetComponent<Drone>();
        currentSegment = Path;
    }

    void Update()
    {
        if (IsPlaying && currentSegment != null) {
            T += Time.deltaTime / Duration;
            if (T > 1f) {
                T = 0f;
                currentSegment = currentSegment.NextSegment;
                
                if (currentSegment == null || currentSegment.NextSegment == null) {
                    IsPlaying = false;
                    return;
                }
            }

            Drone.SetColor(Color.Lerp(startColor, targetColor, T));
            transform.position = EvaluateSegment(currentSegment, T);
        }
    }

    public void PlayFromStart() {
        Drone = GetComponent<Drone>();
        if (Path != null) {
            T = 0f;
            currentSegment = Path;
            startColor = Drone.color;
            gameObject.SetActive(true);  
            IsPlaying = true;
        } else {
            Debug.Log("no path was found");
        }
    }

private static Vector3 EvaluateSegment(DronePath segment, float t)
    {
        if (segment.ControllA.HasValue && segment.ControllB.HasValue)
        {
            return EvaluateCubic(segment.Start, segment.ControllA.Value, segment.ControllB.Value, segment.NextSegment?.Start ?? segment.ControllB.Value, t);
        } else return Vector3.zero;
    }


    private static Vector3 EvaluateQuadratic(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        Vector3 p0 = Vector3.Lerp(a, b, t);
        Vector3 p1 = Vector3.Lerp(b, c, t);
        return Vector3.Lerp(p0, p1, t);
    }

    private static Vector3 EvaluateCubic(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        Vector3 p0 = EvaluateQuadratic(a, b, c, t);
        Vector3 p1 = EvaluateQuadratic(b, c, d, t);
        return Vector3.Lerp(p0, p1, t);
    }
}
