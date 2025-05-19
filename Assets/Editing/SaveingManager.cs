using System.Collections;
using System.IO;
using SimpleFileBrowser;
using UnityEngine;
using Newtonsoft.Json;

public class SaveingManager : MonoBehaviour
{
    // håndterer funksjonaliteten for saving, lasting og laging av nytt prosjekt
    public ProjectLoader projectLoader;

    public TimelineManager timelineManager;

    public ErrorManager errorManager; // Referanse til objekt brukt for å vise feilmeldinger til brukeren

    public void onClickSave() {
        // Hvis vi har et valid prosjekt lastet så lagerer vi prosjektet
        if (projectLoader.ProjectFilePath != null && projectLoader.ParsedProject != null)
        {
            // hent ut alle animasjonene og bildene i form av et tre
            var fullAnimation = timelineManager.getFullAnimation();
            // hent alle globale verdier
            var newProject = projectLoader.ParsedProject;
            newProject.AnimationStart = fullAnimation;

            // serialiser prosjektet og skriv det til angitt fil 
            var serializedProject = JsonConvert.SerializeObject(newProject);
            File.WriteAllText(projectLoader.ProjectFilePath, serializedProject);
            errorManager.DisplayError("Saved successfully", 2);
        }
    }

    public void onClicknNewProject() {
        StartCoroutine(PickLocation());
    }

    public IEnumerator PickLocation() {
        FileBrowser.SetFilters( true, new FileBrowser.Filter( "Project", ".json"));

		FileBrowser.SetDefaultFilter( ".json" );

		FileBrowser.AddQuickLink( "Users", "C:\\Users", null );

        yield return FileBrowser.WaitForSaveDialog(FileBrowser.PickMode.Files, false, null, null, "Create File", "Save" );
        Debug.Log( FileBrowser.Success );



        if( FileBrowser.Success ) {
            string path = FileBrowser.Result[0];

            // Lag et tumt prosjekt 
            string defaultJson = JsonConvert.SerializeObject(new DroneShowData());
            File.WriteAllText(path, defaultJson);

			OnFilesSelected( FileBrowser.Result ); 
        } else {
            errorManager.DisplayError("ERROR: Unable to create new project", 5);
        }
    }

    public void OnClickLoadProject() {
        StartCoroutine(PickFile());
    }

    public IEnumerator PickFile() {
        FileBrowser.SetFilters( true, new FileBrowser.Filter( "Project", ".json"));

		FileBrowser.SetDefaultFilter( ".json" );

		FileBrowser.AddQuickLink( "Users", "C:\\Users", null );

        yield return FileBrowser.WaitForLoadDialog( FileBrowser.PickMode.Files, false, null, null, "Select Files", "Load" );
        Debug.Log( FileBrowser.Success );

        if( FileBrowser.Success )
			OnFilesSelected( FileBrowser.Result ); 
        else
            errorManager.DisplayError("ERROR: Unable to read file", 5);
    }

    private void OnFilesSelected( string[] filePaths ) {
        projectLoader.ProjectFilePath = filePaths[0];

        if (projectLoader.ParsedProject == null) {
            errorManager.DisplayError("ERROR: Malformed or unsupported json file, please try a different file", 5);
            timelineManager.clearTimeline();
            return;
        }

        timelineManager.clearTimeline();
        projectLoader.addToTimeline(projectLoader.ParsedProject.AnimationStart);
	}
}
