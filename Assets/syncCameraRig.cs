using UnityEngine;

public class syncCameraRig : MonoBehaviour
{
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
