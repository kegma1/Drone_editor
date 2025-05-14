using System.Collections;
using System.IO;
using SimpleFileBrowser;
using UnityEngine;
using Newtonsoft.Json;

public class SaveingManager : MonoBehaviour
{
    public ProjectLoader projectLoader;

    public TimelineManager timelineManager;

    public void onClickSave() {
        if (projectLoader.ProjectFilePath != null && projectLoader.ParsedProject != null) {
            var fullAnimation = timelineManager.getFullAnimation();
            var newProject = projectLoader.ParsedProject;
            newProject.AnimationStart = fullAnimation;
            
            var serializedProject = JsonConvert.SerializeObject(newProject);
            File.WriteAllText(projectLoader.ProjectFilePath, serializedProject);
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

            string defaultJson = "{\"Global\" : {\"DroneRadius\" : 0.25, \"MaxDrones\": 1000, \"IsLooping\": false},\"AnimationStart\" : null}";
            File.WriteAllText(path, defaultJson);

			OnFilesSelected( FileBrowser.Result ); 
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
    }

    private void OnFilesSelected( string[] filePaths ) {
		Debug.Log(filePaths[0]);
        projectLoader.ProjectFilePath = filePaths[0];

        if (projectLoader.ParsedProject == null) {
            Debug.Log("unable to parse project");
            return;
        }

        timelineManager.clearTimeline();
        projectLoader.addToTimeline(projectLoader.ParsedProject.AnimationStart);
	}
}
