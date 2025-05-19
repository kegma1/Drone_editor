using UnityEngine;

public class syncCameraRig : MonoBehaviour
{
    // sykroniserer hovedkameras rotasjon med igloo kamera rigget
    public GameObject IglooCameraRig;

    void Start()
    {
        IglooCameraRig = GameObject.Find("IglooCameraRig");
    }

    void Update()
    {
        var globalRotation = transform.rotation;
        IglooCameraRig.transform.rotation = globalRotation;
    }
}
