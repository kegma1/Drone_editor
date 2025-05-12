using UnityEngine;

public class parrentToHead : MonoBehaviour
{
    //IglooMainCamera
    void Start()
    {
        var iglooCamera = GameObject.Find("IglooMainCamera");
        transform.parent = iglooCamera.transform;
    }
}
