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

    private DroneRepulsion droneRepulsion;  

    void Start()
    {
        Drone = GetComponent<Drone>();
        currentSegment = Path;
        droneRepulsion = GetComponent<DroneRepulsion>(); 
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
            Vector3 targetPosition = EvaluateSegment(currentSegment, T);

            Vector3 repulsionOffset = CalculateRepulsion();

            float smoothFactor = 5f;  
            Vector3 newPosition = Vector3.Lerp(transform.position, targetPosition + repulsionOffset, Time.deltaTime * smoothFactor);
            
            transform.position = newPosition;
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
        }
        else {
            Debug.Log("no path was found");
        }
    }

    private Vector3 CalculateRepulsion() {
        Vector3 totalForce = Vector3.zero;

        Drone[] allDrones = FindObjectsByType<Drone>(FindObjectsSortMode.None);

        foreach (var otherDrone in allDrones) {
            if (otherDrone != Drone) {
                float distance = Vector3.Distance(transform.position, otherDrone.transform.position);

                if (distance < droneRepulsion.repulsionRadius && distance > 0.001f) {
                    
                    Vector3 direction = (transform.position - otherDrone.transform.position).normalized;

                    float repulsionForce = (droneRepulsion.repulsionRadius - distance) * droneRepulsion.repulsionStrength; 
                    repulsionForce = Mathf.Pow(repulsionForce, 1.5f); 

                    totalForce += direction * repulsionForce;
                }
            }
        }

        totalForce *= 0.05f; 

        return totalForce;
    }

    private static Vector3 EvaluateSegment(DronePath segment, float t)
        {
            if (segment.ControllA.HasValue && segment.ControllB.HasValue)
            {
                Vector3 p0 = segment.Start;
                Vector3 p1 = segment.ControllA.Value;
                Vector3 p2 = segment.ControllB.Value;

                Vector3 p3 = segment.NextSegment != null
                    ? segment.NextSegment.Start
                    : BezierEvaluate(p0, p1, p2, 1f);

                return EvaluateCubic(p0, p1, p2, p3, t);
            }

            return segment.Start;
        }

    private static Vector3 BezierEvaluate(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            float u = 1f - t;
            return u * u * p0 + 2f * u * t * p1 + t * t * p2;
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
