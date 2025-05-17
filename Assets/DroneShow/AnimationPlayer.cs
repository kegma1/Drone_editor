using UnityEngine;
using Unity.Mathematics;
using Unity.VisualScripting;

public class AnimationPlayer : MonoBehaviour
{
    public DronePath Path;
    public float T = 0f;
    public bool IsPlaying = false;
    private DronePath currentSegment;
    public Drone Drone;
    public Vector3 startPosition;
    public Color targetColor;
    private Color startColor;
    public float Speed;
    private OrcaAgent orca; 
    public float TimeOffset = 0f;
    private float elapsedTime = 0f;
    public Vector3 previousPosition;

    private const float TargetTolerance = 0.02f;

    public droneShow droneShow;

    void Start()
    {
        Drone = GetComponent<Drone>();
        currentSegment = Path;
        orca = GetComponent<OrcaAgent>(); 
        previousPosition = transform.position;
    }

    void Update()
    {
        if (!IsPlaying || currentSegment == null || droneShow.IsPaused) {
            if (orca != null) {
                orca.SetPaused(true);
            }

            return;
        } else {
            if (orca != null) {
                orca.SetPaused(false);
            }
        }
        
        elapsedTime += Time.deltaTime;

        if (elapsedTime < TimeOffset) return;


        Vector3 targetPosition = EvaluateSmoothedPath(currentSegment, T);
        Vector3 tangent = Vector3.zero;

        if (currentSegment.SmoothedPoints != null && currentSegment.SmoothedPoints.Count > 1)
        {
            int index = Mathf.FloorToInt(T * (currentSegment.SmoothedPoints.Count - 1));
            Vector3 p0 = currentSegment.SmoothedPoints[index];
            Vector3 p1 = currentSegment.SmoothedPoints[Mathf.Min(index + 1, currentSegment.SmoothedPoints.Count - 1)];
            tangent = (p1 - p0).normalized;
        }

 
        Vector3 desiredVelocity = tangent * Speed;


         if (orca != null)
         {
             orca.SetTargetHeight(targetPosition.y); 
             orca.SetPreferredVelocity(desiredVelocity); 
        }

        Drone.SetColor(Color.Lerp(startColor, targetColor, T));
    }

    void LateUpdate()
    {
        if (!IsPlaying || currentSegment == null || droneShow.IsPaused) {
            if (orca != null) {
                orca.SetPaused(true);
            }

            return;
        } else {
            if (orca != null) {
                orca.SetPaused(false);
            }
        }

        Vector3 targetPosition = EvaluateSmoothedPath(currentSegment, T);
        float distanceToGoal = Vector3.Distance(transform.position, targetPosition);

        Vector3 tangent = Vector3.zero;

        if (currentSegment.SmoothedPoints != null && currentSegment.SmoothedPoints.Count > 1)
        {
            int index = Mathf.FloorToInt(T * (currentSegment.SmoothedPoints.Count - 1));
            Vector3 p0 = currentSegment.SmoothedPoints[index];
            Vector3 p1 = currentSegment.SmoothedPoints[Mathf.Min(index + 1, currentSegment.SmoothedPoints.Count - 1)];
            tangent = (p1 - p0).normalized;
        }


        Vector3 desiredVelocity = tangent * Speed;

        if (orca != null)
        {

            Vector3 adjustedVelocity = new Vector3(orca.ORCAAgent.velocity.x, 0f, orca.ORCAAgent.velocity.z);

            targetPosition = EvaluateSmoothedPath(currentSegment, T);

            Vector3 directionToTarget = targetPosition - transform.position;
            Vector3 evasionDirection = -directionToTarget.normalized;  


            float smoothingFactor = 0.2f;

            transform.position += (evasionDirection + adjustedVelocity) * Time.deltaTime * smoothingFactor;
        }


        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Speed * Time.deltaTime);
        }

        Vector3 newPos = transform.position;
        if (orca != null && orca.GetTargetHeight(out float targetY))
        {
            float currentY = newPos.y;
            if (Mathf.Abs(currentY - targetY) > 0.1f)
                newPos.y = Mathf.Lerp(currentY, targetY, Time.deltaTime * 2f);
            else
                newPos.y = targetY;
        }
        transform.position = newPos;


        if (T >= 1f && distanceToGoal < TargetTolerance)
        {
            transform.position = targetPosition;  
            T = 1f;  
            orca?.SetPreferredVelocity(Vector3.zero);  
            IsPlaying = false;  
        }
        else
        {
            float moved = Vector3.Distance(transform.position, previousPosition);
            float pathLength = GetSmoothedPathLength(currentSegment);
            float deltaT = (Speed * Time.deltaTime) / Mathf.Max(pathLength, 0.01f);
            deltaT = Mathf.Min(deltaT, 1f - T); 

            T += deltaT;  
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, Speed * Time.deltaTime);

        previousPosition = transform.position;  
    }

    public void PlayFromStart()
    {
        Drone = GetComponent<Drone>();
        if (Path != null && Path.SmoothedPoints != null && Path.SmoothedPoints.Count > 1)
        {
            T = 0f;
            elapsedTime = 0f;
            currentSegment = Path;
            startColor = Drone.color;
            gameObject.SetActive(true);
            IsPlaying = true;
            previousPosition = transform.position;
        }
        else
        {
            Debug.LogWarning("No valid path or smoothed points.");
        }
    }

    private Vector3 EvaluateSmoothedPath(DronePath segment, float t)
    {
        var pts = segment.SmoothedPoints;

        if (pts == null || pts.Count < 4)
            return segment.Start;

        t = Mathf.Clamp01(t);
        int numSegments = pts.Count - 3;
        float totalT = t * numSegments;
        int segIndex = Mathf.Min(Mathf.FloorToInt(totalT), numSegments - 1);

        float localT = totalT - segIndex;

        Vector3 p0 = pts[segIndex];
        Vector3 p1 = pts[segIndex + 1];
        Vector3 p2 = pts[segIndex + 2];
        Vector3 p3 = pts[segIndex + 3];

        return DronePathBuilder.CatmullRom(p0, p1, p2, p3, localT);
    }

    private float GetSmoothedPathLength(DronePath segment)
    {
        if (segment.SmoothedPoints == null || segment.SmoothedPoints.Count < 2)
            return 1f;

        float length = 0f;
        var pts = segment.SmoothedPoints;
        for (int i = 1; i < pts.Count; i++)
            length += Vector3.Distance(pts[i - 1], pts[i]);

        return length;
    }
}
