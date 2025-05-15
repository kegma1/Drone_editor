using UnityEngine;
using Unity.Mathematics;
using Unity.VisualScripting;

public class AnimationPlayer : MonoBehaviour
{
    public DronePath Path;
    public float T = 0f;
    private bool IsPlaying = false;
    private DronePath currentSegment;
    public Drone Drone;
    public Vector3 startPosition;
    public Color targetColor;
    private Color startColor;
    public float Speed;
    private OrcaAgent orca;

    public Vector3 previousPosition;

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

        Vector3 targetPosition = EvaluateSegment(currentSegment, T);

        Vector3 tangent = Vector3.zero;

        if (currentSegment.ControllA.HasValue && currentSegment.ControllB.HasValue)
        {
            Vector3 p0 = currentSegment.Start;
            Vector3 p1 = currentSegment.ControllA.Value;
            Vector3 p2 = currentSegment.ControllB.Value;
            Vector3 p3 = currentSegment.NextSegment != null
                ? currentSegment.NextSegment.Start
                : BezierEvaluate(p0, p1, p2, 1f);

            tangent = EvaluateCubicTangent(p0, p1, p2, p3, T).normalized;
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

        Vector3 adjustedVelocity = orca != null
            ? new Vector3(orca.ORCAAgent.velocity.x, 0f, orca.ORCAAgent.velocity.z)
            : Vector3.zero;

        transform.position += adjustedVelocity * Time.deltaTime;

        //Y-axis smoothing
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


        float actualDistanceMoved = Vector3.Distance(transform.position, previousPosition);
        float segmentLength = GetSegmentLength(currentSegment); 
        float adjustedSegmentLength = Mathf.Max(segmentLength, 0.01f);
        float incrementT = actualDistanceMoved / adjustedSegmentLength;

        if (actualDistanceMoved < 0.01f)
        {
            T += Time.deltaTime * 0.05f; 
        }

        T += incrementT;

        if (T >= 1f || Vector3.Distance(transform.position, EvaluateSegment(currentSegment, 1f)) < 0.2f)
        {
            Vector3 targetPos = EvaluateSegment(currentSegment, 1f);
            float distance = Vector3.Distance(transform.position, targetPos);

            if (distance > 0.1f)
            {
                //in case orca steered agent off the curve
                orca.SetPreferredVelocity((targetPos - transform.position).normalized * Speed);
                return;
            }

            //transform.position = targetPos;

            T = 0f;
            currentSegment = currentSegment.NextSegment;
            
            if (currentSegment == null || currentSegment.NextSegment == null)
            {
                IsPlaying = false;
                orca?.SetPreferredVelocity(Vector3.zero);
                return;
            }
        }


        previousPosition = transform.position;
    }

    public void PlayFromStart()
    {
        Drone = GetComponent<Drone>();
        if (Path != null)
        {
            T = 0f;
            currentSegment = Path;
            startColor = Drone.color;
            gameObject.SetActive(true);
            IsPlaying = true;
            previousPosition = transform.position;
        }
        else
        {
            Debug.Log("No path was found");
        }
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

    private static Vector3 EvaluateCubicTangent(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        float u = 1f - t;
        return
            3f * u * u * (b - a) +
            6f * u * t * (c - b) +
            3f * t * t * (d - c);
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

    private float GetSegmentLength(DronePath segment)
    {
        if (segment.ControllA.HasValue && segment.ControllB.HasValue)
        {
            float length = 0f;
            int steps = 20;
            Vector3 previousPoint = segment.Start;

            for (int i = 1; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector3 point = EvaluateSegment(segment, t);
                length += Vector3.Distance(previousPoint, point);
                previousPoint = point;
            }

            return length;
        }

        return 0f;
    }
}
