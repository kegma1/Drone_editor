using UnityEngine;

public class VirtualDrone
{
    public Vector2 pos;
    public Color color;

    public VirtualDrone(Vector2 vector2, Color color)
    {
        this.pos = vector2;
        this.color = color;
    }

    // Regner ut world-space posisjonen for en drone.
    public Vector3 ApplyTransformation(Transform transform, Rect sceneViewport, float scale, bool flipH, bool flipV)
    {
        int IflipH = flipH ? -1 : 1;
        int IflipV = flipV ? -1 : 1;

        Vector3 localPos = new(this.pos.x * IflipV, this.pos.y * IflipH, 0);
        localPos.x -= (sceneViewport.width / 2) * scale;
        localPos.y += (sceneViewport.height / 2) * scale;

        Vector3 rotatedPos = transform.rotation * localPos;

        return transform.position + rotatedPos;
    }
}
