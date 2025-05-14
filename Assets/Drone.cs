using UnityEngine;

public class Drone : MonoBehaviour
{   
    public Color color = Color.white;
    public float radius = 0.25f;
    private Renderer Renderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {   
        SetRadius(radius);
        Renderer = GetComponent<Renderer>();
    }

    public void SetColor(Color newColor)
    {
        if (Renderer != null && Renderer.material != null)
        {
            Renderer.material.SetColor("_Color", newColor);
            color = newColor;
        }
    }

    public void SetRadius(float newRadius)
    {
        transform.localScale = new Vector3(newRadius*2, newRadius*2, newRadius*2);
    }
}
