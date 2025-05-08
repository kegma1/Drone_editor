using UnityEngine;

public class syncCameraRig : MonoBehaviour
{
    public GameObject IglooCameraRig;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        IglooCameraRig = GameObject.Find("IglooCameraRig");
    }

    // Update is called once per frame
    void Update()
    {
        var globalRotation = transform.rotation;
        IglooCameraRig.transform.rotation = globalRotation;
    }
}
