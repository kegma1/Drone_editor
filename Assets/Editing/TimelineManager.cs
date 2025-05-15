using UnityEngine;

public class TimelineManager : MonoBehaviour
{
    public Transform TimelineContent;
    public InspectorManager inspectorManager;
    public GameObject AnimationPanelPrefab;

    public GameObject addImageButton;

    public ProjectLoader projectLoader;

    public ErrorManager errorManager;

    public GameObject Inspector;

    private GameObject _currentfocusedGraphic;
    public GameObject CurrentfocusedGraphic {
        get => _currentfocusedGraphic;
        set {
            if (_currentfocusedGraphic != value) {
                _currentfocusedGraphic = value;
                if(_currentfocusedGraphic != null) {
                    var panelComp = _currentfocusedGraphic.GetComponent<PanelData>();
                    Inspector.SetActive(true);
                    inspectorManager.setState(panelComp.animationData);
                } else {
                    Inspector.SetActive(false);
                    inspectorManager.setState(new());
                }

            }
        }
    }
    
    public AnimationData getFullAnimation() {
        AnimationData fullAnimation = null;
        foreach (Transform child in TimelineContent) {
            if(child.gameObject == addImageButton)
                continue;

            var panelComp = child.GetComponent<PanelData>();
            AnimationData childAnimation = panelComp.animationData;
            if (fullAnimation == null) {
                childAnimation.NextAnimation = null;
                childAnimation.Type = "NoneAnimation";
                fullAnimation = childAnimation;
            } else {
                childAnimation.NextAnimation = fullAnimation;
                childAnimation.Type = "LerpAnimation";
                fullAnimation = childAnimation;
            }
        }

        return fullAnimation;
    }    

    public void clearTimeline() {
        CurrentfocusedGraphic = null;
        foreach (Transform child in TimelineContent) {
            if(child.gameObject == addImageButton)
                continue;
            else
                Destroy(child.gameObject);
        }
    }

    public void OnAddAnimation() {
        if (projectLoader.ProjectFilePath == null || projectLoader.ParsedProject == null) {
            errorManager.DisplayError("You Have to create/load a project", 10);
            return;
        }

        var newPanel = Instantiate(AnimationPanelPrefab);
        var PanelManager = newPanel.GetComponentInChildren<PanelManager>();
        PanelManager.TimelineManager = this;


        newPanel.transform.SetParent(TimelineContent);
        newPanel.transform.SetSiblingIndex(1);

        CurrentfocusedGraphic = newPanel;

    }
}
