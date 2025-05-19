using UnityEngine;

public class Drone : MonoBehaviour
{   
    // Håndterer fargen og stærrelsen til drone modellen
    public Color color = Color.white;
    public float radius = 0.25f;
    private Renderer Renderer;

    void Awake()
    {
        Renderer = GetComponent<Renderer>();
    }
    void Start()
    {   
        SetRadius(radius);
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
