using UnityEngine;
using Unity.VectorGraphics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class EditorGraphic : MonoBehaviour
{
    // denne klassn skaper og håndterer dronene som hvises i editoren. denne er veldig lik DroneGraphic,
    // men er mer robust siden den kan mangle data. i tilegg må denne håndtere dronene selv.
    public ErrorManager errorManager; // Referanse til objekt brukt for å vise feilmeldinger til brukeren

    private string svgContent;

    private float Scale = 1f;
    public float pointRadius = 0.25f;
    public int MaxDrones;
    public DronePool dronePool;

    public List<GameObject> ActiveDrones;

    public TMP_Text counter;

    private bool Outline = true;

    private float OutlineSpacing = 0.5f; 


    private bool Fill = false;

 
    private float fillSpacing = 0.5f; 
    private Vector2 fillOffset;
    private float fillRotation;

    private bool FlipHorizontal;
    private bool FlipVertical;
    private GraphicData _graphic;
    // oppdaterer dronene når grafikken endrer seg.
    public GraphicData graphic
    {
        get => _graphic;
        set
        {
            _graphic = value;

            Scale = _graphic.Scale;
            Outline = _graphic.Outline;
            OutlineSpacing = _graphic.OutlineSpacing;

            Fill = _graphic.Fill;
            fillSpacing = _graphic.FillSpacing;
            fillOffset = new(_graphic.FillOffset[0], _graphic.FillOffset[1]);
            fillRotation = _graphic.FillRotation;

            FlipHorizontal = _graphic.FlipHorizontal;
            FlipVertical = _graphic.FlipVertical;

            svgContent = _graphic.Source;

            dronePool.createPool();
            GeneratePointsFromPath();
        }
    }

    private List<VirtualDrone> edgePoints = new List<VirtualDrone>();
    private Rect sceneViewport;

    public ComputeShader bezierShader;

    private ComputeBuffer bezierBuffer;
    private ComputeBuffer pointBuffer;
    private ComputeBuffer resultBuffer;

    public void GeneratePointsFromPath() {
        // Sletter alle gamle punkter
        edgePoints.Clear();
        // slipper alle droner tilbake i poolet
        ActiveDrones.ForEach(d => dronePool.releaseDrone(d));
        ActiveDrones.Clear();

        // last svg-en og return tidlig hvis noe er galt
        var svg = LoadSVG();
        if (svg == null) {
            return;
        } else if (svg?.Scene == null) {
            errorManager.DisplayError("ERROR: Unable to parse SVG", 5);
            return;
        }
        var scene = svg?.Scene;

        sceneViewport = (Rect)(svg?.SceneViewport);
        
        // hent ut alle konturene
        List<(BezierContour, Color)> contours = GetBezierContours(scene.Root);

        List<VirtualDrone> possibleDrones = new();

        // legg til omrisset hvis det er valgt
        if (Outline)
        {
            possibleDrones.AddRange(GetEvenlySpacedPointsFromPath(contours, OutlineSpacing, Scale, pointRadius, MaxDrones));
        }

        // legg til fyll hvis det er valgt
        if(Fill) {
            possibleDrones.AddRange(GetEvenlySpacedPointsFromShape(contours, fillSpacing, Scale, pointRadius));
        }

        // slett alle dronene som er for nær
        foreach (var drone in possibleDrones) {
            // vi plusser 0.5 for å hindre at de er borti hværandre, dette er fordi constaint solveren kræsjer hvis de ikke har nokk rom
            if (!edgePoints.Any(p => Vector2.Distance(p.pos, drone.pos) < pointRadius * 2 + 0.5))
            {
                edgePoints.Add(drone);
            }
        }

        // lag faktiske droner for hvær Vdrone.
        foreach (var Vdrone in edgePoints)
        {
            if (ActiveDrones.Count < MaxDrones)
            {
                var drone = dronePool.GetDrone();
                var droneComp = drone.GetComponent<Drone>();

                if (droneComp != null)
                {
                    droneComp.SetColor(Vdrone.color);
                    droneComp.SetRadius(pointRadius);

                    drone.transform.position = Vdrone.ApplyTransformation(transform, sceneViewport, Scale, FlipHorizontal, FlipVertical);

                }

                ActiveDrones.Add(drone);
            }
        }

        // oppdater counteret i ui-en
        counter.text = ActiveDrones.Count.ToString();

    }

    // Sletter alle droner som er inaktive.
    public void CleanDrones()
    {
        foreach (Transform child in transform)
        {
            if (!child.gameObject.activeInHierarchy)
            {
                dronePool.removeDrone(child.gameObject);
            }
        }
    }

    public void SetPos(float? x, float? y, float? z)
    {
        var newPos = transform.position;
        if (x != null) newPos.x = (float)x;
        if (y != null) newPos.y = (float)y;
        if (z != null) newPos.z = (float)z;

        transform.position = newPos;
    }

    public void SetRot(float? x, float? y, float? z) {
        var newRot = transform.rotation.eulerAngles;
        if (x != null) newRot.x = (float)x; 
        if (y != null) newRot.y = (float)y; 
        if (z != null) newRot.z = (float)z; 

        transform.rotation = Quaternion.Euler(newRot);
    }

    // recursivly henter ut alle konturene og fargene far svg formen
    private List<(BezierContour, Color)> GetBezierContours(SceneNode Root)
    {
        List<(BezierContour, Color)> contours = new List<(BezierContour, Color)>();

        if (Root.Children == null)
        {
            foreach (var shape in Root.Shapes)
            {
                var shapeColor = ((SolidFill)shape.Fill).Color;

                foreach (var contour in shape.Contours)
                {
                    contours.Add((contour, shapeColor));
                }
            }

            return contours;
        }

        foreach (var child in Root.Children)
        {
            var newContours = GetBezierContours(child);
            foreach (var contour in newContours)
            {
                contours.Add(contour);
            }
        }
        return contours;
    }

    // Laster svg-en og returnerer null hvis den ikke finnes eller ikke kan parses.
    private SVGParser.SceneInfo? LoadSVG()
    {
        if (svgContent != null)
        {
            var svgDoc = SVGParser.ImportSVG(new StringReader(svgContent));

            return svgDoc;
        }
        return null;
    }
    
    // bruker compute shader for å finne punkter for fyll
    public List<VirtualDrone> GetEvenlySpacedPointsFromShape(List<(BezierContour, Color)> contours, float spacing, float scale, float pointSize)
    {
        // genererer et rutenett av mulige punkter
        var points = GetPointsInViewport(sceneViewport, spacing, scale, pointSize);
        // hvis rutenettet er dastisk større en MaxDrones antar vi at det blir for mange droner, så vi returnerer tidlig
        if (points.Count > MaxDrones * 2)
        {
            errorManager.DisplayError("WARNING: Too many drones", 5);
            return new List<VirtualDrone>();
        }

        Dictionary<Vector2, VirtualDrone> drones = new();

        // lager og binder buffere til compute shaderen
        var results = new int[points.Count];

        pointBuffer = new ComputeBuffer(points.Count, sizeof(float) * 2);
        resultBuffer = new ComputeBuffer(points.Count, sizeof(int));

        pointBuffer.SetData(points);
        resultBuffer.SetData(results);

        // finne main funksjonen
        int kernelHandle = bezierShader.FindKernel("CSMain");

        // binder buffere og unifomer
        bezierShader.SetBuffer(kernelHandle, "points", pointBuffer);
        bezierShader.SetBuffer(kernelHandle, "results", resultBuffer);

        Vector2 scaleFactor = new Vector2(scale, scale);

        bezierShader.SetVector("scale", scaleFactor);

        // looper gjennom alle konturene for å sjekke fyllet
        foreach (var (contour, color) in contours)
        {   
            // lager bezier bufferen
            bezierBuffer?.Release();
            bezierBuffer = new ComputeBuffer(contour.Segments.Length, sizeof(float) * 6);
            bezierBuffer.SetData(contour.Segments);

            bezierShader.SetBuffer(kernelHandle, "bezierSegments", bezierBuffer);

            // regner ut hvor mange thread grupper vi trenger
            int threadGroups = Mathf.CeilToInt(points.Count / 256f);
            bezierShader.Dispatch(kernelHandle, threadGroups, 1, 1);

            // henter ut resultatet og 
            resultBuffer.GetData(results);

            // filterer ut alle punkter som ikke er i formen, en virituell drone for de som er.
            for (int i = 0; i < results.Length; i++)
            {
                if (results[i] == 1)
                {
                    if (!drones.ContainsKey(points[i]))
                    {
                        drones.Add(points[i], new VirtualDrone(points[i], color));
                    }
                    else
                    {
                        drones.Remove(points[i]);
                    }
                }
            }
        }

        // slipper alle bufferene
        pointBuffer.Release();
        resultBuffer.Release();
        bezierBuffer?.Release();


        List<VirtualDrone> droneList = new List<VirtualDrone>(drones.Values);

        return droneList;
    }

    // Lager en liste av punkter som er i et rutenett
    private List<Vector2> GetPointsInViewport(Rect viewPort, float spacing, float scale, float pointSize)
    {
        List<Vector2> points = new();
        float adjustedSpacing = spacing + pointSize;

        for (float x = viewPort.xMin; x <= viewPort.xMax * scale; x += adjustedSpacing)
        {
            for (float y = viewPort.yMin; y <= viewPort.yMax * scale; y += adjustedSpacing)
            {
                Vector2 rotatedPoint = Quaternion.AngleAxis(fillRotation, Vector3.forward) * new Vector2(x, y);
                points.Add(rotatedPoint + fillOffset);
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
                // sjekk om vi er på slutten av listen med konturene
                if (i != contour.Segments.Length - 1)
                    nextSegment = contour.Segments[i + 1];


                float t = 0;
                while(t <= 1) {
                    t += .1f;
                    // slutt tidlig hvis neste segment er null
                    if (nextSegment == null) break;
                    
                    // finn punkt på kurn
                    Vector2 pointOnCurve = EvaluateCubic(segment, (BezierPathSegment)nextSegment, t) * scale;
                    distSinceLastPoint += Vector2.Distance(prevPoint, pointOnCurve);

                    // Hvis vi har overkytt og det er mer plass til droner finner vi disse dronen
                    while (distSinceLastPoint >= spacingWithSize)
                    {
                        // finn ut hvor mye vi overkutte
                        float overshootDist = distSinceLastPoint - spacingWithSize;

                        VirtualDrone newEvenlySpacedPoint = new VirtualDrone(pointOnCurve + (prevPoint - pointOnCurve).normalized * overshootDist, color);

                        contourPoints.Add(newEvenlySpacedPoint);
                        distSinceLastPoint = overshootDist;
                        prevPoint = newEvenlySpacedPoint.pos;

                        // hvis punktene vi har funnet sålangt overskrider maks droner returnerer vi tidlig
                        if (evenlySpacedPoints.Count + contourPoints.Count >= MaxDrones)
                        {
                            errorManager.DisplayError("WARNING: Too many drones", 5);
                            evenlySpacedPoints.AddRange(contourPoints);

                            return evenlySpacedPoints;
                        }
                    }

                    prevPoint = pointOnCurve;
                }
            }

            // sjekk om siste og første punkt overlapper
            if (Vector2.Distance(contourPoints.First().pos, contourPoints.Last().pos) <= pointSize * 2) {
                contourPoints.Remove(contourPoints.Last());
            }

            evenlySpacedPoints.AddRange(contourPoints);
        }
        
        return evenlySpacedPoints;
    }

    // Finner punkt langs Bezier kurven basert på t
    private static Vector2 EvaluateCubic(BezierPathSegment curve, BezierPathSegment nextCurve, float t)
    {
        Vector2 p0 = EvaluateQuadratic(curve.P0, curve.P1, curve.P2, t);
        Vector2 p1 = EvaluateQuadratic(curve.P1, curve.P2, nextCurve.P0, t);
        return Vector2.Lerp(p0, p1, t);
    }
    
    private static Vector2 EvaluateQuadratic(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        Vector2 p0 = Vector2.Lerp(a, b, t);
        Vector2 p1 = Vector2.Lerp(b, c, t);
        return Vector2.Lerp(p0, p1, t);
    }


}
