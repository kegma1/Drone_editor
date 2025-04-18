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
        }
        get => _ProjectFilePath;
    }
    private string ProjectFileContent;
    private DroneShowData ParsedProject;

    public GameObject TimelineContent;
    public TimelineManager timelineManager;

    public GameObject AnimationPanelPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ProjectFilePath = "Assets/test.json";

        if(ParsedProject == null) {
            return;
        }

        addToTimeline(ParsedProject.AnimationStart);

    }

    private void addToTimeline(AnimationData data) {
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
