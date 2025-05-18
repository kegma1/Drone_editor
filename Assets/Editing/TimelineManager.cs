using Google.OrTools.Sat;
using UnityEngine;

public class TimelineManager : MonoBehaviour
{
    // håndterer timlinen
    public Transform TimelineContent;
    public InspectorManager inspectorManager;
    public GameObject AnimationPanelPrefab;

    public GameObject addImageButton;

    public ProjectLoader projectLoader;

    public ErrorManager errorManager; // Referanse til objekt brukt for å vise feilmeldinger til brukeren

    public GameObject Inspector;

    private GameObject _currentfocusedGraphic;
    public GameObject CurrentfocusedGraphic {
        get => _currentfocusedGraphic;
        set {
            if (_currentfocusedGraphic != value)
            {
                var oldValue = _currentfocusedGraphic;
                _currentfocusedGraphic = value;
                if (_currentfocusedGraphic != null)
                {
                    var panelComp = _currentfocusedGraphic.GetComponent<PanelData>();
                    Inspector.SetActive(true);
                    inspectorManager.setState(panelComp.animationData);
                    panelComp.Select();
                }
                else
                {
                    Inspector.SetActive(false);
                    inspectorManager.setState(new());
                }

                if (oldValue != null)
                {
                    var panelComp = oldValue.GetComponent<PanelData>();
                    panelComp.Deselect();
                }
            }
        }
    }

    // Looper gjennom timelinen og konstruerer animasjonen
    // siden timelinen viser objektene baklengs er den siste animasjonen på index 1 og den førster er på slutten av listen
    // derfor setter NextAnimation til fullAnimation og så blir fullAnimation satt til NextAnimation.
    public AnimationData getFullAnimation()
    {
        AnimationData fullAnimation = null;
        foreach (Transform child in TimelineContent)
        {
            // Skip addImageButton
            if (child.gameObject == addImageButton)
                continue;

            var panelComp = child.GetComponent<PanelData>();
            AnimationData childAnimation = panelComp.animationData;
            if (fullAnimation == null)
            {
                childAnimation.NextAnimation = null;
                childAnimation.Type = "NoneAnimation";
                fullAnimation = childAnimation;
            }
            else
            {
                childAnimation.NextAnimation = fullAnimation;
                childAnimation.Type = "LerpAnimation";
                fullAnimation = childAnimation;
            }
        }

        return fullAnimation;
    }    

    public void clearTimeline()
    {
        CurrentfocusedGraphic = null;
        foreach (Transform child in TimelineContent)
        {
            if (child.gameObject == addImageButton)
                continue;
            else
                Destroy(child.gameObject);
        }
    }

    // når plussknappen blir trykket blir lager vi et nytt panel og fokuserer på det
    public void OnAddAnimation()
    {
        // brukeren må ha et valid prosjekt for å lage nytt bilde
        if (projectLoader.ProjectFilePath == null || projectLoader.ParsedProject == null)
        {
            errorManager.DisplayError("You Have to create/load a project", 10);
            return;
        }
 
        var newPanel = Instantiate(AnimationPanelPrefab);
        var PanelManager = newPanel.GetComponentInChildren<PanelManager>();
        PanelManager.TimelineManager = this;


        newPanel.transform.SetParent(TimelineContent);
        // timelinen viser ting baklegs. dette er for å ungå å flytte rundt på ting unødvendig.
        // plussknappen er det førse elementet så det nye bildet blir plassert på index 1.
        newPanel.transform.SetSiblingIndex(1);

        CurrentfocusedGraphic = newPanel;

    }
}
