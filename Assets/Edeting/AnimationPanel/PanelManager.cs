using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public TimelineManager TimelineManager;

    public void OnDelete() {
        Destroy(transform.parent.gameObject);
    }

    public void OnFocus() {
        TimelineManager.CurrentfocusedGraphic = transform.parent.gameObject;
    }
}
