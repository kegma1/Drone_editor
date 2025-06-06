using UnityEngine;
using UnityEngine.UI;

public class PanelManager : MonoBehaviour
{
    // Håndterer uien som er på de individuelle panelene
    public TimelineManager TimelineManager;


    public void OnDelete()
    {
        TimelineManager.CurrentfocusedGraphic = null;
        Destroy(transform.parent.gameObject);
    }

    public void OnFocus()
    {
        TimelineManager.CurrentfocusedGraphic = transform.parent.gameObject;
    }
    public void onMoveLeft()
    {
        var currentIndex = transform.parent.GetSiblingIndex();
        var newIndex = currentIndex + 1;
        // bound checking slik at bildet ikke havner utenfor listen
        if (newIndex == TimelineManager.TimelineContent.childCount) return;

        var sibling = TimelineManager.TimelineContent.GetChild(newIndex);
        sibling.transform.SetSiblingIndex(currentIndex);
        transform.parent.SetSiblingIndex(newIndex);
    }

    public void onMoveRight()
    {
        var currentIndex = transform.parent.GetSiblingIndex();
        var newIndex = currentIndex - 1;
        // bound checking slik at bildet ikke havner utenfor listen
        if (newIndex < 1) return;

        var sibling = TimelineManager.TimelineContent.GetChild(newIndex);
        sibling.transform.SetSiblingIndex(currentIndex);
        transform.parent.SetSiblingIndex(newIndex);

    }

}
