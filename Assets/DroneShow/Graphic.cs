using UnityEngine;
using Unity.VectorGraphics;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class DroneGraphic : MonoBehaviour
{
    public string svgContent;

    [Min(0.01f)]
    public float Scale = 1f;
    public float Duration = 1f;
    [Min(0.01f)]
    public float pointRadius = 0.25f;
    public int MaxDrones;

    public bool Outline = true;
    [Min(0f)]
    public float OutlineSpacing = 0.5f; 


    public bool Fill = false;

    [Min(0f)]
    public float FillSpacing = 0.5f; 
    public Vector2 FillOffset;
    public float FillRotation;

    public bool FlipHorizontal;
    public bool FlipVertical;


    public List<VirtualDrone> edgePoints = new List<VirtualDrone>();
    public Rect sceneViewport;


    public ComputeShader bezierShader;

    private ComputeBuffer bezierBuffer;
    private ComputeBuffer pointBuffer;
    private ComputeBuffer resultBuffer;


    public void GeneratePointsFromPath() {
        edgePoints.Clear();

        var svg = LoadSVG();
        if (svg.Scene == null) {
            return;
        }
        var scene = svg.Scene;
        sceneViewport = svg.SceneViewport;

        List<(BezierContour, Color)> contours = GetBezierContours(scene.Root);

        List<VirtualDrone> possibleDrones = new();

        if(Outline) {
            possibleDrones.AddRange(GetEvenlySpacedPointsFromPath(contours, OutlineSpacing, Scale, pointRadius, MaxDrones));
        }
        
        if(Fill) {
            possibleDrones.AddRange(GetEvenlySpacedPointsFromShape(contours, FillSpacing, Scale, pointRadius));
        }
        
        foreach (var drone in possibleDrones) {
            if(!edgePoints.Any(p => Vector2.Distance(p.pos, drone.pos) < pointRadius*2 + 0.5)) {
                edgePoints.Add(drone);
            }
        }
    }

    private List<(BezierContour, Color)> GetBezierContours(SceneNode Root) {
        List<(BezierContour, Color)> contours = new List<(BezierContour, Color)>();

        if (Root.Children == null) {
            foreach(var shape in Root.Shapes) {
                var shapeColor = ((SolidFill)shape.Fill).Color;

                foreach (var contour in shape.Contours) {
                    contours.Add((contour, shapeColor));
                }
            }
            
            return contours;
        }

        foreach(var child in Root.Children) {
            var newContours = GetBezierContours(child);
            foreach (var contour in newContours) {
                contours.Add(contour);
            }
        }
        return contours;
    }

    private SVGParser.SceneInfo LoadSVG()
    {
        var svgDoc = SVGParser.ImportSVG(new StringReader(svgContent));

        return svgDoc;
    }

    public List<VirtualDrone> GetEvenlySpacedPointsFromShape(List<(BezierContour, Color)> contours, float spacing, float scale, float pointSize) {
        var points = GetPointsInViewport(sceneViewport, spacing, scale, pointSize);
        if (points.Count > MaxDrones * 2) {
            return new List<VirtualDrone>();
        }
        
        Dictionary<Vector2, VirtualDrone> drones = new();
        
        var results = new int[points.Count];
        
        pointBuffer = new ComputeBuffer(points.Count, sizeof(float) * 2);
        resultBuffer = new ComputeBuffer(points.Count, sizeof(int));

        pointBuffer.SetData(points);
        resultBuffer.SetData(results);

        int kernelHandle = bezierShader.FindKernel("CSMain");

        bezierShader.SetBuffer(kernelHandle, "points", pointBuffer);
        bezierShader.SetBuffer(kernelHandle, "results", resultBuffer);

        Vector2 scaleFactor = new Vector2(scale, scale); 

        bezierShader.SetVector("scale", scaleFactor); 

        foreach (var (contour, color) in contours) {
            bezierBuffer?.Release();
            bezierBuffer = new ComputeBuffer(contour.Segments.Length, sizeof(float) * 6);
            bezierBuffer.SetData(contour.Segments);

            bezierShader.SetBuffer(kernelHandle, "bezierSegments", bezierBuffer);
            
            int threadGroups = Mathf.CeilToInt(points.Count / 256f);
            bezierShader.Dispatch(kernelHandle, threadGroups, 1, 1);

            resultBuffer.GetData(results);

            for (int i = 0; i < results.Length; i++) {
                if (results[i] == 1) { 
                    if(!drones.ContainsKey(points[i])) {
                        drones.Add(points[i], new VirtualDrone(points[i], color));
                    } else {
                        drones.Remove(points[i]);
                    }
                }
            }
        }

        pointBuffer.Release();
        resultBuffer.Release();
        bezierBuffer?.Release();

        List<VirtualDrone> droneList = new List<VirtualDrone>(drones.Values);

        return droneList;      
    }

    private List<Vector2> GetPointsInViewport(Rect viewPort, float spacing, float scale, float pointSize) {
        List<Vector2> points = new();
        float adjustedSpacing = spacing + pointSize;

        for (float x = viewPort.xMin; x <= viewPort.xMax *scale; x += adjustedSpacing)
        {
            for (float y = viewPort.yMin; y <= viewPort.yMax *scale; y += adjustedSpacing) {
                Vector2 rotatedPoint = Quaternion.AngleAxis(FillRotation, Vector3.forward) * new Vector2(x, y);
                points.Add(rotatedPoint + FillOffset);
            }
        }

        return points;
    }


    public List<VirtualDrone> GetEvenlySpacedPointsFromPath(List<(BezierContour, Color)> contours, float spacing, float scale, float pointSize, int MaxDrones)
    {
        List<VirtualDrone> evenlySpacedPoints = new();
        float spacingWithSize = spacing + pointSize*5; //adjusting this to make it not look wonky with repulsion

        foreach (var (contour, color) in contours) {
            List<VirtualDrone> contourPoints = new()
            {
                new VirtualDrone(contour.Segments[0].P0 * scale, color)
            };

            Vector2 prevPoint = contour.Segments[0].P0 * scale;
            float distSinceLastPoint = 0;

            for (int i = 0; i < contour.Segments.Length; i++) {
                var segment = contour.Segments[i];
                BezierPathSegment? nextSegment = null;
                if (i != contour.Segments.Length - 1)
                    nextSegment = contour.Segments[i + 1];

                float t = 0;
                while(t <= 1) {
                    t += .1f;
                    if (nextSegment == null) break;
                    
                    Vector2 pointOnCurve = EvaluateCubic(segment, (BezierPathSegment)nextSegment, t) * scale;
                    distSinceLastPoint += Vector2.Distance(prevPoint, pointOnCurve);

                    while (distSinceLastPoint >= spacingWithSize) {
                        float overshootDist = distSinceLastPoint - spacingWithSize;

                        VirtualDrone newEvenlySpacedPoint = new VirtualDrone(pointOnCurve + (prevPoint-pointOnCurve).normalized * overshootDist, color);

                        contourPoints.Add(newEvenlySpacedPoint);
                        distSinceLastPoint = overshootDist;
                        prevPoint = newEvenlySpacedPoint.pos;

                        if (evenlySpacedPoints.Count + contourPoints.Count >= MaxDrones) {
                            evenlySpacedPoints.AddRange(contourPoints);
                            return evenlySpacedPoints;
                        }
                    }

                    prevPoint = pointOnCurve;
                }
            }

            // TODO: Check if its possible to move fit the point if its moved back and do so if possible
            if (Vector2.Distance(contourPoints.First().pos, contourPoints.Last().pos) <= pointSize * 2) {
                contourPoints.Remove(contourPoints.Last());
            }

            evenlySpacedPoints.AddRange(contourPoints);
        }
        
        return evenlySpacedPoints;
    }

    private static Vector2 EvaluateQuadratic(Vector2 a, Vector2 b, Vector2 c, float t)
    {  
        Vector2 p0 = Vector2.Lerp(a, b, t);
        Vector2 p1 = Vector2.Lerp(b, c, t);
        return Vector2.Lerp(p0, p1, t);
    }

    private static Vector2 EvaluateCubic(BezierPathSegment curve, BezierPathSegment nextCurve, float t)
    {  
        Vector2 p0 = EvaluateQuadratic(curve.P0, curve.P1, curve.P2, t);
        Vector2 p1 = EvaluateQuadratic(curve.P1, curve.P2, nextCurve.P0, t);
        return Vector2.Lerp(p0, p1, t);
    }

 
}