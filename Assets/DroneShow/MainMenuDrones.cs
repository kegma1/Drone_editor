using UnityEngine;
using System.IO;

public class StaticDroneGraphic : MonoBehaviour
{
    public ComputeShader bezierShader;
    public GameObject dronePrefab;
    private string svgFilePath = "Assets/SVG/uit.svg";

    [Min(0.01f)]
    private float Scale = 0.012f;
    private float Duration = 4f;
    [Min(0.01f)]
    private float pointRadius = 0.25f;
    private int MaxDrones = 3000;

    private bool Outline = true;
    [Min(0f)]
    private float OutlineSpacing = 0.5f;

    private bool Fill = false;
    [Min(0f)]
    private float FillSpacing = 0.0f;
    private Vector2 FillOffset = new Vector2(0, 0);
    private float FillRotation = 1f;

    private bool FlipHorizontal = false;
    private bool FlipVertical = false;

    private Rect sceneViewport; 

    private DroneGraphic droneGraphic;

    private Vector3 formationPosition = new Vector3(280, -400, 0);  

    void Start()
    {
        Camera cam = Camera.main;

        float distance = 10f; 
        float frustumHeight = 2f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float frustumWidth = frustumHeight * cam.aspect;

        Vector3 frustumCenter = cam.transform.position + cam.transform.forward * distance;
        Vector3 bottomLeft = frustumCenter 
                        - cam.transform.right * (frustumWidth / 2f) 
                        - cam.transform.up * (frustumHeight / 2f);

        float sceneWidth = frustumWidth * 0.8f;
        float sceneHeight = frustumHeight * 0.5f;

        sceneViewport = new Rect(bottomLeft.x, bottomLeft.y, sceneWidth, sceneHeight);

        LoadSVGContent();
        ApplyStaticTransformations();
    }

    void LoadSVGContent()
    {
        if (File.Exists(svgFilePath))
        {
            string svgContent = File.ReadAllText(svgFilePath);
            Debug.Log("SVG Content Loaded Successfully");

            droneGraphic = new GameObject("DroneGraphic").AddComponent<DroneGraphic>();
            droneGraphic.svgContent = svgContent;
            droneGraphic.Scale = Scale;
            droneGraphic.Duration = Duration;
            droneGraphic.Outline = Outline;
            droneGraphic.OutlineSpacing = OutlineSpacing;
            droneGraphic.Fill = Fill;
            droneGraphic.FillSpacing = FillSpacing;
            droneGraphic.FillOffset = FillOffset;
            droneGraphic.FillRotation = FillRotation;
            droneGraphic.pointRadius = pointRadius;
            droneGraphic.bezierShader = bezierShader;
            droneGraphic.MaxDrones = MaxDrones;
            droneGraphic.FlipHorizontal = FlipHorizontal;
            droneGraphic.FlipVertical = FlipVertical;
        }
        else
        {
            Debug.LogError("SVG file not found at path: " + svgFilePath);
        }
    }

    void ApplyStaticTransformations()
    {
        if (droneGraphic != null)
        {
            droneGraphic.sceneViewport = sceneViewport;
            droneGraphic.GeneratePointsFromPath();

       
            float xOffset = sceneViewport.width * 3f;  
            float yOffset = sceneViewport.height * 8.5f; 

         
            formationPosition = new Vector3(
                sceneViewport.x + sceneViewport.width * 0.7f + xOffset,  
                sceneViewport.y + sceneViewport.height * 0.25f - yOffset, 
                Camera.main.transform.position.z + 65f  
            );

            Debug.Log($"Formation position: {formationPosition}");
    
            foreach (var edgePoint in droneGraphic.edgePoints)
            {
                VirtualDrone virtualDrone = new VirtualDrone(edgePoint.pos, edgePoint.color);
                    

                Vector3 transformedPos = virtualDrone.ApplyTransformation(transform, sceneViewport, droneGraphic.Scale, droneGraphic.FlipHorizontal, droneGraphic.FlipVertical);
        
                transformedPos.z = Camera.main.transform.position.z + 5f;  
                


                transformedPos += formationPosition;

        
                GameObject drone = CreateDrone();
                drone.transform.position = transformedPos;

                var droneComp = drone.GetComponent<Drone>();
                if (droneComp != null)
                {
                    droneComp.color = edgePoint.color;
                    droneComp.radius = pointRadius;
                }

                Debug.DrawLine(Camera.main.transform.position, transformedPos, Color.red, 5f); 
            }
        }
        else
        {
            Debug.LogError("DroneGraphic is not initialized.");
        }
    }


    private GameObject CreateDrone()
    {
        var drone = Instantiate(dronePrefab);
        var droneComp = drone.GetComponent<Drone>();
        if (droneComp != null)
        {
            droneComp.color = Color.white;
            droneComp.radius = pointRadius;
        }
        return drone;
    }
}