using UnityEngine;

public class Drone : MonoBehaviour
{   
    public Color color = Color.white;
    public float radius = 0.25f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {   
        transform.localScale = new Vector3(radius*2, radius*2, radius*2);
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            renderer.material.SetColor("_Color", color);
        }
    }
}
