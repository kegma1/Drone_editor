using UnityEngine;

public class Drone : MonoBehaviour
{   
    public Color color = Color.white;
    public float radius = 0.25f;
    private Renderer Renderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {   
        transform.localScale = new Vector3(radius*2, radius*2, radius*2);
        Renderer = GetComponent<Renderer>();
        SetColor(color);
    }

    public void SetColor(Color newColor)
    {
        if (Renderer != null && Renderer.material != null)
        {
            Renderer.material.SetColor("_Color", newColor);
            color = newColor;
        }
    }
}
