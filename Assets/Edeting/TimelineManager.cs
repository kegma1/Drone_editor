using UnityEngine;

public class TimelineManager : MonoBehaviour
{
    public Transform TimelineContent;
    public GameObject AnimationPanelPrefab;

    private GameObject _currentfocusedGraphic;
    public GameObject CurrentfocusedGraphic {
        get => _currentfocusedGraphic;
        set {
            _currentfocusedGraphic = value;
        }
    }
    

    public void OnAddAnimation() {
        var newPanel = Instantiate(AnimationPanelPrefab);
        var PanelManager = newPanel.GetComponentInChildren<PanelManager>();
        PanelManager.TimelineManager = this;

        CurrentfocusedGraphic = newPanel;

        newPanel.transform.SetParent(TimelineContent);

    }
}
