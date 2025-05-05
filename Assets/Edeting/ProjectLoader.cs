using UnityEngine;
using Newtonsoft.Json;
using System.IO;

public class ProjectLoader : MonoBehaviour
{
    private string _ProjectFilePath;

    public string ProjectFilePath {
        set {
            _ProjectFilePath = value;
            ProjectFileContent = File.ReadAllText(_ProjectFilePath);

            ParsedProject = JsonConvert.DeserializeObject<DroneShowData>(ProjectFileContent);
            timelineManager.CurrentfocusedGraphic = null;
        }
        get => _ProjectFilePath;
    }
    private string ProjectFileContent;
#nullable enable
    public DroneShowData? ParsedProject = null;
#nullable disable

    public GameObject TimelineContent;
    public TimelineManager timelineManager;

    public GameObject AnimationPanelPrefab;

    public void addToTimeline(AnimationData data) {
        if (data == null)
            return;

        var newPanel = Instantiate(AnimationPanelPrefab);

        PanelData panelData = newPanel.GetComponent<PanelData>();
        panelData.animationData = data;

        PanelManager panelManager = newPanel.GetComponentInChildren<PanelManager>();
        panelManager.TimelineManager = timelineManager;
        
        newPanel.transform.SetParent(TimelineContent.transform);
        newPanel.transform.SetSiblingIndex(1);

        if(data.NextAnimation != null) {
            addToTimeline(data.NextAnimation);
        }

    }

    
}
