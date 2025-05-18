using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System;

public class ProjectLoader : MonoBehaviour
{   
    // Håndterer lasting av prosjektet samt populerer timelinen.
    private string _ProjectFilePath;
    public ErrorManager errorManager; // Referanse til objekt brukt for å vise feilmeldinger til brukeren

    // når vi oppdaterer filstien burde vi forsøke å parse filen og lager resultatet. dette blir lagret i ParsedProject
    public string ProjectFilePath
    {
        set
        {
            _ProjectFilePath = value;
            ProjectFileContent = File.ReadAllText(_ProjectFilePath);

            try
            {
                ParsedProject = JsonConvert.DeserializeObject<DroneShowData>(ProjectFileContent);
            }
            catch (Exception)
            {
                errorManager.DisplayError("ERROR: Malformed or unsupported json file, please try a different file", 10);
                _ProjectFilePath = null;
                ParsedProject = null;
                timelineManager.CurrentfocusedGraphic = null;
                return;
            }

            // reseter CurrentfocusedGraphic fordi den kan holde gammel data fra et tidligere prosjekt
            timelineManager.CurrentfocusedGraphic = null;
        }
        get => _ProjectFilePath;
    }
    private string ProjectFileContent;
#nullable enable
    // Denne starter som null og kan være nulle ganske langt inn.
    public DroneShowData? ParsedProject = null;
#nullable disable

    public GameObject TimelineContent;
    public TimelineManager timelineManager;

    public GameObject AnimationPanelPrefab;

    
    // recursive funksjon for å legge till alle animasjonene til timelinen
    public void addToTimeline(AnimationData data)
    {   
        // base case. hvis data er null betyr det at forige animasjon var siste, eller at det ikke er noen animasjoner
        if (data == null)
            return;

        // vi lager et nytt panel
        var newPanel = Instantiate(AnimationPanelPrefab);

        // henter ut panel data komponenten
        PanelData panelData = newPanel.GetComponent<PanelData>();
        // fyller den med data
        panelData.animationData = data;

        // gir det nye panelet en referanse til timelinen slik at knappene fungerer
        PanelManager panelManager = newPanel.GetComponentInChildren<PanelManager>();
        panelManager.TimelineManager = timelineManager;

        newPanel.transform.SetParent(TimelineContent.transform);
        newPanel.transform.SetSiblingIndex(1);

        if (data.NextAnimation != null)
        {
            addToTimeline(data.NextAnimation);
        }

    }

    
}
