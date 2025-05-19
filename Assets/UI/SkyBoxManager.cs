using System;
using UnityEngine;
using SimpleFileBrowser;
using System.Collections;
using System.IO;

public class SkyBoxManager : MonoBehaviour
{
    public ErrorManager errorManager; // Referanse til objekt brukt for å vise feilmeldinger til brukeren
    private Material skybox;
    
    public Texture2D OriginalSkybox;
    
    void Start() {
        skybox = RenderSettings.skybox;
        onOriginalBackground();
    }

    public void onOriginalBackground() {
            skybox.SetTexture("_MainTex", OriginalSkybox);
            RenderSettings.skybox = skybox;

            DynamicGI.UpdateEnvironment();
    }

    public void onChangeBackground() {
        StartCoroutine(PickNewBackground());
    }

    private IEnumerator PickNewBackground() {
        FileBrowser.SetFilters( true, new FileBrowser.Filter( "Background", ".png", ".jpg"));

		FileBrowser.SetDefaultFilter( ".png" );

		FileBrowser.AddQuickLink( "Users", "C:\\Users", null );

        yield return FileBrowser.WaitForLoadDialog( FileBrowser.PickMode.Files, false, null, null, "Select background", "Load" );
        Debug.Log( FileBrowser.Success );

        if( FileBrowser.Success )
			OnBackgroundSelected( FileBrowser.Result ); 
    }

    private void OnBackgroundSelected(string[] result) {
        if (result.Length == 0) {
            errorManager.DisplayError("ERROR: No file selected", 5);
            return;
        }

        var backgroundFilePath = result[0];

        // hvis vi har bilde dataen gjør vi den til en tekstur og oppdaterer teksturen til skybokesn og oppdaterer envoirmentet
        try
        {
            byte[] backgroundData = File.ReadAllBytes(backgroundFilePath);
            var texture = new Texture2D(1, 1);

            if (texture.LoadImage(backgroundData))
            {
                texture.wrapMode = TextureWrapMode.Repeat;

                skybox.SetTexture("_MainTex", texture);
                RenderSettings.skybox = skybox;

                DynamicGI.UpdateEnvironment();
            }
            else
            {
                errorManager.DisplayError("ERROR: Unable to load background", 5);
            }
        }
        catch (Exception e)
        {
            errorManager.DisplayError("Error: " + e.Message, 5);
        }
    }
}
