using UnityEngine;

public class TimelineManager : MonoBehaviour
{
    public Transform TimelineContent;
    public InspectorManager inspectorManager;
    public GameObject AnimationPanelPrefab;

    private GameObject _currentfocusedGraphic;
    public GameObject CurrentfocusedGraphic {
        get => _currentfocusedGraphic;
        set {
            if (_currentfocusedGraphic != value) {
                _currentfocusedGraphic = value;
                if(_currentfocusedGraphic != null) {
                    var panelComp = _currentfocusedGraphic.GetComponent<PanelData>();
                    inspectorManager.setState(panelComp.animationData);
                } else {
                    inspectorManager.setState(new());
                }

            }
        }
    }
    

    public void OnAddAnimation() {
        var newPanel = Instantiate(AnimationPanelPrefab);
        var PanelManager = newPanel.GetComponentInChildren<PanelManager>();
        PanelManager.TimelineManager = this;


        newPanel.transform.SetParent(TimelineContent);
        newPanel.transform.SetSiblingIndex(1);

        CurrentfocusedGraphic = newPanel;

    }
}
