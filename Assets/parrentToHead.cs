using UnityEngine;
using UnityEngine.Rendering.Universal;

public class parrentToHead : MonoBehaviour
{
    //IglooMainCamera
    void Start()
    {
        var iglooCamera = GameObject.Find("IglooMainCamera");
        var camera = iglooCamera.GetComponent<Camera>();
        Debug.Log(camera);
        var additionalData = camera.GetUniversalAdditionalCameraData();
        additionalData.renderPostProcessing = true;
        
        transform.SetParent(iglooCamera.transform);
    }
}
